// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package elliptic -- go2cs converted at 2020 August 29 08:29:53 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Go\src\crypto\elliptic\p224.go
// This is a constant-time, 32-bit implementation of P224. See FIPS 186-3,
// section D.2.2.
//
// See http://www.imperialviolet.org/2010/12/04/ecc.html ([1]) for background.

using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class elliptic_package
    {
        private static p224Curve p224 = default;

        private partial struct p224Curve
        {
            public ref CurveParams CurveParams => ref CurveParams_ptr;
            public p224FieldElement gx;
            public p224FieldElement gy;
            public p224FieldElement b;
        }

        private static void initP224()
        { 
            // See FIPS 186-3, section D.2.2
            p224.CurveParams = ref new CurveParams(Name:"P-224");
            p224.P, _ = @new<big.Int>().SetString("26959946667150639794667015087019630673557916260026308143510066298881", 10L);
            p224.N, _ = @new<big.Int>().SetString("26959946667150639794667015087019625940457807714424391721682722368061", 10L);
            p224.B, _ = @new<big.Int>().SetString("b4050a850c04b3abf54132565044b0b7d7bfd8ba270b39432355ffb4", 16L);
            p224.Gx, _ = @new<big.Int>().SetString("b70e0cbd6bb4bf7f321390b94a03c1d356c21122343280d6115c1d21", 16L);
            p224.Gy, _ = @new<big.Int>().SetString("bd376388b5f723fb4c22dfe6cd4375a05a07476444d5819985007e34", 16L);
            p224.BitSize = 224L;

            p224FromBig(ref p224.gx, p224.Gx);
            p224FromBig(ref p224.gy, p224.Gy);
            p224FromBig(ref p224.b, p224.B);
        }

        // P224 returns a Curve which implements P-224 (see FIPS 186-3, section D.2.2).
        //
        // The cryptographic operations are implemented using constant-time algorithms.
        public static Curve P224()
        {
            initonce.Do(initAll);
            return p224;
        }

        private static ref CurveParams Params(this p224Curve curve)
        {
            return curve.CurveParams;
        }

        private static bool IsOnCurve(this p224Curve curve, ref big.Int bigX, ref big.Int bigY)
        {
            p224FieldElement x = default;            p224FieldElement y = default;

            p224FromBig(ref x, bigX);
            p224FromBig(ref y, bigY); 

            // y² = x³ - 3x + b
            p224LargeFieldElement tmp = default;
            p224FieldElement x3 = default;
            p224Square(ref x3, ref x, ref tmp);
            p224Mul(ref x3, ref x3, ref x, ref tmp);

            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    x[i] *= 3L;
                }


                i = i__prev1;
            }
            p224Sub(ref x3, ref x3, ref x);
            p224Reduce(ref x3);
            p224Add(ref x3, ref x3, ref curve.b);
            p224Contract(ref x3, ref x3);

            p224Square(ref y, ref y, ref tmp);
            p224Contract(ref y, ref y);

            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    if (y[i] != x3[i])
                    {
                        return false;
                    }
                }


                i = i__prev1;
            }
            return true;
        }

        private static (ref big.Int, ref big.Int) Add(this p224Curve _p0, ref big.Int bigX1, ref big.Int bigY1, ref big.Int bigX2, ref big.Int bigY2)
        {
            p224FieldElement x1 = default;            p224FieldElement y1 = default;            p224FieldElement z1 = default;            p224FieldElement x2 = default;            p224FieldElement y2 = default;            p224FieldElement z2 = default;            p224FieldElement x3 = default;            p224FieldElement y3 = default;            p224FieldElement z3 = default;



            p224FromBig(ref x1, bigX1);
            p224FromBig(ref y1, bigY1);
            if (bigX1.Sign() != 0L || bigY1.Sign() != 0L)
            {
                z1[0L] = 1L;
            }
            p224FromBig(ref x2, bigX2);
            p224FromBig(ref y2, bigY2);
            if (bigX2.Sign() != 0L || bigY2.Sign() != 0L)
            {
                z2[0L] = 1L;
            }
            p224AddJacobian(ref x3, ref y3, ref z3, ref x1, ref y1, ref z1, ref x2, ref y2, ref z2);
            return p224ToAffine(ref x3, ref y3, ref z3);
        }

        private static (ref big.Int, ref big.Int) Double(this p224Curve _p0, ref big.Int bigX1, ref big.Int bigY1)
        {
            p224FieldElement x1 = default;            p224FieldElement y1 = default;            p224FieldElement z1 = default;            p224FieldElement x2 = default;            p224FieldElement y2 = default;            p224FieldElement z2 = default;



            p224FromBig(ref x1, bigX1);
            p224FromBig(ref y1, bigY1);
            z1[0L] = 1L;

            p224DoubleJacobian(ref x2, ref y2, ref z2, ref x1, ref y1, ref z1);
            return p224ToAffine(ref x2, ref y2, ref z2);
        }

        private static (ref big.Int, ref big.Int) ScalarMult(this p224Curve _p0, ref big.Int bigX1, ref big.Int bigY1, slice<byte> scalar)
        {
            p224FieldElement x1 = default;            p224FieldElement y1 = default;            p224FieldElement z1 = default;            p224FieldElement x2 = default;            p224FieldElement y2 = default;            p224FieldElement z2 = default;



            p224FromBig(ref x1, bigX1);
            p224FromBig(ref y1, bigY1);
            z1[0L] = 1L;

            p224ScalarMult(ref x2, ref y2, ref z2, ref x1, ref y1, ref z1, scalar);
            return p224ToAffine(ref x2, ref y2, ref z2);
        }

        private static (ref big.Int, ref big.Int) ScalarBaseMult(this p224Curve curve, slice<byte> scalar)
        {
            p224FieldElement z1 = default;            p224FieldElement x2 = default;            p224FieldElement y2 = default;            p224FieldElement z2 = default;



            z1[0L] = 1L;
            p224ScalarMult(ref x2, ref y2, ref z2, ref curve.gx, ref curve.gy, ref z1, scalar);
            return p224ToAffine(ref x2, ref y2, ref z2);
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
        private partial struct p224FieldElement // : array<uint>
        {
        }

        // p224P is the order of the field, represented as a p224FieldElement.
        private static array<uint> p224P = new array<uint>(new uint[] { 1, 0, 0, 0xffff000, 0xfffffff, 0xfffffff, 0xfffffff, 0xfffffff });

        // p224IsZero returns 1 if a == 0 mod p and 0 otherwise.
        //
        // a[i] < 2**29
        private static uint p224IsZero(ref p224FieldElement a)
        { 
            // Since a p224FieldElement contains 224 bits there are two possible
            // representations of 0: 0 and p.
            p224FieldElement minimal = default;
            p224Contract(ref minimal, a);

            uint isZero = default;            uint isP = default;

            foreach (var (i, v) in minimal)
            {
                isZero |= v;
                isP |= v - p224P[i];
            } 

            // If either isZero or isP is 0, then we should return 1.
            isZero |= isZero >> (int)(16L);
            isZero |= isZero >> (int)(8L);
            isZero |= isZero >> (int)(4L);
            isZero |= isZero >> (int)(2L);
            isZero |= isZero >> (int)(1L);

            isP |= isP >> (int)(16L);
            isP |= isP >> (int)(8L);
            isP |= isP >> (int)(4L);
            isP |= isP >> (int)(2L);
            isP |= isP >> (int)(1L); 

            // For isZero and isP, the LSB is 0 iff all the bits are zero.
            var result = isZero & isP;
            result = (~result) & 1L;

            return result;
        }

        // p224Add computes *out = a+b
        //
        // a[i] + b[i] < 2**32
        private static void p224Add(ref p224FieldElement @out, ref p224FieldElement a, ref p224FieldElement b)
        {
            for (long i = 0L; i < 8L; i++)
            {
                out[i] = a[i] + b[i];
            }

        }

        private static readonly long two31p3 = 1L << (int)(31L) + 1L << (int)(3L);

        private static readonly long two31m3 = 1L << (int)(31L) - 1L << (int)(3L);

        private static readonly long two31m15m3 = 1L << (int)(31L) - 1L << (int)(15L) - 1L << (int)(3L);

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
        private static void p224Sub(ref p224FieldElement @out, ref p224FieldElement a, ref p224FieldElement b)
        {
            for (long i = 0L; i < 8L; i++)
            {
                out[i] = a[i] + p224ZeroModP31[i] - b[i];
            }

        }

        // LargeFieldElement also represents an element of the field. The limbs are
        // still spaced 28-bits apart and in little-endian order. So the limbs are at
        // 0, 28, 56, ..., 392 bits, each 64-bits wide.
        private partial struct p224LargeFieldElement // : array<ulong>
        {
        }

        private static readonly long two63p35 = 1L << (int)(63L) + 1L << (int)(35L);

        private static readonly long two63m35 = 1L << (int)(63L) - 1L << (int)(35L);

        private static readonly long two63m35m19 = 1L << (int)(63L) - 1L << (int)(35L) - 1L << (int)(19L);

        // p224ZeroModP63 is 0 mod p where bit 63 is set in all limbs. See the section
        // "Subtraction" in [1] for why.


        // p224ZeroModP63 is 0 mod p where bit 63 is set in all limbs. See the section
        // "Subtraction" in [1] for why.
        private static array<ulong> p224ZeroModP63 = new array<ulong>(new ulong[] { two63p35, two63m35, two63m35, two63m35, two63m35m19, two63m35, two63m35, two63m35 });

        private static readonly ulong bottom12Bits = 0xfffUL;

        private static readonly ulong bottom28Bits = 0xfffffffUL;

        // p224Mul computes *out = a*b
        //
        // a[i] < 2**29, b[i] < 2**30 (or vice versa)
        // out[i] < 2**29


        // p224Mul computes *out = a*b
        //
        // a[i] < 2**29, b[i] < 2**30 (or vice versa)
        // out[i] < 2**29
        private static void p224Mul(ref p224FieldElement @out, ref p224FieldElement a, ref p224FieldElement b, ref p224LargeFieldElement tmp)
        {
            {
                long i__prev1 = i;

                for (long i = 0L; i < 15L; i++)
                {
                    tmp[i] = 0L;
                }


                i = i__prev1;
            }

            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    for (long j = 0L; j < 8L; j++)
                    {
                        tmp[i + j] += uint64(a[i]) * uint64(b[j]);
                    }

                }


                i = i__prev1;
            }

            p224ReduceLarge(out, tmp);
        }

        // Square computes *out = a*a
        //
        // a[i] < 2**29
        // out[i] < 2**29
        private static void p224Square(ref p224FieldElement @out, ref p224FieldElement a, ref p224LargeFieldElement tmp)
        {
            {
                long i__prev1 = i;

                for (long i = 0L; i < 15L; i++)
                {
                    tmp[i] = 0L;
                }


                i = i__prev1;
            }

            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    for (long j = 0L; j <= i; j++)
                    {
                        var r = uint64(a[i]) * uint64(a[j]);
                        if (i == j)
                        {
                            tmp[i + j] += r;
                        }
                        else
                        {
                            tmp[i + j] += r << (int)(1L);
                        }
                    }

                }


                i = i__prev1;
            }

            p224ReduceLarge(out, tmp);
        }

        // ReduceLarge converts a p224LargeFieldElement to a p224FieldElement.
        //
        // in[i] < 2**62
        private static void p224ReduceLarge(ref p224FieldElement @out, ref p224LargeFieldElement @in)
        {
            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    in[i] += p224ZeroModP63[i];
                } 

                // Eliminate the coefficients at 2**224 and greater.


                i = i__prev1;
            } 

            // Eliminate the coefficients at 2**224 and greater.
            {
                long i__prev1 = i;

                for (i = 14L; i >= 8L; i--)
                {
                    in[i - 8L] -= in[i];
                    in[i - 5L] += (in[i] & 0xffffUL) << (int)(12L);
                    in[i - 4L] += in[i] >> (int)(16L);
                }


                i = i__prev1;
            }
            in[8L] = 0L; 
            // in[0..8] < 2**64

            // As the values become small enough, we start to store them in |out|
            // and use 32-bit operations.
            {
                long i__prev1 = i;

                for (i = 1L; i < 8L; i++)
                {
                    in[i + 1L] += in[i] >> (int)(28L);
                    out[i] = uint32(in[i] & bottom28Bits);
                }


                i = i__prev1;
            }
            in[0L] -= in[8L];
            out[3L] += uint32(in[8L] & 0xffffUL) << (int)(12L);
            out[4L] += uint32(in[8L] >> (int)(16L)); 
            // in[0] < 2**64
            // out[3] < 2**29
            // out[4] < 2**29
            // out[1,2,5..7] < 2**28

            out[0L] = uint32(in[0L] & bottom28Bits);
            out[1L] += uint32((in[0L] >> (int)(28L)) & bottom28Bits);
            out[2L] += uint32(in[0L] >> (int)(56L)); 
            // out[0] < 2**28
            // out[1..4] < 2**29
            // out[5..7] < 2**28
        }

        // Reduce reduces the coefficients of a to smaller bounds.
        //
        // On entry: a[i] < 2**31 + 2**30
        // On exit: a[i] < 2**29
        private static void p224Reduce(ref p224FieldElement a)
        {
            for (long i = 0L; i < 7L; i++)
            {
                a[i + 1L] += a[i] >> (int)(28L);
                a[i] &= bottom28Bits;
            }

            var top = a[7L] >> (int)(28L);
            a[7L] &= bottom28Bits; 

            // top < 2**4
            var mask = top;
            mask |= mask >> (int)(2L);
            mask |= mask >> (int)(1L);
            mask <<= 31L;
            mask = uint32(int32(mask) >> (int)(31L)); 
            // Mask is all ones if top != 0, all zero otherwise

            a[0L] -= top;
            a[3L] += top << (int)(12L); 

            // We may have just made a[0] negative but, if we did, then we must
            // have added something to a[3], this it's > 2**12. Therefore we can
            // carry down to a[0].
            a[3L] -= 1L & mask;
            a[2L] += mask & (1L << (int)(28L) - 1L);
            a[1L] += mask & (1L << (int)(28L) - 1L);
            a[0L] += mask & (1L << (int)(28L));
        }

        // p224Invert calculates *out = in**-1 by computing in**(2**224 - 2**96 - 1),
        // i.e. Fermat's little theorem.
        private static void p224Invert(ref p224FieldElement @out, ref p224FieldElement @in)
        {
            p224FieldElement f1 = default;            p224FieldElement f2 = default;            p224FieldElement f3 = default;            p224FieldElement f4 = default;

            p224LargeFieldElement c = default;

            p224Square(ref f1, in, ref c); // 2
            p224Mul(ref f1, ref f1, in, ref c); // 2**2 - 1
            p224Square(ref f1, ref f1, ref c); // 2**3 - 2
            p224Mul(ref f1, ref f1, in, ref c); // 2**3 - 1
            p224Square(ref f2, ref f1, ref c); // 2**4 - 2
            p224Square(ref f2, ref f2, ref c); // 2**5 - 4
            p224Square(ref f2, ref f2, ref c); // 2**6 - 8
            p224Mul(ref f1, ref f1, ref f2, ref c); // 2**6 - 1
            p224Square(ref f2, ref f1, ref c); // 2**7 - 2
            {
                long i__prev1 = i;

                for (long i = 0L; i < 5L; i++)
                { // 2**12 - 2**6
                    p224Square(ref f2, ref f2, ref c);
                }


                i = i__prev1;
            }
            p224Mul(ref f2, ref f2, ref f1, ref c); // 2**12 - 1
            p224Square(ref f3, ref f2, ref c); // 2**13 - 2
            {
                long i__prev1 = i;

                for (i = 0L; i < 11L; i++)
                { // 2**24 - 2**12
                    p224Square(ref f3, ref f3, ref c);
                }


                i = i__prev1;
            }
            p224Mul(ref f2, ref f3, ref f2, ref c); // 2**24 - 1
            p224Square(ref f3, ref f2, ref c); // 2**25 - 2
            {
                long i__prev1 = i;

                for (i = 0L; i < 23L; i++)
                { // 2**48 - 2**24
                    p224Square(ref f3, ref f3, ref c);
                }


                i = i__prev1;
            }
            p224Mul(ref f3, ref f3, ref f2, ref c); // 2**48 - 1
            p224Square(ref f4, ref f3, ref c); // 2**49 - 2
            {
                long i__prev1 = i;

                for (i = 0L; i < 47L; i++)
                { // 2**96 - 2**48
                    p224Square(ref f4, ref f4, ref c);
                }


                i = i__prev1;
            }
            p224Mul(ref f3, ref f3, ref f4, ref c); // 2**96 - 1
            p224Square(ref f4, ref f3, ref c); // 2**97 - 2
            {
                long i__prev1 = i;

                for (i = 0L; i < 23L; i++)
                { // 2**120 - 2**24
                    p224Square(ref f4, ref f4, ref c);
                }


                i = i__prev1;
            }
            p224Mul(ref f2, ref f4, ref f2, ref c); // 2**120 - 1
            {
                long i__prev1 = i;

                for (i = 0L; i < 6L; i++)
                { // 2**126 - 2**6
                    p224Square(ref f2, ref f2, ref c);
                }


                i = i__prev1;
            }
            p224Mul(ref f1, ref f1, ref f2, ref c); // 2**126 - 1
            p224Square(ref f1, ref f1, ref c); // 2**127 - 2
            p224Mul(ref f1, ref f1, in, ref c); // 2**127 - 1
            {
                long i__prev1 = i;

                for (i = 0L; i < 97L; i++)
                { // 2**224 - 2**97
                    p224Square(ref f1, ref f1, ref c);
                }


                i = i__prev1;
            }
            p224Mul(out, ref f1, ref f3, ref c); // 2**224 - 2**96 - 1
        }

        // p224Contract converts a FieldElement to its unique, minimal form.
        //
        // On entry, in[i] < 2**29
        // On exit, in[i] < 2**28
        private static void p224Contract(ref p224FieldElement @out, ref p224FieldElement @in)
        {
            copy(out[..], in[..]);

            {
                long i__prev1 = i;

                for (long i = 0L; i < 7L; i++)
                {
                    out[i + 1L] += out[i] >> (int)(28L);
                    out[i] &= bottom28Bits;
                }


                i = i__prev1;
            }
            var top = out[7L] >> (int)(28L);
            out[7L] &= bottom28Bits;

            out[0L] -= top;
            out[3L] += top << (int)(12L); 

            // We may just have made out[i] negative. So we carry down. If we made
            // out[0] negative then we know that out[3] is sufficiently positive
            // because we just added to it.
            {
                long i__prev1 = i;

                for (i = 0L; i < 3L; i++)
                {
                    var mask = uint32(int32(out[i]) >> (int)(31L));
                    out[i] += (1L << (int)(28L)) & mask;
                    out[i + 1L] -= 1L & mask;
                } 

                // We might have pushed out[3] over 2**28 so we perform another, partial,
                // carry chain.


                i = i__prev1;
            } 

            // We might have pushed out[3] over 2**28 so we perform another, partial,
            // carry chain.
            {
                long i__prev1 = i;

                for (i = 3L; i < 7L; i++)
                {
                    out[i + 1L] += out[i] >> (int)(28L);
                    out[i] &= bottom28Bits;
                }


                i = i__prev1;
            }
            top = out[7L] >> (int)(28L);
            out[7L] &= bottom28Bits; 

            // Eliminate top while maintaining the same value mod p.
            out[0L] -= top;
            out[3L] += top << (int)(12L); 

            // There are two cases to consider for out[3]:
            //   1) The first time that we eliminated top, we didn't push out[3] over
            //      2**28. In this case, the partial carry chain didn't change any values
            //      and top is zero.
            //   2) We did push out[3] over 2**28 the first time that we eliminated top.
            //      The first value of top was in [0..16), therefore, prior to eliminating
            //      the first top, 0xfff1000 <= out[3] <= 0xfffffff. Therefore, after
            //      overflowing and being reduced by the second carry chain, out[3] <=
            //      0xf000. Thus it cannot have overflowed when we eliminated top for the
            //      second time.

            // Again, we may just have made out[0] negative, so do the same carry down.
            // As before, if we made out[0] negative then we know that out[3] is
            // sufficiently positive.
            {
                long i__prev1 = i;

                for (i = 0L; i < 3L; i++)
                {
                    mask = uint32(int32(out[i]) >> (int)(31L));
                    out[i] += (1L << (int)(28L)) & mask;
                    out[i + 1L] -= 1L & mask;
                } 

                // Now we see if the value is >= p and, if so, subtract p.

                // First we build a mask from the top four limbs, which must all be
                // equal to bottom28Bits if the whole value is >= p. If top4AllOnes
                // ends up with any zero bits in the bottom 28 bits, then this wasn't
                // true.


                i = i__prev1;
            } 

            // Now we see if the value is >= p and, if so, subtract p.

            // First we build a mask from the top four limbs, which must all be
            // equal to bottom28Bits if the whole value is >= p. If top4AllOnes
            // ends up with any zero bits in the bottom 28 bits, then this wasn't
            // true.
            var top4AllOnes = uint32(0xffffffffUL);
            {
                long i__prev1 = i;

                for (i = 4L; i < 8L; i++)
                {
                    top4AllOnes &= out[i];
                }


                i = i__prev1;
            }
            top4AllOnes |= 0xf0000000UL; 
            // Now we replicate any zero bits to all the bits in top4AllOnes.
            top4AllOnes &= top4AllOnes >> (int)(16L);
            top4AllOnes &= top4AllOnes >> (int)(8L);
            top4AllOnes &= top4AllOnes >> (int)(4L);
            top4AllOnes &= top4AllOnes >> (int)(2L);
            top4AllOnes &= top4AllOnes >> (int)(1L);
            top4AllOnes = uint32(int32(top4AllOnes << (int)(31L)) >> (int)(31L)); 

            // Now we test whether the bottom three limbs are non-zero.
            var bottom3NonZero = out[0L] | out[1L] | out[2L];
            bottom3NonZero |= bottom3NonZero >> (int)(16L);
            bottom3NonZero |= bottom3NonZero >> (int)(8L);
            bottom3NonZero |= bottom3NonZero >> (int)(4L);
            bottom3NonZero |= bottom3NonZero >> (int)(2L);
            bottom3NonZero |= bottom3NonZero >> (int)(1L);
            bottom3NonZero = uint32(int32(bottom3NonZero << (int)(31L)) >> (int)(31L)); 

            // Everything depends on the value of out[3].
            //    If it's > 0xffff000 and top4AllOnes != 0 then the whole value is >= p
            //    If it's = 0xffff000 and top4AllOnes != 0 and bottom3NonZero != 0,
            //      then the whole value is >= p
            //    If it's < 0xffff000, then the whole value is < p
            var n = out[3L] - 0xffff000UL;
            var out3Equal = n;
            out3Equal |= out3Equal >> (int)(16L);
            out3Equal |= out3Equal >> (int)(8L);
            out3Equal |= out3Equal >> (int)(4L);
            out3Equal |= out3Equal >> (int)(2L);
            out3Equal |= out3Equal >> (int)(1L);
            out3Equal = ~uint32(int32(out3Equal << (int)(31L)) >> (int)(31L)); 

            // If out[3] > 0xffff000 then n's MSB will be zero.
            var out3GT = ~uint32(int32(n) >> (int)(31L));

            mask = top4AllOnes & ((out3Equal & bottom3NonZero) | out3GT);
            out[0L] -= 1L & mask;
            out[3L] -= 0xffff000UL & mask;
            out[4L] -= 0xfffffffUL & mask;
            out[5L] -= 0xfffffffUL & mask;
            out[6L] -= 0xfffffffUL & mask;
            out[7L] -= 0xfffffffUL & mask;
        }

        // Group element functions.
        //
        // These functions deal with group elements. The group is an elliptic curve
        // group with a = -3 defined in FIPS 186-3, section D.2.2.

        // p224AddJacobian computes *out = a+b where a != b.
        private static void p224AddJacobian(ref p224FieldElement x3, ref p224FieldElement y3, ref p224FieldElement z3, ref p224FieldElement x1, ref p224FieldElement y1, ref p224FieldElement z1, ref p224FieldElement x2, ref p224FieldElement y2, ref p224FieldElement z2)
        { 
            // See http://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#addition-p224Add-2007-bl
            p224FieldElement z1z1 = default;            p224FieldElement z2z2 = default;            p224FieldElement u1 = default;            p224FieldElement u2 = default;            p224FieldElement s1 = default;            p224FieldElement s2 = default;            p224FieldElement h = default;            p224FieldElement i = default;            p224FieldElement j = default;            p224FieldElement r = default;            p224FieldElement v = default;

            p224LargeFieldElement c = default;

            var z1IsZero = p224IsZero(z1);
            var z2IsZero = p224IsZero(z2); 

            // Z1Z1 = Z1²
            p224Square(ref z1z1, z1, ref c); 
            // Z2Z2 = Z2²
            p224Square(ref z2z2, z2, ref c); 
            // U1 = X1*Z2Z2
            p224Mul(ref u1, x1, ref z2z2, ref c); 
            // U2 = X2*Z1Z1
            p224Mul(ref u2, x2, ref z1z1, ref c); 
            // S1 = Y1*Z2*Z2Z2
            p224Mul(ref s1, z2, ref z2z2, ref c);
            p224Mul(ref s1, y1, ref s1, ref c); 
            // S2 = Y2*Z1*Z1Z1
            p224Mul(ref s2, z1, ref z1z1, ref c);
            p224Mul(ref s2, y2, ref s2, ref c); 
            // H = U2-U1
            p224Sub(ref h, ref u2, ref u1);
            p224Reduce(ref h);
            var xEqual = p224IsZero(ref h); 
            // I = (2*H)²
            {
                p224FieldElement j__prev1 = j;

                for (j = 0L; j < 8L; j++)
                {
                    i[j] = h[j] << (int)(1L);
                }


                j = j__prev1;
            }
            p224Reduce(ref i);
            p224Square(ref i, ref i, ref c); 
            // J = H*I
            p224Mul(ref j, ref h, ref i, ref c); 
            // r = 2*(S2-S1)
            p224Sub(ref r, ref s2, ref s1);
            p224Reduce(ref r);
            var yEqual = p224IsZero(ref r);
            if (xEqual == 1L && yEqual == 1L && z1IsZero == 0L && z2IsZero == 0L)
            {
                p224DoubleJacobian(x3, y3, z3, x1, y1, z1);
                return;
            }
            {
                p224FieldElement i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    r[i] <<= 1L;
                }


                i = i__prev1;
            }
            p224Reduce(ref r); 
            // V = U1*I
            p224Mul(ref v, ref u1, ref i, ref c); 
            // Z3 = ((Z1+Z2)²-Z1Z1-Z2Z2)*H
            p224Add(ref z1z1, ref z1z1, ref z2z2);
            p224Add(ref z2z2, z1, z2);
            p224Reduce(ref z2z2);
            p224Square(ref z2z2, ref z2z2, ref c);
            p224Sub(z3, ref z2z2, ref z1z1);
            p224Reduce(z3);
            p224Mul(z3, z3, ref h, ref c); 
            // X3 = r²-J-2*V
            {
                p224FieldElement i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    z1z1[i] = v[i] << (int)(1L);
                }


                i = i__prev1;
            }
            p224Add(ref z1z1, ref j, ref z1z1);
            p224Reduce(ref z1z1);
            p224Square(x3, ref r, ref c);
            p224Sub(x3, x3, ref z1z1);
            p224Reduce(x3); 
            // Y3 = r*(V-X3)-2*S1*J
            {
                p224FieldElement i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    s1[i] <<= 1L;
                }


                i = i__prev1;
            }
            p224Mul(ref s1, ref s1, ref j, ref c);
            p224Sub(ref z1z1, ref v, x3);
            p224Reduce(ref z1z1);
            p224Mul(ref z1z1, ref z1z1, ref r, ref c);
            p224Sub(y3, ref z1z1, ref s1);
            p224Reduce(y3);

            p224CopyConditional(x3, x2, z1IsZero);
            p224CopyConditional(x3, x1, z2IsZero);
            p224CopyConditional(y3, y2, z1IsZero);
            p224CopyConditional(y3, y1, z2IsZero);
            p224CopyConditional(z3, z2, z1IsZero);
            p224CopyConditional(z3, z1, z2IsZero);
        }

        // p224DoubleJacobian computes *out = a+a.
        private static void p224DoubleJacobian(ref p224FieldElement x3, ref p224FieldElement y3, ref p224FieldElement z3, ref p224FieldElement x1, ref p224FieldElement y1, ref p224FieldElement z1)
        {
            p224FieldElement delta = default;            p224FieldElement gamma = default;            p224FieldElement beta = default;            p224FieldElement alpha = default;            p224FieldElement t = default;

            p224LargeFieldElement c = default;

            p224Square(ref delta, z1, ref c);
            p224Square(ref gamma, y1, ref c);
            p224Mul(ref beta, x1, ref gamma, ref c); 

            // alpha = 3*(X1-delta)*(X1+delta)
            p224Add(ref t, x1, ref delta);
            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    t[i] += t[i] << (int)(1L);
                }


                i = i__prev1;
            }
            p224Reduce(ref t);
            p224Sub(ref alpha, x1, ref delta);
            p224Reduce(ref alpha);
            p224Mul(ref alpha, ref alpha, ref t, ref c); 

            // Z3 = (Y1+Z1)²-gamma-delta
            p224Add(z3, y1, z1);
            p224Reduce(z3);
            p224Square(z3, z3, ref c);
            p224Sub(z3, z3, ref gamma);
            p224Reduce(z3);
            p224Sub(z3, z3, ref delta);
            p224Reduce(z3); 

            // X3 = alpha²-8*beta
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    delta[i] = beta[i] << (int)(3L);
                }


                i = i__prev1;
            }
            p224Reduce(ref delta);
            p224Square(x3, ref alpha, ref c);
            p224Sub(x3, x3, ref delta);
            p224Reduce(x3); 

            // Y3 = alpha*(4*beta-X3)-8*gamma²
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    beta[i] <<= 2L;
                }


                i = i__prev1;
            }
            p224Sub(ref beta, ref beta, x3);
            p224Reduce(ref beta);
            p224Square(ref gamma, ref gamma, ref c);
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    gamma[i] <<= 3L;
                }


                i = i__prev1;
            }
            p224Reduce(ref gamma);
            p224Mul(y3, ref alpha, ref beta, ref c);
            p224Sub(y3, y3, ref gamma);
            p224Reduce(y3);
        }

        // p224CopyConditional sets *out = *in iff the least-significant-bit of control
        // is true, and it runs in constant time.
        private static void p224CopyConditional(ref p224FieldElement @out, ref p224FieldElement @in, uint control)
        {
            control <<= 31L;
            control = uint32(int32(control) >> (int)(31L));

            for (long i = 0L; i < 8L; i++)
            {
                out[i] ^= (out[i] ^ in[i]) & control;
            }

        }

        private static void p224ScalarMult(ref p224FieldElement outX, ref p224FieldElement outY, ref p224FieldElement outZ, ref p224FieldElement inX, ref p224FieldElement inY, ref p224FieldElement inZ, slice<byte> scalar)
        {
            p224FieldElement xx = default;            p224FieldElement yy = default;            p224FieldElement zz = default;

            for (long i = 0L; i < 8L; i++)
            {
                outX[i] = 0L;
                outY[i] = 0L;
                outZ[i] = 0L;
            }


            foreach (var (_, byte) in scalar)
            {
                for (var bitNum = uint(0L); bitNum < 8L; bitNum++)
                {
                    p224DoubleJacobian(outX, outY, outZ, outX, outY, outZ);
                    var bit = uint32((byte >> (int)((7L - bitNum))) & 1L);
                    p224AddJacobian(ref xx, ref yy, ref zz, inX, inY, inZ, outX, outY, outZ);
                    p224CopyConditional(outX, ref xx, bit);
                    p224CopyConditional(outY, ref yy, bit);
                    p224CopyConditional(outZ, ref zz, bit);
                }

            }
        }

        // p224ToAffine converts from Jacobian to affine form.
        private static (ref big.Int, ref big.Int) p224ToAffine(ref p224FieldElement x, ref p224FieldElement y, ref p224FieldElement z)
        {
            p224FieldElement zinv = default;            p224FieldElement zinvsq = default;            p224FieldElement outx = default;            p224FieldElement outy = default;

            p224LargeFieldElement tmp = default;

            {
                var isPointAtInfinity = p224IsZero(z);

                if (isPointAtInfinity == 1L)
                {
                    return (@new<big.Int>(), @new<big.Int>());
                }

            }

            p224Invert(ref zinv, z);
            p224Square(ref zinvsq, ref zinv, ref tmp);
            p224Mul(x, x, ref zinvsq, ref tmp);
            p224Mul(ref zinvsq, ref zinvsq, ref zinv, ref tmp);
            p224Mul(y, y, ref zinvsq, ref tmp);

            p224Contract(ref outx, x);
            p224Contract(ref outy, y);
            return (p224ToBig(ref outx), p224ToBig(ref outy));
        }

        // get28BitsFromEnd returns the least-significant 28 bits from buf>>shift,
        // where buf is interpreted as a big-endian number.
        private static (uint, slice<byte>) get28BitsFromEnd(slice<byte> buf, ulong shift)
        {
            uint ret = default;

            for (var i = uint(0L); i < 4L; i++)
            {
                byte b = default;
                {
                    var l = len(buf);

                    if (l > 0L)
                    {
                        b = buf[l - 1L]; 
                        // We don't remove the byte if we're about to return and we're not
                        // reading all of it.
                        if (i != 3L || shift == 4L)
                        {
                            buf = buf[..l - 1L];
                        }
                    }

                }
                ret |= uint32(b) << (int)((8L * i)) >> (int)(shift);
            }

            ret &= bottom28Bits;
            return (ret, buf);
        }

        // p224FromBig sets *out = *in.
        private static void p224FromBig(ref p224FieldElement @out, ref big.Int @in)
        {
            var bytes = @in.Bytes();
            out[0L], bytes = get28BitsFromEnd(bytes, 0L);
            out[1L], bytes = get28BitsFromEnd(bytes, 4L);
            out[2L], bytes = get28BitsFromEnd(bytes, 0L);
            out[3L], bytes = get28BitsFromEnd(bytes, 4L);
            out[4L], bytes = get28BitsFromEnd(bytes, 0L);
            out[5L], bytes = get28BitsFromEnd(bytes, 4L);
            out[6L], bytes = get28BitsFromEnd(bytes, 0L);
            out[7L], bytes = get28BitsFromEnd(bytes, 4L);
        }

        // p224ToBig returns in as a big.Int.
        private static ref big.Int p224ToBig(ref p224FieldElement @in)
        {
            array<byte> buf = new array<byte>(28L);
            buf[27L] = byte(in[0L]);
            buf[26L] = byte(in[0L] >> (int)(8L));
            buf[25L] = byte(in[0L] >> (int)(16L));
            buf[24L] = byte(((in[0L] >> (int)(24L)) & 0x0fUL) | (in[1L] << (int)(4L)) & 0xf0UL);

            buf[23L] = byte(in[1L] >> (int)(4L));
            buf[22L] = byte(in[1L] >> (int)(12L));
            buf[21L] = byte(in[1L] >> (int)(20L));

            buf[20L] = byte(in[2L]);
            buf[19L] = byte(in[2L] >> (int)(8L));
            buf[18L] = byte(in[2L] >> (int)(16L));
            buf[17L] = byte(((in[2L] >> (int)(24L)) & 0x0fUL) | (in[3L] << (int)(4L)) & 0xf0UL);

            buf[16L] = byte(in[3L] >> (int)(4L));
            buf[15L] = byte(in[3L] >> (int)(12L));
            buf[14L] = byte(in[3L] >> (int)(20L));

            buf[13L] = byte(in[4L]);
            buf[12L] = byte(in[4L] >> (int)(8L));
            buf[11L] = byte(in[4L] >> (int)(16L));
            buf[10L] = byte(((in[4L] >> (int)(24L)) & 0x0fUL) | (in[5L] << (int)(4L)) & 0xf0UL);

            buf[9L] = byte(in[5L] >> (int)(4L));
            buf[8L] = byte(in[5L] >> (int)(12L));
            buf[7L] = byte(in[5L] >> (int)(20L));

            buf[6L] = byte(in[6L]);
            buf[5L] = byte(in[6L] >> (int)(8L));
            buf[4L] = byte(in[6L] >> (int)(16L));
            buf[3L] = byte(((in[6L] >> (int)(24L)) & 0x0fUL) | (in[7L] << (int)(4L)) & 0xf0UL);

            buf[2L] = byte(in[7L] >> (int)(4L));
            buf[1L] = byte(in[7L] >> (int)(12L));
            buf[0L] = byte(in[7L] >> (int)(20L));

            return @new<big.Int>().SetBytes(buf[..]);
        }
    }
}}
