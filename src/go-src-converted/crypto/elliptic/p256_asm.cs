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

// +build amd64 arm64

// package elliptic -- go2cs converted at 2020 October 09 04:54:26 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Go\src\crypto\elliptic\p256_asm.go
using big = go.math.big_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class elliptic_package
    {
        private partial struct p256Curve
        {
            public ref ptr<CurveParams> ptr<CurveParams> => ref ptr<CurveParams>_ptr;
        }

        private partial struct p256Point
        {
            public array<ulong> xyz;
        }
        private static p256Curve p256 = default;        private static ptr<array<array<ulong>>> p256Precomputed;        private static sync.Once precomputeOnce = default;

        private static void initP256()
        { 
            // See FIPS 186-3, section D.2.3
            p256.CurveParams = addr(new CurveParams(Name:"P-256"));
            p256.P, _ = @new<big.Int>().SetString("115792089210356248762697446949407573530086143415290314195533631308867097853951", 10L);
            p256.N, _ = @new<big.Int>().SetString("115792089210356248762697446949407573529996955224135760342422259061068512044369", 10L);
            p256.B, _ = @new<big.Int>().SetString("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b", 16L);
            p256.Gx, _ = @new<big.Int>().SetString("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296", 16L);
            p256.Gy, _ = @new<big.Int>().SetString("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5", 16L);
            p256.BitSize = 256L;

        }

        private static ptr<CurveParams> Params(this p256Curve curve)
        {
            return _addr_curve.CurveParams!;
        }

        // Functions implemented in p256_asm_*64.s
        // Montgomery multiplication modulo P256
        //go:noescape
        private static void p256Mul(slice<ulong> res, slice<ulong> in1, slice<ulong> in2)
;

        // Montgomery square modulo P256, repeated n times (n >= 1)
        //go:noescape
        private static void p256Sqr(slice<ulong> res, slice<ulong> @in, long n)
;

        // Montgomery multiplication by 1
        //go:noescape
        private static void p256FromMont(slice<ulong> res, slice<ulong> @in)
;

        // iff cond == 1  val <- -val
        //go:noescape
        private static void p256NegCond(slice<ulong> val, long cond)
;

        // if cond == 0 res <- b; else res <- a
        //go:noescape
        private static void p256MovCond(slice<ulong> res, slice<ulong> a, slice<ulong> b, long cond)
;

        // Endianness swap
        //go:noescape
        private static void p256BigToLittle(slice<ulong> res, slice<byte> @in)
;

        //go:noescape
        private static void p256LittleToBig(slice<byte> res, slice<ulong> @in)
;

        // Constant time table access
        //go:noescape
        private static void p256Select(slice<ulong> point, slice<ulong> table, long idx)
;

        //go:noescape
        private static void p256SelectBase(slice<ulong> point, slice<ulong> table, long idx)
;

        // Montgomery multiplication modulo Ord(G)
        //go:noescape
        private static void p256OrdMul(slice<ulong> res, slice<ulong> in1, slice<ulong> in2)
;

        // Montgomery square modulo Ord(G), repeated n times
        //go:noescape
        private static void p256OrdSqr(slice<ulong> res, slice<ulong> @in, long n)
;

        // Point add with in2 being affine point
        // If sign == 1 -> in2 = -in2
        // If sel == 0 -> res = in1
        // if zero == 0 -> res = in2
        //go:noescape
        private static void p256PointAddAffineAsm(slice<ulong> res, slice<ulong> in1, slice<ulong> in2, long sign, long sel, long zero)
;

        // Point add. Returns one if the two input points were equal and zero
        // otherwise. (Note that, due to the way that the equations work out, some
        // representations of ∞ are considered equal to everything by this function.)
        //go:noescape
        private static long p256PointAddAsm(slice<ulong> res, slice<ulong> in1, slice<ulong> in2)
;

        // Point double
        //go:noescape
        private static void p256PointDoubleAsm(slice<ulong> res, slice<ulong> @in)
;

        private static ptr<big.Int> Inverse(this p256Curve curve, ptr<big.Int> _addr_k)
        {
            ref big.Int k = ref _addr_k.val;

            if (k.Sign() < 0L)
            {>>MARKER:FUNCTION_p256PointDoubleAsm_BLOCK_PREFIX<< 
                // This should never happen.
                k = @new<big.Int>().Neg(k);

            }

            if (k.Cmp(p256.N) >= 0L)
            {>>MARKER:FUNCTION_p256PointAddAsm_BLOCK_PREFIX<< 
                // This should never happen.
                k = @new<big.Int>().Mod(k, p256.N);

            } 

            // table will store precomputed powers of x.
            array<ulong> table = new array<ulong>(4L * 9L);
            var _1 = table[4L * 0L..4L * 1L];            var _11 = table[4L * 1L..4L * 2L];            var _101 = table[4L * 2L..4L * 3L];            var _111 = table[4L * 3L..4L * 4L];            var _1111 = table[4L * 4L..4L * 5L];            var _10101 = table[4L * 5L..4L * 6L];            var _101111 = table[4L * 6L..4L * 7L];            var x = table[4L * 7L..4L * 8L];            var t = table[4L * 8L..4L * 9L];

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
            p256OrdSqr(x, _1, 1L); // _10
            p256OrdMul(_11, x, _1); // _11
            p256OrdMul(_101, x, _11); // _101
            p256OrdMul(_111, x, _101); // _111
            p256OrdSqr(x, _101, 1L); // _1010
            p256OrdMul(_1111, _101, x); // _1111

            p256OrdSqr(t, x, 1L); // _10100
            p256OrdMul(_10101, t, _1); // _10101
            p256OrdSqr(x, _10101, 1L); // _101010
            p256OrdMul(_101111, _101, x); // _101111
            p256OrdMul(x, _10101, x); // _111111 = x6
            p256OrdSqr(t, x, 2L); // _11111100
            p256OrdMul(t, t, _11); // _11111111 = x8
            p256OrdSqr(x, t, 8L); // _ff00
            p256OrdMul(x, x, t); // _ffff = x16
            p256OrdSqr(t, x, 16L); // _ffff0000
            p256OrdMul(t, t, x); // _ffffffff = x32

            p256OrdSqr(x, t, 64L);
            p256OrdMul(x, x, t);
            p256OrdSqr(x, x, 32L);
            p256OrdMul(x, x, t);

            byte sqrs = new slice<byte>(new byte[] { 6, 5, 4, 5, 5, 4, 3, 3, 5, 9, 6, 2, 5, 6, 5, 4, 5, 5, 3, 10, 2, 5, 5, 3, 7, 6 });
            slice<ulong> muls = new slice<slice<ulong>>(new slice<ulong>[] { _101111, _111, _11, _1111, _10101, _101, _101, _101, _111, _101111, _1111, _1, _1, _1111, _111, _111, _111, _101, _11, _101111, _11, _11, _11, _1, _10101, _1111 });

            foreach (var (i, s) in sqrs)
            {
                p256OrdSqr(x, x, int(s));
                p256OrdMul(x, x, muls[i]);
            } 

            // Multiplying by one in the Montgomery domain converts a Montgomery
            // value out of the domain.
            ulong one = new slice<ulong>(new ulong[] { 1, 0, 0, 0 });
            p256OrdMul(x, x, one);

            var xOut = make_slice<byte>(32L);
            p256LittleToBig(xOut, x);
            return @new<big.Int>().SetBytes(xOut);

        }

        // fromBig converts a *big.Int into a format used by this code.
        private static void fromBig(slice<ulong> @out, ptr<big.Int> _addr_big)
        {
            ref big.Int big = ref _addr_big.val;

            {
                var i__prev1 = i;

                foreach (var (__i) in out)
                {
                    i = __i;
                    out[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i, __v) in big.Bits())
                {
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
        private static void p256GetScalar(slice<ulong> @out, slice<byte> @in)
        {
            ptr<big.Int> n = @new<big.Int>().SetBytes(in);

            if (n.Cmp(p256.N) >= 0L)
            {>>MARKER:FUNCTION_p256PointAddAffineAsm_BLOCK_PREFIX<<
                n.Mod(n, p256.N);
            }

            fromBig(out, n);

        }

        // p256Mul operates in a Montgomery domain with R = 2^256 mod p, where p is the
        // underlying field of the curve. (See initP256 for the value.) Thus rr here is
        // R×R mod p. See comment in Inverse about how this is used.
        private static ulong rr = new slice<ulong>(new ulong[] { 0x0000000000000003, 0xfffffffbffffffff, 0xfffffffffffffffe, 0x00000004fffffffd });

        private static ptr<big.Int> maybeReduceModP(ptr<big.Int> _addr_@in)
        {
            ref big.Int @in = ref _addr_@in.val;

            if (@in.Cmp(p256.P) < 0L)
            {>>MARKER:FUNCTION_p256OrdSqr_BLOCK_PREFIX<<
                return _addr_in!;
            }

            return @new<big.Int>().Mod(in, p256.P);

        }

        private static (ptr<big.Int>, ptr<big.Int>) CombinedMult(this p256Curve curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> baseScalar, slice<byte> scalar)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;
            ref big.Int bigX = ref _addr_bigX.val;
            ref big.Int bigY = ref _addr_bigY.val;

            var scalarReversed = make_slice<ulong>(4L);
            ref p256Point r1 = ref heap(out ptr<p256Point> _addr_r1);            ref p256Point r2 = ref heap(out ptr<p256Point> _addr_r2);

            p256GetScalar(scalarReversed, baseScalar);
            var r1IsInfinity = scalarIsZero(scalarReversed);
            r1.p256BaseMult(scalarReversed);

            p256GetScalar(scalarReversed, scalar);
            var r2IsInfinity = scalarIsZero(scalarReversed);
            fromBig(r2.xyz[0L..4L], _addr_maybeReduceModP(_addr_bigX));
            fromBig(r2.xyz[4L..8L], _addr_maybeReduceModP(_addr_bigY));
            p256Mul(r2.xyz[0L..4L], r2.xyz[0L..4L], rr[..]);
            p256Mul(r2.xyz[4L..8L], r2.xyz[4L..8L], rr[..]); 

            // This sets r2's Z value to 1, in the Montgomery domain.
            r2.xyz[8L] = 0x0000000000000001UL;
            r2.xyz[9L] = 0xffffffff00000000UL;
            r2.xyz[10L] = 0xffffffffffffffffUL;
            r2.xyz[11L] = 0x00000000fffffffeUL;

            r2.p256ScalarMult(scalarReversed);

            p256Point sum = default;            ref p256Point @double = ref heap(out ptr<p256Point> _addr_@double);

            var pointsEqual = p256PointAddAsm(sum.xyz[..], r1.xyz[..], r2.xyz[..]);
            p256PointDoubleAsm(@double.xyz[..], r1.xyz[..]);
            sum.CopyConditional(_addr_double, pointsEqual);
            sum.CopyConditional(_addr_r1, r2IsInfinity);
            sum.CopyConditional(_addr_r2, r1IsInfinity);

            return _addr_sum.p256PointToAffine()!;

        }

        private static (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(this p256Curve curve, slice<byte> scalar)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;

            var scalarReversed = make_slice<ulong>(4L);
            p256GetScalar(scalarReversed, scalar);

            p256Point r = default;
            r.p256BaseMult(scalarReversed);
            return _addr_r.p256PointToAffine()!;
        }

        private static (ptr<big.Int>, ptr<big.Int>) ScalarMult(this p256Curve curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> scalar)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;
            ref big.Int bigX = ref _addr_bigX.val;
            ref big.Int bigY = ref _addr_bigY.val;

            var scalarReversed = make_slice<ulong>(4L);
            p256GetScalar(scalarReversed, scalar);

            p256Point r = default;
            fromBig(r.xyz[0L..4L], _addr_maybeReduceModP(_addr_bigX));
            fromBig(r.xyz[4L..8L], _addr_maybeReduceModP(_addr_bigY));
            p256Mul(r.xyz[0L..4L], r.xyz[0L..4L], rr[..]);
            p256Mul(r.xyz[4L..8L], r.xyz[4L..8L], rr[..]); 
            // This sets r2's Z value to 1, in the Montgomery domain.
            r.xyz[8L] = 0x0000000000000001UL;
            r.xyz[9L] = 0xffffffff00000000UL;
            r.xyz[10L] = 0xffffffffffffffffUL;
            r.xyz[11L] = 0x00000000fffffffeUL;

            r.p256ScalarMult(scalarReversed);
            return _addr_r.p256PointToAffine()!;

        }

        // uint64IsZero returns 1 if x is zero and zero otherwise.
        private static long uint64IsZero(ulong x)
        {
            x = ~x;
            x &= x >> (int)(32L);
            x &= x >> (int)(16L);
            x &= x >> (int)(8L);
            x &= x >> (int)(4L);
            x &= x >> (int)(2L);
            x &= x >> (int)(1L);
            return int(x & 1L);
        }

        // scalarIsZero returns 1 if scalar represents the zero value, and zero
        // otherwise.
        private static long scalarIsZero(slice<ulong> scalar)
        {
            return uint64IsZero(scalar[0L] | scalar[1L] | scalar[2L] | scalar[3L]);
        }

        private static (ptr<big.Int>, ptr<big.Int>) p256PointToAffine(this ptr<p256Point> _addr_p)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;
            ref p256Point p = ref _addr_p.val;

            var zInv = make_slice<ulong>(4L);
            var zInvSq = make_slice<ulong>(4L);
            p256Inverse(zInv, p.xyz[8L..12L]);
            p256Sqr(zInvSq, zInv, 1L);
            p256Mul(zInv, zInv, zInvSq);

            p256Mul(zInvSq, p.xyz[0L..4L], zInvSq);
            p256Mul(zInv, p.xyz[4L..8L], zInv);

            p256FromMont(zInvSq, zInvSq);
            p256FromMont(zInv, zInv);

            var xOut = make_slice<byte>(32L);
            var yOut = make_slice<byte>(32L);
            p256LittleToBig(xOut, zInvSq);
            p256LittleToBig(yOut, zInv);

            return (@new<big.Int>().SetBytes(xOut), @new<big.Int>().SetBytes(yOut));
        }

        // CopyConditional copies overwrites p with src if v == 1, and leaves p
        // unchanged if v == 0.
        private static void CopyConditional(this ptr<p256Point> _addr_p, ptr<p256Point> _addr_src, long v)
        {
            ref p256Point p = ref _addr_p.val;
            ref p256Point src = ref _addr_src.val;

            var pMask = uint64(v) - 1L;
            var srcMask = ~pMask;

            foreach (var (i, n) in p.xyz)
            {
                p.xyz[i] = (n & pMask) | (src.xyz[i] & srcMask);
            }

        }

        // p256Inverse sets out to in^-1 mod p.
        private static void p256Inverse(slice<ulong> @out, slice<ulong> @in)
        {
            array<ulong> stack = new array<ulong>(6L * 4L);
            var p2 = stack[4L * 0L..4L * 0L + 4L];
            var p4 = stack[4L * 1L..4L * 1L + 4L];
            var p8 = stack[4L * 2L..4L * 2L + 4L];
            var p16 = stack[4L * 3L..4L * 3L + 4L];
            var p32 = stack[4L * 4L..4L * 4L + 4L];

            p256Sqr(out, in, 1L);
            p256Mul(p2, out, in); // 3*p

            p256Sqr(out, p2, 2L);
            p256Mul(p4, out, p2); // f*p

            p256Sqr(out, p4, 4L);
            p256Mul(p8, out, p4); // ff*p

            p256Sqr(out, p8, 8L);
            p256Mul(p16, out, p8); // ffff*p

            p256Sqr(out, p16, 16L);
            p256Mul(p32, out, p16); // ffffffff*p

            p256Sqr(out, p32, 32L);
            p256Mul(out, out, in);

            p256Sqr(out, out, 128L);
            p256Mul(out, out, p32);

            p256Sqr(out, out, 32L);
            p256Mul(out, out, p32);

            p256Sqr(out, out, 16L);
            p256Mul(out, out, p16);

            p256Sqr(out, out, 8L);
            p256Mul(out, out, p8);

            p256Sqr(out, out, 4L);
            p256Mul(out, out, p4);

            p256Sqr(out, out, 2L);
            p256Mul(out, out, p2);

            p256Sqr(out, out, 2L);
            p256Mul(out, out, in);

        }

        private static void p256StorePoint(this ptr<p256Point> _addr_p, ptr<array<ulong>> _addr_r, long index)
        {
            ref p256Point p = ref _addr_p.val;
            ref array<ulong> r = ref _addr_r.val;

            copy(r[index * 12L..], p.xyz[..]);
        }

        private static (long, long) boothW5(ulong @in)
        {
            long _p0 = default;
            long _p0 = default;

            ulong s = ~((in >> (int)(5L)) - 1L);
            ulong d = (1L << (int)(6L)) - in - 1L;
            d = (d & s) | (in & (~s));
            d = (d >> (int)(1L)) + (d & 1L);
            return (int(d), int(s & 1L));
        }

        private static (long, long) boothW6(ulong @in)
        {
            long _p0 = default;
            long _p0 = default;

            ulong s = ~((in >> (int)(6L)) - 1L);
            ulong d = (1L << (int)(7L)) - in - 1L;
            d = (d & s) | (in & (~s));
            d = (d >> (int)(1L)) + (d & 1L);
            return (int(d), int(s & 1L));
        }

        private static void initTable()
        {
            p256Precomputed = @new<[43][32*8]uint64>();

            ulong basePoint = new slice<ulong>(new ulong[] { 0x79e730d418a9143c, 0x75ba95fc5fedb601, 0x79fb732b77622510, 0x18905f76a53755c6, 0xddf25357ce95560a, 0x8b4ab8e4ba19e45c, 0xd2e88688dd21f325, 0x8571ff1825885d85, 0x0000000000000001, 0xffffffff00000000, 0xffffffffffffffff, 0x00000000fffffffe });
            var t1 = make_slice<ulong>(12L);
            var t2 = make_slice<ulong>(12L);
            copy(t2, basePoint);

            var zInv = make_slice<ulong>(4L);
            var zInvSq = make_slice<ulong>(4L);
            for (long j = 0L; j < 32L; j++)
            {>>MARKER:FUNCTION_p256OrdMul_BLOCK_PREFIX<<
                copy(t1, t2);
                for (long i = 0L; i < 43L; i++)
                {>>MARKER:FUNCTION_p256SelectBase_BLOCK_PREFIX<< 
                    // The window size is 6 so we need to double 6 times.
                    if (i != 0L)
                    {>>MARKER:FUNCTION_p256Select_BLOCK_PREFIX<<
                        for (long k = 0L; k < 6L; k++)
                        {>>MARKER:FUNCTION_p256LittleToBig_BLOCK_PREFIX<<
                            p256PointDoubleAsm(t1, t1);
                        }


                    } 
                    // Convert the point to affine form. (Its values are
                    // still in Montgomery form however.)
                    p256Inverse(zInv, t1[8L..12L]);
                    p256Sqr(zInvSq, zInv, 1L);
                    p256Mul(zInv, zInv, zInvSq);

                    p256Mul(t1[..4L], t1[..4L], zInvSq);
                    p256Mul(t1[4L..8L], t1[4L..8L], zInv);

                    copy(t1[8L..12L], basePoint[8L..12L]); 
                    // Update the table entry
                    copy(p256Precomputed[i][j * 8L..], t1[..8L]);

                }

                if (j == 0L)
                {>>MARKER:FUNCTION_p256BigToLittle_BLOCK_PREFIX<<
                    p256PointDoubleAsm(t2, basePoint);
                }
                else
                {>>MARKER:FUNCTION_p256MovCond_BLOCK_PREFIX<<
                    p256PointAddAsm(t2, t2, basePoint);
                }

            }


        }

        private static void p256BaseMult(this ptr<p256Point> _addr_p, slice<ulong> scalar)
        {
            ref p256Point p = ref _addr_p.val;

            precomputeOnce.Do(initTable);

            var wvalue = (scalar[0L] << (int)(1L)) & 0x7fUL;
            var (sel, sign) = boothW6(uint(wvalue));
            p256SelectBase(p.xyz[0L..8L], p256Precomputed[0L][0L..], sel);
            p256NegCond(p.xyz[4L..8L], sign); 

            // (This is one, in the Montgomery domain.)
            p.xyz[8L] = 0x0000000000000001UL;
            p.xyz[9L] = 0xffffffff00000000UL;
            p.xyz[10L] = 0xffffffffffffffffUL;
            p.xyz[11L] = 0x00000000fffffffeUL;

            p256Point t0 = default; 
            // (This is one, in the Montgomery domain.)
            t0.xyz[8L] = 0x0000000000000001UL;
            t0.xyz[9L] = 0xffffffff00000000UL;
            t0.xyz[10L] = 0xffffffffffffffffUL;
            t0.xyz[11L] = 0x00000000fffffffeUL;

            var index = uint(5L);
            var zero = sel;

            for (long i = 1L; i < 43L; i++)
            {>>MARKER:FUNCTION_p256NegCond_BLOCK_PREFIX<<
                if (index < 192L)
                {>>MARKER:FUNCTION_p256FromMont_BLOCK_PREFIX<<
                    wvalue = ((scalar[index / 64L] >> (int)((index % 64L))) + (scalar[index / 64L + 1L] << (int)((64L - (index % 64L))))) & 0x7fUL;
                }
                else
                {>>MARKER:FUNCTION_p256Sqr_BLOCK_PREFIX<<
                    wvalue = (scalar[index / 64L] >> (int)((index % 64L))) & 0x7fUL;
                }

                index += 6L;
                sel, sign = boothW6(uint(wvalue));
                p256SelectBase(t0.xyz[0L..8L], p256Precomputed[i][0L..], sel);
                p256PointAddAffineAsm(p.xyz[0L..12L], p.xyz[0L..12L], t0.xyz[0L..8L], sign, sel, zero);
                zero |= sel;

            }


        }

        private static void p256ScalarMult(this ptr<p256Point> _addr_p, slice<ulong> scalar)
        {
            ref p256Point p = ref _addr_p.val;
 
            // precomp is a table of precomputed points that stores powers of p
            // from p^1 to p^16.
            ref array<ulong> precomp = ref heap(new array<ulong>(16L * 4L * 3L), out ptr<array<ulong>> _addr_precomp);
            p256Point t0 = default;            p256Point t1 = default;            p256Point t2 = default;            p256Point t3 = default; 

            // Prepare the table
 

            // Prepare the table
            p.p256StorePoint(_addr_precomp, 0L); // 1

            p256PointDoubleAsm(t0.xyz[..], p.xyz[..]);
            p256PointDoubleAsm(t1.xyz[..], t0.xyz[..]);
            p256PointDoubleAsm(t2.xyz[..], t1.xyz[..]);
            p256PointDoubleAsm(t3.xyz[..], t2.xyz[..]);
            t0.p256StorePoint(_addr_precomp, 1L); // 2
            t1.p256StorePoint(_addr_precomp, 3L); // 4
            t2.p256StorePoint(_addr_precomp, 7L); // 8
            t3.p256StorePoint(_addr_precomp, 15L); // 16

            p256PointAddAsm(t0.xyz[..], t0.xyz[..], p.xyz[..]);
            p256PointAddAsm(t1.xyz[..], t1.xyz[..], p.xyz[..]);
            p256PointAddAsm(t2.xyz[..], t2.xyz[..], p.xyz[..]);
            t0.p256StorePoint(_addr_precomp, 2L); // 3
            t1.p256StorePoint(_addr_precomp, 4L); // 5
            t2.p256StorePoint(_addr_precomp, 8L); // 9

            p256PointDoubleAsm(t0.xyz[..], t0.xyz[..]);
            p256PointDoubleAsm(t1.xyz[..], t1.xyz[..]);
            t0.p256StorePoint(_addr_precomp, 5L); // 6
            t1.p256StorePoint(_addr_precomp, 9L); // 10

            p256PointAddAsm(t2.xyz[..], t0.xyz[..], p.xyz[..]);
            p256PointAddAsm(t1.xyz[..], t1.xyz[..], p.xyz[..]);
            t2.p256StorePoint(_addr_precomp, 6L); // 7
            t1.p256StorePoint(_addr_precomp, 10L); // 11

            p256PointDoubleAsm(t0.xyz[..], t0.xyz[..]);
            p256PointDoubleAsm(t2.xyz[..], t2.xyz[..]);
            t0.p256StorePoint(_addr_precomp, 11L); // 12
            t2.p256StorePoint(_addr_precomp, 13L); // 14

            p256PointAddAsm(t0.xyz[..], t0.xyz[..], p.xyz[..]);
            p256PointAddAsm(t2.xyz[..], t2.xyz[..], p.xyz[..]);
            t0.p256StorePoint(_addr_precomp, 12L); // 13
            t2.p256StorePoint(_addr_precomp, 14L); // 15

            // Start scanning the window from top bit
            var index = uint(254L);
            long sel = default;            long sign = default;



            var wvalue = (scalar[index / 64L] >> (int)((index % 64L))) & 0x3fUL;
            sel, _ = boothW5(uint(wvalue));

            p256Select(p.xyz[0L..12L], precomp[0L..], sel);
            var zero = sel;

            while (index > 4L)
            {>>MARKER:FUNCTION_p256Mul_BLOCK_PREFIX<<
                index -= 5L;
                p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
                p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
                p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
                p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
                p256PointDoubleAsm(p.xyz[..], p.xyz[..]);

                if (index < 192L)
                {
                    wvalue = ((scalar[index / 64L] >> (int)((index % 64L))) + (scalar[index / 64L + 1L] << (int)((64L - (index % 64L))))) & 0x3fUL;
                }
                else
                {
                    wvalue = (scalar[index / 64L] >> (int)((index % 64L))) & 0x3fUL;
                }

                sel, sign = boothW5(uint(wvalue));

                p256Select(t0.xyz[0L..], precomp[0L..], sel);
                p256NegCond(t0.xyz[4L..8L], sign);
                p256PointAddAsm(t1.xyz[..], p.xyz[..], t0.xyz[..]);
                p256MovCond(t1.xyz[0L..12L], t1.xyz[0L..12L], p.xyz[0L..12L], sel);
                p256MovCond(p.xyz[0L..12L], t1.xyz[0L..12L], t0.xyz[0L..12L], zero);
                zero |= sel;

            }


            p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
            p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
            p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
            p256PointDoubleAsm(p.xyz[..], p.xyz[..]);
            p256PointDoubleAsm(p.xyz[..], p.xyz[..]);

            wvalue = (scalar[0L] << (int)(1L)) & 0x3fUL;
            sel, sign = boothW5(uint(wvalue));

            p256Select(t0.xyz[0L..], precomp[0L..], sel);
            p256NegCond(t0.xyz[4L..8L], sign);
            p256PointAddAsm(t1.xyz[..], p.xyz[..], t0.xyz[..]);
            p256MovCond(t1.xyz[0L..12L], t1.xyz[0L..12L], p.xyz[0L..12L], sel);
            p256MovCond(p.xyz[0L..12L], t1.xyz[0L..12L], t0.xyz[0L..12L], zero);

        }
    }
}}
