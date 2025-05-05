// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crc64 implements the 64-bit cyclic redundancy check, or CRC-64,
// checksum. See https://en.wikipedia.org/wiki/Cyclic_redundancy_check for
// information.
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using sync = sync_package;
using @internal;

partial class crc64_package {

// The size of a CRC-64 checksum in bytes.
public static readonly UntypedInt ΔSize = 8;

// Predefined polynomials.
public static readonly GoUntyped ISO = /* 0xD800000000000000 */
    GoUntyped.Parse("15564440312192434176");

public static readonly GoUntyped ECMA = /* 0xC96C5795D7870F42 */
    GoUntyped.Parse("14514072000185962306");

[GoType("[256]uint64")] partial struct Table;

internal static sync.Once slicing8TablesBuildOnce;
internal static ж<array<Table>> slicing8TableISO;
internal static ж<array<Table>> slicing8TableECMA;

internal static void buildSlicing8TablesOnce() {
    slicing8TablesBuildOnce.Do(buildSlicing8Tables);
}

internal static void buildSlicing8Tables() {
    slicing8TableISO = makeSlicingBy8Table(makeTable(ISO));
    slicing8TableECMA = makeSlicingBy8Table(makeTable(ECMA));
}

// MakeTable returns a [Table] constructed from the specified polynomial.
// The contents of this [Table] must not be modified.
public static ж<Table> MakeTable(uint64 poly) {
    buildSlicing8TablesOnce();
    switch (poly) {
    case ISO: {
        return Ꮡ(slicing8TableISO[0]);
    }
    case ECMA: {
        return Ꮡ(slicing8TableECMA[0]);
    }
    default: {
        return makeTable(poly);
    }}

}

internal static ж<Table> makeTable(uint64 poly) {
    var t = @new<Table>();
    for (nint i = 0; i < 256; i++) {
        var crc = ((uint64)i);
        for (nint j = 0; j < 8; j++) {
            if ((uint64)(crc & 1) == 1){
                crc = (uint64)((crc >> (int)(1)) ^ poly);
            } else {
                crc >>= (UntypedInt)(1);
            }
        }
        t[i] = crc;
    }
    return t;
}

internal static ж<array<Table>> makeSlicingBy8Table(ж<Table> Ꮡt) {
    ref var t = ref Ꮡt.val;

    ref var helperTable = ref heap(new array<Table>(8), out var ᏑhelperTable);
    helperTable[0] = t;
    for (nint i = 0; i < 256; i++) {
        var crc = t[i];
        for (nint j = 1; j < 8; j++) {
            crc = (uint64)(t[(uint64)(crc & 255)] ^ (crc >> (int)(8)));
            helperTable[j][i] = crc;
        }
    }
    return ᏑhelperTable;
}

// digest represents the partial evaluation of a checksum.
[GoType] partial struct digest {
    internal uint64 crc;
    internal ж<Table> tab;
}

// New creates a new hash.Hash64 computing the CRC-64 checksum using the
// polynomial represented by the [Table]. Its Sum method will lay the
// value out in big-endian byte order. The returned Hash64 also
// implements [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash64 New(ж<Table> Ꮡtab) {
    ref var tab = ref Ꮡtab.val;

    return new digest(0, Ꮡtab);
}

[GoRecv] internal static nint Size(this ref digest d) {
    return ΔSize;
}

[GoRecv] internal static nint BlockSize(this ref digest d) {
    return 1;
}

[GoRecv] internal static void Reset(this ref digest d) {
    d.crc = 0;
}

internal static readonly @string magic = "crc\x02"u8;
internal const nint marshaledSize = /* len(magic) + 8 + 8 */ 20;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    var b = new slice<byte>(0, marshaledSize);
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BeAppendUint64(b, tableSum(d.tab));
    b = byteorder.BeAppendUint64(b, d.crc);
    return (b, default!);
}

[GoRecv] internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((@string)(b[..(int)(len(magic))])) != magic) {
        return errors.New("hash/crc64: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize) {
        return errors.New("hash/crc64: invalid hash state size"u8);
    }
    if (tableSum(d.tab) != byteorder.BeUint64(b[4..])) {
        return errors.New("hash/crc64: tables do not match"u8);
    }
    d.crc = byteorder.BeUint64(b[12..]);
    return default!;
}

internal static uint64 update(uint64 crc, ж<Table> Ꮡtab, slice<byte> p) {
    ref var tab = ref Ꮡtab.val;

    buildSlicing8TablesOnce();
    crc = ^crc;
    // Table comparison is somewhat expensive, so avoid it for small sizes
    while (len(p) >= 64) {
        ж<array<Table>> helperTable = default!;
        if (tab == slicing8TableECMA[0]){
            helperTable = slicing8TableECMA;
        } else 
        if (tab == slicing8TableISO[0]){
            helperTable = slicing8TableISO;
        } else 
        if (len(p) >= 2048){
            // For smaller sizes creating extended table takes too much time
            // According to the tests between various x86 and arm CPUs, 2k is a reasonable
            // threshold for now. This may change in the future.
            helperTable = makeSlicingBy8Table(Ꮡtab);
        } else {
            break;
        }
        // Update using slicing-by-8
        while (len(p) > 8) {
            crc ^= (uint64)(byteorder.LeUint64(p));
            crc = (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(helperTable[7][(uint64)(crc & 255)] ^ helperTable[6][(uint64)((crc >> (int)(8)) & 255)]) ^ helperTable[5][(uint64)((crc >> (int)(16)) & 255)]) ^ helperTable[4][(uint64)((crc >> (int)(24)) & 255)]) ^ helperTable[3][(uint64)((crc >> (int)(32)) & 255)]) ^ helperTable[2][(uint64)((crc >> (int)(40)) & 255)]) ^ helperTable[1][(uint64)((crc >> (int)(48)) & 255)]) ^ helperTable[0][crc >> (int)(56)]);
            p = p[8..];
        }
    }
    // For reminders or small sizes
    foreach (var (_, v) in p) {
        crc = (uint64)(tab[(byte)(((byte)crc) ^ v)] ^ (crc >> (int)(8)));
    }
    return ^crc;
}

// Update returns the result of adding the bytes in p to the crc.
public static uint64 Update(uint64 crc, ж<Table> Ꮡtab, slice<byte> p) {
    ref var tab = ref Ꮡtab.val;

    return update(crc, Ꮡtab, p);
}

[GoRecv] internal static (nint n, error err) Write(this ref digest d, slice<byte> p) {
    nint n = default!;
    error err = default!;

    d.crc = update(d.crc, d.tab, p);
    return (len(p), default!);
}

[GoRecv] internal static uint64 Sum64(this ref digest d) {
    return d.crc;
}

[GoRecv] internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    var s = d.Sum64();
    return append(@in, ((byte)(s >> (int)(56))), ((byte)(s >> (int)(48))), ((byte)(s >> (int)(40))), ((byte)(s >> (int)(32))), ((byte)(s >> (int)(24))), ((byte)(s >> (int)(16))), ((byte)(s >> (int)(8))), ((byte)s));
}

// Checksum returns the CRC-64 checksum of data
// using the polynomial represented by the [Table].
public static uint64 Checksum(slice<byte> data, ж<Table> Ꮡtab) {
    ref var tab = ref Ꮡtab.val;

    return update(0, Ꮡtab, data);
}

// tableSum returns the ISO checksum of table t.
internal static uint64 tableSum(ж<Table> Ꮡt) {
    ref var t = ref Ꮡt.val;

    array<byte> a = new(2048);
    var b = a[..0];
    if (t != nil) {
        /* for _, x := range t {
	b = byteorder.BeAppendUint64(b, x)
} */
    }
    return Checksum(b, MakeTable(ISO));
}

} // end crc64_package
