// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build s390x
// +build s390x

// package elliptic -- go2cs converted at 2022 March 06 22:19:08 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Program Files\Go\src\crypto\elliptic\p256_s390x.go
using subtle = go.crypto.subtle_package;
using cpu = go.@internal.cpu_package;
using big = go.math.big_package;
using @unsafe = go.@unsafe_package;

namespace go.crypto;

public static partial class elliptic_package {

private static readonly var offsetS390xHasVX = @unsafe.Offsetof(cpu.S390X.HasVX);
private static readonly var offsetS390xHasVE1 = @unsafe.Offsetof(cpu.S390X.HasVXE);


private partial struct p256CurveFast {
    public ref ptr<CurveParams> ptr<CurveParams> => ref ptr<CurveParams>_ptr;
}

private partial struct p256Point {
    public array<byte> x;
    public array<byte> y;
    public array<byte> z;
}

private static Curve p256 = default;private static ptr<array<array<p256Point>>> p256PreFast;

//go:noescape
private static void p256MulInternalTrampolineSetup();

//go:noescape
private static void p256SqrInternalTrampolineSetup();

//go:noescape
private static void p256MulInternalVX();

//go:noescape
private static void p256MulInternalVMSL();

//go:noescape
private static void p256SqrInternalVX();

//go:noescape
private static void p256SqrInternalVMSL();

private static void initP256Arch() {
    if (cpu.S390X.HasVX) {>>MARKER:FUNCTION_p256SqrInternalVMSL_BLOCK_PREFIX<<
        p256 = new p256CurveFast(p256Params);
        initTable();
        return ;
    }
    p256 = new p256Curve(p256Params);
    return ;

}

private static ptr<CurveParams> Params(this p256CurveFast curve) {
    return _addr_curve.CurveParams!;
}

// Functions implemented in p256_asm_s390x.s
// Montgomery multiplication modulo P256
//
//go:noescape
private static void p256SqrAsm(slice<byte> res, slice<byte> in1);

//go:noescape
private static void p256MulAsm(slice<byte> res, slice<byte> in1, slice<byte> in2);

// Montgomery square modulo P256
private static void p256Sqr(slice<byte> res, slice<byte> @in) {
    p256SqrAsm(res, in);
}

// Montgomery multiplication by 1
//
//go:noescape
private static void p256FromMont(slice<byte> res, slice<byte> @in);

// iff cond == 1  val <- -val
//
//go:noescape
private static void p256NegCond(ptr<p256Point> val, nint cond);

// if cond == 0 res <- b; else res <- a
//
//go:noescape
private static void p256MovCond(ptr<p256Point> res, ptr<p256Point> a, ptr<p256Point> b, nint cond);

// Constant time table access
//
//go:noescape
private static void p256Select(ptr<p256Point> point, slice<p256Point> table, nint idx);

//go:noescape
private static void p256SelectBase(ptr<p256Point> point, slice<p256Point> table, nint idx);

// Montgomery multiplication modulo Ord(G)
//
//go:noescape
private static void p256OrdMul(slice<byte> res, slice<byte> in1, slice<byte> in2);

// Montgomery square modulo Ord(G), repeated n times
private static void p256OrdSqr(slice<byte> res, slice<byte> @in, nint n) {
    copy(res, in);
    {
        nint i = 0;

        while (i < n) {>>MARKER:FUNCTION_p256OrdMul_BLOCK_PREFIX<<
            p256OrdMul(res, res, res);
            i += 1;
        }
    }

}

// Point add with P2 being affine point
// If sign == 1 -> P2 = -P2
// If sel == 0 -> P3 = P1
// if zero == 0 -> P3 = P2
//
//go:noescape
private static void p256PointAddAffineAsm(ptr<p256Point> P3, ptr<p256Point> P1, ptr<p256Point> P2, nint sign, nint sel, nint zero);

// Point add
//
//go:noescape
private static nint p256PointAddAsm(ptr<p256Point> P3, ptr<p256Point> P1, ptr<p256Point> P2);

//go:noescape
private static void p256PointDoubleAsm(ptr<p256Point> P3, ptr<p256Point> P1);

private static ptr<big.Int> Inverse(this p256CurveFast curve, ptr<big.Int> _addr_k) {
    ref big.Int k = ref _addr_k.val;

    if (k.Cmp(p256Params.N) >= 0) {>>MARKER:FUNCTION_p256PointDoubleAsm_BLOCK_PREFIX<< 
        // This should never happen.
        ptr<big.Int> reducedK = @new<big.Int>().Mod(k, p256Params.N);
        k = reducedK;

    }
    array<array<byte>> table = new array<array<byte>>(15);

    var x = fromBig(_addr_k); 
    // This code operates in the Montgomery domain where R = 2^256 mod n
    // and n is the order of the scalar field. (See initP256 for the
    // value.) Elements in the Montgomery domain take the form a×R and
    // multiplication of x and y in the calculates (x × y × R^-1) mod n. RR
    // is R×R mod n thus the Montgomery multiplication x and RR gives x×R,
    // i.e. converts x into the Montgomery domain. Stored in BigEndian form
    byte RR = new slice<byte>(new byte[] { 0x66, 0xe1, 0x2d, 0x94, 0xf3, 0xd9, 0x56, 0x20, 0x28, 0x45, 0xb2, 0x39, 0x2b, 0x6b, 0xec, 0x59, 0x46, 0x99, 0x79, 0x9c, 0x49, 0xbd, 0x6f, 0xa6, 0x83, 0x24, 0x4c, 0x95, 0xbe, 0x79, 0xee, 0xa2 });

    p256OrdMul(table[0][..], x, RR); 

    // Prepare the table, no need in constant time access, because the
    // power is not a secret. (Entry 0 is never used.)
    {
        nint i__prev1 = i;

        nint i = 2;

        while (i < 16) {>>MARKER:FUNCTION_p256PointAddAsm_BLOCK_PREFIX<<
            p256OrdSqr(table[i - 1][..], table[(i / 2) - 1][..], 1);
            p256OrdMul(table[i][..], table[i - 1][..], table[0][..]);
            i += 2;
        }

        i = i__prev1;
    }

    copy(x, table[14][..]); // f

    p256OrdSqr(x[(int)0..(int)32], x[(int)0..(int)32], 4);
    p256OrdMul(x[(int)0..(int)32], x[(int)0..(int)32], table[14][..]); // ff
    var t = make_slice<byte>(32);
    copy(t, x);

    p256OrdSqr(x, x, 8);
    p256OrdMul(x, x, t); // ffff
    copy(t, x);

    p256OrdSqr(x, x, 16);
    p256OrdMul(x, x, t); // ffffffff
    copy(t, x);

    p256OrdSqr(x, x, 64); // ffffffff0000000000000000
    p256OrdMul(x, x, t); // ffffffff00000000ffffffff
    p256OrdSqr(x, x, 32); // ffffffff00000000ffffffff00000000
    p256OrdMul(x, x, t); // ffffffff00000000ffffffffffffffff

    // Remaining 32 windows
    array<byte> expLo = new array<byte>(new byte[] { 0xb, 0xc, 0xe, 0x6, 0xf, 0xa, 0xa, 0xd, 0xa, 0x7, 0x1, 0x7, 0x9, 0xe, 0x8, 0x4, 0xf, 0x3, 0xb, 0x9, 0xc, 0xa, 0xc, 0x2, 0xf, 0xc, 0x6, 0x3, 0x2, 0x5, 0x4, 0xf });
    {
        nint i__prev1 = i;

        for (i = 0; i < 32; i++) {>>MARKER:FUNCTION_p256PointAddAffineAsm_BLOCK_PREFIX<<
            p256OrdSqr(x, x, 4);
            p256OrdMul(x, x, table[expLo[i] - 1][..]);
        }

        i = i__prev1;
    } 

    // Multiplying by one in the Montgomery domain converts a Montgomery
    // value out of the domain.
    byte one = new slice<byte>(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 });
    p256OrdMul(x, x, one);

    return @new<big.Int>().SetBytes(x);

}

// fromBig converts a *big.Int into a format used by this code.
private static slice<byte> fromBig(ptr<big.Int> _addr_big) {
    ref big.Int big = ref _addr_big.val;
 
    // This could be done a lot more efficiently...
    var res = big.Bytes();
    if (32 == len(res)) {>>MARKER:FUNCTION_p256SelectBase_BLOCK_PREFIX<<
        return res;
    }
    var t = make_slice<byte>(32);
    nint offset = 32 - len(res);
    for (var i = len(res) - 1; i >= 0; i--) {>>MARKER:FUNCTION_p256Select_BLOCK_PREFIX<<
        t[i + offset] = res[i];
    }
    return t;

}

// p256GetMultiplier makes sure byte array will have 32 byte elements, If the scalar
// is equal or greater than the order of the group, it's reduced modulo that order.
private static slice<byte> p256GetMultiplier(slice<byte> @in) {
    ptr<big.Int> n = @new<big.Int>().SetBytes(in);

    if (n.Cmp(p256Params.N) >= 0) {>>MARKER:FUNCTION_p256MovCond_BLOCK_PREFIX<<
        n.Mod(n, p256Params.N);
    }
    return fromBig(n);

}

// p256MulAsm operates in a Montgomery domain with R = 2^256 mod p, where p is the
// underlying field of the curve. (See initP256 for the value.) Thus rr here is
// R×R mod p. See comment in Inverse about how this is used.
private static byte rr = new slice<byte>(new byte[] { 0x00, 0x00, 0x00, 0x04, 0xff, 0xff, 0xff, 0xfd, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xff, 0xfb, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03 });

// (This is one, in the Montgomery domain.)
private static byte one = new slice<byte>(new byte[] { 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 });

private static ptr<big.Int> maybeReduceModP(ptr<big.Int> _addr_@in) {
    ref big.Int @in = ref _addr_@in.val;

    if (@in.Cmp(p256Params.P) < 0) {>>MARKER:FUNCTION_p256NegCond_BLOCK_PREFIX<<
        return _addr_in!;
    }
    return @new<big.Int>().Mod(in, p256Params.P);

}

private static (ptr<big.Int>, ptr<big.Int>) CombinedMult(this p256CurveFast curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> baseScalar, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref big.Int bigX = ref _addr_bigX.val;
    ref big.Int bigY = ref _addr_bigY.val;

    ref p256Point r1 = ref heap(out ptr<p256Point> _addr_r1);    ref p256Point r2 = ref heap(out ptr<p256Point> _addr_r2);

    var scalarReduced = p256GetMultiplier(baseScalar);
    var r1IsInfinity = scalarIsZero(scalarReduced);
    r1.p256BaseMult(scalarReduced);

    copy(r2.x[..], fromBig(_addr_maybeReduceModP(_addr_bigX)));
    copy(r2.y[..], fromBig(_addr_maybeReduceModP(_addr_bigY)));
    copy(r2.z[..], one);
    p256MulAsm(r2.x[..], r2.x[..], rr[..]);
    p256MulAsm(r2.y[..], r2.y[..], rr[..]);

    scalarReduced = p256GetMultiplier(scalar);
    var r2IsInfinity = scalarIsZero(scalarReduced);
    r2.p256ScalarMult(p256GetMultiplier(scalar));

    ref p256Point sum = ref heap(out ptr<p256Point> _addr_sum);    ref p256Point @double = ref heap(out ptr<p256Point> _addr_@double);

    var pointsEqual = p256PointAddAsm(_addr_sum, _addr_r1, _addr_r2);
    p256PointDoubleAsm(_addr_double, _addr_r1);
    p256MovCond(_addr_sum, _addr_double, _addr_sum, pointsEqual);
    p256MovCond(_addr_sum, _addr_r1, _addr_sum, r2IsInfinity);
    p256MovCond(_addr_sum, _addr_r2, _addr_sum, r1IsInfinity);
    return _addr_sum.p256PointToAffine()!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(this p256CurveFast curve, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;

    p256Point r = default;
    r.p256BaseMult(p256GetMultiplier(scalar));
    return _addr_r.p256PointToAffine()!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarMult(this p256CurveFast curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref big.Int bigX = ref _addr_bigX.val;
    ref big.Int bigY = ref _addr_bigY.val;

    p256Point r = default;
    copy(r.x[..], fromBig(_addr_maybeReduceModP(_addr_bigX)));
    copy(r.y[..], fromBig(_addr_maybeReduceModP(_addr_bigY)));
    copy(r.z[..], one);
    p256MulAsm(r.x[..], r.x[..], rr[..]);
    p256MulAsm(r.y[..], r.y[..], rr[..]);
    r.p256ScalarMult(p256GetMultiplier(scalar));
    return _addr_r.p256PointToAffine()!;
}

// scalarIsZero returns 1 if scalar represents the zero value, and zero
// otherwise.
private static nint scalarIsZero(slice<byte> scalar) {
    var b = byte(0);
    foreach (var (_, s) in scalar) {
        b |= s;
    }    return subtle.ConstantTimeByteEq(b, 0);
}

private static (ptr<big.Int>, ptr<big.Int>) p256PointToAffine(this ptr<p256Point> _addr_p) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref p256Point p = ref _addr_p.val;

    var zInv = make_slice<byte>(32);
    var zInvSq = make_slice<byte>(32);

    p256Inverse(zInv, p.z[..]);
    p256Sqr(zInvSq, zInv);
    p256MulAsm(zInv, zInv, zInvSq);

    p256MulAsm(zInvSq, p.x[..], zInvSq);
    p256MulAsm(zInv, p.y[..], zInv);

    p256FromMont(zInvSq, zInvSq);
    p256FromMont(zInv, zInv);

    return (@new<big.Int>().SetBytes(zInvSq), @new<big.Int>().SetBytes(zInv));
}

// p256Inverse sets out to in^-1 mod p.
private static void p256Inverse(slice<byte> @out, slice<byte> @in) {
    array<byte> stack = new array<byte>(6 * 32);
    var p2 = stack[(int)32 * 0..(int)32 * 0 + 32];
    var p4 = stack[(int)32 * 1..(int)32 * 1 + 32];
    var p8 = stack[(int)32 * 2..(int)32 * 2 + 32];
    var p16 = stack[(int)32 * 3..(int)32 * 3 + 32];
    var p32 = stack[(int)32 * 4..(int)32 * 4 + 32];

    p256Sqr(out, in);
    p256MulAsm(p2, out, in); // 3*p

    p256Sqr(out, p2);
    p256Sqr(out, out);
    p256MulAsm(p4, out, p2); // f*p

    p256Sqr(out, p4);
    p256Sqr(out, out);
    p256Sqr(out, out);
    p256Sqr(out, out);
    p256MulAsm(p8, out, p4); // ff*p

    p256Sqr(out, p8);

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 7; i++) {>>MARKER:FUNCTION_p256FromMont_BLOCK_PREFIX<<
            p256Sqr(out, out);
        }

        i = i__prev1;
    }
    p256MulAsm(p16, out, p8); // ffff*p

    p256Sqr(out, p16);
    {
        nint i__prev1 = i;

        for (i = 0; i < 15; i++) {>>MARKER:FUNCTION_p256MulAsm_BLOCK_PREFIX<<
            p256Sqr(out, out);
        }

        i = i__prev1;
    }
    p256MulAsm(p32, out, p16); // ffffffff*p

    p256Sqr(out, p32);

    {
        nint i__prev1 = i;

        for (i = 0; i < 31; i++) {>>MARKER:FUNCTION_p256SqrAsm_BLOCK_PREFIX<<
            p256Sqr(out, out);
        }

        i = i__prev1;
    }
    p256MulAsm(out, out, in);

    {
        nint i__prev1 = i;

        for (i = 0; i < 32 * 4; i++) {>>MARKER:FUNCTION_p256SqrInternalVX_BLOCK_PREFIX<<
            p256Sqr(out, out);
        }

        i = i__prev1;
    }
    p256MulAsm(out, out, p32);

    {
        nint i__prev1 = i;

        for (i = 0; i < 32; i++) {>>MARKER:FUNCTION_p256MulInternalVMSL_BLOCK_PREFIX<<
            p256Sqr(out, out);
        }

        i = i__prev1;
    }
    p256MulAsm(out, out, p32);

    {
        nint i__prev1 = i;

        for (i = 0; i < 16; i++) {>>MARKER:FUNCTION_p256MulInternalVX_BLOCK_PREFIX<<
            p256Sqr(out, out);
        }

        i = i__prev1;
    }
    p256MulAsm(out, out, p16);

    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {>>MARKER:FUNCTION_p256SqrInternalTrampolineSetup_BLOCK_PREFIX<<
            p256Sqr(out, out);
        }

        i = i__prev1;
    }
    p256MulAsm(out, out, p8);

    p256Sqr(out, out);
    p256Sqr(out, out);
    p256Sqr(out, out);
    p256Sqr(out, out);
    p256MulAsm(out, out, p4);

    p256Sqr(out, out);
    p256Sqr(out, out);
    p256MulAsm(out, out, p2);

    p256Sqr(out, out);
    p256Sqr(out, out);
    p256MulAsm(out, out, in);

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

private static (nint, nint) boothW7(nuint @in) {
    nint _p0 = default;
    nint _p0 = default;

    nuint s = ~((in >> 7) - 1);
    nuint d = (1 << 8) - in - 1;
    d = (d & s) | (in & (~s));
    d = (d >> 1) + (d & 1);
    return (int(d), int(s & 1));
}

private static void initTable() {
    p256PreFast = @new<[37][64]p256Point>(); //z coordinate not used
    ref p256Point basePoint = ref heap(new p256Point(x:[32]byte{0x18,0x90,0x5f,0x76,0xa5,0x37,0x55,0xc6,0x79,0xfb,0x73,0x2b,0x77,0x62,0x25,0x10,0x75,0xba,0x95,0xfc,0x5f,0xed,0xb6,0x01,0x79,0xe7,0x30,0xd4,0x18,0xa9,0x14,0x3c},y:[32]byte{0x85,0x71,0xff,0x18,0x25,0x88,0x5d,0x85,0xd2,0xe8,0x86,0x88,0xdd,0x21,0xf3,0x25,0x8b,0x4a,0xb8,0xe4,0xba,0x19,0xe4,0x5c,0xdd,0xf2,0x53,0x57,0xce,0x95,0x56,0x0a},z:[32]byte{0x00,0x00,0x00,0x00,0xff,0xff,0xff,0xfe,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01},), out ptr<p256Point> _addr_basePoint);

    ptr<p256Point> t1 = @new<p256Point>();
    ptr<p256Point> t2 = @new<p256Point>();
    t2.val = basePoint;

    var zInv = make_slice<byte>(32);
    var zInvSq = make_slice<byte>(32);
    for (nint j = 0; j < 64; j++) {>>MARKER:FUNCTION_p256MulInternalTrampolineSetup_BLOCK_PREFIX<<
        t1.val = t2.val;
        for (nint i = 0; i < 37; i++) { 
            // The window size is 7 so we need to double 7 times.
            if (i != 0) {
                for (nint k = 0; k < 7; k++) {
                    p256PointDoubleAsm(t1, t1);
                }
            } 
            // Convert the point to affine form. (Its values are
            // still in Montgomery form however.)
            p256Inverse(zInv, t1.z[..]);
            p256Sqr(zInvSq, zInv);
            p256MulAsm(zInv, zInv, zInvSq);

            p256MulAsm(t1.x[..], t1.x[..], zInvSq);
            p256MulAsm(t1.y[..], t1.y[..], zInv);

            copy(t1.z[..], basePoint.z[..]); 
            // Update the table entry
            copy(p256PreFast[i][j].x[..], t1.x[..]);
            copy(p256PreFast[i][j].y[..], t1.y[..]);

        }
        if (j == 0) {
            p256PointDoubleAsm(t2, _addr_basePoint);
        }
        else
 {
            p256PointAddAsm(t2, t2, _addr_basePoint);
        }
    }

}

private static void p256BaseMult(this ptr<p256Point> _addr_p, slice<byte> scalar) {
    ref p256Point p = ref _addr_p.val;

    var wvalue = (uint(scalar[31]) << 1) & 0xff;
    var (sel, sign) = boothW7(uint(wvalue));
    p256SelectBase(_addr_p, p256PreFast[0][..], sel);
    p256NegCond(_addr_p, sign);

    copy(p.z[..], one[..]);
    ref p256Point t0 = ref heap(out ptr<p256Point> _addr_t0);

    copy(t0.z[..], one[..]);

    var index = uint(6);
    var zero = sel;

    for (nint i = 1; i < 37; i++) {
        if (index < 247) {
            wvalue = ((uint(scalar[31 - index / 8]) >> (int)((index % 8))) + (uint(scalar[31 - index / 8 - 1]) << (int)((8 - (index % 8))))) & 0xff;
        }
        else
 {
            wvalue = (uint(scalar[31 - index / 8]) >> (int)((index % 8))) & 0xff;
        }
        index += 7;
        sel, sign = boothW7(uint(wvalue));
        p256SelectBase(_addr_t0, p256PreFast[i][..], sel);
        p256PointAddAffineAsm(_addr_p, _addr_p, _addr_t0, sign, sel, zero);
        zero |= sel;

    }

}

private static void p256ScalarMult(this ptr<p256Point> _addr_p, slice<byte> scalar) {
    ref p256Point p = ref _addr_p.val;
 
    // precomp is a table of precomputed points that stores powers of p
    // from p^1 to p^16.
    array<p256Point> precomp = new array<p256Point>(16);
    ref p256Point t0 = ref heap(out ptr<p256Point> _addr_t0);    ref p256Point t1 = ref heap(out ptr<p256Point> _addr_t1);    ref p256Point t2 = ref heap(out ptr<p256Point> _addr_t2);    ref p256Point t3 = ref heap(out ptr<p256Point> _addr_t3); 

    // Prepare the table
 

    // Prepare the table
    _addr_precomp[0].val = p.val;

    p256PointDoubleAsm(_addr_t0, _addr_p);
    p256PointDoubleAsm(_addr_t1, _addr_t0);
    p256PointDoubleAsm(_addr_t2, _addr_t1);
    p256PointDoubleAsm(_addr_t3, _addr_t2);
    _addr_precomp[1].val = t0; // 2
    _addr_precomp[3].val = t1; // 4
    _addr_precomp[7].val = t2; // 8
    _addr_precomp[15].val = t3; // 16

    p256PointAddAsm(_addr_t0, _addr_t0, _addr_p);
    p256PointAddAsm(_addr_t1, _addr_t1, _addr_p);
    p256PointAddAsm(_addr_t2, _addr_t2, _addr_p);
    _addr_precomp[2].val = t0; // 3
    _addr_precomp[4].val = t1; // 5
    _addr_precomp[8].val = t2; // 9

    p256PointDoubleAsm(_addr_t0, _addr_t0);
    p256PointDoubleAsm(_addr_t1, _addr_t1);
    _addr_precomp[5].val = t0; // 6
    _addr_precomp[9].val = t1; // 10

    p256PointAddAsm(_addr_t2, _addr_t0, _addr_p);
    p256PointAddAsm(_addr_t1, _addr_t1, _addr_p);
    _addr_precomp[6].val = t2; // 7
    _addr_precomp[10].val = t1; // 11

    p256PointDoubleAsm(_addr_t0, _addr_t0);
    p256PointDoubleAsm(_addr_t2, _addr_t2);
    _addr_precomp[11].val = t0; // 12
    _addr_precomp[13].val = t2; // 14

    p256PointAddAsm(_addr_t0, _addr_t0, _addr_p);
    p256PointAddAsm(_addr_t2, _addr_t2, _addr_p);
    _addr_precomp[12].val = t0; // 13
    _addr_precomp[14].val = t2; // 15

    // Start scanning the window from top bit
    var index = uint(254);
    nint sel = default;    nint sign = default;



    var wvalue = (uint(scalar[31 - index / 8]) >> (int)((index % 8))) & 0x3f;
    sel, _ = boothW5(uint(wvalue));
    p256Select(_addr_p, precomp[..], sel);
    var zero = sel;

    while (index > 4) {
        index -= 5;
        p256PointDoubleAsm(_addr_p, _addr_p);
        p256PointDoubleAsm(_addr_p, _addr_p);
        p256PointDoubleAsm(_addr_p, _addr_p);
        p256PointDoubleAsm(_addr_p, _addr_p);
        p256PointDoubleAsm(_addr_p, _addr_p);

        if (index < 247) {
            wvalue = ((uint(scalar[31 - index / 8]) >> (int)((index % 8))) + (uint(scalar[31 - index / 8 - 1]) << (int)((8 - (index % 8))))) & 0x3f;
        }
        else
 {
            wvalue = (uint(scalar[31 - index / 8]) >> (int)((index % 8))) & 0x3f;
        }
        sel, sign = boothW5(uint(wvalue));

        p256Select(_addr_t0, precomp[..], sel);
        p256NegCond(_addr_t0, sign);
        p256PointAddAsm(_addr_t1, _addr_p, _addr_t0);
        p256MovCond(_addr_t1, _addr_t1, _addr_p, sel);
        p256MovCond(_addr_p, _addr_t1, _addr_t0, zero);
        zero |= sel;

    }

    p256PointDoubleAsm(_addr_p, _addr_p);
    p256PointDoubleAsm(_addr_p, _addr_p);
    p256PointDoubleAsm(_addr_p, _addr_p);
    p256PointDoubleAsm(_addr_p, _addr_p);
    p256PointDoubleAsm(_addr_p, _addr_p);

    wvalue = (uint(scalar[31]) << 1) & 0x3f;
    sel, sign = boothW5(uint(wvalue));

    p256Select(_addr_t0, precomp[..], sel);
    p256NegCond(_addr_t0, sign);
    p256PointAddAsm(_addr_t1, _addr_p, _addr_t0);
    p256MovCond(_addr_t1, _addr_t1, _addr_p, sel);
    p256MovCond(_addr_p, _addr_t1, _addr_t0, zero);

}

} // end elliptic_package
