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

//go:build amd64 || arm64
// +build amd64 arm64

// package elliptic -- go2cs converted at 2022 March 06 22:19:00 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Program Files\Go\src\crypto\elliptic\p256_asm.go
using big = go.math.big_package;

namespace go.crypto;

public static partial class elliptic_package {

private partial struct p256Curve {
    public ref ptr<CurveParams> ptr<CurveParams> => ref ptr<CurveParams>_ptr;
}

private partial struct p256Point {
    public array<ulong> xyz;
}
private static p256Curve p256 = default;

private static void initP256() { 
    // See FIPS 186-3, section D.2.3
    p256.CurveParams = addr(new CurveParams(Name:"P-256"));
    p256.P, _ = @new<big.Int>().SetString("115792089210356248762697446949407573530086143415290314195533631308867097853951", 10);
    p256.N, _ = @new<big.Int>().SetString("115792089210356248762697446949407573529996955224135760342422259061068512044369", 10);
    p256.B, _ = @new<big.Int>().SetString("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b", 16);
    p256.Gx, _ = @new<big.Int>().SetString("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296", 16);
    p256.Gy, _ = @new<big.Int>().SetString("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5", 16);
    p256.BitSize = 256;

}

private static ptr<CurveParams> Params(this p256Curve curve) {
    return _addr_curve.CurveParams!;
}

// Functions implemented in p256_asm_*64.s
// Montgomery multiplication modulo P256
//go:noescape
private static void p256Mul(slice<ulong> res, slice<ulong> in1, slice<ulong> in2);

// Montgomery square modulo P256, repeated n times (n >= 1)
//go:noescape
private static void p256Sqr(slice<ulong> res, slice<ulong> @in, nint n);

// Montgomery multiplication by 1
//go:noescape
private static void p256FromMont(slice<ulong> res, slice<ulong> @in);

// iff cond == 1  val <- -val
//go:noescape
private static void p256NegCond(slice<ulong> val, nint cond);

// if cond == 0 res <- b; else res <- a
//go:noescape
private static void p256MovCond(slice<ulong> res, slice<ulong> a, slice<ulong> b, nint cond);

// Endianness swap
//go:noescape
private static void p256BigToLittle(slice<ulong> res, slice<byte> @in);

//go:noescape
private static void p256LittleToBig(slice<byte> res, slice<ulong> @in);

// Constant time table access
//go:noescape
private static void p256Select(slice<ulong> point, slice<ulong> table, nint idx);

//go:noescape
private static void p256SelectBase(slice<ulong> point, slice<ulong> table, nint idx);

// Montgomery multiplication modulo Ord(G)
//go:noescape
private static void p256OrdMul(slice<ulong> res, slice<ulong> in1, slice<ulong> in2);

// Montgomery square modulo Ord(G), repeated n times
//go:noescape
private static void p256OrdSqr(slice<ulong> res, slice<ulong> @in, nint n);

// Point add with in2 being affine point
// If sign == 1 -> in2 = -in2
// If sel == 0 -> res = in1
// if zero == 0 -> res = in2
//go:noescape
private static void p256PointAddAffineAsm(slice<ulong> res, slice<ulong> in1, slice<ulong> in2, nint sign, nint sel, nint zero);

// Point add. Returns one if the two input points were equal and zero
// otherwise. (Note that, due to the way that the equations work out, some
// representations of ∞ are considered equal to everything by this function.)
//go:noescape
private static nint p256PointAddAsm(slice<ulong> res, slice<ulong> in1, slice<ulong> in2);

// Point double
//go:noescape
private static void p256PointDoubleAsm(slice<ulong> res, slice<ulong> @in);

private static ptr<big.Int> Inverse(this p256Curve curve, ptr<big.Int> _addr_k) {
    ref big.Int k = ref _addr_k.val;

    if (k.Sign() < 0) {>>MARKER:FUNCTION_p256PointDoubleAsm_BLOCK_PREFIX<< 
        // This should never happen.
        k = @new<big.Int>().Neg(k);

    }
    if (k.Cmp(p256.N) >= 0) {>>MARKER:FUNCTION_p256PointAddAsm_BLOCK_PREFIX<< 
        // This should never happen.
        k = @new<big.Int>().Mod(k, p256.N);

    }
    array<ulong> table = new array<ulong>(4 * 9);
    var _1 = table[(int)4 * 0..(int)4 * 1];    var _11 = table[(int)4 * 1..(int)4 * 2];    var _101 = table[(int)4 * 2..(int)4 * 3];    var _111 = table[(int)4 * 3..(int)4 * 4];    var _1111 = table[(int)4 * 4..(int)4 * 5];    var _10101 = table[(int)4 * 5..(int)4 * 6];    var _101111 = table[(int)4 * 6..(int)4 * 7];    var x = table[(int)4 * 7..(int)4 * 8];    var t = table[(int)4 * 8..(int)4 * 9];

    fromBig(x[..], _addr_k); 
    // This code operates in the Montgomery domain where R = 2^256 mod n
    // and n is the order of the scalar field. (See initP256 for the
    // value.) Elements in the Montgomery domain take the form a×R and
    // multiplication of x and y in the calculates (x × y × R^-1) mod n. RR
    // is R×R mod n thus the Montgomery multiplication x and RR gives x×R,
    // i.e. converts x into the Montgomery domain.
    // Window values borrowed from https://briansmith.org/ecc-inversion-addition-chains-01#p256_scalar_inversion
    ulong RR = new slice<ulong>(new ulong[] { 0x83244c95be79eea2, 0x4699799c49bd6fa6, 0x2845b2392b6bec59, 0x66e12d94f3d95620 });
    p256OrdMul(_1, x, RR); // _1
    p256OrdSqr(x, _1, 1); // _10
    p256OrdMul(_11, x, _1); // _11
    p256OrdMul(_101, x, _11); // _101
    p256OrdMul(_111, x, _101); // _111
    p256OrdSqr(x, _101, 1); // _1010
    p256OrdMul(_1111, _101, x); // _1111

    p256OrdSqr(t, x, 1); // _10100
    p256OrdMul(_10101, t, _1); // _10101
    p256OrdSqr(x, _10101, 1); // _101010
    p256OrdMul(_101111, _101, x); // _101111
    p256OrdMul(x, _10101, x); // _111111 = x6
    p256OrdSqr(t, x, 2); // _11111100
    p256OrdMul(t, t, _11); // _11111111 = x8
    p256OrdSqr(x, t, 8); // _ff00
    p256OrdMul(x, x, t); // _ffff = x16
    p256OrdSqr(t, x, 16); // _ffff0000
    p256OrdMul(t, t, x); // _ffffffff = x32

    p256OrdSqr(x, t, 64);
    p256OrdMul(x, x, t);
    p256OrdSqr(x, x, 32);
    p256OrdMul(x, x, t);

    byte sqrs = new slice<byte>(new byte[] { 6, 5, 4, 5, 5, 4, 3, 3, 5, 9, 6, 2, 5, 6, 5, 4, 5, 5, 3, 10, 2, 5, 5, 3, 7, 6 });
    slice<ulong> muls = new slice<slice<ulong>>(new slice<ulong>[] { _101111, _111, _11, _1111, _10101, _101, _101, _101, _111, _101111, _1111, _1, _1, _1111, _111, _111, _111, _101, _11, _101111, _11, _11, _11, _1, _10101, _1111 });

    foreach (var (i, s) in sqrs) {
        p256OrdSqr(x, x, int(s));
        p256OrdMul(x, x, muls[i]);
    }    ulong one = new slice<ulong>(new ulong[] { 1, 0, 0, 0 });
    p256OrdMul(x, x, one);

    var xOut = make_slice<byte>(32);
    p256LittleToBig(xOut, x);
    return @new<big.Int>().SetBytes(xOut);

}

// fromBig converts a *big.Int into a format used by this code.
private static void fromBig(slice<ulong> @out, ptr<big.Int> _addr_big) {
    ref big.Int big = ref _addr_big.val;

    {
        var i__prev1 = i;

        foreach (var (__i) in out) {
            i = __i;
            out[i] = 0;
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i, __v) in big.Bits()) {
            i = __i;
            v = __v;
            out[i] = uint64(v);
        }
        i = i__prev1;
    }
}

// p256GetScalar endian-swaps the big-endian scalar value from in and writes it
// to out. If the scalar is equal or greater than the order of the group, it's
// reduced modulo that order.
private static void p256GetScalar(slice<ulong> @out, slice<byte> @in) {
    ptr<big.Int> n = @new<big.Int>().SetBytes(in);

    if (n.Cmp(p256.N) >= 0) {>>MARKER:FUNCTION_p256PointAddAffineAsm_BLOCK_PREFIX<<
        n.Mod(n, p256.N);
    }
    fromBig(out, n);

}

// p256Mul operates in a Montgomery domain with R = 2^256 mod p, where p is the
// underlying field of the curve. (See initP256 for the value.) Thus rr here is
// R×R mod p. See comment in Inverse about how this is used.
private static ulong rr = new slice<ulong>(new ulong[] { 0x0000000000000003, 0xfffffffbffffffff, 0xfffffffffffffffe, 0x00000004fffffffd });

private static ptr<big.Int> maybeReduceModP(ptr<big.Int> _addr_@in) {
    ref big.Int @in = ref _addr_@in.val;

    if (@in.Cmp(p256.P) < 0) {>>MARKER:FUNCTION_p256OrdSqr_BLOCK_PREFIX<<
        return _addr_in!;
    }
    return @new<big.Int>().Mod(in, p256.P);

}

private static (ptr<big.Int>, ptr<big.Int>) CombinedMult(this p256Curve curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> baseScalar, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref big.Int bigX = ref _addr_bigX.val;
    ref big.Int bigY = ref _addr_bigY.val;

    var scalarReversed = make_slice<ulong>(4);
    ref p256Point r1 = ref heap(out ptr<p256Point> _addr_r1);    ref p256Point r2 = ref heap(out ptr<p256Point> _addr_r2);

    p256GetScalar(scalarReversed, baseScalar);
    var r1IsInfinity = scalarIsZero(scalarReversed);
    r1.p256BaseMult(scalarReversed);

    p256GetScalar(scalarReversed, scalar);
    var r2IsInfinity = scalarIsZero(scalarReversed);
    fromBig(r2.xyz[(int)0..(int)4], _addr_maybeReduceModP(_addr_bigX));
    fromBig(r2.xyz[(int)4..(int)8], _addr_maybeReduceModP(_addr_bigY));
    p256Mul(r2.xyz[(int)0..(int)4], r2.xyz[(int)0..(int)4], rr[..]);
    p256Mul(r2.xyz[(int)4..(int)8], r2.xyz[(int)4..(int)8], rr[..]); 

    // This sets r2's Z value to 1, in the Montgomery domain.
    r2.xyz[8] = 0x0000000000000001;
    r2.xyz[9] = 0xffffffff00000000;
    r2.xyz[10] = 0xffffffffffffffff;
    r2.xyz[11] = 0x00000000fffffffe;

    r2.p256ScalarMult(scalarReversed);

    p256Point sum = default;    ref p256Point @double = ref heap(out ptr<p256Point> _addr_@double);

    var pointsEqual = p256PointAddAsm(sum.xyz[..], r1.xyz[..], r2.xyz[..]);
    p256PointDoubleAsm(@double.xyz[..], r1.xyz[..]);
    sum.CopyConditional(_addr_double, pointsEqual);
    sum.CopyConditional(_addr_r1, r2IsInfinity);
    sum.CopyConditional(_addr_r2, r1IsInfinity);

    return _addr_sum.p256PointToAffine()!;

}

private static (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(this p256Curve curve, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;

    var scalarReversed = make_slice<ulong>(4);
    p256GetScalar(scalarReversed, scalar);

    p256Point r = default;
    r.p256BaseMult(scalarReversed);
    return _addr_r.p256PointToAffine()!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarMult(this p256Curve curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref big.Int bigX = ref _addr_bigX.val;
    ref big.Int bigY = ref _addr_bigY.val;

    var scalarReversed = make_slice<ulong>(4);
    p256GetScalar(scalarReversed, scalar);

    p256Point r = default;
    fromBig(r.xyz[(int)0..(int)4], _addr_maybeReduceModP(_addr_bigX));
    fromBig(r.xyz[(int)4..(int)8], _addr_maybeReduceModP(_addr_bigY));
    p256Mul(r.xyz[(int)0..(int)4], r.xyz[(int)0..(int)4], rr[..]);
    p256Mul(r.xyz[(int)4..(int)8], r.xyz[(int)4..(int)8], rr[..]); 
    // This sets r2's Z value to 1, in the Montgomery domain.
    r.xyz[8] = 0x0000000000000001;
    r.xyz[9] = 0xffffffff00000000;
    r.xyz[10] = 0xffffffffffffffff;
    r.xyz[11] = 0x00000000fffffffe;

    r.p256ScalarMult(scalarReversed);
    return _addr_r.p256PointToAffine()!;

}

// uint64IsZero returns 1 if x is zero and zero otherwise.
private static nint uint64IsZero(ulong x) {
    x = ~x;
    x &= x >> 32;
    x &= x >> 16;
    x &= x >> 8;
    x &= x >> 4;
    x &= x >> 2;
    x &= x >> 1;
    return int(x & 1);
}

// scalarIsZero returns 1 if scalar represents the zero value, and zero
// otherwise.
private static nint scalarIsZero(slice<ulong> scalar) {
    return uint64IsZero(scalar[0] | scalar[1] | scalar[2] | scalar[3]);
}

private static (ptr<big.Int>, ptr<big.Int>) p256PointToAffine(this ptr<p256Point> _addr_p) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref p256Point p = ref _addr_p.val;

    var zInv = make_slice<ulong>(4);
    var zInvSq = make_slice<ulong>(4);
    p256Inverse(zInv, p.xyz[(int)8..(int)12]);
    p256Sqr(zInvSq, zInv, 1);
    p256Mul(zInv, zInv, zInvSq);

    p256Mul(zInvSq, p.xyz[(int)0..(int)4], zInvSq);
    p256Mul(zInv, p.xyz[(int)4..(int)8], zInv);

    p256FromMont(zInvSq, zInvSq);
    p256FromMont(zInv, zInv);

    var xOut = make_slice<byte>(32);
    var yOut = make_slice<byte>(32);
    p256LittleToBig(xOut, zInvSq);
    p256LittleToBig(yOut, zInv);

    return (@new<big.Int>().SetBytes(xOut), @new<big.Int>().SetBytes(yOut));
}

// CopyConditional copies overwrites p with src if v == 1, and leaves p
// unchanged if v == 0.
private static void CopyConditional(this ptr<p256Point> _addr_p, ptr<p256Point> _addr_src, nint v) {
    ref p256Point p = ref _addr_p.val;
    ref p256Point src = ref _addr_src.val;

    var pMask = uint64(v) - 1;
    var srcMask = ~pMask;

    foreach (var (i, n) in p.xyz) {
        p.xyz[i] = (n & pMask) | (src.xyz[i] & srcMask);
    }
}

// p256Inverse sets out to in^-1 mod p.
private static void p256Inverse(slice<ulong> @out, slice<ulong> @in) {
    array<ulong> stack = new array<ulong>(6 * 4);
    var p2 = stack[(int)4 * 0..(int)4 * 0 + 4];
    var p4 = stack[(int)4 * 1..(int)4 * 1 + 4];
    var p8 = stack[(int)4 * 2..(int)4 * 2 + 4];
    var p16 = stack[(int)4 * 3..(int)4 * 3 + 4];
    var p32 = stack[(int)4 * 4..(int)4 * 4 + 4];

    p256Sqr(out, in, 1);
    p256Mul(p2, out, in); // 3*p

    p256Sqr(out, p2, 2);
    p256Mul(p4, out, p2); // f*p

    p256Sqr(out, p4, 4);
    p256Mul(p8, out, p4); // ff*p

    p256Sqr(out, p8, 8);
    p256Mul(p16, out, p8); // ffff*p

    p256Sqr(out, p16, 16);
    p256Mul(p32, out, p16); // ffffffff*p

    p256Sqr(out, p32, 32);
    p256Mul(out, out, in);

    p256Sqr(out, out, 128);
    p256Mul(out, out, p32);

    p256Sqr(out, out, 32);
    p256Mul(out, out, p32);

    p256Sqr(out, out, 16);
    p256Mul(out, out, p16);

    p256Sqr(out, out, 8);
    p256Mul(out, out, p8);

    p256Sqr(out, out, 4);
    p256Mul(out, out, p4);

    p256Sqr(out, out, 2);
    p256Mul(out, out, p2);

    p256Sqr(out, out, 2);
    p256Mul(out, out, in);

}

private static void p256StorePoint(this ptr<p256Point> _addr_p, ptr<array<ulong>> _addr_r, nint index) {
    ref p256Point p = ref _addr_p.val;
    ref array<ulong> r = ref _addr_r.val;

    copy(r[(int)index * 12..], p.xyz[..]);
}

private static (nint, nint) boothW5(nuint @in) {
    nint _p0 = default;
    nint _p0 = default;

    nuint s = ~((in >> 5) - 1);
    nuint d = (1 << 6) - in - 1;
    d = (d & s) | (in & (~s));
    d = (d >> 1) + (d & 1);
    return (int(d), int(s & 1));
}

private static (nint, nint) boothW6(nuint @in) {
    nint _p0 = default;
    nint _p0 = default;

    nuint s = ~((in >> 6) - 1);
    nuint d = (1 << 7) - in - 1;
    d = (d & s) | (in & (~s));
    d = (d >> 1) + (d & 1);
    return (int(d), int(s & 1));
}

private static void p256BaseMult(this ptr<p256Point> _addr_p, slice<ulong> scalar) {
    ref p256Point p = ref _addr_p.val;

    var wvalue = (scalar[0] << 1) & 0x7f;
    var (sel, sign) = boothW6(uint(wvalue));
    p256SelectBase(p.xyz[(int)0..(int)8], p256Precomputed[0][(int)0..], sel);
    p256NegCond(p.xyz[(int)4..(int)8], sign); 

    // (This is one, in the Montgomery domain.)
    p.xyz[8] = 0x0000000000000001;
    p.xyz[9] = 0xffffffff00000000;
    p.xyz[10] = 0xffffffffffffffff;
    p.xyz[11] = 0x00000000fffffffe;

    p256Point t0 = default; 
    // (This is one, in the Montgomery domain.)
    t0.xyz[8] = 0x0000000000000001;
    t0.xyz[9] = 0xffffffff00000000;
    t0.xyz[10] = 0xffffffffffffffff;
    t0.xyz[11] = 0x00000000fffffffe;

    var index = uint(5);
    var zero = sel;

    for (nint i = 1; i < 43; i++) {>>MARKER:FUNCTION_p256OrdMul_BLOCK_PREFIX<<
        if (index < 192) {>>MARKER:FUNCTION_p256SelectBase_BLOCK_PREFIX<<
            wvalue = ((scalar[index / 64] >> (int)((index % 64))) + (scalar[index / 64 + 1] << (int)((64 - (index % 64))))) & 0x7f;
        }
        else
 {>>MARKER:FUNCTION_p256Select_BLOCK_PREFIX<<
            wvalue = (scalar[index / 64] >> (int)((index % 64))) & 0x7f;
        }
        index += 6;
        sel, sign = boothW6(uint(wvalue));
        p256SelectBase(t0.xyz[(int)0..(int)8], p256Precomputed[i][(int)0..], sel);
        p256PointAddAffineAsm(p.xyz[(int)0..(int)12], p.xyz[(int)0..(int)12], t0.xyz[(int)0..(int)8], sign, sel, zero);
        zero |= sel;

    }

}

private static void p256ScalarMult(this ptr<p256Point> _addr_p, slice<ulong> scalar) {
    ref p256Point p = ref _addr_p.val;
 
    // precomp is a table of precomputed points that stores powers of p
    // from p^1 to p^16.
    ref array<ulong> precomp = ref heap(new array<ulong>(16 * 4 * 3), out ptr<array<ulong>> _addr_precomp);
    p256Point t0 = default;    p256Point t1 = default;    p256Point t2 = default;    p256Point t3 = default; 

    // Prepare the table
 

    // Prepare the table
    p.p256StorePoint(_addr_precomp, 0); // 1

    p256PointDoubleAsm(t0.xyz[..], p.xyz[..]);
    p256PointDoubleAsm(t1.xyz[..], t0.xyz[..]);
    p256PointDoubleAsm(t2.xyz[..], t1.xyz[..]);
    p256PointDoubleAsm(t3.xyz[..], t2.xyz[..]);
    t0.p256StorePoint(_addr_precomp, 1); // 2
    t1.p256StorePoint(_addr_precomp, 3); // 4
    t2.p256StorePoint(_addr_precomp, 7); // 8
    t3.p256StorePoint(_addr_precomp, 15); // 16

    p256PointAddAsm(t0.xyz[..], t0.xyz[..], p.xyz[..]);
    p256PointAddAsm(t1.xyz[..], t1.xyz[..], p.xyz[..]);
    p256PointAddAsm(t2.xyz[..], t2.xyz[..], p.xyz[..]);
    t0.p256StorePoint(_addr_precomp, 2); // 3
    t1.p256StorePoint(_addr_precomp, 4); // 5
    t2.p256StorePoint(_addr_precomp, 8); // 9

    p256PointDoubleAsm(t0.xyz[..], t0.xyz[..]);
    p256PointDoubleAsm(t1.xyz[..], t1.xyz[..]);
    t0.p256StorePoint(_addr_precomp, 5); // 6
    t1.p256StorePoint(_addr_precomp, 9); // 10

    p256PointAddAsm(t2.xyz[..], t0.xyz[..], p.xyz[..]);
    p256PointAddAsm(t1.xyz[..], t1.xyz[..], p.xyz[..]);
    t2.p256StorePoint(_addr_precomp, 6); // 7
    t1.p256StorePoint(_addr_precomp, 10); // 11

    p256PointDoubleAsm(t0.xyz[..], t0.xyz[..]);
    p256PointDoubleAsm(t2.xyz[..], t2.xyz[..]);
    t0.p256StorePoint(_addr_precomp, 11); // 12
    t2.p256StorePoint(_addr_precomp, 13); // 14

    p256PointAddAsm(t0.xyz[..], t0.xyz[..], p.xyz[..]);
    p256PointAddAsm(t2.xyz[..], t2.xyz[..], p.xyz[..]);
    t0.p256StorePoint(_addr_precomp, 12); // 13
    t2.p256StorePoint(_addr_precomp, 14); // 15

    // Start scanning the window from top bit
    var index = uint(254);
    nint sel = default;    nint sign = default;



    var wvalue = (scalar[index / 64] >> (int)((index % 64))) & 0x3f;
    sel, _ = boothW5(uint(wvalue));

    p256Select(p.xyz[(int)0..(int)12], precomp[(int)0..], sel);
    var zero = sel;

    while (index > 4) {>>MARKER:FUNCTION_p256LittleToBig_BLOCK_PREFIX<<
        index -= 5;
        p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
        p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
        p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
        p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
        p256PointDoubleAsm(p.xyz[..], p.xyz[..]);

        if (index < 192) {>>MARKER:FUNCTION_p256BigToLittle_BLOCK_PREFIX<<
            wvalue = ((scalar[index / 64] >> (int)((index % 64))) + (scalar[index / 64 + 1] << (int)((64 - (index % 64))))) & 0x3f;
        }
        else
 {>>MARKER:FUNCTION_p256MovCond_BLOCK_PREFIX<<
            wvalue = (scalar[index / 64] >> (int)((index % 64))) & 0x3f;
        }
        sel, sign = boothW5(uint(wvalue));

        p256Select(t0.xyz[(int)0..], precomp[(int)0..], sel);
        p256NegCond(t0.xyz[(int)4..(int)8], sign);
        p256PointAddAsm(t1.xyz[..], p.xyz[..], t0.xyz[..]);
        p256MovCond(t1.xyz[(int)0..(int)12], t1.xyz[(int)0..(int)12], p.xyz[(int)0..(int)12], sel);
        p256MovCond(p.xyz[(int)0..(int)12], t1.xyz[(int)0..(int)12], t0.xyz[(int)0..(int)12], zero);
        zero |= sel;

    }

    p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
    p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
    p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
    p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
    p256PointDoubleAsm(p.xyz[..], p.xyz[..]);

    wvalue = (scalar[0] << 1) & 0x3f;
    sel, sign = boothW5(uint(wvalue));

    p256Select(t0.xyz[(int)0..], precomp[(int)0..], sel);
    p256NegCond(t0.xyz[(int)4..(int)8], sign);
    p256PointAddAsm(t1.xyz[..], p.xyz[..], t0.xyz[..]);
    p256MovCond(t1.xyz[(int)0..(int)12], t1.xyz[(int)0..(int)12], p.xyz[(int)0..(int)12], sel);
    p256MovCond(p.xyz[(int)0..(int)12], t1.xyz[(int)0..(int)12], t0.xyz[(int)0..(int)12], zero);

}

} // end elliptic_package
