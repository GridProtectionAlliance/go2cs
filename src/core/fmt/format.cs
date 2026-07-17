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

        // Go renders floating-point infinities as "+Inf"/"-Inf" (strconv keeps the sign
        // even for positives) — .NET default formatting would produce "∞"/"-∞"
        if (arg is double doubleValue && double.IsInfinity(doubleValue))
            return doubleValue > 0.0D ? "+Inf" : "-Inf";

        if (arg is float floatValue && float.IsInfinity(floatValue))
            return floatValue > 0.0F ? "+Inf" : "-Inf";

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
    // stub-proxy scope: fixed-point precision (%f/%F), the ' '/'+' sign flags on numbers,
    // and '-'/'0' width padding. Other verb-specific renderings still fall back to ToString.
    private static string FormatArg(object? arg, Match match)
    {
        string flags = match.Groups["flags"].Value;
        Group precision = match.Groups["precision"];
        string verb = match.Groups["type"].Value;
        bool numeric = IsNumericValue(arg);
        string value;

        if (precision.Success && verb is "f" or "F" && TryGetDouble(arg, out double number) && !double.IsInfinity(number))
            value = number.ToString("F" + (precision.Value.Length == 0 ? "0" : precision.Value), CultureInfo.InvariantCulture);
        else
            value = ToString(arg!);

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
