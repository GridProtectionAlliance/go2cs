// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// map_ssa.go tests map operations.
// package main -- go2cs converted at 2020 August 29 09:58:23 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\map.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        //go:noinline
        private static long lenMap_ssa(map<long, long> v)
        {
            return len(v);
        }

        private static void testLenMap()
        {
            var v = make_map<long, long>();
            v[0L] = 0L;
            v[1L] = 0L;
            v[2L] = 0L;

            {
                long want = 3L;
                var got = lenMap_ssa(v);

                if (got != want)
                {
                    fmt.Printf("expected len(map) = %d, got %d", want, got);
                    failed = true;
                }

            }
        }

        private static void testLenNilMap()
        {
            map<long, long> v = default;
            {
                long want = 0L;
                var got = lenMap_ssa(v);

                if (got != want)
                {
                    fmt.Printf("expected len(nil) = %d, got %d", want, got);
                    failed = true;
                }

            }
        }
        private static void Main() => func((_, panic, __) =>
        {
            testLenMap();
            testLenNilMap();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
