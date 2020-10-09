// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2020 October 09 04:45:31 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_ppc64x.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static readonly long CacheLinePadSize = (long)128L;

        // ppc64x doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are initialized by archauxv and should not be changed after they are
        // initialized.
        // On aix/ppc64, these values are initialized early in the runtime in runtime/os_aix.go.


        // ppc64x doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
        // These are initialized by archauxv and should not be changed after they are
        // initialized.
        // On aix/ppc64, these values are initialized early in the runtime in runtime/os_aix.go.
        public static ulong HWCap = default;
        public static ulong HWCap2 = default;

        // HWCAP/HWCAP2 bits. These are exposed by the kernel.
 
        // ISA Level
        public static readonly ulong PPC_FEATURE2_ARCH_2_07 = (ulong)0x80000000UL;
        public static readonly ulong PPC_FEATURE2_ARCH_3_00 = (ulong)0x00800000UL; 

        // CPU features
        public static readonly ulong PPC_FEATURE2_DARN = (ulong)0x00200000UL;
        public static readonly ulong PPC_FEATURE2_SCV = (ulong)0x00100000UL;


        private static void doinit()
        {
            options = new slice<option>(new option[] { {Name:"darn",Feature:&PPC64.HasDARN}, {Name:"scv",Feature:&PPC64.HasSCV}, {Name:"power9",Feature:&PPC64.IsPOWER9}, {Name:"power8",Feature:&PPC64.IsPOWER8,Required:true} }); 

            // HWCAP2 feature bits
            PPC64.IsPOWER8 = isSet(HWCap2, PPC_FEATURE2_ARCH_2_07);
            PPC64.IsPOWER9 = isSet(HWCap2, PPC_FEATURE2_ARCH_3_00);
            PPC64.HasDARN = isSet(HWCap2, PPC_FEATURE2_DARN);
            PPC64.HasSCV = isSet(HWCap2, PPC_FEATURE2_SCV);

        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}
