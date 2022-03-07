// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_arm.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static readonly nint cacheLineSize = 32;

// HWCAP/HWCAP2 bits.
// These are specific to Linux.


// HWCAP/HWCAP2 bits.
// These are specific to Linux.
private static readonly nint hwcap_SWP = 1 << 0;
private static readonly nint hwcap_HALF = 1 << 1;
private static readonly nint hwcap_THUMB = 1 << 2;
private static readonly nint hwcap_26BIT = 1 << 3;
private static readonly nint hwcap_FAST_MULT = 1 << 4;
private static readonly nint hwcap_FPA = 1 << 5;
private static readonly nint hwcap_VFP = 1 << 6;
private static readonly nint hwcap_EDSP = 1 << 7;
private static readonly nint hwcap_JAVA = 1 << 8;
private static readonly nint hwcap_IWMMXT = 1 << 9;
private static readonly nint hwcap_CRUNCH = 1 << 10;
private static readonly nint hwcap_THUMBEE = 1 << 11;
private static readonly nint hwcap_NEON = 1 << 12;
private static readonly nint hwcap_VFPv3 = 1 << 13;
private static readonly nint hwcap_VFPv3D16 = 1 << 14;
private static readonly nint hwcap_TLS = 1 << 15;
private static readonly nint hwcap_VFPv4 = 1 << 16;
private static readonly nint hwcap_IDIVA = 1 << 17;
private static readonly nint hwcap_IDIVT = 1 << 18;
private static readonly nint hwcap_VFPD32 = 1 << 19;
private static readonly nint hwcap_LPAE = 1 << 20;
private static readonly nint hwcap_EVTSTRM = 1 << 21;

private static readonly nint hwcap2_AES = 1 << 0;
private static readonly nint hwcap2_PMULL = 1 << 1;
private static readonly nint hwcap2_SHA1 = 1 << 2;
private static readonly nint hwcap2_SHA2 = 1 << 3;
private static readonly nint hwcap2_CRC32 = 1 << 4;


private static void initOptions() {
    options = new slice<option>(new option[] { {Name:"pmull",Feature:&ARM.HasPMULL}, {Name:"sha1",Feature:&ARM.HasSHA1}, {Name:"sha2",Feature:&ARM.HasSHA2}, {Name:"swp",Feature:&ARM.HasSWP}, {Name:"thumb",Feature:&ARM.HasTHUMB}, {Name:"thumbee",Feature:&ARM.HasTHUMBEE}, {Name:"tls",Feature:&ARM.HasTLS}, {Name:"vfp",Feature:&ARM.HasVFP}, {Name:"vfpd32",Feature:&ARM.HasVFPD32}, {Name:"vfpv3",Feature:&ARM.HasVFPv3}, {Name:"vfpv3d16",Feature:&ARM.HasVFPv3D16}, {Name:"vfpv4",Feature:&ARM.HasVFPv4}, {Name:"half",Feature:&ARM.HasHALF}, {Name:"26bit",Feature:&ARM.Has26BIT}, {Name:"fastmul",Feature:&ARM.HasFASTMUL}, {Name:"fpa",Feature:&ARM.HasFPA}, {Name:"edsp",Feature:&ARM.HasEDSP}, {Name:"java",Feature:&ARM.HasJAVA}, {Name:"iwmmxt",Feature:&ARM.HasIWMMXT}, {Name:"crunch",Feature:&ARM.HasCRUNCH}, {Name:"neon",Feature:&ARM.HasNEON}, {Name:"idivt",Feature:&ARM.HasIDIVT}, {Name:"idiva",Feature:&ARM.HasIDIVA}, {Name:"lpae",Feature:&ARM.HasLPAE}, {Name:"evtstrm",Feature:&ARM.HasEVTSTRM}, {Name:"aes",Feature:&ARM.HasAES}, {Name:"crc32",Feature:&ARM.HasCRC32} });
}

} // end cpu_package
