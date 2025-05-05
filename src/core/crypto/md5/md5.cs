// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go run gen.go -output md5block.go

// Package md5 implements the MD5 hash algorithm as defined in RFC 1321.
//
// MD5 is cryptographically broken and should not be used for secure
// applications.
namespace go.crypto;

using crypto = crypto_package;
using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using @internal;

partial class md5_package {

[GoInit] internal static void init() {
    crypto.RegisterHash(crypto.MD5, New);
}

// The size of an MD5 checksum in bytes.
public static readonly UntypedInt ΔSize = 16;

// The blocksize of MD5 in bytes.
public static readonly UntypedInt ΔBlockSize = 64;

internal static readonly UntypedInt init0 = /* 0x67452301 */ 1732584193;
internal static readonly UntypedInt init1 = /* 0xEFCDAB89 */ 4023233417;
internal static readonly UntypedInt init2 = /* 0x98BADCFE */ 2562383102;
internal static readonly UntypedInt init3 = /* 0x10325476 */ 271733878;

// digest represents the partial evaluation of a checksum.
[GoType] partial struct digest {
    internal array<uint32> s = new(4);
    internal array<byte> x = new(ΔBlockSize);
    internal nint nx;
    internal uint64 len;
}

[GoRecv] internal static void Reset(this ref digest d) {
    d.s[0] = init0;
    d.s[1] = init1;
    d.s[2] = init2;
    d.s[3] = init3;
    d.nx = 0;
    d.len = 0;
}

internal static readonly @string magic = "md5\x01"u8;
internal const nint marshaledSize = /* len(magic) + 4*4 + BlockSize + 8 */ 92;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    var b = new slice<byte>(0, marshaledSize);
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BeAppendUint32(b, d.s[0]);
    b = byteorder.BeAppendUint32(b, d.s[1]);
    b = byteorder.BeAppendUint32(b, d.s[2]);
    b = byteorder.BeAppendUint32(b, d.s[3]);
    b = append(b, d.x[..(int)(d.nx)].ꓸꓸꓸ);
    b = b[..(int)(len(b) + len(d.x) - d.nx)];
    // already zero
    b = byteorder.BeAppendUint64(b, d.len);
    return (b, default!);
}

[GoRecv] internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((@string)(b[..(int)(len(magic))])) != magic) {
        return errors.New("crypto/md5: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize) {
        return errors.New("crypto/md5: invalid hash state size"u8);
    }
    b = b[(int)(len(magic))..];
    (b, d.s[0]) = consumeUint32(b);
    (b, d.s[1]) = consumeUint32(b);
    (b, d.s[2]) = consumeUint32(b);
    (b, d.s[3]) = consumeUint32(b);
    b = b[(int)(copy(d.x[..], b))..];
    (b, d.len) = consumeUint64(b);
    d.nx = ((nint)(d.len % ΔBlockSize));
    return default!;
}

internal static (slice<byte>, uint64) consumeUint64(slice<byte> b) {
    return (b[8..], byteorder.BeUint64(b[0..8]));
}

internal static (slice<byte>, uint32) consumeUint32(slice<byte> b) {
    return (b[4..], byteorder.BeUint32(b[0..4]));
}

// New returns a new hash.Hash computing the MD5 checksum. The Hash also
// implements [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash New() {
    var d = @new<digest>();
    d.Reset();
    return ~d;
}

[GoRecv] internal static nint Size(this ref digest d) {
    return ΔSize;
}

[GoRecv] internal static nint BlockSize(this ref digest d) {
    return ΔBlockSize;
}

[GoRecv] internal static (nint nn, error err) Write(this ref digest d, slice<byte> p) {
    nint nn = default!;
    error err = default!;

    // Note that we currently call block or blockGeneric
    // directly (guarded using haveAsm) because this allows
    // escape analysis to see that p and d don't escape.
    nn = len(p);
    d.len += ((uint64)nn);
    if (d.nx > 0) {
        nint n = copy(d.x[(int)(d.nx)..], p);
        d.nx += n;
        if (d.nx == ΔBlockSize) {
            if (haveAsm){
                block(d, d.x[..]);
            } else {
                blockGeneric(d, d.x[..]);
            }
            d.nx = 0;
        }
        p = p[(int)(n)..];
    }
    if (len(p) >= ΔBlockSize) {
        nint n = (nint)(len(p) & ~(ΔBlockSize - 1));
        if (haveAsm){
            block(d, p[..(int)(n)]);
        } else {
            blockGeneric(d, p[..(int)(n)]);
        }
        p = p[(int)(n)..];
    }
    if (len(p) > 0) {
        d.nx = copy(d.x[..], p);
    }
    return (nn, err);
}

[GoRecv] internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    // Make a copy of d so that caller can keep writing and summing.
    var d0 = d;
    var hash = d0.checkSum();
    return append(@in, hash[..].ꓸꓸꓸ);
}

[GoRecv] internal static array<byte> checkSum(this ref digest d) {
    // Append 0x80 to the end of the message and then append zeros
    // until the length is a multiple of 56 bytes. Finally append
    // 8 bytes representing the message length in bits.
    //
    // 1 byte end marker :: 0-63 padding bytes :: 8 byte length
    var tmp = new byte[]{128}.array();
    var pad = (55 - d.len) % 64;
    // calculate number of padding bytes
    byteorder.LePutUint64(tmp[(int)(1 + pad)..], d.len << (int)(3));
    // append length in bits
    d.Write(tmp[..(int)(1 + pad + 8)]);
    // The previous write ensures that a whole number of
    // blocks (i.e. a multiple of 64 bytes) have been hashed.
    if (d.nx != 0) {
        throw panic("d.nx != 0");
    }
    array<byte> digest = new(16); /* ΔSize */
    byteorder.LePutUint32(digest[0..], d.s[0]);
    byteorder.LePutUint32(digest[4..], d.s[1]);
    byteorder.LePutUint32(digest[8..], d.s[2]);
    byteorder.LePutUint32(digest[12..], d.s[3]);
    return digest;
}

// Sum returns the MD5 checksum of the data.
public static array<byte> Sum(slice<byte> data) {
    digest d = default!;
    d.Reset();
    d.Write(data);
    return d.checkSum();
}

} // end md5_package
