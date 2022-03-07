// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_arm64.go
using runtime = go.runtime_package;

namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static readonly nint cacheLineSize = 64;



private static void initOptions() {
    options = new slice<option>(new option[] { {Name:"fp",Feature:&ARM64.HasFP}, {Name:"asimd",Feature:&ARM64.HasASIMD}, {Name:"evstrm",Feature:&ARM64.HasEVTSTRM}, {Name:"aes",Feature:&ARM64.HasAES}, {Name:"fphp",Feature:&ARM64.HasFPHP}, {Name:"jscvt",Feature:&ARM64.HasJSCVT}, {Name:"lrcpc",Feature:&ARM64.HasLRCPC}, {Name:"pmull",Feature:&ARM64.HasPMULL}, {Name:"sha1",Feature:&ARM64.HasSHA1}, {Name:"sha2",Feature:&ARM64.HasSHA2}, {Name:"sha3",Feature:&ARM64.HasSHA3}, {Name:"sha512",Feature:&ARM64.HasSHA512}, {Name:"sm3",Feature:&ARM64.HasSM3}, {Name:"sm4",Feature:&ARM64.HasSM4}, {Name:"sve",Feature:&ARM64.HasSVE}, {Name:"crc32",Feature:&ARM64.HasCRC32}, {Name:"atomics",Feature:&ARM64.HasATOMICS}, {Name:"asimdhp",Feature:&ARM64.HasASIMDHP}, {Name:"cpuid",Feature:&ARM64.HasCPUID}, {Name:"asimrdm",Feature:&ARM64.HasASIMDRDM}, {Name:"fcma",Feature:&ARM64.HasFCMA}, {Name:"dcpop",Feature:&ARM64.HasDCPOP}, {Name:"asimddp",Feature:&ARM64.HasASIMDDP}, {Name:"asimdfhm",Feature:&ARM64.HasASIMDFHM} });
}

private static void archInit() {
    switch (runtime.GOOS) {
        case "freebsd": 
            readARM64Registers();
            break;
        case "linux": 

        case "netbsd": 
            doinit();
            break;
        default: 
            // Most platforms don't seem to allow reading these registers.
            //
            // OpenBSD:
            // See https://golang.org/issue/31746
            setMinimalFeatures();
            break;
    }

}

// setMinimalFeatures fakes the minimal ARM64 features expected by
// TestARM64minimalFeatures.
private static void setMinimalFeatures() {
    ARM64.HasASIMD = true;
    ARM64.HasFP = true;
}

private static void readARM64Registers() {
    Initialized = true;

    parseARM64SystemRegisters(getisar0(), getisar1(), getpfr0());
}

private static void parseARM64SystemRegisters(ulong isar0, ulong isar1, ulong pfr0) { 
    // ID_AA64ISAR0_EL1
    switch (extractBits(isar0, 4, 7)) {
        case 1: 
            ARM64.HasAES = true;
            break;
        case 2: 
            ARM64.HasAES = true;
            ARM64.HasPMULL = true;
            break;
    }

    switch (extractBits(isar0, 8, 11)) {
        case 1: 
            ARM64.HasSHA1 = true;
            break;
    }

    switch (extractBits(isar0, 12, 15)) {
        case 1: 
            ARM64.HasSHA2 = true;
            break;
        case 2: 
            ARM64.HasSHA2 = true;
            ARM64.HasSHA512 = true;
            break;
    }

    switch (extractBits(isar0, 16, 19)) {
        case 1: 
            ARM64.HasCRC32 = true;
            break;
    }

    switch (extractBits(isar0, 20, 23)) {
        case 2: 
            ARM64.HasATOMICS = true;
            break;
    }

    switch (extractBits(isar0, 28, 31)) {
        case 1: 
            ARM64.HasASIMDRDM = true;
            break;
    }

    switch (extractBits(isar0, 32, 35)) {
        case 1: 
            ARM64.HasSHA3 = true;
            break;
    }

    switch (extractBits(isar0, 36, 39)) {
        case 1: 
            ARM64.HasSM3 = true;
            break;
    }

    switch (extractBits(isar0, 40, 43)) {
        case 1: 
            ARM64.HasSM4 = true;
            break;
    }

    switch (extractBits(isar0, 44, 47)) {
        case 1: 
            ARM64.HasASIMDDP = true;
            break;
    } 

    // ID_AA64ISAR1_EL1
    switch (extractBits(isar1, 0, 3)) {
        case 1: 
            ARM64.HasDCPOP = true;
            break;
    }

    switch (extractBits(isar1, 12, 15)) {
        case 1: 
            ARM64.HasJSCVT = true;
            break;
    }

    switch (extractBits(isar1, 16, 19)) {
        case 1: 
            ARM64.HasFCMA = true;
            break;
    }

    switch (extractBits(isar1, 20, 23)) {
        case 1: 
            ARM64.HasLRCPC = true;
            break;
    } 

    // ID_AA64PFR0_EL1
    switch (extractBits(pfr0, 16, 19)) {
        case 0: 
            ARM64.HasFP = true;
            break;
        case 1: 
            ARM64.HasFP = true;
            ARM64.HasFPHP = true;
            break;
    }

    switch (extractBits(pfr0, 20, 23)) {
        case 0: 
            ARM64.HasASIMD = true;
            break;
        case 1: 
            ARM64.HasASIMD = true;
            ARM64.HasASIMDHP = true;
            break;
    }

    switch (extractBits(pfr0, 32, 35)) {
        case 1: 
            ARM64.HasSVE = true;
            break;
    }

}

private static nuint extractBits(ulong data, nuint start, nuint end) {
    return (uint)(data >> (int)(start)) & ((1 << (int)((end - start + 1))) - 1);
}

} // end cpu_package
