// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:42:06 UTC
// Original source: C:\Go\src\cmd\oldlink\internal\ld\testdata\issue10978\main.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void undefined()
;

        private static long defined1()
        { 
            // To check multiple errors for a single symbol,
            // reference undefined more than once.
            undefined();
            undefined();
            return 0L;

        }

        private static void defined2()
        {
            undefined();
            undefined();
        }

        private static void init()
        {
            _ = defined1();
            defined2();
        }

        // The "main" function remains undeclared.
    }
}
