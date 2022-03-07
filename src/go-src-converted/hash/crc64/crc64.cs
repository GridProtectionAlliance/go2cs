// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crc64 implements the 64-bit cyclic redundancy check, or CRC-64,
// checksum. See https://en.wikipedia.org/wiki/Cyclic_redundancy_check for
// information.
// package crc64 -- go2cs converted at 2022 March 06 22:14:55 UTC
// import "hash/crc64" ==> using crc64 = go.hash.crc64_package
// Original source: C:\Program Files\Go\src\hash\crc64\crc64.go
using errors = go.errors_package;
using hash = go.hash_package;
using sync = go.sync_package;

namespace go.hash;

public static partial class crc64_package {

    // The size of a CRC-64 checksum in bytes.
public static readonly nint Size = 8;

// Predefined polynomials.


// Predefined polynomials.
 
// The ISO polynomial, defined in ISO 3309 and used in HDLC.
public static readonly nuint ISO = 0xD800000000000000; 

// The ECMA polynomial, defined in ECMA 182.
public static readonly nuint ECMA = 0xC96C5795D7870F42;


// Table is a 256-word table representing the polynomial for efficient processing.
public partial struct Table { // : array<ulong>
}

private static sync.Once slicing8TablesBuildOnce = default;private static ptr<array<Table>> slicing8TableISO;private static ptr<array<Table>> slicing8TableECMA;

private static void buildSlicing8TablesOnce() {
    slicing8TablesBuildOnce.Do(buildSlicing8Tables);
}

private static void buildSlicing8Tables() {
    slicing8TableISO = makeSlicingBy8Table(_addr_makeTable(ISO));
    slicing8TableECMA = makeSlicingBy8Table(_addr_makeTable(ECMA));
}

// MakeTable returns a Table constructed from the specified polynomial.
// The contents of this Table must not be modified.
public static ptr<Table> MakeTable(ulong poly) {
    buildSlicing8TablesOnce();

    if (poly == ISO) 
        return _addr__addr_slicing8TableISO[0]!;
    else if (poly == ECMA) 
        return _addr__addr_slicing8TableECMA[0]!;
    else 
        return _addr_makeTable(poly)!;
    
}

private static ptr<Table> makeTable(ulong poly) {
    ptr<Table> t = @new<Table>();
    for (nint i = 0; i < 256; i++) {
        var crc = uint64(i);
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
    return _addr_t!;

}

private static ptr<array<Table>> makeSlicingBy8Table(ptr<Table> _addr_t) {
    ref Table t = ref _addr_t.val;

    ref array<Table> helperTable = ref heap(new array<Table>(8), out ptr<array<Table>> _addr_helperTable);
    helperTable[0] = t;
    for (nint i = 0; i < 256; i++) {
        var crc = t[i];
        for (nint j = 1; j < 8; j++) {
            crc = t[crc & 0xff] ^ (crc >> 8);
            helperTable[j][i] = crc;
        }
    }
    return _addr__addr_helperTable!;
}

// digest represents the partial evaluation of a checksum.
private partial struct digest {
    public ulong crc;
    public ptr<Table> tab;
}

// New creates a new hash.Hash64 computing the CRC-64 checksum using the
// polynomial represented by the Table. Its Sum method will lay the
// value out in big-endian byte order. The returned Hash64 also
// implements encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash64 New(ptr<Table> _addr_tab) {
    ref Table tab = ref _addr_tab.val;

    return addr(new digest(0,tab));
}

private static nint Size(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return Size;
}

private static nint BlockSize(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return 1;
}

private static void Reset(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    d.crc = 0;
}

private static readonly @string magic = "crc\x02";
private static readonly var marshaledSize = len(magic) + 8 + 8;


private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref digest d = ref _addr_d.val;

    var b = make_slice<byte>(0, marshaledSize);
    b = append(b, magic);
    b = appendUint64(b, tableSum(_addr_d.tab));
    b = appendUint64(b, d.crc);
    return (b, error.As(null!)!);
}

private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b) {
    ref digest d = ref _addr_d.val;

    if (len(b) < len(magic) || string(b[..(int)len(magic)]) != magic) {
        return error.As(errors.New("hash/crc64: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize) {
        return error.As(errors.New("hash/crc64: invalid hash state size"))!;
    }
    if (tableSum(_addr_d.tab) != readUint64(b[(int)4..])) {
        return error.As(errors.New("hash/crc64: tables do not match"))!;
    }
    d.crc = readUint64(b[(int)12..]);
    return error.As(null!)!;

}

private static slice<byte> appendUint64(slice<byte> b, ulong x) {
    array<byte> a = new array<byte>(new byte[] { byte(x>>56), byte(x>>48), byte(x>>40), byte(x>>32), byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
    return append(b, a[..]);
}

private static ulong readUint64(slice<byte> b) {
    _ = b[7];
    return uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;
}

private static ulong update(ulong crc, ptr<Table> _addr_tab, slice<byte> p) {
    ref Table tab = ref _addr_tab.val;

    buildSlicing8TablesOnce();
    crc = ~crc; 
    // Table comparison is somewhat expensive, so avoid it for small sizes
    while (len(p) >= 64) {
        ptr<array<Table>> helperTable;
        if (tab == slicing8TableECMA[0].val) {
            helperTable = slicing8TableECMA;
        }
        else if (tab == slicing8TableISO[0].val) {
            helperTable = slicing8TableISO; 
            // For smaller sizes creating extended table takes too much time
        }
        else if (len(p) > 16384) {
            helperTable = makeSlicingBy8Table(_addr_tab);
        }
        else
 {
            break;
        }
        while (len(p) > 8) {
            crc ^= uint64(p[0]) | uint64(p[1]) << 8 | uint64(p[2]) << 16 | uint64(p[3]) << 24 | uint64(p[4]) << 32 | uint64(p[5]) << 40 | uint64(p[6]) << 48 | uint64(p[7]) << 56;
            crc = helperTable[7][crc & 0xff] ^ helperTable[6][(crc >> 8) & 0xff] ^ helperTable[5][(crc >> 16) & 0xff] ^ helperTable[4][(crc >> 24) & 0xff] ^ helperTable[3][(crc >> 32) & 0xff] ^ helperTable[2][(crc >> 40) & 0xff] ^ helperTable[1][(crc >> 48) & 0xff] ^ helperTable[0][crc >> 56];
            p = p[(int)8..];
        }

    } 
    // For reminders or small sizes
    foreach (var (_, v) in p) {
        crc = tab[byte(crc) ^ v] ^ (crc >> 8);
    }    return ~crc;

}

// Update returns the result of adding the bytes in p to the crc.
public static ulong Update(ulong crc, ptr<Table> _addr_tab, slice<byte> p) {
    ref Table tab = ref _addr_tab.val;

    return update(crc, _addr_tab, p);
}

private static (nint, error) Write(this ptr<digest> _addr_d, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref digest d = ref _addr_d.val;

    d.crc = update(d.crc, _addr_d.tab, p);
    return (len(p), error.As(null!)!);
}

private static ulong Sum64(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return d.crc;
}

private static slice<byte> Sum(this ptr<digest> _addr_d, slice<byte> @in) {
    ref digest d = ref _addr_d.val;

    var s = d.Sum64();
    return append(in, byte(s >> 56), byte(s >> 48), byte(s >> 40), byte(s >> 32), byte(s >> 24), byte(s >> 16), byte(s >> 8), byte(s));
}

// Checksum returns the CRC-64 checksum of data
// using the polynomial represented by the Table.
public static ulong Checksum(slice<byte> data, ptr<Table> _addr_tab) {
    ref Table tab = ref _addr_tab.val;

    return update(0, _addr_tab, data);
}

// tableSum returns the ISO checksum of table t.
private static ulong tableSum(ptr<Table> _addr_t) {
    ref Table t = ref _addr_t.val;

    array<byte> a = new array<byte>(2048);
    var b = a[..(int)0];
    if (t != null) {
        foreach (var (_, x) in t) {
            b = appendUint64(b, x);
        }
    }
    return Checksum(b, _addr_MakeTable(ISO));

}

} // end crc64_package
