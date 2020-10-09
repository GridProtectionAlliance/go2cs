// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fmt -- go2cs converted at 2020 October 09 05:07:50 UTC
// import "fmt" ==> using fmt = go.fmt_package
// Original source: C:\Go\src\fmt\errors.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class fmt_package
    {
        // Errorf formats according to a format specifier and returns the string as a
        // value that satisfies error.
        //
        // If the format specifier includes a %w verb with an error operand,
        // the returned error will implement an Unwrap method returning the operand. It is
        // invalid to include more than one %w verb or to supply it with an operand
        // that does not implement the error interface. The %w verb is otherwise
        // a synonym for %v.
        public static error Errorf(@string format, params object[] a)
        {
            a = a.Clone();

            var p = newPrinter();
            p.wrapErrs = true;
            p.doPrintf(format, a);
            var s = string(p.buf);
            error err = default!;
            if (p.wrappedErr == null)
            {
                err = error.As(errors.New(s))!;
            }
            else
            {
                err = error.As(addr(new wrapError(s,p.wrappedErr)))!;
            }
            p.free();
            return error.As(err)!;

        }

        private partial struct wrapError
        {
            public @string msg;
            public error err;
        }

        private static @string Error(this ptr<wrapError> _addr_e)
        {
            ref wrapError e = ref _addr_e.val;

            return e.msg;
        }

        private static error Unwrap(this ptr<wrapError> _addr_e)
        {
            ref wrapError e = ref _addr_e.val;

            return error.As(e.err)!;
        }
    }
}
