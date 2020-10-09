// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 06:07:54 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_arm64.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        // HWCAP/HWCAP2 bits. These are exposed by Linux.
        private static readonly long hwcap_FP = (long)1L << (int)(0L);
        private static readonly long hwcap_ASIMD = (long)1L << (int)(1L);
        private static readonly long hwcap_EVTSTRM = (long)1L << (int)(2L);
        private static readonly long hwcap_AES = (long)1L << (int)(3L);
        private static readonly long hwcap_PMULL = (long)1L << (int)(4L);
        private static readonly long hwcap_SHA1 = (long)1L << (int)(5L);
        private static readonly long hwcap_SHA2 = (long)1L << (int)(6L);
        private static readonly long hwcap_CRC32 = (long)1L << (int)(7L);
        private static readonly long hwcap_ATOMICS = (long)1L << (int)(8L);
        private static readonly long hwcap_FPHP = (long)1L << (int)(9L);
        private static readonly long hwcap_ASIMDHP = (long)1L << (int)(10L);
        private static readonly long hwcap_CPUID = (long)1L << (int)(11L);
        private static readonly long hwcap_ASIMDRDM = (long)1L << (int)(12L);
        private static readonly long hwcap_JSCVT = (long)1L << (int)(13L);
        private static readonly long hwcap_FCMA = (long)1L << (int)(14L);
        private static readonly long hwcap_LRCPC = (long)1L << (int)(15L);
        private static readonly long hwcap_DCPOP = (long)1L << (int)(16L);
        private static readonly long hwcap_SHA3 = (long)1L << (int)(17L);
        private static readonly long hwcap_SM3 = (long)1L << (int)(18L);
        private static readonly long hwcap_SM4 = (long)1L << (int)(19L);
        private static readonly long hwcap_ASIMDDP = (long)1L << (int)(20L);
        private static readonly long hwcap_SHA512 = (long)1L << (int)(21L);
        private static readonly long hwcap_SVE = (long)1L << (int)(22L);
        private static readonly long hwcap_ASIMDFHM = (long)1L << (int)(23L);


        private static void doinit()
        {
            {
                var err = readHWCAP();

                if (err != null)
                { 
                    // failed to read /proc/self/auxv, try reading registers directly
                    readARM64Registers();
                    return ;

                } 

                // HWCAP feature bits

            } 

            // HWCAP feature bits
            ARM64.HasFP = isSet(hwCap, hwcap_FP);
            ARM64.HasASIMD = isSet(hwCap, hwcap_ASIMD);
            ARM64.HasEVTSTRM = isSet(hwCap, hwcap_EVTSTRM);
            ARM64.HasAES = isSet(hwCap, hwcap_AES);
            ARM64.HasPMULL = isSet(hwCap, hwcap_PMULL);
            ARM64.HasSHA1 = isSet(hwCap, hwcap_SHA1);
            ARM64.HasSHA2 = isSet(hwCap, hwcap_SHA2);
            ARM64.HasCRC32 = isSet(hwCap, hwcap_CRC32);
            ARM64.HasATOMICS = isSet(hwCap, hwcap_ATOMICS);
            ARM64.HasFPHP = isSet(hwCap, hwcap_FPHP);
            ARM64.HasASIMDHP = isSet(hwCap, hwcap_ASIMDHP);
            ARM64.HasCPUID = isSet(hwCap, hwcap_CPUID);
            ARM64.HasASIMDRDM = isSet(hwCap, hwcap_ASIMDRDM);
            ARM64.HasJSCVT = isSet(hwCap, hwcap_JSCVT);
            ARM64.HasFCMA = isSet(hwCap, hwcap_FCMA);
            ARM64.HasLRCPC = isSet(hwCap, hwcap_LRCPC);
            ARM64.HasDCPOP = isSet(hwCap, hwcap_DCPOP);
            ARM64.HasSHA3 = isSet(hwCap, hwcap_SHA3);
            ARM64.HasSM3 = isSet(hwCap, hwcap_SM3);
            ARM64.HasSM4 = isSet(hwCap, hwcap_SM4);
            ARM64.HasASIMDDP = isSet(hwCap, hwcap_ASIMDDP);
            ARM64.HasSHA512 = isSet(hwCap, hwcap_SHA512);
            ARM64.HasSVE = isSet(hwCap, hwcap_SVE);
            ARM64.HasASIMDFHM = isSet(hwCap, hwcap_ASIMDFHM);

        }

        private static bool isSet(ulong hwc, ulong value)
        {
            return hwc & value != 0L;
        }
    }
}}}}}
