// Copyright (c) 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package field implements fast arithmetic modulo 2^255-19.
namespace go.crypto.@internal.edwards25519;

using subtle = crypto.subtle_package;
using errors = errors_package;
using byteorder = @internal.byteorder_package;
using bits = math.bits_package;
using @internal;
using crypto;
using math;

partial class field_package {

// Element represents an element of the field GF(2^255-19). Note that this
// is not a cryptographically secure group, and should only be used to interact
// with edwards25519.Point coordinates.
//
// This type works similarly to math/big.Int, and all arguments and receivers
// are allowed to alias.
//
// The zero value is a valid zero element.
[GoType] partial struct Element {
    // An element t represents the integer
    //     t.l0 + t.l1*2^51 + t.l2*2^102 + t.l3*2^153 + t.l4*2^204
    //
    // Between operations, all limbs are expected to be lower than 2^52.
    internal uint64 l0;
    internal uint64 l1;
    internal uint64 l2;
    internal uint64 l3;
    internal uint64 l4;
}

internal const uint64 maskLow51Bits = /* (1 << 51) - 1 */ 2251799813685247;

internal static ж<Element> feZero = Ꮡ(new Element(0, 0, 0, 0, 0));

// Zero sets v = 0, and returns v.
[GoRecv("capture")] public static ж<Element> Zero(this ref Element v) {
    v = feZero.val;
    return ZeroꓸᏑv;
}

internal static ж<Element> feOne = Ꮡ(new Element(1, 0, 0, 0, 0));

// One sets v = 1, and returns v.
[GoRecv("capture")] public static ж<Element> One(this ref Element v) {
    v = feOne.val;
    return OneꓸᏑv;
}

// reduce reduces v modulo 2^255 - 19 and returns it.
[GoRecv("capture")] internal static ж<Element> reduce(this ref Element v) {
    v.carryPropagate();
    // After the light reduction we now have a field element representation
    // v < 2^255 + 2^13 * 19, but need v < 2^255 - 19.
    // If v >= 2^255 - 19, then v + 19 >= 2^255, which would overflow 2^255 - 1,
    // generating a carry. That is, c will be 0 if v < 2^255 - 19, and 1 otherwise.
    var c = (v.l0 + 19) >> (int)(51);
    c = (v.l1 + c) >> (int)(51);
    c = (v.l2 + c) >> (int)(51);
    c = (v.l3 + c) >> (int)(51);
    c = (v.l4 + c) >> (int)(51);
    // If v < 2^255 - 19 and c = 0, this will be a no-op. Otherwise, it's
    // effectively applying the reduction identity to the carry.
    v.l0 += 19 * c;
    v.l1 += v.l0 >> (int)(51);
    v.l0 = (uint64)(v.l0 & maskLow51Bits);
    v.l2 += v.l1 >> (int)(51);
    v.l1 = (uint64)(v.l1 & maskLow51Bits);
    v.l3 += v.l2 >> (int)(51);
    v.l2 = (uint64)(v.l2 & maskLow51Bits);
    v.l4 += v.l3 >> (int)(51);
    v.l3 = (uint64)(v.l3 & maskLow51Bits);
    // no additional carry
    v.l4 = (uint64)(v.l4 & maskLow51Bits);
    return reduceꓸᏑv;
}

// Add sets v = a + b, and returns v.
[GoRecv] public static ж<Element> Add(this ref Element v, ж<Element> Ꮡa, ж<Element> Ꮡb) {
    ref var a = ref Ꮡa.val;
    ref var b = ref Ꮡb.val;

    v.l0 = a.l0 + b.l0;
    v.l1 = a.l1 + b.l1;
    v.l2 = a.l2 + b.l2;
    v.l3 = a.l3 + b.l3;
    v.l4 = a.l4 + b.l4;
    // Using the generic implementation here is actually faster than the
    // assembly. Probably because the body of this function is so simple that
    // the compiler can figure out better optimizations by inlining the carry
    // propagation.
    return v.carryPropagateGeneric();
}

// Subtract sets v = a - b, and returns v.
[GoRecv] public static ж<Element> Subtract(this ref Element v, ж<Element> Ꮡa, ж<Element> Ꮡb) {
    ref var a = ref Ꮡa.val;
    ref var b = ref Ꮡb.val;

    // We first add 2 * p, to guarantee the subtraction won't underflow, and
    // then subtract b (which can be up to 2^255 + 2^13 * 19).
    v.l0 = (a.l0 + (nint)4503599627370458L) - b.l0;
    v.l1 = (a.l1 + (nint)4503599627370494L) - b.l1;
    v.l2 = (a.l2 + (nint)4503599627370494L) - b.l2;
    v.l3 = (a.l3 + (nint)4503599627370494L) - b.l3;
    v.l4 = (a.l4 + (nint)4503599627370494L) - b.l4;
    return v.carryPropagate();
}

// Negate sets v = -a, and returns v.
[GoRecv] public static ж<Element> Negate(this ref Element v, ж<Element> Ꮡa) {
    ref var a = ref Ꮡa.val;

    return v.Subtract(feZero, Ꮡa);
}

// Invert sets v = 1/z mod p, and returns v.
//
// If z == 0, Invert returns v = 0.
[GoRecv] public static ж<Element> Invert(this ref Element v, ж<Element> Ꮡz) {
    ref var z = ref Ꮡz.val;

    // Inversion is implemented as exponentiation with exponent p − 2. It uses the
    // same sequence of 255 squarings and 11 multiplications as [Curve25519].
    ref var z2 = ref heap(new Element(), out var Ꮡz2);
    ref var z9 = ref heap(new Element(), out var Ꮡz9);
    ref var z11 = ref heap(new Element(), out var Ꮡz11);
    ref var z2_5_0 = ref heap(new Element(), out var Ꮡz2_5_0);
    ref var z2_10_0 = ref heap(new Element(), out var Ꮡz2_10_0);
    ref var z2_20_0 = ref heap(new Element(), out var Ꮡz2_20_0);
    ref var z2_50_0 = ref heap(new Element(), out var Ꮡz2_50_0);
    ref var z2_100_0 = ref heap(new Element(), out var Ꮡz2_100_0);
    ref var t = ref heap(new Element(), out var Ꮡt);
    z2.Square(Ꮡz);
    // 2
    t.Square(Ꮡz2);
    // 4
    t.Square(Ꮡt);
    // 8
    z9.Multiply(Ꮡt, Ꮡz);
    // 9
    z11.Multiply(Ꮡz9, Ꮡz2);
    // 11
    t.Square(Ꮡz11);
    // 22
    z2_5_0.Multiply(Ꮡt, Ꮡz9);
    // 31 = 2^5 - 2^0
    t.Square(Ꮡz2_5_0);
    // 2^6 - 2^1
    for (nint i = 0; i < 4; i++) {
        t.Square(Ꮡt);
    }
    // 2^10 - 2^5
    z2_10_0.Multiply(Ꮡt, Ꮡz2_5_0);
    // 2^10 - 2^0
    t.Square(Ꮡz2_10_0);
    // 2^11 - 2^1
    for (nint i = 0; i < 9; i++) {
        t.Square(Ꮡt);
    }
    // 2^20 - 2^10
    z2_20_0.Multiply(Ꮡt, Ꮡz2_10_0);
    // 2^20 - 2^0
    t.Square(Ꮡz2_20_0);
    // 2^21 - 2^1
    for (nint i = 0; i < 19; i++) {
        t.Square(Ꮡt);
    }
    // 2^40 - 2^20
    t.Multiply(Ꮡt, Ꮡz2_20_0);
    // 2^40 - 2^0
    t.Square(Ꮡt);
    // 2^41 - 2^1
    for (nint i = 0; i < 9; i++) {
        t.Square(Ꮡt);
    }
    // 2^50 - 2^10
    z2_50_0.Multiply(Ꮡt, Ꮡz2_10_0);
    // 2^50 - 2^0
    t.Square(Ꮡz2_50_0);
    // 2^51 - 2^1
    for (nint i = 0; i < 49; i++) {
        t.Square(Ꮡt);
    }
    // 2^100 - 2^50
    z2_100_0.Multiply(Ꮡt, Ꮡz2_50_0);
    // 2^100 - 2^0
    t.Square(Ꮡz2_100_0);
    // 2^101 - 2^1
    for (nint i = 0; i < 99; i++) {
        t.Square(Ꮡt);
    }
    // 2^200 - 2^100
    t.Multiply(Ꮡt, Ꮡz2_100_0);
    // 2^200 - 2^0
    t.Square(Ꮡt);
    // 2^201 - 2^1
    for (nint i = 0; i < 49; i++) {
        t.Square(Ꮡt);
    }
    // 2^250 - 2^50
    t.Multiply(Ꮡt, Ꮡz2_50_0);
    // 2^250 - 2^0
    t.Square(Ꮡt);
    // 2^251 - 2^1
    t.Square(Ꮡt);
    // 2^252 - 2^2
    t.Square(Ꮡt);
    // 2^253 - 2^3
    t.Square(Ꮡt);
    // 2^254 - 2^4
    t.Square(Ꮡt);
    // 2^255 - 2^5
    return v.Multiply(Ꮡt, Ꮡz11);
}

// 2^255 - 21

// Set sets v = a, and returns v.
[GoRecv("capture")] public static ж<Element> Set(this ref Element v, ж<Element> Ꮡa) {
    ref var a = ref Ꮡa.val;

    v = a;
    return SetꓸᏑv;
}

// SetBytes sets v to x, where x is a 32-byte little-endian encoding. If x is
// not of the right length, SetBytes returns nil and an error, and the
// receiver is unchanged.
//
// Consistent with RFC 7748, the most significant bit (the high bit of the
// last byte) is ignored, and non-canonical values (2^255-19 through 2^255-1)
// are accepted. Note that this is laxer than specified by RFC 8032, but
// consistent with most Ed25519 implementations.
[GoRecv("capture")] public static (ж<Element>, error) SetBytes(this ref Element v, slice<byte> x) {
    if (len(x) != 32) {
        return (default!, errors.New("edwards25519: invalid field element input size"u8));
    }
    // Bits 0:51 (bytes 0:8, bits 0:64, shift 0, mask 51).
    v.l0 = byteorder.LeUint64(x[0..8]);
    v.l0 &= (uint64)(maskLow51Bits);
    // Bits 51:102 (bytes 6:14, bits 48:112, shift 3, mask 51).
    v.l1 = byteorder.LeUint64(x[6..14]) >> (int)(3);
    v.l1 &= (uint64)(maskLow51Bits);
    // Bits 102:153 (bytes 12:20, bits 96:160, shift 6, mask 51).
    v.l2 = byteorder.LeUint64(x[12..20]) >> (int)(6);
    v.l2 &= (uint64)(maskLow51Bits);
    // Bits 153:204 (bytes 19:27, bits 152:216, shift 1, mask 51).
    v.l3 = byteorder.LeUint64(x[19..27]) >> (int)(1);
    v.l3 &= (uint64)(maskLow51Bits);
    // Bits 204:255 (bytes 24:32, bits 192:256, shift 12, mask 51).
    // Note: not bytes 25:33, shift 4, to avoid overread.
    v.l4 = byteorder.LeUint64(x[24..32]) >> (int)(12);
    v.l4 &= (uint64)(maskLow51Bits);
    return (SetBytesꓸᏑv, default!);
}

// Bytes returns the canonical 32-byte little-endian encoding of v.
[GoRecv] public static slice<byte> Bytes(this ref Element v) {
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var out = ref heap(new array<byte>(32), out var Ꮡout);
    return v.bytes(Ꮡ@out);
}

[GoRecv] public static slice<byte> bytes(this ref Element v, ж<array<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    var t = v;
    t.reduce();
    array<byte> buf = new(8);
    foreach (var (i, l) in new uint64[]{t.l0, t.l1, t.l2, t.l3, t.l4}.array()) {
        nint bitsOffset = i * 51;
        byteorder.LePutUint64(buf[..], l << (int)(((nuint)(bitsOffset % 8))));
        foreach (var (iΔ1, bb) in buf) {
            nint off = bitsOffset / 8 + iΔ1;
            if (off >= len(@out)) {
                break;
            }
            @out[off] |= (byte)(bb);
        }
    }
    return @out[..];
}

// Equal returns 1 if v and u are equal, and 0 otherwise.
[GoRecv] public static nint Equal(this ref Element v, ж<Element> Ꮡu) {
    ref var u = ref Ꮡu.val;

    var sa = u.Bytes();
    var sv = v.Bytes();
    return subtle.ConstantTimeCompare(sa, sv);
}

// mask64Bits returns 0xffffffff if cond is 1, and 0 otherwise.
internal static uint64 mask64Bits(nint cond) {
    return ~(((uint64)cond) - 1);
}

// Select sets v to a if cond == 1, and to b if cond == 0.
[GoRecv("capture")] public static ж<Element> Select(this ref Element v, ж<Element> Ꮡa, ж<Element> Ꮡb, nint cond) {
    ref var a = ref Ꮡa.val;
    ref var b = ref Ꮡb.val;

    var m = mask64Bits(cond);
    v.l0 = (uint64)(((uint64)(m & a.l0)) | ((uint64)(~m & b.l0)));
    v.l1 = (uint64)(((uint64)(m & a.l1)) | ((uint64)(~m & b.l1)));
    v.l2 = (uint64)(((uint64)(m & a.l2)) | ((uint64)(~m & b.l2)));
    v.l3 = (uint64)(((uint64)(m & a.l3)) | ((uint64)(~m & b.l3)));
    v.l4 = (uint64)(((uint64)(m & a.l4)) | ((uint64)(~m & b.l4)));
    return SelectꓸᏑv;
}

// Swap swaps v and u if cond == 1 or leaves them unchanged if cond == 0, and returns v.
[GoRecv] public static void Swap(this ref Element v, ж<Element> Ꮡu, nint cond) {
    ref var u = ref Ꮡu.val;

    var m = mask64Bits(cond);
    var t = (uint64)(m & ((uint64)(v.l0 ^ u.l0)));
    v.l0 ^= (uint64)(t);
    u.l0 ^= (uint64)(t);
    t = (uint64)(m & ((uint64)(v.l1 ^ u.l1)));
    v.l1 ^= (uint64)(t);
    u.l1 ^= (uint64)(t);
    t = (uint64)(m & ((uint64)(v.l2 ^ u.l2)));
    v.l2 ^= (uint64)(t);
    u.l2 ^= (uint64)(t);
    t = (uint64)(m & ((uint64)(v.l3 ^ u.l3)));
    v.l3 ^= (uint64)(t);
    u.l3 ^= (uint64)(t);
    t = (uint64)(m & ((uint64)(v.l4 ^ u.l4)));
    v.l4 ^= (uint64)(t);
    u.l4 ^= (uint64)(t);
}

// IsNegative returns 1 if v is negative, and 0 otherwise.
[GoRecv] public static nint IsNegative(this ref Element v) {
    return ((nint)((byte)(v.Bytes()[0] & 1)));
}

// Absolute sets v to |u|, and returns v.
[GoRecv] public static ж<Element> Absolute(this ref Element v, ж<Element> Ꮡu) {
    ref var u = ref Ꮡu.val;

    return v.Select(@new<Element>().Negate(Ꮡu), Ꮡu, u.IsNegative());
}

// Multiply sets v = x * y, and returns v.
[GoRecv("capture")] public static ж<Element> Multiply(this ref Element v, ж<Element> Ꮡx, ж<Element> Ꮡy) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    feMul(v, Ꮡx, Ꮡy);
    return MultiplyꓸᏑv;
}

// Square sets v = x * x, and returns v.
[GoRecv("capture")] public static ж<Element> Square(this ref Element v, ж<Element> Ꮡx) {
    ref var x = ref Ꮡx.val;

    feSquare(v, Ꮡx);
    return SquareꓸᏑv;
}

// Mult32 sets v = x * y, and returns v.
[GoRecv("capture")] public static ж<Element> Mult32(this ref Element v, ж<Element> Ꮡx, uint32 y) {
    ref var x = ref Ꮡx.val;

    var (x0lo, x0hi) = mul51(x.l0, y);
    var (x1lo, x1hi) = mul51(x.l1, y);
    var (x2lo, x2hi) = mul51(x.l2, y);
    var (x3lo, x3hi) = mul51(x.l3, y);
    var (x4lo, x4hi) = mul51(x.l4, y);
    v.l0 = x0lo + 19 * x4hi;
    // carried over per the reduction identity
    v.l1 = x1lo + x0hi;
    v.l2 = x2lo + x1hi;
    v.l3 = x3lo + x2hi;
    v.l4 = x4lo + x3hi;
    // The hi portions are going to be only 32 bits, plus any previous excess,
    // so we can skip the carry propagation.
    return Mult32ꓸᏑv;
}

// mul51 returns lo + hi * 2⁵¹ = a * b.
internal static (uint64 lo, uint64 hi) mul51(uint64 a, uint32 b) {
    uint64 lo = default!;
    uint64 hi = default!;

    var (mh, ml) = bits.Mul64(a, ((uint64)b));
    lo = (uint64)(ml & maskLow51Bits);
    hi = (uint64)((mh << (int)(13)) | (ml >> (int)(51)));
    return (lo, hi);
}

// Pow22523 set v = x^((p-5)/8), and returns v. (p-5)/8 is 2^252-3.
[GoRecv] public static ж<Element> Pow22523(this ref Element v, ж<Element> Ꮡx) {
    ref var x = ref Ꮡx.val;

    ref var t0 = ref heap(new Element(), out var Ꮡt0);
    ref var t1 = ref heap(new Element(), out var Ꮡt1);
    ref var t2 = ref heap(new Element(), out var Ꮡt2);
    t0.Square(Ꮡx);
    // x^2
    t1.Square(Ꮡt0);
    // x^4
    t1.Square(Ꮡt1);
    // x^8
    t1.Multiply(Ꮡx, Ꮡt1);
    // x^9
    t0.Multiply(Ꮡt0, Ꮡt1);
    // x^11
    t0.Square(Ꮡt0);
    // x^22
    t0.Multiply(Ꮡt1, Ꮡt0);
    // x^31
    t1.Square(Ꮡt0);
    // x^62
    for (nint i = 1; i < 5; i++) {
        // x^992
        t1.Square(Ꮡt1);
    }
    t0.Multiply(Ꮡt1, Ꮡt0);
    // x^1023 -> 1023 = 2^10 - 1
    t1.Square(Ꮡt0);
    // 2^11 - 2
    for (nint i = 1; i < 10; i++) {
        // 2^20 - 2^10
        t1.Square(Ꮡt1);
    }
    t1.Multiply(Ꮡt1, Ꮡt0);
    // 2^20 - 1
    t2.Square(Ꮡt1);
    // 2^21 - 2
    for (nint i = 1; i < 20; i++) {
        // 2^40 - 2^20
        t2.Square(Ꮡt2);
    }
    t1.Multiply(Ꮡt2, Ꮡt1);
    // 2^40 - 1
    t1.Square(Ꮡt1);
    // 2^41 - 2
    for (nint i = 1; i < 10; i++) {
        // 2^50 - 2^10
        t1.Square(Ꮡt1);
    }
    t0.Multiply(Ꮡt1, Ꮡt0);
    // 2^50 - 1
    t1.Square(Ꮡt0);
    // 2^51 - 2
    for (nint i = 1; i < 50; i++) {
        // 2^100 - 2^50
        t1.Square(Ꮡt1);
    }
    t1.Multiply(Ꮡt1, Ꮡt0);
    // 2^100 - 1
    t2.Square(Ꮡt1);
    // 2^101 - 2
    for (nint i = 1; i < 100; i++) {
        // 2^200 - 2^100
        t2.Square(Ꮡt2);
    }
    t1.Multiply(Ꮡt2, Ꮡt1);
    // 2^200 - 1
    t1.Square(Ꮡt1);
    // 2^201 - 2
    for (nint i = 1; i < 50; i++) {
        // 2^250 - 2^50
        t1.Square(Ꮡt1);
    }
    t0.Multiply(Ꮡt1, Ꮡt0);
    // 2^250 - 1
    t0.Square(Ꮡt0);
    // 2^251 - 2
    t0.Square(Ꮡt0);
    // 2^252 - 4
    return v.Multiply(Ꮡt0, Ꮡx);
}

// 2^252 - 3 -> x^(2^252-3)

// sqrtM1 is 2^((p-1)/4), which squared is equal to -1 by Euler's Criterion.
internal static ж<Element> sqrtM1 = Ꮡ(new Element((nint)1718705420411056L, (nint)234908883556509L,
    (nint)2233514472574048L, (nint)2117202627021982L, (nint)765476049583133L));

// SqrtRatio sets r to the non-negative square root of the ratio of u and v.
//
// If u/v is square, SqrtRatio returns r and 1. If u/v is not square, SqrtRatio
// sets r according to Section 4.3 of draft-irtf-cfrg-ristretto255-decaf448-00,
// and returns r and 0.
[GoRecv("capture")] public static (ж<Element> R, nint wasSquare) SqrtRatio(this ref Element r, ж<Element> Ꮡu, ж<Element> Ꮡv) {
    ж<Element> R = default!;
    nint wasSquare = default!;

    ref var u = ref Ꮡu.val;
    ref var v = ref Ꮡv.val;
    var t0 = @new<Element>();
    // r = (u * v3) * (u * v7)^((p-5)/8)
    var v2 = @new<Element>().Square(Ꮡv);
    var uv3 = @new<Element>().Multiply(Ꮡu, t0.Multiply(v2, Ꮡv));
    var uv7 = @new<Element>().Multiply(uv3, t0.Square(v2));
    var rr = @new<Element>().Multiply(uv3, t0.Pow22523(uv7));
    var check = @new<Element>().Multiply(Ꮡv, t0.Square(rr));
    // check = v * r^2
    var uNeg = @new<Element>().Negate(Ꮡu);
    nint correctSignSqrt = check.Equal(Ꮡu);
    nint flippedSignSqrt = check.Equal(uNeg);
    nint flippedSignSqrtI = check.Equal(t0.Multiply(uNeg, sqrtM1));
    var rPrime = @new<Element>().Multiply(rr, sqrtM1);
    // r_prime = SQRT_M1 * r
    // r = CT_SELECT(r_prime IF flipped_sign_sqrt | flipped_sign_sqrt_i ELSE r)
    rr.Select(rPrime, rr, (nint)(flippedSignSqrt | flippedSignSqrtI));
    r.Absolute(rr);
    // Choose the nonnegative square root.
    return (SqrtRatioꓸᏑr, (nint)(correctSignSqrt | flippedSignSqrt));
}

} // end field_package
