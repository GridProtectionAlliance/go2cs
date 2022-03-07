// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips64 || mips64le
// +build mips64 mips64le

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_mips64x.go


namespace go.@internal;

public static partial class cpu_package {

public static readonly nint CacheLinePadSize = 32;

// This is initialized by archauxv and should not be changed after it is
// initialized.


// This is initialized by archauxv and should not be changed after it is
// initialized.
public static nuint HWCap = default;

// HWCAP bits. These are exposed by the Linux kernel 5.4.
 
// CPU features
private static readonly nint hwcap_MIPS_MSA = 1 << 1;


private static void doinit() {
    options = new slice<option>(new option[] { {Name:"msa",Feature:&MIPS64X.HasMSA} }); 

    // HWCAP feature bits
    MIPS64X.HasMSA = isSet(HWCap, hwcap_MIPS_MSA);

}

private static bool isSet(nuint hwc, nuint value) {
    return hwc & value != 0;
}

} // end cpu_package
