// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build ppc64le

// package elliptic -- go2cs converted at 2020 October 09 04:54:28 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Go\src\crypto\elliptic\p256_ppc64le.go
using subtle = go.crypto.subtle_package;
using binary = go.encoding.binary_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class elliptic_package
    {
        // This was ported from the s390x implementation for ppc64le.
        // Some hints are included here for changes that should be
        // in the big endian ppc64 implementation, however more
        // investigation and testing is needed for the ppc64 big
        // endian version to work.
        private partial struct p256CurveFast
        {
            public ref ptr<CurveParams> ptr<CurveParams> => ref ptr<CurveParams>_ptr;
        }

        private partial struct p256Point
        {
            public array<byte> x;
            public array<byte> y;
            public array<byte> z;
        }

        private static Curve p256 = default;        private static ptr<array<array<p256Point>>> p256PreFast;

        private static void initP256Arch()
        {
            p256 = new p256CurveFast(p256Params);
            initTable();
            return ;
        }

        private static ptr<CurveParams> Params(this p256CurveFast curve)
        {
            return _addr_curve.CurveParams!;
        }

        // Functions implemented in p256_asm_ppc64le.s
        // Montgomery multiplication modulo P256
        //
        //go:noescape
        private static void p256MulAsm(slice<byte> res, slice<byte> in1, slice<byte> in2)
;

        // Montgomery square modulo P256
        //
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
        private static void p256NegCond(ptr<p256Point> val, long cond)
;

        // if cond == 0 res <- b; else res <- a
        //
        //go:noescape
        private static void p256MovCond(ptr<p256Point> res, ptr<p256Point> a, ptr<p256Point> b, long cond)
;

        // Constant time table access
        //
        //go:noescape
        private static void p256Select(ptr<p256Point> point, slice<p256Point> table, long idx)
;

        //
        //go:noescape
        private static void p256SelectBase(ptr<p256Point> point, slice<p256Point> table, long idx)
;

        // Point add with P2 being affine point
        // If sign == 1 -> P2 = -P2
        // If sel == 0 -> P3 = P1
        // if zero == 0 -> P3 = P2
        //
        //go:noescape
        private static void p256PointAddAffineAsm(ptr<p256Point> res, ptr<p256Point> in1, ptr<p256Point> in2, long sign, long sel, long zero)
;

        // Point add
        //
        //go:noescape
        private static long p256PointAddAsm(ptr<p256Point> res, ptr<p256Point> in1, ptr<p256Point> in2)
;

        //
        //go:noescape
        private static void p256PointDoubleAsm(ptr<p256Point> res, ptr<p256Point> @in)
;

        // The result should be a slice in LE order, but the slice
        // from big.Bytes is in BE order.
        // TODO: For big endian implementation, do not reverse bytes.
        //
        private static slice<byte> fromBig(ptr<big.Int> _addr_big)
        {
            ref big.Int big = ref _addr_big.val;
 
            // This could be done a lot more efficiently...
            var res = big.Bytes();
            var t = make_slice<byte>(32L);
            if (len(res) < 32L)
            {>>MARKER:FUNCTION_p256PointDoubleAsm_BLOCK_PREFIX<<
                copy(t[32L - len(res)..], res);
            }
            else if (len(res) == 32L)
            {>>MARKER:FUNCTION_p256PointAddAsm_BLOCK_PREFIX<<
                copy(t, res);
            }
            else
            {>>MARKER:FUNCTION_p256PointAddAffineAsm_BLOCK_PREFIX<<
                copy(t, res[len(res) - 32L..]);
            }

            p256ReverseBytes(t, t);
            return t;

        }

        // p256GetMultiplier makes sure byte array will have 32 byte elements, If the scalar
        // is equal or greater than the order of the group, it's reduced modulo that order.
        private static slice<byte> p256GetMultiplier(slice<byte> @in)
        {
            ptr<big.Int> n = @new<big.Int>().SetBytes(in);

            if (n.Cmp(p256Params.N) >= 0L)
            {>>MARKER:FUNCTION_p256SelectBase_BLOCK_PREFIX<<
                n.Mod(n, p256Params.N);
            }

            return fromBig(n);

        }

        // p256MulAsm operates in a Montgomery domain with R = 2^256 mod p, where p is the
        // underlying field of the curve. (See initP256 for the value.) Thus rr here is
        // R×R mod p. See comment in Inverse about how this is used.
        // TODO: For big endian implementation, the bytes in these slices should be in reverse order,
        // as found in the s390x implementation.
        private static byte rr = new slice<byte>(new byte[] { 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0, 0xff, 0xff, 0xff, 0xff, 0xfb, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfd, 0xff, 0xff, 0xff, 0x04, 0x00, 0x00, 0x00 });

        // (This is one, in the Montgomery domain.)
        private static byte one = new slice<byte>(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00 });

        private static ptr<big.Int> maybeReduceModP(ptr<big.Int> _addr_@in)
        {
            ref big.Int @in = ref _addr_@in.val;

            if (@in.Cmp(p256Params.P) < 0L)
            {>>MARKER:FUNCTION_p256Select_BLOCK_PREFIX<<
                return _addr_in!;
            }

            return @new<big.Int>().Mod(in, p256Params.P);

        }

        // p256ReverseBytes copies the first 32 bytes from in to res in reverse order.
        private static void p256ReverseBytes(slice<byte> res, slice<byte> @in)
        { 
            // remove bounds check
            in = in[..32L];
            res = res[..32L]; 

            // Load in reverse order
            var a = binary.BigEndian.Uint64(in[0L..]);
            var b = binary.BigEndian.Uint64(in[8L..]);
            var c = binary.BigEndian.Uint64(in[16L..]);
            var d = binary.BigEndian.Uint64(in[24L..]); 

            // Store in normal order
            binary.LittleEndian.PutUint64(res[0L..], d);
            binary.LittleEndian.PutUint64(res[8L..], c);
            binary.LittleEndian.PutUint64(res[16L..], b);
            binary.LittleEndian.PutUint64(res[24L..], a);

        }

        private static (ptr<big.Int>, ptr<big.Int>) CombinedMult(this p256CurveFast curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> baseScalar, slice<byte> scalar)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;
            ref big.Int bigX = ref _addr_bigX.val;
            ref big.Int bigY = ref _addr_bigY.val;

            ref p256Point r1 = ref heap(out ptr<p256Point> _addr_r1);            ref p256Point r2 = ref heap(out ptr<p256Point> _addr_r2);



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
            r2.p256ScalarMult(scalarReduced);

            ref p256Point sum = ref heap(out ptr<p256Point> _addr_sum);            ref p256Point @double = ref heap(out ptr<p256Point> _addr_@double);

            var pointsEqual = p256PointAddAsm(_addr_sum, _addr_r1, _addr_r2);
            p256PointDoubleAsm(_addr_double, _addr_r1);
            p256MovCond(_addr_sum, _addr_double, _addr_sum, pointsEqual);
            p256MovCond(_addr_sum, _addr_r1, _addr_sum, r2IsInfinity);
            p256MovCond(_addr_sum, _addr_r2, _addr_sum, r1IsInfinity);
            return _addr_sum.p256PointToAffine()!;
        }

        private static (ptr<big.Int>, ptr<big.Int>) ScalarBaseMult(this p256CurveFast curve, slice<byte> scalar)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;

            p256Point r = default;
            var reducedScalar = p256GetMultiplier(scalar);
            r.p256BaseMult(reducedScalar);
            return _addr_r.p256PointToAffine()!;
        }

        private static (ptr<big.Int>, ptr<big.Int>) ScalarMult(this p256CurveFast curve, ptr<big.Int> _addr_bigX, ptr<big.Int> _addr_bigY, slice<byte> scalar)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;
            ref big.Int bigX = ref _addr_bigX.val;
            ref big.Int bigY = ref _addr_bigY.val;

            var scalarReduced = p256GetMultiplier(scalar);
            p256Point r = default;
            copy(r.x[..], fromBig(_addr_maybeReduceModP(_addr_bigX)));
            copy(r.y[..], fromBig(_addr_maybeReduceModP(_addr_bigY)));
            copy(r.z[..], one);
            p256MulAsm(r.x[..], r.x[..], rr[..]);
            p256MulAsm(r.y[..], r.y[..], rr[..]);
            r.p256ScalarMult(scalarReduced);
            return _addr_r.p256PointToAffine()!;
        }

        private static long scalarIsZero(slice<byte> scalar)
        { 
            // If any byte is not zero, return 0.
            // Check for -0.... since that appears to compare to 0.
            var b = byte(0L);
            foreach (var (_, s) in scalar)
            {
                b |= s;
            }
            return subtle.ConstantTimeByteEq(b, 0L);

        }

        private static (ptr<big.Int>, ptr<big.Int>) p256PointToAffine(this ptr<p256Point> _addr_p)
        {
            ptr<big.Int> x = default!;
            ptr<big.Int> y = default!;
            ref p256Point p = ref _addr_p.val;

            var zInv = make_slice<byte>(32L);
            var zInvSq = make_slice<byte>(32L);

            p256Inverse(zInv, p.z[..]);
            p256Sqr(zInvSq, zInv);
            p256MulAsm(zInv, zInv, zInvSq);

            p256MulAsm(zInvSq, p.x[..], zInvSq);
            p256MulAsm(zInv, p.y[..], zInv);

            p256FromMont(zInvSq, zInvSq);
            p256FromMont(zInv, zInv); 

            // SetBytes expects a slice in big endian order,
            // since ppc64le is little endian, reverse the bytes.
            // TODO: For big endian, bytes don't need to be reversed.
            p256ReverseBytes(zInvSq, zInvSq);
            p256ReverseBytes(zInv, zInv);
            ptr<big.Int> rx = @new<big.Int>().SetBytes(zInvSq);
            ptr<big.Int> ry = @new<big.Int>().SetBytes(zInv);
            return (_addr_rx!, _addr_ry!);

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
                {>>MARKER:FUNCTION_p256MovCond_BLOCK_PREFIX<<
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(p16, out, p8); // ffff*p

            p256Sqr(out, p16);
            {
                long i__prev1 = i;

                for (i = 0L; i < 15L; i++)
                {>>MARKER:FUNCTION_p256NegCond_BLOCK_PREFIX<<
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(p32, out, p16); // ffffffff*p

            p256Sqr(out, p32);

            {
                long i__prev1 = i;

                for (i = 0L; i < 31L; i++)
                {>>MARKER:FUNCTION_p256FromMont_BLOCK_PREFIX<<
                    p256Sqr(out, out);
                }


                i = i__prev1;
            }
            p256MulAsm(out, out, in);

            {
                long i__prev1 = i;

                for (i = 0L; i < 32L * 4L; i++)
                {>>MARKER:FUNCTION_p256MulAsm_BLOCK_PREFIX<<
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

        private static (long, long) boothW7(ulong @in)
        {
            long _p0 = default;
            long _p0 = default;

            ulong s = ~((in >> (int)(7L)) - 1L);
            ulong d = (1L << (int)(8L)) - in - 1L;
            d = (d & s) | (in & (~s));
            d = (d >> (int)(1L)) + (d & 1L);
            return (int(d), int(s & 1L));
        }

        private static void initTable()
        {
            p256PreFast = @new<[37][64]p256Point>(); 

            // TODO: For big endian, these slices should be in reverse byte order,
            // as found in the s390x implementation.
            ref p256Point basePoint = ref heap(new p256Point(x:[32]byte{0x3c,0x14,0xa9,0x18,0xd4,0x30,0xe7,0x79,0x01,0xb6,0xed,0x5f,0xfc,0x95,0xba,0x75,0x10,0x25,0x62,0x77,0x2b,0x73,0xfb,0x79,0xc6,0x55,0x37,0xa5,0x76,0x5f,0x90,0x18},y:[32]byte{0x0a,0x56,0x95,0xce,0x57,0x53,0xf2,0xdd,0x5c,0xe4,0x19,0xba,0xe4,0xb8,0x4a,0x8b,0x25,0xf3,0x21,0xdd,0x88,0x86,0xe8,0xd2,0x85,0x5d,0x88,0x25,0x18,0xff,0x71,0x85},z:[32]byte{0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xfe,0xff,0xff,0xff,0x00,0x00,0x00,0x00},), out ptr<p256Point> _addr_basePoint);

            ptr<p256Point> t1 = @new<p256Point>();
            ptr<p256Point> t2 = @new<p256Point>();
            t2.val = basePoint;

            var zInv = make_slice<byte>(32L);
            var zInvSq = make_slice<byte>(32L);
            for (long j = 0L; j < 64L; j++)
            {
                t1.val = t2.val;
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
                    p256PointDoubleAsm(t2, _addr_basePoint);
                }
                else
                {
                    p256PointAddAsm(t2, t2, _addr_basePoint);
                }

            }


        }

        private static void p256BaseMult(this ptr<p256Point> _addr_p, slice<byte> scalar)
        {
            ref p256Point p = ref _addr_p.val;
 
            // TODO: For big endian, the index should be 31 not 0.
            var wvalue = (uint(scalar[0L]) << (int)(1L)) & 0xffUL;
            var (sel, sign) = boothW7(uint(wvalue));
            p256SelectBase(_addr_p, p256PreFast[0L][..], sel);
            p256NegCond(_addr_p, sign);

            copy(p.z[..], one[..]);
            ref p256Point t0 = ref heap(out ptr<p256Point> _addr_t0);

            copy(t0.z[..], one[..]);

            var index = uint(6L);
            var zero = sel;
            for (long i = 1L; i < 37L; i++)
            { 
                // TODO: For big endian, use the same index values as found
                // in the  s390x implementation.
                if (index < 247L)
                {
                    wvalue = ((uint(scalar[index / 8L]) >> (int)((index % 8L))) + (uint(scalar[index / 8L + 1L]) << (int)((8L - (index % 8L))))) & 0xffUL;
                }
                else
                {
                    wvalue = (uint(scalar[index / 8L]) >> (int)((index % 8L))) & 0xffUL;
                }

                index += 7L;
                sel, sign = boothW7(uint(wvalue));
                p256SelectBase(_addr_t0, p256PreFast[i][..], sel);
                p256PointAddAffineAsm(_addr_p, _addr_p, _addr_t0, sign, sel, zero);
                zero |= sel;

            }


        }

        private static void p256ScalarMult(this ptr<p256Point> _addr_p, slice<byte> scalar)
        {
            ref p256Point p = ref _addr_p.val;
 
            // precomp is a table of precomputed points that stores powers of p
            // from p^1 to p^16.
            array<p256Point> precomp = new array<p256Point>(16L);
            ref p256Point t0 = ref heap(out ptr<p256Point> _addr_t0);            ref p256Point t1 = ref heap(out ptr<p256Point> _addr_t1);            ref p256Point t2 = ref heap(out ptr<p256Point> _addr_t2);            ref p256Point t3 = ref heap(out ptr<p256Point> _addr_t3);



            _addr_precomp[0L].val = p.val;
            p256PointDoubleAsm(_addr_t0, _addr_p);
            p256PointDoubleAsm(_addr_t1, _addr_t0);
            p256PointDoubleAsm(_addr_t2, _addr_t1);
            p256PointDoubleAsm(_addr_t3, _addr_t2);
            _addr_precomp[1L].val = t0;
            _addr_precomp[3L].val = t1;
            _addr_precomp[7L].val = t2;
            _addr_precomp[15L].val = t3;

            p256PointAddAsm(_addr_t0, _addr_t0, _addr_p);
            p256PointAddAsm(_addr_t1, _addr_t1, _addr_p);
            p256PointAddAsm(_addr_t2, _addr_t2, _addr_p);

            _addr_precomp[2L].val = t0;
            _addr_precomp[4L].val = t1;
            _addr_precomp[8L].val = t2;

            p256PointDoubleAsm(_addr_t0, _addr_t0);
            p256PointDoubleAsm(_addr_t1, _addr_t1);
            _addr_precomp[5L].val = t0;
            _addr_precomp[9L].val = t1;

            p256PointAddAsm(_addr_t2, _addr_t0, _addr_p);
            p256PointAddAsm(_addr_t1, _addr_t1, _addr_p);
            _addr_precomp[6L].val = t2;
            _addr_precomp[10L].val = t1;

            p256PointDoubleAsm(_addr_t0, _addr_t0);
            p256PointDoubleAsm(_addr_t2, _addr_t2);
            _addr_precomp[11L].val = t0;
            _addr_precomp[13L].val = t2;

            p256PointAddAsm(_addr_t0, _addr_t0, _addr_p);
            p256PointAddAsm(_addr_t2, _addr_t2, _addr_p);
            _addr_precomp[12L].val = t0;
            _addr_precomp[14L].val = t2; 

            // Start scanning the window from top bit
            var index = uint(254L);
            long sel = default;            long sign = default; 

            // TODO: For big endian, use index found in s390x implementation.
 

            // TODO: For big endian, use index found in s390x implementation.
            var wvalue = (uint(scalar[index / 8L]) >> (int)((index % 8L))) & 0x3fUL;
            sel, _ = boothW5(uint(wvalue));
            p256Select(_addr_p, precomp[..], sel);
            var zero = sel;

            while (index > 4L)
            {
                index -= 5L;
                p256PointDoubleAsm(_addr_p, _addr_p);
                p256PointDoubleAsm(_addr_p, _addr_p);
                p256PointDoubleAsm(_addr_p, _addr_p);
                p256PointDoubleAsm(_addr_p, _addr_p);
                p256PointDoubleAsm(_addr_p, _addr_p); 

                // TODO: For big endian, use index values as found in s390x implementation.
                if (index < 247L)
                {
                    wvalue = ((uint(scalar[index / 8L]) >> (int)((index % 8L))) + (uint(scalar[index / 8L + 1L]) << (int)((8L - (index % 8L))))) & 0x3fUL;
                }
                else
                {
                    wvalue = (uint(scalar[index / 8L]) >> (int)((index % 8L))) & 0x3fUL;
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

            // TODO: Use index for big endian as found in s390x implementation.
            wvalue = (uint(scalar[0L]) << (int)(1L)) & 0x3fUL;
            sel, sign = boothW5(uint(wvalue));

            p256Select(_addr_t0, precomp[..], sel);
            p256NegCond(_addr_t0, sign);
            p256PointAddAsm(_addr_t1, _addr_p, _addr_t0);
            p256MovCond(_addr_t1, _addr_t1, _addr_p, sel);
            p256MovCond(_addr_p, _addr_t1, _addr_t0, zero);

        }
    }
}}
