// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fmt -- go2cs converted at 2020 August 29 08:45:11 UTC
// import "fmt" ==> using fmt = go.fmt_package
// Original source: C:\Go\src\fmt\print.go
using errors = go.errors_package;
using io = go.io_package;
using os = go.os_package;
using reflect = go.reflect_package;
using sync = go.sync_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class fmt_package
    {
        // Strings for use with buffer.WriteString.
        // This is less overhead than using buffer.Write with byte arrays.
        private static readonly @string commaSpaceString = ", ";
        private static readonly @string nilAngleString = "<nil>";
        private static readonly @string nilParenString = "(nil)";
        private static readonly @string nilString = "nil";
        private static readonly @string mapString = "map[";
        private static readonly @string percentBangString = "%!";
        private static readonly @string missingString = "(MISSING)";
        private static readonly @string badIndexString = "(BADINDEX)";
        private static readonly @string panicString = "(PANIC=";
        private static readonly @string extraString = "%!(EXTRA ";
        private static readonly @string badWidthString = "%!(BADWIDTH)";
        private static readonly @string badPrecString = "%!(BADPREC)";
        private static readonly @string noVerbString = "%!(NOVERB)";
        private static readonly @string invReflectString = "<invalid reflect.Value>";

        // State represents the printer state passed to custom formatters.
        // It provides access to the io.Writer interface plus information about
        // the flags and options for the operand's format specifier.
        public partial interface State
        {
            bool Write(slice<byte> b); // Width returns the value of the width option and whether it has been set.
            bool Width(); // Precision returns the value of the precision option and whether it has been set.
            bool Precision(); // Flag reports whether the flag c, a character, has been set.
            bool Flag(long c);
        }

        // Formatter is the interface implemented by values with a custom formatter.
        // The implementation of Format may call Sprint(f) or Fprint(f) etc.
        // to generate its output.
        public partial interface Formatter
        {
            void Format(State f, int c);
        }

        // Stringer is implemented by any value that has a String method,
        // which defines the ``native'' format for that value.
        // The String method is used to print values passed as an operand
        // to any format that accepts a string or to an unformatted printer
        // such as Print.
        public partial interface Stringer
        {
            @string String();
        }

        // GoStringer is implemented by any value that has a GoString method,
        // which defines the Go syntax for that value.
        // The GoString method is used to print values passed as an operand
        // to a %#v format.
        public partial interface GoStringer
        {
            @string GoString();
        }

        // Use simple []byte instead of bytes.Buffer to avoid large dependency.
        private partial struct buffer // : slice<byte>
        {
        }

        private static void Write(this ref buffer b, slice<byte> p)
        {
            b.Value = append(b.Value, p);
        }

        private static void WriteString(this ref buffer b, @string s)
        {
            b.Value = append(b.Value, s);
        }

        private static void WriteByte(this ref buffer b, byte c)
        {
            b.Value = append(b.Value, c);
        }

        private static void WriteRune(this ref buffer bp, int r)
        {
            if (r < utf8.RuneSelf)
            {
                bp.Value = append(bp.Value, byte(r));
                return;
            }
            var b = bp.Value;
            var n = len(b);
            while (n + utf8.UTFMax > cap(b))
            {
                b = append(b, 0L);
            }

            var w = utf8.EncodeRune(b[n..n + utf8.UTFMax], r);
            bp.Value = b[..n + w];
        }

        // pp is used to store a printer's state and is reused with sync.Pool to avoid allocations.
        private partial struct pp
        {
            public buffer buf; // arg holds the current item, as an interface{}.
            public reflect.Value value; // fmt is used to format basic items such as integers or strings.
            public fmt fmt; // reordered records whether the format string used argument reordering.
            public bool reordered; // goodArgNum records whether the most recent reordering directive was valid.
            public bool goodArgNum; // panicking is set by catchPanic to avoid infinite panic, recover, panic, ... recursion.
            public bool panicking; // erroring is set when printing an error string to guard against calling handleMethods.
            public bool erroring;
        }

        private static sync.Pool ppFree = new sync.Pool(New:func()interface{}{returnnew(pp)},);

        // newPrinter allocates a new pp struct or grabs a cached one.
        private static ref pp newPrinter()
        {
            ref pp p = ppFree.Get()._<ref pp>();
            p.panicking = false;
            p.erroring = false;
            p.fmt.init(ref p.buf);
            return p;
        }

        // free saves used pp structs in ppFree; avoids an allocation per invocation.
        private static void free(this ref pp p)
        {
            p.buf = p.buf[..0L];
            p.arg = null;
            p.value = new reflect.Value();
            ppFree.Put(p);
        }

        private static (long, bool) Width(this ref pp p)
        {
            return (p.fmt.wid, p.fmt.widPresent);
        }

        private static (long, bool) Precision(this ref pp p)
        {
            return (p.fmt.prec, p.fmt.precPresent);
        }

        private static bool Flag(this ref pp p, long b)
        {
            switch (b)
            {
                case '-': 
                    return p.fmt.minus;
                    break;
                case '+': 
                    return p.fmt.plus || p.fmt.plusV;
                    break;
                case '#': 
                    return p.fmt.sharp || p.fmt.sharpV;
                    break;
                case ' ': 
                    return p.fmt.space;
                    break;
                case '0': 
                    return p.fmt.zero;
                    break;
            }
            return false;
        }

        // Implement Write so we can call Fprintf on a pp (through State), for
        // recursive use in custom verbs.
        private static (long, error) Write(this ref pp p, slice<byte> b)
        {
            p.buf.Write(b);
            return (len(b), null);
        }

        // Implement WriteString so that we can call io.WriteString
        // on a pp (through state), for efficiency.
        private static (long, error) WriteString(this ref pp p, @string s)
        {
            p.buf.WriteString(s);
            return (len(s), null);
        }

        // These routines end in 'f' and take a format string.

        // Fprintf formats according to a format specifier and writes to w.
        // It returns the number of bytes written and any write error encountered.
        public static (long, error) Fprintf(io.Writer w, @string format, params object[] a)
        {
            a = a.Clone();

            var p = newPrinter();
            p.doPrintf(format, a);
            n, err = w.Write(p.buf);
            p.free();
            return;
        }

        // Printf formats according to a format specifier and writes to standard output.
        // It returns the number of bytes written and any write error encountered.
        public static (long, error) Printf(@string format, params object[] a)
        {
            a = a.Clone();

            return Fprintf(os.Stdout, format, a);
        }

        // Sprintf formats according to a format specifier and returns the resulting string.
        public static @string Sprintf(@string format, params object[] a)
        {
            a = a.Clone();

            var p = newPrinter();
            p.doPrintf(format, a);
            var s = string(p.buf);
            p.free();
            return s;
        }

        // Errorf formats according to a format specifier and returns the string
        // as a value that satisfies error.
        public static error Errorf(@string format, params object[] a)
        {
            a = a.Clone();

            return error.As(errors.New(Sprintf(format, a)));
        }

        // These routines do not take a format string

        // Fprint formats using the default formats for its operands and writes to w.
        // Spaces are added between operands when neither is a string.
        // It returns the number of bytes written and any write error encountered.
        public static (long, error) Fprint(io.Writer w, params object[] a)
        {
            a = a.Clone();

            var p = newPrinter();
            p.doPrint(a);
            n, err = w.Write(p.buf);
            p.free();
            return;
        }

        // Print formats using the default formats for its operands and writes to standard output.
        // Spaces are added between operands when neither is a string.
        // It returns the number of bytes written and any write error encountered.
        public static (long, error) Print(params object[] a)
        {
            a = a.Clone();

            return Fprint(os.Stdout, a);
        }

        // Sprint formats using the default formats for its operands and returns the resulting string.
        // Spaces are added between operands when neither is a string.
        public static @string Sprint(params object[] a)
        {
            a = a.Clone();

            var p = newPrinter();
            p.doPrint(a);
            var s = string(p.buf);
            p.free();
            return s;
        }

        // These routines end in 'ln', do not take a format string,
        // always add spaces between operands, and add a newline
        // after the last operand.

        // Fprintln formats using the default formats for its operands and writes to w.
        // Spaces are always added between operands and a newline is appended.
        // It returns the number of bytes written and any write error encountered.
        public static (long, error) Fprintln(io.Writer w, params object[] a)
        {
            a = a.Clone();

            var p = newPrinter();
            p.doPrintln(a);
            n, err = w.Write(p.buf);
            p.free();
            return;
        }

        // Println formats using the default formats for its operands and writes to standard output.
        // Spaces are always added between operands and a newline is appended.
        // It returns the number of bytes written and any write error encountered.
        public static (long, error) Println(params object[] a)
        {
            a = a.Clone();

            return Fprintln(os.Stdout, a);
        }

        // Sprintln formats using the default formats for its operands and returns the resulting string.
        // Spaces are always added between operands and a newline is appended.
        public static @string Sprintln(params object[] a)
        {
            a = a.Clone();

            var p = newPrinter();
            p.doPrintln(a);
            var s = string(p.buf);
            p.free();
            return s;
        }

        // getField gets the i'th field of the struct value.
        // If the field is itself is an interface, return a value for
        // the thing inside the interface, not the interface itself.
        private static reflect.Value getField(reflect.Value v, long i)
        {
            var val = v.Field(i);
            if (val.Kind() == reflect.Interface && !val.IsNil())
            {
                val = val.Elem();
            }
            return val;
        }

        // tooLarge reports whether the magnitude of the integer is
        // too large to be used as a formatting width or precision.
        private static bool tooLarge(long x)
        {
            const long max = 1e6F;

            return x > max || x < -max;
        }

        // parsenum converts ASCII to integer.  num is 0 (and isnum is false) if no number present.
        private static (long, bool, long) parsenum(@string s, long start, long end)
        {
            if (start >= end)
            {
                return (0L, false, end);
            }
            for (newi = start; newi < end && '0' <= s[newi] && s[newi] <= '9'; newi++)
            {
                if (tooLarge(num))
                {
                    return (0L, false, end); // Overflow; crazy long number most likely.
                }
                num = num * 10L + int(s[newi] - '0');
                isnum = true;
            }

            return;
        }

        private static void unknownType(this ref pp p, reflect.Value v)
        {
            if (!v.IsValid())
            {
                p.buf.WriteString(nilAngleString);
                return;
            }
            p.buf.WriteByte('?');
            p.buf.WriteString(v.Type().String());
            p.buf.WriteByte('?');
        }

        private static void badVerb(this ref pp p, int verb)
        {
            p.erroring = true;
            p.buf.WriteString(percentBangString);
            p.buf.WriteRune(verb);
            p.buf.WriteByte('(');

            if (p.arg != null) 
                p.buf.WriteString(reflect.TypeOf(p.arg).String());
                p.buf.WriteByte('=');
                p.printArg(p.arg, 'v');
            else if (p.value.IsValid()) 
                p.buf.WriteString(p.value.Type().String());
                p.buf.WriteByte('=');
                p.printValue(p.value, 'v', 0L);
            else 
                p.buf.WriteString(nilAngleString);
                        p.buf.WriteByte(')');
            p.erroring = false;
        }

        private static void fmtBool(this ref pp p, bool v, int verb)
        {
            switch (verb)
            {
                case 't': 

                case 'v': 
                    p.fmt.fmt_boolean(v);
                    break;
                default: 
                    p.badVerb(verb);
                    break;
            }
        }

        // fmt0x64 formats a uint64 in hexadecimal and prefixes it with 0x or
        // not, as requested, by temporarily setting the sharp flag.
        private static void fmt0x64(this ref pp p, ulong v, bool leading0x)
        {
            var sharp = p.fmt.sharp;
            p.fmt.sharp = leading0x;
            p.fmt.fmt_integer(v, 16L, unsigned, ldigits);
            p.fmt.sharp = sharp;
        }

        // fmtInteger formats a signed or unsigned integer.
        private static void fmtInteger(this ref pp p, ulong v, bool isSigned, int verb)
        {
            switch (verb)
            {
                case 'v': 
                    if (p.fmt.sharpV && !isSigned)
                    {
                        p.fmt0x64(v, true);
                    }
                    else
                    {
                        p.fmt.fmt_integer(v, 10L, isSigned, ldigits);
                    }
                    break;
                case 'd': 
                    p.fmt.fmt_integer(v, 10L, isSigned, ldigits);
                    break;
                case 'b': 
                    p.fmt.fmt_integer(v, 2L, isSigned, ldigits);
                    break;
                case 'o': 
                    p.fmt.fmt_integer(v, 8L, isSigned, ldigits);
                    break;
                case 'x': 
                    p.fmt.fmt_integer(v, 16L, isSigned, ldigits);
                    break;
                case 'X': 
                    p.fmt.fmt_integer(v, 16L, isSigned, udigits);
                    break;
                case 'c': 
                    p.fmt.fmt_c(v);
                    break;
                case 'q': 
                    if (v <= utf8.MaxRune)
                    {
                        p.fmt.fmt_qc(v);
                    }
                    else
                    {
                        p.badVerb(verb);
                    }
                    break;
                case 'U': 
                    p.fmt.fmt_unicode(v);
                    break;
                default: 
                    p.badVerb(verb);
                    break;
            }
        }

        // fmtFloat formats a float. The default precision for each verb
        // is specified as last argument in the call to fmt_float.
        private static void fmtFloat(this ref pp p, double v, long size, int verb)
        {
            switch (verb)
            {
                case 'v': 
                    p.fmt.fmt_float(v, size, 'g', -1L);
                    break;
                case 'b': 

                case 'g': 

                case 'G': 
                    p.fmt.fmt_float(v, size, verb, -1L);
                    break;
                case 'f': 

                case 'e': 

                case 'E': 
                    p.fmt.fmt_float(v, size, verb, 6L);
                    break;
                case 'F': 
                    p.fmt.fmt_float(v, size, 'f', 6L);
                    break;
                default: 
                    p.badVerb(verb);
                    break;
            }
        }

        // fmtComplex formats a complex number v with
        // r = real(v) and j = imag(v) as (r+ji) using
        // fmtFloat for r and j formatting.
        private static void fmtComplex(this ref pp p, System.Numerics.Complex128 v, long size, int verb)
        { 
            // Make sure any unsupported verbs are found before the
            // calls to fmtFloat to not generate an incorrect error string.
            switch (verb)
            {
                case 'v': 

                case 'b': 

                case 'g': 

                case 'G': 

                case 'f': 

                case 'F': 

                case 'e': 

                case 'E': 
                    var oldPlus = p.fmt.plus;
                    p.buf.WriteByte('(');
                    p.fmtFloat(real(v), size / 2L, verb); 
                    // Imaginary part always has a sign.
                    p.fmt.plus = true;
                    p.fmtFloat(imag(v), size / 2L, verb);
                    p.buf.WriteString("i)");
                    p.fmt.plus = oldPlus;
                    break;
                default: 
                    p.badVerb(verb);
                    break;
            }
        }

        private static void fmtString(this ref pp p, @string v, int verb)
        {
            switch (verb)
            {
                case 'v': 
                    if (p.fmt.sharpV)
                    {
                        p.fmt.fmt_q(v);
                    }
                    else
                    {
                        p.fmt.fmt_s(v);
                    }
                    break;
                case 's': 
                    p.fmt.fmt_s(v);
                    break;
                case 'x': 
                    p.fmt.fmt_sx(v, ldigits);
                    break;
                case 'X': 
                    p.fmt.fmt_sx(v, udigits);
                    break;
                case 'q': 
                    p.fmt.fmt_q(v);
                    break;
                default: 
                    p.badVerb(verb);
                    break;
            }
        }

        private static void fmtBytes(this ref pp p, slice<byte> v, int verb, @string typeString)
        {
            switch (verb)
            {
                case 'v': 

                case 'd': 
                    if (p.fmt.sharpV)
                    {
                        p.buf.WriteString(typeString);
                        if (v == null)
                        {
                            p.buf.WriteString(nilParenString);
                            return;
                        }
                        p.buf.WriteByte('{');
                        {
                            var i__prev1 = i;
                            var c__prev1 = c;

                            foreach (var (__i, __c) in v)
                            {
                                i = __i;
                                c = __c;
                                if (i > 0L)
                                {
                                    p.buf.WriteString(commaSpaceString);
                                }
                                p.fmt0x64(uint64(c), true);
                            }
                    else

                            i = i__prev1;
                            c = c__prev1;
                        }

                        p.buf.WriteByte('}');
                    }                {
                        p.buf.WriteByte('[');
                        {
                            var i__prev1 = i;
                            var c__prev1 = c;

                            foreach (var (__i, __c) in v)
                            {
                                i = __i;
                                c = __c;
                                if (i > 0L)
                                {
                                    p.buf.WriteByte(' ');
                                }
                                p.fmt.fmt_integer(uint64(c), 10L, unsigned, ldigits);
                            }

                            i = i__prev1;
                            c = c__prev1;
                        }

                        p.buf.WriteByte(']');
                    }
                    break;
                case 's': 
                    p.fmt.fmt_s(string(v));
                    break;
                case 'x': 
                    p.fmt.fmt_bx(v, ldigits);
                    break;
                case 'X': 
                    p.fmt.fmt_bx(v, udigits);
                    break;
                case 'q': 
                    p.fmt.fmt_q(string(v));
                    break;
                default: 
                    p.printValue(reflect.ValueOf(v), verb, 0L);
                    break;
            }
        }

        private static void fmtPointer(this ref pp p, reflect.Value value, int verb)
        {
            System.UIntPtr u = default;

            if (value.Kind() == reflect.Chan || value.Kind() == reflect.Func || value.Kind() == reflect.Map || value.Kind() == reflect.Ptr || value.Kind() == reflect.Slice || value.Kind() == reflect.UnsafePointer) 
                u = value.Pointer();
            else 
                p.badVerb(verb);
                return;
                        switch (verb)
            {
                case 'v': 
                    if (p.fmt.sharpV)
                    {
                        p.buf.WriteByte('(');
                        p.buf.WriteString(value.Type().String());
                        p.buf.WriteString(")(");
                        if (u == 0L)
                        {
                            p.buf.WriteString(nilString);
                        }
                        else
                        {
                            p.fmt0x64(uint64(u), true);
                        }
                        p.buf.WriteByte(')');
                    }
                    else
                    {
                        if (u == 0L)
                        {
                            p.fmt.padString(nilAngleString);
                        }
                        else
                        {
                            p.fmt0x64(uint64(u), !p.fmt.sharp);
                        }
                    }
                    break;
                case 'p': 
                    p.fmt0x64(uint64(u), !p.fmt.sharp);
                    break;
                case 'b': 

                case 'o': 

                case 'd': 

                case 'x': 

                case 'X': 
                    p.fmtInteger(uint64(u), unsigned, verb);
                    break;
                default: 
                    p.badVerb(verb);
                    break;
            }
        }

        private static void catchPanic(this ref pp _p, object arg, int verb) => func(_p, (ref pp p, Defer _, Panic panic, Recover __) =>
        {
            {
                var err = recover();

                if (err != null)
                { 
                    // If it's a nil pointer, just say "<nil>". The likeliest causes are a
                    // Stringer that fails to guard against nil or a nil pointer for a
                    // value receiver, and in either case, "<nil>" is a nice result.
                    {
                        var v = reflect.ValueOf(arg);

                        if (v.Kind() == reflect.Ptr && v.IsNil())
                        {
                            p.buf.WriteString(nilAngleString);
                            return;
                        } 
                        // Otherwise print a concise panic message. Most of the time the panic
                        // value will print itself nicely.

                    } 
                    // Otherwise print a concise panic message. Most of the time the panic
                    // value will print itself nicely.
                    if (p.panicking)
                    { 
                        // Nested panics; the recursion in printArg cannot succeed.
                        panic(err);
                    }
                    var oldFlags = p.fmt.fmtFlags; 
                    // For this output we want default behavior.
                    p.fmt.clearflags();

                    p.buf.WriteString(percentBangString);
                    p.buf.WriteRune(verb);
                    p.buf.WriteString(panicString);
                    p.panicking = true;
                    p.printArg(err, 'v');
                    p.panicking = false;
                    p.buf.WriteByte(')');

                    p.fmt.fmtFlags = oldFlags;
                }

            }
        });

        private static bool handleMethods(this ref pp _p, int verb) => func(_p, (ref pp p, Defer defer, Panic _, Recover __) =>
        {
            if (p.erroring)
            {
                return;
            } 
            // Is it a Formatter?
            {
                Formatter (formatter, ok) = p.arg._<Formatter>();

                if (ok)
                {
                    handled = true;
                    defer(p.catchPanic(p.arg, verb));
                    formatter.Format(p, verb);
                    return;
                } 

                // If we're doing Go syntax and the argument knows how to supply it, take care of it now.

            } 

            // If we're doing Go syntax and the argument knows how to supply it, take care of it now.
            if (p.fmt.sharpV)
            {
                {
                    GoStringer (stringer, ok) = p.arg._<GoStringer>();

                    if (ok)
                    {
                        handled = true;
                        defer(p.catchPanic(p.arg, verb)); 
                        // Print the result of GoString unadorned.
                        p.fmt.fmt_s(stringer.GoString());
                        return;
                    }

                }
            }
            else
            { 
                // If a string is acceptable according to the format, see if
                // the value satisfies one of the string-valued interfaces.
                // Println etc. set verb to %v, which is "stringable".
                switch (verb)
                {
                    case 'v': 
                        // Is it an error or Stringer?
                        // The duplication in the bodies is necessary:
                        // setting handled and deferring catchPanic
                        // must happen before calling the method.

                    case 's': 
                        // Is it an error or Stringer?
                        // The duplication in the bodies is necessary:
                        // setting handled and deferring catchPanic
                        // must happen before calling the method.

                    case 'x': 
                        // Is it an error or Stringer?
                        // The duplication in the bodies is necessary:
                        // setting handled and deferring catchPanic
                        // must happen before calling the method.

                    case 'X': 
                        // Is it an error or Stringer?
                        // The duplication in the bodies is necessary:
                        // setting handled and deferring catchPanic
                        // must happen before calling the method.

                    case 'q': 
                        // Is it an error or Stringer?
                        // The duplication in the bodies is necessary:
                        // setting handled and deferring catchPanic
                        // must happen before calling the method.
                        switch (p.arg.type())
                        {
                            case error v:
                                handled = true;
                                defer(p.catchPanic(p.arg, verb));
                                p.fmtString(v.Error(), verb);
                                return;
                                break;
                            case Stringer v:
                                handled = true;
                                defer(p.catchPanic(p.arg, verb));
                                p.fmtString(v.String(), verb);
                                return;
                                break;
                        }
                        break;
                }
            }
            return false;
        });

        private static void printArg(this ref pp p, object arg, int verb)
        {
            p.arg = arg;
            p.value = new reflect.Value();

            if (arg == null)
            {
                switch (verb)
                {
                    case 'T': 

                    case 'v': 
                        p.fmt.padString(nilAngleString);
                        break;
                    default: 
                        p.badVerb(verb);
                        break;
                }
                return;
            } 

            // Special processing considerations.
            // %T (the value's type) and %p (its address) are special; we always do them first.
            switch (verb)
            {
                case 'T': 
                    p.fmt.fmt_s(reflect.TypeOf(arg).String());
                    return;
                    break;
                case 'p': 
                    p.fmtPointer(reflect.ValueOf(arg), 'p');
                    return;
                    break;
            } 

            // Some types can be done without reflection.
            switch (arg.type())
            {
                case bool f:
                    p.fmtBool(f, verb);
                    break;
                case float f:
                    p.fmtFloat(float64(f), 32L, verb);
                    break;
                case double f:
                    p.fmtFloat(f, 64L, verb);
                    break;
                case complex64 f:
                    p.fmtComplex(complex128(f), 64L, verb);
                    break;
                case System.Numerics.Complex128 f:
                    p.fmtComplex(f, 128L, verb);
                    break;
                case long f:
                    p.fmtInteger(uint64(f), signed, verb);
                    break;
                case sbyte f:
                    p.fmtInteger(uint64(f), signed, verb);
                    break;
                case short f:
                    p.fmtInteger(uint64(f), signed, verb);
                    break;
                case int f:
                    p.fmtInteger(uint64(f), signed, verb);
                    break;
                case long f:
                    p.fmtInteger(uint64(f), signed, verb);
                    break;
                case ulong f:
                    p.fmtInteger(uint64(f), unsigned, verb);
                    break;
                case byte f:
                    p.fmtInteger(uint64(f), unsigned, verb);
                    break;
                case ushort f:
                    p.fmtInteger(uint64(f), unsigned, verb);
                    break;
                case uint f:
                    p.fmtInteger(uint64(f), unsigned, verb);
                    break;
                case ulong f:
                    p.fmtInteger(f, unsigned, verb);
                    break;
                case System.UIntPtr f:
                    p.fmtInteger(uint64(f), unsigned, verb);
                    break;
                case @string f:
                    p.fmtString(f, verb);
                    break;
                case slice<byte> f:
                    p.fmtBytes(f, verb, "[]byte");
                    break;
                case reflect.Value f:
                    if (f.IsValid() && f.CanInterface())
                    {
                        p.arg = f.Interface();
                        if (p.handleMethods(verb))
                        {
                            return;
                        }
                    }
                    p.printValue(f, verb, 0L);
                    break;
                default:
                {
                    var f = arg.type();
                    if (!p.handleMethods(verb))
                    { 
                        // Need to use reflection, since the type had no
                        // interface methods that could be used for formatting.
                        p.printValue(reflect.ValueOf(f), verb, 0L);
                    }
                    break;
                }
            }
        }

        // printValue is similar to printArg but starts with a reflect value, not an interface{} value.
        // It does not handle 'p' and 'T' verbs because these should have been already handled by printArg.
        private static void printValue(this ref pp p, reflect.Value value, int verb, long depth)
        { 
            // Handle values with special methods if not already handled by printArg (depth == 0).
            if (depth > 0L && value.IsValid() && value.CanInterface())
            {
                p.arg = value.Interface();
                if (p.handleMethods(verb))
                {
                    return;
                }
            }
            p.arg = null;
            p.value = value;

            {
                var f = value;


                if (value.Kind() == reflect.Invalid)
                {
                    if (depth == 0L)
                    {
                        p.buf.WriteString(invReflectString);
                    }
                    else
                    {
                        switch (verb)
                        {
                            case 'v': 
                                p.buf.WriteString(nilAngleString);
                                break;
                            default: 
                                p.badVerb(verb);
                                break;
                        }
                    }
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Bool)
                {
                    p.fmtBool(f.Bool(), verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Int || value.Kind() == reflect.Int8 || value.Kind() == reflect.Int16 || value.Kind() == reflect.Int32 || value.Kind() == reflect.Int64)
                {
                    p.fmtInteger(uint64(f.Int()), signed, verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Uint || value.Kind() == reflect.Uint8 || value.Kind() == reflect.Uint16 || value.Kind() == reflect.Uint32 || value.Kind() == reflect.Uint64 || value.Kind() == reflect.Uintptr)
                {
                    p.fmtInteger(f.Uint(), unsigned, verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Float32)
                {
                    p.fmtFloat(f.Float(), 32L, verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Float64)
                {
                    p.fmtFloat(f.Float(), 64L, verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Complex64)
                {
                    p.fmtComplex(f.Complex(), 64L, verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Complex128)
                {
                    p.fmtComplex(f.Complex(), 128L, verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.String)
                {
                    p.fmtString(f.String(), verb);
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Map)
                {
                    if (p.fmt.sharpV)
                    {
                        p.buf.WriteString(f.Type().String());
                        if (f.IsNil())
                        {
                            p.buf.WriteString(nilParenString);
                            return;
                        }
                        p.buf.WriteByte('{');
                    }
                    else
                    {
                        p.buf.WriteString(mapString);
                    }
                    var keys = f.MapKeys();
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __key) in keys)
                        {
                            i = __i;
                            key = __key;
                            if (i > 0L)
                            {
                                if (p.fmt.sharpV)
                                {
                                    p.buf.WriteString(commaSpaceString);
                                }
                                else
                                {
                                    p.buf.WriteByte(' ');
                                }
                            }
                            p.printValue(key, verb, depth + 1L);
                            p.buf.WriteByte(':');
                            p.printValue(f.MapIndex(key), verb, depth + 1L);
                        }

                        i = i__prev1;
                    }

                    if (p.fmt.sharpV)
                    {
                        p.buf.WriteByte('}');
                    }
                    else
                    {
                        p.buf.WriteByte(']');
                    }
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Struct)
                {
                    if (p.fmt.sharpV)
                    {
                        p.buf.WriteString(f.Type().String());
                    }
                    p.buf.WriteByte('{');
                    {
                        var i__prev1 = i;

                        for (long i = 0L; i < f.NumField(); i++)
                        {
                            if (i > 0L)
                            {
                                if (p.fmt.sharpV)
                                {
                                    p.buf.WriteString(commaSpaceString);
                                }
                                else
                                {
                                    p.buf.WriteByte(' ');
                                }
                            }
                            if (p.fmt.plusV || p.fmt.sharpV)
                            {
                                {
                                    var name = f.Type().Field(i).Name;

                                    if (name != "")
                                    {
                                        p.buf.WriteString(name);
                                        p.buf.WriteByte(':');
                                    }

                                }
                            }
                            p.printValue(getField(f, i), verb, depth + 1L);
                        }


                        i = i__prev1;
                    }
                    p.buf.WriteByte('}');
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Interface)
                {
                    var value = f.Elem();
                    if (!value.IsValid())
                    {
                        if (p.fmt.sharpV)
                        {
                            p.buf.WriteString(f.Type().String());
                            p.buf.WriteString(nilParenString);
                        }
                        else
                        {
                            p.buf.WriteString(nilAngleString);
                        }
                    }
                    else
                    {
                        p.printValue(value, verb, depth + 1L);
                    }
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Array || value.Kind() == reflect.Slice)
                {
                    switch (verb)
                    {
                        case 's': 
                            // Handle byte and uint8 slices and arrays special for the above verbs.

                        case 'q': 
                            // Handle byte and uint8 slices and arrays special for the above verbs.

                        case 'x': 
                            // Handle byte and uint8 slices and arrays special for the above verbs.

                        case 'X': 
                            // Handle byte and uint8 slices and arrays special for the above verbs.
                            var t = f.Type();
                            if (t.Elem().Kind() == reflect.Uint8)
                            {
                                slice<byte> bytes = default;
                                if (f.Kind() == reflect.Slice)
                                {
                                    bytes = f.Bytes();
                                }
                                else if (f.CanAddr())
                                {
                                    bytes = f.Slice(0L, f.Len()).Bytes();
                                }
                                else
                                { 
                                    // We have an array, but we cannot Slice() a non-addressable array,
                                    // so we build a slice by hand. This is a rare case but it would be nice
                                    // if reflection could help a little more.
                                    bytes = make_slice<byte>(f.Len());
                                    {
                                        var i__prev1 = i;

                                        foreach (var (__i) in bytes)
                                        {
                                            i = __i;
                                            bytes[i] = byte(f.Index(i).Uint());
                                        }

                                        i = i__prev1;
                                    }

                                }
                                p.fmtBytes(bytes, verb, t.String());
                                return;
                            }
                            break;
                    }
                    if (p.fmt.sharpV)
                    {
                        p.buf.WriteString(f.Type().String());
                        if (f.Kind() == reflect.Slice && f.IsNil())
                        {
                            p.buf.WriteString(nilParenString);
                            return;
                        }
                        p.buf.WriteByte('{');
                        {
                            var i__prev1 = i;

                            for (i = 0L; i < f.Len(); i++)
                            {
                                if (i > 0L)
                                {
                                    p.buf.WriteString(commaSpaceString);
                                }
                                p.printValue(f.Index(i), verb, depth + 1L);
                            }
                    else


                            i = i__prev1;
                        }
                        p.buf.WriteByte('}');
                    }                    {
                        p.buf.WriteByte('[');
                        {
                            var i__prev1 = i;

                            for (i = 0L; i < f.Len(); i++)
                            {
                                if (i > 0L)
                                {
                                    p.buf.WriteByte(' ');
                                }
                                p.printValue(f.Index(i), verb, depth + 1L);
                            }


                            i = i__prev1;
                        }
                        p.buf.WriteByte(']');
                    }
                    goto __switch_break0;
                }
                if (value.Kind() == reflect.Ptr) 
                {
                    // pointer to array or slice or struct? ok at top level
                    // but not embedded (avoid loops)
                    if (depth == 0L && f.Pointer() != 0L)
                    {
                        {
                            var a = f.Elem();


                            if (a.Kind() == reflect.Array || a.Kind() == reflect.Slice || a.Kind() == reflect.Struct || a.Kind() == reflect.Map) 
                                p.buf.WriteByte('&');
                                p.printValue(a, verb, depth + 1L);
                                return;

                        }
                    }
                    fallthrough = true;
                }
                if (fallthrough || value.Kind() == reflect.Chan || value.Kind() == reflect.Func || value.Kind() == reflect.UnsafePointer)
                {
                    p.fmtPointer(f, verb);
                    goto __switch_break0;
                }
                // default: 
                    p.unknownType(f);

                __switch_break0:;
            }
        }

        // intFromArg gets the argNumth element of a. On return, isInt reports whether the argument has integer type.
        private static (long, bool, long) intFromArg(slice<object> a, long argNum)
        {
            newArgNum = argNum;
            if (argNum < len(a))
            {
                num, isInt = a[argNum]._<long>(); // Almost always OK.
                if (!isInt)
                { 
                    // Work harder.
                    {
                        var v = reflect.ValueOf(a[argNum]);


                        if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                            var n = v.Int();
                            if (int64(int(n)) == n)
                            {
                                num = int(n);
                                isInt = true;
                            }
                        else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                            n = v.Uint();
                            if (int64(n) >= 0L && uint64(int(n)) == n)
                            {
                                num = int(n);
                                isInt = true;
                            }
                        else 
                    }
                }
                newArgNum = argNum + 1L;
                if (tooLarge(num))
                {
                    num = 0L;
                    isInt = false;
                }
            }
            return;
        }

        // parseArgNumber returns the value of the bracketed number, minus 1
        // (explicit argument numbers are one-indexed but we want zero-indexed).
        // The opening bracket is known to be present at format[0].
        // The returned values are the index, the number of bytes to consume
        // up to the closing paren, if present, and whether the number parsed
        // ok. The bytes to consume will be 1 if no closing paren is present.
        private static (long, long, bool) parseArgNumber(@string format)
        { 
            // There must be at least 3 bytes: [n].
            if (len(format) < 3L)
            {
                return (0L, 1L, false);
            } 

            // Find closing bracket.
            for (long i = 1L; i < len(format); i++)
            {
                if (format[i] == ']')
                {
                    var (width, ok, newi) = parsenum(format, 1L, i);
                    if (!ok || newi != i)
                    {
                        return (0L, i + 1L, false);
                    }
                    return (width - 1L, i + 1L, true); // arg numbers are one-indexed and skip paren.
                }
            }

            return (0L, 1L, false);
        }

        // argNumber returns the next argument to evaluate, which is either the value of the passed-in
        // argNum or the value of the bracketed integer that begins format[i:]. It also returns
        // the new value of i, that is, the index of the next byte of the format to process.
        private static (long, long, bool) argNumber(this ref pp p, long argNum, @string format, long i, long numArgs)
        {
            if (len(format) <= i || format[i] != '[')
            {
                return (argNum, i, false);
            }
            p.reordered = true;
            var (index, wid, ok) = parseArgNumber(format[i..]);
            if (ok && 0L <= index && index < numArgs)
            {
                return (index, i + wid, true);
            }
            p.goodArgNum = false;
            return (argNum, i + wid, ok);
        }

        private static void badArgNum(this ref pp p, int verb)
        {
            p.buf.WriteString(percentBangString);
            p.buf.WriteRune(verb);
            p.buf.WriteString(badIndexString);
        }

        private static void missingArg(this ref pp p, int verb)
        {
            p.buf.WriteString(percentBangString);
            p.buf.WriteRune(verb);
            p.buf.WriteString(missingString);
        }

        private static void doPrintf(this ref pp p, @string format, slice<object> a)
        {
            var end = len(format);
            long argNum = 0L; // we process one argument per non-trivial format
            var afterIndex = false; // previous item in format was an index like [3].
            p.reordered = false;
formatLoop: 

            // Check for extra arguments unless the call accessed the arguments
            // out of order, in which case it's too expensive to detect if they've all
            // been used and arguably OK if they're not.
            {
                long i__prev1 = i;

                long i = 0L;

                while (i < end)
                {
                    p.goodArgNum = true;
                    var lasti = i;
                    while (i < end && format[i] != '%')
                    {
                        i++;
                    }

                    if (i > lasti)
                    {
                        p.buf.WriteString(format[lasti..i]);
                    }
                    if (i >= end)
                    { 
                        // done processing format string
                        break;
                    } 

                    // Process one verb
                    i++; 

                    // Do we have flags?
                    p.fmt.clearflags();
simpleFormat: 

                    // Do we have an explicit argument index?
                    while (i < end)
                    {
                        var c = format[i];
                        switch (c)
                        {
                            case '#': 
                                p.fmt.sharp = true;
                                break;
                            case '0': 
                                p.fmt.zero = !p.fmt.minus; // Only allow zero padding to the left.
                                break;
                            case '+': 
                                p.fmt.plus = true;
                                break;
                            case '-': 
                                p.fmt.minus = true;
                                p.fmt.zero = false; // Do not pad with zeros to the right.
                                break;
                            case ' ': 
                                p.fmt.space = true;
                                break;
                            default: 
                                // Fast path for common case of ascii lower case simple verbs
                                // without precision or width or argument indices.
                                                        if ('a' <= c && c <= 'z' && argNum < len(a))
                                                        {
                                                            if (c == 'v')
                                                            { 
                                                                // Go syntax
                                                                p.fmt.sharpV = p.fmt.sharp;
                                                                p.fmt.sharp = false; 
                                                                // Struct-field syntax
                                                                p.fmt.plusV = p.fmt.plus;
                                                                p.fmt.plus = false;
                                                    i++;
                                                            }
                                                            p.printArg(a[argNum], rune(c));
                                                            argNum++;
                                                            i++;
                                                            _continueformatLoop = true;
                                                            break;
                                                        } 
                                                        // Format is more complex than simple flags and a verb or is malformed.
                                                        _breaksimpleFormat = true;
                                                        break;
                                break;
                        }
                    } 

                    // Do we have an explicit argument index?
 

                    // Do we have an explicit argument index?
                    argNum, i, afterIndex = p.argNumber(argNum, format, i, len(a)); 

                    // Do we have width?
                    if (i < end && format[i] == '*')
                    {
                        i++;
                        p.fmt.wid, p.fmt.widPresent, argNum = intFromArg(a, argNum);

                        if (!p.fmt.widPresent)
                        {
                            p.buf.WriteString(badWidthString);
                        } 

                        // We have a negative width, so take its value and ensure
                        // that the minus flag is set
                        if (p.fmt.wid < 0L)
                        {
                            p.fmt.wid = -p.fmt.wid;
                            p.fmt.minus = true;
                            p.fmt.zero = false; // Do not pad with zeros to the right.
                        }
                        afterIndex = false;
                    }
                    else
                    {
                        p.fmt.wid, p.fmt.widPresent, i = parsenum(format, i, end);
                        if (afterIndex && p.fmt.widPresent)
                        { // "%[3]2d"
                            p.goodArgNum = false;
                        }
                    } 

                    // Do we have precision?
                    if (i + 1L < end && format[i] == '.')
                    {
                        i++;
                        if (afterIndex)
                        { // "%[3].2d"
                            p.goodArgNum = false;
                        }
                        argNum, i, afterIndex = p.argNumber(argNum, format, i, len(a));
                        if (i < end && format[i] == '*')
                        {
                            i++;
                            p.fmt.prec, p.fmt.precPresent, argNum = intFromArg(a, argNum); 
                            // Negative precision arguments don't make sense
                            if (p.fmt.prec < 0L)
                            {
                                p.fmt.prec = 0L;
                                p.fmt.precPresent = false;
                            }
                            if (!p.fmt.precPresent)
                            {
                                p.buf.WriteString(badPrecString);
                            }
                            afterIndex = false;
                        }
                        else
                        {
                            p.fmt.prec, p.fmt.precPresent, i = parsenum(format, i, end);
                            if (!p.fmt.precPresent)
                            {
                                p.fmt.prec = 0L;
                                p.fmt.precPresent = true;
                            }
                        }
                    }
                    if (!afterIndex)
                    {
                        argNum, i, afterIndex = p.argNumber(argNum, format, i, len(a));
                    }
                    if (i >= end)
                    {
                        p.buf.WriteString(noVerbString);
                        break;
                    }
                    var verb = rune(format[i]);
                    long size = 1L;
                    if (verb >= utf8.RuneSelf)
                    {
                        verb, size = utf8.DecodeRuneInString(format[i..]);
                    }
                    i += size;


                    if (verb == '%') // Percent does not absorb operands and ignores f.wid and f.prec.
                    {
                        p.buf.WriteByte('%');
                        goto __switch_break1;
                    }
                    if (!p.goodArgNum)
                    {
                        p.badArgNum(verb);
                        goto __switch_break1;
                    }
                    if (argNum >= len(a)) // No argument left over to print for the current verb.
                    {
                        p.missingArg(verb);
                        goto __switch_break1;
                    }
                    if (verb == 'v') 
                    {
                        // Go syntax
                        p.fmt.sharpV = p.fmt.sharp;
                        p.fmt.sharp = false; 
                        // Struct-field syntax
                        p.fmt.plusV = p.fmt.plus;
                        p.fmt.plus = false;
                    }
                    // default: 
                        p.printArg(a[argNum], verb);
                        argNum++;

                    __switch_break1:;
                } 

                // Check for extra arguments unless the call accessed the arguments
                // out of order, in which case it's too expensive to detect if they've all
                // been used and arguably OK if they're not.


                i = i__prev1;
            } 

            // Check for extra arguments unless the call accessed the arguments
            // out of order, in which case it's too expensive to detect if they've all
            // been used and arguably OK if they're not.
            if (!p.reordered && argNum < len(a))
            {
                p.fmt.clearflags();
                p.buf.WriteString(extraString);
                {
                    long i__prev1 = i;

                    foreach (var (__i, __arg) in a[argNum..])
                    {
                        i = __i;
                        arg = __arg;
                        if (i > 0L)
                        {
                            p.buf.WriteString(commaSpaceString);
                        }
                        if (arg == null)
                        {
                            p.buf.WriteString(nilAngleString);
                        }
                        else
                        {
                            p.buf.WriteString(reflect.TypeOf(arg).String());
                            p.buf.WriteByte('=');
                            p.printArg(arg, 'v');
                        }
                    }

                    i = i__prev1;
                }

                p.buf.WriteByte(')');
            }
        }

        private static void doPrint(this ref pp p, slice<object> a)
        {
            var prevString = false;
            foreach (var (argNum, arg) in a)
            {
                var isString = arg != null && reflect.TypeOf(arg).Kind() == reflect.String; 
                // Add a space between two non-string arguments.
                if (argNum > 0L && !isString && !prevString)
                {
                    p.buf.WriteByte(' ');
                }
                p.printArg(arg, 'v');
                prevString = isString;
            }
        }

        // doPrintln is like doPrint but always adds a space between arguments
        // and a newline after the last argument.
        private static void doPrintln(this ref pp p, slice<object> a)
        {
            foreach (var (argNum, arg) in a)
            {
                if (argNum > 0L)
                {
                    p.buf.WriteByte(' ');
                }
                p.printArg(arg, 'v');
            }
            p.buf.WriteByte('\n');
        }
    }
}
