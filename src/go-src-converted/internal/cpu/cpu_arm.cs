// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_arm.go


namespace go.@internal;

public static partial class cpu_package {

public static readonly nint CacheLinePadSize = 32;

// arm doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
// These are initialized by archauxv() and should not be changed after they are
// initialized.


// arm doesn't have a 'cpuid' equivalent, so we rely on HWCAP/HWCAP2.
// These are initialized by archauxv() and should not be changed after they are
// initialized.
public static nuint HWCap = default;
public static nuint HWCap2 = default;

// HWCAP/HWCAP2 bits. These are exposed by Linux and FreeBSD.
private static readonly nint hwcap_VFPv4 = 1 << 16;
private static readonly nint hwcap_IDIVA = 1 << 17;


private static void doinit() {
    options = new slice<option>(new option[] { {Name:"vfpv4",Feature:&ARM.HasVFPv4}, {Name:"idiva",Feature:&ARM.HasIDIVA} }); 

    // HWCAP feature bits
    ARM.HasVFPv4 = isSet(HWCap, hwcap_VFPv4);
    ARM.HasIDIVA = isSet(HWCap, hwcap_IDIVA);

}

private static bool isSet(nuint hwc, nuint value) {
    return hwc & value != 0;
}

} // end cpu_package
