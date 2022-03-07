// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run gen.go -output md5block.go

// Package md5 implements the MD5 hash algorithm as defined in RFC 1321.
//
// MD5 is cryptographically broken and should not be used for secure
// applications.
// package md5 -- go2cs converted at 2022 March 06 22:19:22 UTC
// import "crypto/md5" ==> using md5 = go.crypto.md5_package
// Original source: C:\Program Files\Go\src\crypto\md5\md5.go
using crypto = go.crypto_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;

namespace go.crypto;

public static partial class md5_package {

private static void init() {
    crypto.RegisterHash(crypto.MD5, New);
}

// The size of an MD5 checksum in bytes.
public static readonly nint Size = 16;

// The blocksize of MD5 in bytes.


// The blocksize of MD5 in bytes.
public static readonly nint BlockSize = 64;



private static readonly nuint init0 = 0x67452301;
private static readonly nuint init1 = 0xEFCDAB89;
private static readonly nuint init2 = 0x98BADCFE;
private static readonly nuint init3 = 0x10325476;


// digest represents the partial evaluation of a checksum.
private partial struct digest {
    public array<uint> s;
    public array<byte> x;
    public nint nx;
    public ulong len;
}

private static void Reset(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    d.s[0] = init0;
    d.s[1] = init1;
    d.s[2] = init2;
    d.s[3] = init3;
    d.nx = 0;
    d.len = 0;
}

private static readonly @string magic = "md5\x01";
private static readonly var marshaledSize = len(magic) + 4 * 4 + BlockSize + 8;


private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref digest d = ref _addr_d.val;

    var b = make_slice<byte>(0, marshaledSize);
    b = append(b, magic);
    b = appendUint32(b, d.s[0]);
    b = appendUint32(b, d.s[1]);
    b = appendUint32(b, d.s[2]);
    b = appendUint32(b, d.s[3]);
    b = append(b, d.x[..(int)d.nx]);
    b = b[..(int)len(b) + len(d.x) - d.nx]; // already zero
    b = appendUint64(b, d.len);
    return (b, error.As(null!)!);

}

private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b) {
    ref digest d = ref _addr_d.val;

    if (len(b) < len(magic) || string(b[..(int)len(magic)]) != magic) {
        return error.As(errors.New("crypto/md5: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize) {
        return error.As(errors.New("crypto/md5: invalid hash state size"))!;
    }
    b = b[(int)len(magic)..];
    b, d.s[0] = consumeUint32(b);
    b, d.s[1] = consumeUint32(b);
    b, d.s[2] = consumeUint32(b);
    b, d.s[3] = consumeUint32(b);
    b = b[(int)copy(d.x[..], b)..];
    b, d.len = consumeUint64(b);
    d.nx = int(d.len % BlockSize);
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

    return (b[(int)8..], binary.BigEndian.Uint64(b[(int)0..(int)8]));
}

private static (slice<byte>, uint) consumeUint32(slice<byte> b) {
    slice<byte> _p0 = default;
    uint _p0 = default;

    return (b[(int)4..], binary.BigEndian.Uint32(b[(int)0..(int)4]));
}

// New returns a new hash.Hash computing the MD5 checksum. The Hash also
// implements encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash New() {
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

    return BlockSize;
}

private static (nint, error) Write(this ptr<digest> _addr_d, slice<byte> p) {
    nint nn = default;
    error err = default!;
    ref digest d = ref _addr_d.val;
 
    // Note that we currently call block or blockGeneric
    // directly (guarded using haveAsm) because this allows
    // escape analysis to see that p and d don't escape.
    nn = len(p);
    d.len += uint64(nn);
    if (d.nx > 0) {
        var n = copy(d.x[(int)d.nx..], p);
        d.nx += n;
        if (d.nx == BlockSize) {
            if (haveAsm) {
                block(d, d.x[..]);
            }
            else
 {
                blockGeneric(d, d.x[..]);
            }

            d.nx = 0;

        }
        p = p[(int)n..];

    }
    if (len(p) >= BlockSize) {
        n = len(p) & ~(BlockSize - 1);
        if (haveAsm) {
            block(d, p[..(int)n]);
        }
        else
 {
            blockGeneric(d, p[..(int)n]);
        }
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
    return append(in, hash[..]);

}

private static array<byte> checkSum(this ptr<digest> _addr_d) => func((_, panic, _) => {
    ref digest d = ref _addr_d.val;
 
    // Append 0x80 to the end of the message and then append zeros
    // until the length is a multiple of 56 bytes. Finally append
    // 8 bytes representing the message length in bits.
    //
    // 1 byte end marker :: 0-63 padding bytes :: 8 byte length
    array<byte> tmp = new array<byte>(new byte[] { 0x80 });
    nint pad = (55 - d.len) % 64; // calculate number of padding bytes
    binary.LittleEndian.PutUint64(tmp[(int)1 + pad..], d.len << 3); // append length in bits
    d.Write(tmp[..(int)1 + pad + 8]); 

    // The previous write ensures that a whole number of
    // blocks (i.e. a multiple of 64 bytes) have been hashed.
    if (d.nx != 0) {
        panic("d.nx != 0");
    }
    array<byte> digest = new array<byte>(Size);
    binary.LittleEndian.PutUint32(digest[(int)0..], d.s[0]);
    binary.LittleEndian.PutUint32(digest[(int)4..], d.s[1]);
    binary.LittleEndian.PutUint32(digest[(int)8..], d.s[2]);
    binary.LittleEndian.PutUint32(digest[(int)12..], d.s[3]);
    return digest;

});

// Sum returns the MD5 checksum of the data.
public static array<byte> Sum(slice<byte> data) {
    digest d = default;
    d.Reset();
    d.Write(data);
    return d.checkSum();
}

} // end md5_package
