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

// package crc32 -- go2cs converted at 2022 March 13 05:28:56 UTC
// import "hash/crc32" ==> using crc32 = go.hash.crc32_package
// Original source: C:\Program Files\Go\src\hash\crc32\crc32.go
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using sync = sync_package;
using atomic = sync.atomic_package;


// The size of a CRC-32 checksum in bytes.

using System;
public static partial class crc32_package {

public static readonly nint Size = 4;

// Predefined polynomials.


// Predefined polynomials.
 
// IEEE is by far and away the most common CRC-32 polynomial.
// Used by ethernet (IEEE 802.3), v.42, fddi, gzip, zip, png, ...
public static readonly nuint IEEE = 0xedb88320; 

// Castagnoli's polynomial, used in iSCSI.
// Has better error detection characteristics than IEEE.
// https://dx.doi.org/10.1109/26.231911
public static readonly nuint Castagnoli = 0x82f63b78; 

// Koopman's polynomial.
// Also has better error detection characteristics than IEEE.
// https://dx.doi.org/10.1109/DSN.2002.1028931
public static readonly nuint Koopman = 0xeb31d82e;

// Table is a 256-word table representing the polynomial for efficient processing.
public partial struct Table { // : array<uint>
}

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
private static ptr<Table> castagnoliTable;
private static ptr<slicing8Table> castagnoliTable8;
private static bool castagnoliArchImpl = default;
private static Func<uint, slice<byte>, uint> updateCastagnoli = default;
private static sync.Once castagnoliOnce = default;
private static uint haveCastagnoli = default;

private static void castagnoliInit() {
    castagnoliTable = simpleMakeTable(Castagnoli);
    castagnoliArchImpl = archAvailableCastagnoli();

    if (castagnoliArchImpl) {
        archInitCastagnoli();
        updateCastagnoli = archUpdateCastagnoli;
    }
    else
 { 
        // Initialize the slicing-by-8 table.
        castagnoliTable8 = slicingMakeTable(Castagnoli);
        updateCastagnoli = (crc, p) => slicingUpdate(crc, castagnoliTable8, p);
    }
    atomic.StoreUint32(_addr_haveCastagnoli, 1);
}

// IEEETable is the table for the IEEE polynomial.
public static var IEEETable = simpleMakeTable(IEEE);

// ieeeTable8 is the slicing8Table for IEEE
private static ptr<slicing8Table> ieeeTable8;
private static bool ieeeArchImpl = default;
private static Func<uint, slice<byte>, uint> updateIEEE = default;
private static sync.Once ieeeOnce = default;

private static void ieeeInit() {
    ieeeArchImpl = archAvailableIEEE();

    if (ieeeArchImpl) {
        archInitIEEE();
        updateIEEE = archUpdateIEEE;
    }
    else
 { 
        // Initialize the slicing-by-8 table.
        ieeeTable8 = slicingMakeTable(IEEE);
        updateIEEE = (crc, p) => slicingUpdate(crc, ieeeTable8, p);
    }
}

// MakeTable returns a Table constructed from the specified polynomial.
// The contents of this Table must not be modified.
public static ptr<Table> MakeTable(uint poly) {

    if (poly == IEEE) 
        ieeeOnce.Do(ieeeInit);
        return _addr_IEEETable!;
    else if (poly == Castagnoli) 
        castagnoliOnce.Do(castagnoliInit);
        return _addr_castagnoliTable!;
        return _addr_simpleMakeTable(poly)!;
}

// digest represents the partial evaluation of a checksum.
private partial struct digest {
    public uint crc;
    public ptr<Table> tab;
}

// New creates a new hash.Hash32 computing the CRC-32 checksum using the
// polynomial represented by the Table. Its Sum method will lay the
// value out in big-endian byte order. The returned Hash32 also
// implements encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash32 New(ptr<Table> _addr_tab) {
    ref Table tab = ref _addr_tab.val;

    if (tab == IEEETable) {
        ieeeOnce.Do(ieeeInit);
    }
    return addr(new digest(0,tab));
}

// NewIEEE creates a new hash.Hash32 computing the CRC-32 checksum using
// the IEEE polynomial. Its Sum method will lay the value out in
// big-endian byte order. The returned Hash32 also implements
// encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to marshal
// and unmarshal the internal state of the hash.
public static hash.Hash32 NewIEEE() {
    return New(_addr_IEEETable);
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

private static readonly @string magic = "crc\x01";
private static readonly var marshaledSize = len(magic) + 4 + 4;

private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref digest d = ref _addr_d.val;

    var b = make_slice<byte>(0, marshaledSize);
    b = append(b, magic);
    b = appendUint32(b, tableSum(_addr_d.tab));
    b = appendUint32(b, d.crc);
    return (b, error.As(null!)!);
}

private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b) {
    ref digest d = ref _addr_d.val;

    if (len(b) < len(magic) || string(b[..(int)len(magic)]) != magic) {
        return error.As(errors.New("hash/crc32: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize) {
        return error.As(errors.New("hash/crc32: invalid hash state size"))!;
    }
    if (tableSum(_addr_d.tab) != readUint32(b[(int)4..])) {
        return error.As(errors.New("hash/crc32: tables do not match"))!;
    }
    d.crc = readUint32(b[(int)8..]);
    return error.As(null!)!;
}

private static slice<byte> appendUint32(slice<byte> b, uint x) {
    array<byte> a = new array<byte>(new byte[] { byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
    return append(b, a[..]);
}

private static uint readUint32(slice<byte> b) {
    _ = b[3];
    return uint32(b[3]) | uint32(b[2]) << 8 | uint32(b[1]) << 16 | uint32(b[0]) << 24;
}

// Update returns the result of adding the bytes in p to the crc.
public static uint Update(uint crc, ptr<Table> _addr_tab, slice<byte> p) {
    ref Table tab = ref _addr_tab.val;


    if (atomic.LoadUint32(_addr_haveCastagnoli) != 0 && tab == castagnoliTable) 
        return updateCastagnoli(crc, p);
    else if (tab == IEEETable) 
        // Unfortunately, because IEEETable is exported, IEEE may be used without a
        // call to MakeTable. We have to make sure it gets initialized in that case.
        ieeeOnce.Do(ieeeInit);
        return updateIEEE(crc, p);
    else 
        return simpleUpdate(crc, tab, p);
    }

private static (nint, error) Write(this ptr<digest> _addr_d, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref digest d = ref _addr_d.val;


    if (atomic.LoadUint32(_addr_haveCastagnoli) != 0 && d.tab == castagnoliTable) 
        d.crc = updateCastagnoli(d.crc, p);
    else if (d.tab == IEEETable) 
        // We only create digest objects through New() which takes care of
        // initialization in this case.
        d.crc = updateIEEE(d.crc, p);
    else 
        d.crc = simpleUpdate(d.crc, d.tab, p);
        return (len(p), error.As(null!)!);
}

private static uint Sum32(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return d.crc;
}

private static slice<byte> Sum(this ptr<digest> _addr_d, slice<byte> @in) {
    ref digest d = ref _addr_d.val;

    var s = d.Sum32();
    return append(in, byte(s >> 24), byte(s >> 16), byte(s >> 8), byte(s));
}

// Checksum returns the CRC-32 checksum of data
// using the polynomial represented by the Table.
public static uint Checksum(slice<byte> data, ptr<Table> _addr_tab) {
    ref Table tab = ref _addr_tab.val;

    return Update(0, _addr_tab, data);
}

// ChecksumIEEE returns the CRC-32 checksum of data
// using the IEEE polynomial.
public static uint ChecksumIEEE(slice<byte> data) {
    ieeeOnce.Do(ieeeInit);
    return updateIEEE(0, data);
}

// tableSum returns the IEEE checksum of table t.
private static uint tableSum(ptr<Table> _addr_t) {
    ref Table t = ref _addr_t.val;

    array<byte> a = new array<byte>(1024);
    var b = a[..(int)0];
    if (t != null) {
        foreach (var (_, x) in t) {
            b = appendUint32(b, x);
        }
    }
    return ChecksumIEEE(b);
}

} // end crc32_package
