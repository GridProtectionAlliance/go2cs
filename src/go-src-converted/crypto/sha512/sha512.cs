// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha512 implements the SHA-384, SHA-512, SHA-512/224, and SHA-512/256
// hash algorithms as defined in FIPS 180-4.
//
// All the hash.Hash implementations returned by this package also
// implement encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.

// package sha512 -- go2cs converted at 2022 March 13 05:30:37 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Program Files\Go\src\crypto\sha512\sha512.go
namespace go.crypto;

using crypto = crypto_package;
using binary = encoding.binary_package;
using errors = errors_package;
using hash = hash_package;

public static partial class sha512_package {

private static void init() {
    crypto.RegisterHash(crypto.SHA384, New384);
    crypto.RegisterHash(crypto.SHA512, New);
    crypto.RegisterHash(crypto.SHA512_224, New512_224);
    crypto.RegisterHash(crypto.SHA512_256, New512_256);
}

 
// Size is the size, in bytes, of a SHA-512 checksum.
public static readonly nint Size = 64; 

// Size224 is the size, in bytes, of a SHA-512/224 checksum.
public static readonly nint Size224 = 28; 

// Size256 is the size, in bytes, of a SHA-512/256 checksum.
public static readonly nint Size256 = 32; 

// Size384 is the size, in bytes, of a SHA-384 checksum.
public static readonly nint Size384 = 48; 

// BlockSize is the block size, in bytes, of the SHA-512/224,
// SHA-512/256, SHA-384 and SHA-512 hash functions.
public static readonly nint BlockSize = 128;

private static readonly nint chunk = 128;
private static readonly nuint init0 = 0x6a09e667f3bcc908;
private static readonly nuint init1 = 0xbb67ae8584caa73b;
private static readonly nuint init2 = 0x3c6ef372fe94f82b;
private static readonly nuint init3 = 0xa54ff53a5f1d36f1;
private static readonly nuint init4 = 0x510e527fade682d1;
private static readonly nuint init5 = 0x9b05688c2b3e6c1f;
private static readonly nuint init6 = 0x1f83d9abfb41bd6b;
private static readonly nuint init7 = 0x5be0cd19137e2179;
private static readonly nuint init0_224 = 0x8c3d37c819544da2;
private static readonly nuint init1_224 = 0x73e1996689dcd4d6;
private static readonly nuint init2_224 = 0x1dfab7ae32ff9c82;
private static readonly nuint init3_224 = 0x679dd514582f9fcf;
private static readonly nuint init4_224 = 0x0f6d2b697bd44da8;
private static readonly nuint init5_224 = 0x77e36f7304c48942;
private static readonly nuint init6_224 = 0x3f9d85a86a1d36c8;
private static readonly nuint init7_224 = 0x1112e6ad91d692a1;
private static readonly nuint init0_256 = 0x22312194fc2bf72c;
private static readonly nuint init1_256 = 0x9f555fa3c84c64c2;
private static readonly nuint init2_256 = 0x2393b86b6f53b151;
private static readonly nuint init3_256 = 0x963877195940eabd;
private static readonly nuint init4_256 = 0x96283ee2a88effe3;
private static readonly nuint init5_256 = 0xbe5e1e2553863992;
private static readonly nuint init6_256 = 0x2b0199fc2c85b8aa;
private static readonly nuint init7_256 = 0x0eb72ddc81c52ca2;
private static readonly nuint init0_384 = 0xcbbb9d5dc1059ed8;
private static readonly nuint init1_384 = 0x629a292a367cd507;
private static readonly nuint init2_384 = 0x9159015a3070dd17;
private static readonly nuint init3_384 = 0x152fecd8f70e5939;
private static readonly nuint init4_384 = 0x67332667ffc00b31;
private static readonly nuint init5_384 = 0x8eb44a8768581511;
private static readonly nuint init6_384 = 0xdb0c2e0d64f98fa7;
private static readonly nuint init7_384 = 0x47b5481dbefa4fa4;

// digest represents the partial evaluation of a checksum.
private partial struct digest {
    public array<ulong> h;
    public array<byte> x;
    public nint nx;
    public ulong len;
    public crypto.Hash function;
}

private static void Reset(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;


    if (d.function == crypto.SHA384) 
        d.h[0] = init0_384;
        d.h[1] = init1_384;
        d.h[2] = init2_384;
        d.h[3] = init3_384;
        d.h[4] = init4_384;
        d.h[5] = init5_384;
        d.h[6] = init6_384;
        d.h[7] = init7_384;
    else if (d.function == crypto.SHA512_224) 
        d.h[0] = init0_224;
        d.h[1] = init1_224;
        d.h[2] = init2_224;
        d.h[3] = init3_224;
        d.h[4] = init4_224;
        d.h[5] = init5_224;
        d.h[6] = init6_224;
        d.h[7] = init7_224;
    else if (d.function == crypto.SHA512_256) 
        d.h[0] = init0_256;
        d.h[1] = init1_256;
        d.h[2] = init2_256;
        d.h[3] = init3_256;
        d.h[4] = init4_256;
        d.h[5] = init5_256;
        d.h[6] = init6_256;
        d.h[7] = init7_256;
    else 
        d.h[0] = init0;
        d.h[1] = init1;
        d.h[2] = init2;
        d.h[3] = init3;
        d.h[4] = init4;
        d.h[5] = init5;
        d.h[6] = init6;
        d.h[7] = init7;
        d.nx = 0;
    d.len = 0;
}

private static readonly @string magic384 = "sha\x04";
private static readonly @string magic512_224 = "sha\x05";
private static readonly @string magic512_256 = "sha\x06";
private static readonly @string magic512 = "sha\x07";
private static readonly var marshaledSize = len(magic512) + 8 * 8 + chunk + 8;

private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref digest d = ref _addr_d.val;

    var b = make_slice<byte>(0, marshaledSize);

    if (d.function == crypto.SHA384) 
        b = append(b, magic384);
    else if (d.function == crypto.SHA512_224) 
        b = append(b, magic512_224);
    else if (d.function == crypto.SHA512_256) 
        b = append(b, magic512_256);
    else if (d.function == crypto.SHA512) 
        b = append(b, magic512);
    else 
        return (null, error.As(errors.New("crypto/sha512: invalid hash function"))!);
        b = appendUint64(b, d.h[0]);
    b = appendUint64(b, d.h[1]);
    b = appendUint64(b, d.h[2]);
    b = appendUint64(b, d.h[3]);
    b = appendUint64(b, d.h[4]);
    b = appendUint64(b, d.h[5]);
    b = appendUint64(b, d.h[6]);
    b = appendUint64(b, d.h[7]);
    b = append(b, d.x[..(int)d.nx]);
    b = b[..(int)len(b) + len(d.x) - int(d.nx)]; // already zero
    b = appendUint64(b, d.len);
    return (b, error.As(null!)!);
}

private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b) {
    ref digest d = ref _addr_d.val;

    if (len(b) < len(magic512)) {
        return error.As(errors.New("crypto/sha512: invalid hash state identifier"))!;
    }

    if (d.function == crypto.SHA384 && string(b[..(int)len(magic384)]) == magic384)     else if (d.function == crypto.SHA512_224 && string(b[..(int)len(magic512_224)]) == magic512_224)     else if (d.function == crypto.SHA512_256 && string(b[..(int)len(magic512_256)]) == magic512_256)     else if (d.function == crypto.SHA512 && string(b[..(int)len(magic512)]) == magic512)     else 
        return error.As(errors.New("crypto/sha512: invalid hash state identifier"))!;
        if (len(b) != marshaledSize) {
        return error.As(errors.New("crypto/sha512: invalid hash state size"))!;
    }
    b = b[(int)len(magic512)..];
    b, d.h[0] = consumeUint64(b);
    b, d.h[1] = consumeUint64(b);
    b, d.h[2] = consumeUint64(b);
    b, d.h[3] = consumeUint64(b);
    b, d.h[4] = consumeUint64(b);
    b, d.h[5] = consumeUint64(b);
    b, d.h[6] = consumeUint64(b);
    b, d.h[7] = consumeUint64(b);
    b = b[(int)copy(d.x[..], b)..];
    b, d.len = consumeUint64(b);
    d.nx = int(d.len % chunk);
    return error.As(null!)!;
}

private static slice<byte> appendUint64(slice<byte> b, ulong x) {
    array<byte> a = new array<byte>(8);
    binary.BigEndian.PutUint64(a[..], x);
    return append(b, a[..]);
}

private static (slice<byte>, ulong) consumeUint64(slice<byte> b) {
    slice<byte> _p0 = default;
    ulong _p0 = default;

    _ = b[7];
    var x = uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;
    return (b[(int)8..], x);
}

// New returns a new hash.Hash computing the SHA-512 checksum.
public static hash.Hash New() {
    ptr<digest> d = addr(new digest(function:crypto.SHA512));
    d.Reset();
    return d;
}

// New512_224 returns a new hash.Hash computing the SHA-512/224 checksum.
public static hash.Hash New512_224() {
    ptr<digest> d = addr(new digest(function:crypto.SHA512_224));
    d.Reset();
    return d;
}

// New512_256 returns a new hash.Hash computing the SHA-512/256 checksum.
public static hash.Hash New512_256() {
    ptr<digest> d = addr(new digest(function:crypto.SHA512_256));
    d.Reset();
    return d;
}

// New384 returns a new hash.Hash computing the SHA-384 checksum.
public static hash.Hash New384() {
    ptr<digest> d = addr(new digest(function:crypto.SHA384));
    d.Reset();
    return d;
}

private static nint Size(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;


    if (d.function == crypto.SHA512_224) 
        return Size224;
    else if (d.function == crypto.SHA512_256) 
        return Size256;
    else if (d.function == crypto.SHA384) 
        return Size384;
    else 
        return Size;
    }

private static nint BlockSize(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    return BlockSize;
}

private static (nint, error) Write(this ptr<digest> _addr_d, slice<byte> p) {
    nint nn = default;
    error err = default!;
    ref digest d = ref _addr_d.val;

    nn = len(p);
    d.len += uint64(nn);
    if (d.nx > 0) {
        var n = copy(d.x[(int)d.nx..], p);
        d.nx += n;
        if (d.nx == chunk) {
            block(d, d.x[..]);
            d.nx = 0;
        }
        p = p[(int)n..];
    }
    if (len(p) >= chunk) {
        n = len(p) & ~(chunk - 1);
        block(d, p[..(int)n]);
        p = p[(int)n..];
    }
    if (len(p) > 0) {
        d.nx = copy(d.x[..], p);
    }
    return ;
}

private static slice<byte> Sum(this ptr<digest> _addr_d, slice<byte> @in) {
    ref digest d = ref _addr_d.val;
 
    // Make a copy of d so that caller can keep writing and summing.
    ptr<digest> d0 = @new<digest>();
    d0.val = d.val;
    var hash = d0.checkSum();

    if (d0.function == crypto.SHA384) 
        return append(in, hash[..(int)Size384]);
    else if (d0.function == crypto.SHA512_224) 
        return append(in, hash[..(int)Size224]);
    else if (d0.function == crypto.SHA512_256) 
        return append(in, hash[..(int)Size256]);
    else 
        return append(in, hash[..]);
    }

private static array<byte> checkSum(this ptr<digest> _addr_d) => func((_, panic, _) => {
    ref digest d = ref _addr_d.val;
 
    // Padding. Add a 1 bit and 0 bits until 112 bytes mod 128.
    var len = d.len;
    array<byte> tmp = new array<byte>(128);
    tmp[0] = 0x80;
    if (len % 128 < 112) {
        d.Write(tmp[(int)0..(int)112 - len % 128]);
    }
    else
 {
        d.Write(tmp[(int)0..(int)128 + 112 - len % 128]);
    }
    len<<=3;
    binary.BigEndian.PutUint64(tmp[(int)0..], 0); // upper 64 bits are always zero, because len variable has type uint64
    binary.BigEndian.PutUint64(tmp[(int)8..], len);
    d.Write(tmp[(int)0..(int)16]);

    if (d.nx != 0) {
        panic("d.nx != 0");
    }
    array<byte> digest = new array<byte>(Size);
    binary.BigEndian.PutUint64(digest[(int)0..], d.h[0]);
    binary.BigEndian.PutUint64(digest[(int)8..], d.h[1]);
    binary.BigEndian.PutUint64(digest[(int)16..], d.h[2]);
    binary.BigEndian.PutUint64(digest[(int)24..], d.h[3]);
    binary.BigEndian.PutUint64(digest[(int)32..], d.h[4]);
    binary.BigEndian.PutUint64(digest[(int)40..], d.h[5]);
    if (d.function != crypto.SHA384) {
        binary.BigEndian.PutUint64(digest[(int)48..], d.h[6]);
        binary.BigEndian.PutUint64(digest[(int)56..], d.h[7]);
    }
    return digest;
});

// Sum512 returns the SHA512 checksum of the data.
public static array<byte> Sum512(slice<byte> data) {
    digest d = new digest(function:crypto.SHA512);
    d.Reset();
    d.Write(data);
    return d.checkSum();
}

// Sum384 returns the SHA384 checksum of the data.
public static array<byte> Sum384(slice<byte> data) {
    array<byte> sum384 = default;

    digest d = new digest(function:crypto.SHA384);
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    copy(sum384[..], sum[..(int)Size384]);
    return ;
}

// Sum512_224 returns the Sum512/224 checksum of the data.
public static array<byte> Sum512_224(slice<byte> data) {
    array<byte> sum224 = default;

    digest d = new digest(function:crypto.SHA512_224);
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    copy(sum224[..], sum[..(int)Size224]);
    return ;
}

// Sum512_256 returns the Sum512/256 checksum of the data.
public static array<byte> Sum512_256(slice<byte> data) {
    array<byte> sum256 = default;

    digest d = new digest(function:crypto.SHA512_256);
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    copy(sum256[..], sum[..(int)Size256]);
    return ;
}

} // end sha512_package
