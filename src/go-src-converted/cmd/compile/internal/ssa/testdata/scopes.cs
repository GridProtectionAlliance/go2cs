// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:39:57 UTC
// Original source: C:\Go\src\cmd\compile\internal\ssa\testdata\scopes.go
using fmt = go.fmt_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            growstack(); // Use stack early to prevent growth during test, which confuses gdb
            test();

        }

        //go:noinline
        private static long id(long x)
        {
            return x;
        }

        private static void test()
        {
            var x = id(0L);
            var y = id(0L);
            fmt.Println(x);
            {
                var i__prev1 = i;

                for (var i = x; i < 3L; i++)
                {
                    x = i * i;
                    y += id(x); //gdb-dbg=(x,y)//gdb-opt=(x,y)
                }


                i = i__prev1;
            }
            y = x + y; //gdb-dbg=(x,y)//gdb-opt=(x,y)
            fmt.Println(x, y);

            {
                var x__prev1 = x;

                for (x = 0L; x <= 1L; x++)
                { // From delve scopetest.go
                    var a = y;
                    f1(a);
                    {
                        long b = 0L;
                        f2(b);
                        if (gretbool())
                        {
                            long c = 0L;
                            f3(c);
                        }
                        else
                        {
                            c = 1.1F;
                            f4(int(c));
                        }

                        f5(b);

                    }
                    f6(a);

                }


                x = x__prev1;
            }

            { // From delve testnextprog.go
                var j = id(1L);                var f = id(2L);
                {
                    var i__prev1 = i;

                    for (i = 0L; i <= 5L; i++)
                    {
                        j += j * (j ^ 3L) / 100L;
                        if (i == f)
                        {
                            fmt.Println("foo");
                            break;
                        }

                        sleepytime();

                    }


                    i = i__prev1;
                }
                helloworld();

            }

        }

        private static void sleepytime()
        {
            time.Sleep(5L * time.Millisecond);
        }

        private static void helloworld()
        {
            fmt.Println("Hello, World!");
        }

        //go:noinline
        private static void f1(long x)
        {
        }

        //go:noinline
        private static void f2(long x)
        {
        }

        //go:noinline
        private static void f3(long x)
        {
        }

        //go:noinline
        private static void f4(long x)
        {
        }

        //go:noinline
        private static void f5(long x)
        {
        }

        //go:noinline
        private static void f6(long x)
        {
        }

        private static var boolvar = true;

        private static bool gretbool()
        {
            var x = boolvar;
            boolvar = !boolvar;
            return x;
        }

        private static @string sink = default;

        //go:noinline
        private static void growstack()
        {
            sink = fmt.Sprintf("%#v,%#v,%#v", 1L, true, "cat");
        }
    }
}
