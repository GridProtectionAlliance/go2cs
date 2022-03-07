// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm64
// +build arm64

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_arm64_freebsd.go


namespace go.@internal;

public static partial class cpu_package {

private static void osInit() { 
    // Retrieve info from system register ID_AA64ISAR0_EL1.
    var isar0 = getisar0(); 

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

        case 2: 
            ARM64.HasSHA2 = true;
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

}

private static nuint extractBits(ulong data, nuint start, nuint end) {
    return (uint)(data >> (int)(start)) & ((1 << (int)((end - start + 1))) - 1);
}

} // end cpu_package
