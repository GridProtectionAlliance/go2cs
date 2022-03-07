// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package elliptic -- go2cs converted at 2022 March 06 22:18:23 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Program Files\Go\src\crypto\elliptic\p224.go
// This is a constant-time, 32-bit implementation of P224. See FIPS 186-3,
// section D.2.2.
//
// See https://www.imperialviolet.org/2010/12/04/ecc.html ([1]) for background.

using big = go.math.big_package;

namespace go.crypto;

public static partial class elliptic_package {

private static p224Curve p224 = default;

private partial struct p224Curve {
    public ref ptr<CurveParams> ptr<CurveParams> => ref ptr<CurveParams>_ptr;
    public p224FieldElement gx;
    public p224FieldElement gy;
    public p224FieldElement b;
}

private static void initP224() { 
    // See FIPS 186-3, section D.2.2
    p224.CurveParams = addr(new CurveParams(Name:"P-224"));
    p224.P, _ = @new<big.Int>().SetString("26959946667150639794667015087019630673557916260026308143510066298881", 10);
    p224.N, _ = @new<big.Int>().SetString("26959946667150639794667015087019625940457807714424391721682722368061", 10);
    p224.B, _ = @new<big.Int>().SetString("b4050a850c04b3abf54132565044b0b7d7bfd8ba270b39432355ffb4", 16);
    p224.Gx, _ = @new<big.Int>().SetString("b70e0cbd6bb4bf7f321390b94a03c1d356c21122343280d6115c1d21", 16);
    p224.Gy, _ = @new<big.Int>().SetString("bd376388b5f723fb4c22dfe6cd4375a05a07476444d5819985007e34", 16);
    p224.BitSize = 224;

    p224FromBig(_addr_p224.gx, _addr_p224.Gx);
    p224FromBig(_addr_p224.gy, _addr_p224.Gy);
    p224FromBig(_addr_p224.b, _addr_p224.B);

}

// P224 returns a Curve which implements P-224 (see FIPS 186-3, section D.2.2).
//
// The cryptographic operations are implemented using constant-time algorithms.
public static Curve P224() {
    initonce.Do(initAll);
    return p224;
}

private static ptr<CurveParams> Params(this p224Curve curve) {
    return _addr_curve.CurveParams!;
}

private static bool IsOnCurve(this p224Curve curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY) {
    ref big.Int bigX = ref _addr_bigX.val;
    ref big.Int bigY = ref _addr_bigY.val;

    ref p224FieldElement x = ref heap(out ptr<p224FieldElement> _addr_x);    ref p224FieldElement y = ref heap(out ptr<p224FieldElement> _addr_y);

    p224FromBig(_addr_x, _addr_bigX);
    p224FromBig(_addr_y, _addr_bigY); 

    // y² = x³ - 3x + b
    ref p224LargeFieldElement tmp = ref heap(out ptr<p224LargeFieldElement> _addr_tmp);
    ref p224FieldElement x3 = ref heap(out ptr<p224FieldElement> _addr_x3);
    p224Square(_addr_x3, _addr_x, _addr_tmp);
    p224Mul(_addr_x3, _addr_x3, _addr_x, _addr_tmp);

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 8; i++) {
            x[i] *= 3;
        }

        i = i__prev1;
    }
    p224Sub(_addr_x3, _addr_x3, _addr_x);
    p224Reduce(_addr_x3);
    p224Add(_addr_x3, _addr_x3, _addr_curve.b);
    p224Contract(_addr_x3, _addr_x3);

    p224Square(_addr_y, _addr_y, _addr_tmp);
    p224Contract(_addr_y, _addr_y);

    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            if (y[i] != x3[i]) {
                return false;
            }
        }

        i = i__prev1;
    }
    return true;

}

private static (ptr<big.Int>, ptr<big.Int>) Add(this p224Curve _p0, ptr<big.Int> _addr_bigX1, ptr<big.Int> _addr_bigY1, ptr<big.Int> _addr_bigX2, ptr<big.Int> _addr_bigY2) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref big.Int bigX1 = ref _addr_bigX1.val;
    ref big.Int bigY1 = ref _addr_bigY1.val;
    ref big.Int bigX2 = ref _addr_bigX2.val;
    ref big.Int bigY2 = ref _addr_bigY2.val;

    ref p224FieldElement x1 = ref heap(out ptr<p224FieldElement> _addr_x1);    ref p224FieldElement y1 = ref heap(out ptr<p224FieldElement> _addr_y1);    ref p224FieldElement z1 = ref heap(out ptr<p224FieldElement> _addr_z1);    ref p224FieldElement x2 = ref heap(out ptr<p224FieldElement> _addr_x2);    ref p224FieldElement y2 = ref heap(out ptr<p224FieldElement> _addr_y2);    ref p224FieldElement z2 = ref heap(out ptr<p224FieldElement> _addr_z2);    ref p224FieldElement x3 = ref heap(out ptr<p224FieldElement> _addr_x3);    ref p224FieldElement y3 = ref heap(out ptr<p224FieldElement> _addr_y3);    ref p224FieldElement z3 = ref heap(out ptr<p224FieldElement> _addr_z3);



    p224FromBig(_addr_x1, _addr_bigX1);
    p224FromBig(_addr_y1, _addr_bigY1);
    if (bigX1.Sign() != 0 || bigY1.Sign() != 0) {
        z1[0] = 1;
    }
    p224FromBig(_addr_x2, _addr_bigX2);
    p224FromBig(_addr_y2, _addr_bigY2);
    if (bigX2.Sign() != 0 || bigY2.Sign() != 0) {
        z2[0] = 1;
    }
    p224AddJacobian(_addr_x3, _addr_y3, _addr_z3, _addr_x1, _addr_y1, _addr_z1, _addr_x2, _addr_y2, _addr_z2);
    return _addr_p224ToAffine(_addr_x3, _addr_y3, _addr_z3)!;

}

private static (ptr<big.Int>, ptr<big.Int>) Double(this p224Curve _p0, ptr<big.Int> _addr_bigX1, ptr<big.Int> _addr_bigY1) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref big.Int bigX1 = ref _addr_bigX1.val;
    ref big.Int bigY1 = ref _addr_bigY1.val;

    ref p224FieldElement x1 = ref heap(out ptr<p224FieldElement> _addr_x1);    ref p224FieldElement y1 = ref heap(out ptr<p224FieldElement> _addr_y1);    ref p224FieldElement z1 = ref heap(out ptr<p224FieldElement> _addr_z1);    ref p224FieldElement x2 = ref heap(out ptr<p224FieldElement> _addr_x2);    ref p224FieldElement y2 = ref heap(out ptr<p224FieldElement> _addr_y2);    ref p224FieldElement z2 = ref heap(out ptr<p224FieldElement> _addr_z2);



    p224FromBig(_addr_x1, _addr_bigX1);
    p224FromBig(_addr_y1, _addr_bigY1);
    z1[0] = 1;

    p224DoubleJacobian(_addr_x2, _addr_y2, _addr_z2, _addr_x1, _addr_y1, _addr_z1);
    return _addr_p224ToAffine(_addr_x2, _addr_y2, _addr_z2)!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarMult(this p224Curve _p0, ptr<big.Int> _addr_bigX1, ptr<big.Int> _addr_bigY1, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;
    ref big.Int bigX1 = ref _addr_bigX1.val;
    ref big.Int bigY1 = ref _addr_bigY1.val;

    ref p224FieldElement x1 = ref heap(out ptr<p224FieldElement> _addr_x1);    ref p224FieldElement y1 = ref heap(out ptr<p224FieldElement> _addr_y1);    ref p224FieldElement z1 = ref heap(out ptr<p224FieldElement> _addr_z1);    ref p224FieldElement x2 = ref heap(out ptr<p224FieldElement> _addr_x2);    ref p224FieldElement y2 = ref heap(out ptr<p224FieldElement> _addr_y2);    ref p224FieldElement z2 = ref heap(out ptr<p224FieldElement> _addr_z2);



    p224FromBig(_addr_x1, _addr_bigX1);
    p224FromBig(_addr_y1, _addr_bigY1);
    z1[0] = 1;

    p224ScalarMult(_addr_x2, _addr_y2, _addr_z2, _addr_x1, _addr_y1, _addr_z1, scalar);
    return _addr_p224ToAffine(_addr_x2, _addr_y2, _addr_z2)!;
}

private static (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(this p224Curve curve, slice<byte> scalar) {
    ptr<big.Int> x = default!;
    ptr<big.Int> y = default!;

    ref p224FieldElement z1 = ref heap(out ptr<p224FieldElement> _addr_z1);    ref p224FieldElement x2 = ref heap(out ptr<p224FieldElement> _addr_x2);    ref p224FieldElement y2 = ref heap(out ptr<p224FieldElement> _addr_y2);    ref p224FieldElement z2 = ref heap(out ptr<p224FieldElement> _addr_z2);



    z1[0] = 1;
    p224ScalarMult(_addr_x2, _addr_y2, _addr_z2, _addr_curve.gx, _addr_curve.gy, _addr_z1, scalar);
    return _addr_p224ToAffine(_addr_x2, _addr_y2, _addr_z2)!;
}

// Field element functions.
//
// The field that we're dealing with is ℤ/pℤ where p = 2**224 - 2**96 + 1.
//
// Field elements are represented by a FieldElement, which is a typedef to an
// array of 8 uint32's. The value of a FieldElement, a, is:
//   a[0] + 2**28·a[1] + 2**56·a[1] + ... + 2**196·a[7]
//
// Using 28-bit limbs means that there's only 4 bits of headroom, which is less
// than we would really like. But it has the useful feature that we hit 2**224
// exactly, making the reflections during a reduce much nicer.
private partial struct p224FieldElement { // : array<uint>
}

// p224P is the order of the field, represented as a p224FieldElement.
private static array<uint> p224P = new array<uint>(new uint[] { 1, 0, 0, 0xffff000, 0xfffffff, 0xfffffff, 0xfffffff, 0xfffffff });

// p224IsZero returns 1 if a == 0 mod p and 0 otherwise.
//
// a[i] < 2**29
private static uint p224IsZero(ptr<p224FieldElement> _addr_a) {
    ref p224FieldElement a = ref _addr_a.val;
 
    // Since a p224FieldElement contains 224 bits there are two possible
    // representations of 0: 0 and p.
    ref p224FieldElement minimal = ref heap(out ptr<p224FieldElement> _addr_minimal);
    p224Contract(_addr_minimal, _addr_a);

    uint isZero = default;    uint isP = default;

    foreach (var (i, v) in minimal) {
        isZero |= v;
        isP |= v - p224P[i];
    }    isZero |= isZero >> 16;
    isZero |= isZero >> 8;
    isZero |= isZero >> 4;
    isZero |= isZero >> 2;
    isZero |= isZero >> 1;

    isP |= isP >> 16;
    isP |= isP >> 8;
    isP |= isP >> 4;
    isP |= isP >> 2;
    isP |= isP >> 1; 

    // For isZero and isP, the LSB is 0 iff all the bits are zero.
    var result = isZero & isP;
    result = (~result) & 1;

    return result;

}

// p224Add computes *out = a+b
//
// a[i] + b[i] < 2**32
private static void p224Add(ptr<p224FieldElement> _addr_@out, ptr<p224FieldElement> _addr_a, ptr<p224FieldElement> _addr_b) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224FieldElement a = ref _addr_a.val;
    ref p224FieldElement b = ref _addr_b.val;

    for (nint i = 0; i < 8; i++) {
        out[i] = a[i] + b[i];
    }
}

private static readonly nint two31p3 = 1 << 31 + 1 << 3;

private static readonly nint two31m3 = 1 << 31 - 1 << 3;

private static readonly nint two31m15m3 = 1 << 31 - 1 << 15 - 1 << 3;

// p224ZeroModP31 is 0 mod p where bit 31 is set in all limbs so that we can
// subtract smaller amounts without underflow. See the section "Subtraction" in
// [1] for reasoning.


// p224ZeroModP31 is 0 mod p where bit 31 is set in all limbs so that we can
// subtract smaller amounts without underflow. See the section "Subtraction" in
// [1] for reasoning.
private static uint p224ZeroModP31 = new slice<uint>(new uint[] { two31p3, two31m3, two31m3, two31m15m3, two31m3, two31m3, two31m3, two31m3 });

// p224Sub computes *out = a-b
//
// a[i], b[i] < 2**30
// out[i] < 2**32
private static void p224Sub(ptr<p224FieldElement> _addr_@out, ptr<p224FieldElement> _addr_a, ptr<p224FieldElement> _addr_b) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224FieldElement a = ref _addr_a.val;
    ref p224FieldElement b = ref _addr_b.val;

    for (nint i = 0; i < 8; i++) {
        out[i] = a[i] + p224ZeroModP31[i] - b[i];
    }
}

// LargeFieldElement also represents an element of the field. The limbs are
// still spaced 28-bits apart and in little-endian order. So the limbs are at
// 0, 28, 56, ..., 392 bits, each 64-bits wide.
private partial struct p224LargeFieldElement { // : array<ulong>
}

private static readonly nint two63p35 = 1 << 63 + 1 << 35;

private static readonly nint two63m35 = 1 << 63 - 1 << 35;

private static readonly nint two63m35m19 = 1 << 63 - 1 << 35 - 1 << 19;

// p224ZeroModP63 is 0 mod p where bit 63 is set in all limbs. See the section
// "Subtraction" in [1] for why.


// p224ZeroModP63 is 0 mod p where bit 63 is set in all limbs. See the section
// "Subtraction" in [1] for why.
private static array<ulong> p224ZeroModP63 = new array<ulong>(new ulong[] { two63p35, two63m35, two63m35, two63m35, two63m35m19, two63m35, two63m35, two63m35 });

private static readonly nuint bottom12Bits = 0xfff;

private static readonly nuint bottom28Bits = 0xfffffff;

// p224Mul computes *out = a*b
//
// a[i] < 2**29, b[i] < 2**30 (or vice versa)
// out[i] < 2**29


// p224Mul computes *out = a*b
//
// a[i] < 2**29, b[i] < 2**30 (or vice versa)
// out[i] < 2**29
private static void p224Mul(ptr<p224FieldElement> _addr_@out, ptr<p224FieldElement> _addr_a, ptr<p224FieldElement> _addr_b, ptr<p224LargeFieldElement> _addr_tmp) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224FieldElement a = ref _addr_a.val;
    ref p224FieldElement b = ref _addr_b.val;
    ref p224LargeFieldElement tmp = ref _addr_tmp.val;

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 15; i++) {
            tmp[i] = 0;
        }

        i = i__prev1;
    }

    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            for (nint j = 0; j < 8; j++) {
                tmp[i + j] += uint64(a[i]) * uint64(b[j]);
            }
        }

        i = i__prev1;
    }

    p224ReduceLarge(_addr_out, _addr_tmp);

}

// Square computes *out = a*a
//
// a[i] < 2**29
// out[i] < 2**29
private static void p224Square(ptr<p224FieldElement> _addr_@out, ptr<p224FieldElement> _addr_a, ptr<p224LargeFieldElement> _addr_tmp) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224FieldElement a = ref _addr_a.val;
    ref p224LargeFieldElement tmp = ref _addr_tmp.val;

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 15; i++) {
            tmp[i] = 0;
        }

        i = i__prev1;
    }

    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            for (nint j = 0; j <= i; j++) {
                var r = uint64(a[i]) * uint64(a[j]);
                if (i == j) {
                    tmp[i + j] += r;
                }
                else
 {
                    tmp[i + j] += r << 1;
                }

            }


        }

        i = i__prev1;
    }

    p224ReduceLarge(_addr_out, _addr_tmp);

}

// ReduceLarge converts a p224LargeFieldElement to a p224FieldElement.
//
// in[i] < 2**62
private static void p224ReduceLarge(ptr<p224FieldElement> _addr_@out, ptr<p224LargeFieldElement> _addr_@in) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224LargeFieldElement @in = ref _addr_@in.val;

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 8; i++) {
            in[i] += p224ZeroModP63[i];
        }

        i = i__prev1;
    } 

    // Eliminate the coefficients at 2**224 and greater.
    {
        nint i__prev1 = i;

        for (i = 14; i >= 8; i--) {
            in[i - 8] -= in[i];
            in[i - 5] += (in[i] & 0xffff) << 12;
            in[i - 4] += in[i] >> 16;
        }

        i = i__prev1;
    }
    in[8] = 0; 
    // in[0..8] < 2**64

    // As the values become small enough, we start to store them in |out|
    // and use 32-bit operations.
    {
        nint i__prev1 = i;

        for (i = 1; i < 8; i++) {
            in[i + 1] += in[i] >> 28;
            out[i] = uint32(in[i] & bottom28Bits);
        }

        i = i__prev1;
    }
    in[0] -= in[8];
    out[3] += uint32(in[8] & 0xffff) << 12;
    out[4] += uint32(in[8] >> 16); 
    // in[0] < 2**64
    // out[3] < 2**29
    // out[4] < 2**29
    // out[1,2,5..7] < 2**28

    out[0] = uint32(in[0] & bottom28Bits);
    out[1] += uint32((in[0] >> 28) & bottom28Bits);
    out[2] += uint32(in[0] >> 56); 
    // out[0] < 2**28
    // out[1..4] < 2**29
    // out[5..7] < 2**28
}

// Reduce reduces the coefficients of a to smaller bounds.
//
// On entry: a[i] < 2**31 + 2**30
// On exit: a[i] < 2**29
private static void p224Reduce(ptr<p224FieldElement> _addr_a) {
    ref p224FieldElement a = ref _addr_a.val;

    for (nint i = 0; i < 7; i++) {
        a[i + 1] += a[i] >> 28;
        a[i] &= bottom28Bits;
    }
    var top = a[7] >> 28;
    a[7] &= bottom28Bits; 

    // top < 2**4
    var mask = top;
    mask |= mask >> 2;
    mask |= mask >> 1;
    mask<<=31;
    mask = uint32(int32(mask) >> 31); 
    // Mask is all ones if top != 0, all zero otherwise

    a[0] -= top;
    a[3] += top << 12; 

    // We may have just made a[0] negative but, if we did, then we must
    // have added something to a[3], this it's > 2**12. Therefore we can
    // carry down to a[0].
    a[3] -= 1 & mask;
    a[2] += mask & (1 << 28 - 1);
    a[1] += mask & (1 << 28 - 1);
    a[0] += mask & (1 << 28);

}

// p224Invert calculates *out = in**-1 by computing in**(2**224 - 2**96 - 1),
// i.e. Fermat's little theorem.
private static void p224Invert(ptr<p224FieldElement> _addr_@out, ptr<p224FieldElement> _addr_@in) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224FieldElement @in = ref _addr_@in.val;

    ref p224FieldElement f1 = ref heap(out ptr<p224FieldElement> _addr_f1);    ref p224FieldElement f2 = ref heap(out ptr<p224FieldElement> _addr_f2);    ref p224FieldElement f3 = ref heap(out ptr<p224FieldElement> _addr_f3);    ref p224FieldElement f4 = ref heap(out ptr<p224FieldElement> _addr_f4);

    ref p224LargeFieldElement c = ref heap(out ptr<p224LargeFieldElement> _addr_c);

    p224Square(_addr_f1, _addr_in, _addr_c); // 2
    p224Mul(_addr_f1, _addr_f1, _addr_in, _addr_c); // 2**2 - 1
    p224Square(_addr_f1, _addr_f1, _addr_c); // 2**3 - 2
    p224Mul(_addr_f1, _addr_f1, _addr_in, _addr_c); // 2**3 - 1
    p224Square(_addr_f2, _addr_f1, _addr_c); // 2**4 - 2
    p224Square(_addr_f2, _addr_f2, _addr_c); // 2**5 - 4
    p224Square(_addr_f2, _addr_f2, _addr_c); // 2**6 - 8
    p224Mul(_addr_f1, _addr_f1, _addr_f2, _addr_c); // 2**6 - 1
    p224Square(_addr_f2, _addr_f1, _addr_c); // 2**7 - 2
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 5; i++) { // 2**12 - 2**6
            p224Square(_addr_f2, _addr_f2, _addr_c);

        }

        i = i__prev1;
    }
    p224Mul(_addr_f2, _addr_f2, _addr_f1, _addr_c); // 2**12 - 1
    p224Square(_addr_f3, _addr_f2, _addr_c); // 2**13 - 2
    {
        nint i__prev1 = i;

        for (i = 0; i < 11; i++) { // 2**24 - 2**12
            p224Square(_addr_f3, _addr_f3, _addr_c);

        }

        i = i__prev1;
    }
    p224Mul(_addr_f2, _addr_f3, _addr_f2, _addr_c); // 2**24 - 1
    p224Square(_addr_f3, _addr_f2, _addr_c); // 2**25 - 2
    {
        nint i__prev1 = i;

        for (i = 0; i < 23; i++) { // 2**48 - 2**24
            p224Square(_addr_f3, _addr_f3, _addr_c);

        }

        i = i__prev1;
    }
    p224Mul(_addr_f3, _addr_f3, _addr_f2, _addr_c); // 2**48 - 1
    p224Square(_addr_f4, _addr_f3, _addr_c); // 2**49 - 2
    {
        nint i__prev1 = i;

        for (i = 0; i < 47; i++) { // 2**96 - 2**48
            p224Square(_addr_f4, _addr_f4, _addr_c);

        }

        i = i__prev1;
    }
    p224Mul(_addr_f3, _addr_f3, _addr_f4, _addr_c); // 2**96 - 1
    p224Square(_addr_f4, _addr_f3, _addr_c); // 2**97 - 2
    {
        nint i__prev1 = i;

        for (i = 0; i < 23; i++) { // 2**120 - 2**24
            p224Square(_addr_f4, _addr_f4, _addr_c);

        }

        i = i__prev1;
    }
    p224Mul(_addr_f2, _addr_f4, _addr_f2, _addr_c); // 2**120 - 1
    {
        nint i__prev1 = i;

        for (i = 0; i < 6; i++) { // 2**126 - 2**6
            p224Square(_addr_f2, _addr_f2, _addr_c);

        }

        i = i__prev1;
    }
    p224Mul(_addr_f1, _addr_f1, _addr_f2, _addr_c); // 2**126 - 1
    p224Square(_addr_f1, _addr_f1, _addr_c); // 2**127 - 2
    p224Mul(_addr_f1, _addr_f1, _addr_in, _addr_c); // 2**127 - 1
    {
        nint i__prev1 = i;

        for (i = 0; i < 97; i++) { // 2**224 - 2**97
            p224Square(_addr_f1, _addr_f1, _addr_c);

        }

        i = i__prev1;
    }
    p224Mul(_addr_out, _addr_f1, _addr_f3, _addr_c); // 2**224 - 2**96 - 1
}

// p224Contract converts a FieldElement to its unique, minimal form.
//
// On entry, in[i] < 2**29
// On exit, out[i] < 2**28 and out < p
private static void p224Contract(ptr<p224FieldElement> _addr_@out, ptr<p224FieldElement> _addr_@in) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224FieldElement @in = ref _addr_@in.val;

    copy(out[..], in[..]); 

    // First, carry the bits above 28 to the higher limb.
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 7; i++) {
            out[i + 1] += out[i] >> 28;
            out[i] &= bottom28Bits;
        }

        i = i__prev1;
    }
    var top = out[7] >> 28;
    out[7] &= bottom28Bits; 

    // Use the reduction identity to carry the overflow.
    //
    //   a + top * 2²²⁴ = a + top * 2⁹⁶ - top
    out[0] -= top;
    out[3] += top << 12; 

    // We may just have made out[0] negative. So we carry down. If we made
    // out[0] negative then we know that out[3] is sufficiently positive
    // because we just added to it.
    {
        nint i__prev1 = i;

        for (i = 0; i < 3; i++) {
            var mask = uint32(int32(out[i]) >> 31);
            out[i] += (1 << 28) & mask;
            out[i + 1] -= 1 & mask;
        }

        i = i__prev1;
    } 

    // We might have pushed out[3] over 2**28 so we perform another, partial,
    // carry chain.
    {
        nint i__prev1 = i;

        for (i = 3; i < 7; i++) {
            out[i + 1] += out[i] >> 28;
            out[i] &= bottom28Bits;
        }

        i = i__prev1;
    }
    top = out[7] >> 28;
    out[7] &= bottom28Bits; 

    // Eliminate top while maintaining the same value mod p.
    out[0] -= top;
    out[3] += top << 12; 

    // There are two cases to consider for out[3]:
    //   1) The first time that we eliminated top, we didn't push out[3] over
    //      2**28. In this case, the partial carry chain didn't change any values
    //      and top is now zero.
    //   2) We did push out[3] over 2**28 the first time that we eliminated top.
    //      The first value of top was in [0..2], therefore, after overflowing
    //      and being reduced by the second carry chain, out[3] <= 2<<12 - 1.
    // In both cases, out[3] cannot have overflowed when we eliminated top for
    // the second time.

    // Again, we may just have made out[0] negative, so do the same carry down.
    // As before, if we made out[0] negative then we know that out[3] is
    // sufficiently positive.
    {
        nint i__prev1 = i;

        for (i = 0; i < 3; i++) {
            mask = uint32(int32(out[i]) >> 31);
            out[i] += (1 << 28) & mask;
            out[i + 1] -= 1 & mask;
        }

        i = i__prev1;
    } 

    // Now we see if the value is >= p and, if so, subtract p.

    // First we build a mask from the top four limbs, which must all be
    // equal to bottom28Bits if the whole value is >= p. If top4AllOnes
    // ends up with any zero bits in the bottom 28 bits, then this wasn't
    // true.
    var top4AllOnes = uint32(0xffffffff);
    {
        nint i__prev1 = i;

        for (i = 4; i < 8; i++) {
            top4AllOnes &= out[i];
        }

        i = i__prev1;
    }
    top4AllOnes |= 0xf0000000; 
    // Now we replicate any zero bits to all the bits in top4AllOnes.
    top4AllOnes &= top4AllOnes >> 16;
    top4AllOnes &= top4AllOnes >> 8;
    top4AllOnes &= top4AllOnes >> 4;
    top4AllOnes &= top4AllOnes >> 2;
    top4AllOnes &= top4AllOnes >> 1;
    top4AllOnes = uint32(int32(top4AllOnes << 31) >> 31); 

    // Now we test whether the bottom three limbs are non-zero.
    var bottom3NonZero = out[0] | out[1] | out[2];
    bottom3NonZero |= bottom3NonZero >> 16;
    bottom3NonZero |= bottom3NonZero >> 8;
    bottom3NonZero |= bottom3NonZero >> 4;
    bottom3NonZero |= bottom3NonZero >> 2;
    bottom3NonZero |= bottom3NonZero >> 1;
    bottom3NonZero = uint32(int32(bottom3NonZero << 31) >> 31); 

    // Assuming top4AllOnes != 0, everything depends on the value of out[3].
    //    If it's > 0xffff000 then the whole value is > p
    //    If it's = 0xffff000 and bottom3NonZero != 0, then the whole value is >= p
    //    If it's < 0xffff000, then the whole value is < p
    nuint n = 0xffff000 - out[3];
    var out3Equal = n;
    out3Equal |= out3Equal >> 16;
    out3Equal |= out3Equal >> 8;
    out3Equal |= out3Equal >> 4;
    out3Equal |= out3Equal >> 2;
    out3Equal |= out3Equal >> 1;
    out3Equal = ~uint32(int32(out3Equal << 31) >> 31); 

    // If out[3] > 0xffff000 then n's MSB will be one.
    var out3GT = uint32(int32(n) >> 31);

    mask = top4AllOnes & ((out3Equal & bottom3NonZero) | out3GT);
    out[0] -= 1 & mask;
    out[3] -= 0xffff000 & mask;
    out[4] -= 0xfffffff & mask;
    out[5] -= 0xfffffff & mask;
    out[6] -= 0xfffffff & mask;
    out[7] -= 0xfffffff & mask; 

    // Do one final carry down, in case we made out[0] negative. One of
    // out[0..3] needs to be positive and able to absorb the -1 or the value
    // would have been < p, and the subtraction wouldn't have happened.
    {
        nint i__prev1 = i;

        for (i = 0; i < 3; i++) {
            mask = uint32(int32(out[i]) >> 31);
            out[i] += (1 << 28) & mask;
            out[i + 1] -= 1 & mask;
        }

        i = i__prev1;
    }

}

// Group element functions.
//
// These functions deal with group elements. The group is an elliptic curve
// group with a = -3 defined in FIPS 186-3, section D.2.2.

// p224AddJacobian computes *out = a+b where a != b.
private static void p224AddJacobian(ptr<p224FieldElement> _addr_x3, ptr<p224FieldElement> _addr_y3, ptr<p224FieldElement> _addr_z3, ptr<p224FieldElement> _addr_x1, ptr<p224FieldElement> _addr_y1, ptr<p224FieldElement> _addr_z1, ptr<p224FieldElement> _addr_x2, ptr<p224FieldElement> _addr_y2, ptr<p224FieldElement> _addr_z2) {
    ref p224FieldElement x3 = ref _addr_x3.val;
    ref p224FieldElement y3 = ref _addr_y3.val;
    ref p224FieldElement z3 = ref _addr_z3.val;
    ref p224FieldElement x1 = ref _addr_x1.val;
    ref p224FieldElement y1 = ref _addr_y1.val;
    ref p224FieldElement z1 = ref _addr_z1.val;
    ref p224FieldElement x2 = ref _addr_x2.val;
    ref p224FieldElement y2 = ref _addr_y2.val;
    ref p224FieldElement z2 = ref _addr_z2.val;
 
    // See https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#addition-p224Add-2007-bl
    ref p224FieldElement z1z1 = ref heap(out ptr<p224FieldElement> _addr_z1z1);    ref p224FieldElement z2z2 = ref heap(out ptr<p224FieldElement> _addr_z2z2);    ref p224FieldElement u1 = ref heap(out ptr<p224FieldElement> _addr_u1);    ref p224FieldElement u2 = ref heap(out ptr<p224FieldElement> _addr_u2);    ref p224FieldElement s1 = ref heap(out ptr<p224FieldElement> _addr_s1);    ref p224FieldElement s2 = ref heap(out ptr<p224FieldElement> _addr_s2);    ref p224FieldElement h = ref heap(out ptr<p224FieldElement> _addr_h);    ref p224FieldElement i = ref heap(out ptr<p224FieldElement> _addr_i);    ref p224FieldElement j = ref heap(out ptr<p224FieldElement> _addr_j);    ref p224FieldElement r = ref heap(out ptr<p224FieldElement> _addr_r);    ref p224FieldElement v = ref heap(out ptr<p224FieldElement> _addr_v);

    ref p224LargeFieldElement c = ref heap(out ptr<p224LargeFieldElement> _addr_c);

    var z1IsZero = p224IsZero(_addr_z1);
    var z2IsZero = p224IsZero(_addr_z2); 

    // Z1Z1 = Z1²
    p224Square(_addr_z1z1, _addr_z1, _addr_c); 
    // Z2Z2 = Z2²
    p224Square(_addr_z2z2, _addr_z2, _addr_c); 
    // U1 = X1*Z2Z2
    p224Mul(_addr_u1, _addr_x1, _addr_z2z2, _addr_c); 
    // U2 = X2*Z1Z1
    p224Mul(_addr_u2, _addr_x2, _addr_z1z1, _addr_c); 
    // S1 = Y1*Z2*Z2Z2
    p224Mul(_addr_s1, _addr_z2, _addr_z2z2, _addr_c);
    p224Mul(_addr_s1, _addr_y1, _addr_s1, _addr_c); 
    // S2 = Y2*Z1*Z1Z1
    p224Mul(_addr_s2, _addr_z1, _addr_z1z1, _addr_c);
    p224Mul(_addr_s2, _addr_y2, _addr_s2, _addr_c); 
    // H = U2-U1
    p224Sub(_addr_h, _addr_u2, _addr_u1);
    p224Reduce(_addr_h);
    var xEqual = p224IsZero(_addr_h); 
    // I = (2*H)²
    {
        p224FieldElement j__prev1 = j;

        for (j = 0; j < 8; j++) {
            i[j] = h[j] << 1;
        }

        j = j__prev1;
    }
    p224Reduce(_addr_i);
    p224Square(_addr_i, _addr_i, _addr_c); 
    // J = H*I
    p224Mul(_addr_j, _addr_h, _addr_i, _addr_c); 
    // r = 2*(S2-S1)
    p224Sub(_addr_r, _addr_s2, _addr_s1);
    p224Reduce(_addr_r);
    var yEqual = p224IsZero(_addr_r);
    if (xEqual == 1 && yEqual == 1 && z1IsZero == 0 && z2IsZero == 0) {
        p224DoubleJacobian(_addr_x3, _addr_y3, _addr_z3, _addr_x1, _addr_y1, _addr_z1);
        return ;
    }
    {
        p224FieldElement i__prev1 = i;

        for (i = 0; i < 8; i++) {
            r[i]<<=1;
        }

        i = i__prev1;
    }
    p224Reduce(_addr_r); 
    // V = U1*I
    p224Mul(_addr_v, _addr_u1, _addr_i, _addr_c); 
    // Z3 = ((Z1+Z2)²-Z1Z1-Z2Z2)*H
    p224Add(_addr_z1z1, _addr_z1z1, _addr_z2z2);
    p224Add(_addr_z2z2, _addr_z1, _addr_z2);
    p224Reduce(_addr_z2z2);
    p224Square(_addr_z2z2, _addr_z2z2, _addr_c);
    p224Sub(_addr_z3, _addr_z2z2, _addr_z1z1);
    p224Reduce(_addr_z3);
    p224Mul(_addr_z3, _addr_z3, _addr_h, _addr_c); 
    // X3 = r²-J-2*V
    {
        p224FieldElement i__prev1 = i;

        for (i = 0; i < 8; i++) {
            z1z1[i] = v[i] << 1;
        }

        i = i__prev1;
    }
    p224Add(_addr_z1z1, _addr_j, _addr_z1z1);
    p224Reduce(_addr_z1z1);
    p224Square(_addr_x3, _addr_r, _addr_c);
    p224Sub(_addr_x3, _addr_x3, _addr_z1z1);
    p224Reduce(_addr_x3); 
    // Y3 = r*(V-X3)-2*S1*J
    {
        p224FieldElement i__prev1 = i;

        for (i = 0; i < 8; i++) {
            s1[i]<<=1;
        }

        i = i__prev1;
    }
    p224Mul(_addr_s1, _addr_s1, _addr_j, _addr_c);
    p224Sub(_addr_z1z1, _addr_v, _addr_x3);
    p224Reduce(_addr_z1z1);
    p224Mul(_addr_z1z1, _addr_z1z1, _addr_r, _addr_c);
    p224Sub(_addr_y3, _addr_z1z1, _addr_s1);
    p224Reduce(_addr_y3);

    p224CopyConditional(_addr_x3, _addr_x2, z1IsZero);
    p224CopyConditional(_addr_x3, _addr_x1, z2IsZero);
    p224CopyConditional(_addr_y3, _addr_y2, z1IsZero);
    p224CopyConditional(_addr_y3, _addr_y1, z2IsZero);
    p224CopyConditional(_addr_z3, _addr_z2, z1IsZero);
    p224CopyConditional(_addr_z3, _addr_z1, z2IsZero);

}

// p224DoubleJacobian computes *out = a+a.
private static void p224DoubleJacobian(ptr<p224FieldElement> _addr_x3, ptr<p224FieldElement> _addr_y3, ptr<p224FieldElement> _addr_z3, ptr<p224FieldElement> _addr_x1, ptr<p224FieldElement> _addr_y1, ptr<p224FieldElement> _addr_z1) {
    ref p224FieldElement x3 = ref _addr_x3.val;
    ref p224FieldElement y3 = ref _addr_y3.val;
    ref p224FieldElement z3 = ref _addr_z3.val;
    ref p224FieldElement x1 = ref _addr_x1.val;
    ref p224FieldElement y1 = ref _addr_y1.val;
    ref p224FieldElement z1 = ref _addr_z1.val;

    ref p224FieldElement delta = ref heap(out ptr<p224FieldElement> _addr_delta);    ref p224FieldElement gamma = ref heap(out ptr<p224FieldElement> _addr_gamma);    ref p224FieldElement beta = ref heap(out ptr<p224FieldElement> _addr_beta);    ref p224FieldElement alpha = ref heap(out ptr<p224FieldElement> _addr_alpha);    ref p224FieldElement t = ref heap(out ptr<p224FieldElement> _addr_t);

    ref p224LargeFieldElement c = ref heap(out ptr<p224LargeFieldElement> _addr_c);

    p224Square(_addr_delta, _addr_z1, _addr_c);
    p224Square(_addr_gamma, _addr_y1, _addr_c);
    p224Mul(_addr_beta, _addr_x1, _addr_gamma, _addr_c); 

    // alpha = 3*(X1-delta)*(X1+delta)
    p224Add(_addr_t, _addr_x1, _addr_delta);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 8; i++) {
            t[i] += t[i] << 1;
        }

        i = i__prev1;
    }
    p224Reduce(_addr_t);
    p224Sub(_addr_alpha, _addr_x1, _addr_delta);
    p224Reduce(_addr_alpha);
    p224Mul(_addr_alpha, _addr_alpha, _addr_t, _addr_c); 

    // Z3 = (Y1+Z1)²-gamma-delta
    p224Add(_addr_z3, _addr_y1, _addr_z1);
    p224Reduce(_addr_z3);
    p224Square(_addr_z3, _addr_z3, _addr_c);
    p224Sub(_addr_z3, _addr_z3, _addr_gamma);
    p224Reduce(_addr_z3);
    p224Sub(_addr_z3, _addr_z3, _addr_delta);
    p224Reduce(_addr_z3); 

    // X3 = alpha²-8*beta
    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            delta[i] = beta[i] << 3;
        }

        i = i__prev1;
    }
    p224Reduce(_addr_delta);
    p224Square(_addr_x3, _addr_alpha, _addr_c);
    p224Sub(_addr_x3, _addr_x3, _addr_delta);
    p224Reduce(_addr_x3); 

    // Y3 = alpha*(4*beta-X3)-8*gamma²
    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            beta[i]<<=2;
        }

        i = i__prev1;
    }
    p224Sub(_addr_beta, _addr_beta, _addr_x3);
    p224Reduce(_addr_beta);
    p224Square(_addr_gamma, _addr_gamma, _addr_c);
    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            gamma[i]<<=3;
        }

        i = i__prev1;
    }
    p224Reduce(_addr_gamma);
    p224Mul(_addr_y3, _addr_alpha, _addr_beta, _addr_c);
    p224Sub(_addr_y3, _addr_y3, _addr_gamma);
    p224Reduce(_addr_y3);

}

// p224CopyConditional sets *out = *in iff the least-significant-bit of control
// is true, and it runs in constant time.
private static void p224CopyConditional(ptr<p224FieldElement> _addr_@out, ptr<p224FieldElement> _addr_@in, uint control) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref p224FieldElement @in = ref _addr_@in.val;

    control<<=31;
    control = uint32(int32(control) >> 31);

    for (nint i = 0; i < 8; i++) {
        out[i] ^= (out[i] ^ in[i]) & control;
    }
}

private static void p224ScalarMult(ptr<p224FieldElement> _addr_outX, ptr<p224FieldElement> _addr_outY, ptr<p224FieldElement> _addr_outZ, ptr<p224FieldElement> _addr_inX, ptr<p224FieldElement> _addr_inY, ptr<p224FieldElement> _addr_inZ, slice<byte> scalar) {
    ref p224FieldElement outX = ref _addr_outX.val;
    ref p224FieldElement outY = ref _addr_outY.val;
    ref p224FieldElement outZ = ref _addr_outZ.val;
    ref p224FieldElement inX = ref _addr_inX.val;
    ref p224FieldElement inY = ref _addr_inY.val;
    ref p224FieldElement inZ = ref _addr_inZ.val;

    ref p224FieldElement xx = ref heap(out ptr<p224FieldElement> _addr_xx);    ref p224FieldElement yy = ref heap(out ptr<p224FieldElement> _addr_yy);    ref p224FieldElement zz = ref heap(out ptr<p224FieldElement> _addr_zz);

    for (nint i = 0; i < 8; i++) {
        outX[i] = 0;
        outY[i] = 0;
        outZ[i] = 0;
    }

    foreach (var (_, byte) in scalar) {
        for (var bitNum = uint(0); bitNum < 8; bitNum++) {
            p224DoubleJacobian(_addr_outX, _addr_outY, _addr_outZ, _addr_outX, _addr_outY, _addr_outZ);
            var bit = uint32((byte >> (int)((7 - bitNum))) & 1);
            p224AddJacobian(_addr_xx, _addr_yy, _addr_zz, _addr_inX, _addr_inY, _addr_inZ, _addr_outX, _addr_outY, _addr_outZ);
            p224CopyConditional(_addr_outX, _addr_xx, bit);
            p224CopyConditional(_addr_outY, _addr_yy, bit);
            p224CopyConditional(_addr_outZ, _addr_zz, bit);
        }
    }
}

// p224ToAffine converts from Jacobian to affine form.
private static (ptr<big.Int>, ptr<big.Int>) p224ToAffine(ptr<p224FieldElement> _addr_x, ptr<p224FieldElement> _addr_y, ptr<p224FieldElement> _addr_z) {
    ptr<big.Int> _p0 = default!;
    ptr<big.Int> _p0 = default!;
    ref p224FieldElement x = ref _addr_x.val;
    ref p224FieldElement y = ref _addr_y.val;
    ref p224FieldElement z = ref _addr_z.val;

    ref p224FieldElement zinv = ref heap(out ptr<p224FieldElement> _addr_zinv);    ref p224FieldElement zinvsq = ref heap(out ptr<p224FieldElement> _addr_zinvsq);    ref p224FieldElement outx = ref heap(out ptr<p224FieldElement> _addr_outx);    ref p224FieldElement outy = ref heap(out ptr<p224FieldElement> _addr_outy);

    ref p224LargeFieldElement tmp = ref heap(out ptr<p224LargeFieldElement> _addr_tmp);

    {
        var isPointAtInfinity = p224IsZero(_addr_z);

        if (isPointAtInfinity == 1) {
            return (@new<big.Int>(), @new<big.Int>());
        }
    }


    p224Invert(_addr_zinv, _addr_z);
    p224Square(_addr_zinvsq, _addr_zinv, _addr_tmp);
    p224Mul(_addr_x, _addr_x, _addr_zinvsq, _addr_tmp);
    p224Mul(_addr_zinvsq, _addr_zinvsq, _addr_zinv, _addr_tmp);
    p224Mul(_addr_y, _addr_y, _addr_zinvsq, _addr_tmp);

    p224Contract(_addr_outx, _addr_x);
    p224Contract(_addr_outy, _addr_y);
    return (_addr_p224ToBig(_addr_outx)!, _addr_p224ToBig(_addr_outy)!);

}

// get28BitsFromEnd returns the least-significant 28 bits from buf>>shift,
// where buf is interpreted as a big-endian number.
private static (uint, slice<byte>) get28BitsFromEnd(slice<byte> buf, nuint shift) {
    uint _p0 = default;
    slice<byte> _p0 = default;

    uint ret = default;

    for (var i = uint(0); i < 4; i++) {
        byte b = default;
        {
            var l = len(buf);

            if (l > 0) {
                b = buf[l - 1]; 
                // We don't remove the byte if we're about to return and we're not
                // reading all of it.
                if (i != 3 || shift == 4) {
                    buf = buf[..(int)l - 1];
                }

            }

        }

        ret |= uint32(b) << (int)((8 * i)) >> (int)(shift);

    }
    ret &= bottom28Bits;
    return (ret, buf);

}

// p224FromBig sets *out = *in.
private static void p224FromBig(ptr<p224FieldElement> _addr_@out, ptr<big.Int> _addr_@in) {
    ref p224FieldElement @out = ref _addr_@out.val;
    ref big.Int @in = ref _addr_@in.val;

    var bytes = @in.Bytes();
    out[0], bytes = get28BitsFromEnd(bytes, 0);
    out[1], bytes = get28BitsFromEnd(bytes, 4);
    out[2], bytes = get28BitsFromEnd(bytes, 0);
    out[3], bytes = get28BitsFromEnd(bytes, 4);
    out[4], bytes = get28BitsFromEnd(bytes, 0);
    out[5], bytes = get28BitsFromEnd(bytes, 4);
    out[6], bytes = get28BitsFromEnd(bytes, 0);
    out[7], bytes = get28BitsFromEnd(bytes, 4);
}

// p224ToBig returns in as a big.Int.
private static ptr<big.Int> p224ToBig(ptr<p224FieldElement> _addr_@in) {
    ref p224FieldElement @in = ref _addr_@in.val;

    array<byte> buf = new array<byte>(28);
    buf[27] = byte(in[0]);
    buf[26] = byte(in[0] >> 8);
    buf[25] = byte(in[0] >> 16);
    buf[24] = byte(((in[0] >> 24) & 0x0f) | (in[1] << 4) & 0xf0);

    buf[23] = byte(in[1] >> 4);
    buf[22] = byte(in[1] >> 12);
    buf[21] = byte(in[1] >> 20);

    buf[20] = byte(in[2]);
    buf[19] = byte(in[2] >> 8);
    buf[18] = byte(in[2] >> 16);
    buf[17] = byte(((in[2] >> 24) & 0x0f) | (in[3] << 4) & 0xf0);

    buf[16] = byte(in[3] >> 4);
    buf[15] = byte(in[3] >> 12);
    buf[14] = byte(in[3] >> 20);

    buf[13] = byte(in[4]);
    buf[12] = byte(in[4] >> 8);
    buf[11] = byte(in[4] >> 16);
    buf[10] = byte(((in[4] >> 24) & 0x0f) | (in[5] << 4) & 0xf0);

    buf[9] = byte(in[5] >> 4);
    buf[8] = byte(in[5] >> 12);
    buf[7] = byte(in[5] >> 20);

    buf[6] = byte(in[6]);
    buf[5] = byte(in[6] >> 8);
    buf[4] = byte(in[6] >> 16);
    buf[3] = byte(((in[6] >> 24) & 0x0f) | (in[7] << 4) & 0xf0);

    buf[2] = byte(in[7] >> 4);
    buf[1] = byte(in[7] >> 12);
    buf[0] = byte(in[7] >> 20);

    return @new<big.Int>().SetBytes(buf[..]);
}

} // end elliptic_package
