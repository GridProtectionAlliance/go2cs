// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2020 August 29 08:22:20 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_ppc64x.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLineSize = 128L;

        // ppc64x doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are linknamed in runtime/os_linux_ppc64x.go and are initialized by
        // archauxv().


        // ppc64x doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are linknamed in runtime/os_linux_ppc64x.go and are initialized by
        // archauxv().
        private static ulong ppc64x_hwcap = default;
        private static ulong ppc64x_hwcap2 = default;

        // HWCAP/HWCAP2 bits. These are exposed by the kernel.
 
        // ISA Level
        private static readonly ulong _PPC_FEATURE2_ARCH_2_07 = 0x80000000UL;
        private static readonly ulong _PPC_FEATURE2_ARCH_3_00 = 0x00800000UL; 

        // CPU features
        private static readonly ulong _PPC_FEATURE_HAS_ALTIVEC = 0x10000000UL;
        private static readonly ulong _PPC_FEATURE_HAS_DFP = 0x00000400UL;
        private static readonly ulong _PPC_FEATURE_HAS_VSX = 0x00000080UL;
        private static readonly ulong _PPC_FEATURE2_HAS_HTM = 0x40000000UL;
        private static readonly ulong _PPC_FEATURE2_HAS_ISEL = 0x08000000UL;
        private static readonly ulong _PPC_FEATURE2_HAS_VEC_CRYPTO = 0x02000000UL;
        private static readonly ulong _PPC_FEATURE2_HTM_NOSC = 0x01000000UL;
        private static readonly ulong _PPC_FEATURE2_DARN = 0x00200000UL;
        private static readonly ulong _PPC_FEATURE2_SCV = 0x00100000UL;

        private static void init()
        { 
            // HWCAP feature bits
            PPC64.HasVMX = isSet(ppc64x_hwcap, _PPC_FEATURE_HAS_ALTIVEC);
            PPC64.HasDFP = isSet(ppc64x_hwcap, _PPC_FEATURE_HAS_DFP);
            PPC64.HasVSX = isSet(ppc64x_hwcap, _PPC_FEATURE_HAS_VSX); 

            // HWCAP2 feature bits
            PPC64.IsPOWER8 = isSet(ppc64x_hwcap2, _PPC_FEATURE2_ARCH_2_07);
            PPC64.HasHTM = isSet(ppc64x_hwcap2, _PPC_FEATURE2_HAS_HTM);
            PPC64.HasISEL = isSet(ppc64x_hwcap2, _PPC_FEATURE2_HAS_ISEL);
            PPC64.HasVCRYPTO = isSet(ppc64x_hwcap2, _PPC_FEATURE2_HAS_VEC_CRYPTO);
            PPC64.HasHTMNOSC = isSet(ppc64x_hwcap2, _PPC_FEATURE2_HTM_NOSC);
            PPC64.IsPOWER9 = isSet(ppc64x_hwcap2, _PPC_FEATURE2_ARCH_3_00);
            PPC64.HasDARN = isSet(ppc64x_hwcap2, _PPC_FEATURE2_DARN);
            PPC64.HasSCV = isSet(ppc64x_hwcap2, _PPC_FEATURE2_SCV);
        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}
