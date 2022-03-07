// Copyright (c) 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package field implements fast arithmetic modulo 2^255-19.
// package field -- go2cs converted at 2022 March 06 22:17:26 UTC
// import "crypto/ed25519/internal/edwards25519/field" ==> using field = go.crypto.ed25519.@internal.edwards25519.field_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\field\fe.go
using subtle = go.crypto.subtle_package;
using binary = go.encoding.binary_package;
using bits = go.math.bits_package;

namespace go.crypto.ed25519.@internal.edwards25519;

public static partial class field_package {

    // Element represents an element of the field GF(2^255-19). Note that this
    // is not a cryptographically secure group, and should only be used to interact
    // with edwards25519.Point coordinates.
    //
    // This type works similarly to math/big.Int, and all arguments and receivers
    // are allowed to alias.
    //
    // The zero value is a valid zero element.
public partial struct Element {
    public ulong l0;
    public ulong l1;
    public ulong l2;
    public ulong l3;
    public ulong l4;
}

private static readonly ulong maskLow51Bits = (1 << 51) - 1;



private static ptr<Element> feZero = addr(new Element(0,0,0,0,0));

// Zero sets v = 0, and returns v.
private static ptr<Element> Zero(this ptr<Element> _addr_v) {
    ref Element v = ref _addr_v.val;

    v.val = feZero.val;
    return _addr_v!;
}

private static ptr<Element> feOne = addr(new Element(1,0,0,0,0));

// One sets v = 1, and returns v.
private static ptr<Element> One(this ptr<Element> _addr_v) {
    ref Element v = ref _addr_v.val;

    v.val = feOne.val;
    return _addr_v!;
}

// reduce reduces v modulo 2^255 - 19 and returns it.
private static ptr<Element> reduce(this ptr<Element> _addr_v) {
    ref Element v = ref _addr_v.val;

    v.carryPropagate(); 

    // After the light reduction we now have a field element representation
    // v < 2^255 + 2^13 * 19, but need v < 2^255 - 19.

    // If v >= 2^255 - 19, then v + 19 >= 2^255, which would overflow 2^255 - 1,
    // generating a carry. That is, c will be 0 if v < 2^255 - 19, and 1 otherwise.
    var c = (v.l0 + 19) >> 51;
    c = (v.l1 + c) >> 51;
    c = (v.l2 + c) >> 51;
    c = (v.l3 + c) >> 51;
    c = (v.l4 + c) >> 51; 

    // If v < 2^255 - 19 and c = 0, this will be a no-op. Otherwise, it's
    // effectively applying the reduction identity to the carry.
    v.l0 += 19 * c;

    v.l1 += v.l0 >> 51;
    v.l0 = v.l0 & maskLow51Bits;
    v.l2 += v.l1 >> 51;
    v.l1 = v.l1 & maskLow51Bits;
    v.l3 += v.l2 >> 51;
    v.l2 = v.l2 & maskLow51Bits;
    v.l4 += v.l3 >> 51;
    v.l3 = v.l3 & maskLow51Bits; 
    // no additional carry
    v.l4 = v.l4 & maskLow51Bits;

    return _addr_v!;

}

// Add sets v = a + b, and returns v.
private static ptr<Element> Add(this ptr<Element> _addr_v, ptr<Element> _addr_a, ptr<Element> _addr_b) {
    ref Element v = ref _addr_v.val;
    ref Element a = ref _addr_a.val;
    ref Element b = ref _addr_b.val;

    v.l0 = a.l0 + b.l0;
    v.l1 = a.l1 + b.l1;
    v.l2 = a.l2 + b.l2;
    v.l3 = a.l3 + b.l3;
    v.l4 = a.l4 + b.l4; 
    // Using the generic implementation here is actually faster than the
    // assembly. Probably because the body of this function is so simple that
    // the compiler can figure out better optimizations by inlining the carry
    // propagation.
    return _addr_v.carryPropagateGeneric()!;

}

// Subtract sets v = a - b, and returns v.
private static ptr<Element> Subtract(this ptr<Element> _addr_v, ptr<Element> _addr_a, ptr<Element> _addr_b) {
    ref Element v = ref _addr_v.val;
    ref Element a = ref _addr_a.val;
    ref Element b = ref _addr_b.val;
 
    // We first add 2 * p, to guarantee the subtraction won't underflow, and
    // then subtract b (which can be up to 2^255 + 2^13 * 19).
    v.l0 = (a.l0 + 0xFFFFFFFFFFFDA) - b.l0;
    v.l1 = (a.l1 + 0xFFFFFFFFFFFFE) - b.l1;
    v.l2 = (a.l2 + 0xFFFFFFFFFFFFE) - b.l2;
    v.l3 = (a.l3 + 0xFFFFFFFFFFFFE) - b.l3;
    v.l4 = (a.l4 + 0xFFFFFFFFFFFFE) - b.l4;
    return _addr_v.carryPropagate()!;

}

// Negate sets v = -a, and returns v.
private static ptr<Element> Negate(this ptr<Element> _addr_v, ptr<Element> _addr_a) {
    ref Element v = ref _addr_v.val;
    ref Element a = ref _addr_a.val;

    return _addr_v.Subtract(feZero, a)!;
}

// Invert sets v = 1/z mod p, and returns v.
//
// If z == 0, Invert returns v = 0.
private static ptr<Element> Invert(this ptr<Element> _addr_v, ptr<Element> _addr_z) {
    ref Element v = ref _addr_v.val;
    ref Element z = ref _addr_z.val;
 
    // Inversion is implemented as exponentiation with exponent p − 2. It uses the
    // same sequence of 255 squarings and 11 multiplications as [Curve25519].
    ref Element z2 = ref heap(out ptr<Element> _addr_z2);    ref Element z9 = ref heap(out ptr<Element> _addr_z9);    ref Element z11 = ref heap(out ptr<Element> _addr_z11);    ref Element z2_5_0 = ref heap(out ptr<Element> _addr_z2_5_0);    ref Element z2_10_0 = ref heap(out ptr<Element> _addr_z2_10_0);    ref Element z2_20_0 = ref heap(out ptr<Element> _addr_z2_20_0);    ref Element z2_50_0 = ref heap(out ptr<Element> _addr_z2_50_0);    ref Element z2_100_0 = ref heap(out ptr<Element> _addr_z2_100_0);    ref Element t = ref heap(out ptr<Element> _addr_t);



    z2.Square(z); // 2
    t.Square(_addr_z2); // 4
    t.Square(_addr_t); // 8
    z9.Multiply(_addr_t, z); // 9
    z11.Multiply(_addr_z9, _addr_z2); // 11
    t.Square(_addr_z11); // 22
    z2_5_0.Multiply(_addr_t, _addr_z9); // 31 = 2^5 - 2^0

    t.Square(_addr_z2_5_0); // 2^6 - 2^1
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 4; i++) {
            t.Square(_addr_t); // 2^10 - 2^5
        }

        i = i__prev1;
    }
    z2_10_0.Multiply(_addr_t, _addr_z2_5_0); // 2^10 - 2^0

    t.Square(_addr_z2_10_0); // 2^11 - 2^1
    {
        nint i__prev1 = i;

        for (i = 0; i < 9; i++) {
            t.Square(_addr_t); // 2^20 - 2^10
        }

        i = i__prev1;
    }
    z2_20_0.Multiply(_addr_t, _addr_z2_10_0); // 2^20 - 2^0

    t.Square(_addr_z2_20_0); // 2^21 - 2^1
    {
        nint i__prev1 = i;

        for (i = 0; i < 19; i++) {
            t.Square(_addr_t); // 2^40 - 2^20
        }

        i = i__prev1;
    }
    t.Multiply(_addr_t, _addr_z2_20_0); // 2^40 - 2^0

    t.Square(_addr_t); // 2^41 - 2^1
    {
        nint i__prev1 = i;

        for (i = 0; i < 9; i++) {
            t.Square(_addr_t); // 2^50 - 2^10
        }

        i = i__prev1;
    }
    z2_50_0.Multiply(_addr_t, _addr_z2_10_0); // 2^50 - 2^0

    t.Square(_addr_z2_50_0); // 2^51 - 2^1
    {
        nint i__prev1 = i;

        for (i = 0; i < 49; i++) {
            t.Square(_addr_t); // 2^100 - 2^50
        }

        i = i__prev1;
    }
    z2_100_0.Multiply(_addr_t, _addr_z2_50_0); // 2^100 - 2^0

    t.Square(_addr_z2_100_0); // 2^101 - 2^1
    {
        nint i__prev1 = i;

        for (i = 0; i < 99; i++) {
            t.Square(_addr_t); // 2^200 - 2^100
        }

        i = i__prev1;
    }
    t.Multiply(_addr_t, _addr_z2_100_0); // 2^200 - 2^0

    t.Square(_addr_t); // 2^201 - 2^1
    {
        nint i__prev1 = i;

        for (i = 0; i < 49; i++) {
            t.Square(_addr_t); // 2^250 - 2^50
        }

        i = i__prev1;
    }
    t.Multiply(_addr_t, _addr_z2_50_0); // 2^250 - 2^0

    t.Square(_addr_t); // 2^251 - 2^1
    t.Square(_addr_t); // 2^252 - 2^2
    t.Square(_addr_t); // 2^253 - 2^3
    t.Square(_addr_t); // 2^254 - 2^4
    t.Square(_addr_t); // 2^255 - 2^5

    return _addr_v.Multiply(_addr_t, _addr_z11)!; // 2^255 - 21
}

// Set sets v = a, and returns v.
private static ptr<Element> Set(this ptr<Element> _addr_v, ptr<Element> _addr_a) {
    ref Element v = ref _addr_v.val;
    ref Element a = ref _addr_a.val;

    v.val = a;
    return _addr_v!;
}

// SetBytes sets v to x, which must be a 32-byte little-endian encoding.
//
// Consistent with RFC 7748, the most significant bit (the high bit of the
// last byte) is ignored, and non-canonical values (2^255-19 through 2^255-1)
// are accepted. Note that this is laxer than specified by RFC 8032.
private static ptr<Element> SetBytes(this ptr<Element> _addr_v, slice<byte> x) => func((_, panic, _) => {
    ref Element v = ref _addr_v.val;

    if (len(x) != 32) {
        panic("edwards25519: invalid field element input size");
    }
    v.l0 = binary.LittleEndian.Uint64(x[(int)0..(int)8]);
    v.l0 &= maskLow51Bits; 
    // Bits 51:102 (bytes 6:14, bits 48:112, shift 3, mask 51).
    v.l1 = binary.LittleEndian.Uint64(x[(int)6..(int)14]) >> 3;
    v.l1 &= maskLow51Bits; 
    // Bits 102:153 (bytes 12:20, bits 96:160, shift 6, mask 51).
    v.l2 = binary.LittleEndian.Uint64(x[(int)12..(int)20]) >> 6;
    v.l2 &= maskLow51Bits; 
    // Bits 153:204 (bytes 19:27, bits 152:216, shift 1, mask 51).
    v.l3 = binary.LittleEndian.Uint64(x[(int)19..(int)27]) >> 1;
    v.l3 &= maskLow51Bits; 
    // Bits 204:251 (bytes 24:32, bits 192:256, shift 12, mask 51).
    // Note: not bytes 25:33, shift 4, to avoid overread.
    v.l4 = binary.LittleEndian.Uint64(x[(int)24..(int)32]) >> 12;
    v.l4 &= maskLow51Bits;

    return _addr_v!;

});

// Bytes returns the canonical 32-byte little-endian encoding of v.
private static slice<byte> Bytes(this ptr<Element> _addr_v) {
    ref Element v = ref _addr_v.val;
 
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref array<byte> @out = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_@out);
    return v.bytes(_addr_out);

}

private static slice<byte> bytes(this ptr<Element> _addr_v, ptr<array<byte>> _addr_@out) {
    ref Element v = ref _addr_v.val;
    ref array<byte> @out = ref _addr_@out.val;

    var t = v.val;
    t.reduce();

    array<byte> buf = new array<byte>(8);
    {
        array<ulong> i__prev1 = i;

        foreach (var (__i, __l) in new array<ulong>(new ulong[] { t.l0, t.l1, t.l2, t.l3, t.l4 })) {
            i = __i;
            l = __l;
            var bitsOffset = i * 51;
            binary.LittleEndian.PutUint64(buf[..], l << (int)(uint(bitsOffset % 8)));
            {
                array<ulong> i__prev2 = i;

                foreach (var (__i, __bb) in buf) {
                    i = __i;
                    bb = __bb;
                    var off = bitsOffset / 8 + i;
                    if (off >= len(out)) {
                        break;
                    }
                    out[off] |= bb;
                }

                i = i__prev2;
            }
        }
        i = i__prev1;
    }

    return out[..];

}

// Equal returns 1 if v and u are equal, and 0 otherwise.
private static nint Equal(this ptr<Element> _addr_v, ptr<Element> _addr_u) {
    ref Element v = ref _addr_v.val;
    ref Element u = ref _addr_u.val;

    var sa = u.Bytes();
    var sv = v.Bytes();
    return subtle.ConstantTimeCompare(sa, sv);

}

// mask64Bits returns 0xffffffff if cond is 1, and 0 otherwise.
private static ulong mask64Bits(nint cond) {
    return ~(uint64(cond) - 1);
}

// Select sets v to a if cond == 1, and to b if cond == 0.
private static ptr<Element> Select(this ptr<Element> _addr_v, ptr<Element> _addr_a, ptr<Element> _addr_b, nint cond) {
    ref Element v = ref _addr_v.val;
    ref Element a = ref _addr_a.val;
    ref Element b = ref _addr_b.val;

    var m = mask64Bits(cond);
    v.l0 = (m & a.l0) | (~m & b.l0);
    v.l1 = (m & a.l1) | (~m & b.l1);
    v.l2 = (m & a.l2) | (~m & b.l2);
    v.l3 = (m & a.l3) | (~m & b.l3);
    v.l4 = (m & a.l4) | (~m & b.l4);
    return _addr_v!;
}

// Swap swaps v and u if cond == 1 or leaves them unchanged if cond == 0, and returns v.
private static void Swap(this ptr<Element> _addr_v, ptr<Element> _addr_u, nint cond) {
    ref Element v = ref _addr_v.val;
    ref Element u = ref _addr_u.val;

    var m = mask64Bits(cond);
    var t = m & (v.l0 ^ u.l0);
    v.l0 ^= t;
    u.l0 ^= t;
    t = m & (v.l1 ^ u.l1);
    v.l1 ^= t;
    u.l1 ^= t;
    t = m & (v.l2 ^ u.l2);
    v.l2 ^= t;
    u.l2 ^= t;
    t = m & (v.l3 ^ u.l3);
    v.l3 ^= t;
    u.l3 ^= t;
    t = m & (v.l4 ^ u.l4);
    v.l4 ^= t;
    u.l4 ^= t;
}

// IsNegative returns 1 if v is negative, and 0 otherwise.
private static nint IsNegative(this ptr<Element> _addr_v) {
    ref Element v = ref _addr_v.val;

    return int(v.Bytes()[0] & 1);
}

// Absolute sets v to |u|, and returns v.
private static ptr<Element> Absolute(this ptr<Element> _addr_v, ptr<Element> _addr_u) {
    ref Element v = ref _addr_v.val;
    ref Element u = ref _addr_u.val;

    return _addr_v.Select(@new<Element>().Negate(u), u, u.IsNegative())!;
}

// Multiply sets v = x * y, and returns v.
private static ptr<Element> Multiply(this ptr<Element> _addr_v, ptr<Element> _addr_x, ptr<Element> _addr_y) {
    ref Element v = ref _addr_v.val;
    ref Element x = ref _addr_x.val;
    ref Element y = ref _addr_y.val;

    feMul(v, x, y);
    return _addr_v!;
}

// Square sets v = x * x, and returns v.
private static ptr<Element> Square(this ptr<Element> _addr_v, ptr<Element> _addr_x) {
    ref Element v = ref _addr_v.val;
    ref Element x = ref _addr_x.val;

    feSquare(v, x);
    return _addr_v!;
}

// Mult32 sets v = x * y, and returns v.
private static ptr<Element> Mult32(this ptr<Element> _addr_v, ptr<Element> _addr_x, uint y) {
    ref Element v = ref _addr_v.val;
    ref Element x = ref _addr_x.val;

    var (x0lo, x0hi) = mul51(x.l0, y);
    var (x1lo, x1hi) = mul51(x.l1, y);
    var (x2lo, x2hi) = mul51(x.l2, y);
    var (x3lo, x3hi) = mul51(x.l3, y);
    var (x4lo, x4hi) = mul51(x.l4, y);
    v.l0 = x0lo + 19 * x4hi; // carried over per the reduction identity
    v.l1 = x1lo + x0hi;
    v.l2 = x2lo + x1hi;
    v.l3 = x3lo + x2hi;
    v.l4 = x4lo + x3hi; 
    // The hi portions are going to be only 32 bits, plus any previous excess,
    // so we can skip the carry propagation.
    return _addr_v!;

}

// mul51 returns lo + hi * 2⁵¹ = a * b.
private static (ulong, ulong) mul51(ulong a, uint b) {
    ulong lo = default;
    ulong hi = default;

    var (mh, ml) = bits.Mul64(a, uint64(b));
    lo = ml & maskLow51Bits;
    hi = (mh << 13) | (ml >> 51);
    return ;
}

// Pow22523 set v = x^((p-5)/8), and returns v. (p-5)/8 is 2^252-3.
private static ptr<Element> Pow22523(this ptr<Element> _addr_v, ptr<Element> _addr_x) {
    ref Element v = ref _addr_v.val;
    ref Element x = ref _addr_x.val;

    ref Element t0 = ref heap(out ptr<Element> _addr_t0);    ref Element t1 = ref heap(out ptr<Element> _addr_t1);    ref Element t2 = ref heap(out ptr<Element> _addr_t2);



    t0.Square(x); // x^2
    t1.Square(_addr_t0); // x^4
    t1.Square(_addr_t1); // x^8
    t1.Multiply(x, _addr_t1); // x^9
    t0.Multiply(_addr_t0, _addr_t1); // x^11
    t0.Square(_addr_t0); // x^22
    t0.Multiply(_addr_t1, _addr_t0); // x^31
    t1.Square(_addr_t0); // x^62
    {
        nint i__prev1 = i;

        for (nint i = 1; i < 5; i++) { // x^992
            t1.Square(_addr_t1);

        }

        i = i__prev1;
    }
    t0.Multiply(_addr_t1, _addr_t0); // x^1023 -> 1023 = 2^10 - 1
    t1.Square(_addr_t0); // 2^11 - 2
    {
        nint i__prev1 = i;

        for (i = 1; i < 10; i++) { // 2^20 - 2^10
            t1.Square(_addr_t1);

        }

        i = i__prev1;
    }
    t1.Multiply(_addr_t1, _addr_t0); // 2^20 - 1
    t2.Square(_addr_t1); // 2^21 - 2
    {
        nint i__prev1 = i;

        for (i = 1; i < 20; i++) { // 2^40 - 2^20
            t2.Square(_addr_t2);

        }

        i = i__prev1;
    }
    t1.Multiply(_addr_t2, _addr_t1); // 2^40 - 1
    t1.Square(_addr_t1); // 2^41 - 2
    {
        nint i__prev1 = i;

        for (i = 1; i < 10; i++) { // 2^50 - 2^10
            t1.Square(_addr_t1);

        }

        i = i__prev1;
    }
    t0.Multiply(_addr_t1, _addr_t0); // 2^50 - 1
    t1.Square(_addr_t0); // 2^51 - 2
    {
        nint i__prev1 = i;

        for (i = 1; i < 50; i++) { // 2^100 - 2^50
            t1.Square(_addr_t1);

        }

        i = i__prev1;
    }
    t1.Multiply(_addr_t1, _addr_t0); // 2^100 - 1
    t2.Square(_addr_t1); // 2^101 - 2
    {
        nint i__prev1 = i;

        for (i = 1; i < 100; i++) { // 2^200 - 2^100
            t2.Square(_addr_t2);

        }

        i = i__prev1;
    }
    t1.Multiply(_addr_t2, _addr_t1); // 2^200 - 1
    t1.Square(_addr_t1); // 2^201 - 2
    {
        nint i__prev1 = i;

        for (i = 1; i < 50; i++) { // 2^250 - 2^50
            t1.Square(_addr_t1);

        }

        i = i__prev1;
    }
    t0.Multiply(_addr_t1, _addr_t0); // 2^250 - 1
    t0.Square(_addr_t0); // 2^251 - 2
    t0.Square(_addr_t0); // 2^252 - 4
    return _addr_v.Multiply(_addr_t0, x)!; // 2^252 - 3 -> x^(2^252-3)
}

// sqrtM1 is 2^((p-1)/4), which squared is equal to -1 by Euler's Criterion.
private static ptr<Element> sqrtM1 = addr(new Element(1718705420411056,234908883556509,2233514472574048,2117202627021982,765476049583133));

// SqrtRatio sets r to the non-negative square root of the ratio of u and v.
//
// If u/v is square, SqrtRatio returns r and 1. If u/v is not square, SqrtRatio
// sets r according to Section 4.3 of draft-irtf-cfrg-ristretto255-decaf448-00,
// and returns r and 0.
private static (ptr<Element>, nint) SqrtRatio(this ptr<Element> _addr_r, ptr<Element> _addr_u, ptr<Element> _addr_v) {
    ptr<Element> rr = default!;
    nint wasSquare = default;
    ref Element r = ref _addr_r.val;
    ref Element u = ref _addr_u.val;
    ref Element v = ref _addr_v.val;

    Element a = default;    Element b = default; 

    // r = (u * v3) * (u * v7)^((p-5)/8)
 

    // r = (u * v3) * (u * v7)^((p-5)/8)
    var v2 = a.Square(v);
    var uv3 = b.Multiply(u, b.Multiply(v2, v));
    var uv7 = a.Multiply(uv3, a.Square(v2));
    r.Multiply(uv3, r.Pow22523(uv7));

    var check = a.Multiply(v, a.Square(r)); // check = v * r^2

    var uNeg = b.Negate(u);
    var correctSignSqrt = check.Equal(u);
    var flippedSignSqrt = check.Equal(uNeg);
    var flippedSignSqrtI = check.Equal(uNeg.Multiply(uNeg, sqrtM1));

    var rPrime = b.Multiply(r, sqrtM1); // r_prime = SQRT_M1 * r
    // r = CT_SELECT(r_prime IF flipped_sign_sqrt | flipped_sign_sqrt_i ELSE r)
    r.Select(rPrime, r, flippedSignSqrt | flippedSignSqrtI);

    r.Absolute(r); // Choose the nonnegative square root.
    return (_addr_r!, correctSignSqrt | flippedSignSqrt);

}

} // end field_package
