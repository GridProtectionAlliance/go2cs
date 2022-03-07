// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains CRC32 algorithms that are not specific to any architecture
// and don't use hardware acceleration.
//
// The simple (and slow) CRC32 implementation only uses a 256*4 bytes table.
//
// The slicing-by-8 algorithm is a faster implementation that uses a bigger
// table (8*256*4 bytes).

// package crc32 -- go2cs converted at 2022 March 06 22:14:54 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Program Files\Go\src\hash\crc32\crc32_generic.go


namespace go.hash;

public static partial class crc32_package {

    // simpleMakeTable allocates and constructs a Table for the specified
    // polynomial. The table is suitable for use with the simple algorithm
    // (simpleUpdate).
private static ptr<Table> simpleMakeTable(uint poly) {
    ptr<Table> t = @new<Table>();
    simplePopulateTable(poly, t);
    return _addr_t!;
}

// simplePopulateTable constructs a Table for the specified polynomial, suitable
// for use with simpleUpdate.
private static void simplePopulateTable(uint poly, ptr<Table> _addr_t) {
    ref Table t = ref _addr_t.val;

    for (nint i = 0; i < 256; i++) {
        var crc = uint32(i);
        for (nint j = 0; j < 8; j++) {
            if (crc & 1 == 1) {
                crc = (crc >> 1) ^ poly;
            }
            else
 {
                crc>>=1;
            }

        }
        t[i] = crc;

    }

}

// simpleUpdate uses the simple algorithm to update the CRC, given a table that
// was previously computed using simpleMakeTable.
private static uint simpleUpdate(uint crc, ptr<Table> _addr_tab, slice<byte> p) {
    ref Table tab = ref _addr_tab.val;

    crc = ~crc;
    foreach (var (_, v) in p) {
        crc = tab[byte(crc) ^ v] ^ (crc >> 8);
    }    return ~crc;
}

// Use slicing-by-8 when payload >= this value.
private static readonly nint slicing8Cutoff = 16;

// slicing8Table is array of 8 Tables, used by the slicing-by-8 algorithm.


// slicing8Table is array of 8 Tables, used by the slicing-by-8 algorithm.
private partial struct slicing8Table { // : array<Table>
}

// slicingMakeTable constructs a slicing8Table for the specified polynomial. The
// table is suitable for use with the slicing-by-8 algorithm (slicingUpdate).
private static ptr<slicing8Table> slicingMakeTable(uint poly) {
    ptr<slicing8Table> t = @new<slicing8Table>();
    simplePopulateTable(poly, _addr_t[0]);
    for (nint i = 0; i < 256; i++) {
        var crc = t[0][i];
        for (nint j = 1; j < 8; j++) {
            crc = t[0][crc & 0xFF] ^ (crc >> 8);
            t[j][i] = crc;
        }
    }
    return _addr_t!;
}

// slicingUpdate uses the slicing-by-8 algorithm to update the CRC, given a
// table that was previously computed using slicingMakeTable.
private static uint slicingUpdate(uint crc, ptr<slicing8Table> _addr_tab, slice<byte> p) {
    ref slicing8Table tab = ref _addr_tab.val;

    if (len(p) >= slicing8Cutoff) {
        crc = ~crc;
        while (len(p) > 8) {
            crc ^= uint32(p[0]) | uint32(p[1]) << 8 | uint32(p[2]) << 16 | uint32(p[3]) << 24;
            crc = tab[0][p[7]] ^ tab[1][p[6]] ^ tab[2][p[5]] ^ tab[3][p[4]] ^ tab[4][crc >> 24] ^ tab[5][(crc >> 16) & 0xFF] ^ tab[6][(crc >> 8) & 0xFF] ^ tab[7][crc & 0xFF];
            p = p[(int)8..];
        }
        crc = ~crc;
    }
    if (len(p) == 0) {
        return crc;
    }
    return simpleUpdate(crc, _addr_tab[0], p);

}

} // end crc32_package
