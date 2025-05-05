// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha256 implements the SHA224 and SHA256 hash algorithms as defined
// in FIPS 180-4.
namespace go.crypto;

using crypto = crypto_package;
using boring = crypto.@internal.boring_package;
using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using @internal;
using crypto.@internal;

partial class sha256_package {

[GoInit] internal static void init() {
    crypto.RegisterHash(crypto.SHA224, New224);
    crypto.RegisterHash(crypto.SHA256, New);
}

// The size of a SHA256 checksum in bytes.
public static readonly UntypedInt ΔSize = 32;

// The size of a SHA224 checksum in bytes.
public static readonly UntypedInt Size224 = 28;

// The blocksize of SHA256 and SHA224 in bytes.
public static readonly UntypedInt ΔBlockSize = 64;

internal static readonly UntypedInt chunk = 64;
internal static readonly UntypedInt init0 = /* 0x6A09E667 */ 1779033703;
internal static readonly UntypedInt init1 = /* 0xBB67AE85 */ 3144134277;
internal static readonly UntypedInt init2 = /* 0x3C6EF372 */ 1013904242;
internal static readonly UntypedInt init3 = /* 0xA54FF53A */ 2773480762;
internal static readonly UntypedInt init4 = /* 0x510E527F */ 1359893119;
internal static readonly UntypedInt init5 = /* 0x9B05688C */ 2600822924;
internal static readonly UntypedInt init6 = /* 0x1F83D9AB */ 528734635;
internal static readonly UntypedInt init7 = /* 0x5BE0CD19 */ 1541459225;
internal static readonly UntypedInt init0_224 = /* 0xC1059ED8 */ 3238371032;
internal static readonly UntypedInt init1_224 = /* 0x367CD507 */ 914150663;
internal static readonly UntypedInt init2_224 = /* 0x3070DD17 */ 812702999;
internal static readonly UntypedInt init3_224 = /* 0xF70E5939 */ 4144912697;
internal static readonly UntypedInt init4_224 = /* 0xFFC00B31 */ 4290775857;
internal static readonly UntypedInt init5_224 = /* 0x68581511 */ 1750603025;
internal static readonly UntypedInt init6_224 = /* 0x64F98FA7 */ 1694076839;
internal static readonly UntypedInt init7_224 = /* 0xBEFA4FA4 */ 3204075428;

// digest represents the partial evaluation of a checksum.
[GoType] partial struct digest {
    internal array<uint32> h = new(8);
    internal array<byte> x = new(chunk);
    internal nint nx;
    internal uint64 len;
    internal bool is224; // mark if this digest is SHA-224
}

internal static readonly @string magic224 = "sha\x02"u8;
internal static readonly @string magic256 = "sha\x03"u8;
internal const nint marshaledSize = /* len(magic256) + 8*4 + chunk + 8 */ 108;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    var b = new slice<byte>(0, marshaledSize);
    if (d.is224){
        b = append(b, magic224.ꓸꓸꓸ);
    } else {
        b = append(b, magic256.ꓸꓸꓸ);
    }
    b = byteorder.BeAppendUint32(b, d.h[0]);
    b = byteorder.BeAppendUint32(b, d.h[1]);
    b = byteorder.BeAppendUint32(b, d.h[2]);
    b = byteorder.BeAppendUint32(b, d.h[3]);
    b = byteorder.BeAppendUint32(b, d.h[4]);
    b = byteorder.BeAppendUint32(b, d.h[5]);
    b = byteorder.BeAppendUint32(b, d.h[6]);
    b = byteorder.BeAppendUint32(b, d.h[7]);
    b = append(b, d.x[..(int)(d.nx)].ꓸꓸꓸ);
    b = b[..(int)(len(b) + len(d.x) - d.nx)];
    // already zero
    b = byteorder.BeAppendUint64(b, d.len);
    return (b, default!);
}

[GoRecv] internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic224) || (d.is224 && ((@string)(b[..(int)(len(magic224))])) != magic224) || (!d.is224 && ((@string)(b[..(int)(len(magic256))])) != magic256)) {
        return errors.New("crypto/sha256: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize) {
        return errors.New("crypto/sha256: invalid hash state size"u8);
    }
    b = b[(int)(len(magic224))..];
    (b, d.h[0]) = consumeUint32(b);
    (b, d.h[1]) = consumeUint32(b);
    (b, d.h[2]) = consumeUint32(b);
    (b, d.h[3]) = consumeUint32(b);
    (b, d.h[4]) = consumeUint32(b);
    (b, d.h[5]) = consumeUint32(b);
    (b, d.h[6]) = consumeUint32(b);
    (b, d.h[7]) = consumeUint32(b);
    b = b[(int)(copy(d.x[..], b))..];
    (b, d.len) = consumeUint64(b);
    d.nx = ((nint)(d.len % chunk));
    return default!;
}

internal static (slice<byte>, uint64) consumeUint64(slice<byte> b) {
    return (b[8..], byteorder.BeUint64(b));
}

internal static (slice<byte>, uint32) consumeUint32(slice<byte> b) {
    return (b[4..], byteorder.BeUint32(b));
}

[GoRecv] internal static void Reset(this ref digest d) {
    if (!d.is224){
        d.h[0] = init0;
        d.h[1] = init1;
        d.h[2] = init2;
        d.h[3] = init3;
        d.h[4] = init4;
        d.h[5] = init5;
        d.h[6] = init6;
        d.h[7] = init7;
    } else {
        d.h[0] = init0_224;
        d.h[1] = init1_224;
        d.h[2] = init2_224;
        d.h[3] = init3_224;
        d.h[4] = init4_224;
        d.h[5] = init5_224;
        d.h[6] = init6_224;
        d.h[7] = init7_224;
    }
    d.nx = 0;
    d.len = 0;
}

// New returns a new hash.Hash computing the SHA256 checksum. The Hash
// also implements [encoding.BinaryMarshaler] and
// [encoding.BinaryUnmarshaler] to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash New() {
    if (boring.Enabled) {
        return boring.NewSHA256();
    }
    var d = @new<digest>();
    d.Reset();
    return ~d;
}

// New224 returns a new hash.Hash computing the SHA224 checksum.
public static hash.Hash New224() {
    if (boring.Enabled) {
        return boring.NewSHA224();
    }
    var d = @new<digest>();
    d.val.is224 = true;
    d.Reset();
    return ~d;
}

[GoRecv] internal static nint Size(this ref digest d) {
    if (!d.is224) {
        return ΔSize;
    }
    return Size224;
}

[GoRecv] internal static nint BlockSize(this ref digest d) {
    return ΔBlockSize;
}

[GoRecv] internal static (nint nn, error err) Write(this ref digest d, slice<byte> p) {
    nint nn = default!;
    error err = default!;

    boring.Unreachable();
    nn = len(p);
    d.len += ((uint64)nn);
    if (d.nx > 0) {
        nint n = copy(d.x[(int)(d.nx)..], p);
        d.nx += n;
        if (d.nx == chunk) {
            block(d, d.x[..]);
            d.nx = 0;
        }
        p = p[(int)(n)..];
    }
    if (len(p) >= chunk) {
        nint n = (nint)(len(p) & ~(chunk - 1));
        block(d, p[..(int)(n)]);
        p = p[(int)(n)..];
    }
    if (len(p) > 0) {
        d.nx = copy(d.x[..], p);
    }
    return (nn, err);
}

[GoRecv] internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    boring.Unreachable();
    // Make a copy of d so that caller can keep writing and summing.
    var d0 = d;
    var hash = d0.checkSum();
    if (d0.is224) {
        return append(@in, hash[..(int)(Size224)].ꓸꓸꓸ);
    }
    return append(@in, hash[..].ꓸꓸꓸ);
}

[GoRecv] internal static array<byte> checkSum(this ref digest d) {
    var len = d.len;
    // Padding. Add a 1 bit and 0 bits until 56 bytes mod 64.
    array<byte> tmp = new(72); /* 64 + 8 */                   // padding + length buffer
    tmp[0] = 128;
    uint64 t = default!;
    if (len % 64 < 56){
        t = 56 - len % 64;
    } else {
        t = 64 + 56 - len % 64;
    }
    // Length in bits.
    len <<= (UntypedInt)(3);
    var padlen = tmp[..(int)(t + 8)];
    byteorder.BePutUint64(padlen[(int)(t + 0)..], len);
    d.Write(padlen);
    if (d.nx != 0) {
        throw panic("d.nx != 0");
    }
    array<byte> digest = new(32); /* ΔSize */
    byteorder.BePutUint32(digest[0..], d.h[0]);
    byteorder.BePutUint32(digest[4..], d.h[1]);
    byteorder.BePutUint32(digest[8..], d.h[2]);
    byteorder.BePutUint32(digest[12..], d.h[3]);
    byteorder.BePutUint32(digest[16..], d.h[4]);
    byteorder.BePutUint32(digest[20..], d.h[5]);
    byteorder.BePutUint32(digest[24..], d.h[6]);
    if (!d.is224) {
        byteorder.BePutUint32(digest[28..], d.h[7]);
    }
    return digest;
}

// Sum256 returns the SHA256 checksum of the data.
public static array<byte> Sum256(slice<byte> data) {
    if (boring.Enabled) {
        return boring.SHA256(data);
    }
    digest d = default!;
    d.Reset();
    d.Write(data);
    return d.checkSum();
}

// Sum224 returns the SHA224 checksum of the data.
public static array<byte> Sum224(slice<byte> data) {
    if (boring.Enabled) {
        return boring.SHA224(data);
    }
    digest d = default!;
    d.is224 = true;
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    var ap = (ж<array<byte>>)(sum[..]);
    return ap.val;
}

} // end sha256_package
