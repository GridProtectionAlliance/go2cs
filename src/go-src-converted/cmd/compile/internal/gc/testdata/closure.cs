// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// map_ssa.go tests map operations.
// package main -- go2cs converted at 2020 August 29 09:57:39 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\closure.go
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static var failed = false;

        //go:noinline
        private static long testCFunc_ssa()
        {
            long a = 0L;
            Action b = () =>
            {

                                a++;
            }
;
            b();
            b();
            return a;
        }

        private static void testCFunc()
        {
            {
                long want = 2L;
                var got = testCFunc_ssa();

                if (got != want)
                {
                    fmt.Printf("expected %d, got %d", want, got);
                    failed = true;
                }

            }
        }

        private static void Main() => func((_, panic, __) =>
        {
            testCFunc();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
