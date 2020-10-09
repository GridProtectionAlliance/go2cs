// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 amd64

// package cpu -- go2cs converted at 2020 October 09 04:45:32 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_x86.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLinePadSize = (long)64L;

        // cpuid is implemented in cpu_x86.s.


        // cpuid is implemented in cpu_x86.s.
        private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg)
;

        // xgetbv with ecx = 0 is implemented in cpu_x86.s.
        private static (uint, uint) xgetbv()
;

 
        // edx bits
        private static readonly long cpuid_SSE2 = (long)1L << (int)(26L); 

        // ecx bits
        private static readonly long cpuid_SSE3 = (long)1L << (int)(0L);
        private static readonly long cpuid_PCLMULQDQ = (long)1L << (int)(1L);
        private static readonly long cpuid_SSSE3 = (long)1L << (int)(9L);
        private static readonly long cpuid_FMA = (long)1L << (int)(12L);
        private static readonly long cpuid_SSE41 = (long)1L << (int)(19L);
        private static readonly long cpuid_SSE42 = (long)1L << (int)(20L);
        private static readonly long cpuid_POPCNT = (long)1L << (int)(23L);
        private static readonly long cpuid_AES = (long)1L << (int)(25L);
        private static readonly long cpuid_OSXSAVE = (long)1L << (int)(27L);
        private static readonly long cpuid_AVX = (long)1L << (int)(28L); 

        // ebx bits
        private static readonly long cpuid_BMI1 = (long)1L << (int)(3L);
        private static readonly long cpuid_AVX2 = (long)1L << (int)(5L);
        private static readonly long cpuid_BMI2 = (long)1L << (int)(8L);
        private static readonly long cpuid_ERMS = (long)1L << (int)(9L);
        private static readonly long cpuid_ADX = (long)1L << (int)(19L);


        private static void doinit()
        {
            options = new slice<option>(new option[] { {Name:"adx",Feature:&X86.HasADX}, {Name:"aes",Feature:&X86.HasAES}, {Name:"avx",Feature:&X86.HasAVX}, {Name:"avx2",Feature:&X86.HasAVX2}, {Name:"bmi1",Feature:&X86.HasBMI1}, {Name:"bmi2",Feature:&X86.HasBMI2}, {Name:"erms",Feature:&X86.HasERMS}, {Name:"fma",Feature:&X86.HasFMA}, {Name:"pclmulqdq",Feature:&X86.HasPCLMULQDQ}, {Name:"popcnt",Feature:&X86.HasPOPCNT}, {Name:"sse3",Feature:&X86.HasSSE3}, {Name:"sse41",Feature:&X86.HasSSE41}, {Name:"sse42",Feature:&X86.HasSSE42}, {Name:"ssse3",Feature:&X86.HasSSSE3}, {Name:"sse2",Feature:&X86.HasSSE2,Required:GOARCH=="amd64"} });

            var (maxID, _, _, _) = cpuid(0L, 0L);

            if (maxID < 1L)
            {>>MARKER:FUNCTION_xgetbv_BLOCK_PREFIX<<
                return ;
            }

            var (_, _, ecx1, edx1) = cpuid(1L, 0L);
            X86.HasSSE2 = isSet(edx1, cpuid_SSE2);

            X86.HasSSE3 = isSet(ecx1, cpuid_SSE3);
            X86.HasPCLMULQDQ = isSet(ecx1, cpuid_PCLMULQDQ);
            X86.HasSSSE3 = isSet(ecx1, cpuid_SSSE3);
            X86.HasFMA = isSet(ecx1, cpuid_FMA);
            X86.HasSSE41 = isSet(ecx1, cpuid_SSE41);
            X86.HasSSE42 = isSet(ecx1, cpuid_SSE42);
            X86.HasPOPCNT = isSet(ecx1, cpuid_POPCNT);
            X86.HasAES = isSet(ecx1, cpuid_AES);
            X86.HasOSXSAVE = isSet(ecx1, cpuid_OSXSAVE);

            var osSupportsAVX = false; 
            // For XGETBV, OSXSAVE bit is required and sufficient.
            if (X86.HasOSXSAVE)
            {>>MARKER:FUNCTION_cpuid_BLOCK_PREFIX<<
                var (eax, _) = xgetbv(); 
                // Check if XMM and YMM registers have OS support.
                osSupportsAVX = isSet(eax, 1L << (int)(1L)) && isSet(eax, 1L << (int)(2L));

            }

            X86.HasAVX = isSet(ecx1, cpuid_AVX) && osSupportsAVX;

            if (maxID < 7L)
            {
                return ;
            }

            var (_, ebx7, _, _) = cpuid(7L, 0L);
            X86.HasBMI1 = isSet(ebx7, cpuid_BMI1);
            X86.HasAVX2 = isSet(ebx7, cpuid_AVX2) && osSupportsAVX;
            X86.HasBMI2 = isSet(ebx7, cpuid_BMI2);
            X86.HasERMS = isSet(ebx7, cpuid_ERMS);
            X86.HasADX = isSet(ebx7, cpuid_ADX);

        }

        private static bool isSet(uint hwc, uint value)
        {
            return hwc & value != 0L;
        }
    }
}}
