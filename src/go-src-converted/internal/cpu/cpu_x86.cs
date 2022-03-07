// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386 || amd64
// +build 386 amd64

// package cpu -- go2cs converted at 2022 March 06 22:29:52 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_x86.go


namespace go.@internal;

public static partial class cpu_package {

public static readonly nint CacheLinePadSize = 64;

// cpuid is implemented in cpu_x86.s.


// cpuid is implemented in cpu_x86.s.
private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg);

// xgetbv with ecx = 0 is implemented in cpu_x86.s.
private static (uint, uint) xgetbv();

 
// edx bits
private static readonly nint cpuid_SSE2 = 1 << 26; 

// ecx bits
private static readonly nint cpuid_SSE3 = 1 << 0;
private static readonly nint cpuid_PCLMULQDQ = 1 << 1;
private static readonly nint cpuid_SSSE3 = 1 << 9;
private static readonly nint cpuid_FMA = 1 << 12;
private static readonly nint cpuid_SSE41 = 1 << 19;
private static readonly nint cpuid_SSE42 = 1 << 20;
private static readonly nint cpuid_POPCNT = 1 << 23;
private static readonly nint cpuid_AES = 1 << 25;
private static readonly nint cpuid_OSXSAVE = 1 << 27;
private static readonly nint cpuid_AVX = 1 << 28; 

// ebx bits
private static readonly nint cpuid_BMI1 = 1 << 3;
private static readonly nint cpuid_AVX2 = 1 << 5;
private static readonly nint cpuid_BMI2 = 1 << 8;
private static readonly nint cpuid_ERMS = 1 << 9;
private static readonly nint cpuid_ADX = 1 << 19;


private static uint maxExtendedFunctionInformation = default;

private static void doinit() {
    options = new slice<option>(new option[] { {Name:"adx",Feature:&X86.HasADX}, {Name:"aes",Feature:&X86.HasAES}, {Name:"avx",Feature:&X86.HasAVX}, {Name:"avx2",Feature:&X86.HasAVX2}, {Name:"bmi1",Feature:&X86.HasBMI1}, {Name:"bmi2",Feature:&X86.HasBMI2}, {Name:"erms",Feature:&X86.HasERMS}, {Name:"fma",Feature:&X86.HasFMA}, {Name:"pclmulqdq",Feature:&X86.HasPCLMULQDQ}, {Name:"popcnt",Feature:&X86.HasPOPCNT}, {Name:"sse3",Feature:&X86.HasSSE3}, {Name:"sse41",Feature:&X86.HasSSE41}, {Name:"sse42",Feature:&X86.HasSSE42}, {Name:"ssse3",Feature:&X86.HasSSSE3}, {Name:"sse2",Feature:&X86.HasSSE2,Required:GOARCH=="amd64"} });

    var (maxID, _, _, _) = cpuid(0, 0);

    if (maxID < 1) {>>MARKER:FUNCTION_xgetbv_BLOCK_PREFIX<<
        return ;
    }
    maxExtendedFunctionInformation, _, _, _ = cpuid(0x80000000, 0);

    var (_, _, ecx1, edx1) = cpuid(1, 0);
    X86.HasSSE2 = isSet(edx1, cpuid_SSE2);

    X86.HasSSE3 = isSet(ecx1, cpuid_SSE3);
    X86.HasPCLMULQDQ = isSet(ecx1, cpuid_PCLMULQDQ);
    X86.HasSSSE3 = isSet(ecx1, cpuid_SSSE3);
    X86.HasSSE41 = isSet(ecx1, cpuid_SSE41);
    X86.HasSSE42 = isSet(ecx1, cpuid_SSE42);
    X86.HasPOPCNT = isSet(ecx1, cpuid_POPCNT);
    X86.HasAES = isSet(ecx1, cpuid_AES); 

    // OSXSAVE can be false when using older Operating Systems
    // or when explicitly disabled on newer Operating Systems by
    // e.g. setting the xsavedisable boot option on Windows 10.
    X86.HasOSXSAVE = isSet(ecx1, cpuid_OSXSAVE); 

    // The FMA instruction set extension only has VEX prefixed instructions.
    // VEX prefixed instructions require OSXSAVE to be enabled.
    // See Intel 64 and IA-32 Architecture Software Developer’s Manual Volume 2
    // Section 2.4 "AVX and SSE Instruction Exception Specification"
    X86.HasFMA = isSet(ecx1, cpuid_FMA) && X86.HasOSXSAVE;

    var osSupportsAVX = false; 
    // For XGETBV, OSXSAVE bit is required and sufficient.
    if (X86.HasOSXSAVE) {>>MARKER:FUNCTION_cpuid_BLOCK_PREFIX<<
        var (eax, _) = xgetbv(); 
        // Check if XMM and YMM registers have OS support.
        osSupportsAVX = isSet(eax, 1 << 1) && isSet(eax, 1 << 2);

    }
    X86.HasAVX = isSet(ecx1, cpuid_AVX) && osSupportsAVX;

    if (maxID < 7) {
        return ;
    }
    var (_, ebx7, _, _) = cpuid(7, 0);
    X86.HasBMI1 = isSet(ebx7, cpuid_BMI1);
    X86.HasAVX2 = isSet(ebx7, cpuid_AVX2) && osSupportsAVX;
    X86.HasBMI2 = isSet(ebx7, cpuid_BMI2);
    X86.HasERMS = isSet(ebx7, cpuid_ERMS);
    X86.HasADX = isSet(ebx7, cpuid_ADX);

}

private static bool isSet(uint hwc, uint value) {
    return hwc & value != 0;
}

// Name returns the CPU name given by the vendor.
// If the CPU name can not be determined an
// empty string is returned.
public static @string Name() {
    if (maxExtendedFunctionInformation < 0x80000004) {
        return "";
    }
    var data = make_slice<byte>(0, 3 * 4 * 4);

    uint eax = default;    uint ebx = default;    uint ecx = default;    uint edx = default;

    eax, ebx, ecx, edx = cpuid(0x80000002, 0);
    data = appendBytes(data, eax, ebx, ecx, edx);
    eax, ebx, ecx, edx = cpuid(0x80000003, 0);
    data = appendBytes(data, eax, ebx, ecx, edx);
    eax, ebx, ecx, edx = cpuid(0x80000004, 0);
    data = appendBytes(data, eax, ebx, ecx, edx); 

    // Trim leading spaces.
    while (len(data) > 0 && data[0] == ' ') {
        data = data[(int)1..];
    } 

    // Trim tail after and including the first null byte.
    foreach (var (i, c) in data) {
        if (c == '\x00') {
            data = data[..(int)i];
            break;
        }
    }    return string(data);

}

private static slice<byte> appendBytes(slice<byte> b, params uint[] args) {
    args = args.Clone();

    foreach (var (_, arg) in args) {
        b = append(b, byte((arg >> 0)), byte((arg >> 8)), byte((arg >> 16)), byte((arg >> 24)));
    }    return b;
}

} // end cpu_package
