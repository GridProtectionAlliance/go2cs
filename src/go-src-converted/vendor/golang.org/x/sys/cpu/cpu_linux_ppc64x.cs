// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2020 October 09 06:07:54 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_ppc64x.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        private static readonly long cacheLineSize = (long)128L;

        // HWCAP/HWCAP2 bits. These are exposed by the kernel.


        // HWCAP/HWCAP2 bits. These are exposed by the kernel.
 
        // ISA Level
        private static readonly ulong _PPC_FEATURE2_ARCH_2_07 = (ulong)0x80000000UL;
        private static readonly ulong _PPC_FEATURE2_ARCH_3_00 = (ulong)0x00800000UL; 

        // CPU features
        private static readonly ulong _PPC_FEATURE2_DARN = (ulong)0x00200000UL;
        private static readonly ulong _PPC_FEATURE2_SCV = (ulong)0x00100000UL;


        private static void doinit()
        { 
            // HWCAP2 feature bits
            PPC64.IsPOWER8 = isSet(hwCap2, _PPC_FEATURE2_ARCH_2_07);
            PPC64.IsPOWER9 = isSet(hwCap2, _PPC_FEATURE2_ARCH_3_00);
            PPC64.HasDARN = isSet(hwCap2, _PPC_FEATURE2_DARN);
            PPC64.HasSCV = isSet(hwCap2, _PPC_FEATURE2_SCV);

        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}}}}
