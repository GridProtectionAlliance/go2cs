using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace go.testing_runtime;

/// <summary>
/// Diagnostic-grade Go-style formatting for the bootstrap testing shim.
/// </summary>
/// <remarks>
/// The testing runtime must stay fmt-free: converted test projects resolve every standard-library
/// dependency, including fmt, from the overlaid go-src-converted tree, while this shim builds in the
/// baseline solution — a baseline core/fmt reference here would let one test build transitively
/// contain both trees' "namespace go" partial classes (the mixed-tree collision CLAUDE.md forbids).
/// The shim therefore carries this small self-contained formatter instead. Its output feeds
/// t.Log/t.Error diagnostics only — the differential oracle never byte-compares log text
/// (TestingInfrastructureRequirements §7) — so coverage of the common verbs is sufficient, and the
/// converted Go testing package replaces the whole shim at Phase 4D.
/// </remarks>
internal static class TestFormat
{
    /// <summary>
    /// Formats arguments like Go's fmt.Sprintln without the trailing newline — t.Log documents
    /// Println-style default formatting: spaces between all operands.
    /// </summary>
    public static string Sprint(ReadOnlySpan<object> args)
    {
        StringBuilder result = new();

        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0)
                result.Append(' ');
            result.Append(Default(args[i]));
        }

        return result.ToString();
    }

    /// <summary>
    /// Formats a Go format string with common-verb coverage: %v %s %q %d %x %X %c %t %e %E %f %F
    /// %g %G %T %%. Width/precision are honored for floats; other flags are parsed and ignored.
    /// Unknown verbs, missing arguments, and extra arguments render in Go's disclosure style
    /// (%!x(...), %!v(MISSING), %!(EXTRA ...)) so a formatting gap is visible, never silent.
    /// </summary>
    public static string Sprintf(string format, ReadOnlySpan<object> args)
    {
        StringBuilder result = new(format.Length + 16);
        int argIndex = 0;

        for (int i = 0; i < format.Length; i++)
        {
            char ch = format[i];

            if (ch != '%')
            {
                result.Append(ch);
                continue;
            }

            if (i + 1 >= format.Length)
            {
                result.Append("%!(NOVERB)");
                break;
            }

            // %[flags][width][.precision]verb
            i++;
            while (i < format.Length && format[i] is '+' or '-' or '#' or ' ' or '0')
                i++;

            while (i < format.Length && char.IsAsciiDigit(format[i]))
                i++;

            int precision = -1;

            if (i < format.Length && format[i] == '.')
            {
                int digitsStart = ++i;
                while (i < format.Length && char.IsAsciiDigit(format[i]))
                    i++;
                precision = i > digitsStart ? int.Parse(format[digitsStart..i], CultureInfo.InvariantCulture) : 0;
            }

            if (i >= format.Length)
            {
                result.Append("%!(NOVERB)");
                break;
            }

            char verb = format[i];

            if (verb == '%')
            {
                result.Append('%');
                continue;
            }

            if (argIndex >= args.Length)
            {
                result.Append($"%!{verb}(MISSING)");
                continue;
            }

            result.Append(Format(verb, precision, args[argIndex++]));
        }

        if (argIndex < args.Length)
        {
            result.Append("%!(EXTRA ");

            for (int i = argIndex; i < args.Length; i++)
            {
                if (i > argIndex)
                    result.Append(", ");
                result.Append(Default(args[i]));
            }

            result.Append(')');
        }

        return result.ToString();
    }

    private static string Format(char verb, int precision, object? arg)
    {
        switch (verb)
        {
            case 'v':
            case 's':
            case 'd':
            case 'w':
                return Default(arg);
            case 't':
                return arg is bool boolValue ? boolValue ? "true" : "false" : BadVerb(verb, arg);
            case 'q':
                return Quote(Default(arg));
            case 'c':
                return TryGetInt64(arg, out long rune) ? char.ConvertFromUtf32(checked((int)rune)) : BadVerb(verb, arg);
            case 'x':
            case 'X':
                return FormatHex(verb, arg);
            case 'e':
            case 'E':
            case 'f':
            case 'F':
            case 'g':
            case 'G':
                return FormatFloat(verb, precision, arg);
            case 'T':
                return TrimGoPrefix(builtin.GetGoTypeName(arg));
            case 'p':
                return arg is null ? "<nil>" : $"0x{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(arg):x}";
            default:
                return BadVerb(verb, arg);
        }
    }

    private static string BadVerb(char verb, object? arg) =>
        $"%!{verb}({TrimGoPrefix(builtin.GetGoTypeName(arg))}={Default(arg)})";

    private static string FormatHex(char verb, object? arg)
    {
        string hex;

        if (TryGetInt64(arg, out long integral))
            hex = integral.ToString("x", CultureInfo.InvariantCulture);
        else if (arg is @string or string)
            hex = Convert.ToHexStringLower(Encoding.UTF8.GetBytes(Default(arg)));
        else
            return BadVerb(verb, arg);

        return verb == 'X' ? hex.ToUpperInvariant() : hex;
    }

    private static string FormatFloat(char verb, int precision, object? arg)
    {
        double number;

        if (arg is double doubleValue)
            number = doubleValue;
        else if (arg is float floatValue)
            number = floatValue;
        else if (TryGetInt64(arg, out long integral))
            number = integral;
        else
            return BadVerb(verb, arg);

        switch (char.ToLowerInvariant(verb))
        {
            case 'f':
                return number.ToString("F" + (precision >= 0 ? precision : 6), CultureInfo.InvariantCulture);
            case 'e':
            {
                string text = number.ToString((precision >= 0 ? "0." + new string('0', precision) : "0.000000") + "e+00", CultureInfo.InvariantCulture);
                return verb == 'E' ? text.ToUpperInvariant() : text;
            }
            default: // 'g' / 'G'
                return precision >= 0
                    ? number.ToString("G" + precision, CultureInfo.InvariantCulture)
                    : number.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static string Quote(string value)
    {
        StringBuilder result = new(value.Length + 2);
        result.Append('"');

        foreach (char ch in value)
        {
            switch (ch)
            {
                case '"':
                    result.Append("\\\"");
                    break;
                case '\\':
                    result.Append("\\\\");
                    break;
                case '\n':
                    result.Append("\\n");
                    break;
                case '\r':
                    result.Append("\\r");
                    break;
                case '\t':
                    result.Append("\\t");
                    break;
                default:
                    if (char.IsControl(ch))
                        result.Append($"\\u{(int)ch:x4}");
                    else
                        result.Append(ch);
                    break;
            }
        }

        result.Append('"');
        return result.ToString();
    }

    private static bool TryGetInt64(object? arg, out long value)
    {
        switch (arg)
        {
            case sbyte or byte or short or ushort or int or uint or long or nint or nuint:
                value = Convert.ToInt64(arg, CultureInfo.InvariantCulture);
                return true;
            case ulong ulongValue when ulongValue <= long.MaxValue:
                value = (long)ulongValue;
                return true;
            default:
                value = 0L;
                return false;
        }
    }

    private static string TrimGoPrefix(string goTypeName) =>
        goTypeName.StartsWith("go.", StringComparison.Ordinal) ? goTypeName[3..] : goTypeName;

    internal static string Default(object? arg)
    {
        switch (arg)
        {
            case null:
                return "<nil>";
            case @string goString:
                return goString.ToString();
            case string text:
                return text;
            case bool boolValue:
                return boolValue ? "true" : "false";
            case error err:
                return err.Error();
        }

        // Duck-typed fmt.Stringer: the Stringer interface itself is declared in the fmt package,
        // which this shim deliberately does not reference (mixed-tree ruling in the class remarks).
        MethodInfo? stringMethod = arg.GetType().GetMethod("String", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);

        if (stringMethod is not null && stringMethod.ReturnType == typeof(@string))
            return ((@string)stringMethod.Invoke(arg, null)!).ToString();

        if (arg is IFormattable formattable)
            return formattable.ToString(null, CultureInfo.InvariantCulture);

        return arg.ToString() ?? "<nil>";
    }
}
