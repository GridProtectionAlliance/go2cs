// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// chan_ssa.go tests chan operations.
// package main -- go2cs converted at 2020 August 29 09:57:38 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\chan.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        //go:noinline
        private static long lenChan_ssa(channel<long> v)
        {
            return len(v);
        }

        //go:noinline
        private static long capChan_ssa(channel<long> v)
        {
            return cap(v);
        }

        private static void testLenChan()
        {
            var v = make_channel<long>(10L);
            v.Send(1L);
            v.Send(1L);
            v.Send(1L);

            {
                long want = 3L;
                var got = lenChan_ssa(v);

                if (got != want)
                {
                    fmt.Printf("expected len(chan) = %d, got %d", want, got);
                    failed = true;
                }

            }
        }

        private static void testLenNilChan()
        {
            channel<long> v = default;
            {
                long want = 0L;
                var got = lenChan_ssa(v);

                if (got != want)
                {
                    fmt.Printf("expected len(nil) = %d, got %d", want, got);
                    failed = true;
                }

            }
        }

        private static void testCapChan()
        {
            var v = make_channel<long>(25L);

            {
                long want = 25L;
                var got = capChan_ssa(v);

                if (got != want)
                {
                    fmt.Printf("expected cap(chan) = %d, got %d", want, got);
                    failed = true;
                }

            }
        }

        private static void testCapNilChan()
        {
            channel<long> v = default;
            {
                long want = 0L;
                var got = capChan_ssa(v);

                if (got != want)
                {
                    fmt.Printf("expected cap(nil) = %d, got %d", want, got);
                    failed = true;
                }

            }
        }

        private static void Main() => func((_, panic, __) =>
        {
            testLenChan();
            testLenNilChan();

            testCapChan();
            testCapNilChan();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
