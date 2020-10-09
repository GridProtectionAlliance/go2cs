// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 06:07:54 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_arm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        private static void doinit()
        {
            ARM.HasSWP = isSet(hwCap, hwcap_SWP);
            ARM.HasHALF = isSet(hwCap, hwcap_HALF);
            ARM.HasTHUMB = isSet(hwCap, hwcap_THUMB);
            ARM.Has26BIT = isSet(hwCap, hwcap_26BIT);
            ARM.HasFASTMUL = isSet(hwCap, hwcap_FAST_MULT);
            ARM.HasFPA = isSet(hwCap, hwcap_FPA);
            ARM.HasVFP = isSet(hwCap, hwcap_VFP);
            ARM.HasEDSP = isSet(hwCap, hwcap_EDSP);
            ARM.HasJAVA = isSet(hwCap, hwcap_JAVA);
            ARM.HasIWMMXT = isSet(hwCap, hwcap_IWMMXT);
            ARM.HasCRUNCH = isSet(hwCap, hwcap_CRUNCH);
            ARM.HasTHUMBEE = isSet(hwCap, hwcap_THUMBEE);
            ARM.HasNEON = isSet(hwCap, hwcap_NEON);
            ARM.HasVFPv3 = isSet(hwCap, hwcap_VFPv3);
            ARM.HasVFPv3D16 = isSet(hwCap, hwcap_VFPv3D16);
            ARM.HasTLS = isSet(hwCap, hwcap_TLS);
            ARM.HasVFPv4 = isSet(hwCap, hwcap_VFPv4);
            ARM.HasIDIVA = isSet(hwCap, hwcap_IDIVA);
            ARM.HasIDIVT = isSet(hwCap, hwcap_IDIVT);
            ARM.HasVFPD32 = isSet(hwCap, hwcap_VFPD32);
            ARM.HasLPAE = isSet(hwCap, hwcap_LPAE);
            ARM.HasEVTSTRM = isSet(hwCap, hwcap_EVTSTRM);
            ARM.HasAES = isSet(hwCap2, hwcap2_AES);
            ARM.HasPMULL = isSet(hwCap2, hwcap2_PMULL);
            ARM.HasSHA1 = isSet(hwCap2, hwcap2_SHA1);
            ARM.HasSHA2 = isSet(hwCap2, hwcap2_SHA2);
            ARM.HasCRC32 = isSet(hwCap2, hwcap2_CRC32);
        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}}}}
