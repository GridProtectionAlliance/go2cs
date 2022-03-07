// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package adler32 implements the Adler-32 checksum.
//
// It is defined in RFC 1950:
//    Adler-32 is composed of two sums accumulated per byte: s1 is
//    the sum of all bytes, s2 is the sum of all s1 values. Both sums
//    are done modulo 65521. s1 is initialized to 1, s2 to zero.  The
//    Adler-32 checksum is stored as s2*65536 + s1 in most-
//    significant-byte first (network) order.
// package adler32 -- go2cs converted at 2022 March 06 22:14:53 UTC
// import "hash/adler32" ==> using adler32 = go.hash.adler32_package
// Original source: C:\Program Files\Go\src\hash\adler32\adler32.go
using errors = go.errors_package;
using hash = go.hash_package;

namespace go.hash;

public static partial class adler32_package {

 
// mod is the largest prime that is less than 65536.
private static readonly nint mod = 65521; 
// nmax is the largest n such that
// 255 * n * (n+1) / 2 + (n+1) * (mod-1) <= 2^32-1.
// It is mentioned in RFC 1950 (search for "5552").
private static readonly nint nmax = 5552;


// The size of an Adler-32 checksum in bytes.
public static readonly nint Size = 4;

// digest represents the partial evaluation of a checksum.
// The low 16 bits are s1, the high 16 bits are s2.


// digest represents the partial evaluation of a checksum.
// The low 16 bits are s1, the high 16 bits are s2.
private partial struct digest { // : uint
}

private static void Reset(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    d.val = 1;
}

// New returns a new hash.Hash32 computing the Adler-32 checksum. Its
// Sum method will lay the value out in big-endian byte order. The
// returned Hash32 also implements encoding.BinaryMarshaler and
// encoding.BinaryUnmarshaler to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash32 New() {
    ptr<digest> d = @new<digest>();
    d.Reset();
    return d;
}

private static nint Size(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return Size;
}

private static nint BlockSize(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return 4;
}

private static readonly @string magic = "adl\x01";
private static readonly var marshaledSize = len(magic) + 4;


private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref digest d = ref _addr_d.val;

    var b = make_slice<byte>(0, marshaledSize);
    b = append(b, magic);
    b = appendUint32(b, uint32(d.val));
    return (b, error.As(null!)!);
}

private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b) {
    ref digest d = ref _addr_d.val;

    if (len(b) < len(magic) || string(b[..(int)len(magic)]) != magic) {
        return error.As(errors.New("hash/adler32: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize) {
        return error.As(errors.New("hash/adler32: invalid hash state size"))!;
    }
    d.val = digest(readUint32(b[(int)len(magic)..]));
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

// Add p to the running checksum d.
private static digest update(digest d, slice<byte> p) {
    var s1 = uint32(d & 0xffff);
    var s2 = uint32(d >> 16);
    while (len(p) > 0) {
        slice<byte> q = default;
        if (len(p) > nmax) {
            (p, q) = (p[..(int)nmax], p[(int)nmax..]);
        }
        while (len(p) >= 4) {
            s1 += uint32(p[0]);
            s2 += s1;
            s1 += uint32(p[1]);
            s2 += s1;
            s1 += uint32(p[2]);
            s2 += s1;
            s1 += uint32(p[3]);
            s2 += s1;
            p = p[(int)4..];
        }
        foreach (var (_, x) in p) {
            s1 += uint32(x);
            s2 += s1;
        }        s1 %= mod;
        s2 %= mod;
        p = q;

    }
    return digest(s2 << 16 | s1);

}

private static (nint, error) Write(this ptr<digest> _addr_d, slice<byte> p) {
    nint nn = default;
    error err = default!;
    ref digest d = ref _addr_d.val;

    d.val = update(d.val, p);
    return (len(p), error.As(null!)!);
}

private static uint Sum32(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return uint32(d.val);
}

private static slice<byte> Sum(this ptr<digest> _addr_d, slice<byte> @in) {
    ref digest d = ref _addr_d.val;

    var s = uint32(d.val);
    return append(in, byte(s >> 24), byte(s >> 16), byte(s >> 8), byte(s));
}

// Checksum returns the Adler-32 checksum of data.
public static uint Checksum(slice<byte> data) {
    return uint32(update(1, data));
}

} // end adler32_package
