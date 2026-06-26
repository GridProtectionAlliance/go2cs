// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crc32 implements the 32-bit cyclic redundancy check, or CRC-32,
// checksum. See https://en.wikipedia.org/wiki/Cyclic_redundancy_check for
// information.
//
// Polynomials are represented in LSB-first form also known as reversed representation.
//
// See https://en.wikipedia.org/wiki/Mathematics_of_cyclic_redundancy_checks#Reversed_representations_and_reciprocal_polynomials
// for information.
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using @internal;
using sync;

partial class crc32_package {

// The size of a CRC-32 checksum in bytes.
public static readonly UntypedInt ΔSize = 4;

// Predefined polynomials.
public static readonly UntypedInt IEEE = /* 0xedb88320 */ 3988292384;

public static readonly UntypedInt Castagnoli = /* 0x82f63b78 */ 2197175160;

public static readonly UntypedInt Koopman = /* 0xeb31d82e */ 3945912366;

[GoType("[256]uint32")] partial struct Table;

// This file makes use of functions implemented in architecture-specific files.
// The interface that they implement is as follows:
//
//    // archAvailableIEEE reports whether an architecture-specific CRC32-IEEE
//    // algorithm is available.
//    archAvailableIEEE() bool
//
//    // archInitIEEE initializes the architecture-specific CRC3-IEEE algorithm.
//    // It can only be called if archAvailableIEEE() returns true.
//    archInitIEEE()
//
//    // archUpdateIEEE updates the given CRC32-IEEE. It can only be called if
//    // archInitIEEE() was previously called.
//    archUpdateIEEE(crc uint32, p []byte) uint32
//
//    // archAvailableCastagnoli reports whether an architecture-specific
//    // CRC32-C algorithm is available.
//    archAvailableCastagnoli() bool
//
//    // archInitCastagnoli initializes the architecture-specific CRC32-C
//    // algorithm. It can only be called if archAvailableCastagnoli() returns
//    // true.
//    archInitCastagnoli()
//
//    // archUpdateCastagnoli updates the given CRC32-C. It can only be called
//    // if archInitCastagnoli() was previously called.
//    archUpdateCastagnoli(crc uint32, p []byte) uint32

// castagnoliTable points to a lazily initialized Table for the Castagnoli
// polynomial. MakeTable will always return this value when asked to make a
// Castagnoli table so we can compare against it to find when the caller is
// using this polynomial.
internal static ж<Table> castagnoliTable;

internal static ж<slicing8Table> castagnoliTable8;

internal static Func<uint32, slice<byte>, uint32> updateCastagnoli;

internal static sync.Once castagnoliOnce;

internal static atomic.Bool haveCastagnoli;

internal static void castagnoliInit() {
    castagnoliTable = simpleMakeTable(Castagnoli);
    if (archAvailableCastagnoli()){
        archInitCastagnoli();
        updateCastagnoli = archUpdateCastagnoli;
    } else {
        // Initialize the slicing-by-8 table.
        castagnoliTable8 = slicingMakeTable(Castagnoli);
        updateCastagnoli = (uint32 crc, slice<byte> p) => slicingUpdate(crc, castagnoliTable8, p);
    }
    haveCastagnoli.Store(true);
}

// IEEETable is the table for the [IEEE] polynomial.
public static ж<Table> IEEETable = simpleMakeTable(IEEE);

// ieeeTable8 is the slicing8Table for IEEE
internal static ж<slicing8Table> ieeeTable8;

internal static Func<uint32, slice<byte>, uint32> updateIEEE;

internal static sync.Once ieeeOnce;

internal static void ieeeInit() {
    if (archAvailableIEEE()){
        archInitIEEE();
        updateIEEE = archUpdateIEEE;
    } else {
        // Initialize the slicing-by-8 table.
        ieeeTable8 = slicingMakeTable(IEEE);
        updateIEEE = (uint32 crc, slice<byte> p) => slicingUpdate(crc, ieeeTable8, p);
    }
}

// MakeTable returns a [Table] constructed from the specified polynomial.
// The contents of this [Table] must not be modified.
public static ж<Table> MakeTable(uint32 poly) {
    var exprᴛ1 = poly;
    if (exprᴛ1 == IEEE) {
        ieeeOnce.Do(ieeeInit);
        return IEEETable;
    }
    if (exprᴛ1 == Castagnoli) {
        castagnoliOnce.Do(castagnoliInit);
        return castagnoliTable;
    }
    { /* default: */
        return simpleMakeTable(poly);
    }

}

// digest represents the partial evaluation of a checksum.
[GoType] partial struct digest {
    internal uint32 crc;
    internal ж<Table> tab;
}

// New creates a new [hash.Hash32] computing the CRC-32 checksum using the
// polynomial represented by the [Table]. Its Sum method will lay the
// value out in big-endian byte order. The returned Hash32 also
// implements [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash32 New(ж<Table> Ꮡtab) {
    ref var tab = ref Ꮡtab.val;

    if (Ꮡtab == IEEETable) {
        ieeeOnce.Do(ieeeInit);
    }
    return new digest(0, Ꮡtab);
}

// NewIEEE creates a new [hash.Hash32] computing the CRC-32 checksum using
// the [IEEE] polynomial. Its Sum method will lay the value out in
// big-endian byte order. The returned Hash32 also implements
// [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to marshal
// and unmarshal the internal state of the hash.
public static hash.Hash32 NewIEEE() {
    return New(IEEETable);
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

internal static readonly @string magic = "crc\x01"u8;
internal const nint marshaledSize = /* len(magic) + 4 + 4 */ 12;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    var b = new slice<byte>(0, marshaledSize);
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BeAppendUint32(b, tableSum(d.tab));
    b = byteorder.BeAppendUint32(b, d.crc);
    return (b, default!);
}

[GoRecv] internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((@string)(b[..(int)(len(magic))])) != magic) {
        return errors.New("hash/crc32: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize) {
        return errors.New("hash/crc32: invalid hash state size"u8);
    }
    if (tableSum(d.tab) != byteorder.BeUint32(b[4..])) {
        return errors.New("hash/crc32: tables do not match"u8);
    }
    d.crc = byteorder.BeUint32(b[8..]);
    return default!;
}

internal static uint32 update(uint32 crc, ж<Table> Ꮡtab, slice<byte> p, bool checkInitIEEE) {
    ref var tab = ref Ꮡtab.val;

    switch (ᐧ) {
    case {} when haveCastagnoli.Load() && Ꮡtab == castagnoliTable: {
        return updateCastagnoli(crc, p);
    }
    case {} when Ꮡtab is IEEETable: {
        if (checkInitIEEE) {
            ieeeOnce.Do(ieeeInit);
        }
        return updateIEEE(crc, p);
    }
    default: {
        return simpleUpdate(crc, Ꮡtab, p);
    }}

}

// Update returns the result of adding the bytes in p to the crc.
public static uint32 Update(uint32 crc, ж<Table> Ꮡtab, slice<byte> p) {
    ref var tab = ref Ꮡtab.val;

    // Unfortunately, because IEEETable is exported, IEEE may be used without a
    // call to MakeTable. We have to make sure it gets initialized in that case.
    return update(crc, Ꮡtab, p, true);
}

[GoRecv] internal static (nint n, error err) Write(this ref digest d, slice<byte> p) {
    nint n = default!;
    error err = default!;

    // We only create digest objects through New() which takes care of
    // initialization in this case.
    d.crc = update(d.crc, d.tab, p, false);
    return (len(p), default!);
}

[GoRecv] internal static uint32 Sum32(this ref digest d) {
    return d.crc;
}

[GoRecv] internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    var s = d.Sum32();
    return append(@in, ((byte)(s >> (int)(24))), ((byte)(s >> (int)(16))), ((byte)(s >> (int)(8))), ((byte)s));
}

// Checksum returns the CRC-32 checksum of data
// using the polynomial represented by the [Table].
public static uint32 Checksum(slice<byte> data, ж<Table> Ꮡtab) {
    ref var tab = ref Ꮡtab.val;

    return Update(0, Ꮡtab, data);
}

// ChecksumIEEE returns the CRC-32 checksum of data
// using the [IEEE] polynomial.
public static uint32 ChecksumIEEE(slice<byte> data) {
    ieeeOnce.Do(ieeeInit);
    return updateIEEE(0, data);
}

// tableSum returns the IEEE checksum of table t.
internal static uint32 tableSum(ж<Table> Ꮡt) {
    ref var t = ref Ꮡt.val;

    array<byte> a = new(1024);
    var b = a[..0];
    if (t != nil) {
        foreach (var (_, x) in t.val) {
            b = byteorder.BeAppendUint32(b, x);
        }
    }
    return ChecksumIEEE(b);
}

} // end crc32_package
