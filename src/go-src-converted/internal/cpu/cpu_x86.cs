// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 amd64 amd64p32

// package cpu -- go2cs converted at 2020 August 29 08:22:21 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_x86.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLineSize = 64L;

        // cpuid is implemented in cpu_x86.s.


        // cpuid is implemented in cpu_x86.s.
        private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg)
;

        // xgetbv with ecx = 0 is implemented in cpu_x86.s.
        private static (uint, uint) xgetbv()
;

        private static void init()
        {
            var (maxID, _, _, _) = cpuid(0L, 0L);

            if (maxID < 1L)
            {>>MARKER:FUNCTION_xgetbv_BLOCK_PREFIX<<
                return;
            }
            var (_, _, ecx1, edx1) = cpuid(1L, 0L);
            X86.HasSSE2 = isSet(26L, edx1);

            X86.HasSSE3 = isSet(0L, ecx1);
            X86.HasPCLMULQDQ = isSet(1L, ecx1);
            X86.HasSSSE3 = isSet(9L, ecx1);
            X86.HasFMA = isSet(12L, ecx1);
            X86.HasSSE41 = isSet(19L, ecx1);
            X86.HasSSE42 = isSet(20L, ecx1);
            X86.HasPOPCNT = isSet(23L, ecx1);
            X86.HasAES = isSet(25L, ecx1);
            X86.HasOSXSAVE = isSet(27L, ecx1);

            var osSupportsAVX = false; 
            // For XGETBV, OSXSAVE bit is required and sufficient.
            if (X86.HasOSXSAVE)
            {>>MARKER:FUNCTION_cpuid_BLOCK_PREFIX<<
                var (eax, _) = xgetbv(); 
                // Check if XMM and YMM registers have OS support.
                osSupportsAVX = isSet(1L, eax) && isSet(2L, eax);
            }
            X86.HasAVX = isSet(28L, ecx1) && osSupportsAVX;

            if (maxID < 7L)
            {
                return;
            }
            var (_, ebx7, _, _) = cpuid(7L, 0L);
            X86.HasBMI1 = isSet(3L, ebx7);
            X86.HasAVX2 = isSet(5L, ebx7) && osSupportsAVX;
            X86.HasBMI2 = isSet(8L, ebx7);
            X86.HasERMS = isSet(9L, ebx7);
            X86.HasADX = isSet(19L, ebx7);
        }

        private static bool isSet(ulong bitpos, uint value)
        {
            return value & (1L << (int)(bitpos)) != 0L;
        }
    }
}}
