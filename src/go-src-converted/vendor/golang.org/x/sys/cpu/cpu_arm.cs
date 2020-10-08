// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 08 05:01:48 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_arm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        private static readonly long cacheLineSize = (long)32L;

        // HWCAP/HWCAP2 bits.
        // These are specific to Linux.


        // HWCAP/HWCAP2 bits.
        // These are specific to Linux.
        private static readonly long hwcap_SWP = (long)1L << (int)(0L);
        private static readonly long hwcap_HALF = (long)1L << (int)(1L);
        private static readonly long hwcap_THUMB = (long)1L << (int)(2L);
        private static readonly long hwcap_26BIT = (long)1L << (int)(3L);
        private static readonly long hwcap_FAST_MULT = (long)1L << (int)(4L);
        private static readonly long hwcap_FPA = (long)1L << (int)(5L);
        private static readonly long hwcap_VFP = (long)1L << (int)(6L);
        private static readonly long hwcap_EDSP = (long)1L << (int)(7L);
        private static readonly long hwcap_JAVA = (long)1L << (int)(8L);
        private static readonly long hwcap_IWMMXT = (long)1L << (int)(9L);
        private static readonly long hwcap_CRUNCH = (long)1L << (int)(10L);
        private static readonly long hwcap_THUMBEE = (long)1L << (int)(11L);
        private static readonly long hwcap_NEON = (long)1L << (int)(12L);
        private static readonly long hwcap_VFPv3 = (long)1L << (int)(13L);
        private static readonly long hwcap_VFPv3D16 = (long)1L << (int)(14L);
        private static readonly long hwcap_TLS = (long)1L << (int)(15L);
        private static readonly long hwcap_VFPv4 = (long)1L << (int)(16L);
        private static readonly long hwcap_IDIVA = (long)1L << (int)(17L);
        private static readonly long hwcap_IDIVT = (long)1L << (int)(18L);
        private static readonly long hwcap_VFPD32 = (long)1L << (int)(19L);
        private static readonly long hwcap_LPAE = (long)1L << (int)(20L);
        private static readonly long hwcap_EVTSTRM = (long)1L << (int)(21L);

        private static readonly long hwcap2_AES = (long)1L << (int)(0L);
        private static readonly long hwcap2_PMULL = (long)1L << (int)(1L);
        private static readonly long hwcap2_SHA1 = (long)1L << (int)(2L);
        private static readonly long hwcap2_SHA2 = (long)1L << (int)(3L);
        private static readonly long hwcap2_CRC32 = (long)1L << (int)(4L);

    }
}}}}}
