// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains the Go wrapper for the constant-time, 64-bit assembly
// implementation of P256. The optimizations performed here are described in
// detail in:
// S.Gueron and V.Krasnov, "Fast prime field elliptic-curve cryptography with
//                          256-bit primes"
// https://link.springer.com/article/10.1007%2Fs13389-014-0090-x
// https://eprint.iacr.org/2013/816.pdf
//go:build (amd64 || arm64 || ppc64le || s390x) && !purego
namespace go.crypto.@internal;

// blank import: embed_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using bits = math.bits_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;
using go.@internal;
using math;

partial class nistec_package {

[GoType("[4]uint64")] partial struct p256Element;

// p256One is one in the Montgomery domain.
internal static p256Element p256One = new p256Element(new uint64[]{0x0000000000000001, (nuint)0xffffffff00000000UL,
    (nuint)0xffffffffffffffffUL, 0x00000000fffffffeU}.array());

internal static ж<p256Element> Ꮡp256Zero = new(new p256Element(new uint64[4].array()));
internal static ref p256Element p256Zero => ref Ꮡp256Zero.Value;

// p256P is 2²⁵⁶ - 2²²⁴ + 2¹⁹² + 2⁹⁶ - 1 in the Montgomery domain.
internal static p256Element p256P = new p256Element(new uint64[]{(nuint)0xffffffffffffffffUL, 0x00000000ffffffffU,
    0x0000000000000000, (nuint)0xffffffff00000001UL}.array());

// P256Point is a P-256 point. The zero value should not be assumed to be valid
// (although it is in this implementation).
[GoType] partial struct P256Point {
    // (X:Y:Z) are Jacobian coordinates where x = X/Z² and y = Y/Z³. The point
    // at infinity can be represented by any set of coordinates with Z = 0.
    internal p256Element x, y, z;
}

// NewP256Point returns a new P256Point representing the point at infinity.
public static ж<P256Point> NewP256Point() {
    return Ꮡ(new P256Point(
        x: p256One, y: p256One, z: p256Zero
    ));
}

// SetGenerator sets p to the canonical generator and returns p.
public static ж<P256Point> SetGenerator(this ж<P256Point> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p.x = new p256Element(new uint64[]{0x79e730d418a9143cUL, 0x75ba95fc5fedb601UL,
        0x79fb732b77622510UL, 0x18905f76a53755c6UL}.array());
    p.y = new p256Element(new uint64[]{(nuint)0xddf25357ce95560aUL, (nuint)0x8b4ab8e4ba19e45cUL,
        (nuint)0xd2e88688dd21f325UL, (nuint)0x8571ff1825885d85UL}.array());
    p.z = p256One;
    return Ꮡp;
}

// Set sets p = q and returns p.
public static ж<P256Point> Set(this ж<P256Point> Ꮡp, ж<P256Point> Ꮡq) {
    ref var p = ref Ꮡp.Value;
    ref var q = ref Ꮡq.Value;

    p.x = q.x;
    p.y = q.y;
    p.z = q.z;
    return Ꮡp;
}

internal static readonly UntypedInt p256ElementLength = 32;

internal static readonly UntypedInt p256UncompressedLength = /* 1 + 2*p256ElementLength */ 65;

internal static readonly UntypedInt p256CompressedLength = /* 1 + p256ElementLength */ 33;

// SetBytes sets p to the compressed, uncompressed, or infinity value encoded in
// b, as specified in SEC 1, Version 2.0, Section 2.3.4. If the point is not on
// the curve, it returns nil and an error, and the receiver is unchanged.
// Otherwise, it returns p.
public static (ж<P256Point>, error) SetBytes(this ж<P256Point> Ꮡp, slice<byte> b) {
    ref var p = ref Ꮡp.Value;

    // p256Mul operates in the Montgomery domain with R = 2²⁵⁶ mod p. Thus rr
    // here is R in the Montgomery domain, or R×R mod p. See comment in
    // P256OrdInverse about how this is used.
    ref var rr = ref heap<p256Element>(out var Ꮡrr);
    rr = new p256Element(new uint64[]{0x0000000000000003, (nuint)0xfffffffbffffffffUL,
        (nuint)0xfffffffffffffffeUL, 0x00000004fffffffdUL}.array());
    switch (ᐧ) {
    case {} when len(b) == 1 && b[0] == 0: {
        return (Ꮡp.Set(NewP256Point()), default!);
    }
    case {} when len(b) == p256UncompressedLength && b[0] == 4: {
// Point at infinity.
// Uncompressed form.
        ref var r = ref heap(new P256Point(), out var Ꮡr);
        p256BigToLittle(Ꮡr.of(P256Point.Ꮡx), Ꮡ(new array<byte>(b[1..33], 32)));
        p256BigToLittle(Ꮡr.of(P256Point.Ꮡy), Ꮡ(new array<byte>(b[33..65], 32)));
        if (p256LessThanP(Ꮡr.of(P256Point.Ꮡx)) == 0 || p256LessThanP(Ꮡr.of(P256Point.Ꮡy)) == 0) {
            return (default!, errors.New("invalid P256 element encoding"u8));
        }
        p256Mul(Ꮡr.of(P256Point.Ꮡx), Ꮡr.of(P256Point.Ꮡx), Ꮡrr);
        p256Mul(Ꮡr.of(P256Point.Ꮡy), Ꮡr.of(P256Point.Ꮡy), Ꮡrr);
        {
            var err = p256CheckOnCurve(Ꮡr.of(P256Point.Ꮡx), Ꮡr.of(P256Point.Ꮡy)); if (err != default!) {
                return (default!, err);
            }
        }
        r.z = p256One;
        return (Ꮡp.Set(Ꮡr), default!);
    }
    case {} when len(b) == p256CompressedLength && (b[0] == 2 || b[0] == 3): {
// Compressed form.
        ref var r = ref heap(new P256Point(), out var Ꮡr);
        p256BigToLittle(Ꮡr.of(P256Point.Ꮡx), Ꮡ(new array<byte>(b[1..33], 32)));
        if (p256LessThanP(Ꮡr.of(P256Point.Ꮡx)) == 0) {
            return (default!, errors.New("invalid P256 element encoding"u8));
        }
        p256Mul(Ꮡr.of(P256Point.Ꮡx), Ꮡr.of(P256Point.Ꮡx), Ꮡrr);
        p256Polynomial(Ꮡr.of(P256Point.Ꮡy), // y² = x³ - 3x + b
 Ꮡr.of(P256Point.Ꮡx));
        if (!p256Sqrt(Ꮡr.of(P256Point.Ꮡy), Ꮡr.of(P256Point.Ꮡy))) {
            return (default!, errors.New("invalid P256 compressed point encoding"u8));
        }
        var yy = @new<p256Element>();
        p256FromMont(yy, // Select the positive or negative root, as indicated by the least
 // significant bit, based on the encoding type byte.
 Ꮡr.of(P256Point.Ꮡy));
        nint cond = (nint)((nint)((uint64)(yy.Value[0] & 1)) ^ (nint)((byte)(b[0] & 1)));
        p256NegCond(Ꮡr.of(P256Point.Ꮡy), cond);
        r.z = p256One;
        return (Ꮡp.Set(Ꮡr), default!);
    }
    default: {
        return (default!, errors.New("invalid P256 point encoding"u8));
    }}

}

// p256Polynomial sets y2 to x³ - 3x + b, and returns y2.
internal static ж<p256Element> p256Polynomial(ж<p256Element> Ꮡy2, ж<p256Element> Ꮡx) {
    ref var y2 = ref Ꮡy2.Value;
    ref var x = ref Ꮡx.Value;

    var x3 = @new<p256Element>();
    p256Sqr(x3, Ꮡx, 1);
    p256Mul(x3, x3, Ꮡx);
    var threeX = @new<p256Element>();
    p256Add(threeX, Ꮡx, Ꮡx);
    p256Add(threeX, threeX, Ꮡx);
    p256NegCond(threeX, 1);
    var p256B = Ꮡ(new p256Element(new uint64[]{(nuint)0xd89cdf6229c4bddfUL, (nuint)0xacf005cd78843090UL,
        (nuint)0xe5a220abf7212ed6UL, (nuint)0xdc30061d04874834UL}.array()));
    p256Add(x3, x3, threeX);
    p256Add(x3, x3, p256B);
    y2 = x3.Value;
    return Ꮡy2;
}

internal static error p256CheckOnCurve(ж<p256Element> Ꮡx, ж<p256Element> Ꮡy) {
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    // y² = x³ - 3x + b
    var rhs = p256Polynomial(@new<p256Element>(), Ꮡx);
    var lhs = @new<p256Element>();
    p256Sqr(lhs, Ꮡy, 1);
    if (p256Equal(lhs, rhs) != 1) {
        return errors.New("P256 point not on curve"u8);
    }
    return default!;
}

// p256LessThanP returns 1 if x < p, and 0 otherwise. Note that a p256Element is
// not allowed to be equal to or greater than p, so if this function returns 0
// then x is invalid.
internal static nint p256LessThanP(ж<p256Element> Ꮡx) {
    ref var x = ref Ꮡx.Value;

    uint64 b = default!;
    (_, b) = bits.Sub64(x[0], p256P[0], b);
    (_, b) = bits.Sub64(x[1], p256P[1], b);
    (_, b) = bits.Sub64(x[2], p256P[2], b);
    (_, b) = bits.Sub64(x[3], p256P[3], b);
    return (nint)b;
}

// p256Add sets res = x + y.
internal static void p256Add(ж<p256Element> Ꮡres, ж<p256Element> Ꮡx, ж<p256Element> Ꮡy) {
    ref var res = ref Ꮡres.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    uint64 c = default!;
    uint64 b = default!;
    var t1 = new slice<uint64>(4);
    (t1[0], c) = bits.Add64(x[0], y[0], 0);
    (t1[1], c) = bits.Add64(x[1], y[1], c);
    (t1[2], c) = bits.Add64(x[2], y[2], c);
    (t1[3], c) = bits.Add64(x[3], y[3], c);
    var t2 = new slice<uint64>(4);
    (t2[0], b) = bits.Sub64(t1[0], p256P[0], 0);
    (t2[1], b) = bits.Sub64(t1[1], p256P[1], b);
    (t2[2], b) = bits.Sub64(t1[2], p256P[2], b);
    (t2[3], b) = bits.Sub64(t1[3], p256P[3], b);
    // Three options:
    //   - a+b < p
    //     then c is 0, b is 1, and t1 is correct
    //   - p <= a+b < 2^256
    //     then c is 0, b is 0, and t2 is correct
    //   - 2^256 <= a+b
    //     then c is 1, b is 1, and t2 is correct
    var t2Mask = ((uint64)(c ^ b)) - 1;
    res[0] = (uint64)(((uint64)(t1[0] & ~t2Mask)) | ((uint64)(t2[0] & t2Mask)));
    res[1] = (uint64)(((uint64)(t1[1] & ~t2Mask)) | ((uint64)(t2[1] & t2Mask)));
    res[2] = (uint64)(((uint64)(t1[2] & ~t2Mask)) | ((uint64)(t2[2] & t2Mask)));
    res[3] = (uint64)(((uint64)(t1[3] & ~t2Mask)) | ((uint64)(t2[3] & t2Mask)));
}

// p256Sqrt sets e to a square root of x. If x is not a square, p256Sqrt returns
// false and e is unchanged. e and x can overlap.
internal static bool /*isSquare*/ p256Sqrt(ж<p256Element> Ꮡe, ж<p256Element> Ꮡx) {
    bool isSquare = default!;

    ref var e = ref Ꮡe.Value;
    ref var x = ref Ꮡx.Value;
    var (t0, t1) = (@new<p256Element>(), @new<p256Element>());
    // Since p = 3 mod 4, exponentiation by (p + 1) / 4 yields a square root candidate.
    //
    // The sequence of 7 multiplications and 253 squarings is derived from the
    // following addition chain generated with github.com/mmcloughlin/addchain v0.4.0.
    //
    //	_10       = 2*1
    //	_11       = 1 + _10
    //	_1100     = _11 << 2
    //	_1111     = _11 + _1100
    //	_11110000 = _1111 << 4
    //	_11111111 = _1111 + _11110000
    //	x16       = _11111111 << 8 + _11111111
    //	x32       = x16 << 16 + x16
    //	return      ((x32 << 32 + 1) << 96 + 1) << 94
    //
    p256Sqr(t0, Ꮡx, 1);
    p256Mul(t0, Ꮡx, t0);
    p256Sqr(t1, t0, 2);
    p256Mul(t0, t0, t1);
    p256Sqr(t1, t0, 4);
    p256Mul(t0, t0, t1);
    p256Sqr(t1, t0, 8);
    p256Mul(t0, t0, t1);
    p256Sqr(t1, t0, 16);
    p256Mul(t0, t0, t1);
    p256Sqr(t0, t0, 32);
    p256Mul(t0, Ꮡx, t0);
    p256Sqr(t0, t0, 96);
    p256Mul(t0, Ꮡx, t0);
    p256Sqr(t0, t0, 94);
    p256Sqr(t1, t0, 1);
    if (p256Equal(t1, Ꮡx) != 1) {
        return false;
    }
    e = t0.Value;
    return true;
}

// The following assembly functions are implemented in p256_asm_*.s

// Montgomery multiplication. Sets res = in1 * in2 * R⁻¹ mod p.
//
//go:noescape
internal static partial void p256Mul(ж<p256Element> res, ж<p256Element> in1, ж<p256Element> in2);

// Montgomery square, repeated n times (n >= 1).
//
//go:noescape
internal static partial void p256Sqr(ж<p256Element> res, ж<p256Element> @in, nint n);

// Montgomery multiplication by R⁻¹, or 1 outside the domain.
// Sets res = in * R⁻¹, bringing res out of the Montgomery domain.
//
//go:noescape
internal static partial void p256FromMont(ж<p256Element> res, ж<p256Element> @in);

// If cond is not 0, sets val = -val mod p.
//
//go:noescape
internal static partial void p256NegCond(ж<p256Element> val, nint cond);

// If cond is 0, sets res = b, otherwise sets res = a.
//
//go:noescape
internal static partial void p256MovCond(ж<P256Point> res, ж<P256Point> a, ж<P256Point> b, nint cond);

//go:noescape
internal static partial void p256BigToLittle(ж<p256Element> res, ж<array<byte>> @in);

//go:noescape
internal static partial void p256LittleToBig(ж<array<byte>> res, ж<p256Element> @in);

//go:noescape
internal static partial void p256OrdBigToLittle(ж<p256OrdElement> res, ж<array<byte>> @in);

//go:noescape
internal static partial void p256OrdLittleToBig(ж<array<byte>> res, ж<p256OrdElement> @in);

[GoType("[16]P256Point")] partial struct p256Table;

// p256Select sets res to the point at index idx in the table.
// idx must be in [0, 15]. It executes in constant time.
//
//go:noescape
internal static partial void p256Select(ж<P256Point> res, ж<p256Table> table, nint idx);

// p256AffinePoint is a point in affine coordinates (x, y). x and y are still
// Montgomery domain elements. The point can't be the point at infinity.
[GoType] partial struct p256AffinePoint {
    internal p256Element x, y;
}

[GoType("[32]p256AffinePoint")] partial struct p256AffineTable;

// p256Precomputed is a series of precomputed multiples of G, the canonical
// generator. The first p256AffineTable contains multiples of G. The second one
// multiples of [2⁶]G, the third one of [2¹²]G, and so on, where each successive
// table is the previous table doubled six times. Six is the width of the
// sliding window used in p256ScalarMult, and having each table already
// pre-doubled lets us avoid the doublings between windows entirely. This table
// MUST NOT be modified, as it aliases into p256PrecomputedEmbed below.
internal static ж<ж<array<p256AffineTable>>> Ꮡp256Precomputed = new(default(ж<array<p256AffineTable>>));
internal static ref ж<array<p256AffineTable>> p256Precomputed => ref Ꮡp256Precomputed.ValueSlot;

//go:embed p256_asm_table.bin
internal static ж<@string> Ꮡp256PrecomputedEmbed = new(default(@string));
internal static ref @string p256PrecomputedEmbed => ref Ꮡp256PrecomputedEmbed.Value;

[GoInit] internal static void init() {
    var p256PrecomputedPtr = (ж<@unsafe.Pointer>)(uintptr)(new @unsafe.Pointer(Ꮡp256PrecomputedEmbed));
    if (runtime.GOARCH == "s390x"u8) {
        ref var newTable = ref heap(new array<uint64>(11008), out var ᏑnewTable);
        foreach (var (i, x) in ((ж<array<array<byte>>>)(uintptr)(p256PrecomputedPtr.Value)).Value) {
            newTable[i] = byteorder.LeUint64(x[..]);
        }
        ref var newTablePtr = ref heap<@unsafe.Pointer>(out var ᏑnewTablePtr);
        newTablePtr = new @unsafe.Pointer(ᏑnewTable);
        p256PrecomputedPtr = ᏑnewTablePtr;
    }
    p256Precomputed = (ж<array<p256AffineTable>>)(uintptr)(p256PrecomputedPtr.Value);
}

// p256SelectAffine sets res to the point at index idx in the table.
// idx must be in [0, 31]. It executes in constant time.
//
//go:noescape
internal static partial void p256SelectAffine(ж<p256AffinePoint> res, ж<p256AffineTable> table, nint idx);

// Point addition with an affine point and constant time conditions.
// If zero is 0, sets res = in2. If sel is 0, sets res = in1.
// If sign is not 0, sets res = in1 + -in2. Otherwise, sets res = in1 + in2
//
//go:noescape
internal static partial void p256PointAddAffineAsm(ж<P256Point> res, ж<P256Point> in1, ж<p256AffinePoint> in2, nint sign, nint sel, nint zero);

// Point addition. Sets res = in1 + in2. Returns one if the two input points
// were equal and zero otherwise. If in1 or in2 are the point at infinity, res
// and the return value are undefined.
//
//go:noescape
internal static partial nint p256PointAddAsm(ж<P256Point> res, ж<P256Point> in1, ж<P256Point> in2);

// Point doubling. Sets res = in + in. in can be the point at infinity.
//
//go:noescape
internal static partial void p256PointDoubleAsm(ж<P256Point> res, ж<P256Point> @in);

[GoType("[4]uint64")] partial struct p256OrdElement;

// p256OrdReduce ensures s is in the range [0, ord(G)-1].
internal static void p256OrdReduce(ж<p256OrdElement> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    // Since 2 * ord(G) > 2²⁵⁶, we can just conditionally subtract ord(G),
    // keeping the result if it doesn't underflow.
    var (t0, b) = bits.Sub64(s[0], (nuint)0xf3b9cac2fc632551UL, 0);
    (var t1, b) = bits.Sub64(s[1], (nuint)0xbce6faada7179e84UL, b);
    (var t2, b) = bits.Sub64(s[2], (nuint)0xffffffffffffffffUL, b);
    (var t3, b) = bits.Sub64(s[3], (nuint)0xffffffff00000000UL, b);
    var tMask = b - 1;
    // zero if subtraction underflowed
    s[0] ^= (uint64)((uint64)(((uint64)(t0 ^ s[0])) & tMask));
    s[1] ^= (uint64)((uint64)(((uint64)(t1 ^ s[1])) & tMask));
    s[2] ^= (uint64)((uint64)(((uint64)(t2 ^ s[2])) & tMask));
    s[3] ^= (uint64)((uint64)(((uint64)(t3 ^ s[3])) & tMask));
}

// Add sets q = p1 + p2, and returns q. The points may overlap.
public static ж<P256Point> Add(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡr1, ж<P256Point> Ꮡr2) {
    ref var q = ref Ꮡq.Value;
    ref var r1 = ref Ꮡr1.Value;
    ref var r2 = ref Ꮡr2.Value;

    ref var sum = ref heap(new P256Point(), out var Ꮡsum);
    ref var @double = ref heap(new P256Point(), out var Ꮡdouble);
    nint r1IsInfinity = Ꮡr1.isInfinity();
    nint r2IsInfinity = Ꮡr2.isInfinity();
    nint pointsEqual = p256PointAddAsm(Ꮡsum, Ꮡr1, Ꮡr2);
    p256PointDoubleAsm(Ꮡdouble, Ꮡr1);
    p256MovCond(Ꮡsum, Ꮡdouble, Ꮡsum, pointsEqual);
    p256MovCond(Ꮡsum, Ꮡr1, Ꮡsum, r2IsInfinity);
    p256MovCond(Ꮡsum, Ꮡr2, Ꮡsum, r1IsInfinity);
    return Ꮡq.Set(Ꮡsum);
}

// Double sets q = p + p, and returns q. The points may overlap.
public static ж<P256Point> Double(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp) {
    ref var q = ref Ꮡq.Value;
    ref var p = ref Ꮡp.Value;

    ref var @double = ref heap(new P256Point(), out var Ꮡdouble);
    p256PointDoubleAsm(Ꮡdouble, Ꮡp);
    return Ꮡq.Set(Ꮡdouble);
}

// ScalarBaseMult sets r = scalar * generator, where scalar is a 32-byte big
// endian value, and returns r. If scalar is not 32 bytes long, ScalarBaseMult
// returns an error and the receiver is unchanged.
public static (ж<P256Point>, error) ScalarBaseMult(this ж<P256Point> Ꮡr, slice<byte> scalar) {
    ref var r = ref Ꮡr.Value;

    if (len(scalar) != 32) {
        return (default!, errors.New("invalid scalar length"u8));
    }
    var scalarReversed = @new<p256OrdElement>();
    p256OrdBigToLittle(scalarReversed, Ꮡ(new array<byte>(scalar, 32)));
    p256OrdReduce(scalarReversed);
    Ꮡr.p256BaseMult(scalarReversed);
    return (Ꮡr, default!);
}

// ScalarMult sets r = scalar * q, where scalar is a 32-byte big endian value,
// and returns r. If scalar is not 32 bytes long, ScalarBaseMult returns an
// error and the receiver is unchanged.
public static (ж<P256Point>, error) ScalarMult(this ж<P256Point> Ꮡr, ж<P256Point> Ꮡq, slice<byte> scalar) {
    ref var r = ref Ꮡr.Value;
    ref var q = ref Ꮡq.Value;

    if (len(scalar) != 32) {
        return (default!, errors.New("invalid scalar length"u8));
    }
    var scalarReversed = @new<p256OrdElement>();
    p256OrdBigToLittle(scalarReversed, Ꮡ(new array<byte>(scalar, 32)));
    p256OrdReduce(scalarReversed);
    Ꮡr.Set(Ꮡq).p256ScalarMult(scalarReversed);
    return (Ꮡr, default!);
}

// uint64IsZero returns 1 if x is zero and zero otherwise.
internal static nint uint64IsZero(uint64 x) {
    x = ~x;
    x &= (uint64)((x >> (int)(32)));
    x &= (uint64)((x >> (int)(16)));
    x &= (uint64)((x >> (int)(8)));
    x &= (uint64)((x >> (int)(4)));
    x &= (uint64)((x >> (int)(2)));
    x &= (uint64)((x >> (int)(1)));
    return (nint)((uint64)(x & 1));
}

// p256Equal returns 1 if a and b are equal and 0 otherwise.
internal static nint p256Equal(ж<p256Element> Ꮡa, ж<p256Element> Ꮡb) {
    ref var a = ref Ꮡa.Value;
    ref var b = ref Ꮡb.Value;

    uint64 acc = default!;
    foreach (var (i, _) in a) {
        acc |= (uint64)((uint64)(a[i] ^ b[i]));
    }
    return uint64IsZero(acc);
}

// isInfinity returns 1 if p is the point at infinity and 0 otherwise.
internal static nint isInfinity(this ж<P256Point> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    return p256Equal(Ꮡp.of(P256Point.Ꮡz), Ꮡp256Zero);
}

// Bytes returns the uncompressed or infinity encoding of p, as specified in
// SEC 1, Version 2.0, Section 2.3.3. Note that the encoding of the point at
// infinity is shorter than all other encodings.
public static slice<byte> Bytes(this ж<P256Point> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var @out = ref heap(new array<byte>(65), out var Ꮡout);
    return Ꮡp.bytes(Ꮡout);
}

internal static slice<byte> bytes(this ж<P256Point> Ꮡp, ж<array<byte>> Ꮡout) {
    ref var p = ref Ꮡp.Value;
    ref var @out = ref Ꮡout.Value;

    // The proper representation of the point at infinity is a single zero byte.
    if (Ꮡp.isInfinity() == 1) {
        return append(@out[..0], (byte)(0));
    }
    var (x, y) = (@new<p256Element>(), @new<p256Element>());
    Ꮡp.affineFromMont(x, y);
    @out[0] = 4;
    // Uncompressed form.
    p256LittleToBig(Ꮡ(new array<byte>(@out[1..33], 32)), x);
    p256LittleToBig(Ꮡ(new array<byte>(@out[33..65], 32)), y);
    return @out[..];
}

// affineFromMont sets (x, y) to the affine coordinates of p, converted out of the
// Montgomery domain.
internal static void affineFromMont(this ж<P256Point> Ꮡp, ж<p256Element> Ꮡx, ж<p256Element> Ꮡy) {
    ref var p = ref Ꮡp.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    p256Inverse(Ꮡy, Ꮡp.of(P256Point.Ꮡz));
    p256Sqr(Ꮡx, Ꮡy, 1);
    p256Mul(Ꮡy, Ꮡy, Ꮡx);
    p256Mul(Ꮡx, Ꮡp.of(P256Point.Ꮡx), Ꮡx);
    p256Mul(Ꮡy, Ꮡp.of(P256Point.Ꮡy), Ꮡy);
    p256FromMont(Ꮡx, Ꮡx);
    p256FromMont(Ꮡy, Ꮡy);
}

// BytesX returns the encoding of the x-coordinate of p, as specified in SEC 1,
// Version 2.0, Section 2.3.5, or an error if p is the point at infinity.
public static (slice<byte>, error) BytesX(this ж<P256Point> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var @out = ref heap(new array<byte>(32), out var Ꮡout);
    return Ꮡp.bytesX(Ꮡout);
}

internal static (slice<byte>, error) bytesX(this ж<P256Point> Ꮡp, ж<array<byte>> Ꮡout) {
    ref var p = ref Ꮡp.Value;
    ref var @out = ref Ꮡout.Value;

    if (Ꮡp.isInfinity() == 1) {
        return (default!, errors.New("P256 point is the point at infinity"u8));
    }
    var x = @new<p256Element>();
    p256Inverse(x, Ꮡp.of(P256Point.Ꮡz));
    p256Sqr(x, x, 1);
    p256Mul(x, Ꮡp.of(P256Point.Ꮡx), x);
    p256FromMont(x, x);
    p256LittleToBig(Ꮡ(new array<byte>(@out[..], 32)), x);
    return (@out[..], default!);
}

// BytesCompressed returns the compressed or infinity encoding of p, as
// specified in SEC 1, Version 2.0, Section 2.3.3. Note that the encoding of the
// point at infinity is shorter than all other encodings.
public static slice<byte> BytesCompressed(this ж<P256Point> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var @out = ref heap(new array<byte>(33), out var Ꮡout);
    return Ꮡp.bytesCompressed(Ꮡout);
}

internal static slice<byte> bytesCompressed(this ж<P256Point> Ꮡp, ж<array<byte>> Ꮡout) {
    ref var p = ref Ꮡp.Value;
    ref var @out = ref Ꮡout.Value;

    if (Ꮡp.isInfinity() == 1) {
        return append(@out[..0], (byte)(0));
    }
    var (x, y) = (@new<p256Element>(), @new<p256Element>());
    Ꮡp.affineFromMont(x, y);
    @out[0] = (byte)(2 | (byte)((uint64)(y.Value[0] & 1)));
    p256LittleToBig(Ꮡ(new array<byte>(@out[1..33], 32)), x);
    return @out[..];
}

// Select sets q to p1 if cond == 1, and to p2 if cond == 0.
public static ж<P256Point> Select(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp1, ж<P256Point> Ꮡp2, nint cond) {
    ref var q = ref Ꮡq.Value;
    ref var p1 = ref Ꮡp1.Value;
    ref var p2 = ref Ꮡp2.Value;

    p256MovCond(Ꮡq, Ꮡp1, Ꮡp2, cond);
    return Ꮡq;
}

// p256Inverse sets out to in⁻¹ mod p. If in is zero, out will be zero.
internal static void p256Inverse(ж<p256Element> Ꮡout, ж<p256Element> Ꮡin) {
    ref var @out = ref Ꮡout.Value;
    ref var @in = ref Ꮡin.Value;

    // Inversion is calculated through exponentiation by p - 2, per Fermat's
    // little theorem.
    //
    // The sequence of 12 multiplications and 255 squarings is derived from the
    // following addition chain generated with github.com/mmcloughlin/addchain
    // v0.4.0.
    //
    //  _10     = 2*1
    //  _11     = 1 + _10
    //  _110    = 2*_11
    //  _111    = 1 + _110
    //  _111000 = _111 << 3
    //  _111111 = _111 + _111000
    //  x12     = _111111 << 6 + _111111
    //  x15     = x12 << 3 + _111
    //  x16     = 2*x15 + 1
    //  x32     = x16 << 16 + x16
    //  i53     = x32 << 15
    //  x47     = x15 + i53
    //  i263    = ((i53 << 17 + 1) << 143 + x47) << 47
    //  return    (x47 + i263) << 2 + 1
    //
    ж<p256Element> z = @new<p256Element>();
    ж<p256Element> t0 = @new<p256Element>();
    ж<p256Element> t1 = @new<p256Element>();
    p256Sqr(z, Ꮡin, 1);
    p256Mul(z, Ꮡin, z);
    p256Sqr(z, z, 1);
    p256Mul(z, Ꮡin, z);
    p256Sqr(t0, z, 3);
    p256Mul(t0, z, t0);
    p256Sqr(t1, t0, 6);
    p256Mul(t0, t0, t1);
    p256Sqr(t0, t0, 3);
    p256Mul(z, z, t0);
    p256Sqr(t0, z, 1);
    p256Mul(t0, Ꮡin, t0);
    p256Sqr(t1, t0, 16);
    p256Mul(t0, t0, t1);
    p256Sqr(t0, t0, 15);
    p256Mul(z, z, t0);
    p256Sqr(t0, t0, 17);
    p256Mul(t0, Ꮡin, t0);
    p256Sqr(t0, t0, 143);
    p256Mul(t0, z, t0);
    p256Sqr(t0, t0, 47);
    p256Mul(z, z, t0);
    p256Sqr(z, z, 2);
    p256Mul(Ꮡout, Ꮡin, z);
}

internal static (nint, nint) boothW5(nuint @in) {
    nuint s = ~(((@in >> (int)(5))) - 1);
    nuint d = (((nuint)1 << (int)(6))) - @in - 1;
    d = (nuint)(((nuint)(d & s)) | ((nuint)(@in & (~s))));
    d = ((d >> (int)(1))) + ((nuint)(d & 1));
    return ((nint)d, (nint)((nuint)(s & 1)));
}

internal static (nint, nint) boothW6(nuint @in) {
    nuint s = ~(((@in >> (int)(6))) - 1);
    nuint d = (((nuint)1 << (int)(7))) - @in - 1;
    d = (nuint)(((nuint)(d & s)) | ((nuint)(@in & (~s))));
    d = ((d >> (int)(1))) + ((nuint)(d & 1));
    return ((nint)d, (nint)((nuint)(s & 1)));
}

internal static void p256BaseMult(this ж<P256Point> Ꮡp, ж<p256OrdElement> Ꮡscalar) {
    ref var p = ref Ꮡp.Value;
    ref var scalar = ref Ꮡscalar.Value;

    ref var t0 = ref heap(new p256AffinePoint(), out var Ꮡt0);
    var wvalue = (uint64)(((scalar[0] << (int)(1))) & 0x7f);
    var (sel, sign) = boothW6((nuint)wvalue);
    p256SelectAffine(Ꮡt0, Ꮡ(p256Precomputed.Value[0]), sel);
    p.x = t0.x;
    p.y = t0.y;
    p.z = p256One;
    p256NegCond(Ꮡp.of(P256Point.Ꮡy), sign);
    nuint index = (nuint)5;
    nint zero = sel;
    for (nint i = 1; i < 43; i++) {
        if (index < 192){
            wvalue = (uint64)((((scalar[index / 64] >> (int)((index % 64)))) + ((scalar[index / 64 + 1] << (int)((64 - (index % 64)))))) & 0x7f);
        } else {
            wvalue = (uint64)(((scalar[index / 64] >> (int)((index % 64)))) & 0x7f);
        }
        index += 6;
        (sel, sign) = boothW6((nuint)wvalue);
        p256SelectAffine(Ꮡt0, Ꮡ(p256Precomputed.Value[i]), sel);
        p256PointAddAffineAsm(Ꮡp, Ꮡp, Ꮡt0, sign, sel, zero);
        zero |= (nint)(sel);
    }
    // If the whole scalar was zero, set to the point at infinity.
    p256MovCond(Ꮡp, Ꮡp, NewP256Point(), zero);
}

internal static void p256ScalarMult(this ж<P256Point> Ꮡp, ж<p256OrdElement> Ꮡscalar) {
    ref var p = ref Ꮡp.Value;
    ref var scalar = ref Ꮡscalar.Value;

    // precomp is a table of precomputed points that stores powers of p
    // from p^1 to p^16.
    ref var precomp = ref heap(new p256Table(), out var Ꮡprecomp);
    ref var t0 = ref heap(new P256Point(), out var Ꮡt0);
    ref var t1 = ref heap(new P256Point(), out var Ꮡt1);
    ref var t2 = ref heap(new P256Point(), out var Ꮡt2);
    ref var t3 = ref heap(new P256Point(), out var Ꮡt3);
    // Prepare the table
    precomp[0] = p;
    // 1
    p256PointDoubleAsm(Ꮡt0, Ꮡp);
    p256PointDoubleAsm(Ꮡt1, Ꮡt0);
    p256PointDoubleAsm(Ꮡt2, Ꮡt1);
    p256PointDoubleAsm(Ꮡt3, Ꮡt2);
    precomp[1] = t0;
    // 2
    precomp[3] = t1;
    // 4
    precomp[7] = t2;
    // 8
    precomp[15] = t3;
    // 16
    p256PointAddAsm(Ꮡt0, Ꮡt0, Ꮡp);
    p256PointAddAsm(Ꮡt1, Ꮡt1, Ꮡp);
    p256PointAddAsm(Ꮡt2, Ꮡt2, Ꮡp);
    precomp[2] = t0;
    // 3
    precomp[4] = t1;
    // 5
    precomp[8] = t2;
    // 9
    p256PointDoubleAsm(Ꮡt0, Ꮡt0);
    p256PointDoubleAsm(Ꮡt1, Ꮡt1);
    precomp[5] = t0;
    // 6
    precomp[9] = t1;
    // 10
    p256PointAddAsm(Ꮡt2, Ꮡt0, Ꮡp);
    p256PointAddAsm(Ꮡt1, Ꮡt1, Ꮡp);
    precomp[6] = t2;
    // 7
    precomp[10] = t1;
    // 11
    p256PointDoubleAsm(Ꮡt0, Ꮡt0);
    p256PointDoubleAsm(Ꮡt2, Ꮡt2);
    precomp[11] = t0;
    // 12
    precomp[13] = t2;
    // 14
    p256PointAddAsm(Ꮡt0, Ꮡt0, Ꮡp);
    p256PointAddAsm(Ꮡt2, Ꮡt2, Ꮡp);
    precomp[12] = t0;
    // 13
    precomp[14] = t2;
    // 15
    // Start scanning the window from top bit
    nuint index = (nuint)254;
    nint sel = default!;
    nint sign = default!;
    var wvalue = (uint64)(((scalar[index / 64] >> (int)((index % 64)))) & 0x3f);
    (sel, _) = boothW5((nuint)wvalue);
    p256Select(Ꮡp, Ꮡprecomp, sel);
    nint zero = sel;
    while (index > 4) {
        index -= 5;
        p256PointDoubleAsm(Ꮡp, Ꮡp);
        p256PointDoubleAsm(Ꮡp, Ꮡp);
        p256PointDoubleAsm(Ꮡp, Ꮡp);
        p256PointDoubleAsm(Ꮡp, Ꮡp);
        p256PointDoubleAsm(Ꮡp, Ꮡp);
        if (index < 192){
            wvalue = (uint64)((((scalar[index / 64] >> (int)((index % 64)))) + ((scalar[index / 64 + 1] << (int)((64 - (index % 64)))))) & 0x3f);
        } else {
            wvalue = (uint64)(((scalar[index / 64] >> (int)((index % 64)))) & 0x3f);
        }
        (sel, sign) = boothW5((nuint)wvalue);
        p256Select(Ꮡt0, Ꮡprecomp, sel);
        p256NegCond(Ꮡt0.of(P256Point.Ꮡy), sign);
        p256PointAddAsm(Ꮡt1, Ꮡp, Ꮡt0);
        p256MovCond(Ꮡt1, Ꮡt1, Ꮡp, sel);
        p256MovCond(Ꮡp, Ꮡt1, Ꮡt0, zero);
        zero |= (nint)(sel);
    }
    p256PointDoubleAsm(Ꮡp, Ꮡp);
    p256PointDoubleAsm(Ꮡp, Ꮡp);
    p256PointDoubleAsm(Ꮡp, Ꮡp);
    p256PointDoubleAsm(Ꮡp, Ꮡp);
    p256PointDoubleAsm(Ꮡp, Ꮡp);
    wvalue = (uint64)(((scalar[0] << (int)(1))) & 0x3f);
    (sel, sign) = boothW5((nuint)wvalue);
    p256Select(Ꮡt0, Ꮡprecomp, sel);
    p256NegCond(Ꮡt0.of(P256Point.Ꮡy), sign);
    p256PointAddAsm(Ꮡt1, Ꮡp, Ꮡt0);
    p256MovCond(Ꮡt1, Ꮡt1, Ꮡp, sel);
    p256MovCond(Ꮡp, Ꮡt1, Ꮡt0, zero);
}

} // end nistec_package
