// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (mips64 || mips64le)
// +build linux
// +build mips64 mips64le

// package cpu -- go2cs converted at 2022 March 13 06:46:34 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux_mips64x.go
namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

// HWCAP bits. These are exposed by the Linux kernel 5.4.
 
// CPU features
private static readonly nint hwcap_MIPS_MSA = 1 << 1;

private static void doinit() { 
    // HWCAP feature bits
    MIPS64X.HasMSA = isSet(hwCap, hwcap_MIPS_MSA);
}

private static bool isSet(nuint hwc, nuint value) {
    return hwc & value != 0;
}

} // end cpu_package
