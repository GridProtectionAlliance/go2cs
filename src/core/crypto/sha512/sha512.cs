// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha512 implements the SHA-384, SHA-512, SHA-512/224, and SHA-512/256
// hash algorithms as defined in FIPS 180-4.
//
// All the hash.Hash implementations returned by this package also
// implement encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.
namespace go.crypto;

using crypto = crypto_package;
using boring = crypto.@internal.boring_package;
using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using @internal;
using crypto.@internal;

partial class sha512_package {

[GoInit] internal static void init() {
    crypto.RegisterHash(crypto.SHA384, New384);
    crypto.RegisterHash(crypto.SHA512, New);
    crypto.RegisterHash(crypto.SHA512_224, New512_224);
    crypto.RegisterHash(crypto.SHA512_256, New512_256);
}

public static readonly UntypedInt ΔSize = 64;
public static readonly UntypedInt Size224 = 28;
public static readonly UntypedInt Size256 = 32;
public static readonly UntypedInt Size384 = 48;
public static readonly UntypedInt ΔBlockSize = 128;

internal static readonly UntypedInt chunk = 128;
internal static readonly UntypedInt init0 = /* 0x6a09e667f3bcc908 */ 7640891576956012808;
internal static readonly GoUntyped init1 = /* 0xbb67ae8584caa73b */
    GoUntyped.Parse("13503953896175478587");
internal static readonly UntypedInt init2 = /* 0x3c6ef372fe94f82b */ 4354685564936845355;
internal static readonly GoUntyped init3 = /* 0xa54ff53a5f1d36f1 */
    GoUntyped.Parse("11912009170470909681");
internal static readonly UntypedInt init4 = /* 0x510e527fade682d1 */ 5840696475078001361;
internal static readonly GoUntyped init5 = /* 0x9b05688c2b3e6c1f */
    GoUntyped.Parse("11170449401992604703");
internal static readonly UntypedInt init6 = /* 0x1f83d9abfb41bd6b */ 2270897969802886507;
internal static readonly UntypedInt init7 = /* 0x5be0cd19137e2179 */ 6620516959819538809;
internal static readonly GoUntyped init0_224 = /* 0x8c3d37c819544da2 */
    GoUntyped.Parse("10105294471447203234");
internal static readonly UntypedInt init1_224 = /* 0x73e1996689dcd4d6 */ 8350123849800275158;
internal static readonly UntypedInt init2_224 = /* 0x1dfab7ae32ff9c82 */ 2160240930085379202;
internal static readonly UntypedInt init3_224 = /* 0x679dd514582f9fcf */ 7466358040605728719;
internal static readonly UntypedInt init4_224 = /* 0x0f6d2b697bd44da8 */ 1111592415079452072;
internal static readonly UntypedInt init5_224 = /* 0x77e36f7304c48942 */ 8638871050018654530;
internal static readonly UntypedInt init6_224 = /* 0x3f9d85a86a1d36c8 */ 4583966954114332360;
internal static readonly UntypedInt init7_224 = /* 0x1112e6ad91d692a1 */ 1230299281376055969;
internal static readonly UntypedInt init0_256 = /* 0x22312194fc2bf72c */ 2463787394917988140;
internal static readonly GoUntyped init1_256 = /* 0x9f555fa3c84c64c2 */
    GoUntyped.Parse("11481187982095705282");
internal static readonly UntypedInt init2_256 = /* 0x2393b86b6f53b151 */ 2563595384472711505;
internal static readonly GoUntyped init3_256 = /* 0x963877195940eabd */
    GoUntyped.Parse("10824532655140301501");
internal static readonly GoUntyped init4_256 = /* 0x96283ee2a88effe3 */
    GoUntyped.Parse("10819967247969091555");
internal static readonly GoUntyped init5_256 = /* 0xbe5e1e2553863992 */
    GoUntyped.Parse("13717434660681038226");
internal static readonly UntypedInt init6_256 = /* 0x2b0199fc2c85b8aa */ 3098927326965381290;
internal static readonly UntypedInt init7_256 = /* 0x0eb72ddc81c52ca2 */ 1060366662362279074;
internal static readonly GoUntyped init0_384 = /* 0xcbbb9d5dc1059ed8 */
    GoUntyped.Parse("14680500436340154072");
internal static readonly UntypedInt init1_384 = /* 0x629a292a367cd507 */ 7105036623409894663;
internal static readonly GoUntyped init2_384 = /* 0x9159015a3070dd17 */
    GoUntyped.Parse("10473403895298186519");
internal static readonly UntypedInt init3_384 = /* 0x152fecd8f70e5939 */ 1526699215303891257;
internal static readonly UntypedInt init4_384 = /* 0x67332667ffc00b31 */ 7436329637833083697;
internal static readonly GoUntyped init5_384 = /* 0x8eb44a8768581511 */
    GoUntyped.Parse("10282925794625328401");
internal static readonly GoUntyped init6_384 = /* 0xdb0c2e0d64f98fa7 */
    GoUntyped.Parse("15784041429090275239");
internal static readonly UntypedInt init7_384 = /* 0x47b5481dbefa4fa4 */ 5167115440072839076;

// digest represents the partial evaluation of a checksum.
[GoType] partial struct digest {
    internal array<uint64> h = new(8);
    internal array<byte> x = new(chunk);
    internal nint nx;
    internal uint64 len;
    internal crypto_package.Hash function;
}

[GoRecv] internal static void Reset(this ref digest d) {
    var exprᴛ1 = d.function;
    if (exprᴛ1 == crypto.SHA384) {
        d.h[0] = init0_384;
        d.h[1] = init1_384;
        d.h[2] = init2_384;
        d.h[3] = init3_384;
        d.h[4] = init4_384;
        d.h[5] = init5_384;
        d.h[6] = init6_384;
        d.h[7] = init7_384;
    }
    else if (exprᴛ1 == crypto.SHA512_224) {
        d.h[0] = init0_224;
        d.h[1] = init1_224;
        d.h[2] = init2_224;
        d.h[3] = init3_224;
        d.h[4] = init4_224;
        d.h[5] = init5_224;
        d.h[6] = init6_224;
        d.h[7] = init7_224;
    }
    else if (exprᴛ1 == crypto.SHA512_256) {
        d.h[0] = init0_256;
        d.h[1] = init1_256;
        d.h[2] = init2_256;
        d.h[3] = init3_256;
        d.h[4] = init4_256;
        d.h[5] = init5_256;
        d.h[6] = init6_256;
        d.h[7] = init7_256;
    }
    else { /* default: */
        d.h[0] = init0;
        d.h[1] = init1;
        d.h[2] = init2;
        d.h[3] = init3;
        d.h[4] = init4;
        d.h[5] = init5;
        d.h[6] = init6;
        d.h[7] = init7;
    }

    d.nx = 0;
    d.len = 0;
}

internal static readonly @string magic384 = "sha\x04"u8;
internal static readonly @string magic512_224 = "sha\x05"u8;
internal static readonly @string magic512_256 = "sha\x06"u8;
internal static readonly @string magic512 = "sha\x07"u8;
internal const nint marshaledSize = /* len(magic512) + 8*8 + chunk + 8 */ 204;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    var b = new slice<byte>(0, marshaledSize);
    var exprᴛ1 = d.function;
    if (exprᴛ1 == crypto.SHA384) {
        b = append(b, magic384.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == crypto.SHA512_224) {
        b = append(b, magic512_224.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == crypto.SHA512_256) {
        b = append(b, magic512_256.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == crypto.SHA512) {
        b = append(b, magic512.ꓸꓸꓸ);
    }
    else { /* default: */
        return (default!, errors.New("crypto/sha512: invalid hash function"u8));
    }

    b = byteorder.BeAppendUint64(b, d.h[0]);
    b = byteorder.BeAppendUint64(b, d.h[1]);
    b = byteorder.BeAppendUint64(b, d.h[2]);
    b = byteorder.BeAppendUint64(b, d.h[3]);
    b = byteorder.BeAppendUint64(b, d.h[4]);
    b = byteorder.BeAppendUint64(b, d.h[5]);
    b = byteorder.BeAppendUint64(b, d.h[6]);
    b = byteorder.BeAppendUint64(b, d.h[7]);
    b = append(b, d.x[..(int)(d.nx)].ꓸꓸꓸ);
    b = b[..(int)(len(b) + len(d.x) - d.nx)];
    // already zero
    b = byteorder.BeAppendUint64(b, d.len);
    return (b, default!);
}

[GoRecv] internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic512)) {
        return errors.New("crypto/sha512: invalid hash state identifier"u8);
    }
    switch (ᐧ) {
    case {} when d.function == crypto.SHA384 && ((@string)(b[..(int)(len(magic384))])) == magic384: {
        break;
    }
    case {} when d.function == crypto.SHA512_224 && ((@string)(b[..(int)(len(magic512_224))])) == magic512_224: {
        break;
    }
    case {} when d.function == crypto.SHA512_256 && ((@string)(b[..(int)(len(magic512_256))])) == magic512_256: {
        break;
    }
    case {} when d.function == crypto.SHA512 && ((@string)(b[..(int)(len(magic512))])) == magic512: {
        break;
    }
    default: {
        return errors.New("crypto/sha512: invalid hash state identifier"u8);
    }}

    if (len(b) != marshaledSize) {
        return errors.New("crypto/sha512: invalid hash state size"u8);
    }
    b = b[(int)(len(magic512))..];
    (b, d.h[0]) = consumeUint64(b);
    (b, d.h[1]) = consumeUint64(b);
    (b, d.h[2]) = consumeUint64(b);
    (b, d.h[3]) = consumeUint64(b);
    (b, d.h[4]) = consumeUint64(b);
    (b, d.h[5]) = consumeUint64(b);
    (b, d.h[6]) = consumeUint64(b);
    (b, d.h[7]) = consumeUint64(b);
    b = b[(int)(copy(d.x[..], b))..];
    (b, d.len) = consumeUint64(b);
    d.nx = ((nint)(d.len % chunk));
    return default!;
}

internal static (slice<byte>, uint64) consumeUint64(slice<byte> b) {
    return (b[8..], byteorder.BeUint64(b));
}

// New returns a new hash.Hash computing the SHA-512 checksum.
public static hash.Hash New() {
    if (boring.Enabled) {
        return boring.NewSHA512();
    }
    var d = Ꮡ(new digest(function: crypto.SHA512));
    d.Reset();
    return ~d;
}

// New512_224 returns a new hash.Hash computing the SHA-512/224 checksum.
public static hash.Hash New512_224() {
    var d = Ꮡ(new digest(function: crypto.SHA512_224));
    d.Reset();
    return ~d;
}

// New512_256 returns a new hash.Hash computing the SHA-512/256 checksum.
public static hash.Hash New512_256() {
    var d = Ꮡ(new digest(function: crypto.SHA512_256));
    d.Reset();
    return ~d;
}

// New384 returns a new hash.Hash computing the SHA-384 checksum.
public static hash.Hash New384() {
    if (boring.Enabled) {
        return boring.NewSHA384();
    }
    var d = Ꮡ(new digest(function: crypto.SHA384));
    d.Reset();
    return ~d;
}

[GoRecv] internal static nint Size(this ref digest d) {
    var exprᴛ1 = d.function;
    if (exprᴛ1 == crypto.SHA512_224) {
        return Size224;
    }
    if (exprᴛ1 == crypto.SHA512_256) {
        return Size256;
    }
    if (exprᴛ1 == crypto.SHA384) {
        return Size384;
    }
    { /* default: */
        return ΔSize;
    }

}

[GoRecv] internal static nint BlockSize(this ref digest d) {
    return ΔBlockSize;
}

[GoRecv] internal static (nint nn, error err) Write(this ref digest d, slice<byte> p) {
    nint nn = default!;
    error err = default!;

    if (d.function != crypto.SHA512_224 && d.function != crypto.SHA512_256) {
        boring.Unreachable();
    }
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
    if (d.function != crypto.SHA512_224 && d.function != crypto.SHA512_256) {
        boring.Unreachable();
    }
    // Make a copy of d so that caller can keep writing and summing.
    var d0 = @new<digest>();
    d0.val = d;
    var hash = d0.checkSum();
    var exprᴛ1 = (~d0).function;
    if (exprᴛ1 == crypto.SHA384) {
        return append(@in, hash[..(int)(Size384)].ꓸꓸꓸ);
    }
    if (exprᴛ1 == crypto.SHA512_224) {
        return append(@in, hash[..(int)(Size224)].ꓸꓸꓸ);
    }
    if (exprᴛ1 == crypto.SHA512_256) {
        return append(@in, hash[..(int)(Size256)].ꓸꓸꓸ);
    }
    { /* default: */
        return append(@in, hash[..].ꓸꓸꓸ);
    }

}

[GoRecv] internal static array<byte> checkSum(this ref digest d) {
    // Padding. Add a 1 bit and 0 bits until 112 bytes mod 128.
    var len = d.len;
    array<byte> tmp = new(144); /* 128 + 16 */                      // padding + length buffer
    tmp[0] = 128;
    uint64 t = default!;
    if (len % 128 < 112){
        t = 112 - len % 128;
    } else {
        t = 128 + 112 - len % 128;
    }
    // Length in bits.
    len <<= (UntypedInt)(3);
    var padlen = tmp[..(int)(t + 16)];
    // Upper 64 bits are always zero, because len variable has type uint64,
    // and tmp is already zeroed at that index, so we can skip updating it.
    // byteorder.BePutUint64(padlen[t+0:], 0)
    byteorder.BePutUint64(padlen[(int)(t + 8)..], len);
    d.Write(padlen);
    if (d.nx != 0) {
        throw panic("d.nx != 0");
    }
    array<byte> digest = new(64); /* ΔSize */
    byteorder.BePutUint64(digest[0..], d.h[0]);
    byteorder.BePutUint64(digest[8..], d.h[1]);
    byteorder.BePutUint64(digest[16..], d.h[2]);
    byteorder.BePutUint64(digest[24..], d.h[3]);
    byteorder.BePutUint64(digest[32..], d.h[4]);
    byteorder.BePutUint64(digest[40..], d.h[5]);
    if (d.function != crypto.SHA384) {
        byteorder.BePutUint64(digest[48..], d.h[6]);
        byteorder.BePutUint64(digest[56..], d.h[7]);
    }
    return digest;
}

// Sum512 returns the SHA512 checksum of the data.
public static array<byte> Sum512(slice<byte> data) {
    if (boring.Enabled) {
        return boring.SHA512(data);
    }
    var d = new digest(function: crypto.SHA512);
    d.Reset();
    d.Write(data);
    return d.checkSum();
}

// Sum384 returns the SHA384 checksum of the data.
public static array<byte> Sum384(slice<byte> data) {
    if (boring.Enabled) {
        return boring.SHA384(data);
    }
    var d = new digest(function: crypto.SHA384);
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    var ap = (ж<array<byte>>)(sum[..]);
    return ap.val;
}

// Sum512_224 returns the Sum512/224 checksum of the data.
public static array<byte> Sum512_224(slice<byte> data) {
    var d = new digest(function: crypto.SHA512_224);
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    var ap = (ж<array<byte>>)(sum[..]);
    return ap.val;
}

// Sum512_256 returns the Sum512/256 checksum of the data.
public static array<byte> Sum512_256(slice<byte> data) {
    var d = new digest(function: crypto.SHA512_256);
    d.Reset();
    d.Write(data);
    var sum = d.checkSum();
    var ap = (ж<array<byte>>)(sum[..]);
    return ap.val;
}

} // end sha512_package
