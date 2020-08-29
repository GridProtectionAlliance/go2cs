// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gccgo

// package main -- go2cs converted at 2020 August 29 10:00:01 UTC
// Original source: C:\Go\src\cmd\dist\util_gccgo.go
/*
int supports_sse2() {
#if defined(__i386__) || defined(__x86_64__)
    return __builtin_cpu_supports("sse2");
#else
    return 0;
#endif
}
*/
using C = go.C_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static bool cansse2()
        {
            return C.supports_sse2() != 0L;
        }

        private static void useVFPv1()
        {
        }

        private static void useVFPv3()
        {
        }

        private static void useARMv6K()
        {
        }
    }
}
