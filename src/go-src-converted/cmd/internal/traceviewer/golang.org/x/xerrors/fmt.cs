// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xerrors -- go2cs converted at 2022 March 06 23:16:45 UTC
// import "golang.org/x/xerrors" ==> using xerrors = go.golang.org.x.xerrors_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\xerrors\fmt.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using @internal = go.golang.org.x.xerrors.@internal_package;

namespace go.golang.org.x;

public static partial class xerrors_package {

private static readonly @string percentBangString = "%!";

// Errorf formats according to a format specifier and returns the string as a
// value that satisfies error.
//
// The returned error includes the file and line number of the caller when
// formatted with additional detail enabled. If the last argument is an error
// the returned error's Format method will return it if the format string ends
// with ": %s", ": %v", or ": %w". If the last argument is an error and the
// format string ends with ": %w", the returned error implements an Unwrap
// method returning it.
//
// If the format specifier includes a %w verb with an error operand in a
// position other than at the end, the returned error will still implement an
// Unwrap method returning the operand, but the error's Format method will not
// return the wrapped error.
//
// It is invalid to include more than one %w verb or to supply it with an
// operand that does not implement the error interface. The %w verb is otherwise
// a synonym for %v.


// Errorf formats according to a format specifier and returns the string as a
// value that satisfies error.
//
// The returned error includes the file and line number of the caller when
// formatted with additional detail enabled. If the last argument is an error
// the returned error's Format method will return it if the format string ends
// with ": %s", ": %v", or ": %w". If the last argument is an error and the
// format string ends with ": %w", the returned error implements an Unwrap
// method returning it.
//
// If the format specifier includes a %w verb with an error operand in a
// position other than at the end, the returned error will still implement an
// Unwrap method returning the operand, but the error's Format method will not
// return the wrapped error.
//
// It is invalid to include more than one %w verb or to supply it with an
// operand that does not implement the error interface. The %w verb is otherwise
// a synonym for %v.
public static error Errorf(@string format, params object[] a) {
    a = a.Clone();

    format = formatPlusW(format); 
    // Support a ": %[wsv]" suffix, which works well with xerrors.Formatter.
    var wrap = strings.HasSuffix(format, ": %w");
    var (idx, format2, ok) = parsePercentW(format);
    var percentWElsewhere = !wrap && idx >= 0;
    if (!percentWElsewhere && (wrap || strings.HasSuffix(format, ": %s") || strings.HasSuffix(format, ": %v"))) {
        var err = errorAt(a, len(a) - 1);
        if (err == null) {
            return error.As(addr(new noWrapError(fmt.Sprintf(format,a...),nil,Caller(1)))!)!;
        }
        var msg = fmt.Sprintf(format[..(int)len(format) - len(": %s")], a[..(int)len(a) - 1]);
        Frame frame = new Frame();
        if (@internal.EnableTrace) {
            frame = Caller(1);
        }
        if (wrap) {
            return error.As(addr(new wrapError(msg,err,frame))!)!;
        }
        return error.As(addr(new noWrapError(msg,err,frame))!)!;
    }
    msg = fmt.Sprintf(format2, a);
    if (idx < 0) {
        return error.As(addr(new noWrapError(msg,nil,Caller(1)))!)!;
    }
    err = errorAt(a, idx);
    if (!ok || err == null) { 
        // Too many %ws or argument of %w is not an error. Approximate the Go
        // 1.13 fmt.Errorf message.
        return error.As(addr(new noWrapError(fmt.Sprintf("%sw(%s)",percentBangString,msg),nil,Caller(1)))!)!;
    }
    frame = new Frame();
    if (@internal.EnableTrace) {
        frame = Caller(1);
    }
    return error.As(addr(new wrapError(msg,err,frame))!)!;
}

private static error errorAt(slice<object> args, nint i) {
    if (i < 0 || i >= len(args)) {
        return error.As(null!)!;
    }
    error (err, ok) = error.As(args[i]._<error>())!;
    if (!ok) {
        return error.As(null!)!;
    }
    return error.As(err)!;
}

// formatPlusW is used to avoid the vet check that will barf at %w.
private static @string formatPlusW(@string s) {
    return s;
}

// Return the index of the only %w in format, or -1 if none.
// Also return a rewritten format string with %w replaced by %v, and
// false if there is more than one %w.
// TODO: handle "%[N]w".
private static (nint, @string, bool) parsePercentW(@string format) {
    nint idx = default;
    @string newFormat = default;
    bool ok = default;
 
    // Loosely copied from golang.org/x/tools/go/analysis/passes/printf/printf.go.
    idx = -1;
    ok = true;
    nint n = 0;
    nint sz = 0;
    bool isW = default;
    {
        nint i = 0;

        while (i < len(format)) {
            if (format[i] != '%') {
                sz = 1;
                continue;
            i += sz;
            } 
            // "%%" is not a format directive.
            if (i + 1 < len(format) && format[i + 1] == '%') {
                sz = 2;
                continue;
            }
            sz, isW = parsePrintfVerb(format[(int)i..]);
            if (isW) {
                if (idx >= 0) {
                    ok = false;
                }
                else
 {
                    idx = n;
                } 
                // "Replace" the last character, the 'w', with a 'v'.
                var p = i + sz - 1;
                format = format[..(int)p] + "v" + format[(int)p + 1..];
            }
            n++;
        }
    }
    return (idx, format, ok);
}

// Parse the printf verb starting with a % at s[0].
// Return how many bytes it occupies and whether the verb is 'w'.
private static (nint, bool) parsePrintfVerb(@string s) {
    nint _p0 = default;
    bool _p0 = default;
 
    // Assume only that the directive is a sequence of non-letters followed by a single letter.
    nint sz = 0;
    int r = default;
    {
        nint i = 1;

        while (i < len(s)) {
            r, sz = utf8.DecodeRuneInString(s[(int)i..]);
            if (unicode.IsLetter(r)) {
                return (i + sz, r == 'w');
            i += sz;
            }
        }
    }
    return (len(s), false);
}

private partial struct noWrapError {
    public @string msg;
    public error err;
    public Frame frame;
}

private static @string Error(this ptr<noWrapError> _addr_e) {
    ref noWrapError e = ref _addr_e.val;

    return fmt.Sprint(e);
}

private static void Format(this ptr<noWrapError> _addr_e, fmt.State s, int v) {
    ref noWrapError e = ref _addr_e.val;

    FormatError(e, s, v);
}

private static error FormatError(this ptr<noWrapError> _addr_e, Printer p) {
    error next = default!;
    ref noWrapError e = ref _addr_e.val;

    p.Print(e.msg);
    e.frame.Format(p);
    return error.As(e.err)!;
}

private partial struct wrapError {
    public @string msg;
    public error err;
    public Frame frame;
}

private static @string Error(this ptr<wrapError> _addr_e) {
    ref wrapError e = ref _addr_e.val;

    return fmt.Sprint(e);
}

private static void Format(this ptr<wrapError> _addr_e, fmt.State s, int v) {
    ref wrapError e = ref _addr_e.val;

    FormatError(e, s, v);
}

private static error FormatError(this ptr<wrapError> _addr_e, Printer p) {
    error next = default!;
    ref wrapError e = ref _addr_e.val;

    p.Print(e.msg);
    e.frame.Format(p);
    return error.As(e.err)!;
}

private static error Unwrap(this ptr<wrapError> _addr_e) {
    ref wrapError e = ref _addr_e.val;

    return error.As(e.err)!;
}

} // end xerrors_package
