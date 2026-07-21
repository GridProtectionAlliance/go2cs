//******************************************************************************************************
//  fmt_package.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
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
    // stub-proxy scope: the float verbs (%v/%g/%G/%e/%E/%f/%F) with their precisions, the
    // integer base verbs (%b/%o/%O/%x/%X) with their prefix/digit-count rules, the strconv
    // float forms of %x/%X (hex mantissa-exponent) and %b (power-of-two exponent), the
    // ' '/'+' sign flags on numbers, and '-'/'0' width padding. Other verb-specific
    // renderings still fall back to ToString — notably composites, which Go renders
    // elementwise (%x of []int is "[ff 100]", %b of []byte likewise), and the %!-error
    // forms for verb/operand mismatches (%b of a string, %o/%O of a float).
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

        // %b/%o/%O of an integer run the same base-N machinery (%x's byte-sequence reading
        // has no analog: Go renders those elementwise or as %!-error forms — out of scope)
        if (verb is "b" or "o" or "O" && TryGetInteger(arg, out bool intNegative, out ulong intMagnitude))
            return FormatBaseInteger(intNegative, intMagnitude, match, verb[0]);

        if (verb is "v" or "g" or "G" or "e" or "E" or "f" or "F" && TryGetFloat(arg, out double value64, out bool single))
        {
            // Go defaults %e/%f to a precision of 6 and %v/%g to the shortest round-trip; an
            // explicit precision in the verb overrules either. %v renders as %g, %F as %f.
            int prec = precision.Success
                ? precision.Value.Length == 0 ? 0 : int.Parse(precision.Value, CultureInfo.InvariantCulture)
                : verb is "e" or "E" or "f" or "F" ? 6 : -1;

            value = FormatFloat(value64, verb switch { "v" => 'g', "F" => 'f', _ => verb[0] }, prec, single);
        }
        else if (verb is "x" or "X" or "b" && TryGetFloat(arg, out double bits64, out bool bitsSingle))
        {
            // Go's float forms for the base verbs come from strconv: %x/%X is the
            // hexadecimal-exponent form ("0x1.91eb851eb851fp+01"), %b the decimalless
            // power-of-two-exponent form ("4503599627370496p-52"). Sign flags and the
            // (naive, whole-rendering) '0'-flag padding follow the shared path below.
            if (verb is "b")
                value = FormatBinaryFloat(bits64, bitsSingle);
            else
                value = FormatHexFloat(bits64, bitsSingle, verb == "X",
                    precision.Success ? precision.Value.Length == 0 ? 0 : int.Parse(precision.Value, CultureInfo.InvariantCulture) : -1,
                    flags.Contains('#'));
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

    // Splits a finite float into strconv's (neg, mant, exp) triple, where the value is
    // mant x 2^(exp - mantissa bits): the stored fraction with its implicit leading 1 restored
    // for normals, and a subnormal's exponent pinned to the minimum normal exponent
    private static (bool negative, ulong mantissa, int exponent) DecomposeFloatBits(double value, bool single)
    {
        int mantissaBits = single ? 23 : 52;
        int exponentBits = single ? 8 : 11;
        int bias = single ? -127 : -1023;
        ulong bits = single ? BitConverter.SingleToUInt32Bits((float)value) : BitConverter.DoubleToUInt64Bits(value);

        bool negative = bits >> (mantissaBits + exponentBits) != 0UL;
        int exponent = (int)(bits >> mantissaBits) & ((1 << exponentBits) - 1);
        ulong mantissa = bits & ((1UL << mantissaBits) - 1UL);

        if (exponent == 0)
            exponent = 1;
        else
            mantissa |= 1UL << mantissaBits;

        return (negative, mantissa, exponent + bias);
    }

    // Renders a float the way Go's strconv.FormatFloat(value, 'b', -1, bitSize) does — the
    // decimalless-scientific power-of-two-exponent form "4503599627370496p-52": the raw
    // (unnormalized) mantissa in decimal and an exponent with no zero padding ("8388608p-9").
    // fmt ignores both precision and the '#' flag for %b.
    private static string FormatBinaryFloat(double value, bool single)
    {
        if (double.IsNaN(value))
            return "NaN";

        if (double.IsInfinity(value))
            return value > 0.0D ? "+Inf" : "-Inf";

        (bool negative, ulong mantissa, int exponent) = DecomposeFloatBits(value, single);

        exponent -= single ? 23 : 52;

        return (negative ? "-" : "") + mantissa.ToString(CultureInfo.InvariantCulture) +
            "p" + (exponent >= 0 ? "+" : "") + exponent.ToString(CultureInfo.InvariantCulture);
    }

    // Renders a float the way Go's strconv.FormatFloat(value, 'x', prec, bitSize) does — the
    // hexadecimal-exponent form "0x1.91eb851eb851fp+01": mantissa normalized to a leading 1
    // (even for subnormals, so 5e-324 is "0x1p-1074"), the shortest form trimming trailing
    // zero fraction digits, and an explicit precision rounding the binary mantissa to
    // nearest-ties-to-even at that hex digit (with carry renormalization: %.0x of 255.9999
    // is "0x1p+08"). The exponent is decimal with at least two digits.
    private static string FormatHexFloat(double value, bool single, bool upper, int prec, bool sharp)
    {
        if (double.IsNaN(value))
            return "NaN";

        if (double.IsInfinity(value))
            return value > 0.0D ? "+Inf" : "-Inf";

        (bool negative, ulong mantissa, int exponent) = DecomposeFloatBits(value, single);

        if (mantissa == 0UL)
            exponent = 0;

        // strconv's fmtX: align the leading 1 (if any) to bit 60
        mantissa <<= 60 - (single ? 23 : 52);

        while (mantissa != 0UL && (mantissa & (1UL << 60)) == 0UL)
        {
            mantissa <<= 1;
            exponent--;
        }

        // Round to `prec` hex fraction digits (a precision of 15+ already holds every bit)
        if (prec >= 0 && prec < 15)
        {
            int shift = prec * 4;
            ulong extra = (mantissa << shift) & ((1UL << 60) - 1UL);

            mantissa >>= 60 - shift;

            if ((extra | (mantissa & 1UL)) > 1UL << 59)
                mantissa++;

            mantissa <<= 60 - shift;

            // The round-up wrapped the mantissa past 2.0 — renormalize
            if ((mantissa & (1UL << 61)) != 0UL)
            {
                mantissa >>= 1;
                exponent++;
            }
        }

        string hexDigits = upper ? "0123456789ABCDEF" : "0123456789abcdef";
        StringBuilder result = new();

        if (negative)
            result.Append('-');

        result.Append('0');
        result.Append(upper ? 'X' : 'x');
        result.Append((mantissa & (1UL << 60)) != 0UL ? '1' : '0');

        mantissa <<= 4; // drop the leading digit

        if (prec < 0 && mantissa != 0UL)
        {
            result.Append('.');

            while (mantissa != 0UL)
            {
                result.Append(hexDigits[(int)((mantissa >> 60) & 15UL)]);
                mantissa <<= 4;
            }
        }
        else if (prec > 0)
        {
            result.Append('.');

            for (int i = 0; i < prec; i++)
            {
                result.Append(hexDigits[(int)((mantissa >> 60) & 15UL)]);
                mantissa <<= 4;
            }
        }

        result.Append(upper ? 'P' : 'p');

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

        return sharp ? RestoreSharpZeros(result.ToString(), prec, upper) : result.ToString();
    }

    // fmt's '#' pass over a hex-float rendering: force a decimal point and restore trailing
    // zeros up to a budget of `prec` significant characters (default 6 when no explicit
    // precision), counted from the first nonzero character on — which includes the 'x'
    // itself, making %#x of 1.5 the four-fraction-digit "0x1.8000p+00". The budget applies
    // to lowercase %x only; %#X gets budget 0 but still forces the point ("0X1.P+00").
    private static string RestoreSharpZeros(string value, int prec, bool upper)
    {
        int digits = upper ? 0 : prec < 0 ? 6 : prec;
        int tailIndex = value.IndexOf(upper ? 'P' : 'p');
        string number = value[..tailIndex];
        bool sawNonzero = false;

        foreach (char c in number)
        {
            if (c is '-' or '.')
                continue;

            if (c != '0')
                sawNonzero = true;

            if (sawNonzero)
                digits--;
        }

        StringBuilder result = new(number);

        if (!number.Contains('.'))
            result.Append('.');

        for (; digits > 0; digits--)
            result.Append('0');

        result.Append(value, tailIndex, value.Length - tailIndex);

        return result.ToString();
    }

    // Renders the %x/%X verbs' whole-rendering operand kinds, which Go treats with distinct
    // rules: an integer becomes base-16 digits, and a byte sequence (string, []byte) becomes
    // bytewise hex. Other kinds return false — a float is rendered downstream
    // (FormatHexFloat), and composites fall back to ToString (Go renders those elementwise,
    // "[ff 100]").
    private static bool TryFormatHex(object? arg, Match match, bool upper, out string value)
    {
        if (TryGetByteSequence(arg, out ReadOnlySpan<byte> bytes))
        {
            value = FormatHexBytes(bytes, match, upper);
            return true;
        }

        if (TryGetInteger(arg, out bool negative, out ulong magnitude))
        {
            value = FormatBaseInteger(negative, magnitude, match, upper ? 'X' : 'x');
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

    // Renders an integer under any of the base verbs (%b/%o/%O/%x/%X), whose sign, width,
    // precision and '0'-flag digit-count rules are shared — only the base and the '#'
    // alternate form differ per verb
    private static string FormatBaseInteger(bool negative, ulong magnitude, Match match, char verb)
    {
        string flags = match.Groups["flags"].Value;
        Group precision = match.Groups["precision"];
        string widthText = match.Groups["width"].Value;
        int width = widthText.Length == 0 ? 0 : int.Parse(widthText, CultureInfo.InvariantCulture);
        string sign = negative ? "-" : flags.Contains('+') ? "+" : flags.Contains(' ') ? " " : "";
        bool sharp = flags.Contains('#');

        // Convert.ToString(long, base) renders the two's-complement bit pattern, which for the
        // sign-magnitude-reduced value is exactly the unsigned magnitude in that base
        string digits = verb switch
        {
            'b' => Convert.ToString(unchecked((long)magnitude), 2),
            'o' or 'O' => Convert.ToString(unchecked((long)magnitude), 8),
            'X' => magnitude.ToString("X", CultureInfo.InvariantCulture),
            _ => magnitude.ToString("x", CultureInfo.InvariantCulture)
        };

        if (precision.Success)
        {
            int digitCount = precision.Value.Length == 0 ? 0 : int.Parse(precision.Value, CultureInfo.InvariantCulture);

            // Precision 0 applied to a 0 value prints nothing at all — not even the sign or
            // the '#'/'0o' prefix — and pads the width with spaces regardless of the '0' flag
            if (digitCount == 0 && magnitude == 0UL)
                return PadHex("", width, flags, zeroPad: false);

            // An explicit precision is a minimum digit count that also overrides the '0' flag
            digits = digits.PadLeft(digitCount, '0');
        }
        else if (flags.Contains('0') && !flags.Contains('-') && width > 0)
        {
            // Go's '0' flag on an integer is NOT padding to the width — it sets the digit count
            // (an implicit precision) that leaves room for the sign but does NOT count the '0x'
            // ('0b', '0o') prefix. So `%#08x` of 255 is the 10-char "0x000000ff", where the
            // space-padded `%#8x` is "    0xff".
            digits = digits.PadLeft(width - sign.Length, '0');
        }

        // '#' gives base 16 and 2 a "0x"/"0X"/"0b" prefix; %O always carries "0o". Octal's
        // '#' form instead guarantees a leading '0' DIGIT (inserted under any "0o" prefix),
        // so it is absorbed by zero-padded digits: `%#08o` of 255 is just "00000377".
        string prefix = verb switch
        {
            'x' when sharp => "0x",
            'X' when sharp => "0X",
            'b' when sharp => "0b",
            'O' => "0o",
            _ => ""
        };

        if (sharp && verb is 'o' or 'O' && digits[0] != '0')
            digits = "0" + digits;

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
