// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package curve25519 -- go2cs converted at 2020 October 09 06:06:31 UTC
// import "vendor/golang.org/x/crypto/curve25519" ==> using curve25519 = go.vendor.golang.org.x.crypto.curve25519_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\curve25519\curve25519_generic.go
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class curve25519_package
    {
        // This code is a port of the public domain, "ref10" implementation of
        // curve25519 from SUPERCOP 20130419 by D. J. Bernstein.

        // fieldElement represents an element of the field GF(2^255 - 19). An element
        // t, entries t[0]...t[9], represents the integer t[0]+2^26 t[1]+2^51 t[2]+2^77
        // t[3]+2^102 t[4]+...+2^230 t[9]. Bounds on each t[i] vary depending on
        // context.
        private partial struct fieldElement // : array<int>
        {
        }

        private static void feZero(ptr<fieldElement> _addr_fe)
        {
            ref fieldElement fe = ref _addr_fe.val;

            foreach (var (i) in fe)
            {
                fe[i] = 0L;
            }

        }

        private static void feOne(ptr<fieldElement> _addr_fe)
        {
            ref fieldElement fe = ref _addr_fe.val;

            feZero(_addr_fe);
            fe[0L] = 1L;
        }

        private static void feAdd(ptr<fieldElement> _addr_dst, ptr<fieldElement> _addr_a, ptr<fieldElement> _addr_b)
        {
            ref fieldElement dst = ref _addr_dst.val;
            ref fieldElement a = ref _addr_a.val;
            ref fieldElement b = ref _addr_b.val;

            foreach (var (i) in dst)
            {
                dst[i] = a[i] + b[i];
            }

        }

        private static void feSub(ptr<fieldElement> _addr_dst, ptr<fieldElement> _addr_a, ptr<fieldElement> _addr_b)
        {
            ref fieldElement dst = ref _addr_dst.val;
            ref fieldElement a = ref _addr_a.val;
            ref fieldElement b = ref _addr_b.val;

            foreach (var (i) in dst)
            {
                dst[i] = a[i] - b[i];
            }

        }

        private static void feCopy(ptr<fieldElement> _addr_dst, ptr<fieldElement> _addr_src)
        {
            ref fieldElement dst = ref _addr_dst.val;
            ref fieldElement src = ref _addr_src.val;

            foreach (var (i) in dst)
            {
                dst[i] = src[i];
            }

        }

        // feCSwap replaces (f,g) with (g,f) if b == 1; replaces (f,g) with (f,g) if b == 0.
        //
        // Preconditions: b in {0,1}.
        private static void feCSwap(ptr<fieldElement> _addr_f, ptr<fieldElement> _addr_g, int b)
        {
            ref fieldElement f = ref _addr_f.val;
            ref fieldElement g = ref _addr_g.val;

            b = -b;
            foreach (var (i) in f)
            {
                var t = b & (f[i] ^ g[i]);
                f[i] ^= t;
                g[i] ^= t;
            }

        }

        // load3 reads a 24-bit, little-endian value from in.
        private static long load3(slice<byte> @in)
        {
            long r = default;
            r = int64(in[0L]);
            r |= int64(in[1L]) << (int)(8L);
            r |= int64(in[2L]) << (int)(16L);
            return r;
        }

        // load4 reads a 32-bit, little-endian value from in.
        private static long load4(slice<byte> @in)
        {
            return int64(binary.LittleEndian.Uint32(in));
        }

        private static void feFromBytes(ptr<fieldElement> _addr_dst, ptr<array<byte>> _addr_src)
        {
            ref fieldElement dst = ref _addr_dst.val;
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
            var h9 = (load3(src[29L..]) & 0x7fffffUL) << (int)(2L);

            array<long> carry = new array<long>(10L);
            carry[9L] = (h9 + 1L << (int)(24L)) >> (int)(25L);
            h0 += carry[9L] * 19L;
            h9 -= carry[9L] << (int)(25L);
            carry[1L] = (h1 + 1L << (int)(24L)) >> (int)(25L);
            h2 += carry[1L];
            h1 -= carry[1L] << (int)(25L);
            carry[3L] = (h3 + 1L << (int)(24L)) >> (int)(25L);
            h4 += carry[3L];
            h3 -= carry[3L] << (int)(25L);
            carry[5L] = (h5 + 1L << (int)(24L)) >> (int)(25L);
            h6 += carry[5L];
            h5 -= carry[5L] << (int)(25L);
            carry[7L] = (h7 + 1L << (int)(24L)) >> (int)(25L);
            h8 += carry[7L];
            h7 -= carry[7L] << (int)(25L);

            carry[0L] = (h0 + 1L << (int)(25L)) >> (int)(26L);
            h1 += carry[0L];
            h0 -= carry[0L] << (int)(26L);
            carry[2L] = (h2 + 1L << (int)(25L)) >> (int)(26L);
            h3 += carry[2L];
            h2 -= carry[2L] << (int)(26L);
            carry[4L] = (h4 + 1L << (int)(25L)) >> (int)(26L);
            h5 += carry[4L];
            h4 -= carry[4L] << (int)(26L);
            carry[6L] = (h6 + 1L << (int)(25L)) >> (int)(26L);
            h7 += carry[6L];
            h6 -= carry[6L] << (int)(26L);
            carry[8L] = (h8 + 1L << (int)(25L)) >> (int)(26L);
            h9 += carry[8L];
            h8 -= carry[8L] << (int)(26L);

            dst[0L] = int32(h0);
            dst[1L] = int32(h1);
            dst[2L] = int32(h2);
            dst[3L] = int32(h3);
            dst[4L] = int32(h4);
            dst[5L] = int32(h5);
            dst[6L] = int32(h6);
            dst[7L] = int32(h7);
            dst[8L] = int32(h8);
            dst[9L] = int32(h9);
        }

        // feToBytes marshals h to s.
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
        private static void feToBytes(ptr<array<byte>> _addr_s, ptr<fieldElement> _addr_h)
        {
            ref array<byte> s = ref _addr_s.val;
            ref fieldElement h = ref _addr_h.val;

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

        // feMul calculates h = f * g
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
        // With tighter constraints on inputs can squeeze carries into int32.
        private static void feMul(ptr<fieldElement> _addr_h, ptr<fieldElement> _addr_f, ptr<fieldElement> _addr_g)
        {
            ref fieldElement h = ref _addr_h.val;
            ref fieldElement f = ref _addr_f.val;
            ref fieldElement g = ref _addr_g.val;

            var f0 = f[0L];
            var f1 = f[1L];
            var f2 = f[2L];
            var f3 = f[3L];
            var f4 = f[4L];
            var f5 = f[5L];
            var f6 = f[6L];
            var f7 = f[7L];
            var f8 = f[8L];
            var f9 = f[9L];
            var g0 = g[0L];
            var g1 = g[1L];
            var g2 = g[2L];
            var g3 = g[3L];
            var g4 = g[4L];
            var g5 = g[5L];
            var g6 = g[6L];
            var g7 = g[7L];
            var g8 = g[8L];
            var g9 = g[9L];
            long g1_19 = 19L * g1; // 1.4*2^29
            long g2_19 = 19L * g2; // 1.4*2^30; still ok
            long g3_19 = 19L * g3;
            long g4_19 = 19L * g4;
            long g5_19 = 19L * g5;
            long g6_19 = 19L * g6;
            long g7_19 = 19L * g7;
            long g8_19 = 19L * g8;
            long g9_19 = 19L * g9;
            long f1_2 = 2L * f1;
            long f3_2 = 2L * f3;
            long f5_2 = 2L * f5;
            long f7_2 = 2L * f7;
            long f9_2 = 2L * f9;
            var f0g0 = int64(f0) * int64(g0);
            var f0g1 = int64(f0) * int64(g1);
            var f0g2 = int64(f0) * int64(g2);
            var f0g3 = int64(f0) * int64(g3);
            var f0g4 = int64(f0) * int64(g4);
            var f0g5 = int64(f0) * int64(g5);
            var f0g6 = int64(f0) * int64(g6);
            var f0g7 = int64(f0) * int64(g7);
            var f0g8 = int64(f0) * int64(g8);
            var f0g9 = int64(f0) * int64(g9);
            var f1g0 = int64(f1) * int64(g0);
            var f1g1_2 = int64(f1_2) * int64(g1);
            var f1g2 = int64(f1) * int64(g2);
            var f1g3_2 = int64(f1_2) * int64(g3);
            var f1g4 = int64(f1) * int64(g4);
            var f1g5_2 = int64(f1_2) * int64(g5);
            var f1g6 = int64(f1) * int64(g6);
            var f1g7_2 = int64(f1_2) * int64(g7);
            var f1g8 = int64(f1) * int64(g8);
            var f1g9_38 = int64(f1_2) * int64(g9_19);
            var f2g0 = int64(f2) * int64(g0);
            var f2g1 = int64(f2) * int64(g1);
            var f2g2 = int64(f2) * int64(g2);
            var f2g3 = int64(f2) * int64(g3);
            var f2g4 = int64(f2) * int64(g4);
            var f2g5 = int64(f2) * int64(g5);
            var f2g6 = int64(f2) * int64(g6);
            var f2g7 = int64(f2) * int64(g7);
            var f2g8_19 = int64(f2) * int64(g8_19);
            var f2g9_19 = int64(f2) * int64(g9_19);
            var f3g0 = int64(f3) * int64(g0);
            var f3g1_2 = int64(f3_2) * int64(g1);
            var f3g2 = int64(f3) * int64(g2);
            var f3g3_2 = int64(f3_2) * int64(g3);
            var f3g4 = int64(f3) * int64(g4);
            var f3g5_2 = int64(f3_2) * int64(g5);
            var f3g6 = int64(f3) * int64(g6);
            var f3g7_38 = int64(f3_2) * int64(g7_19);
            var f3g8_19 = int64(f3) * int64(g8_19);
            var f3g9_38 = int64(f3_2) * int64(g9_19);
            var f4g0 = int64(f4) * int64(g0);
            var f4g1 = int64(f4) * int64(g1);
            var f4g2 = int64(f4) * int64(g2);
            var f4g3 = int64(f4) * int64(g3);
            var f4g4 = int64(f4) * int64(g4);
            var f4g5 = int64(f4) * int64(g5);
            var f4g6_19 = int64(f4) * int64(g6_19);
            var f4g7_19 = int64(f4) * int64(g7_19);
            var f4g8_19 = int64(f4) * int64(g8_19);
            var f4g9_19 = int64(f4) * int64(g9_19);
            var f5g0 = int64(f5) * int64(g0);
            var f5g1_2 = int64(f5_2) * int64(g1);
            var f5g2 = int64(f5) * int64(g2);
            var f5g3_2 = int64(f5_2) * int64(g3);
            var f5g4 = int64(f5) * int64(g4);
            var f5g5_38 = int64(f5_2) * int64(g5_19);
            var f5g6_19 = int64(f5) * int64(g6_19);
            var f5g7_38 = int64(f5_2) * int64(g7_19);
            var f5g8_19 = int64(f5) * int64(g8_19);
            var f5g9_38 = int64(f5_2) * int64(g9_19);
            var f6g0 = int64(f6) * int64(g0);
            var f6g1 = int64(f6) * int64(g1);
            var f6g2 = int64(f6) * int64(g2);
            var f6g3 = int64(f6) * int64(g3);
            var f6g4_19 = int64(f6) * int64(g4_19);
            var f6g5_19 = int64(f6) * int64(g5_19);
            var f6g6_19 = int64(f6) * int64(g6_19);
            var f6g7_19 = int64(f6) * int64(g7_19);
            var f6g8_19 = int64(f6) * int64(g8_19);
            var f6g9_19 = int64(f6) * int64(g9_19);
            var f7g0 = int64(f7) * int64(g0);
            var f7g1_2 = int64(f7_2) * int64(g1);
            var f7g2 = int64(f7) * int64(g2);
            var f7g3_38 = int64(f7_2) * int64(g3_19);
            var f7g4_19 = int64(f7) * int64(g4_19);
            var f7g5_38 = int64(f7_2) * int64(g5_19);
            var f7g6_19 = int64(f7) * int64(g6_19);
            var f7g7_38 = int64(f7_2) * int64(g7_19);
            var f7g8_19 = int64(f7) * int64(g8_19);
            var f7g9_38 = int64(f7_2) * int64(g9_19);
            var f8g0 = int64(f8) * int64(g0);
            var f8g1 = int64(f8) * int64(g1);
            var f8g2_19 = int64(f8) * int64(g2_19);
            var f8g3_19 = int64(f8) * int64(g3_19);
            var f8g4_19 = int64(f8) * int64(g4_19);
            var f8g5_19 = int64(f8) * int64(g5_19);
            var f8g6_19 = int64(f8) * int64(g6_19);
            var f8g7_19 = int64(f8) * int64(g7_19);
            var f8g8_19 = int64(f8) * int64(g8_19);
            var f8g9_19 = int64(f8) * int64(g9_19);
            var f9g0 = int64(f9) * int64(g0);
            var f9g1_38 = int64(f9_2) * int64(g1_19);
            var f9g2_19 = int64(f9) * int64(g2_19);
            var f9g3_38 = int64(f9_2) * int64(g3_19);
            var f9g4_19 = int64(f9) * int64(g4_19);
            var f9g5_38 = int64(f9_2) * int64(g5_19);
            var f9g6_19 = int64(f9) * int64(g6_19);
            var f9g7_38 = int64(f9_2) * int64(g7_19);
            var f9g8_19 = int64(f9) * int64(g8_19);
            var f9g9_38 = int64(f9_2) * int64(g9_19);
            var h0 = f0g0 + f1g9_38 + f2g8_19 + f3g7_38 + f4g6_19 + f5g5_38 + f6g4_19 + f7g3_38 + f8g2_19 + f9g1_38;
            var h1 = f0g1 + f1g0 + f2g9_19 + f3g8_19 + f4g7_19 + f5g6_19 + f6g5_19 + f7g4_19 + f8g3_19 + f9g2_19;
            var h2 = f0g2 + f1g1_2 + f2g0 + f3g9_38 + f4g8_19 + f5g7_38 + f6g6_19 + f7g5_38 + f8g4_19 + f9g3_38;
            var h3 = f0g3 + f1g2 + f2g1 + f3g0 + f4g9_19 + f5g8_19 + f6g7_19 + f7g6_19 + f8g5_19 + f9g4_19;
            var h4 = f0g4 + f1g3_2 + f2g2 + f3g1_2 + f4g0 + f5g9_38 + f6g8_19 + f7g7_38 + f8g6_19 + f9g5_38;
            var h5 = f0g5 + f1g4 + f2g3 + f3g2 + f4g1 + f5g0 + f6g9_19 + f7g8_19 + f8g7_19 + f9g6_19;
            var h6 = f0g6 + f1g5_2 + f2g4 + f3g3_2 + f4g2 + f5g1_2 + f6g0 + f7g9_38 + f8g8_19 + f9g7_38;
            var h7 = f0g7 + f1g6 + f2g5 + f3g4 + f4g3 + f5g2 + f6g1 + f7g0 + f8g9_19 + f9g8_19;
            var h8 = f0g8 + f1g7_2 + f2g6 + f3g5_2 + f4g4 + f5g3_2 + f6g2 + f7g1_2 + f8g0 + f9g9_38;
            var h9 = f0g9 + f1g8 + f2g7 + f3g6 + f4g5 + f5g4 + f6g3 + f7g2 + f8g1 + f9g0;
            array<long> carry = new array<long>(10L); 

            // |h0| <= (1.1*1.1*2^52*(1+19+19+19+19)+1.1*1.1*2^50*(38+38+38+38+38))
            //   i.e. |h0| <= 1.2*2^59; narrower ranges for h2, h4, h6, h8
            // |h1| <= (1.1*1.1*2^51*(1+1+19+19+19+19+19+19+19+19))
            //   i.e. |h1| <= 1.5*2^58; narrower ranges for h3, h5, h7, h9

            carry[0L] = (h0 + (1L << (int)(25L))) >> (int)(26L);
            h1 += carry[0L];
            h0 -= carry[0L] << (int)(26L);
            carry[4L] = (h4 + (1L << (int)(25L))) >> (int)(26L);
            h5 += carry[4L];
            h4 -= carry[4L] << (int)(26L); 
            // |h0| <= 2^25
            // |h4| <= 2^25
            // |h1| <= 1.51*2^58
            // |h5| <= 1.51*2^58

            carry[1L] = (h1 + (1L << (int)(24L))) >> (int)(25L);
            h2 += carry[1L];
            h1 -= carry[1L] << (int)(25L);
            carry[5L] = (h5 + (1L << (int)(24L))) >> (int)(25L);
            h6 += carry[5L];
            h5 -= carry[5L] << (int)(25L); 
            // |h1| <= 2^24; from now on fits into int32
            // |h5| <= 2^24; from now on fits into int32
            // |h2| <= 1.21*2^59
            // |h6| <= 1.21*2^59

            carry[2L] = (h2 + (1L << (int)(25L))) >> (int)(26L);
            h3 += carry[2L];
            h2 -= carry[2L] << (int)(26L);
            carry[6L] = (h6 + (1L << (int)(25L))) >> (int)(26L);
            h7 += carry[6L];
            h6 -= carry[6L] << (int)(26L); 
            // |h2| <= 2^25; from now on fits into int32 unchanged
            // |h6| <= 2^25; from now on fits into int32 unchanged
            // |h3| <= 1.51*2^58
            // |h7| <= 1.51*2^58

            carry[3L] = (h3 + (1L << (int)(24L))) >> (int)(25L);
            h4 += carry[3L];
            h3 -= carry[3L] << (int)(25L);
            carry[7L] = (h7 + (1L << (int)(24L))) >> (int)(25L);
            h8 += carry[7L];
            h7 -= carry[7L] << (int)(25L); 
            // |h3| <= 2^24; from now on fits into int32 unchanged
            // |h7| <= 2^24; from now on fits into int32 unchanged
            // |h4| <= 1.52*2^33
            // |h8| <= 1.52*2^33

            carry[4L] = (h4 + (1L << (int)(25L))) >> (int)(26L);
            h5 += carry[4L];
            h4 -= carry[4L] << (int)(26L);
            carry[8L] = (h8 + (1L << (int)(25L))) >> (int)(26L);
            h9 += carry[8L];
            h8 -= carry[8L] << (int)(26L); 
            // |h4| <= 2^25; from now on fits into int32 unchanged
            // |h8| <= 2^25; from now on fits into int32 unchanged
            // |h5| <= 1.01*2^24
            // |h9| <= 1.51*2^58

            carry[9L] = (h9 + (1L << (int)(24L))) >> (int)(25L);
            h0 += carry[9L] * 19L;
            h9 -= carry[9L] << (int)(25L); 
            // |h9| <= 2^24; from now on fits into int32 unchanged
            // |h0| <= 1.8*2^37

            carry[0L] = (h0 + (1L << (int)(25L))) >> (int)(26L);
            h1 += carry[0L];
            h0 -= carry[0L] << (int)(26L); 
            // |h0| <= 2^25; from now on fits into int32 unchanged
            // |h1| <= 1.01*2^24

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

        // feSquare calculates h = f*f. Can overlap h with f.
        //
        // Preconditions:
        //    |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
        //
        // Postconditions:
        //    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
        private static void feSquare(ptr<fieldElement> _addr_h, ptr<fieldElement> _addr_f)
        {
            ref fieldElement h = ref _addr_h.val;
            ref fieldElement f = ref _addr_f.val;

            var f0 = f[0L];
            var f1 = f[1L];
            var f2 = f[2L];
            var f3 = f[3L];
            var f4 = f[4L];
            var f5 = f[5L];
            var f6 = f[6L];
            var f7 = f[7L];
            var f8 = f[8L];
            var f9 = f[9L];
            long f0_2 = 2L * f0;
            long f1_2 = 2L * f1;
            long f2_2 = 2L * f2;
            long f3_2 = 2L * f3;
            long f4_2 = 2L * f4;
            long f5_2 = 2L * f5;
            long f6_2 = 2L * f6;
            long f7_2 = 2L * f7;
            long f5_38 = 38L * f5; // 1.31*2^30
            long f6_19 = 19L * f6; // 1.31*2^30
            long f7_38 = 38L * f7; // 1.31*2^30
            long f8_19 = 19L * f8; // 1.31*2^30
            long f9_38 = 38L * f9; // 1.31*2^30
            var f0f0 = int64(f0) * int64(f0);
            var f0f1_2 = int64(f0_2) * int64(f1);
            var f0f2_2 = int64(f0_2) * int64(f2);
            var f0f3_2 = int64(f0_2) * int64(f3);
            var f0f4_2 = int64(f0_2) * int64(f4);
            var f0f5_2 = int64(f0_2) * int64(f5);
            var f0f6_2 = int64(f0_2) * int64(f6);
            var f0f7_2 = int64(f0_2) * int64(f7);
            var f0f8_2 = int64(f0_2) * int64(f8);
            var f0f9_2 = int64(f0_2) * int64(f9);
            var f1f1_2 = int64(f1_2) * int64(f1);
            var f1f2_2 = int64(f1_2) * int64(f2);
            var f1f3_4 = int64(f1_2) * int64(f3_2);
            var f1f4_2 = int64(f1_2) * int64(f4);
            var f1f5_4 = int64(f1_2) * int64(f5_2);
            var f1f6_2 = int64(f1_2) * int64(f6);
            var f1f7_4 = int64(f1_2) * int64(f7_2);
            var f1f8_2 = int64(f1_2) * int64(f8);
            var f1f9_76 = int64(f1_2) * int64(f9_38);
            var f2f2 = int64(f2) * int64(f2);
            var f2f3_2 = int64(f2_2) * int64(f3);
            var f2f4_2 = int64(f2_2) * int64(f4);
            var f2f5_2 = int64(f2_2) * int64(f5);
            var f2f6_2 = int64(f2_2) * int64(f6);
            var f2f7_2 = int64(f2_2) * int64(f7);
            var f2f8_38 = int64(f2_2) * int64(f8_19);
            var f2f9_38 = int64(f2) * int64(f9_38);
            var f3f3_2 = int64(f3_2) * int64(f3);
            var f3f4_2 = int64(f3_2) * int64(f4);
            var f3f5_4 = int64(f3_2) * int64(f5_2);
            var f3f6_2 = int64(f3_2) * int64(f6);
            var f3f7_76 = int64(f3_2) * int64(f7_38);
            var f3f8_38 = int64(f3_2) * int64(f8_19);
            var f3f9_76 = int64(f3_2) * int64(f9_38);
            var f4f4 = int64(f4) * int64(f4);
            var f4f5_2 = int64(f4_2) * int64(f5);
            var f4f6_38 = int64(f4_2) * int64(f6_19);
            var f4f7_38 = int64(f4) * int64(f7_38);
            var f4f8_38 = int64(f4_2) * int64(f8_19);
            var f4f9_38 = int64(f4) * int64(f9_38);
            var f5f5_38 = int64(f5) * int64(f5_38);
            var f5f6_38 = int64(f5_2) * int64(f6_19);
            var f5f7_76 = int64(f5_2) * int64(f7_38);
            var f5f8_38 = int64(f5_2) * int64(f8_19);
            var f5f9_76 = int64(f5_2) * int64(f9_38);
            var f6f6_19 = int64(f6) * int64(f6_19);
            var f6f7_38 = int64(f6) * int64(f7_38);
            var f6f8_38 = int64(f6_2) * int64(f8_19);
            var f6f9_38 = int64(f6) * int64(f9_38);
            var f7f7_38 = int64(f7) * int64(f7_38);
            var f7f8_38 = int64(f7_2) * int64(f8_19);
            var f7f9_76 = int64(f7_2) * int64(f9_38);
            var f8f8_19 = int64(f8) * int64(f8_19);
            var f8f9_38 = int64(f8) * int64(f9_38);
            var f9f9_38 = int64(f9) * int64(f9_38);
            var h0 = f0f0 + f1f9_76 + f2f8_38 + f3f7_76 + f4f6_38 + f5f5_38;
            var h1 = f0f1_2 + f2f9_38 + f3f8_38 + f4f7_38 + f5f6_38;
            var h2 = f0f2_2 + f1f1_2 + f3f9_76 + f4f8_38 + f5f7_76 + f6f6_19;
            var h3 = f0f3_2 + f1f2_2 + f4f9_38 + f5f8_38 + f6f7_38;
            var h4 = f0f4_2 + f1f3_4 + f2f2 + f5f9_76 + f6f8_38 + f7f7_38;
            var h5 = f0f5_2 + f1f4_2 + f2f3_2 + f6f9_38 + f7f8_38;
            var h6 = f0f6_2 + f1f5_4 + f2f4_2 + f3f3_2 + f7f9_76 + f8f8_19;
            var h7 = f0f7_2 + f1f6_2 + f2f5_2 + f3f4_2 + f8f9_38;
            var h8 = f0f8_2 + f1f7_4 + f2f6_2 + f3f5_4 + f4f4 + f9f9_38;
            var h9 = f0f9_2 + f1f8_2 + f2f7_2 + f3f6_2 + f4f5_2;
            array<long> carry = new array<long>(10L);

            carry[0L] = (h0 + (1L << (int)(25L))) >> (int)(26L);
            h1 += carry[0L];
            h0 -= carry[0L] << (int)(26L);
            carry[4L] = (h4 + (1L << (int)(25L))) >> (int)(26L);
            h5 += carry[4L];
            h4 -= carry[4L] << (int)(26L);

            carry[1L] = (h1 + (1L << (int)(24L))) >> (int)(25L);
            h2 += carry[1L];
            h1 -= carry[1L] << (int)(25L);
            carry[5L] = (h5 + (1L << (int)(24L))) >> (int)(25L);
            h6 += carry[5L];
            h5 -= carry[5L] << (int)(25L);

            carry[2L] = (h2 + (1L << (int)(25L))) >> (int)(26L);
            h3 += carry[2L];
            h2 -= carry[2L] << (int)(26L);
            carry[6L] = (h6 + (1L << (int)(25L))) >> (int)(26L);
            h7 += carry[6L];
            h6 -= carry[6L] << (int)(26L);

            carry[3L] = (h3 + (1L << (int)(24L))) >> (int)(25L);
            h4 += carry[3L];
            h3 -= carry[3L] << (int)(25L);
            carry[7L] = (h7 + (1L << (int)(24L))) >> (int)(25L);
            h8 += carry[7L];
            h7 -= carry[7L] << (int)(25L);

            carry[4L] = (h4 + (1L << (int)(25L))) >> (int)(26L);
            h5 += carry[4L];
            h4 -= carry[4L] << (int)(26L);
            carry[8L] = (h8 + (1L << (int)(25L))) >> (int)(26L);
            h9 += carry[8L];
            h8 -= carry[8L] << (int)(26L);

            carry[9L] = (h9 + (1L << (int)(24L))) >> (int)(25L);
            h0 += carry[9L] * 19L;
            h9 -= carry[9L] << (int)(25L);

            carry[0L] = (h0 + (1L << (int)(25L))) >> (int)(26L);
            h1 += carry[0L];
            h0 -= carry[0L] << (int)(26L);

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

        // feMul121666 calculates h = f * 121666. Can overlap h with f.
        //
        // Preconditions:
        //    |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
        //
        // Postconditions:
        //    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
        private static void feMul121666(ptr<fieldElement> _addr_h, ptr<fieldElement> _addr_f)
        {
            ref fieldElement h = ref _addr_h.val;
            ref fieldElement f = ref _addr_f.val;

            var h0 = int64(f[0L]) * 121666L;
            var h1 = int64(f[1L]) * 121666L;
            var h2 = int64(f[2L]) * 121666L;
            var h3 = int64(f[3L]) * 121666L;
            var h4 = int64(f[4L]) * 121666L;
            var h5 = int64(f[5L]) * 121666L;
            var h6 = int64(f[6L]) * 121666L;
            var h7 = int64(f[7L]) * 121666L;
            var h8 = int64(f[8L]) * 121666L;
            var h9 = int64(f[9L]) * 121666L;
            array<long> carry = new array<long>(10L);

            carry[9L] = (h9 + (1L << (int)(24L))) >> (int)(25L);
            h0 += carry[9L] * 19L;
            h9 -= carry[9L] << (int)(25L);
            carry[1L] = (h1 + (1L << (int)(24L))) >> (int)(25L);
            h2 += carry[1L];
            h1 -= carry[1L] << (int)(25L);
            carry[3L] = (h3 + (1L << (int)(24L))) >> (int)(25L);
            h4 += carry[3L];
            h3 -= carry[3L] << (int)(25L);
            carry[5L] = (h5 + (1L << (int)(24L))) >> (int)(25L);
            h6 += carry[5L];
            h5 -= carry[5L] << (int)(25L);
            carry[7L] = (h7 + (1L << (int)(24L))) >> (int)(25L);
            h8 += carry[7L];
            h7 -= carry[7L] << (int)(25L);

            carry[0L] = (h0 + (1L << (int)(25L))) >> (int)(26L);
            h1 += carry[0L];
            h0 -= carry[0L] << (int)(26L);
            carry[2L] = (h2 + (1L << (int)(25L))) >> (int)(26L);
            h3 += carry[2L];
            h2 -= carry[2L] << (int)(26L);
            carry[4L] = (h4 + (1L << (int)(25L))) >> (int)(26L);
            h5 += carry[4L];
            h4 -= carry[4L] << (int)(26L);
            carry[6L] = (h6 + (1L << (int)(25L))) >> (int)(26L);
            h7 += carry[6L];
            h6 -= carry[6L] << (int)(26L);
            carry[8L] = (h8 + (1L << (int)(25L))) >> (int)(26L);
            h9 += carry[8L];
            h8 -= carry[8L] << (int)(26L);

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

        // feInvert sets out = z^-1.
        private static void feInvert(ptr<fieldElement> _addr_@out, ptr<fieldElement> _addr_z)
        {
            ref fieldElement @out = ref _addr_@out.val;
            ref fieldElement z = ref _addr_z.val;

            ref fieldElement t0 = ref heap(out ptr<fieldElement> _addr_t0);            ref fieldElement t1 = ref heap(out ptr<fieldElement> _addr_t1);            ref fieldElement t2 = ref heap(out ptr<fieldElement> _addr_t2);            ref fieldElement t3 = ref heap(out ptr<fieldElement> _addr_t3);

            long i = default;

            feSquare(_addr_t0, _addr_z);
            for (i = 1L; i < 1L; i++)
            {
                feSquare(_addr_t0, _addr_t0);
            }

            feSquare(_addr_t1, _addr_t0);
            for (i = 1L; i < 2L; i++)
            {
                feSquare(_addr_t1, _addr_t1);
            }

            feMul(_addr_t1, _addr_z, _addr_t1);
            feMul(_addr_t0, _addr_t0, _addr_t1);
            feSquare(_addr_t2, _addr_t0);
            for (i = 1L; i < 1L; i++)
            {
                feSquare(_addr_t2, _addr_t2);
            }

            feMul(_addr_t1, _addr_t1, _addr_t2);
            feSquare(_addr_t2, _addr_t1);
            for (i = 1L; i < 5L; i++)
            {
                feSquare(_addr_t2, _addr_t2);
            }

            feMul(_addr_t1, _addr_t2, _addr_t1);
            feSquare(_addr_t2, _addr_t1);
            for (i = 1L; i < 10L; i++)
            {
                feSquare(_addr_t2, _addr_t2);
            }

            feMul(_addr_t2, _addr_t2, _addr_t1);
            feSquare(_addr_t3, _addr_t2);
            for (i = 1L; i < 20L; i++)
            {
                feSquare(_addr_t3, _addr_t3);
            }

            feMul(_addr_t2, _addr_t3, _addr_t2);
            feSquare(_addr_t2, _addr_t2);
            for (i = 1L; i < 10L; i++)
            {
                feSquare(_addr_t2, _addr_t2);
            }

            feMul(_addr_t1, _addr_t2, _addr_t1);
            feSquare(_addr_t2, _addr_t1);
            for (i = 1L; i < 50L; i++)
            {
                feSquare(_addr_t2, _addr_t2);
            }

            feMul(_addr_t2, _addr_t2, _addr_t1);
            feSquare(_addr_t3, _addr_t2);
            for (i = 1L; i < 100L; i++)
            {
                feSquare(_addr_t3, _addr_t3);
            }

            feMul(_addr_t2, _addr_t3, _addr_t2);
            feSquare(_addr_t2, _addr_t2);
            for (i = 1L; i < 50L; i++)
            {
                feSquare(_addr_t2, _addr_t2);
            }

            feMul(_addr_t1, _addr_t2, _addr_t1);
            feSquare(_addr_t1, _addr_t1);
            for (i = 1L; i < 5L; i++)
            {
                feSquare(_addr_t1, _addr_t1);
            }

            feMul(_addr_out, _addr_t1, _addr_t0);

        }

        private static void scalarMultGeneric(ptr<array<byte>> _addr_@out, ptr<array<byte>> _addr_@in, ptr<array<byte>> _addr_@base)
        {
            ref array<byte> @out = ref _addr_@out.val;
            ref array<byte> @in = ref _addr_@in.val;
            ref array<byte> @base = ref _addr_@base.val;

            array<byte> e = new array<byte>(32L);

            copy(e[..], in[..]);
            e[0L] &= 248L;
            e[31L] &= 127L;
            e[31L] |= 64L;

            ref fieldElement x1 = ref heap(out ptr<fieldElement> _addr_x1);            ref fieldElement x2 = ref heap(out ptr<fieldElement> _addr_x2);            ref fieldElement z2 = ref heap(out ptr<fieldElement> _addr_z2);            ref fieldElement x3 = ref heap(out ptr<fieldElement> _addr_x3);            ref fieldElement z3 = ref heap(out ptr<fieldElement> _addr_z3);            ref fieldElement tmp0 = ref heap(out ptr<fieldElement> _addr_tmp0);            ref fieldElement tmp1 = ref heap(out ptr<fieldElement> _addr_tmp1);

            feFromBytes(_addr_x1, _addr_base);
            feOne(_addr_x2);
            feCopy(_addr_x3, _addr_x1);
            feOne(_addr_z3);

            var swap = int32(0L);
            for (long pos = 254L; pos >= 0L; pos--)
            {
                var b = e[pos / 8L] >> (int)(uint(pos & 7L));
                b &= 1L;
                swap ^= int32(b);
                feCSwap(_addr_x2, _addr_x3, swap);
                feCSwap(_addr_z2, _addr_z3, swap);
                swap = int32(b);

                feSub(_addr_tmp0, _addr_x3, _addr_z3);
                feSub(_addr_tmp1, _addr_x2, _addr_z2);
                feAdd(_addr_x2, _addr_x2, _addr_z2);
                feAdd(_addr_z2, _addr_x3, _addr_z3);
                feMul(_addr_z3, _addr_tmp0, _addr_x2);
                feMul(_addr_z2, _addr_z2, _addr_tmp1);
                feSquare(_addr_tmp0, _addr_tmp1);
                feSquare(_addr_tmp1, _addr_x2);
                feAdd(_addr_x3, _addr_z3, _addr_z2);
                feSub(_addr_z2, _addr_z3, _addr_z2);
                feMul(_addr_x2, _addr_tmp1, _addr_tmp0);
                feSub(_addr_tmp1, _addr_tmp1, _addr_tmp0);
                feSquare(_addr_z2, _addr_z2);
                feMul121666(_addr_z3, _addr_tmp1);
                feSquare(_addr_x3, _addr_x3);
                feAdd(_addr_tmp0, _addr_tmp0, _addr_z3);
                feMul(_addr_z3, _addr_x1, _addr_z2);
                feMul(_addr_z2, _addr_tmp1, _addr_tmp0);
            }


            feCSwap(_addr_x2, _addr_x3, swap);
            feCSwap(_addr_z2, _addr_z3, swap);

            feInvert(_addr_z2, _addr_z2);
            feMul(_addr_x2, _addr_x2, _addr_z2);
            feToBytes(_addr_out, _addr_x2);

        }
    }
}}}}}
