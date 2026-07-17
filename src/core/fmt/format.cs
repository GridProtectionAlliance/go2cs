//******************************************************************************************************
//  fmt_package.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/21/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ꓸꓸꓸobject = System.Span<object>;

namespace go;

// This is a simple proxy for the fmt package for testing...
public static partial class fmt_package
{
    [GeneratedRegex(@"\%(?<flags>[\x20+\-#0]*)(?<width>[0-9]*)(?:\.(?<precision>[0-9]*))?(?<type>[vtbcdoOqxXUeEfFgGspTw%])", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex FormatExpr();

    private static readonly Regex s_formatExpr = FormatExpr();
    
    private static @string ToString(object arg)
    {
        if (arg is Stringer stringer)
            return stringer.String();

        if (arg is error err)
            return err.Error();

        if (arg is bool)
            return arg.ToString()!.ToLowerInvariant();

        // A float's default format is Go's %g with the shortest round-tripping digits
        if (TryGetFloat(arg, out double number, out bool single))
            return FormatFloat(number, 'g', -1, single);

        // Go renders complex values as "(2+3i)" — each part formatted like a scalar float,
        // imaginary part always signed — where the .NET ToString() forms are "<2; 3>"
        // (System.Numerics.Complex) and "(2, 3)" (go.complex64)
        if (arg is complex128 complex128Value)
            return FormatComplex(ToString(complex128Value.Real), ToString(complex128Value.Imaginary));

        if (arg is complex64 complex64Value)
            return FormatComplex(ToString(complex64Value.Real), ToString(complex64Value.Imaginary));

        if (arg is UntypedComplex untypedComplexValue)
            return ToString((complex128)untypedComplexValue);

        return arg?.ToString() ?? "<nil>";
    }

    private static @string FormatComplex(string realText, string imaginaryText)
    {
        return imaginaryText[0] is '-' or '+' ?
            $"({realText}{imaginaryText}i)" :
            $"({realText}+{imaginaryText}i)";
    }

    public static void Print(params object[] args) =>
        Console.Write(string.Join("", args.Select(ToString)));

    /// <summary>
    /// Formats arguments in an implementation-specific way and writes the result to console along with a new line.
    /// </summary>
    /// <param name="args">Arguments to display.</param>
    public static void Println(params object[] args)
    {
        if (args.Length == 1 && args[0] is not @string && args[0] is not string && args[0] is IEnumerable array)
        {
            Console.WriteLine($"[{string.Join(" ", array.Cast<object>().Select(ToString!))}]");
            return;
        }

        Console.WriteLine(string.Join(" ", args.Select(ToString)));
    }

    public static void Printf(@string format, params object[] args) =>
        Console.Write(Sprintf(format, args));

    // A Go spread through a variadic func VALUE (`f(format, args...)`) passes the slice's
    // `ꓸꓸꓸ` Span in normal form — mirror Sprintf's Span overload so Printf binds it too.
    public static void Printf(string format, params ꓸꓸꓸobject args) =>
        Console.Write(Sprintf(format, args));

    public static string Sprint(object? arg) => arg is null ? "<nil>" : ToString(arg);

    public static string Sprintf(@string format, params object[] args)
    {
        (format, args) = ConvertFormat(format, args);
        return string.Format(format, args.Select(arg => (object)ToString(arg)).ToArray());
    }

    public static string Sprintf(string format, params ꓸꓸꓸobject args)
    {
        (format, object[] args2) = ConvertFormat(format, args.ToArray());
        return string.Format(format, args2.Select(arg => (object)ToString(arg)).ToArray());
    }

    private static (string, object[]) ConvertFormat(string format, object[] args)
    {
        int index = 0;

        foreach (Match match in s_formatExpr.Matches(format))
        {
            if (match.Groups["type"].Value == "%")
            {
                format = new Regex(Regex.Escape(match.Value)).Replace(format, "%", 1);
                continue;
            }

            if (index >= args.Length)
                break;

            if (match.Groups["type"].Value == "T")
            {
                string gotTypeName = GetGoTypeName(args[index]);

                if (gotTypeName.StartsWith("go."))
                    gotTypeName = gotTypeName[3..];

                if (gotTypeName == "nil")
                    gotTypeName = "<nil>";

                args[index] = ApplyWidth(gotTypeName, match, numeric: false);
            }
            else
            {
                args[index] = FormatArg(args[index], match);
            }

            format = new Regex(Regex.Escape(match.Value)).Replace(format, $"{{{index++}}}", 1);
        }

        return (format, args);
    }

    // Renders a single argument applying the matched verb's flags, width and precision —
    // stub-proxy scope: the float verbs (%v/%g/%G/%e/%E/%f/%F) with their precisions, base-16
    // rendering (%x/%X) with the '#' alternate form, the ' '/'+' sign flags on numbers, and
    // '-'/'0' width padding. Other verb-specific renderings still fall back to ToString —
    // notably the sibling base verbs %b/%o/%O, which diverge from Go the same way %x did
    // before TryFormatHex was added.
    private static string FormatArg(object? arg, Match match)
    {
        string flags = match.Groups["flags"].Value;
        Group precision = match.Groups["precision"];
        string verb = match.Groups["type"].Value;
        bool numeric = IsNumericValue(arg);
        string value;

        // %x/%X impose their own sign, prefix, precision and padding rules, so they render
        // whole here rather than falling through to the shared sign/ApplyWidth handling
        if (verb is "x" or "X" && TryFormatHex(arg, match, verb == "X", out string hex))
            return hex;

        if (verb is "v" or "g" or "G" or "e" or "E" or "f" or "F" && TryGetFloat(arg, out double value64, out bool single))
        {
            // Go defaults %e/%f to a precision of 6 and %v/%g to the shortest round-trip; an
            // explicit precision in the verb overrules either. %v renders as %g, %F as %f.
            int prec = precision.Success
                ? precision.Value.Length == 0 ? 0 : int.Parse(precision.Value, CultureInfo.InvariantCulture)
                : verb is "e" or "E" or "f" or "F" ? 6 : -1;

            value = FormatFloat(value64, verb switch { "v" => 'g', "F" => 'f', _ => verb[0] }, prec, single);
        }
        else if (precision.Success && verb is "f" or "F" && TryGetDouble(arg, out double number) && !double.IsInfinity(number))
        {
            value = number.ToString("F" + (precision.Value.Length == 0 ? "0" : precision.Value), CultureInfo.InvariantCulture);
        }
        else
        {
            value = ToString(arg!);
        }

        if (numeric && !value.StartsWith('-'))
        {
            if (value.StartsWith('+'))
            {
                // Only ±Inf renders with an inherent '+' — Go keeps it unless the space flag
                // (without the plus flag) demotes it to a space
                if (flags.Contains(' ') && !flags.Contains('+'))
                    value = " " + value[1..];
            }
            else if (flags.Contains('+'))
                value = "+" + value;
            else if (flags.Contains(' '))
                value = " " + value;
        }

        return ApplyWidth(value, match, numeric);
    }

    private static string ApplyWidth(string value, Match match, bool numeric)
    {
        string widthText = match.Groups["width"].Value;

        if (widthText.Length == 0)
            return value;

        int width = int.Parse(widthText, CultureInfo.InvariantCulture);
        string flags = match.Groups["flags"].Value;

        if (flags.Contains('-'))
            return value.PadRight(width);

        if (flags.Contains('0') && numeric && value.Length < width)
        {
            // Zero padding goes after any sign (or the space/plus sign placeholder)
            int prefixLength = value.Length > 0 && value[0] is '-' or '+' or ' ' ? 1 : 0;

            // Inf and NaN don't look like numbers, so Go space-pads them despite the '0' flag
            if (prefixLength < value.Length && value[prefixLength] is 'I' or 'N')
                return value.PadLeft(width);

            return value[..prefixLength] + value[prefixLength..].PadLeft(width - prefixLength, '0');
        }

        return value.PadLeft(width);
    }

    // Renders a float the way Go's strconv.FormatFloat(value, verb, prec, bitSize) does, where a
    // negative precision selects the shortest representation that round-trips. Go and .NET agree on
    // the digits — .NET's "R" is the same shortest round-trip, and its "E<n>" rounds the exact value
    // to n+1 significant digits just as strconv does (half-to-even, and exact well past 17 digits) —
    // but they disagree on presentation: .NET switches to exponent form on different thresholds and
    // writes an uppercase, three-digit exponent ("1E+006") where Go writes lowercase and at least
    // two ("1e+06"). So take .NET's digits and lay them out the way strconv's formatDigits does.
    private static string FormatFloat(double value, char verb, int prec, bool single)
    {
        if (double.IsNaN(value))
            return "NaN";

        if (double.IsInfinity(value))
            return value > 0.0D ? "+Inf" : "-Inf";

        // Fixed-point wants digits rounded at a decimal place rather than a significant-digit
        // count; .NET's "F" already matches strconv's fmtF exactly, so let it render this one.
        if (verb is 'f' && prec >= 0)
            return NetFormat(value, single, "F" + prec.ToString(CultureInfo.InvariantCulture));

        bool shortest = prec < 0;
        string netForm;

        if (shortest)
        {
            netForm = NetFormat(value, single, "R");
        }
        else
        {
            // Go promotes a zero precision to one for 'g', and carries that through to the layout
            // below — not just to the digit count — so that %.0g of 0.7 stays "0.7" rather than
            // rounding away to "0"
            if (verb is 'g' or 'G' && prec == 0)
                prec = 1;

            // 'e' carries one digit before the point and `prec` after it; 'g' counts significant
            // digits outright
            int digits = verb is 'e' or 'E' ? prec + 1 : prec;
            netForm = NetFormat(value, single, "E" + (digits - 1).ToString(CultureInfo.InvariantCulture));
        }

        (string digs, int dp) = DecomposeDigits(netForm);
        bool negative = double.IsNegative(value);

        if (shortest)
        {
            prec = verb switch
            {
                'e' or 'E' => digs.Length - 1,
                'f' => Math.Max(digs.Length - dp, 0),
                _ => digs.Length
            };
        }

        if (verb is 'e' or 'E')
            return FmtE(negative, digs, dp, prec, verb);

        if (verb is 'f')
            return FmtF(negative, digs, dp, prec);

        // 'g'/'G': exponent form when the decimal exponent is below -4 or has reached the
        // precision — where "the precision" is a flat 6 whenever the digits were the shortest
        // round-trip, which is what makes Go print 1000000.0 as "1e+06" but 999999.0 as "999999"
        int eprec = prec;

        if (eprec > digs.Length && digs.Length >= dp)
            eprec = digs.Length;

        if (shortest)
            eprec = 6;

        int exponent = dp - 1;

        if (exponent < -4 || exponent >= eprec)
        {
            if (prec > digs.Length)
                prec = digs.Length;

            return FmtE(negative, digs, dp, prec - 1, verb == 'G' ? 'E' : 'e');
        }

        if (prec > dp)
            prec = digs.Length;

        return FmtF(negative, digs, dp, Math.Max(prec - dp, 0));
    }

    private static string NetFormat(double value, bool single, string format) =>
        single
            ? ((float)value).ToString(format, CultureInfo.InvariantCulture)
            : value.ToString(format, CultureInfo.InvariantCulture);

    // Reduces a .NET rendering ("123456.789", "1.234568E+005", "5E-324") to strconv's decimalSlice
    // shape: the significant digits, sign and trailing zeros stripped, scaled so that the value is
    // 0.<digits> x 10^dp. Zero decomposes to no digits at all, which both layouts special-case.
    private static (string digits, int dp) DecomposeDigits(string netForm)
    {
        ReadOnlySpan<char> text = netForm;

        if (text.Length > 0 && text[0] is '-' or '+')
            text = text[1..];

        int exponent = 0;
        int exponentIndex = text.IndexOfAny('E', 'e');

        if (exponentIndex >= 0)
        {
            exponent = int.Parse(text[(exponentIndex + 1)..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            text = text[..exponentIndex];
        }

        int pointIndex = text.IndexOf('.');
        string digits;
        int integerLength;

        if (pointIndex >= 0)
        {
            integerLength = pointIndex;
            digits = string.Concat(text[..pointIndex], text[(pointIndex + 1)..]);
        }
        else
        {
            integerLength = text.Length;
            digits = text.ToString();
        }

        int dp = integerLength + exponent;
        int leading = 0;

        while (leading < digits.Length && digits[leading] == '0')
        {
            leading++;
            dp--;
        }

        digits = digits[leading..].TrimEnd('0');

        return digits.Length == 0 ? ("", 0) : (digits, dp);
    }

    // strconv's fmtE: one digit, `prec` more after the point (zero-filled), then a signed
    // exponent of at least two digits
    private static string FmtE(bool negative, string digits, int dp, int prec, char exponentChar)
    {
        StringBuilder result = new();

        if (negative)
            result.Append('-');

        result.Append(digits.Length == 0 ? '0' : digits[0]);

        if (prec > 0)
        {
            result.Append('.');

            int index = 1;
            int available = Math.Min(digits.Length, prec + 1);

            if (index < available)
            {
                result.Append(digits, index, available - index);
                index = available;
            }

            result.Append('0', prec + 1 - index);
        }

        result.Append(char.IsUpper(exponentChar) ? 'E' : 'e');

        int exponent = digits.Length == 0 ? 0 : dp - 1;

        if (exponent < 0)
        {
            result.Append('-');
            exponent = -exponent;
        }
        else
        {
            result.Append('+');
        }

        if (exponent < 10)
            result.Append('0');

        result.Append(exponent.ToString(CultureInfo.InvariantCulture));

        return result.ToString();
    }

    // strconv's fmtF: the integer part zero-padded out to the decimal point, then `prec` fractional
    // digits drawn from the digit string (zero where it has run out)
    private static string FmtF(bool negative, string digits, int dp, int prec)
    {
        StringBuilder result = new();

        if (negative)
            result.Append('-');

        if (dp > 0)
        {
            int available = Math.Min(digits.Length, dp);
            result.Append(digits, 0, available);
            result.Append('0', dp - available);
        }
        else
        {
            result.Append('0');
        }

        if (prec > 0)
        {
            result.Append('.');

            for (int i = 0; i < prec; i++)
            {
                int index = dp + i;
                result.Append(index >= 0 && index < digits.Length ? digits[index] : '0');
            }
        }

        return result.ToString();
    }

    private static bool TryGetFloat(object? arg, out double value, out bool single)
    {
        switch (arg)
        {
            case float source:
                value = source;
                single = true;
                return true;
            case double source:
                value = source;
                single = false;
                return true;
            // An untyped float constant reaching fmt takes its default type, float64
            case UntypedFloat source:
                value = (float64)source;
                single = false;
                return true;
            default:
                value = 0.0D;
                single = false;
                return false;
        }
    }

    // Renders the %x/%X verbs, which Go applies to two operand kinds with distinct rules: an
    // integer becomes base-16 digits, and a byte sequence (string, []byte) becomes bytewise hex.
    // Operand kinds outside stub scope return false and fall back to ToString — floats (Go renders
    // those in hexadecimal-exponent form, "0x1.91eb851eb851fp+01") and composites (Go renders those
    // elementwise, "[ff 100]").
    private static bool TryFormatHex(object? arg, Match match, bool upper, out string value)
    {
        if (TryGetByteSequence(arg, out ReadOnlySpan<byte> bytes))
        {
            value = FormatHexBytes(bytes, match, upper);
            return true;
        }

        if (TryGetInteger(arg, out bool negative, out ulong magnitude))
        {
            value = FormatHexInteger(negative, magnitude, match, upper);
            return true;
        }

        value = "";
        return false;
    }

    // Go renders %x/%X of an integer as sign-magnitude ("-ff", never a two's-complement form), so
    // the operand is reduced to a sign and an unsigned magnitude.
    private static bool TryGetInteger(object? arg, out bool negative, out ulong magnitude)
    {
        long signed;

        switch (arg)
        {
            case byte or ushort or uint or ulong:
                negative = false;
                magnitude = Convert.ToUInt64(arg, CultureInfo.InvariantCulture);
                return true;
            case nuint source:
                negative = false;
                magnitude = source;
                return true;
            case uintptr source:
                negative = false;
                magnitude = (nuint)source;
                return true;
            case sbyte or short or int or long:
                signed = Convert.ToInt64(arg, CultureInfo.InvariantCulture);
                break;
            case nint source:
                signed = source;
                break;
            case UntypedInt source:
                // An UntypedInt's payload may be an unsigned 64-bit constant that reads back
                // negative as int64 — its ToString() accounts for that, so it decides the sign
                if (!source.ToString().StartsWith('-'))
                {
                    negative = false;
                    magnitude = (ulong)source;
                    return true;
                }

                signed = (long)source;
                break;
            default:
                negative = false;
                magnitude = 0UL;
                return false;
        }

        negative = signed < 0L;

        // Negating long.MinValue overflows back to itself — the unchecked cast still yields the
        // correct magnitude (0x8000000000000000)
        magnitude = negative ? unchecked((ulong)-signed) : (ulong)signed;
        return true;
    }

    // Go renders %x/%X of a byte sequence over its raw bytes — for a string that is its UTF-8
    // encoding, not its chars
    private static bool TryGetByteSequence(object? arg, out ReadOnlySpan<byte> bytes)
    {
        switch (arg)
        {
            case @string source:
                bytes = source.ToSpan();
                return true;
            case slice<byte> source:
                bytes = source.ToSpan();
                return true;
            case string source:
                bytes = Encoding.UTF8.GetBytes(source);
                return true;
            default:
                bytes = default;
                return false;
        }
    }

    private static string FormatHexInteger(bool negative, ulong magnitude, Match match, bool upper)
    {
        string flags = match.Groups["flags"].Value;
        Group precision = match.Groups["precision"];
        string widthText = match.Groups["width"].Value;
        int width = widthText.Length == 0 ? 0 : int.Parse(widthText, CultureInfo.InvariantCulture);
        string sign = negative ? "-" : flags.Contains('+') ? "+" : flags.Contains(' ') ? " " : "";
        string prefix = flags.Contains('#') ? upper ? "0X" : "0x" : "";
        string digits = magnitude.ToString(upper ? "X" : "x", CultureInfo.InvariantCulture);

        if (precision.Success)
        {
            int digitCount = precision.Value.Length == 0 ? 0 : int.Parse(precision.Value, CultureInfo.InvariantCulture);

            // An explicit precision is a minimum digit count that also overrides the '0' flag;
            // precision 0 applied to a 0 value prints no digits at all, only padding
            digits = digitCount == 0 && magnitude == 0UL ? "" : digits.PadLeft(digitCount, '0');
        }
        else if (flags.Contains('0') && !flags.Contains('-') && width > 0)
        {
            // Go's '0' flag on an integer is NOT padding to the width — it sets the digit count
            // (an implicit precision) that leaves room for the sign but does NOT count the '0x'
            // prefix. So `%#08x` of 255 is the 10-char "0x000000ff", where the space-padded
            // `%#8x` is "    0xff".
            digits = digits.PadLeft(width - sign.Length, '0');
        }

        return PadHex(sign + prefix + digits, width, flags, zeroPad: false);
    }

    private static string FormatHexBytes(ReadOnlySpan<byte> bytes, Match match, bool upper)
    {
        string flags = match.Groups["flags"].Value;
        Group precision = match.Groups["precision"];
        string widthText = match.Groups["width"].Value;
        int width = widthText.Length == 0 ? 0 : int.Parse(widthText, CultureInfo.InvariantCulture);
        int length = bytes.Length;

        // Precision limits the length of the INPUT (the bytes consumed), not of the hex output
        if (precision.Success)
        {
            int limit = precision.Value.Length == 0 ? 0 : int.Parse(precision.Value, CultureInfo.InvariantCulture);

            if (limit < length)
                length = limit;
        }

        // Unlike an integer's digit-count reading of the flag, a byte sequence zero-pads its whole
        // rendering — prefix included, so `%#08x` of "ab" is "000x6162"
        bool zeroPad = flags.Contains('0');

        // An empty sequence renders as padding alone — no '0x' prefix
        if (length == 0)
            return PadHex("", width, flags, zeroPad);

        // The ' ' flag separates the bytes, and makes '#' prefix every byte rather than the run
        bool spaced = flags.Contains(' ');
        string prefix = flags.Contains('#') ? upper ? "0X" : "0x" : "";
        string format = upper ? "X2" : "x2";
        StringBuilder builder = new();

        for (int index = 0; index < length; index++)
        {
            if (spaced && index > 0)
                builder.Append(' ');

            if (prefix.Length > 0 && (spaced || index == 0))
                builder.Append(prefix);

            builder.Append(bytes[index].ToString(format, CultureInfo.InvariantCulture));
        }

        return PadHex(builder.ToString(), width, flags, zeroPad);
    }

    private static string PadHex(string value, int width, string flags, bool zeroPad)
    {
        if (value.Length >= width)
            return value;

        // '-' left-justifies and always pads with spaces, outranking the '0' flag
        return flags.Contains('-') ? value.PadRight(width) : value.PadLeft(width, zeroPad ? '0' : ' ');
    }

    private static bool IsNumericValue(object? arg) =>
        arg is sbyte or byte or short or ushort or int or uint or long or ulong
            or nint or nuint or float or double or decimal or UntypedInt or UntypedFloat;

    private static bool TryGetDouble(object? arg, out double value)
    {
        switch (arg)
        {
            case float source:
                value = source;
                return true;
            case double source:
                value = source;
                return true;
            case decimal source:
                value = (double)source;
                return true;
            case sbyte or byte or short or ushort or int or uint or long or ulong:
                value = Convert.ToDouble(arg, CultureInfo.InvariantCulture);
                return true;
            case nint source:
                value = source;
                return true;
            case nuint source:
                value = source;
                return true;
            case UntypedFloat source:
                value = (float64)source;
                return true;
            case UntypedInt source:
                value = (int64)source;
                return true;
            default:
                value = 0.0D;
                return false;
        }
    }
    
    public static void Printf(ReadOnlySpan<byte> format, params object[] args) =>
        Printf((@string)format, args);

    public static string Sprintf(ReadOnlySpan<byte> format, params object[] args) =>
        Sprintf((@string)format, args);

    // Temporary error implementation for the stub fmt.Errorf — formats the message via Sprintf
    // and returns it as an error. NOTE: the `%w` verb is rendered like `%v`; error-chain
    // wrapping (the Unwrap method that a real %w Errorf produces) is not implemented here.
    // Full behavior arrives with stdlib fmt resolution.
    private sealed class formatError(string message) : error
    {
        public @string Error() => message;
        public override string ToString() => message;
    }

    public static error Errorf(ReadOnlySpan<byte> format, params object[] args) =>
        new formatError(Sprintf(format, args));
}
