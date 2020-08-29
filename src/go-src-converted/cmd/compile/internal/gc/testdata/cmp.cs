// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// cmp_ssa.go tests compare simplification operations.
// package main -- go2cs converted at 2020 August 29 09:57:39 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\cmp.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        //go:noinline
        private static bool eq_ssa(long a)
        {
            return 4L + a == 10L;
        }

        //go:noinline
        private static bool neq_ssa(long a)
        {
            return 10L != a + 4L;
        }

        private static void testCmp()
        {
            {
                var wanted__prev1 = wanted;
                var got__prev1 = got;

                var wanted = true;
                var got = eq_ssa(6L);

                if (wanted != got)
                {
                    fmt.Printf("eq_ssa: expected %v, got %v\n", wanted, got);
                    failed = true;
                }

                wanted = wanted__prev1;
                got = got__prev1;

            }
            {
                var wanted__prev1 = wanted;
                var got__prev1 = got;

                wanted = false;
                got = eq_ssa(7L);

                if (wanted != got)
                {
                    fmt.Printf("eq_ssa: expected %v, got %v\n", wanted, got);
                    failed = true;
                }

                wanted = wanted__prev1;
                got = got__prev1;

            }

            {
                var wanted__prev1 = wanted;
                var got__prev1 = got;

                wanted = false;
                got = neq_ssa(6L);

                if (wanted != got)
                {
                    fmt.Printf("neq_ssa: expected %v, got %v\n", wanted, got);
                    failed = true;
                }

                wanted = wanted__prev1;
                got = got__prev1;

            }
            {
                var wanted__prev1 = wanted;
                var got__prev1 = got;

                wanted = true;
                got = neq_ssa(7L);

                if (wanted != got)
                {
                    fmt.Printf("neq_ssa: expected %v, got %v\n", wanted, got);
                    failed = true;
                }

                wanted = wanted__prev1;
                got = got__prev1;

            }
        }

        private static void Main() => func((_, panic, __) =>
        {
            testCmp();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
