// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_arm64.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // HWCAP/HWCAP2 bits. These are exposed by Linux.
private static readonly nint hwcap_FP = 1 << 0;
private static readonly nint hwcap_ASIMD = 1 << 1;
private static readonly nint hwcap_EVTSTRM = 1 << 2;
private static readonly nint hwcap_AES = 1 << 3;
private static readonly nint hwcap_PMULL = 1 << 4;
private static readonly nint hwcap_SHA1 = 1 << 5;
private static readonly nint hwcap_SHA2 = 1 << 6;
private static readonly nint hwcap_CRC32 = 1 << 7;
private static readonly nint hwcap_ATOMICS = 1 << 8;
private static readonly nint hwcap_FPHP = 1 << 9;
private static readonly nint hwcap_ASIMDHP = 1 << 10;
private static readonly nint hwcap_CPUID = 1 << 11;
private static readonly nint hwcap_ASIMDRDM = 1 << 12;
private static readonly nint hwcap_JSCVT = 1 << 13;
private static readonly nint hwcap_FCMA = 1 << 14;
private static readonly nint hwcap_LRCPC = 1 << 15;
private static readonly nint hwcap_DCPOP = 1 << 16;
private static readonly nint hwcap_SHA3 = 1 << 17;
private static readonly nint hwcap_SM3 = 1 << 18;
private static readonly nint hwcap_SM4 = 1 << 19;
private static readonly nint hwcap_ASIMDDP = 1 << 20;
private static readonly nint hwcap_SHA512 = 1 << 21;
private static readonly nint hwcap_SVE = 1 << 22;
private static readonly nint hwcap_ASIMDFHM = 1 << 23;


private static void doinit() {
    {
        var err = readHWCAP();

        if (err != null) { 
            // failed to read /proc/self/auxv, try reading registers directly
            readARM64Registers();
            return ;

        }
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

private static bool isSet(nuint hwc, nuint value) {
    return hwc & value != 0;
}

} // end cpu_package
