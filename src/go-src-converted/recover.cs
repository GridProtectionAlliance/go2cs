// package main -- go2cs converted at 2020 October 08 04:57:34 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\recover.go
// Tests of panic/recover.

using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static long fortyTwo() => func((defer, panic, recover) =>
        {
            long r = default;

            r = 42L; 
            // The next two statements simulate a 'return' statement.
            defer(() =>
            {
                recover();
            }());
            panic(null);

        });

        private static long zero() => func((defer, panic, recover) =>
        {
            defer(() =>
            {
                recover();
            }());
            panic(1L);

        });

        private static (long, @string) zeroEmpty() => func((defer, panic, recover) =>
        {
            long _p0 = default;
            @string _p0 = default;

            defer(() =>
            {
                recover();
            }());
            panic(1L);

        });

        private static void Main() => func((_, panic, __) =>
        {
            {
                var r__prev1 = r;

                var r = fortyTwo();

                if (r != 42L)
                {
                    panic(r);
                }

                r = r__prev1;

            }

            {
                var r__prev1 = r;

                r = zero();

                if (r != 0L)
                {
                    panic(r);
                }

                r = r__prev1;

            }

            {
                var r__prev1 = r;

                var (r, s) = zeroEmpty();

                if (r != 0L || s != "")
                {
                    panic(fmt.Sprint(r, s));
                }

                r = r__prev1;

            }

        });
    }
}
