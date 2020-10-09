// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package edwards25519 -- go2cs converted at 2020 October 09 05:55:45 UTC
// import "cmd/vendor/golang.org/x/crypto/ed25519/internal/edwards25519" ==> using edwards25519 = go.cmd.vendor.golang.org.x.crypto.ed25519.@internal.edwards25519_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\crypto\ed25519\internal\edwards25519\edwards25519.go
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto {
namespace ed25519 {
namespace @internal
{
    public static partial class edwards25519_package
    {
        // This code is a port of the public domain, “ref10” implementation of ed25519
        // from SUPERCOP.

        // FieldElement represents an element of the field GF(2^255 - 19).  An element
        // t, entries t[0]...t[9], represents the integer t[0]+2^26 t[1]+2^51 t[2]+2^77
        // t[3]+2^102 t[4]+...+2^230 t[9].  Bounds on each t[i] vary depending on
        // context.
        public partial struct FieldElement // : array<int>
        {
        }

        private static FieldElement zero = default;

        public static void FeZero(ptr<FieldElement> _addr_fe)
        {
            ref FieldElement fe = ref _addr_fe.val;

            copy(fe[..], zero[..]);
        }

        public static void FeOne(ptr<FieldElement> _addr_fe)
        {
            ref FieldElement fe = ref _addr_fe.val;

            FeZero(_addr_fe);
            fe[0L] = 1L;
        }

        public static void FeAdd(ptr<FieldElement> _addr_dst, ptr<FieldElement> _addr_a, ptr<FieldElement> _addr_b)
        {
            ref FieldElement dst = ref _addr_dst.val;
            ref FieldElement a = ref _addr_a.val;
            ref FieldElement b = ref _addr_b.val;

            dst[0L] = a[0L] + b[0L];
            dst[1L] = a[1L] + b[1L];
            dst[2L] = a[2L] + b[2L];
            dst[3L] = a[3L] + b[3L];
            dst[4L] = a[4L] + b[4L];
            dst[5L] = a[5L] + b[5L];
            dst[6L] = a[6L] + b[6L];
            dst[7L] = a[7L] + b[7L];
            dst[8L] = a[8L] + b[8L];
            dst[9L] = a[9L] + b[9L];
        }

        public static void FeSub(ptr<FieldElement> _addr_dst, ptr<FieldElement> _addr_a, ptr<FieldElement> _addr_b)
        {
            ref FieldElement dst = ref _addr_dst.val;
            ref FieldElement a = ref _addr_a.val;
            ref FieldElement b = ref _addr_b.val;

            dst[0L] = a[0L] - b[0L];
            dst[1L] = a[1L] - b[1L];
            dst[2L] = a[2L] - b[2L];
            dst[3L] = a[3L] - b[3L];
            dst[4L] = a[4L] - b[4L];
            dst[5L] = a[5L] - b[5L];
            dst[6L] = a[6L] - b[6L];
            dst[7L] = a[7L] - b[7L];
            dst[8L] = a[8L] - b[8L];
            dst[9L] = a[9L] - b[9L];
        }

        public static void FeCopy(ptr<FieldElement> _addr_dst, ptr<FieldElement> _addr_src)
        {
            ref FieldElement dst = ref _addr_dst.val;
            ref FieldElement src = ref _addr_src.val;

            copy(dst[..], src[..]);
        }

        // Replace (f,g) with (g,g) if b == 1;
        // replace (f,g) with (f,g) if b == 0.
        //
        // Preconditions: b in {0,1}.
        public static void FeCMove(ptr<FieldElement> _addr_f, ptr<FieldElement> _addr_g, int b)
        {
            ref FieldElement f = ref _addr_f.val;
            ref FieldElement g = ref _addr_g.val;

            b = -b;
            f[0L] ^= b & (f[0L] ^ g[0L]);
            f[1L] ^= b & (f[1L] ^ g[1L]);
            f[2L] ^= b & (f[2L] ^ g[2L]);
            f[3L] ^= b & (f[3L] ^ g[3L]);
            f[4L] ^= b & (f[4L] ^ g[4L]);
            f[5L] ^= b & (f[5L] ^ g[5L]);
            f[6L] ^= b & (f[6L] ^ g[6L]);
            f[7L] ^= b & (f[7L] ^ g[7L]);
            f[8L] ^= b & (f[8L] ^ g[8L]);
            f[9L] ^= b & (f[9L] ^ g[9L]);
        }

        private static long load3(slice<byte> @in)
        {
            long r = default;
            r = int64(in[0L]);
            r |= int64(in[1L]) << (int)(8L);
            r |= int64(in[2L]) << (int)(16L);
            return r;
        }

        private static long load4(slice<byte> @in)
        {
            long r = default;
            r = int64(in[0L]);
            r |= int64(in[1L]) << (int)(8L);
            r |= int64(in[2L]) << (int)(16L);
            r |= int64(in[3L]) << (int)(24L);
            return r;
        }

        public static void FeFromBytes(ptr<FieldElement> _addr_dst, ptr<array<byte>> _addr_src)
        {
            ref FieldElement dst = ref _addr_dst.val;
            ref array<byte> src = ref _addr_src.val;

            var h0 = load4(src[..]);
            var h1 = load3(src[4L..]) << (int)(6L);
            var h2 = load3(src[7L..]) << (int)(5L);
            var h3 = load3(src[10L..]) << (int)(3L);
            var h4 = load3(src[13L..]) << (int)(2L);
            var h5 = load4(src[16L..]);
            var h6 = load3(src[20L..]) << (int)(7L);
            var h7 = load3(src[23L..]) << (int)(5L);
            var h8 = load3(src[26L..]) << (int)(4L);
            var h9 = (load3(src[29L..]) & 8388607L) << (int)(2L);

            FeCombine(_addr_dst, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);
        }

        // FeToBytes marshals h to s.
        // Preconditions:
        //   |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
        //
        // Write p=2^255-19; q=floor(h/p).
        // Basic claim: q = floor(2^(-255)(h + 19 2^(-25)h9 + 2^(-1))).
        //
        // Proof:
        //   Have |h|<=p so |q|<=1 so |19^2 2^(-255) q|<1/4.
        //   Also have |h-2^230 h9|<2^230 so |19 2^(-255)(h-2^230 h9)|<1/4.
        //
        //   Write y=2^(-1)-19^2 2^(-255)q-19 2^(-255)(h-2^230 h9).
        //   Then 0<y<1.
        //
        //   Write r=h-pq.
        //   Have 0<=r<=p-1=2^255-20.
        //   Thus 0<=r+19(2^-255)r<r+19(2^-255)2^255<=2^255-1.
        //
        //   Write x=r+19(2^-255)r+y.
        //   Then 0<x<2^255 so floor(2^(-255)x) = 0 so floor(q+2^(-255)x) = q.
        //
        //   Have q+2^(-255)x = 2^(-255)(h + 19 2^(-25) h9 + 2^(-1))
        //   so floor(2^(-255)(h + 19 2^(-25) h9 + 2^(-1))) = q.
        public static void FeToBytes(ptr<array<byte>> _addr_s, ptr<FieldElement> _addr_h)
        {
            ref array<byte> s = ref _addr_s.val;
            ref FieldElement h = ref _addr_h.val;

            array<int> carry = new array<int>(10L);

            long q = (19L * h[9L] + (1L << (int)(24L))) >> (int)(25L);
            q = (h[0L] + q) >> (int)(26L);
            q = (h[1L] + q) >> (int)(25L);
            q = (h[2L] + q) >> (int)(26L);
            q = (h[3L] + q) >> (int)(25L);
            q = (h[4L] + q) >> (int)(26L);
            q = (h[5L] + q) >> (int)(25L);
            q = (h[6L] + q) >> (int)(26L);
            q = (h[7L] + q) >> (int)(25L);
            q = (h[8L] + q) >> (int)(26L);
            q = (h[9L] + q) >> (int)(25L); 

            // Goal: Output h-(2^255-19)q, which is between 0 and 2^255-20.
            h[0L] += 19L * q; 
            // Goal: Output h-2^255 q, which is between 0 and 2^255-20.

            carry[0L] = h[0L] >> (int)(26L);
            h[1L] += carry[0L];
            h[0L] -= carry[0L] << (int)(26L);
            carry[1L] = h[1L] >> (int)(25L);
            h[2L] += carry[1L];
            h[1L] -= carry[1L] << (int)(25L);
            carry[2L] = h[2L] >> (int)(26L);
            h[3L] += carry[2L];
            h[2L] -= carry[2L] << (int)(26L);
            carry[3L] = h[3L] >> (int)(25L);
            h[4L] += carry[3L];
            h[3L] -= carry[3L] << (int)(25L);
            carry[4L] = h[4L] >> (int)(26L);
            h[5L] += carry[4L];
            h[4L] -= carry[4L] << (int)(26L);
            carry[5L] = h[5L] >> (int)(25L);
            h[6L] += carry[5L];
            h[5L] -= carry[5L] << (int)(25L);
            carry[6L] = h[6L] >> (int)(26L);
            h[7L] += carry[6L];
            h[6L] -= carry[6L] << (int)(26L);
            carry[7L] = h[7L] >> (int)(25L);
            h[8L] += carry[7L];
            h[7L] -= carry[7L] << (int)(25L);
            carry[8L] = h[8L] >> (int)(26L);
            h[9L] += carry[8L];
            h[8L] -= carry[8L] << (int)(26L);
            carry[9L] = h[9L] >> (int)(25L);
            h[9L] -= carry[9L] << (int)(25L); 
            // h10 = carry9

            // Goal: Output h[0]+...+2^255 h10-2^255 q, which is between 0 and 2^255-20.
            // Have h[0]+...+2^230 h[9] between 0 and 2^255-1;
            // evidently 2^255 h10-2^255 q = 0.
            // Goal: Output h[0]+...+2^230 h[9].

            s[0L] = byte(h[0L] >> (int)(0L));
            s[1L] = byte(h[0L] >> (int)(8L));
            s[2L] = byte(h[0L] >> (int)(16L));
            s[3L] = byte((h[0L] >> (int)(24L)) | (h[1L] << (int)(2L)));
            s[4L] = byte(h[1L] >> (int)(6L));
            s[5L] = byte(h[1L] >> (int)(14L));
            s[6L] = byte((h[1L] >> (int)(22L)) | (h[2L] << (int)(3L)));
            s[7L] = byte(h[2L] >> (int)(5L));
            s[8L] = byte(h[2L] >> (int)(13L));
            s[9L] = byte((h[2L] >> (int)(21L)) | (h[3L] << (int)(5L)));
            s[10L] = byte(h[3L] >> (int)(3L));
            s[11L] = byte(h[3L] >> (int)(11L));
            s[12L] = byte((h[3L] >> (int)(19L)) | (h[4L] << (int)(6L)));
            s[13L] = byte(h[4L] >> (int)(2L));
            s[14L] = byte(h[4L] >> (int)(10L));
            s[15L] = byte(h[4L] >> (int)(18L));
            s[16L] = byte(h[5L] >> (int)(0L));
            s[17L] = byte(h[5L] >> (int)(8L));
            s[18L] = byte(h[5L] >> (int)(16L));
            s[19L] = byte((h[5L] >> (int)(24L)) | (h[6L] << (int)(1L)));
            s[20L] = byte(h[6L] >> (int)(7L));
            s[21L] = byte(h[6L] >> (int)(15L));
            s[22L] = byte((h[6L] >> (int)(23L)) | (h[7L] << (int)(3L)));
            s[23L] = byte(h[7L] >> (int)(5L));
            s[24L] = byte(h[7L] >> (int)(13L));
            s[25L] = byte((h[7L] >> (int)(21L)) | (h[8L] << (int)(4L)));
            s[26L] = byte(h[8L] >> (int)(4L));
            s[27L] = byte(h[8L] >> (int)(12L));
            s[28L] = byte((h[8L] >> (int)(20L)) | (h[9L] << (int)(6L)));
            s[29L] = byte(h[9L] >> (int)(2L));
            s[30L] = byte(h[9L] >> (int)(10L));
            s[31L] = byte(h[9L] >> (int)(18L));

        }

        public static byte FeIsNegative(ptr<FieldElement> _addr_f)
        {
            ref FieldElement f = ref _addr_f.val;

            ref array<byte> s = ref heap(new array<byte>(32L), out ptr<array<byte>> _addr_s);
            FeToBytes(_addr_s, _addr_f);
            return s[0L] & 1L;
        }

        public static int FeIsNonZero(ptr<FieldElement> _addr_f)
        {
            ref FieldElement f = ref _addr_f.val;

            ref array<byte> s = ref heap(new array<byte>(32L), out ptr<array<byte>> _addr_s);
            FeToBytes(_addr_s, _addr_f);
            byte x = default;
            foreach (var (_, b) in s)
            {
                x |= b;
            }
            x |= x >> (int)(4L);
            x |= x >> (int)(2L);
            x |= x >> (int)(1L);
            return int32(x & 1L);

        }

        // FeNeg sets h = -f
        //
        // Preconditions:
        //    |f| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
        //
        // Postconditions:
        //    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
        public static void FeNeg(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f)
        {
            ref FieldElement h = ref _addr_h.val;
            ref FieldElement f = ref _addr_f.val;

            h[0L] = -f[0L];
            h[1L] = -f[1L];
            h[2L] = -f[2L];
            h[3L] = -f[3L];
            h[4L] = -f[4L];
            h[5L] = -f[5L];
            h[6L] = -f[6L];
            h[7L] = -f[7L];
            h[8L] = -f[8L];
            h[9L] = -f[9L];
        }

        public static void FeCombine(ptr<FieldElement> _addr_h, long h0, long h1, long h2, long h3, long h4, long h5, long h6, long h7, long h8, long h9)
        {
            ref FieldElement h = ref _addr_h.val;

            long c0 = default;            long c1 = default;            long c2 = default;            long c3 = default;            long c4 = default;            long c5 = default;            long c6 = default;            long c7 = default;            long c8 = default;            long c9 = default;

            /*
                  |h0| <= (1.1*1.1*2^52*(1+19+19+19+19)+1.1*1.1*2^50*(38+38+38+38+38))
                    i.e. |h0| <= 1.2*2^59; narrower ranges for h2, h4, h6, h8
                  |h1| <= (1.1*1.1*2^51*(1+1+19+19+19+19+19+19+19+19))
                    i.e. |h1| <= 1.5*2^58; narrower ranges for h3, h5, h7, h9
                */



            /*
                  |h0| <= (1.1*1.1*2^52*(1+19+19+19+19)+1.1*1.1*2^50*(38+38+38+38+38))
                    i.e. |h0| <= 1.2*2^59; narrower ranges for h2, h4, h6, h8
                  |h1| <= (1.1*1.1*2^51*(1+1+19+19+19+19+19+19+19+19))
                    i.e. |h1| <= 1.5*2^58; narrower ranges for h3, h5, h7, h9
                */

            c0 = (h0 + (1L << (int)(25L))) >> (int)(26L);
            h1 += c0;
            h0 -= c0 << (int)(26L);
            c4 = (h4 + (1L << (int)(25L))) >> (int)(26L);
            h5 += c4;
            h4 -= c4 << (int)(26L);
            /* |h0| <= 2^25 */
            /* |h4| <= 2^25 */
            /* |h1| <= 1.51*2^58 */
            /* |h5| <= 1.51*2^58 */

            c1 = (h1 + (1L << (int)(24L))) >> (int)(25L);
            h2 += c1;
            h1 -= c1 << (int)(25L);
            c5 = (h5 + (1L << (int)(24L))) >> (int)(25L);
            h6 += c5;
            h5 -= c5 << (int)(25L);
            /* |h1| <= 2^24; from now on fits into int32 */
            /* |h5| <= 2^24; from now on fits into int32 */
            /* |h2| <= 1.21*2^59 */
            /* |h6| <= 1.21*2^59 */

            c2 = (h2 + (1L << (int)(25L))) >> (int)(26L);
            h3 += c2;
            h2 -= c2 << (int)(26L);
            c6 = (h6 + (1L << (int)(25L))) >> (int)(26L);
            h7 += c6;
            h6 -= c6 << (int)(26L);
            /* |h2| <= 2^25; from now on fits into int32 unchanged */
            /* |h6| <= 2^25; from now on fits into int32 unchanged */
            /* |h3| <= 1.51*2^58 */
            /* |h7| <= 1.51*2^58 */

            c3 = (h3 + (1L << (int)(24L))) >> (int)(25L);
            h4 += c3;
            h3 -= c3 << (int)(25L);
            c7 = (h7 + (1L << (int)(24L))) >> (int)(25L);
            h8 += c7;
            h7 -= c7 << (int)(25L);
            /* |h3| <= 2^24; from now on fits into int32 unchanged */
            /* |h7| <= 2^24; from now on fits into int32 unchanged */
            /* |h4| <= 1.52*2^33 */
            /* |h8| <= 1.52*2^33 */

            c4 = (h4 + (1L << (int)(25L))) >> (int)(26L);
            h5 += c4;
            h4 -= c4 << (int)(26L);
            c8 = (h8 + (1L << (int)(25L))) >> (int)(26L);
            h9 += c8;
            h8 -= c8 << (int)(26L);
            /* |h4| <= 2^25; from now on fits into int32 unchanged */
            /* |h8| <= 2^25; from now on fits into int32 unchanged */
            /* |h5| <= 1.01*2^24 */
            /* |h9| <= 1.51*2^58 */

            c9 = (h9 + (1L << (int)(24L))) >> (int)(25L);
            h0 += c9 * 19L;
            h9 -= c9 << (int)(25L);
            /* |h9| <= 2^24; from now on fits into int32 unchanged */
            /* |h0| <= 1.8*2^37 */

            c0 = (h0 + (1L << (int)(25L))) >> (int)(26L);
            h1 += c0;
            h0 -= c0 << (int)(26L);
            /* |h0| <= 2^25; from now on fits into int32 unchanged */
            /* |h1| <= 1.01*2^24 */

            h[0L] = int32(h0);
            h[1L] = int32(h1);
            h[2L] = int32(h2);
            h[3L] = int32(h3);
            h[4L] = int32(h4);
            h[5L] = int32(h5);
            h[6L] = int32(h6);
            h[7L] = int32(h7);
            h[8L] = int32(h8);
            h[9L] = int32(h9);

        }

        // FeMul calculates h = f * g
        // Can overlap h with f or g.
        //
        // Preconditions:
        //    |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
        //    |g| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
        //
        // Postconditions:
        //    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
        //
        // Notes on implementation strategy:
        //
        // Using schoolbook multiplication.
        // Karatsuba would save a little in some cost models.
        //
        // Most multiplications by 2 and 19 are 32-bit precomputations;
        // cheaper than 64-bit postcomputations.
        //
        // There is one remaining multiplication by 19 in the carry chain;
        // one *19 precomputation can be merged into this,
        // but the resulting data flow is considerably less clean.
        //
        // There are 12 carries below.
        // 10 of them are 2-way parallelizable and vectorizable.
        // Can get away with 11 carries, but then data flow is much deeper.
        //
        // With tighter constraints on inputs, can squeeze carries into int32.
        public static void FeMul(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f, ptr<FieldElement> _addr_g)
        {
            ref FieldElement h = ref _addr_h.val;
            ref FieldElement f = ref _addr_f.val;
            ref FieldElement g = ref _addr_g.val;

            var f0 = int64(f[0L]);
            var f1 = int64(f[1L]);
            var f2 = int64(f[2L]);
            var f3 = int64(f[3L]);
            var f4 = int64(f[4L]);
            var f5 = int64(f[5L]);
            var f6 = int64(f[6L]);
            var f7 = int64(f[7L]);
            var f8 = int64(f[8L]);
            var f9 = int64(f[9L]);

            var f1_2 = int64(2L * f[1L]);
            var f3_2 = int64(2L * f[3L]);
            var f5_2 = int64(2L * f[5L]);
            var f7_2 = int64(2L * f[7L]);
            var f9_2 = int64(2L * f[9L]);

            var g0 = int64(g[0L]);
            var g1 = int64(g[1L]);
            var g2 = int64(g[2L]);
            var g3 = int64(g[3L]);
            var g4 = int64(g[4L]);
            var g5 = int64(g[5L]);
            var g6 = int64(g[6L]);
            var g7 = int64(g[7L]);
            var g8 = int64(g[8L]);
            var g9 = int64(g[9L]);

            var g1_19 = int64(19L * g[1L]); /* 1.4*2^29 */
            var g2_19 = int64(19L * g[2L]); /* 1.4*2^30; still ok */
            var g3_19 = int64(19L * g[3L]);
            var g4_19 = int64(19L * g[4L]);
            var g5_19 = int64(19L * g[5L]);
            var g6_19 = int64(19L * g[6L]);
            var g7_19 = int64(19L * g[7L]);
            var g8_19 = int64(19L * g[8L]);
            var g9_19 = int64(19L * g[9L]);

            var h0 = f0 * g0 + f1_2 * g9_19 + f2 * g8_19 + f3_2 * g7_19 + f4 * g6_19 + f5_2 * g5_19 + f6 * g4_19 + f7_2 * g3_19 + f8 * g2_19 + f9_2 * g1_19;
            var h1 = f0 * g1 + f1 * g0 + f2 * g9_19 + f3 * g8_19 + f4 * g7_19 + f5 * g6_19 + f6 * g5_19 + f7 * g4_19 + f8 * g3_19 + f9 * g2_19;
            var h2 = f0 * g2 + f1_2 * g1 + f2 * g0 + f3_2 * g9_19 + f4 * g8_19 + f5_2 * g7_19 + f6 * g6_19 + f7_2 * g5_19 + f8 * g4_19 + f9_2 * g3_19;
            var h3 = f0 * g3 + f1 * g2 + f2 * g1 + f3 * g0 + f4 * g9_19 + f5 * g8_19 + f6 * g7_19 + f7 * g6_19 + f8 * g5_19 + f9 * g4_19;
            var h4 = f0 * g4 + f1_2 * g3 + f2 * g2 + f3_2 * g1 + f4 * g0 + f5_2 * g9_19 + f6 * g8_19 + f7_2 * g7_19 + f8 * g6_19 + f9_2 * g5_19;
            var h5 = f0 * g5 + f1 * g4 + f2 * g3 + f3 * g2 + f4 * g1 + f5 * g0 + f6 * g9_19 + f7 * g8_19 + f8 * g7_19 + f9 * g6_19;
            var h6 = f0 * g6 + f1_2 * g5 + f2 * g4 + f3_2 * g3 + f4 * g2 + f5_2 * g1 + f6 * g0 + f7_2 * g9_19 + f8 * g8_19 + f9_2 * g7_19;
            var h7 = f0 * g7 + f1 * g6 + f2 * g5 + f3 * g4 + f4 * g3 + f5 * g2 + f6 * g1 + f7 * g0 + f8 * g9_19 + f9 * g8_19;
            var h8 = f0 * g8 + f1_2 * g7 + f2 * g6 + f3_2 * g5 + f4 * g4 + f5_2 * g3 + f6 * g2 + f7_2 * g1 + f8 * g0 + f9_2 * g9_19;
            var h9 = f0 * g9 + f1 * g8 + f2 * g7 + f3 * g6 + f4 * g5 + f5 * g4 + f6 * g3 + f7 * g2 + f8 * g1 + f9 * g0;

            FeCombine(_addr_h, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);

        }

        private static (long, long, long, long, long, long, long, long, long, long) feSquare(ptr<FieldElement> _addr_f)
        {
            long h0 = default;
            long h1 = default;
            long h2 = default;
            long h3 = default;
            long h4 = default;
            long h5 = default;
            long h6 = default;
            long h7 = default;
            long h8 = default;
            long h9 = default;
            ref FieldElement f = ref _addr_f.val;

            var f0 = int64(f[0L]);
            var f1 = int64(f[1L]);
            var f2 = int64(f[2L]);
            var f3 = int64(f[3L]);
            var f4 = int64(f[4L]);
            var f5 = int64(f[5L]);
            var f6 = int64(f[6L]);
            var f7 = int64(f[7L]);
            var f8 = int64(f[8L]);
            var f9 = int64(f[9L]);
            var f0_2 = int64(2L * f[0L]);
            var f1_2 = int64(2L * f[1L]);
            var f2_2 = int64(2L * f[2L]);
            var f3_2 = int64(2L * f[3L]);
            var f4_2 = int64(2L * f[4L]);
            var f5_2 = int64(2L * f[5L]);
            var f6_2 = int64(2L * f[6L]);
            var f7_2 = int64(2L * f[7L]);
            long f5_38 = 38L * f5; // 1.31*2^30
            long f6_19 = 19L * f6; // 1.31*2^30
            long f7_38 = 38L * f7; // 1.31*2^30
            long f8_19 = 19L * f8; // 1.31*2^30
            long f9_38 = 38L * f9; // 1.31*2^30

            h0 = f0 * f0 + f1_2 * f9_38 + f2_2 * f8_19 + f3_2 * f7_38 + f4_2 * f6_19 + f5 * f5_38;
            h1 = f0_2 * f1 + f2 * f9_38 + f3_2 * f8_19 + f4 * f7_38 + f5_2 * f6_19;
            h2 = f0_2 * f2 + f1_2 * f1 + f3_2 * f9_38 + f4_2 * f8_19 + f5_2 * f7_38 + f6 * f6_19;
            h3 = f0_2 * f3 + f1_2 * f2 + f4 * f9_38 + f5_2 * f8_19 + f6 * f7_38;
            h4 = f0_2 * f4 + f1_2 * f3_2 + f2 * f2 + f5_2 * f9_38 + f6_2 * f8_19 + f7 * f7_38;
            h5 = f0_2 * f5 + f1_2 * f4 + f2_2 * f3 + f6 * f9_38 + f7_2 * f8_19;
            h6 = f0_2 * f6 + f1_2 * f5_2 + f2_2 * f4 + f3_2 * f3 + f7_2 * f9_38 + f8 * f8_19;
            h7 = f0_2 * f7 + f1_2 * f6 + f2_2 * f5 + f3_2 * f4 + f8 * f9_38;
            h8 = f0_2 * f8 + f1_2 * f7_2 + f2_2 * f6 + f3_2 * f5_2 + f4 * f4 + f9 * f9_38;
            h9 = f0_2 * f9 + f1_2 * f8 + f2_2 * f7 + f3_2 * f6 + f4_2 * f5;

            return ;

        }

        // FeSquare calculates h = f*f. Can overlap h with f.
        //
        // Preconditions:
        //    |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
        //
        // Postconditions:
        //    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
        public static void FeSquare(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f)
        {
            ref FieldElement h = ref _addr_h.val;
            ref FieldElement f = ref _addr_f.val;

            var (h0, h1, h2, h3, h4, h5, h6, h7, h8, h9) = feSquare(_addr_f);
            FeCombine(_addr_h, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);
        }

        // FeSquare2 sets h = 2 * f * f
        //
        // Can overlap h with f.
        //
        // Preconditions:
        //    |f| bounded by 1.65*2^26,1.65*2^25,1.65*2^26,1.65*2^25,etc.
        //
        // Postconditions:
        //    |h| bounded by 1.01*2^25,1.01*2^24,1.01*2^25,1.01*2^24,etc.
        // See fe_mul.c for discussion of implementation strategy.
        public static void FeSquare2(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f)
        {
            ref FieldElement h = ref _addr_h.val;
            ref FieldElement f = ref _addr_f.val;

            var (h0, h1, h2, h3, h4, h5, h6, h7, h8, h9) = feSquare(_addr_f);

            h0 += h0;
            h1 += h1;
            h2 += h2;
            h3 += h3;
            h4 += h4;
            h5 += h5;
            h6 += h6;
            h7 += h7;
            h8 += h8;
            h9 += h9;

            FeCombine(_addr_h, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9);
        }

        public static void FeInvert(ptr<FieldElement> _addr_@out, ptr<FieldElement> _addr_z)
        {
            ref FieldElement @out = ref _addr_@out.val;
            ref FieldElement z = ref _addr_z.val;

            ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);            ref FieldElement t1 = ref heap(out ptr<FieldElement> _addr_t1);            ref FieldElement t2 = ref heap(out ptr<FieldElement> _addr_t2);            ref FieldElement t3 = ref heap(out ptr<FieldElement> _addr_t3);

            long i = default;

            FeSquare(_addr_t0, _addr_z); // 2^1
            FeSquare(_addr_t1, _addr_t0); // 2^2
            for (i = 1L; i < 2L; i++)
            { // 2^3
                FeSquare(_addr_t1, _addr_t1);

            }

            FeMul(_addr_t1, _addr_z, _addr_t1); // 2^3 + 2^0
            FeMul(_addr_t0, _addr_t0, _addr_t1); // 2^3 + 2^1 + 2^0
            FeSquare(_addr_t2, _addr_t0); // 2^4 + 2^2 + 2^1
            FeMul(_addr_t1, _addr_t1, _addr_t2); // 2^4 + 2^3 + 2^2 + 2^1 + 2^0
            FeSquare(_addr_t2, _addr_t1); // 5,4,3,2,1
            for (i = 1L; i < 5L; i++)
            { // 9,8,7,6,5
                FeSquare(_addr_t2, _addr_t2);

            }

            FeMul(_addr_t1, _addr_t2, _addr_t1); // 9,8,7,6,5,4,3,2,1,0
            FeSquare(_addr_t2, _addr_t1); // 10..1
            for (i = 1L; i < 10L; i++)
            { // 19..10
                FeSquare(_addr_t2, _addr_t2);

            }

            FeMul(_addr_t2, _addr_t2, _addr_t1); // 19..0
            FeSquare(_addr_t3, _addr_t2); // 20..1
            for (i = 1L; i < 20L; i++)
            { // 39..20
                FeSquare(_addr_t3, _addr_t3);

            }

            FeMul(_addr_t2, _addr_t3, _addr_t2); // 39..0
            FeSquare(_addr_t2, _addr_t2); // 40..1
            for (i = 1L; i < 10L; i++)
            { // 49..10
                FeSquare(_addr_t2, _addr_t2);

            }

            FeMul(_addr_t1, _addr_t2, _addr_t1); // 49..0
            FeSquare(_addr_t2, _addr_t1); // 50..1
            for (i = 1L; i < 50L; i++)
            { // 99..50
                FeSquare(_addr_t2, _addr_t2);

            }

            FeMul(_addr_t2, _addr_t2, _addr_t1); // 99..0
            FeSquare(_addr_t3, _addr_t2); // 100..1
            for (i = 1L; i < 100L; i++)
            { // 199..100
                FeSquare(_addr_t3, _addr_t3);

            }

            FeMul(_addr_t2, _addr_t3, _addr_t2); // 199..0
            FeSquare(_addr_t2, _addr_t2); // 200..1
            for (i = 1L; i < 50L; i++)
            { // 249..50
                FeSquare(_addr_t2, _addr_t2);

            }

            FeMul(_addr_t1, _addr_t2, _addr_t1); // 249..0
            FeSquare(_addr_t1, _addr_t1); // 250..1
            for (i = 1L; i < 5L; i++)
            { // 254..5
                FeSquare(_addr_t1, _addr_t1);

            }

            FeMul(_addr_out, _addr_t1, _addr_t0); // 254..5,3,1,0
        }

        private static void fePow22523(ptr<FieldElement> _addr_@out, ptr<FieldElement> _addr_z)
        {
            ref FieldElement @out = ref _addr_@out.val;
            ref FieldElement z = ref _addr_z.val;

            ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);            ref FieldElement t1 = ref heap(out ptr<FieldElement> _addr_t1);            ref FieldElement t2 = ref heap(out ptr<FieldElement> _addr_t2);

            long i = default;

            FeSquare(_addr_t0, _addr_z);
            for (i = 1L; i < 1L; i++)
            {
                FeSquare(_addr_t0, _addr_t0);
            }

            FeSquare(_addr_t1, _addr_t0);
            for (i = 1L; i < 2L; i++)
            {
                FeSquare(_addr_t1, _addr_t1);
            }

            FeMul(_addr_t1, _addr_z, _addr_t1);
            FeMul(_addr_t0, _addr_t0, _addr_t1);
            FeSquare(_addr_t0, _addr_t0);
            for (i = 1L; i < 1L; i++)
            {
                FeSquare(_addr_t0, _addr_t0);
            }

            FeMul(_addr_t0, _addr_t1, _addr_t0);
            FeSquare(_addr_t1, _addr_t0);
            for (i = 1L; i < 5L; i++)
            {
                FeSquare(_addr_t1, _addr_t1);
            }

            FeMul(_addr_t0, _addr_t1, _addr_t0);
            FeSquare(_addr_t1, _addr_t0);
            for (i = 1L; i < 10L; i++)
            {
                FeSquare(_addr_t1, _addr_t1);
            }

            FeMul(_addr_t1, _addr_t1, _addr_t0);
            FeSquare(_addr_t2, _addr_t1);
            for (i = 1L; i < 20L; i++)
            {
                FeSquare(_addr_t2, _addr_t2);
            }

            FeMul(_addr_t1, _addr_t2, _addr_t1);
            FeSquare(_addr_t1, _addr_t1);
            for (i = 1L; i < 10L; i++)
            {
                FeSquare(_addr_t1, _addr_t1);
            }

            FeMul(_addr_t0, _addr_t1, _addr_t0);
            FeSquare(_addr_t1, _addr_t0);
            for (i = 1L; i < 50L; i++)
            {
                FeSquare(_addr_t1, _addr_t1);
            }

            FeMul(_addr_t1, _addr_t1, _addr_t0);
            FeSquare(_addr_t2, _addr_t1);
            for (i = 1L; i < 100L; i++)
            {
                FeSquare(_addr_t2, _addr_t2);
            }

            FeMul(_addr_t1, _addr_t2, _addr_t1);
            FeSquare(_addr_t1, _addr_t1);
            for (i = 1L; i < 50L; i++)
            {
                FeSquare(_addr_t1, _addr_t1);
            }

            FeMul(_addr_t0, _addr_t1, _addr_t0);
            FeSquare(_addr_t0, _addr_t0);
            for (i = 1L; i < 2L; i++)
            {
                FeSquare(_addr_t0, _addr_t0);
            }

            FeMul(_addr_out, _addr_t0, _addr_z);

        }

        // Group elements are members of the elliptic curve -x^2 + y^2 = 1 + d * x^2 *
        // y^2 where d = -121665/121666.
        //
        // Several representations are used:
        //   ProjectiveGroupElement: (X:Y:Z) satisfying x=X/Z, y=Y/Z
        //   ExtendedGroupElement: (X:Y:Z:T) satisfying x=X/Z, y=Y/Z, XY=ZT
        //   CompletedGroupElement: ((X:Z),(Y:T)) satisfying x=X/Z, y=Y/T
        //   PreComputedGroupElement: (y+x,y-x,2dxy)

        public partial struct ProjectiveGroupElement
        {
            public FieldElement X;
            public FieldElement Y;
            public FieldElement Z;
        }

        public partial struct ExtendedGroupElement
        {
            public FieldElement X;
            public FieldElement Y;
            public FieldElement Z;
            public FieldElement T;
        }

        public partial struct CompletedGroupElement
        {
            public FieldElement X;
            public FieldElement Y;
            public FieldElement Z;
            public FieldElement T;
        }

        public partial struct PreComputedGroupElement
        {
            public FieldElement yPlusX;
            public FieldElement yMinusX;
            public FieldElement xy2d;
        }

        public partial struct CachedGroupElement
        {
            public FieldElement yPlusX;
            public FieldElement yMinusX;
            public FieldElement Z;
            public FieldElement T2d;
        }

        private static void Zero(this ptr<ProjectiveGroupElement> _addr_p)
        {
            ref ProjectiveGroupElement p = ref _addr_p.val;

            FeZero(_addr_p.X);
            FeOne(_addr_p.Y);
            FeOne(_addr_p.Z);
        }

        private static void Double(this ptr<ProjectiveGroupElement> _addr_p, ptr<CompletedGroupElement> _addr_r)
        {
            ref ProjectiveGroupElement p = ref _addr_p.val;
            ref CompletedGroupElement r = ref _addr_r.val;

            ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);

            FeSquare(_addr_r.X, _addr_p.X);
            FeSquare(_addr_r.Z, _addr_p.Y);
            FeSquare2(_addr_r.T, _addr_p.Z);
            FeAdd(_addr_r.Y, _addr_p.X, _addr_p.Y);
            FeSquare(_addr_t0, _addr_r.Y);
            FeAdd(_addr_r.Y, _addr_r.Z, _addr_r.X);
            FeSub(_addr_r.Z, _addr_r.Z, _addr_r.X);
            FeSub(_addr_r.X, _addr_t0, _addr_r.Y);
            FeSub(_addr_r.T, _addr_r.T, _addr_r.Z);
        }

        private static void ToBytes(this ptr<ProjectiveGroupElement> _addr_p, ptr<array<byte>> _addr_s)
        {
            ref ProjectiveGroupElement p = ref _addr_p.val;
            ref array<byte> s = ref _addr_s.val;

            ref FieldElement recip = ref heap(out ptr<FieldElement> _addr_recip);            ref FieldElement x = ref heap(out ptr<FieldElement> _addr_x);            ref FieldElement y = ref heap(out ptr<FieldElement> _addr_y);



            FeInvert(_addr_recip, _addr_p.Z);
            FeMul(_addr_x, _addr_p.X, _addr_recip);
            FeMul(_addr_y, _addr_p.Y, _addr_recip);
            FeToBytes(_addr_s, _addr_y);
            s[31L] ^= FeIsNegative(_addr_x) << (int)(7L);
        }

        private static void Zero(this ptr<ExtendedGroupElement> _addr_p)
        {
            ref ExtendedGroupElement p = ref _addr_p.val;

            FeZero(_addr_p.X);
            FeOne(_addr_p.Y);
            FeOne(_addr_p.Z);
            FeZero(_addr_p.T);
        }

        private static void Double(this ptr<ExtendedGroupElement> _addr_p, ptr<CompletedGroupElement> _addr_r)
        {
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref CompletedGroupElement r = ref _addr_r.val;

            ref ProjectiveGroupElement q = ref heap(out ptr<ProjectiveGroupElement> _addr_q);
            p.ToProjective(_addr_q);
            q.Double(r);
        }

        private static void ToCached(this ptr<ExtendedGroupElement> _addr_p, ptr<CachedGroupElement> _addr_r)
        {
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref CachedGroupElement r = ref _addr_r.val;

            FeAdd(_addr_r.yPlusX, _addr_p.Y, _addr_p.X);
            FeSub(_addr_r.yMinusX, _addr_p.Y, _addr_p.X);
            FeCopy(_addr_r.Z, _addr_p.Z);
            FeMul(_addr_r.T2d, _addr_p.T, _addr_d2);
        }

        private static void ToProjective(this ptr<ExtendedGroupElement> _addr_p, ptr<ProjectiveGroupElement> _addr_r)
        {
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref ProjectiveGroupElement r = ref _addr_r.val;

            FeCopy(_addr_r.X, _addr_p.X);
            FeCopy(_addr_r.Y, _addr_p.Y);
            FeCopy(_addr_r.Z, _addr_p.Z);
        }

        private static void ToBytes(this ptr<ExtendedGroupElement> _addr_p, ptr<array<byte>> _addr_s)
        {
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref array<byte> s = ref _addr_s.val;

            ref FieldElement recip = ref heap(out ptr<FieldElement> _addr_recip);            ref FieldElement x = ref heap(out ptr<FieldElement> _addr_x);            ref FieldElement y = ref heap(out ptr<FieldElement> _addr_y);



            FeInvert(_addr_recip, _addr_p.Z);
            FeMul(_addr_x, _addr_p.X, _addr_recip);
            FeMul(_addr_y, _addr_p.Y, _addr_recip);
            FeToBytes(_addr_s, _addr_y);
            s[31L] ^= FeIsNegative(_addr_x) << (int)(7L);
        }

        private static bool FromBytes(this ptr<ExtendedGroupElement> _addr_p, ptr<array<byte>> _addr_s)
        {
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref array<byte> s = ref _addr_s.val;

            ref FieldElement u = ref heap(out ptr<FieldElement> _addr_u);            ref FieldElement v = ref heap(out ptr<FieldElement> _addr_v);            ref FieldElement v3 = ref heap(out ptr<FieldElement> _addr_v3);            ref FieldElement vxx = ref heap(out ptr<FieldElement> _addr_vxx);            ref FieldElement check = ref heap(out ptr<FieldElement> _addr_check);



            FeFromBytes(_addr_p.Y, _addr_s);
            FeOne(_addr_p.Z);
            FeSquare(_addr_u, _addr_p.Y);
            FeMul(_addr_v, _addr_u, _addr_d);
            FeSub(_addr_u, _addr_u, _addr_p.Z); // y = y^2-1
            FeAdd(_addr_v, _addr_v, _addr_p.Z); // v = dy^2+1

            FeSquare(_addr_v3, _addr_v);
            FeMul(_addr_v3, _addr_v3, _addr_v); // v3 = v^3
            FeSquare(_addr_p.X, _addr_v3);
            FeMul(_addr_p.X, _addr_p.X, _addr_v);
            FeMul(_addr_p.X, _addr_p.X, _addr_u); // x = uv^7

            fePow22523(_addr_p.X, _addr_p.X); // x = (uv^7)^((q-5)/8)
            FeMul(_addr_p.X, _addr_p.X, _addr_v3);
            FeMul(_addr_p.X, _addr_p.X, _addr_u); // x = uv^3(uv^7)^((q-5)/8)

            ref array<byte> tmpX = ref heap(new array<byte>(32L), out ptr<array<byte>> _addr_tmpX);            array<byte> tmp2 = new array<byte>(32L);



            FeSquare(_addr_vxx, _addr_p.X);
            FeMul(_addr_vxx, _addr_vxx, _addr_v);
            FeSub(_addr_check, _addr_vxx, _addr_u); // vx^2-u
            if (FeIsNonZero(_addr_check) == 1L)
            {
                FeAdd(_addr_check, _addr_vxx, _addr_u); // vx^2+u
                if (FeIsNonZero(_addr_check) == 1L)
                {
                    return false;
                }

                FeMul(_addr_p.X, _addr_p.X, _addr_SqrtM1);

                FeToBytes(_addr_tmpX, _addr_p.X);
                {
                    FieldElement v__prev1 = v;

                    foreach (var (__i, __v) in tmpX)
                    {
                        i = __i;
                        v = __v;
                        tmp2[31L - i] = v;
                    }

                    v = v__prev1;
                }
            }

            if (FeIsNegative(_addr_p.X) != (s[31L] >> (int)(7L)))
            {
                FeNeg(_addr_p.X, _addr_p.X);
            }

            FeMul(_addr_p.T, _addr_p.X, _addr_p.Y);
            return true;

        }

        private static void ToProjective(this ptr<CompletedGroupElement> _addr_p, ptr<ProjectiveGroupElement> _addr_r)
        {
            ref CompletedGroupElement p = ref _addr_p.val;
            ref ProjectiveGroupElement r = ref _addr_r.val;

            FeMul(_addr_r.X, _addr_p.X, _addr_p.T);
            FeMul(_addr_r.Y, _addr_p.Y, _addr_p.Z);
            FeMul(_addr_r.Z, _addr_p.Z, _addr_p.T);
        }

        private static void ToExtended(this ptr<CompletedGroupElement> _addr_p, ptr<ExtendedGroupElement> _addr_r)
        {
            ref CompletedGroupElement p = ref _addr_p.val;
            ref ExtendedGroupElement r = ref _addr_r.val;

            FeMul(_addr_r.X, _addr_p.X, _addr_p.T);
            FeMul(_addr_r.Y, _addr_p.Y, _addr_p.Z);
            FeMul(_addr_r.Z, _addr_p.Z, _addr_p.T);
            FeMul(_addr_r.T, _addr_p.X, _addr_p.Y);
        }

        private static void Zero(this ptr<PreComputedGroupElement> _addr_p)
        {
            ref PreComputedGroupElement p = ref _addr_p.val;

            FeOne(_addr_p.yPlusX);
            FeOne(_addr_p.yMinusX);
            FeZero(_addr_p.xy2d);
        }

        private static void geAdd(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<CachedGroupElement> _addr_q)
        {
            ref CompletedGroupElement r = ref _addr_r.val;
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref CachedGroupElement q = ref _addr_q.val;

            ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);

            FeAdd(_addr_r.X, _addr_p.Y, _addr_p.X);
            FeSub(_addr_r.Y, _addr_p.Y, _addr_p.X);
            FeMul(_addr_r.Z, _addr_r.X, _addr_q.yPlusX);
            FeMul(_addr_r.Y, _addr_r.Y, _addr_q.yMinusX);
            FeMul(_addr_r.T, _addr_q.T2d, _addr_p.T);
            FeMul(_addr_r.X, _addr_p.Z, _addr_q.Z);
            FeAdd(_addr_t0, _addr_r.X, _addr_r.X);
            FeSub(_addr_r.X, _addr_r.Z, _addr_r.Y);
            FeAdd(_addr_r.Y, _addr_r.Z, _addr_r.Y);
            FeAdd(_addr_r.Z, _addr_t0, _addr_r.T);
            FeSub(_addr_r.T, _addr_t0, _addr_r.T);
        }

        private static void geSub(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<CachedGroupElement> _addr_q)
        {
            ref CompletedGroupElement r = ref _addr_r.val;
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref CachedGroupElement q = ref _addr_q.val;

            ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);

            FeAdd(_addr_r.X, _addr_p.Y, _addr_p.X);
            FeSub(_addr_r.Y, _addr_p.Y, _addr_p.X);
            FeMul(_addr_r.Z, _addr_r.X, _addr_q.yMinusX);
            FeMul(_addr_r.Y, _addr_r.Y, _addr_q.yPlusX);
            FeMul(_addr_r.T, _addr_q.T2d, _addr_p.T);
            FeMul(_addr_r.X, _addr_p.Z, _addr_q.Z);
            FeAdd(_addr_t0, _addr_r.X, _addr_r.X);
            FeSub(_addr_r.X, _addr_r.Z, _addr_r.Y);
            FeAdd(_addr_r.Y, _addr_r.Z, _addr_r.Y);
            FeSub(_addr_r.Z, _addr_t0, _addr_r.T);
            FeAdd(_addr_r.T, _addr_t0, _addr_r.T);
        }

        private static void geMixedAdd(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<PreComputedGroupElement> _addr_q)
        {
            ref CompletedGroupElement r = ref _addr_r.val;
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref PreComputedGroupElement q = ref _addr_q.val;

            ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);

            FeAdd(_addr_r.X, _addr_p.Y, _addr_p.X);
            FeSub(_addr_r.Y, _addr_p.Y, _addr_p.X);
            FeMul(_addr_r.Z, _addr_r.X, _addr_q.yPlusX);
            FeMul(_addr_r.Y, _addr_r.Y, _addr_q.yMinusX);
            FeMul(_addr_r.T, _addr_q.xy2d, _addr_p.T);
            FeAdd(_addr_t0, _addr_p.Z, _addr_p.Z);
            FeSub(_addr_r.X, _addr_r.Z, _addr_r.Y);
            FeAdd(_addr_r.Y, _addr_r.Z, _addr_r.Y);
            FeAdd(_addr_r.Z, _addr_t0, _addr_r.T);
            FeSub(_addr_r.T, _addr_t0, _addr_r.T);
        }

        private static void geMixedSub(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<PreComputedGroupElement> _addr_q)
        {
            ref CompletedGroupElement r = ref _addr_r.val;
            ref ExtendedGroupElement p = ref _addr_p.val;
            ref PreComputedGroupElement q = ref _addr_q.val;

            ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);

            FeAdd(_addr_r.X, _addr_p.Y, _addr_p.X);
            FeSub(_addr_r.Y, _addr_p.Y, _addr_p.X);
            FeMul(_addr_r.Z, _addr_r.X, _addr_q.yMinusX);
            FeMul(_addr_r.Y, _addr_r.Y, _addr_q.yPlusX);
            FeMul(_addr_r.T, _addr_q.xy2d, _addr_p.T);
            FeAdd(_addr_t0, _addr_p.Z, _addr_p.Z);
            FeSub(_addr_r.X, _addr_r.Z, _addr_r.Y);
            FeAdd(_addr_r.Y, _addr_r.Z, _addr_r.Y);
            FeSub(_addr_r.Z, _addr_t0, _addr_r.T);
            FeAdd(_addr_r.T, _addr_t0, _addr_r.T);
        }

        private static void slide(ptr<array<sbyte>> _addr_r, ptr<array<byte>> _addr_a)
        {
            ref array<sbyte> r = ref _addr_r.val;
            ref array<byte> a = ref _addr_a.val;

            {
                var i__prev1 = i;

                foreach (var (__i) in r)
                {
                    i = __i;
                    r[i] = int8(1L & (a[i >> (int)(3L)] >> (int)(uint(i & 7L))));
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in r)
                {
                    i = __i;
                    if (r[i] != 0L)
                    {
                        for (long b = 1L; b <= 6L && i + b < 256L; b++)
                        {
                            if (r[i + b] != 0L)
                            {
                                if (r[i] + (r[i + b] << (int)(uint(b))) <= 15L)
                                {
                                    r[i] += r[i + b] << (int)(uint(b));
                                    r[i + b] = 0L;
                                }
                                else if (r[i] - (r[i + b] << (int)(uint(b))) >= -15L)
                                {
                                    r[i] -= r[i + b] << (int)(uint(b));
                                    for (var k = i + b; k < 256L; k++)
                                    {
                                        if (r[k] == 0L)
                                        {
                                            r[k] = 1L;
                                            break;
                                        }

                                        r[k] = 0L;

                                    }
                                else


                                }                                {
                                    break;
                                }

                            }

                        }


                    }

                }

                i = i__prev1;
            }
        }

        // GeDoubleScalarMultVartime sets r = a*A + b*B
        // where a = a[0]+256*a[1]+...+256^31 a[31].
        // and b = b[0]+256*b[1]+...+256^31 b[31].
        // B is the Ed25519 base point (x,4/5) with x positive.
        public static void GeDoubleScalarMultVartime(ptr<ProjectiveGroupElement> _addr_r, ptr<array<byte>> _addr_a, ptr<ExtendedGroupElement> _addr_A, ptr<array<byte>> _addr_b)
        {
            ref ProjectiveGroupElement r = ref _addr_r.val;
            ref array<byte> a = ref _addr_a.val;
            ref ExtendedGroupElement A = ref _addr_A.val;
            ref array<byte> b = ref _addr_b.val;

            ref array<sbyte> aSlide = ref heap(new array<sbyte>(256L), out ptr<array<sbyte>> _addr_aSlide);            ref array<sbyte> bSlide = ref heap(new array<sbyte>(256L), out ptr<array<sbyte>> _addr_bSlide);

            array<CachedGroupElement> Ai = new array<CachedGroupElement>(8L); // A,3A,5A,7A,9A,11A,13A,15A
            ref CompletedGroupElement t = ref heap(out ptr<CompletedGroupElement> _addr_t);
            ref ExtendedGroupElement u = ref heap(out ptr<ExtendedGroupElement> _addr_u);            ref ExtendedGroupElement A2 = ref heap(out ptr<ExtendedGroupElement> _addr_A2);

            long i = default;

            slide(_addr_aSlide, _addr_a);
            slide(_addr_bSlide, _addr_b);

            A.ToCached(_addr_Ai[0L]);
            A.Double(_addr_t);
            t.ToExtended(_addr_A2);

            {
                long i__prev1 = i;

                for (i = 0L; i < 7L; i++)
                {
                    geAdd(_addr_t, _addr_A2, _addr_Ai[i]);
                    t.ToExtended(_addr_u);
                    u.ToCached(_addr_Ai[i + 1L]);
                }


                i = i__prev1;
            }

            r.Zero();

            for (i = 255L; i >= 0L; i--)
            {
                if (aSlide[i] != 0L || bSlide[i] != 0L)
                {
                    break;
                }

            }


            while (i >= 0L)
            {
                r.Double(_addr_t);

                if (aSlide[i] > 0L)
                {
                    t.ToExtended(_addr_u);
                    geAdd(_addr_t, _addr_u, _addr_Ai[aSlide[i] / 2L]);
                i--;
                }
                else if (aSlide[i] < 0L)
                {
                    t.ToExtended(_addr_u);
                    geSub(_addr_t, _addr_u, _addr_Ai[(-aSlide[i]) / 2L]);
                }

                if (bSlide[i] > 0L)
                {
                    t.ToExtended(_addr_u);
                    geMixedAdd(_addr_t, _addr_u, _addr_bi[bSlide[i] / 2L]);
                }
                else if (bSlide[i] < 0L)
                {
                    t.ToExtended(_addr_u);
                    geMixedSub(_addr_t, _addr_u, _addr_bi[(-bSlide[i]) / 2L]);
                }

                t.ToProjective(r);

            }


        }

        // equal returns 1 if b == c and 0 otherwise, assuming that b and c are
        // non-negative.
        private static int equal(int b, int c)
        {
            var x = uint32(b ^ c);
            x--;
            return int32(x >> (int)(31L));
        }

        // negative returns 1 if b < 0 and 0 otherwise.
        private static int negative(int b)
        {
            return (b >> (int)(31L)) & 1L;
        }

        public static void PreComputedGroupElementCMove(ptr<PreComputedGroupElement> _addr_t, ptr<PreComputedGroupElement> _addr_u, int b)
        {
            ref PreComputedGroupElement t = ref _addr_t.val;
            ref PreComputedGroupElement u = ref _addr_u.val;

            FeCMove(_addr_t.yPlusX, _addr_u.yPlusX, b);
            FeCMove(_addr_t.yMinusX, _addr_u.yMinusX, b);
            FeCMove(_addr_t.xy2d, _addr_u.xy2d, b);
        }

        private static void selectPoint(ptr<PreComputedGroupElement> _addr_t, int pos, int b)
        {
            ref PreComputedGroupElement t = ref _addr_t.val;

            ref PreComputedGroupElement minusT = ref heap(out ptr<PreComputedGroupElement> _addr_minusT);
            var bNegative = negative(b);
            var bAbs = b - (((-bNegative) & b) << (int)(1L));

            t.Zero();
            for (var i = int32(0L); i < 8L; i++)
            {
                PreComputedGroupElementCMove(_addr_t, _addr_base[pos][i], equal(bAbs, i + 1L));
            }

            FeCopy(_addr_minusT.yPlusX, _addr_t.yMinusX);
            FeCopy(_addr_minusT.yMinusX, _addr_t.yPlusX);
            FeNeg(_addr_minusT.xy2d, _addr_t.xy2d);
            PreComputedGroupElementCMove(_addr_t, _addr_minusT, bNegative);

        }

        // GeScalarMultBase computes h = a*B, where
        //   a = a[0]+256*a[1]+...+256^31 a[31]
        //   B is the Ed25519 base point (x,4/5) with x positive.
        //
        // Preconditions:
        //   a[31] <= 127
        public static void GeScalarMultBase(ptr<ExtendedGroupElement> _addr_h, ptr<array<byte>> _addr_a)
        {
            ref ExtendedGroupElement h = ref _addr_h.val;
            ref array<byte> a = ref _addr_a.val;

            array<sbyte> e = new array<sbyte>(64L);

            {
                var i__prev1 = i;

                foreach (var (__i, __v) in a)
                {
                    i = __i;
                    v = __v;
                    e[2L * i] = int8(v & 15L);
                    e[2L * i + 1L] = int8((v >> (int)(4L)) & 15L);
                } 

                // each e[i] is between 0 and 15 and e[63] is between 0 and 7.

                i = i__prev1;
            }

            var carry = int8(0L);
            {
                var i__prev1 = i;

                for (long i = 0L; i < 63L; i++)
                {
                    e[i] += carry;
                    carry = (e[i] + 8L) >> (int)(4L);
                    e[i] -= carry << (int)(4L);
                }


                i = i__prev1;
            }
            e[63L] += carry; 
            // each e[i] is between -8 and 8.

            h.Zero();
            ref PreComputedGroupElement t = ref heap(out ptr<PreComputedGroupElement> _addr_t);
            ref CompletedGroupElement r = ref heap(out ptr<CompletedGroupElement> _addr_r);
            {
                var i__prev1 = i;

                i = int32(1L);

                while (i < 64L)
                {
                    selectPoint(_addr_t, i / 2L, int32(e[i]));
                    geMixedAdd(_addr_r, _addr_h, _addr_t);
                    r.ToExtended(h);
                    i += 2L;
                }


                i = i__prev1;
            }

            ref ProjectiveGroupElement s = ref heap(out ptr<ProjectiveGroupElement> _addr_s);

            h.Double(_addr_r);
            r.ToProjective(_addr_s);
            s.Double(_addr_r);
            r.ToProjective(_addr_s);
            s.Double(_addr_r);
            r.ToProjective(_addr_s);
            s.Double(_addr_r);
            r.ToExtended(h);

            {
                var i__prev1 = i;

                i = int32(0L);

                while (i < 64L)
                {
                    selectPoint(_addr_t, i / 2L, int32(e[i]));
                    geMixedAdd(_addr_r, _addr_h, _addr_t);
                    r.ToExtended(h);
                    i += 2L;
                }


                i = i__prev1;
            }

        }

        // The scalars are GF(2^252 + 27742317777372353535851937790883648493).

        // Input:
        //   a[0]+256*a[1]+...+256^31*a[31] = a
        //   b[0]+256*b[1]+...+256^31*b[31] = b
        //   c[0]+256*c[1]+...+256^31*c[31] = c
        //
        // Output:
        //   s[0]+256*s[1]+...+256^31*s[31] = (ab+c) mod l
        //   where l = 2^252 + 27742317777372353535851937790883648493.
        public static void ScMulAdd(ptr<array<byte>> _addr_s, ptr<array<byte>> _addr_a, ptr<array<byte>> _addr_b, ptr<array<byte>> _addr_c)
        {
            ref array<byte> s = ref _addr_s.val;
            ref array<byte> a = ref _addr_a.val;
            ref array<byte> b = ref _addr_b.val;
            ref array<byte> c = ref _addr_c.val;

            long a0 = 2097151L & load3(a[..]);
            long a1 = 2097151L & (load4(a[2L..]) >> (int)(5L));
            long a2 = 2097151L & (load3(a[5L..]) >> (int)(2L));
            long a3 = 2097151L & (load4(a[7L..]) >> (int)(7L));
            long a4 = 2097151L & (load4(a[10L..]) >> (int)(4L));
            long a5 = 2097151L & (load3(a[13L..]) >> (int)(1L));
            long a6 = 2097151L & (load4(a[15L..]) >> (int)(6L));
            long a7 = 2097151L & (load3(a[18L..]) >> (int)(3L));
            long a8 = 2097151L & load3(a[21L..]);
            long a9 = 2097151L & (load4(a[23L..]) >> (int)(5L));
            long a10 = 2097151L & (load3(a[26L..]) >> (int)(2L));
            var a11 = (load4(a[28L..]) >> (int)(7L));
            long b0 = 2097151L & load3(b[..]);
            long b1 = 2097151L & (load4(b[2L..]) >> (int)(5L));
            long b2 = 2097151L & (load3(b[5L..]) >> (int)(2L));
            long b3 = 2097151L & (load4(b[7L..]) >> (int)(7L));
            long b4 = 2097151L & (load4(b[10L..]) >> (int)(4L));
            long b5 = 2097151L & (load3(b[13L..]) >> (int)(1L));
            long b6 = 2097151L & (load4(b[15L..]) >> (int)(6L));
            long b7 = 2097151L & (load3(b[18L..]) >> (int)(3L));
            long b8 = 2097151L & load3(b[21L..]);
            long b9 = 2097151L & (load4(b[23L..]) >> (int)(5L));
            long b10 = 2097151L & (load3(b[26L..]) >> (int)(2L));
            var b11 = (load4(b[28L..]) >> (int)(7L));
            long c0 = 2097151L & load3(c[..]);
            long c1 = 2097151L & (load4(c[2L..]) >> (int)(5L));
            long c2 = 2097151L & (load3(c[5L..]) >> (int)(2L));
            long c3 = 2097151L & (load4(c[7L..]) >> (int)(7L));
            long c4 = 2097151L & (load4(c[10L..]) >> (int)(4L));
            long c5 = 2097151L & (load3(c[13L..]) >> (int)(1L));
            long c6 = 2097151L & (load4(c[15L..]) >> (int)(6L));
            long c7 = 2097151L & (load3(c[18L..]) >> (int)(3L));
            long c8 = 2097151L & load3(c[21L..]);
            long c9 = 2097151L & (load4(c[23L..]) >> (int)(5L));
            long c10 = 2097151L & (load3(c[26L..]) >> (int)(2L));
            var c11 = (load4(c[28L..]) >> (int)(7L));
            array<long> carry = new array<long>(23L);

            var s0 = c0 + a0 * b0;
            var s1 = c1 + a0 * b1 + a1 * b0;
            var s2 = c2 + a0 * b2 + a1 * b1 + a2 * b0;
            var s3 = c3 + a0 * b3 + a1 * b2 + a2 * b1 + a3 * b0;
            var s4 = c4 + a0 * b4 + a1 * b3 + a2 * b2 + a3 * b1 + a4 * b0;
            var s5 = c5 + a0 * b5 + a1 * b4 + a2 * b3 + a3 * b2 + a4 * b1 + a5 * b0;
            var s6 = c6 + a0 * b6 + a1 * b5 + a2 * b4 + a3 * b3 + a4 * b2 + a5 * b1 + a6 * b0;
            var s7 = c7 + a0 * b7 + a1 * b6 + a2 * b5 + a3 * b4 + a4 * b3 + a5 * b2 + a6 * b1 + a7 * b0;
            var s8 = c8 + a0 * b8 + a1 * b7 + a2 * b6 + a3 * b5 + a4 * b4 + a5 * b3 + a6 * b2 + a7 * b1 + a8 * b0;
            var s9 = c9 + a0 * b9 + a1 * b8 + a2 * b7 + a3 * b6 + a4 * b5 + a5 * b4 + a6 * b3 + a7 * b2 + a8 * b1 + a9 * b0;
            var s10 = c10 + a0 * b10 + a1 * b9 + a2 * b8 + a3 * b7 + a4 * b6 + a5 * b5 + a6 * b4 + a7 * b3 + a8 * b2 + a9 * b1 + a10 * b0;
            var s11 = c11 + a0 * b11 + a1 * b10 + a2 * b9 + a3 * b8 + a4 * b7 + a5 * b6 + a6 * b5 + a7 * b4 + a8 * b3 + a9 * b2 + a10 * b1 + a11 * b0;
            var s12 = a1 * b11 + a2 * b10 + a3 * b9 + a4 * b8 + a5 * b7 + a6 * b6 + a7 * b5 + a8 * b4 + a9 * b3 + a10 * b2 + a11 * b1;
            var s13 = a2 * b11 + a3 * b10 + a4 * b9 + a5 * b8 + a6 * b7 + a7 * b6 + a8 * b5 + a9 * b4 + a10 * b3 + a11 * b2;
            var s14 = a3 * b11 + a4 * b10 + a5 * b9 + a6 * b8 + a7 * b7 + a8 * b6 + a9 * b5 + a10 * b4 + a11 * b3;
            var s15 = a4 * b11 + a5 * b10 + a6 * b9 + a7 * b8 + a8 * b7 + a9 * b6 + a10 * b5 + a11 * b4;
            var s16 = a5 * b11 + a6 * b10 + a7 * b9 + a8 * b8 + a9 * b7 + a10 * b6 + a11 * b5;
            var s17 = a6 * b11 + a7 * b10 + a8 * b9 + a9 * b8 + a10 * b7 + a11 * b6;
            var s18 = a7 * b11 + a8 * b10 + a9 * b9 + a10 * b8 + a11 * b7;
            var s19 = a8 * b11 + a9 * b10 + a10 * b9 + a11 * b8;
            var s20 = a9 * b11 + a10 * b10 + a11 * b9;
            var s21 = a10 * b11 + a11 * b10;
            var s22 = a11 * b11;
            var s23 = int64(0L);

            carry[0L] = (s0 + (1L << (int)(20L))) >> (int)(21L);
            s1 += carry[0L];
            s0 -= carry[0L] << (int)(21L);
            carry[2L] = (s2 + (1L << (int)(20L))) >> (int)(21L);
            s3 += carry[2L];
            s2 -= carry[2L] << (int)(21L);
            carry[4L] = (s4 + (1L << (int)(20L))) >> (int)(21L);
            s5 += carry[4L];
            s4 -= carry[4L] << (int)(21L);
            carry[6L] = (s6 + (1L << (int)(20L))) >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[8L] = (s8 + (1L << (int)(20L))) >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[10L] = (s10 + (1L << (int)(20L))) >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);
            carry[12L] = (s12 + (1L << (int)(20L))) >> (int)(21L);
            s13 += carry[12L];
            s12 -= carry[12L] << (int)(21L);
            carry[14L] = (s14 + (1L << (int)(20L))) >> (int)(21L);
            s15 += carry[14L];
            s14 -= carry[14L] << (int)(21L);
            carry[16L] = (s16 + (1L << (int)(20L))) >> (int)(21L);
            s17 += carry[16L];
            s16 -= carry[16L] << (int)(21L);
            carry[18L] = (s18 + (1L << (int)(20L))) >> (int)(21L);
            s19 += carry[18L];
            s18 -= carry[18L] << (int)(21L);
            carry[20L] = (s20 + (1L << (int)(20L))) >> (int)(21L);
            s21 += carry[20L];
            s20 -= carry[20L] << (int)(21L);
            carry[22L] = (s22 + (1L << (int)(20L))) >> (int)(21L);
            s23 += carry[22L];
            s22 -= carry[22L] << (int)(21L);

            carry[1L] = (s1 + (1L << (int)(20L))) >> (int)(21L);
            s2 += carry[1L];
            s1 -= carry[1L] << (int)(21L);
            carry[3L] = (s3 + (1L << (int)(20L))) >> (int)(21L);
            s4 += carry[3L];
            s3 -= carry[3L] << (int)(21L);
            carry[5L] = (s5 + (1L << (int)(20L))) >> (int)(21L);
            s6 += carry[5L];
            s5 -= carry[5L] << (int)(21L);
            carry[7L] = (s7 + (1L << (int)(20L))) >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[9L] = (s9 + (1L << (int)(20L))) >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[11L] = (s11 + (1L << (int)(20L))) >> (int)(21L);
            s12 += carry[11L];
            s11 -= carry[11L] << (int)(21L);
            carry[13L] = (s13 + (1L << (int)(20L))) >> (int)(21L);
            s14 += carry[13L];
            s13 -= carry[13L] << (int)(21L);
            carry[15L] = (s15 + (1L << (int)(20L))) >> (int)(21L);
            s16 += carry[15L];
            s15 -= carry[15L] << (int)(21L);
            carry[17L] = (s17 + (1L << (int)(20L))) >> (int)(21L);
            s18 += carry[17L];
            s17 -= carry[17L] << (int)(21L);
            carry[19L] = (s19 + (1L << (int)(20L))) >> (int)(21L);
            s20 += carry[19L];
            s19 -= carry[19L] << (int)(21L);
            carry[21L] = (s21 + (1L << (int)(20L))) >> (int)(21L);
            s22 += carry[21L];
            s21 -= carry[21L] << (int)(21L);

            s11 += s23 * 666643L;
            s12 += s23 * 470296L;
            s13 += s23 * 654183L;
            s14 -= s23 * 997805L;
            s15 += s23 * 136657L;
            s16 -= s23 * 683901L;
            s23 = 0L;

            s10 += s22 * 666643L;
            s11 += s22 * 470296L;
            s12 += s22 * 654183L;
            s13 -= s22 * 997805L;
            s14 += s22 * 136657L;
            s15 -= s22 * 683901L;
            s22 = 0L;

            s9 += s21 * 666643L;
            s10 += s21 * 470296L;
            s11 += s21 * 654183L;
            s12 -= s21 * 997805L;
            s13 += s21 * 136657L;
            s14 -= s21 * 683901L;
            s21 = 0L;

            s8 += s20 * 666643L;
            s9 += s20 * 470296L;
            s10 += s20 * 654183L;
            s11 -= s20 * 997805L;
            s12 += s20 * 136657L;
            s13 -= s20 * 683901L;
            s20 = 0L;

            s7 += s19 * 666643L;
            s8 += s19 * 470296L;
            s9 += s19 * 654183L;
            s10 -= s19 * 997805L;
            s11 += s19 * 136657L;
            s12 -= s19 * 683901L;
            s19 = 0L;

            s6 += s18 * 666643L;
            s7 += s18 * 470296L;
            s8 += s18 * 654183L;
            s9 -= s18 * 997805L;
            s10 += s18 * 136657L;
            s11 -= s18 * 683901L;
            s18 = 0L;

            carry[6L] = (s6 + (1L << (int)(20L))) >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[8L] = (s8 + (1L << (int)(20L))) >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[10L] = (s10 + (1L << (int)(20L))) >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);
            carry[12L] = (s12 + (1L << (int)(20L))) >> (int)(21L);
            s13 += carry[12L];
            s12 -= carry[12L] << (int)(21L);
            carry[14L] = (s14 + (1L << (int)(20L))) >> (int)(21L);
            s15 += carry[14L];
            s14 -= carry[14L] << (int)(21L);
            carry[16L] = (s16 + (1L << (int)(20L))) >> (int)(21L);
            s17 += carry[16L];
            s16 -= carry[16L] << (int)(21L);

            carry[7L] = (s7 + (1L << (int)(20L))) >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[9L] = (s9 + (1L << (int)(20L))) >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[11L] = (s11 + (1L << (int)(20L))) >> (int)(21L);
            s12 += carry[11L];
            s11 -= carry[11L] << (int)(21L);
            carry[13L] = (s13 + (1L << (int)(20L))) >> (int)(21L);
            s14 += carry[13L];
            s13 -= carry[13L] << (int)(21L);
            carry[15L] = (s15 + (1L << (int)(20L))) >> (int)(21L);
            s16 += carry[15L];
            s15 -= carry[15L] << (int)(21L);

            s5 += s17 * 666643L;
            s6 += s17 * 470296L;
            s7 += s17 * 654183L;
            s8 -= s17 * 997805L;
            s9 += s17 * 136657L;
            s10 -= s17 * 683901L;
            s17 = 0L;

            s4 += s16 * 666643L;
            s5 += s16 * 470296L;
            s6 += s16 * 654183L;
            s7 -= s16 * 997805L;
            s8 += s16 * 136657L;
            s9 -= s16 * 683901L;
            s16 = 0L;

            s3 += s15 * 666643L;
            s4 += s15 * 470296L;
            s5 += s15 * 654183L;
            s6 -= s15 * 997805L;
            s7 += s15 * 136657L;
            s8 -= s15 * 683901L;
            s15 = 0L;

            s2 += s14 * 666643L;
            s3 += s14 * 470296L;
            s4 += s14 * 654183L;
            s5 -= s14 * 997805L;
            s6 += s14 * 136657L;
            s7 -= s14 * 683901L;
            s14 = 0L;

            s1 += s13 * 666643L;
            s2 += s13 * 470296L;
            s3 += s13 * 654183L;
            s4 -= s13 * 997805L;
            s5 += s13 * 136657L;
            s6 -= s13 * 683901L;
            s13 = 0L;

            s0 += s12 * 666643L;
            s1 += s12 * 470296L;
            s2 += s12 * 654183L;
            s3 -= s12 * 997805L;
            s4 += s12 * 136657L;
            s5 -= s12 * 683901L;
            s12 = 0L;

            carry[0L] = (s0 + (1L << (int)(20L))) >> (int)(21L);
            s1 += carry[0L];
            s0 -= carry[0L] << (int)(21L);
            carry[2L] = (s2 + (1L << (int)(20L))) >> (int)(21L);
            s3 += carry[2L];
            s2 -= carry[2L] << (int)(21L);
            carry[4L] = (s4 + (1L << (int)(20L))) >> (int)(21L);
            s5 += carry[4L];
            s4 -= carry[4L] << (int)(21L);
            carry[6L] = (s6 + (1L << (int)(20L))) >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[8L] = (s8 + (1L << (int)(20L))) >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[10L] = (s10 + (1L << (int)(20L))) >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);

            carry[1L] = (s1 + (1L << (int)(20L))) >> (int)(21L);
            s2 += carry[1L];
            s1 -= carry[1L] << (int)(21L);
            carry[3L] = (s3 + (1L << (int)(20L))) >> (int)(21L);
            s4 += carry[3L];
            s3 -= carry[3L] << (int)(21L);
            carry[5L] = (s5 + (1L << (int)(20L))) >> (int)(21L);
            s6 += carry[5L];
            s5 -= carry[5L] << (int)(21L);
            carry[7L] = (s7 + (1L << (int)(20L))) >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[9L] = (s9 + (1L << (int)(20L))) >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[11L] = (s11 + (1L << (int)(20L))) >> (int)(21L);
            s12 += carry[11L];
            s11 -= carry[11L] << (int)(21L);

            s0 += s12 * 666643L;
            s1 += s12 * 470296L;
            s2 += s12 * 654183L;
            s3 -= s12 * 997805L;
            s4 += s12 * 136657L;
            s5 -= s12 * 683901L;
            s12 = 0L;

            carry[0L] = s0 >> (int)(21L);
            s1 += carry[0L];
            s0 -= carry[0L] << (int)(21L);
            carry[1L] = s1 >> (int)(21L);
            s2 += carry[1L];
            s1 -= carry[1L] << (int)(21L);
            carry[2L] = s2 >> (int)(21L);
            s3 += carry[2L];
            s2 -= carry[2L] << (int)(21L);
            carry[3L] = s3 >> (int)(21L);
            s4 += carry[3L];
            s3 -= carry[3L] << (int)(21L);
            carry[4L] = s4 >> (int)(21L);
            s5 += carry[4L];
            s4 -= carry[4L] << (int)(21L);
            carry[5L] = s5 >> (int)(21L);
            s6 += carry[5L];
            s5 -= carry[5L] << (int)(21L);
            carry[6L] = s6 >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[7L] = s7 >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[8L] = s8 >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[9L] = s9 >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[10L] = s10 >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);
            carry[11L] = s11 >> (int)(21L);
            s12 += carry[11L];
            s11 -= carry[11L] << (int)(21L);

            s0 += s12 * 666643L;
            s1 += s12 * 470296L;
            s2 += s12 * 654183L;
            s3 -= s12 * 997805L;
            s4 += s12 * 136657L;
            s5 -= s12 * 683901L;
            s12 = 0L;

            carry[0L] = s0 >> (int)(21L);
            s1 += carry[0L];
            s0 -= carry[0L] << (int)(21L);
            carry[1L] = s1 >> (int)(21L);
            s2 += carry[1L];
            s1 -= carry[1L] << (int)(21L);
            carry[2L] = s2 >> (int)(21L);
            s3 += carry[2L];
            s2 -= carry[2L] << (int)(21L);
            carry[3L] = s3 >> (int)(21L);
            s4 += carry[3L];
            s3 -= carry[3L] << (int)(21L);
            carry[4L] = s4 >> (int)(21L);
            s5 += carry[4L];
            s4 -= carry[4L] << (int)(21L);
            carry[5L] = s5 >> (int)(21L);
            s6 += carry[5L];
            s5 -= carry[5L] << (int)(21L);
            carry[6L] = s6 >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[7L] = s7 >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[8L] = s8 >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[9L] = s9 >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[10L] = s10 >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);

            s[0L] = byte(s0 >> (int)(0L));
            s[1L] = byte(s0 >> (int)(8L));
            s[2L] = byte((s0 >> (int)(16L)) | (s1 << (int)(5L)));
            s[3L] = byte(s1 >> (int)(3L));
            s[4L] = byte(s1 >> (int)(11L));
            s[5L] = byte((s1 >> (int)(19L)) | (s2 << (int)(2L)));
            s[6L] = byte(s2 >> (int)(6L));
            s[7L] = byte((s2 >> (int)(14L)) | (s3 << (int)(7L)));
            s[8L] = byte(s3 >> (int)(1L));
            s[9L] = byte(s3 >> (int)(9L));
            s[10L] = byte((s3 >> (int)(17L)) | (s4 << (int)(4L)));
            s[11L] = byte(s4 >> (int)(4L));
            s[12L] = byte(s4 >> (int)(12L));
            s[13L] = byte((s4 >> (int)(20L)) | (s5 << (int)(1L)));
            s[14L] = byte(s5 >> (int)(7L));
            s[15L] = byte((s5 >> (int)(15L)) | (s6 << (int)(6L)));
            s[16L] = byte(s6 >> (int)(2L));
            s[17L] = byte(s6 >> (int)(10L));
            s[18L] = byte((s6 >> (int)(18L)) | (s7 << (int)(3L)));
            s[19L] = byte(s7 >> (int)(5L));
            s[20L] = byte(s7 >> (int)(13L));
            s[21L] = byte(s8 >> (int)(0L));
            s[22L] = byte(s8 >> (int)(8L));
            s[23L] = byte((s8 >> (int)(16L)) | (s9 << (int)(5L)));
            s[24L] = byte(s9 >> (int)(3L));
            s[25L] = byte(s9 >> (int)(11L));
            s[26L] = byte((s9 >> (int)(19L)) | (s10 << (int)(2L)));
            s[27L] = byte(s10 >> (int)(6L));
            s[28L] = byte((s10 >> (int)(14L)) | (s11 << (int)(7L)));
            s[29L] = byte(s11 >> (int)(1L));
            s[30L] = byte(s11 >> (int)(9L));
            s[31L] = byte(s11 >> (int)(17L));
        }

        // Input:
        //   s[0]+256*s[1]+...+256^63*s[63] = s
        //
        // Output:
        //   s[0]+256*s[1]+...+256^31*s[31] = s mod l
        //   where l = 2^252 + 27742317777372353535851937790883648493.
        public static void ScReduce(ptr<array<byte>> _addr_@out, ptr<array<byte>> _addr_s)
        {
            ref array<byte> @out = ref _addr_@out.val;
            ref array<byte> s = ref _addr_s.val;

            long s0 = 2097151L & load3(s[..]);
            long s1 = 2097151L & (load4(s[2L..]) >> (int)(5L));
            long s2 = 2097151L & (load3(s[5L..]) >> (int)(2L));
            long s3 = 2097151L & (load4(s[7L..]) >> (int)(7L));
            long s4 = 2097151L & (load4(s[10L..]) >> (int)(4L));
            long s5 = 2097151L & (load3(s[13L..]) >> (int)(1L));
            long s6 = 2097151L & (load4(s[15L..]) >> (int)(6L));
            long s7 = 2097151L & (load3(s[18L..]) >> (int)(3L));
            long s8 = 2097151L & load3(s[21L..]);
            long s9 = 2097151L & (load4(s[23L..]) >> (int)(5L));
            long s10 = 2097151L & (load3(s[26L..]) >> (int)(2L));
            long s11 = 2097151L & (load4(s[28L..]) >> (int)(7L));
            long s12 = 2097151L & (load4(s[31L..]) >> (int)(4L));
            long s13 = 2097151L & (load3(s[34L..]) >> (int)(1L));
            long s14 = 2097151L & (load4(s[36L..]) >> (int)(6L));
            long s15 = 2097151L & (load3(s[39L..]) >> (int)(3L));
            long s16 = 2097151L & load3(s[42L..]);
            long s17 = 2097151L & (load4(s[44L..]) >> (int)(5L));
            long s18 = 2097151L & (load3(s[47L..]) >> (int)(2L));
            long s19 = 2097151L & (load4(s[49L..]) >> (int)(7L));
            long s20 = 2097151L & (load4(s[52L..]) >> (int)(4L));
            long s21 = 2097151L & (load3(s[55L..]) >> (int)(1L));
            long s22 = 2097151L & (load4(s[57L..]) >> (int)(6L));
            var s23 = (load4(s[60L..]) >> (int)(3L));

            s11 += s23 * 666643L;
            s12 += s23 * 470296L;
            s13 += s23 * 654183L;
            s14 -= s23 * 997805L;
            s15 += s23 * 136657L;
            s16 -= s23 * 683901L;
            s23 = 0L;

            s10 += s22 * 666643L;
            s11 += s22 * 470296L;
            s12 += s22 * 654183L;
            s13 -= s22 * 997805L;
            s14 += s22 * 136657L;
            s15 -= s22 * 683901L;
            s22 = 0L;

            s9 += s21 * 666643L;
            s10 += s21 * 470296L;
            s11 += s21 * 654183L;
            s12 -= s21 * 997805L;
            s13 += s21 * 136657L;
            s14 -= s21 * 683901L;
            s21 = 0L;

            s8 += s20 * 666643L;
            s9 += s20 * 470296L;
            s10 += s20 * 654183L;
            s11 -= s20 * 997805L;
            s12 += s20 * 136657L;
            s13 -= s20 * 683901L;
            s20 = 0L;

            s7 += s19 * 666643L;
            s8 += s19 * 470296L;
            s9 += s19 * 654183L;
            s10 -= s19 * 997805L;
            s11 += s19 * 136657L;
            s12 -= s19 * 683901L;
            s19 = 0L;

            s6 += s18 * 666643L;
            s7 += s18 * 470296L;
            s8 += s18 * 654183L;
            s9 -= s18 * 997805L;
            s10 += s18 * 136657L;
            s11 -= s18 * 683901L;
            s18 = 0L;

            array<long> carry = new array<long>(17L);

            carry[6L] = (s6 + (1L << (int)(20L))) >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[8L] = (s8 + (1L << (int)(20L))) >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[10L] = (s10 + (1L << (int)(20L))) >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);
            carry[12L] = (s12 + (1L << (int)(20L))) >> (int)(21L);
            s13 += carry[12L];
            s12 -= carry[12L] << (int)(21L);
            carry[14L] = (s14 + (1L << (int)(20L))) >> (int)(21L);
            s15 += carry[14L];
            s14 -= carry[14L] << (int)(21L);
            carry[16L] = (s16 + (1L << (int)(20L))) >> (int)(21L);
            s17 += carry[16L];
            s16 -= carry[16L] << (int)(21L);

            carry[7L] = (s7 + (1L << (int)(20L))) >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[9L] = (s9 + (1L << (int)(20L))) >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[11L] = (s11 + (1L << (int)(20L))) >> (int)(21L);
            s12 += carry[11L];
            s11 -= carry[11L] << (int)(21L);
            carry[13L] = (s13 + (1L << (int)(20L))) >> (int)(21L);
            s14 += carry[13L];
            s13 -= carry[13L] << (int)(21L);
            carry[15L] = (s15 + (1L << (int)(20L))) >> (int)(21L);
            s16 += carry[15L];
            s15 -= carry[15L] << (int)(21L);

            s5 += s17 * 666643L;
            s6 += s17 * 470296L;
            s7 += s17 * 654183L;
            s8 -= s17 * 997805L;
            s9 += s17 * 136657L;
            s10 -= s17 * 683901L;
            s17 = 0L;

            s4 += s16 * 666643L;
            s5 += s16 * 470296L;
            s6 += s16 * 654183L;
            s7 -= s16 * 997805L;
            s8 += s16 * 136657L;
            s9 -= s16 * 683901L;
            s16 = 0L;

            s3 += s15 * 666643L;
            s4 += s15 * 470296L;
            s5 += s15 * 654183L;
            s6 -= s15 * 997805L;
            s7 += s15 * 136657L;
            s8 -= s15 * 683901L;
            s15 = 0L;

            s2 += s14 * 666643L;
            s3 += s14 * 470296L;
            s4 += s14 * 654183L;
            s5 -= s14 * 997805L;
            s6 += s14 * 136657L;
            s7 -= s14 * 683901L;
            s14 = 0L;

            s1 += s13 * 666643L;
            s2 += s13 * 470296L;
            s3 += s13 * 654183L;
            s4 -= s13 * 997805L;
            s5 += s13 * 136657L;
            s6 -= s13 * 683901L;
            s13 = 0L;

            s0 += s12 * 666643L;
            s1 += s12 * 470296L;
            s2 += s12 * 654183L;
            s3 -= s12 * 997805L;
            s4 += s12 * 136657L;
            s5 -= s12 * 683901L;
            s12 = 0L;

            carry[0L] = (s0 + (1L << (int)(20L))) >> (int)(21L);
            s1 += carry[0L];
            s0 -= carry[0L] << (int)(21L);
            carry[2L] = (s2 + (1L << (int)(20L))) >> (int)(21L);
            s3 += carry[2L];
            s2 -= carry[2L] << (int)(21L);
            carry[4L] = (s4 + (1L << (int)(20L))) >> (int)(21L);
            s5 += carry[4L];
            s4 -= carry[4L] << (int)(21L);
            carry[6L] = (s6 + (1L << (int)(20L))) >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[8L] = (s8 + (1L << (int)(20L))) >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[10L] = (s10 + (1L << (int)(20L))) >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);

            carry[1L] = (s1 + (1L << (int)(20L))) >> (int)(21L);
            s2 += carry[1L];
            s1 -= carry[1L] << (int)(21L);
            carry[3L] = (s3 + (1L << (int)(20L))) >> (int)(21L);
            s4 += carry[3L];
            s3 -= carry[3L] << (int)(21L);
            carry[5L] = (s5 + (1L << (int)(20L))) >> (int)(21L);
            s6 += carry[5L];
            s5 -= carry[5L] << (int)(21L);
            carry[7L] = (s7 + (1L << (int)(20L))) >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[9L] = (s9 + (1L << (int)(20L))) >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[11L] = (s11 + (1L << (int)(20L))) >> (int)(21L);
            s12 += carry[11L];
            s11 -= carry[11L] << (int)(21L);

            s0 += s12 * 666643L;
            s1 += s12 * 470296L;
            s2 += s12 * 654183L;
            s3 -= s12 * 997805L;
            s4 += s12 * 136657L;
            s5 -= s12 * 683901L;
            s12 = 0L;

            carry[0L] = s0 >> (int)(21L);
            s1 += carry[0L];
            s0 -= carry[0L] << (int)(21L);
            carry[1L] = s1 >> (int)(21L);
            s2 += carry[1L];
            s1 -= carry[1L] << (int)(21L);
            carry[2L] = s2 >> (int)(21L);
            s3 += carry[2L];
            s2 -= carry[2L] << (int)(21L);
            carry[3L] = s3 >> (int)(21L);
            s4 += carry[3L];
            s3 -= carry[3L] << (int)(21L);
            carry[4L] = s4 >> (int)(21L);
            s5 += carry[4L];
            s4 -= carry[4L] << (int)(21L);
            carry[5L] = s5 >> (int)(21L);
            s6 += carry[5L];
            s5 -= carry[5L] << (int)(21L);
            carry[6L] = s6 >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[7L] = s7 >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[8L] = s8 >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[9L] = s9 >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[10L] = s10 >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);
            carry[11L] = s11 >> (int)(21L);
            s12 += carry[11L];
            s11 -= carry[11L] << (int)(21L);

            s0 += s12 * 666643L;
            s1 += s12 * 470296L;
            s2 += s12 * 654183L;
            s3 -= s12 * 997805L;
            s4 += s12 * 136657L;
            s5 -= s12 * 683901L;
            s12 = 0L;

            carry[0L] = s0 >> (int)(21L);
            s1 += carry[0L];
            s0 -= carry[0L] << (int)(21L);
            carry[1L] = s1 >> (int)(21L);
            s2 += carry[1L];
            s1 -= carry[1L] << (int)(21L);
            carry[2L] = s2 >> (int)(21L);
            s3 += carry[2L];
            s2 -= carry[2L] << (int)(21L);
            carry[3L] = s3 >> (int)(21L);
            s4 += carry[3L];
            s3 -= carry[3L] << (int)(21L);
            carry[4L] = s4 >> (int)(21L);
            s5 += carry[4L];
            s4 -= carry[4L] << (int)(21L);
            carry[5L] = s5 >> (int)(21L);
            s6 += carry[5L];
            s5 -= carry[5L] << (int)(21L);
            carry[6L] = s6 >> (int)(21L);
            s7 += carry[6L];
            s6 -= carry[6L] << (int)(21L);
            carry[7L] = s7 >> (int)(21L);
            s8 += carry[7L];
            s7 -= carry[7L] << (int)(21L);
            carry[8L] = s8 >> (int)(21L);
            s9 += carry[8L];
            s8 -= carry[8L] << (int)(21L);
            carry[9L] = s9 >> (int)(21L);
            s10 += carry[9L];
            s9 -= carry[9L] << (int)(21L);
            carry[10L] = s10 >> (int)(21L);
            s11 += carry[10L];
            s10 -= carry[10L] << (int)(21L);

            out[0L] = byte(s0 >> (int)(0L));
            out[1L] = byte(s0 >> (int)(8L));
            out[2L] = byte((s0 >> (int)(16L)) | (s1 << (int)(5L)));
            out[3L] = byte(s1 >> (int)(3L));
            out[4L] = byte(s1 >> (int)(11L));
            out[5L] = byte((s1 >> (int)(19L)) | (s2 << (int)(2L)));
            out[6L] = byte(s2 >> (int)(6L));
            out[7L] = byte((s2 >> (int)(14L)) | (s3 << (int)(7L)));
            out[8L] = byte(s3 >> (int)(1L));
            out[9L] = byte(s3 >> (int)(9L));
            out[10L] = byte((s3 >> (int)(17L)) | (s4 << (int)(4L)));
            out[11L] = byte(s4 >> (int)(4L));
            out[12L] = byte(s4 >> (int)(12L));
            out[13L] = byte((s4 >> (int)(20L)) | (s5 << (int)(1L)));
            out[14L] = byte(s5 >> (int)(7L));
            out[15L] = byte((s5 >> (int)(15L)) | (s6 << (int)(6L)));
            out[16L] = byte(s6 >> (int)(2L));
            out[17L] = byte(s6 >> (int)(10L));
            out[18L] = byte((s6 >> (int)(18L)) | (s7 << (int)(3L)));
            out[19L] = byte(s7 >> (int)(5L));
            out[20L] = byte(s7 >> (int)(13L));
            out[21L] = byte(s8 >> (int)(0L));
            out[22L] = byte(s8 >> (int)(8L));
            out[23L] = byte((s8 >> (int)(16L)) | (s9 << (int)(5L)));
            out[24L] = byte(s9 >> (int)(3L));
            out[25L] = byte(s9 >> (int)(11L));
            out[26L] = byte((s9 >> (int)(19L)) | (s10 << (int)(2L)));
            out[27L] = byte(s10 >> (int)(6L));
            out[28L] = byte((s10 >> (int)(14L)) | (s11 << (int)(7L)));
            out[29L] = byte(s11 >> (int)(1L));
            out[30L] = byte(s11 >> (int)(9L));
            out[31L] = byte(s11 >> (int)(17L));
        }

        // order is the order of Curve25519 in little-endian form.
        private static array<ulong> order = new array<ulong>(new ulong[] { 0x5812631a5cf5d3ed, 0x14def9dea2f79cd6, 0, 0x1000000000000000 });

        // ScMinimal returns true if the given scalar is less than the order of the
        // curve.
        public static bool ScMinimal(ptr<array<byte>> _addr_scalar)
        {
            ref array<byte> scalar = ref _addr_scalar.val;

            for (long i = 3L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i--)
            {
                var v = binary.LittleEndian.Uint64(scalar[i * 8L..]);
                if (v > order[i])
                {
                    return false;
                }
                else if (v < order[i])
                {
                    break;
                }
                else if (i == 0L)
                {
                    return false;
                }

            }


            return true;

        }
    }
}}}}}}}}
