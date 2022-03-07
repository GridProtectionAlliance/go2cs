// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha1 implements the SHA-1 hash algorithm as defined in RFC 3174.
//
// SHA-1 is cryptographically broken and should not be used for secure
// applications.
// package sha1 -- go2cs converted at 2022 March 06 22:19:25 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Program Files\Go\src\crypto\sha1\sha1.go
using crypto = go.crypto_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;

namespace go.crypto;

public static partial class sha1_package {

private static void init() {
    crypto.RegisterHash(crypto.SHA1, New);
}

// The size of a SHA-1 checksum in bytes.
public static readonly nint Size = 20;

// The blocksize of SHA-1 in bytes.


// The blocksize of SHA-1 in bytes.
public static readonly nint BlockSize = 64;



private static readonly nint chunk = 64;
private static readonly nuint init0 = 0x67452301;
private static readonly nuint init1 = 0xEFCDAB89;
private static readonly nuint init2 = 0x98BADCFE;
private static readonly nuint init3 = 0x10325476;
private static readonly nuint init4 = 0xC3D2E1F0;


// digest represents the partial evaluation of a checksum.
private partial struct digest {
    public array<uint> h;
    public array<byte> x;
    public nint nx;
    public ulong len;
}

private static readonly @string magic = "sha\x01";
private static readonly var marshaledSize = len(magic) + 5 * 4 + chunk + 8;


private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref digest d = ref _addr_d.val;

    var b = make_slice<byte>(0, marshaledSize);
    b = append(b, magic);
    b = appendUint32(b, d.h[0]);
    b = appendUint32(b, d.h[1]);
    b = appendUint32(b, d.h[2]);
    b = appendUint32(b, d.h[3]);
    b = appendUint32(b, d.h[4]);
    b = append(b, d.x[..(int)d.nx]);
    b = b[..(int)len(b) + len(d.x) - int(d.nx)]; // already zero
    b = appendUint64(b, d.len);
    return (b, error.As(null!)!);

}

private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b) {
    ref digest d = ref _addr_d.val;

    if (len(b) < len(magic) || string(b[..(int)len(magic)]) != magic) {
        return error.As(errors.New("crypto/sha1: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize) {
        return error.As(errors.New("crypto/sha1: invalid hash state size"))!;
    }
    b = b[(int)len(magic)..];
    b, d.h[0] = consumeUint32(b);
    b, d.h[1] = consumeUint32(b);
    b, d.h[2] = consumeUint32(b);
    b, d.h[3] = consumeUint32(b);
    b, d.h[4] = consumeUint32(b);
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

    d.h[0] = init0;
    d.h[1] = init1;
    d.h[2] = init2;
    d.h[3] = init3;
    d.h[4] = init4;
    d.nx = 0;
    d.len = 0;
}

// New returns a new hash.Hash computing the SHA1 checksum. The Hash also
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
    return append(in, hash[..]);

}

private static array<byte> checkSum(this ptr<digest> _addr_d) => func((_, panic, _) => {
    ref digest d = ref _addr_d.val;

    var len = d.len; 
    // Padding.  Add a 1 bit and 0 bits until 56 bytes mod 64.
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

    return digest;

});

// ConstantTimeSum computes the same result of Sum() but in constant time
private static slice<byte> ConstantTimeSum(this ptr<digest> _addr_d, slice<byte> @in) {
    ref digest d = ref _addr_d.val;

    var d0 = d.val;
    var hash = d0.constSum();
    return append(in, hash[..]);
}

private static array<byte> constSum(this ptr<digest> _addr_d) {
    ref digest d = ref _addr_d.val;

    array<byte> length = new array<byte>(8);
    var l = d.len << 3;
    {
        var i__prev1 = i;

        for (var i = uint(0); i < 8; i++) {
            length[i] = byte(l >> (int)((56 - 8 * i)));
        }

        i = i__prev1;
    }

    var nx = byte(d.nx);
    var t = nx - 56; // if nx < 56 then the MSB of t is one
    var mask1b = byte(int8(t) >> 7); // mask1b is 0xFF iff one block is enough

    var separator = byte(0x80); // gets reset to 0x00 once used
    {
        var i__prev1 = i;

        for (i = byte(0); i < chunk; i++) {
            var mask = byte(int8(i - nx) >> 7); // 0x00 after the end of data

            // if we reached the end of the data, replace with 0x80 or 0x00
            d.x[i] = (~mask & separator) | (mask & d.x[i]); 

            // zero the separator once used
            separator &= mask;

            if (i >= 56) { 
                // we might have to write the length here if all fit in one block
                d.x[i] |= mask1b & length[i - 56];

            }

        }

        i = i__prev1;
    } 

    // compress, and only keep the digest if all fit in one block
    block(d, d.x[..]);

    array<byte> digest = new array<byte>(Size);
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in d.h) {
            i = __i;
            s = __s;
            digest[i * 4] = mask1b & byte(s >> 24);
            digest[i * 4 + 1] = mask1b & byte(s >> 16);
            digest[i * 4 + 2] = mask1b & byte(s >> 8);
            digest[i * 4 + 3] = mask1b & byte(s);
        }
        i = i__prev1;
        s = s__prev1;
    }

    {
        var i__prev1 = i;

        for (i = byte(0); i < chunk; i++) { 
            // second block, it's always past the end of data, might start with 0x80
            if (i < 56) {
                d.x[i] = separator;
                separator = 0;
            }
            else
 {
                d.x[i] = length[i - 56];
            }

        }

        i = i__prev1;
    } 

    // compress, and only keep the digest if we actually needed the second block
    block(d, d.x[..]);

    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in d.h) {
            i = __i;
            s = __s;
            digest[i * 4] |= ~mask1b & byte(s >> 24);
            digest[i * 4 + 1] |= ~mask1b & byte(s >> 16);
            digest[i * 4 + 2] |= ~mask1b & byte(s >> 8);
            digest[i * 4 + 3] |= ~mask1b & byte(s);
        }
        i = i__prev1;
        s = s__prev1;
    }

    return digest;

}

// Sum returns the SHA-1 checksum of the data.
public static array<byte> Sum(slice<byte> data) {
    digest d = default;
    d.Reset();
    d.Write(data);
    return d.checkSum();
}

} // end sha1_package
