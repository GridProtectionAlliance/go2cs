// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:20 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_s390x.go

using System;


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

 
// bit mask values from /usr/include/bits/hwcap.h
private static readonly nint hwcap_ZARCH = 2;
private static readonly nint hwcap_STFLE = 4;
private static readonly nint hwcap_MSA = 8;
private static readonly nint hwcap_LDISP = 16;
private static readonly nint hwcap_EIMM = 32;
private static readonly nint hwcap_DFP = 64;
private static readonly nint hwcap_ETF3EH = 256;
private static readonly nint hwcap_VX = 2048;
private static readonly nint hwcap_VXE = 8192;


private static void initS390Xbase() { 
    // test HWCAP bit vector
    Func<nuint, bool> has = featureMask => {
        return hwCap & featureMask == featureMask;
    }; 

    // mandatory
    S390X.HasZARCH = has(hwcap_ZARCH); 

    // optional
    S390X.HasSTFLE = has(hwcap_STFLE);
    S390X.HasLDISP = has(hwcap_LDISP);
    S390X.HasEIMM = has(hwcap_EIMM);
    S390X.HasETF3EH = has(hwcap_ETF3EH);
    S390X.HasDFP = has(hwcap_DFP);
    S390X.HasMSA = has(hwcap_MSA);
    S390X.HasVX = has(hwcap_VX);
    if (S390X.HasVX) {
        S390X.HasVXE = has(hwcap_VXE);
    }
}

} // end cpu_package
