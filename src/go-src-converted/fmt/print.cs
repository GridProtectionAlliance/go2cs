// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmtsort = @internal.fmtsort_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using strconv = strconv_package;
using sync = sync_package;
using utf8 = unicode.utf8_package;
using @internal;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class fmt_package {

// Strings for use with buffer.WriteString.
// This is less overhead than using buffer.Write with byte arrays.
internal static readonly @string commaSpaceString = ", "u8;

internal static readonly @string nilAngleString = "<nil>"u8;

internal static readonly @string nilParenString = "(nil)"u8;

internal static readonly @string nilString = "nil"u8;

internal static readonly @string mapString = "map["u8;

internal static readonly @string percentBangString = "%!"u8;

internal static readonly @string missingString = "(MISSING)"u8;

internal static readonly @string badIndexString = "(BADINDEX)"u8;

internal static readonly @string panicString = "(PANIC="u8;

internal static readonly @string extraString = "%!(EXTRA "u8;

internal static readonly @string badWidthString = "%!(BADWIDTH)"u8;

internal static readonly @string badPrecString = "%!(BADPREC)"u8;

internal static readonly @string noVerbString = "%!(NOVERB)"u8;

internal static readonly @string invReflectString = "<invalid reflect.Value>"u8;

// State represents the printer state passed to custom formatters.
// It provides access to the [io.Writer] interface plus information about
// the flags and options for the operand's format specifier.
[GoType] partial interface State {
    // Write is the function to call to emit formatted output to be printed.
    (nint n, error err) Write(slice<byte> b);
    // Width returns the value of the width option and whether it has been set.
    (nint wid, bool ok) Width();
    // Precision returns the value of the precision option and whether it has been set.
    (nint prec, bool ok) Precision();
    // Flag reports whether the flag c, a character, has been set.
    bool Flag(nint c);
}

// Formatter is implemented by any value that has a Format method.
// The implementation controls how [State] and rune are interpreted,
// and may call [Sprint] or [Fprint](f) etc. to generate its output.
[GoType] partial interface Formatter {
    void Format(State f, rune verb);
}

// Stringer is implemented by any value that has a String method,
// which defines the “native” format for that value.
// The String method is used to print values passed as an operand
// to any format that accepts a string or to an unformatted printer
// such as [Print].
[GoType] partial interface Stringer {
    @string String();
}

// GoStringer is implemented by any value that has a GoString method,
// which defines the Go syntax for that value.
// The GoString method is used to print values passed as an operand
// to a %#v format.
[GoType] partial interface GoStringer {
    @string GoString();
}

// FormatString returns a string representing the fully qualified formatting
// directive captured by the [State], followed by the argument verb. ([State] does not
// itself contain the verb.) The result has a leading percent sign followed by any
// flags, the width, and the precision. Missing flags, width, and precision are
// omitted. This function allows a [Formatter] to reconstruct the original
// directive triggering the call to Format.
public static @string FormatString(State state, rune verb) {
    array<byte> tmp = new(16);               // Use a local buffer.
    var b = append(tmp[..0], (rune)'%');
    foreach (var (_, c) in (@string)" +-#0"u8) {
        // All known flags
        if (state.Flag(((nint)c))) {
            // The argument is an int for historical reasons.
            b = append(b, ((byte)c));
        }
    }
    {
        var (w, ok) = state.Width(); if (ok) {
            b = strconv.AppendInt(b, ((int64)w), 10);
        }
    }
    {
        var (p, ok) = state.Precision(); if (ok) {
            b = append(b, (rune)'.');
            b = strconv.AppendInt(b, ((int64)p), 10);
        }
    }
    b = utf8.AppendRune(b, verb);
    return ((@string)b);
}

[GoType("[]byte")] partial struct buffer;

[GoRecv] internal static void write(this ref buffer b, slice<byte> p) {
    b = append(b, p.ꓸꓸꓸ);
}

[GoRecv] internal static void writeString(this ref buffer b, @string s) {
    b = append(b, s.ꓸꓸꓸ);
}

[GoRecv] internal static void writeByte(this ref buffer b, byte c) {
    b = append(b, c);
}

[GoRecv] internal static void writeRune(this ref buffer b, rune r) {
    b = utf8.AppendRune(b, r);
}

// pp is used to store a printer's state and is reused with sync.Pool to avoid allocations.
[GoType] partial struct pp {
    internal buffer buf;
    // arg holds the current item, as an interface{}.
    internal any arg;
    // value is used instead of arg for reflect values.
    internal reflect_package.ΔValue value;
    // fmt is used to format basic items such as integers or strings.
    internal fmt fmt;
    // reordered records whether the format string used argument reordering.
    internal bool reordered;
    // goodArgNum records whether the most recent reordering directive was valid.
    internal bool goodArgNum;
    // panicking is set by catchPanic to avoid infinite panic, recover, panic, ... recursion.
    internal bool panicking;
    // erroring is set when printing an error string to guard against calling handleMethods.
    internal bool erroring;
    // wrapErrs is set when the format string may contain a %w verb.
    internal bool wrapErrs;
    // wrappedErrs records the targets of the %w verb.
    internal slice<nint> wrappedErrs;
}

internal static sync.Pool ppFree = new sync.Pool(
    New: () => @new<pp>()
);

// newPrinter allocates a new pp struct or grabs a cached one.
internal static ж<pp> newPrinter() {
    var p = ppFree.Get()._<pp.val>();
    p.val.panicking = false;
    p.val.erroring = false;
    p.val.wrapErrs = false;
    (~p).fmt.init(Ꮡ((~p).buf));
    return p;
}

// free saves used pp structs in ppFree; avoids an allocation per invocation.
[GoRecv] internal static void free(this ref pp p) {
    // Proper usage of a sync.Pool requires each entry to have approximately
    // the same memory cost. To obtain this property when the stored type
    // contains a variably-sized buffer, we add a hard limit on the maximum
    // buffer to place back in the pool. If the buffer is larger than the
    // limit, we drop the buffer and recycle just the printer.
    //
    // See https://golang.org/issue/23199.
    if (cap(p.buf) > 64 * 1024){
        p.buf = default!;
    } else {
        p.buf = p.buf[..0];
    }
    if (cap(p.wrappedErrs) > 8) {
        p.wrappedErrs = default!;
    }
    p.arg = default!;
    p.value = new reflectꓸValue(nil);
    p.wrappedErrs = p.wrappedErrs[..0];
    ppFree.Put(p);
}

[GoRecv] internal static (nint wid, bool ok) Width(this ref pp p) {
    nint wid = default!;
    bool ok = default!;

    return (p.fmt.wid, p.fmt.widPresent);
}

[GoRecv] internal static (nint prec, bool ok) Precision(this ref pp p) {
    nint prec = default!;
    bool ok = default!;

    return (p.fmt.prec, p.fmt.precPresent);
}

[GoRecv] internal static bool Flag(this ref pp p, nint b) {
    switch (b) {
    case (rune)'-': {
        return p.fmt.minus;
    }
    case (rune)'+': {
        return p.fmt.plus || p.fmt.plusV;
    }
    case (rune)'#': {
        return p.fmt.sharp || p.fmt.sharpV;
    }
    case (rune)' ': {
        return p.fmt.space;
    }
    case (rune)'0': {
        return p.fmt.zero;
    }}

    return false;
}

// Implement Write so we can call [Fprintf] on a pp (through [State]), for
// recursive use in custom verbs.
[GoRecv] internal static (nint ret, error err) Write(this ref pp p, slice<byte> b) {
    nint ret = default!;
    error err = default!;

    p.buf.write(b);
    return (len(b), default!);
}

// Implement WriteString so that we can call [io.WriteString]
// on a pp (through state), for efficiency.
[GoRecv] internal static (nint ret, error err) WriteString(this ref pp p, @string s) {
    nint ret = default!;
    error err = default!;

    p.buf.writeString(s);
    return (len(s), default!);
}

// These routines end in 'f' and take a format string.

// Fprintf formats according to a format specifier and writes to w.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Fprintf(io.Writer w, @string format, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintf(format, a);
    (n, err) = w.Write((~p).buf);
    p.free();
    return (n, err);
}

// Printf formats according to a format specifier and writes to standard output.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Printf(@string format, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    return Fprintf(~os.Stdout, format, a.ꓸꓸꓸ);
}

// Sprintf formats according to a format specifier and returns the resulting string.
public static @string Sprintf(@string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintf(format, a);
    @string s = ((@string)(~p).buf);
    p.free();
    return s;
}

// Appendf formats according to a format specifier, appends the result to the byte
// slice, and returns the updated slice.
public static slice<byte> Appendf(slice<byte> b, @string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintf(format, a);
    b = append(b, (~p).buf.ꓸꓸꓸ);
    p.free();
    return b;
}

// These routines do not take a format string

// Fprint formats using the default formats for its operands and writes to w.
// Spaces are added between operands when neither is a string.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Fprint(io.Writer w, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrint(a);
    (n, err) = w.Write((~p).buf);
    p.free();
    return (n, err);
}

// Print formats using the default formats for its operands and writes to standard output.
// Spaces are added between operands when neither is a string.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Print(params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    return Fprint(~os.Stdout, a.ꓸꓸꓸ);
}

// Sprint formats using the default formats for its operands and returns the resulting string.
// Spaces are added between operands when neither is a string.
public static @string Sprint(params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrint(a);
    @string s = ((@string)(~p).buf);
    p.free();
    return s;
}

// Append formats using the default formats for its operands, appends the result to
// the byte slice, and returns the updated slice.
public static slice<byte> Append(slice<byte> b, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrint(a);
    b = append(b, (~p).buf.ꓸꓸꓸ);
    p.free();
    return b;
}

// These routines end in 'ln', do not take a format string,
// always add spaces between operands, and add a newline
// after the last operand.

// Fprintln formats using the default formats for its operands and writes to w.
// Spaces are always added between operands and a newline is appended.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Fprintln(io.Writer w, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintln(a);
    (n, err) = w.Write((~p).buf);
    p.free();
    return (n, err);
}

// Println formats using the default formats for its operands and writes to standard output.
// Spaces are always added between operands and a newline is appended.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Println(params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    return Fprintln(~os.Stdout, a.ꓸꓸꓸ);
}

// Sprintln formats using the default formats for its operands and returns the resulting string.
// Spaces are always added between operands and a newline is appended.
public static @string Sprintln(params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintln(a);
    @string s = ((@string)(~p).buf);
    p.free();
    return s;
}

// Appendln formats using the default formats for its operands, appends the result
// to the byte slice, and returns the updated slice. Spaces are always added
// between operands and a newline is appended.
public static slice<byte> Appendln(slice<byte> b, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintln(a);
    b = append(b, (~p).buf.ꓸꓸꓸ);
    p.free();
    return b;
}

// getField gets the i'th field of the struct value.
// If the field itself is a non-nil interface, return a value for
// the thing inside the interface, not the interface itself.
internal static reflectꓸValue getField(reflectꓸValue v, nint i) {
    var val = v.Field(i);
    if (val.Kind() == reflect.ΔInterface && !val.IsNil()) {
        val = val.Elem();
    }
    return val;
}

// tooLarge reports whether the magnitude of the integer is
// too large to be used as a formatting width or precision.
internal static bool tooLarge(nint x) {
    const nint max = 1000000;
    return x > max || x < -max;
}

// parsenum converts ASCII to integer.  num is 0 (and isnum is false) if no number present.
internal static (nint num, bool isnum, nint newi) parsenum(@string s, nint start, nint end) {
    nint num = default!;
    bool isnum = default!;
    nint newi = default!;

    if (start >= end) {
        return (0, false, end);
    }
    for (newi = start; newi < end && (rune)'0' <= s[newi] && s[newi] <= (rune)'9'; newi++) {
        if (tooLarge(num)) {
            return (0, false, end);
        }
        // Overflow; crazy long number most likely.
        num = num * 10 + ((nint)(s[newi] - (rune)'0'));
        isnum = true;
    }
    return (num, isnum, newi);
}

[GoRecv] internal static void unknownType(this ref pp p, reflectꓸValue v) {
    if (!v.IsValid()) {
        p.buf.writeString(nilAngleString);
        return;
    }
    p.buf.writeByte((rune)'?');
    p.buf.writeString(v.Type().String());
    p.buf.writeByte((rune)'?');
}

[GoRecv] internal static void badVerb(this ref pp p, rune verb) {
    p.erroring = true;
    p.buf.writeString(percentBangString);
    p.buf.writeRune(verb);
    p.buf.writeByte((rune)'(');
    switch (ᐧ) {
    case {} when p.arg != default!: {
        p.buf.writeString(reflect.TypeOf(p.arg).String());
        p.buf.writeByte((rune)'=');
        p.printArg(p.arg, (rune)'v');
        break;
    }
    case {} when p.value.IsValid(): {
        p.buf.writeString(p.value.Type().String());
        p.buf.writeByte((rune)'=');
        p.printValue(p.value, (rune)'v', 0);
        break;
    }
    default: {
        p.buf.writeString(nilAngleString);
        break;
    }}

    p.buf.writeByte((rune)')');
    p.erroring = false;
}

[GoRecv] internal static void fmtBool(this ref pp p, bool v, rune verb) {
    switch (verb) {
    case (rune)'t' or (rune)'v': {
        p.fmt.fmtBoolean(v);
        break;
    }
    default: {
        p.badVerb(verb);
        break;
    }}

}

// fmt0x64 formats a uint64 in hexadecimal and prefixes it with 0x or
// not, as requested, by temporarily setting the sharp flag.
[GoRecv] internal static void fmt0x64(this ref pp p, uint64 v, bool leading0x) {
    var sharp = p.fmt.sharp;
    p.fmt.sharp = leading0x;
    p.fmt.fmtInteger(v, 16, unsigned, (rune)'v', ldigits);
    p.fmt.sharp = sharp;
}

// fmtInteger formats a signed or unsigned integer.
[GoRecv] internal static void fmtInteger(this ref pp p, uint64 v, bool isSigned, rune verb) {
    switch (verb) {
    case (rune)'v': {
        if (p.fmt.sharpV && !isSigned){
            p.fmt0x64(v, true);
        } else {
            p.fmt.fmtInteger(v, 10, isSigned, verb, ldigits);
        }
        break;
    }
    case (rune)'d': {
        p.fmt.fmtInteger(v, 10, isSigned, verb, ldigits);
        break;
    }
    case (rune)'b': {
        p.fmt.fmtInteger(v, 2, isSigned, verb, ldigits);
        break;
    }
    case (rune)'o' or (rune)'O': {
        p.fmt.fmtInteger(v, 8, isSigned, verb, ldigits);
        break;
    }
    case (rune)'x': {
        p.fmt.fmtInteger(v, 16, isSigned, verb, ldigits);
        break;
    }
    case (rune)'X': {
        p.fmt.fmtInteger(v, 16, isSigned, verb, udigits);
        break;
    }
    case (rune)'c': {
        p.fmt.fmtC(v);
        break;
    }
    case (rune)'q': {
        p.fmt.fmtQc(v);
        break;
    }
    case (rune)'U': {
        p.fmt.fmtUnicode(v);
        break;
    }
    default: {
        p.badVerb(verb);
        break;
    }}

}

// fmtFloat formats a float. The default precision for each verb
// is specified as last argument in the call to fmt_float.
[GoRecv] internal static void fmtFloat(this ref pp p, float64 v, nint size, rune verb) {
    switch (verb) {
    case (rune)'v': {
        p.fmt.fmtFloat(v, size, (rune)'g', -1);
        break;
    }
    case (rune)'b' or (rune)'g' or (rune)'G' or (rune)'x' or (rune)'X': {
        p.fmt.fmtFloat(v, size, verb, -1);
        break;
    }
    case (rune)'f' or (rune)'e' or (rune)'E': {
        p.fmt.fmtFloat(v, size, verb, 6);
        break;
    }
    case (rune)'F': {
        p.fmt.fmtFloat(v, size, (rune)'f', 6);
        break;
    }
    default: {
        p.badVerb(verb);
        break;
    }}

}

// fmtComplex formats a complex number v with
// r = real(v) and j = imag(v) as (r+ji) using
// fmtFloat for r and j formatting.
[GoRecv] internal static void fmtComplex(this ref pp p, complex128 v, nint size, rune verb) {
    // Make sure any unsupported verbs are found before the
    // calls to fmtFloat to not generate an incorrect error string.
    switch (verb) {
    case (rune)'v' or (rune)'b' or (rune)'g' or (rune)'G' or (rune)'x' or (rune)'X' or (rune)'f' or (rune)'F' or (rune)'e' or (rune)'E': {
        var oldPlus = p.fmt.plus;
        p.buf.writeByte((rune)'(');
        p.fmtFloat(real(v), size / 2, verb);
        p.fmt.plus = true;
        p.fmtFloat(imag(v), // Imaginary part always has a sign.
 size / 2, verb);
        p.buf.writeString("i)"u8);
        p.fmt.plus = oldPlus;
        break;
    }
    default: {
        p.badVerb(verb);
        break;
    }}

}

[GoRecv] internal static void fmtString(this ref pp p, @string v, rune verb) {
    switch (verb) {
    case (rune)'v': {
        if (p.fmt.sharpV){
            p.fmt.fmtQ(v);
        } else {
            p.fmt.fmtS(v);
        }
        break;
    }
    case (rune)'s': {
        p.fmt.fmtS(v);
        break;
    }
    case (rune)'x': {
        p.fmt.fmtSx(v, ldigits);
        break;
    }
    case (rune)'X': {
        p.fmt.fmtSx(v, udigits);
        break;
    }
    case (rune)'q': {
        p.fmt.fmtQ(v);
        break;
    }
    default: {
        p.badVerb(verb);
        break;
    }}

}

[GoRecv] internal static void fmtBytes(this ref pp p, slice<byte> v, rune verb, @string typeString) {
    switch (verb) {
    case (rune)'v' or (rune)'d': {
        if (p.fmt.sharpV){
            p.buf.writeString(typeString);
            if (v == default!) {
                p.buf.writeString(nilParenString);
                return;
            }
            p.buf.writeByte((rune)'{');
            foreach (var (i, c) in v) {
                if (i > 0) {
                    p.buf.writeString(commaSpaceString);
                }
                p.fmt0x64(((uint64)c), true);
            }
            p.buf.writeByte((rune)'}');
        } else {
            p.buf.writeByte((rune)'[');
            foreach (var (i, c) in v) {
                if (i > 0) {
                    p.buf.writeByte((rune)' ');
                }
                p.fmt.fmtInteger(((uint64)c), 10, unsigned, verb, ldigits);
            }
            p.buf.writeByte((rune)']');
        }
        break;
    }
    case (rune)'s': {
        p.fmt.fmtBs(v);
        break;
    }
    case (rune)'x': {
        p.fmt.fmtBx(v, ldigits);
        break;
    }
    case (rune)'X': {
        p.fmt.fmtBx(v, udigits);
        break;
    }
    case (rune)'q': {
        p.fmt.fmtQ(((@string)v));
        break;
    }
    default: {
        p.printValue(reflect.ValueOf(v), verb, 0);
        break;
    }}

}

[GoRecv] internal static void fmtPointer(this ref pp p, reflectꓸValue value, rune verb) {
    uintptr u = default!;
    var exprᴛ1 = value.Kind();
    if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.ΔSlice || exprᴛ1 == reflect.ΔUnsafePointer) {
        u = ((uintptr)(uintptr)value.UnsafePointer());
    }
    else { /* default: */
        p.badVerb(verb);
        return;
    }

    switch (verb) {
    case (rune)'v': {
        if (p.fmt.sharpV){
            p.buf.writeByte((rune)'(');
            p.buf.writeString(value.Type().String());
            p.buf.writeString(")("u8);
            if (u == 0){
                p.buf.writeString(nilString);
            } else {
                p.fmt0x64(((uint64)u), true);
            }
            p.buf.writeByte((rune)')');
        } else {
            if (u == 0){
                p.fmt.padString(nilAngleString);
            } else {
                p.fmt0x64(((uint64)u), !p.fmt.sharp);
            }
        }
        break;
    }
    case (rune)'p': {
        p.fmt0x64(((uint64)u), !p.fmt.sharp);
        break;
    }
    case (rune)'b' or (rune)'o' or (rune)'d' or (rune)'x' or (rune)'X': {
        p.fmtInteger(((uint64)u), unsigned, verb);
        break;
    }
    default: {
        p.badVerb(verb);
        break;
    }}

}

[GoRecv] internal static void catchPanic(this ref pp p, any arg, rune verb, @string method) => func((_, recover) => {
    {
        var err = recover(); if (err != default!) {
            // If it's a nil pointer, just say "<nil>". The likeliest causes are a
            // Stringer that fails to guard against nil or a nil pointer for a
            // value receiver, and in either case, "<nil>" is a nice result.
            {
                var v = reflect.ValueOf(arg); if (v.Kind() == reflect.ΔPointer && v.IsNil()) {
                    p.buf.writeString(nilAngleString);
                    return;
                }
            }
            // Otherwise print a concise panic message. Most of the time the panic
            // value will print itself nicely.
            if (p.panicking) {
                // Nested panics; the recursion in printArg cannot succeed.
                throw panic(err);
            }
            var oldFlags = p.fmt.fmtFlags;
            // For this output we want default behavior.
            p.fmt.clearflags();
            p.buf.writeString(percentBangString);
            p.buf.writeRune(verb);
            p.buf.writeString(panicString);
            p.buf.writeString(method);
            p.buf.writeString(" method: "u8);
            p.panicking = true;
            p.printArg(err, (rune)'v');
            p.panicking = false;
            p.buf.writeByte((rune)')');
            p.fmt.fmtFlags = oldFlags;
        }
    }
});

[GoRecv] internal static bool /*handled*/ handleMethods(this ref pp p, rune verb) => func((defer, _) => {
    bool handled = default!;

    if (p.erroring) {
        return handled;
    }
    if (verb == (rune)'w') {
        // It is invalid to use %w other than with Errorf or with a non-error arg.
        var (_, ok) = p.arg._<error>(ᐧ);
        if (!ok || !p.wrapErrs) {
            p.badVerb(verb);
            return true;
        }
        // If the arg is a Formatter, pass 'v' as the verb to it.
        verb = (rune)'v';
    }
    // Is it a Formatter?
    {
        var (formatter, ok) = p.arg._<Formatter>(ᐧ); if (ok) {
            handled = true;
            deferǃ(p.catchPanic, p.arg, verb, "Format", defer);
            formatter.Format(~p, verb);
            return handled;
        }
    }
    // If we're doing Go syntax and the argument knows how to supply it, take care of it now.
    if (p.fmt.sharpV){
        {
            var (stringer, ok) = p.arg._<GoStringer>(ᐧ); if (ok) {
                handled = true;
                deferǃ(p.catchPanic, p.arg, verb, "GoString", defer);
                // Print the result of GoString unadorned.
                p.fmt.fmtS(stringer.GoString());
                return handled;
            }
        }
    } else {
        // If a string is acceptable according to the format, see if
        // the value satisfies one of the string-valued interfaces.
        // Println etc. set verb to %v, which is "stringable".
        switch (verb) {
        case (rune)'v' or (rune)'s' or (rune)'x' or (rune)'X' or (rune)'q': {
            switch (p.arg.type()) {
            case error v: {
                handled = true;
                deferǃ(p.catchPanic, p.arg, verb, "Error", defer);
                p.fmtString(v.Error(), verb);
                return handled;
            }
            case Stringer v: {
                handled = true;
                deferǃ(p.catchPanic, p.arg, verb, "String", defer);
                p.fmtString(v.String(), verb);
                return handled;
            }}
            break;
        }}

    }
    return false;
});

[GoRecv] internal static void printArg(this ref pp p, any arg, rune verb) {
    p.arg = arg;
    p.value = new reflectꓸValue(nil);
    if (arg == default!) {
        switch (verb) {
        case (rune)'T' or (rune)'v': {
            p.fmt.padString(nilAngleString);
            break;
        }
        default: {
            p.badVerb(verb);
            break;
        }}

        return;
    }
    // Special processing considerations.
    // %T (the value's type) and %p (its address) are special; we always do them first.
    switch (verb) {
    case (rune)'T': {
        p.fmt.fmtS(reflect.TypeOf(arg).String());
        return;
    }
    case (rune)'p': {
        p.fmtPointer(reflect.ValueOf(arg), (rune)'p');
        return;
    }}

    // Some types can be done without reflection.
    switch (arg.type()) {
    case bool f: {
        p.fmtBool(f, verb);
        break;
    }
    case float32 f: {
        p.fmtFloat(((float64)f), 32, verb);
        break;
    }
    case float64 f: {
        p.fmtFloat(f, 64, verb);
        break;
    }
    case complex64 f: {
        p.fmtComplex(((complex128)f), 64, verb);
        break;
    }
    case complex128 f: {
        p.fmtComplex(f, 128, verb);
        break;
    }
    case nint f: {
        p.fmtInteger(((uint64)f), signed, verb);
        break;
    }
    case int32 f: {
        p.fmtInteger(((uint64)f), signed, verb);
        break;
    }
    case int8 f: {
        p.fmtInteger(((uint64)f), signed, verb);
        break;
    }
    case int16 f: {
        p.fmtInteger(((uint64)f), signed, verb);
        break;
    }
    case int32 f: {
        p.fmtInteger(((uint64)f), signed, verb);
        break;
    }
    case int64 f: {
        p.fmtInteger(((uint64)f), signed, verb);
        break;
    }
    case nuint f: {
        p.fmtInteger(((uint64)f), unsigned, verb);
        break;
    }
    case uint32 f: {
        p.fmtInteger(((uint64)f), unsigned, verb);
        break;
    }
    case uint8 f: {
        p.fmtInteger(((uint64)f), unsigned, verb);
        break;
    }
    case uint16 f: {
        p.fmtInteger(((uint64)f), unsigned, verb);
        break;
    }
    case uint32 f: {
        p.fmtInteger(((uint64)f), unsigned, verb);
        break;
    }
    case uint64 f: {
        p.fmtInteger(f, unsigned, verb);
        break;
    }
    case uintptr f: {
        p.fmtInteger(((uint64)f), unsigned, verb);
        break;
    }
    case @string f: {
        p.fmtString(f, verb);
        break;
    }
    case slice<byte> f: {
        p.fmtBytes(f, verb, "[]byte"u8);
        break;
    }
    case reflectꓸValue f: {
        if (f.IsValid() && f.CanInterface()) {
            // Handle extractable values with special methods
            // since printValue does not handle them at depth 0.
            p.arg = f.Interface();
            if (p.handleMethods(verb)) {
                return;
            }
        }
        p.printValue(f, verb, 0);
        break;
    }
    default: {
        var f = arg.type();
        if (!p.handleMethods(verb)) {
            // If the type is not simple, it might have methods.
            // Need to use reflection, since the type had no
            // interface methods that could be used for formatting.
            p.printValue(reflect.ValueOf(f), verb, 0);
        }
        break;
    }}
}

// printValue is similar to printArg but starts with a reflect value, not an interface{} value.
// It does not handle 'p' and 'T' verbs because these should have been already handled by printArg.
[GoRecv] internal static void printValue(this ref pp p, reflectꓸValue value, rune verb, nint depth) {
    // Handle values with special methods if not already handled by printArg (depth == 0).
    if (depth > 0 && value.IsValid() && value.CanInterface()) {
        p.arg = value.Interface();
        if (p.handleMethods(verb)) {
            return;
        }
    }
    p.arg = default!;
    p.value = value;
    {
        var f = value;
        var exprᴛ1 = value.Kind();
        var matchᴛ1 = false;
        if (exprᴛ1 == reflect.Invalid) { matchᴛ1 = true;
            if (depth == 0){
                p.buf.writeString(invReflectString);
            } else {
                switch (verb) {
                case (rune)'v': {
                    p.buf.writeString(nilAngleString);
                    break;
                }
                default: {
                    p.badVerb(verb);
                    break;
                }}

            }
        }
        else if (exprᴛ1 == reflect.ΔBool) { matchᴛ1 = true;
            p.fmtBool(f.Bool(), verb);
        }
        else if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) { matchᴛ1 = true;
            p.fmtInteger(((uint64)f.Int()), signed, verb);
        }
        else if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) { matchᴛ1 = true;
            p.fmtInteger(f.Uint(), unsigned, verb);
        }
        else if (exprᴛ1 == reflect.Float32) { matchᴛ1 = true;
            p.fmtFloat(f.Float(), 32, verb);
        }
        else if (exprᴛ1 == reflect.Float64) { matchᴛ1 = true;
            p.fmtFloat(f.Float(), 64, verb);
        }
        else if (exprᴛ1 == reflect.Complex64) { matchᴛ1 = true;
            p.fmtComplex(f.Complex(), 64, verb);
        }
        else if (exprᴛ1 == reflect.Complex128) { matchᴛ1 = true;
            p.fmtComplex(f.Complex(), 128, verb);
        }
        else if (exprᴛ1 == reflect.ΔString) { matchᴛ1 = true;
            p.fmtString(f.String(), verb);
        }
        else if (exprᴛ1 == reflect.Map) { matchᴛ1 = true;
            if (p.fmt.sharpV){
                p.buf.writeString(f.Type().String());
                if (f.IsNil()) {
                    p.buf.writeString(nilParenString);
                    return;
                }
                p.buf.writeByte((rune)'{');
            } else {
                p.buf.writeString(mapString);
            }
            var sorted = fmtsort.Sort(f);
            foreach (var (i, m) in sorted) {
                if (i > 0) {
                    if (p.fmt.sharpV){
                        p.buf.writeString(commaSpaceString);
                    } else {
                        p.buf.writeByte((rune)' ');
                    }
                }
                p.printValue(m.Key, verb, depth + 1);
                p.buf.writeByte((rune)':');
                p.printValue(m.Value, verb, depth + 1);
            }
            if (p.fmt.sharpV){
                p.buf.writeByte((rune)'}');
            } else {
                p.buf.writeByte((rune)']');
            }
        }
        else if (exprᴛ1 == reflect.Struct) { matchᴛ1 = true;
            if (p.fmt.sharpV) {
                p.buf.writeString(f.Type().String());
            }
            p.buf.writeByte((rune)'{');
            for (nint i = 0; i < f.NumField(); i++) {
                if (i > 0) {
                    if (p.fmt.sharpV){
                        p.buf.writeString(commaSpaceString);
                    } else {
                        p.buf.writeByte((rune)' ');
                    }
                }
                if (p.fmt.plusV || p.fmt.sharpV) {
                    {
                        @string name = f.Type().Field(i).Name; if (name != ""u8) {
                            p.buf.writeString(name);
                            p.buf.writeByte((rune)':');
                        }
                    }
                }
                p.printValue(getField(f, i), verb, depth + 1);
            }
            p.buf.writeByte((rune)'}');
        }
        else if (exprᴛ1 == reflect.ΔInterface) { matchᴛ1 = true;
            var valueΔ2 = f.Elem();
            if (!valueΔ2.IsValid()){
                if (p.fmt.sharpV){
                    p.buf.writeString(f.Type().String());
                    p.buf.writeString(nilParenString);
                } else {
                    p.buf.writeString(nilAngleString);
                }
            } else {
                p.printValue(valueΔ2, verb, depth + 1);
            }
        }
        else if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) { matchᴛ1 = true;
            switch (verb) {
            case (rune)'s' or (rune)'q' or (rune)'x' or (rune)'X': {
                var t = f.Type();
                if (t.Elem().Kind() == reflect.Uint8) {
                    // Handle byte and uint8 slices and arrays special for the above verbs.
                    slice<byte> bytes = default!;
                    if (f.Kind() == reflect.ΔSlice || f.CanAddr()){
                        bytes = f.Bytes();
                    } else {
                        // We have an array, but we cannot Bytes() a non-addressable array,
                        // so we build a slice by hand. This is a rare case but it would be nice
                        // if reflection could help a little more.
                        bytes = new slice<byte>(f.Len());
                        foreach (var (i, _) in bytes) {
                            bytes[i] = ((byte)f.Index(i).Uint());
                        }
                    }
                    p.fmtBytes(bytes, verb, t.String());
                    return;
                }
                break;
            }}

            if (p.fmt.sharpV){
                p.buf.writeString(f.Type().String());
                if (f.Kind() == reflect.ΔSlice && f.IsNil()) {
                    p.buf.writeString(nilParenString);
                    return;
                }
                p.buf.writeByte((rune)'{');
                for (nint i = 0; i < f.Len(); i++) {
                    if (i > 0) {
                        p.buf.writeString(commaSpaceString);
                    }
                    p.printValue(f.Index(i), verb, depth + 1);
                }
                p.buf.writeByte((rune)'}');
            } else {
                p.buf.writeByte((rune)'[');
                for (nint i = 0; i < f.Len(); i++) {
                    if (i > 0) {
                        p.buf.writeByte((rune)' ');
                    }
                    p.printValue(f.Index(i), verb, depth + 1);
                }
                p.buf.writeByte((rune)']');
            }
        }
        else if (exprᴛ1 == reflect.ΔPointer) { matchᴛ1 = true;
            if (depth == 0 && (uintptr)f.UnsafePointer() != nil) {
                // pointer to array or slice or struct? ok at top level
                // but not embedded (avoid loops)
                {
                    var a = f.Elem();
                    var exprᴛ2 = a.Kind();
                    if (exprᴛ2 == reflect.Array || exprᴛ2 == reflect.ΔSlice || exprᴛ2 == reflect.Struct || exprᴛ2 == reflect.Map) {
                        p.buf.writeByte((rune)'&');
                        p.printValue(a, verb, depth + 1);
                        return;
                    }
                }

            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func || exprᴛ1 == reflect.ΔUnsafePointer)) {
            p.fmtPointer(f, verb);
        }
        else { /* default: */
            p.unknownType(f);
        }
    }

}

// intFromArg gets the argNumth element of a. On return, isInt reports whether the argument has integer type.
internal static (nint num, bool isInt, nint newArgNum) intFromArg(slice<any> a, nint argNum) {
    nint num = default!;
    bool isInt = default!;
    nint newArgNum = default!;

    newArgNum = argNum;
    if (argNum < len(a)) {
        (num, isInt) = a[argNum]._<nint>(ᐧ);
        // Almost always OK.
        if (!isInt) {
            // Work harder.
            {
                var v = reflect.ValueOf(a[argNum]);
                var exprᴛ1 = v.Kind();
                if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
                    var n = v.Int();
                    if (((int64)((nint)n)) == n) {
                        num = ((nint)n);
                        isInt = true;
                    }
                }
                else if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
                    var n = v.Uint();
                    if (((int64)n) >= 0 && ((uint64)((nint)n)) == n) {
                        num = ((nint)n);
                        isInt = true;
                    }
                }
                else { /* default: */
                }
            }

        }
        // Already 0, false.
        newArgNum = argNum + 1;
        if (tooLarge(num)) {
            num = 0;
            isInt = false;
        }
    }
    return (num, isInt, newArgNum);
}

// parseArgNumber returns the value of the bracketed number, minus 1
// (explicit argument numbers are one-indexed but we want zero-indexed).
// The opening bracket is known to be present at format[0].
// The returned values are the index, the number of bytes to consume
// up to the closing paren, if present, and whether the number parsed
// ok. The bytes to consume will be 1 if no closing paren is present.
internal static (nint index, nint wid, bool ok) parseArgNumber(@string format) {
    nint index = default!;
    nint wid = default!;
    bool ok = default!;

    // There must be at least 3 bytes: [n].
    if (len(format) < 3) {
        return (0, 1, false);
    }
    // Find closing bracket.
    for (nint i = 1; i < len(format); i++) {
        if (format[i] == (rune)']') {
            var (width, okΔ1, newi) = parsenum(format, 1, i);
            if (!okΔ1 || newi != i) {
                return (0, i + 1, false);
            }
            return (width - 1, i + 1, true);
        }
    }
    // arg numbers are one-indexed and skip paren.
    return (0, 1, false);
}

// argNumber returns the next argument to evaluate, which is either the value of the passed-in
// argNum or the value of the bracketed integer that begins format[i:]. It also returns
// the new value of i, that is, the index of the next byte of the format to process.
[GoRecv] internal static (nint newArgNum, nint newi, bool found) argNumber(this ref pp p, nint argNum, @string format, nint i, nint numArgs) {
    nint newArgNum = default!;
    nint newi = default!;
    bool found = default!;

    if (len(format) <= i || format[i] != (rune)'[') {
        return (argNum, i, false);
    }
    p.reordered = true;
    var (index, wid, ok) = parseArgNumber(format[(int)(i)..]);
    if (ok && 0 <= index && index < numArgs) {
        return (index, i + wid, true);
    }
    p.goodArgNum = false;
    return (argNum, i + wid, ok);
}

[GoRecv] internal static void badArgNum(this ref pp p, rune verb) {
    p.buf.writeString(percentBangString);
    p.buf.writeRune(verb);
    p.buf.writeString(badIndexString);
}

[GoRecv] internal static void missingArg(this ref pp p, rune verb) {
    p.buf.writeString(percentBangString);
    p.buf.writeRune(verb);
    p.buf.writeString(missingString);
}

[GoRecv] internal static void doPrintf(this ref pp p, @string format, slice<any> a) {
    nint end = len(format);
    nint argNum = 0;
    // we process one argument per non-trivial format
    var afterIndex = false;
    // previous item in format was an index like [3].
    p.reordered = false;
formatLoop:
    for (nint i = 0; i < end; ) {
        p.goodArgNum = true;
        nint lasti = i;
        while (i < end && format[i] != (rune)'%') {
            i++;
        }
        if (i > lasti) {
            p.buf.writeString(format[(int)(lasti)..(int)(i)]);
        }
        if (i >= end) {
            // done processing format string
            break;
        }
        // Process one verb
        i++;
        // Do we have flags?
        p.fmt.clearflags();
simpleFormat:
        for (; i < end; i++) {
            var c = format[i];
            switch (c) {
            case (rune)'#': {
                p.fmt.sharp = true;
                break;
            }
            case (rune)'0': {
                p.fmt.zero = true;
                break;
            }
            case (rune)'+': {
                p.fmt.plus = true;
                break;
            }
            case (rune)'-': {
                p.fmt.minus = true;
                break;
            }
            case (rune)' ': {
                p.fmt.space = true;
                break;
            }
            default: {
                if ((rune)'a' <= c && c <= (rune)'z' && argNum < len(a)) {
                    // Fast path for common case of ascii lower case simple verbs
                    // without precision or width or argument indices.
                    var exprᴛ1 = c;
                    var matchᴛ1 = false;
                    if (exprᴛ1 is (rune)'w') {
                        p.wrappedErrs = append(p.wrappedErrs, argNum);
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ1 && exprᴛ1 is (rune)'v')) { matchᴛ1 = true;
                        p.fmt.sharpV = p.fmt.sharp;
                        p.fmt.sharp = false;
                        p.fmt.plusV = p.fmt.plus;
                        p.fmt.plus = false;
                    }

                    // Go syntax
                    // Struct-field syntax
                    p.printArg(a[argNum], ((rune)c));
                    argNum++;
                    i++;
                    goto continue_formatLoop;
                }
                goto break_simpleFormat;
                break;
            }}

continue_simpleFormat:;
        }
break_simpleFormat:;
        // Format is more complex than simple flags and a verb or is malformed.
        // Do we have an explicit argument index?
        (argNum, i, afterIndex) = p.argNumber(argNum, format, i, len(a));
        // Do we have width?
        if (i < end && format[i] == (rune)'*'){
            i++;
            (p.fmt.wid, p.fmt.widPresent, argNum) = intFromArg(a, argNum);
            if (!p.fmt.widPresent) {
                p.buf.writeString(badWidthString);
            }
            // We have a negative width, so take its value and ensure
            // that the minus flag is set
            if (p.fmt.wid < 0) {
                p.fmt.wid = -p.fmt.wid;
                p.fmt.minus = true;
                p.fmt.zero = false;
            }
            // Do not pad with zeros to the right.
            afterIndex = false;
        } else {
            (p.fmt.wid, p.fmt.widPresent, i) = parsenum(format, i, end);
            if (afterIndex && p.fmt.widPresent) {
                // "%[3]2d"
                p.goodArgNum = false;
            }
        }
        // Do we have precision?
        if (i + 1 < end && format[i] == (rune)'.') {
            i++;
            if (afterIndex) {
                // "%[3].2d"
                p.goodArgNum = false;
            }
            (argNum, i, afterIndex) = p.argNumber(argNum, format, i, len(a));
            if (i < end && format[i] == (rune)'*'){
                i++;
                (p.fmt.prec, p.fmt.precPresent, argNum) = intFromArg(a, argNum);
                // Negative precision arguments don't make sense
                if (p.fmt.prec < 0) {
                    p.fmt.prec = 0;
                    p.fmt.precPresent = false;
                }
                if (!p.fmt.precPresent) {
                    p.buf.writeString(badPrecString);
                }
                afterIndex = false;
            } else {
                (p.fmt.prec, p.fmt.precPresent, i) = parsenum(format, i, end);
                if (!p.fmt.precPresent) {
                    p.fmt.prec = 0;
                    p.fmt.precPresent = true;
                }
            }
        }
        if (!afterIndex) {
            (argNum, i, afterIndex) = p.argNumber(argNum, format, i, len(a));
        }
        if (i >= end) {
            p.buf.writeString(noVerbString);
            break;
        }
        var verb = ((rune)format[i]);
        nint size = 1;
        if (verb >= utf8.RuneSelf) {
            (verb, size) = utf8.DecodeRuneInString(format[(int)(i)..]);
        }
        i += size;
        var matchᴛ2 = false;
        if (verb is (rune)'%') { matchᴛ2 = true;
            p.buf.writeByte((rune)'%');
        }
        else if (!p.goodArgNum) { matchᴛ2 = true;
            p.badArgNum(verb);
        }
        else if (argNum >= len(a)) { matchᴛ2 = true;
            p.missingArg(verb);
        }
        else if (verb is (rune)'w') { matchᴛ2 = true;
            p.wrappedErrs = append(p.wrappedErrs, // Percent does not absorb operands and ignores f.wid and f.prec.
 // No argument left over to print for the current verb.
 argNum);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && verb is (rune)'v')) { matchᴛ2 = true;
            p.fmt.sharpV = p.fmt.sharp;
            p.fmt.sharp = false;
            p.fmt.plusV = p.fmt.plus;
            p.fmt.plus = false;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2) { /* default: */
            p.printArg(a[argNum], // Go syntax
 // Struct-field syntax
 verb);
            argNum++;
        }

continue_formatLoop:;
    }
break_formatLoop:;
    // Check for extra arguments unless the call accessed the arguments
    // out of order, in which case it's too expensive to detect if they've all
    // been used and arguably OK if they're not.
    if (!p.reordered && argNum < len(a)) {
        p.fmt.clearflags();
        p.buf.writeString(extraString);
        foreach (var (i, arg) in a[(int)(argNum)..]) {
            if (i > 0) {
                p.buf.writeString(commaSpaceString);
            }
            if (arg == default!){
                p.buf.writeString(nilAngleString);
            } else {
                p.buf.writeString(reflect.TypeOf(arg).String());
                p.buf.writeByte((rune)'=');
                p.printArg(arg, (rune)'v');
            }
        }
        p.buf.writeByte((rune)')');
    }
}

[GoRecv] internal static void doPrint(this ref pp p, slice<any> a) {
    var prevString = false;
    foreach (var (argNum, arg) in a) {
        var isString = arg != default! && reflect.TypeOf(arg).Kind() == reflect.ΔString;
        // Add a space between two non-string arguments.
        if (argNum > 0 && !isString && !prevString) {
            p.buf.writeByte((rune)' ');
        }
        p.printArg(arg, (rune)'v');
        prevString = isString;
    }
}

// doPrintln is like doPrint but always adds a space between arguments
// and a newline after the last argument.
[GoRecv] internal static void doPrintln(this ref pp p, slice<any> a) {
    foreach (var (argNum, arg) in a) {
        if (argNum > 0) {
            p.buf.writeByte((rune)' ');
        }
        p.printArg(arg, (rune)'v');
    }
    p.buf.writeByte((rune)'\n');
}

} // end fmt_package
