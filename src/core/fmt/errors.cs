// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using slices = slices_package;
using ꓸꓸꓸany = Span<any>;

partial class fmt_package {

// Errorf formats according to a format specifier and returns the string as a
// value that satisfies error.
//
// If the format specifier includes a %w verb with an error operand,
// the returned error will implement an Unwrap method returning the operand.
// If there is more than one %w verb, the returned error will implement an
// Unwrap method returning a []error containing all the %w operands in the
// order they appear in the arguments.
// It is invalid to supply the %w verb with an operand that does not implement
// the error interface. The %w verb is otherwise a synonym for %v.
public static error Errorf(@string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.val.wrapErrs = true;
    p.doPrintf(format, a);
    @string s = ((@string)(~p).buf);
    error err = default!;
    switch (len((~p).wrappedErrs)) {
    case 0: {
        err = errors.New(s);
        break;
    }
    case 1: {
        var w = Ꮡ(new wrapError(msg: s));
        (w.val.err, _) = a[(~p).wrappedErrs[0]]._<error>(ᐧ);
        err = ~w;
        break;
    }
    default: {
        if ((~p).reordered) {
            slices.Sort((~p).wrappedErrs);
        }
        slice<error> errs = default!;
        foreach (var (i, argNum) in (~p).wrappedErrs) {
            if (i > 0 && (~p).wrappedErrs[i - 1] == argNum) {
                continue;
            }
            {
                var (e, ok) = a[argNum]._<error>(ᐧ); if (ok) {
                    errs = append(errs, e);
                }
            }
        }
        Ꮡerr = new wrapErrors(s, errs); err = ref Ꮡerr.val;
        break;
    }}

    p.free();
    return err;
}

[GoType] partial struct wrapError {
    internal @string msg;
    internal error err;
}

[GoRecv] internal static @string Error(this ref wrapError e) {
    return e.msg;
}

[GoRecv] internal static error Unwrap(this ref wrapError e) {
    return e.err;
}

[GoType] partial struct wrapErrors {
    internal @string msg;
    internal slice<error> errs;
}

[GoRecv] internal static @string Error(this ref wrapErrors e) {
    return e.msg;
}

[GoRecv] internal static slice<error> Unwrap(this ref wrapErrors e) {
    return e.errs;
}

} // end fmt_package
