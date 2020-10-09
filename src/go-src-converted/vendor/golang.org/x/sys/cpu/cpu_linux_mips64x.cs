// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build mips64 mips64le

// package cpu -- go2cs converted at 2020 October 09 06:07:54 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_mips64x.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        // HWCAP bits. These are exposed by the Linux kernel 5.4.
 
        // CPU features
        private static readonly long hwcap_MIPS_MSA = (long)1L << (int)(1L);


        private static void doinit()
        { 
            // HWCAP feature bits
            MIPS64X.HasMSA = isSet(hwCap, hwcap_MIPS_MSA);

        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}}}}
