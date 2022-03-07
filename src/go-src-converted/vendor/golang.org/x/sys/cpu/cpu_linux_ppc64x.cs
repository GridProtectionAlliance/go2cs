// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (ppc64 || ppc64le)
// +build linux
// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_ppc64x.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // HWCAP/HWCAP2 bits. These are exposed by the kernel.
 
// ISA Level
private static readonly nuint _PPC_FEATURE2_ARCH_2_07 = 0x80000000;
private static readonly nuint _PPC_FEATURE2_ARCH_3_00 = 0x00800000; 

// CPU features
private static readonly nuint _PPC_FEATURE2_DARN = 0x00200000;
private static readonly nuint _PPC_FEATURE2_SCV = 0x00100000;


private static void doinit() { 
    // HWCAP2 feature bits
    PPC64.IsPOWER8 = isSet(hwCap2, _PPC_FEATURE2_ARCH_2_07);
    PPC64.IsPOWER9 = isSet(hwCap2, _PPC_FEATURE2_ARCH_3_00);
    PPC64.HasDARN = isSet(hwCap2, _PPC_FEATURE2_DARN);
    PPC64.HasSCV = isSet(hwCap2, _PPC_FEATURE2_SCV);

}

private static bool isSet(nuint hwc, nuint value) {
    return hwc & value != 0;
}

} // end cpu_package
