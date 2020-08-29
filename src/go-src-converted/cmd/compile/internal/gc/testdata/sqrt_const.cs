// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:58:27 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\sqrt_const.go
using fmt = go.fmt_package;
using math = go.math_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {




        private static var failed = false;

        private static void Main() => func((_, panic, __) =>
        {
            {
                var test__prev1 = test;

                foreach (var (_, __test) in tests)
                {
                    test = __test;
                    if (test.got != test.want)
                    {
                        fmt.Printf("%s: math.Sqrt(%f): got %f, want %f\n", test.name, test.@in, test.got, test.want);
                        failed = true;
                    }
                }

                test = test__prev1;
            }

            {
                var test__prev1 = test;

                foreach (var (_, __test) in nanTests)
                {
                    test = __test;
                    if (math.IsNaN(test.got) != true)
                    {
                        fmt.Printf("%s: math.Sqrt(%f): got %f, want NaN\n", test.name, test.@in, test.got);
                        failed = true;
                    }
                }

                test = test__prev1;
            }

            {
                var got = math.Sqrt(math.Inf(1L));

                if (!math.IsInf(got, 1L))
                {
                    fmt.Printf("math.Sqrt(+Inf), got %f, want +Inf\n", got);
                    failed = true;
                }

            }

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
