// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build s390x

// package elliptic -- go2cs converted at 2020 August 29 08:30:44 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Go\src\crypto\elliptic\p256_s390x.go
using subtle = go.crypto.subtle_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class elliptic_package
    {
        private partial struct p256CurveFast
        {
            public ref CurveParams CurveParams => ref CurveParams_ptr;
        }

        private partial struct p256Point
        {
            public array<byte> x;
            public array<byte> y;
            public array<byte> z;
        }

        private static Curve p256 = default;        private static ref array<array<p256Point>> p256PreFast = default;

        // hasVectorFacility reports whether the machine has the z/Architecture
        // vector facility installed and enabled.
        private static bool hasVectorFacility()
;

        private static var hasVX = hasVectorFacility();

        private static void initP256Arch()
        {
            if (hasVX)
            {>>MARKER:FUNCTION_hasVectorFacility_BLOCK_PREFIX<<
                p256 = new p256CurveFast(p256Params);
                initTable();
                return;
            } 

            // No vector support, use pure Go implementation.
            p256 = new p256Curve(p256Params);
            return;
        }

        private static ref CurveParams Params(this p256CurveFast curve)
        {
            return curve.CurveParams;
        }

        // Functions implemented in p256_asm_s390x.s
        // Montgomery multiplication modulo P256
        //
        //go:noescape
        private static void p256MulAsm(slice<byte> res, slice<byte> in1, slice<byte> in2)
;

        // Montgomery square modulo P256
        private static void p256Sqr(slice<byte> res, slice<byte> @in)
        {
            p256MulAsm(res, in, in);
        }

        // Montgomery multiplication by 1
        //
        //go:noescape
        private static void p256FromMont(slice<byte> res, slice<byte> @in)
;

        // iff cond == 1  val <- -val
        //
        //go:noescape
        private static void p256NegCond(ref p256Point val, long cond)
;

        // if cond == 0 res <- b; else res <- a
        //
        //go:noescape
        private static void p256MovCond(ref p256Point res, ref p256Point a, ref p256Point b, long cond)
;

        // Constant time table access
        //
        //go:noescape
        private static void p256Select(ref p256Point point, slice<p256Point> table, long idx)
;

        //go:noescape
        private static void p256SelectBase(ref p256Point point, slice<p256Point> table, long idx)
;

        // Montgomery multiplication modulo Ord(G)
        //
        //go:noescape
        private static void p256OrdMul(slice<byte> res, slice<byte> in1, slice<byte> in2)
;

        // Montgomery square modulo Ord(G), repeated n times
        private static void p256OrdSqr(slice<byte> res, slice<byte> @in, long n)
        {
            copy(res, in);
            {
                long i = 0L;

                while (i < n)
                {>>MARKER:FUNCTION_p256OrdMul_BLOCK_PREFIX<<
                    p256OrdMul(res, res, res);
                    i += 1L;
                }

            }
        }

        // Point add with P2 being affine point
        // If sign == 1 -> P2 = -P2
        // If sel == 0 -> P3 = P1
        // if zero == 0 -> P3 = P2
        //
        //go:noescape
        private static void p256PointAddAffineAsm(ref p256Point P3, ref p256Point P1, ref p256Point P2, long sign, long sel, long zero)
;

        // Point add
        //
        //go:noescape
        private static long p256PointAddAsm(ref p256Point P3, ref p256Point P1, ref p256Point P2)
;

        //go:noescape
        private static void p256PointDoubleAsm(ref p256Point P3, ref p256Point P1)
;

        private static ref big.Int Inverse(this p256CurveFast curve, ref big.Int k)
        {>>MARKER:FUNCTION_p256PointDoubleAsm_BLOCK_PREFIX<<
            if (k.Cmp(p256Params.N) >= 0L)
            {>>MARKER:FUNCTION_p256PointAddAsm_BLOCK_PREFIX<< 
                // This should never happen.
                ptr<big.Int> reducedK = @new<big.Int>().Mod(k, p256Params.N);
                k = reducedK;
            } 

            // table will store precomputed powers of x. The 32 bytes at index
            // i store x^(i+1).
            array<array<byte>> table = new array<array<byte>>(15L);

            var x = fromBig(k); 
            // This code operates in the Montgomery domain where R = 2^256 mod n
            // and n is the order of the scalar field. (See initP256 for the
            // value.) Elements in the Montgomery domain take the form a×R and
            // multiplication of x and y in the calculates (x × y × R^-1) mod n. RR
            // is R×R mod n thus the Montgomery multiplication x and RR gives x×R,
            // i.e. converts x into the Montgomery domain. Stored in BigEndian form
            byte RR = new slice<byte>(new byte[] { 0x66, 0xe1, 0x2d, 0x94, 0xf3, 0xd9, 0x56, 0x20, 0x28, 0x45, 0xb2, 0x39, 0x2b, 0x6b, 0xec, 0x59, 0x46, 0x99, 0x79, 0x9c, 0x49, 0xbd, 0x6f, 0xa6, 0x83, 0x24, 0x4c, 0x95, 0xbe, 0x79, 0xee, 0xa2 });

            p256OrdMul(table[0L][..], x, RR); 

            // Prepare the table, no need in constant time access, because the
            // power is not a secret. (Entry 0 is never used.)
            {
                long i__prev1 = i;

                long i = 2L;

                while (i < 16L)
                {>>MARKER:FUNCTION_p256PointAddAffineAsm_BLOCK_PREFIX<<
                    p256OrdSqr(table[i - 1L][..], table[(i / 2L) - 1L][..], 1L);
                    p256OrdMul(table[i][..], table[i - 1L][..], table[0L][..]);
                    i += 2L;
                }


                i = i__prev1;
            }

            copy(x, table[14L][..]); // f

            p256OrdSqr(x[0L..32L], x[0L..32L], 4L);
            p256OrdMul(x[0L..32L], x[0L..32L], table[14L][..]); // ff
            var t = make_slice<byte>(32L);
            copy(t, x);

            p256OrdSqr(x, x, 8L);
            p256OrdMul(x, x, t); // ffff
            copy(t, x);

            p256OrdSqr(x, x, 16L);
            p256OrdMul(x, x, t); // ffffffff
            copy(t, x);

            p256OrdSqr(x, x, 64L); // ffffffff0000000000000000
            p256OrdMul(x, x, t); // ffffffff00000000ffffffff
            p256OrdSqr(x, x, 32L); // ffffffff00000000ffffffff00000000
            p256OrdMul(x, x, t); // ffffffff00000000ffffffffffffffff

            // Remaining 32 windows
            array<byte> expLo = new array<byte>(new byte[] { 0xb, 0xc, 0xe, 0x6, 0xf, 0xa, 0xa, 0xd, 0xa, 0x7, 0x1, 0x7, 0x9, 0xe, 0x8, 0x4, 0xf, 0x3, 0xb, 0x9, 0xc, 0xa, 0xc, 0x2, 0xf, 0xc, 0x6, 0x3, 0x2, 0x5, 0x4, 0xf });
            {
                long i__prev1 = i;

                for (i = 0L; i < 32L; i++)
                {>>MARKER:FUNCTION_p256SelectBase_BLOCK_PREFIX<<
                    p256OrdSqr(x, x, 4L);
                    p256OrdMul(x, x, table[expLo[i] - 1L][..]);
                } 

                // Multiplying by one in the Montgomery domain converts a Montgomery
                // value out of the domain.


                i = i__prev1;
            } 

            // Multiplying by one in the Montgomery domain converts a Montgomery
            // value out of the domain.
            byte one = new slice<byte>(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 });
            p256OrdMul(x, x, one);

            return @new<big.Int>().SetBytes(x);
        }

        // fromBig converts a *big.Int into a format used by this code.
        private static slice<byte> fromBig(ref big.Int big)
        { 
            // This could be done a lot more efficiently...
            var res = big.Bytes();
            if (32L == len(res))
            {>>MARKER:FUNCTION_p256Select_BLOCK_PREFIX<<
                return res;
            }
            var t = make_slice<byte>(32L);
            long offset = 32L - len(res);
            for (var i = len(res) - 1L; i >= 0L; i--)
            {>>MARKER:FUNCTION_p256MovCond_BLOCK_PREFIX<<
                t[i + offset] = res[i];
            }

            return t;
        }

        // p256GetMultiplier makes sure byte array will have 32 byte elements, If the scalar
        // is equal or greater than the order of the group, it's reduced modulo that order.
        private static slice<byte> p256GetMultiplier(slice<byte> @in)
        {
            ptr<big.Int> n = @new<big.Int>().SetBytes(in);

            if (n.Cmp(p256Params.N) >= 0L)
            {>>MARKER:FUNCTION_p256NegCond_BLOCK_PREFIX<<
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

        private static ref big.Int maybeReduceModP(ref big.Int @in)
        {
            if (@in.Cmp(p256Params.P) < 0L)
            {>>MARKER:FUNCTION_p256FromMont_BLOCK_PREFIX<<
                return in;
            }
            return @new<big.Int>().Mod(in, p256Params.P);
        }

        private static (ref big.Int, ref big.Int) CombinedMult(this p256CurveFast curve, ref big.Int bigX, ref big.Int bigY, slice<byte> baseScalar, slice<byte> scalar)
        {>>MARKER:FUNCTION_p256MulAsm_BLOCK_PREFIX<<
            p256Point r1 = default;            p256Point r2 = default;

            var scalarReduced = p256GetMultiplier(baseScalar);
            var r1IsInfinity = scalarIsZero(scalarReduced);
            r1.p256BaseMult(scalarReduced);

            copy(r2.x[..], fromBig(maybeReduceModP(bigX)));
            copy(r2.y[..], fromBig(maybeReduceModP(bigY)));
            copy(r2.z[..], one);
            p256MulAsm(r2.x[..], r2.x[..], rr[..]);
            p256MulAsm(r2.y[..], r2.y[..], rr[..]);

            scalarReduced = p256GetMultiplier(scalar);
            var r2IsInfinity = scalarIsZero(scalarReduced);
            r2.p256ScalarMult(p256GetMultiplier(scalar));

            p256Point sum = default;            p256Point @double = default;

            var pointsEqual = p256PointAddAsm(ref sum, ref r1, ref r2);
            p256PointDoubleAsm(ref double, ref r1);
            p256MovCond(ref sum, ref double, ref sum, pointsEqual);
            p256MovCond(ref sum, ref r1, ref sum, r2IsInfinity);
            p256MovCond(ref sum, ref r2, ref sum, r1IsInfinity);
            return sum.p256PointToAffine();
        }

        private static (ref big.Int, ref big.Int) ScalarBaseMult(this p256CurveFast curve, slice<byte> scalar)
        {
            p256Point r = default;
            r.p256BaseMult(p256GetMultiplier(scalar));
            return r.p256PointToAffine();
        }

        private static (ref big.Int, ref big.Int) ScalarMult(this p256CurveFast curve, ref big.Int bigX, ref big.Int bigY, slice<byte> scalar)
        {
            p256Point r = default;
            copy(r.x[..], fromBig(maybeReduceModP(bigX)));
            copy(r.y[..], fromBig(maybeReduceModP(bigY)));
            copy(r.z[..], one);
            p256MulAsm(r.x[..], r.x[..], rr[..]);
            p256MulAsm(r.y[..], r.y[..], rr[..]);
            r.p256ScalarMult(p256GetMultiplier(scalar));
            return r.p256PointToAffine();
        }

        // scalarIsZero returns 1 if scalar represents the zero value, and zero
        // otherwise.
        private static long scalarIsZero(slice<byte> scalar)
        {
            var b = byte(0L);
            foreach (var (_, s) in scalar)
            {
                b |= s;
            }
            return subtle.ConstantTimeByteEq(b, 0L);
        }

        private static (ref big.Int, ref big.Int) p256PointToAffine(this ref p256Point p)
        {
            var zInv = make_slice<byte>(32L);
            var zInvSq = make_slice<byte>(32L);

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
        private static void p256Inverse(slice<byte> @out, slice<byte> @in)
        {
            array<byte> stack = new array<byte>(6L * 32L);
            var p2 = stack[32L * 0L..32L * 0L + 32L];
            var p4 = stack[32L * 1L..32L * 1L + 32L];
            var p8 = stack[32L * 2L..32L * 2L + 32L];
            var p16 = stack[32L * 3L..32L * 3L + 32L];
            var p32 = stack[32L * 4L..32L * 4L + 32L];

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
                long i__prev1 = i;

                for (long i = 0L; i < 7L; i++)
                {
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(p16, out, p8); // ffff*p

            p256Sqr(out, p16);
            {
                long i__prev1 = i;

                for (i = 0L; i < 15L; i++)
                {
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(p32, out, p16); // ffffffff*p

            p256Sqr(out, p32);

            {
                long i__prev1 = i;

                for (i = 0L; i < 31L; i++)
                {
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(out, out, in);

            {
                long i__prev1 = i;

                for (i = 0L; i < 32L * 4L; i++)
                {
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(out, out, p32);

            {
                long i__prev1 = i;

                for (i = 0L; i < 32L; i++)
                {
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(out, out, p32);

            {
                long i__prev1 = i;

                for (i = 0L; i < 16L; i++)
                {
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(out, out, p16);

            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
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

        private static (long, long) boothW5(ulong @in)
        {
            ulong s = ~((in >> (int)(5L)) - 1L);
            ulong d = (1L << (int)(6L)) - in - 1L;
            d = (d & s) | (in & (~s));
            d = (d >> (int)(1L)) + (d & 1L);
            return (int(d), int(s & 1L));
        }

        private static (long, long) boothW7(ulong @in)
        {
            ulong s = ~((in >> (int)(7L)) - 1L);
            ulong d = (1L << (int)(8L)) - in - 1L;
            d = (d & s) | (in & (~s));
            d = (d >> (int)(1L)) + (d & 1L);
            return (int(d), int(s & 1L));
        }

        private static void initTable()
        {
            p256PreFast = @new<array<array<p256Point>>>(); //z coordinate not used
            p256Point basePoint = new p256Point(x:[32]byte{0x18,0x90,0x5f,0x76,0xa5,0x37,0x55,0xc6,0x79,0xfb,0x73,0x2b,0x77,0x62,0x25,0x10,0x75,0xba,0x95,0xfc,0x5f,0xed,0xb6,0x01,0x79,0xe7,0x30,0xd4,0x18,0xa9,0x14,0x3c},y:[32]byte{0x85,0x71,0xff,0x18,0x25,0x88,0x5d,0x85,0xd2,0xe8,0x86,0x88,0xdd,0x21,0xf3,0x25,0x8b,0x4a,0xb8,0xe4,0xba,0x19,0xe4,0x5c,0xdd,0xf2,0x53,0x57,0xce,0x95,0x56,0x0a},z:[32]byte{0x00,0x00,0x00,0x00,0xff,0xff,0xff,0xfe,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01},);

            ptr<p256Point> t1 = @new<p256Point>();
            ptr<p256Point> t2 = @new<p256Point>();
            t2.Value = basePoint;

            var zInv = make_slice<byte>(32L);
            var zInvSq = make_slice<byte>(32L);
            for (long j = 0L; j < 64L; j++)
            {
                t1.Value = t2.Value;
                for (long i = 0L; i < 37L; i++)
                { 
                    // The window size is 7 so we need to double 7 times.
                    if (i != 0L)
                    {
                        for (long k = 0L; k < 7L; k++)
                        {
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

                if (j == 0L)
                {
                    p256PointDoubleAsm(t2, ref basePoint);
                }
                else
                {
                    p256PointAddAsm(t2, t2, ref basePoint);
                }
            }

        }

        private static void p256BaseMult(this ref p256Point p, slice<byte> scalar)
        {
            var wvalue = (uint(scalar[31L]) << (int)(1L)) & 0xffUL;
            var (sel, sign) = boothW7(uint(wvalue));
            p256SelectBase(p, p256PreFast[0L][..], sel);
            p256NegCond(p, sign);

            copy(p.z[..], one[..]);
            p256Point t0 = default;

            copy(t0.z[..], one[..]);

            var index = uint(6L);
            var zero = sel;

            for (long i = 1L; i < 37L; i++)
            {
                if (index < 247L)
                {
                    wvalue = ((uint(scalar[31L - index / 8L]) >> (int)((index % 8L))) + (uint(scalar[31L - index / 8L - 1L]) << (int)((8L - (index % 8L))))) & 0xffUL;
                }
                else
                {
                    wvalue = (uint(scalar[31L - index / 8L]) >> (int)((index % 8L))) & 0xffUL;
                }
                index += 7L;
                sel, sign = boothW7(uint(wvalue));
                p256SelectBase(ref t0, p256PreFast[i][..], sel);
                p256PointAddAffineAsm(p, p, ref t0, sign, sel, zero);
                zero |= sel;
            }

        }

        private static void p256ScalarMult(this ref p256Point p, slice<byte> scalar)
        { 
            // precomp is a table of precomputed points that stores powers of p
            // from p^1 to p^16.
            array<p256Point> precomp = new array<p256Point>(16L);
            p256Point t0 = default;            p256Point t1 = default;            p256Point t2 = default;            p256Point t3 = default; 

            // Prepare the table
 

            // Prepare the table
            ref precomp[0L].Value = p.Value;

            p256PointDoubleAsm(ref t0, p);
            p256PointDoubleAsm(ref t1, ref t0);
            p256PointDoubleAsm(ref t2, ref t1);
            p256PointDoubleAsm(ref t3, ref t2);
            ref precomp[1L].Value = t0; // 2
            ref precomp[3L].Value = t1; // 4
            ref precomp[7L].Value = t2; // 8
            ref precomp[15L].Value = t3; // 16

            p256PointAddAsm(ref t0, ref t0, p);
            p256PointAddAsm(ref t1, ref t1, p);
            p256PointAddAsm(ref t2, ref t2, p);
            ref precomp[2L].Value = t0; // 3
            ref precomp[4L].Value = t1; // 5
            ref precomp[8L].Value = t2; // 9

            p256PointDoubleAsm(ref t0, ref t0);
            p256PointDoubleAsm(ref t1, ref t1);
            ref precomp[5L].Value = t0; // 6
            ref precomp[9L].Value = t1; // 10

            p256PointAddAsm(ref t2, ref t0, p);
            p256PointAddAsm(ref t1, ref t1, p);
            ref precomp[6L].Value = t2; // 7
            ref precomp[10L].Value = t1; // 11

            p256PointDoubleAsm(ref t0, ref t0);
            p256PointDoubleAsm(ref t2, ref t2);
            ref precomp[11L].Value = t0; // 12
            ref precomp[13L].Value = t2; // 14

            p256PointAddAsm(ref t0, ref t0, p);
            p256PointAddAsm(ref t2, ref t2, p);
            ref precomp[12L].Value = t0; // 13
            ref precomp[14L].Value = t2; // 15

            // Start scanning the window from top bit
            var index = uint(254L);
            long sel = default;            long sign = default;



            var wvalue = (uint(scalar[31L - index / 8L]) >> (int)((index % 8L))) & 0x3fUL;
            sel, _ = boothW5(uint(wvalue));
            p256Select(p, precomp[..], sel);
            var zero = sel;

            while (index > 4L)
            {
                index -= 5L;
                p256PointDoubleAsm(p, p);
                p256PointDoubleAsm(p, p);
                p256PointDoubleAsm(p, p);
                p256PointDoubleAsm(p, p);
                p256PointDoubleAsm(p, p);

                if (index < 247L)
                {
                    wvalue = ((uint(scalar[31L - index / 8L]) >> (int)((index % 8L))) + (uint(scalar[31L - index / 8L - 1L]) << (int)((8L - (index % 8L))))) & 0x3fUL;
                }
                else
                {
                    wvalue = (uint(scalar[31L - index / 8L]) >> (int)((index % 8L))) & 0x3fUL;
                }
                sel, sign = boothW5(uint(wvalue));

                p256Select(ref t0, precomp[..], sel);
                p256NegCond(ref t0, sign);
                p256PointAddAsm(ref t1, p, ref t0);
                p256MovCond(ref t1, ref t1, p, sel);
                p256MovCond(p, ref t1, ref t0, zero);
                zero |= sel;
            }


            p256PointDoubleAsm(p, p);
            p256PointDoubleAsm(p, p);
            p256PointDoubleAsm(p, p);
            p256PointDoubleAsm(p, p);
            p256PointDoubleAsm(p, p);

            wvalue = (uint(scalar[31L]) << (int)(1L)) & 0x3fUL;
            sel, sign = boothW5(uint(wvalue));

            p256Select(ref t0, precomp[..], sel);
            p256NegCond(ref t0, sign);
            p256PointAddAsm(ref t1, p, ref t0);
            p256MovCond(ref t1, ref t1, p, sel);
            p256MovCond(p, ref t1, ref t0, zero);
        }
    }
}}
