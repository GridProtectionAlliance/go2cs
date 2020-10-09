// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !gccgo

// package main -- go2cs converted at 2020 October 09 05:44:45 UTC
// Original source: C:\Go\src\cmd\dist\util_gc.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void cpuid(ptr<array<uint>> info, uint ax)
;

        private static bool cansse2()
        {
            if (gohostarch != "386" && gohostarch != "amd64")
            {>>MARKER:FUNCTION_cpuid_BLOCK_PREFIX<<
                return false;
            }

            ref array<uint> info = ref heap(new array<uint>(4L), out ptr<array<uint>> _addr_info);
            cpuid(_addr_info, 1L);
            return info[3L] & (1L << (int)(26L)) != 0L; // SSE2
        }

        // useVFPv1 tries to execute one VFPv1 instruction on ARM.
        // It will crash the current process if VFPv1 is missing.
        private static void useVFPv1()
;

        // useVFPv3 tries to execute one VFPv3 instruction on ARM.
        // It will crash the current process if VFPv3 is missing.
        private static void useVFPv3()
;

        // useARMv6K tries to run ARMv6K instructions on ARM.
        // It will crash the current process if it doesn't implement
        // ARMv6K or above.
        private static void useARMv6K()
;
    }
}
