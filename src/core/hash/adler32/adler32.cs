// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package adler32 implements the Adler-32 checksum.
//
// It is defined in RFC 1950:
//
//	Adler-32 is composed of two sums accumulated per byte: s1 is
//	the sum of all bytes, s2 is the sum of all s1 values. Both sums
//	are done modulo 65521. s1 is initialized to 1, s2 to zero.  The
//	Adler-32 checksum is stored as s2*65536 + s1 in most-
//	significant-byte first (network) order.
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using @internal;

partial class adler32_package {

internal static readonly UntypedInt mod = 65521;
internal static readonly UntypedInt nmax = 5552;

// The size of an Adler-32 checksum in bytes.
public static readonly UntypedInt ΔSize = 4;

[GoType("num:uint32")] partial struct digest;

[GoRecv] internal static void Reset(this ref digest d) {
    d = 1;
}

// New returns a new hash.Hash32 computing the Adler-32 checksum. Its
// Sum method will lay the value out in big-endian byte order. The
// returned Hash32 also implements [encoding.BinaryMarshaler] and
// [encoding.BinaryUnmarshaler] to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash32 New() {
    var d = @new<digest>();
    d.Reset();
    return ~d;
}

[GoRecv] internal static nint Size(this ref digest d) {
    return ΔSize;
}

[GoRecv] internal static nint BlockSize(this ref digest d) {
    return 4;
}

internal static readonly @string magic = "adl\x01"u8;
internal const nint marshaledSize = /* len(magic) + 4 */ 8;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    var b = new slice<byte>(0, marshaledSize);
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BeAppendUint32(b, ((uint32)(d)));
    return (b, default!);
}

[GoRecv] internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((@string)(b[..(int)(len(magic))])) != magic) {
        return errors.New("hash/adler32: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize) {
        return errors.New("hash/adler32: invalid hash state size"u8);
    }
    d = ((digest)byteorder.BeUint32(b[(int)(len(magic))..]));
    return default!;
}

// Add p to the running checksum d.
internal static digest update(digest d, slice<byte> p) {
    var (s1, s2) = (((uint32)((digest)(d & 65535))), ((uint32)(d >> (int)(16))));
    while (len(p) > 0) {
        slice<byte> q = default!;
        if (len(p) > nmax) {
            (p, q) = (p[..(int)(nmax)], p[(int)(nmax)..]);
        }
        while (len(p) >= 4) {
            s1 += ((uint32)p[0]);
            s2 += s1;
            s1 += ((uint32)p[1]);
            s2 += s1;
            s1 += ((uint32)p[2]);
            s2 += s1;
            s1 += ((uint32)p[3]);
            s2 += s1;
            p = p[4..];
        }
        foreach (var (_, x) in p) {
            s1 += ((uint32)x);
            s2 += s1;
        }
        s1 %= mod;
        s2 %= mod;
        p = q;
    }
    return ((digest)((uint32)(s2 << (int)(16) | s1)));
}

[GoRecv] internal static (nint nn, error err) Write(this ref digest d, slice<byte> p) {
    nint nn = default!;
    error err = default!;

    d = update(d, p);
    return (len(p), default!);
}

[GoRecv] internal static uint32 Sum32(this ref digest d) {
    return ((uint32)(d));
}

[GoRecv] internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    var s = ((uint32)(d));
    return append(@in, ((byte)(s >> (int)(24))), ((byte)(s >> (int)(16))), ((byte)(s >> (int)(8))), ((byte)s));
}

// Checksum returns the Adler-32 checksum of data.
public static uint32 Checksum(slice<byte> data) {
    return ((uint32)update(1, data));
}

} // end adler32_package
