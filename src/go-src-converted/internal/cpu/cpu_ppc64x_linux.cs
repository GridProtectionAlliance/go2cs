// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build ppc64 || ppc64le
// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_ppc64x_linux.go


namespace go.@internal;

public static partial class cpu_package {

    // ppc64 doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
    // These are initialized by archauxv and should not be changed after they are
    // initialized.
public static nuint HWCap = default;
public static nuint HWCap2 = default;

// HWCAP bits. These are exposed by Linux.
 
// ISA Level
private static readonly nuint hwcap2_ARCH_3_00 = 0x00800000; 

// CPU features
private static readonly nuint hwcap2_DARN = 0x00200000;
private static readonly nuint hwcap2_SCV = 0x00100000;


private static void osinit() {
    PPC64.IsPOWER9 = isSet(HWCap2, hwcap2_ARCH_3_00);
    PPC64.HasDARN = isSet(HWCap2, hwcap2_DARN);
    PPC64.HasSCV = isSet(HWCap2, hwcap2_SCV);
}

} // end cpu_package
