// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package crc32 -- go2cs converted at 2022 March 06 22:14:54 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Program Files\Go\src\hash\crc32\crc32_ppc64le.go
using @unsafe = go.@unsafe_package;

namespace go.hash;

public static partial class crc32_package {

private static readonly nint vecMinLen = 16;
private static readonly nint vecAlignMask = 15; // align to 16 bytes
private static readonly nint crcIEEE = 1;
private static readonly nint crcCast = 2;


//go:noescape
private static uint ppc64SlicingUpdateBy8(uint crc, ptr<slicing8Table> table8, slice<byte> p);

// this function requires the buffer to be 16 byte aligned and > 16 bytes long
//go:noescape
private static uint vectorCrc32(uint crc, uint poly, slice<byte> p);

private static ptr<slicing8Table> archCastagnoliTable8;

private static void archInitCastagnoli() {
    archCastagnoliTable8 = slicingMakeTable(Castagnoli);
}

private static uint archUpdateCastagnoli(uint crc, slice<byte> p) {
    if (len(p) >= 4 * vecMinLen) {>>MARKER:FUNCTION_vectorCrc32_BLOCK_PREFIX<< 
        // If not aligned then process the initial unaligned bytes

        if (uint64(uintptr(@unsafe.Pointer(_addr_p[0]))) & uint64(vecAlignMask) != 0) {>>MARKER:FUNCTION_ppc64SlicingUpdateBy8_BLOCK_PREFIX<<
            var align = uint64(uintptr(@unsafe.Pointer(_addr_p[0]))) & uint64(vecAlignMask);
            var newlen = vecMinLen - align;
            crc = ppc64SlicingUpdateBy8(crc, _addr_archCastagnoliTable8, p[..(int)newlen]);
            p = p[(int)newlen..];
        }
        var aligned = len(p) & ~vecAlignMask;
        crc = vectorCrc32(crc, crcCast, p[..(int)aligned]);
        p = p[(int)aligned..];

    }
    if (len(p) == 0) {
        return crc;
    }
    return ppc64SlicingUpdateBy8(crc, _addr_archCastagnoliTable8, p);

}

private static bool archAvailableIEEE() {
    return true;
}
private static bool archAvailableCastagnoli() {
    return true;
}

private static ptr<slicing8Table> archIeeeTable8;

private static void archInitIEEE() { 
    // We still use slicing-by-8 for small buffers.
    archIeeeTable8 = slicingMakeTable(IEEE);

}

// archUpdateIEEE calculates the checksum of p using vectorizedIEEE.
private static uint archUpdateIEEE(uint crc, slice<byte> p) {
    // Check if vector code should be used.  If not aligned, then handle those
    // first up to the aligned bytes.

    if (len(p) >= 4 * vecMinLen) {
        if (uint64(uintptr(@unsafe.Pointer(_addr_p[0]))) & uint64(vecAlignMask) != 0) {
            var align = uint64(uintptr(@unsafe.Pointer(_addr_p[0]))) & uint64(vecAlignMask);
            var newlen = vecMinLen - align;
            crc = ppc64SlicingUpdateBy8(crc, _addr_archIeeeTable8, p[..(int)newlen]);
            p = p[(int)newlen..];
        }
        var aligned = len(p) & ~vecAlignMask;
        crc = vectorCrc32(crc, crcIEEE, p[..(int)aligned]);
        p = p[(int)aligned..];

    }
    if (len(p) == 0) {
        return crc;
    }
    return ppc64SlicingUpdateBy8(crc, _addr_archIeeeTable8, p);

}

} // end crc32_package
