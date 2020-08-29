// package main -- go2cs converted at 2020 August 29 09:58:12 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\divbyzero.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        private static bool checkDivByZero(Action f) => func((defer, _, __) =>
        {
            defer(() =>
            {
                {
                    var r = recover();

                    if (r != null)
                    {
                        {
                            runtime.Error (e, ok) = r._<runtime.Error>();

                            if (ok && e.Error() == "runtime error: integer divide by zero")
                            {
                                divByZero = true;
                            }

                        }
                    }

                }
            }());
            f();
            return false;
        });

        //go:noinline
        private static long a(ulong i, slice<long> s)
        {
            return s[i % uint(len(s))];
        }

        //go:noinline
        private static ulong b(ulong i, ulong j)
        {
            return i / j;
        }

        //go:noinline
        private static long c(long i)
        {
            return 7L / (i - i);
        }

        private static void Main() => func((_, panic, __) =>
        {
            {
                var got__prev1 = got;

                var got = checkDivByZero(() =>
                {
                    b(7L, 0L);

                });

                if (!got)
                {
                    fmt.Printf("expected div by zero for b(7, 0), got no error\n");
                    failed = true;
                }

                got = got__prev1;

            }
            {
                var got__prev1 = got;

                got = checkDivByZero(() =>
                {
                    b(7L, 7L);

                });

                if (got)
                {
                    fmt.Printf("expected no error for b(7, 7), got div by zero\n");
                    failed = true;
                }

                got = got__prev1;

            }
            {
                var got__prev1 = got;

                got = checkDivByZero(() =>
                {
                    a(4L, null);

                });

                if (!got)
                {
                    fmt.Printf("expected div by zero for a(4, nil), got no error\n");
                    failed = true;
                }

                got = got__prev1;

            }
            {
                var got__prev1 = got;

                got = checkDivByZero(() =>
                {
                    c(5L);

                });

                if (!got)
                {
                    fmt.Printf("expected div by zero for c(5), got no error\n");
                    failed = true;
                }

                got = got__prev1;

            }

            if (failed)
            {
                panic("tests failed");
            }
        });
    }
}
