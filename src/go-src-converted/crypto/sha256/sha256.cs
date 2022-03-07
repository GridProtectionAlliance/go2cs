// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha256 implements the SHA224 and SHA256 hash algorithms as defined
// in FIPS 180-4.
// package sha256 -- go2cs converted at 2022 March 06 22:19:27 UTC
// import "crypto/sha256" ==> using sha256 = go.crypto.sha256_package
// Original source: C:\Program Files\Go\src\crypto\sha256\sha256.go
using crypto = go.crypto_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;

namespace go.crypto;

public static partial class sha256_package {

private static void init() {
    crypto.RegisterHash(crypto.SHA224, New224);
    crypto.RegisterHash(crypto.SHA256, New);
}

// The size of a SHA256 checksum in bytes.
public static readonly nint Size = 32;

// The size of a SHA224 checksum in bytes.


// The size of a SHA224 checksum in bytes.
public static readonly nint Size224 = 28;

// The blocksize of SHA256 and SHA224 in bytes.


// The blocksize of SHA256 and SHA224 in bytes.
public static readonly nint BlockSize = 64;



private static readonly nint chunk = 64;
private static readonly nuint init0 = 0x6A09E667;
private static readonly nuint init1 = 0xBB67AE85;
private static readonly nuint init2 = 0x3C6EF372;
private static readonly nuint init3 = 0xA54FF53A;
private static readonly nuint init4 = 0x510E527F;
private static readonly nuint init5 = 0x9B05688C;
private static readonly nuint init6 = 0x1F83D9AB;
private static readonly nuint init7 = 0x5BE0CD19;
private static readonly nuint init0_224 = 0xC1059ED8;
private static readonly nuint init1_224 = 0x367CD507;
private static readonly nuint init2_224 = 0x3070DD17;
private static readonly nuint init3_224 = 0xF70E5939;
private static readonly nuint init4_224 = 0xFFC00B31;
private static readonly nuint init5_224 = 0x68581511;
private static readonly nuint init6_224 = 0x64F98FA7;
private static readonly nuint init7_224 = 0xBEFA4FA4;


// digest represents the partial evaluation of a checksum.
private partial struct digest {
    public array<uint> h;
    public array<byte> x;
    public nint nx;
    public ulong len;
    public bool is224; // mark if this digest is SHA-224
}

private static readonly @string magic224 = "sha\x02";
private static readonly @string magic256 = "sha\x03";
private static readonly var marshaledSize = len(magic256) + 8 * 4 + chunk + 8;


private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref digest d = ref _addr_d.val;

    var b = make_slice<byte>(0, marshaledSize);
    if (d.is224) {
        b = append(b, magic224);
    }
    else
 {
        b = append(b, magic256);
    }
    b = appendUint32(b, d.h[0]);
    b = appendUint32(b, d.h[1]);
    b = appendUint32(b, d.h[2]);
    b = appendUint32(b, d.h[3]);
    b = appendUint32(b, d.h[4]);
    b = appendUint32(b, d.h[5]);
    b = appendUint32(b, d.h[6]);
    b = appendUint32(b, d.h[7]);
    b = append(b, d.x[..(int)d.nx]);
    b = b[..(int)len(b) + len(d.x) - int(d.nx)]; // already zero
    b = appendUint64(b, d.len);
    return (b, error.As(null!)!);

}

private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b) {
    ref digest d = ref _addr_d.val;

    if (len(b) < len(magic224) || (d.is224 && string(b[..(int)len(magic224)]) != magic224) || (!d.is224 && string(b[..(int)len(magic256)]) != magic256)) {
        return error.As(errors.New("crypto/sha256: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize) {
        return error.As(errors.New("crypto/sha256: invalid hash state size"))!;
    }
    b = b[(int)len(magic224)..];
    b, d.h[0] = consumeUint32(b);
    b, d.h[1] = consumeUint32(b);
    b, d.h[2] = consumeUint32(b);
    b, d.h[3] = consumeUint32(b);
    b, d.h[4] = consumeUint32(b);
    b, d.h[5] = consumeUint32(b);
    b, d.h[6] = consumeUint32(b);
    b, d.h[7] = consumeUint32(b);
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

private static slice<byte> appendUint32(slice<byte> b, uint x) {
    array<byte> a = new array<byte>(4);
    binary.BigEndian.PutUint32(a[..], x);
    return append(b, a[..]);
}

private static (slice<byte>, ulong) consumeUint64(slice<byte> b) {
    slice<byte> _p0 = default;
    ulong _p0 = default;

    _ = b[7];
    var x = uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;
    return (b[(int)8..], x);
}

private static (slice<byte>, uint) consumeUint32(slice<byte> b) {
    slice<byte> _p0 = default;
    uint _p0 = default;

    _ = b[3];
    var x = uint32(b[3]) | uint32(b[2]) << 8 | uint32(b[1]) << 16 | uint32(b[0]) << 24;
    return (b[(int)4..], x);
}

private static void Reset(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    if (!d.is224) {
        d.h[0] = init0;
        d.h[1] = init1;
        d.h[2] = init2;
        d.h[3] = init3;
        d.h[4] = init4;
        d.h[5] = init5;
        d.h[6] = init6;
        d.h[7] = init7;
    }
    else
 {
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
// also implements encoding.BinaryMarshaler and
// encoding.BinaryUnmarshaler to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash New() {
    ptr<digest> d = @new<digest>();
    d.Reset();
    return d;
}

// New224 returns a new hash.Hash computing the SHA224 checksum.
public static hash.Hash New224() {
    ptr<digest> d = @new<digest>();
    d.is224 = true;
    d.Reset();
    return d;
}

private static nint Size(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    if (!d.is224) {
        return Size;
    }
    return Size224;

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
    var d0 = d.val;
    var hash = d0.checkSum();
    if (d0.is224) {
        return append(in, hash[..(int)Size224]);
    }
    return append(in, hash[..]);

}

private static array<byte> checkSum(this ptr<digest> _addr_d) => func((_, panic, _) => {
    ref digest d = ref _addr_d.val;

    var len = d.len; 
    // Padding. Add a 1 bit and 0 bits until 56 bytes mod 64.
    array<byte> tmp = new array<byte>(64);
    tmp[0] = 0x80;
    if (len % 64 < 56) {
        d.Write(tmp[(int)0..(int)56 - len % 64]);
    }
    else
 {
        d.Write(tmp[(int)0..(int)64 + 56 - len % 64]);
    }
    len<<=3;
    binary.BigEndian.PutUint64(tmp[..], len);
    d.Write(tmp[(int)0..(int)8]);

    if (d.nx != 0) {
        panic("d.nx != 0");
    }
    array<byte> digest = new array<byte>(Size);

    binary.BigEndian.PutUint32(digest[(int)0..], d.h[0]);
    binary.BigEndian.PutUint32(digest[(int)4..], d.h[1]);
    binary.BigEndian.PutUint32(digest[(int)8..], d.h[2]);
    binary.BigEndian.PutUint32(digest[(int)12..], d.h[3]);
    binary.BigEndian.PutUint32(digest[(int)16..], d.h[4]);
    binary.BigEndian.PutUint32(digest[(int)20..], d.h[5]);
    binary.BigEndian.PutUint32(digest[(int)24..], d.h[6]);
    if (!d.is224) {
        binary.BigEndian.PutUint32(digest[(int)28..], d.h[7]);
    }
    return digest;

});

// Sum256 returns the SHA256 checksum of the data.
public static array<byte> Sum256(slice<byte> data) {
    digest d = default;
    d.Reset();
    d.Write(data);
    return d.checkSum();
}

// Sum224 returns the SHA224 checksum of the data.
public static array<byte> Sum224(slice<byte> data) {
    array<byte> sum224 = default;

    digest d = default;
    d.is224 = true;
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    copy(sum224[..], sum[..(int)Size224]);
    return ;
}

} // end sha256_package
