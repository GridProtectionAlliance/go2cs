// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// append_ssa.go tests append operations.
// package main -- go2cs converted at 2020 August 29 09:30:12 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\append.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        //go:noinline
        private static slice<long> appendOne_ssa(slice<long> a, long x)
        {
            return append(a, x);
        }

        //go:noinline
        private static slice<long> appendThree_ssa(slice<long> a, long x, long y, long z)
        {
            return append(a, x, y, z);
        }

        private static bool eq(slice<long> a, slice<long> b)
        {
            if (len(a) != len(b))
            {
                return false;
            }
            foreach (var (i) in a)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static void expect(slice<long> got, slice<long> want)
        {
            if (eq(got, want))
            {
                return;
            }
            fmt.Printf("expected %v, got %v\n", want, got);
            failed = true;
        }

        private static void testAppend()
        {
            array<long> store = new array<long>(7L);
            var a = store[..0L];

            a = appendOne_ssa(a, 1L);
            expect(a, new slice<long>(new long[] { 1 }));
            a = appendThree_ssa(a, 2L, 3L, 4L);
            expect(a, new slice<long>(new long[] { 1, 2, 3, 4 }));
            a = appendThree_ssa(a, 5L, 6L, 7L);
            expect(a, new slice<long>(new long[] { 1, 2, 3, 4, 5, 6, 7 }));
            if (ref a[0L] != ref store[0L])
            {
                fmt.Println("unnecessary grow");
                failed = true;
            }
            a = appendOne_ssa(a, 8L);
            expect(a, new slice<long>(new long[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            if (ref a[0L] == ref store[0L])
            {
                fmt.Println("didn't grow");
                failed = true;
            }
        }

        private static void Main() => func((_, panic, __) =>
        {
            testAppend();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
