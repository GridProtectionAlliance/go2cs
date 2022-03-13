// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm64 && linux
// +build arm64,linux

// package cpu -- go2cs converted at 2022 March 13 05:40:41 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_arm64_hwcap.go
namespace go.@internal;

public static partial class cpu_package {

// HWCap may be initialized by archauxv and
// should not be changed after it was initialized.
public static nuint HWCap = default;

// HWCAP bits. These are exposed by Linux.
private static readonly nint hwcap_AES = 1 << 3;
private static readonly nint hwcap_PMULL = 1 << 4;
private static readonly nint hwcap_SHA1 = 1 << 5;
private static readonly nint hwcap_SHA2 = 1 << 6;
private static readonly nint hwcap_CRC32 = 1 << 7;
private static readonly nint hwcap_ATOMICS = 1 << 8;
private static readonly nint hwcap_CPUID = 1 << 11;

private static void hwcapInit(@string os) { 
    // HWCap was populated by the runtime from the auxiliary vector.
    // Use HWCap information since reading aarch64 system registers
    // is not supported in user space on older linux kernels.
    ARM64.HasAES = isSet(HWCap, hwcap_AES);
    ARM64.HasPMULL = isSet(HWCap, hwcap_PMULL);
    ARM64.HasSHA1 = isSet(HWCap, hwcap_SHA1);
    ARM64.HasSHA2 = isSet(HWCap, hwcap_SHA2);
    ARM64.HasCRC32 = isSet(HWCap, hwcap_CRC32);
    ARM64.HasCPUID = isSet(HWCap, hwcap_CPUID); 

    // The Samsung S9+ kernel reports support for atomics, but not all cores
    // actually support them, resulting in SIGILL. See issue #28431.
    // TODO(elias.naur): Only disable the optimization on bad chipsets on android.
    ARM64.HasATOMICS = isSet(HWCap, hwcap_ATOMICS) && os != "android"; 

    // Check to see if executing on a NeoverseN1 and in order to do that,
    // check the AUXV for the CPUID bit. The getMIDR function executes an
    // instruction which would normally be an illegal instruction, but it's
    // trapped by the kernel, the value sanitized and then returned. Without
    // the CPUID bit the kernel will not trap the instruction and the process
    // will be terminated with SIGILL.
    if (ARM64.HasCPUID) {
        var midr = getMIDR();
        var part_num = uint16((midr >> 4) & 0xfff);
        var implementor = byte((midr >> 24) & 0xff);

        if (implementor == 'A' && part_num == 0xd0c) {
            ARM64.IsNeoverseN1 = true;
        }
        if (implementor == 'A' && part_num == 0xd40) {
            ARM64.IsZeus = true;
        }
    }
}

private static bool isSet(nuint hwc, nuint value) {
    return hwc & value != 0;
}

} // end cpu_package
