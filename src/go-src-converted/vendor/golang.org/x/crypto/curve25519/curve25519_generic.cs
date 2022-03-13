// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package curve25519 -- go2cs converted at 2022 March 13 06:44:56 UTC
// import "vendor/golang.org/x/crypto/curve25519" ==> using curve25519 = go.vendor.golang.org.x.crypto.curve25519_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\curve25519\curve25519_generic.go
namespace go.vendor.golang.org.x.crypto;

using binary = encoding.binary_package;

public static partial class curve25519_package {

// This code is a port of the public domain, "ref10" implementation of
// curve25519 from SUPERCOP 20130419 by D. J. Bernstein.

// fieldElement represents an element of the field GF(2^255 - 19). An element
// t, entries t[0]...t[9], represents the integer t[0]+2^26 t[1]+2^51 t[2]+2^77
// t[3]+2^102 t[4]+...+2^230 t[9]. Bounds on each t[i] vary depending on
// context.
private partial struct fieldElement { // : array<int>
}

private static void feZero(ptr<fieldElement> _addr_fe) {
    ref fieldElement fe = ref _addr_fe.val;

    foreach (var (i) in fe) {
        fe[i] = 0;
    }
}

private static void feOne(ptr<fieldElement> _addr_fe) {
    ref fieldElement fe = ref _addr_fe.val;

    feZero(_addr_fe);
    fe[0] = 1;
}

private static void feAdd(ptr<fieldElement> _addr_dst, ptr<fieldElement> _addr_a, ptr<fieldElement> _addr_b) {
    ref fieldElement dst = ref _addr_dst.val;
    ref fieldElement a = ref _addr_a.val;
    ref fieldElement b = ref _addr_b.val;

    foreach (var (i) in dst) {
        dst[i] = a[i] + b[i];
    }
}

private static void feSub(ptr<fieldElement> _addr_dst, ptr<fieldElement> _addr_a, ptr<fieldElement> _addr_b) {
    ref fieldElement dst = ref _addr_dst.val;
    ref fieldElement a = ref _addr_a.val;
    ref fieldElement b = ref _addr_b.val;

    foreach (var (i) in dst) {
        dst[i] = a[i] - b[i];
    }
}

private static void feCopy(ptr<fieldElement> _addr_dst, ptr<fieldElement> _addr_src) {
    ref fieldElement dst = ref _addr_dst.val;
    ref fieldElement src = ref _addr_src.val;

    foreach (var (i) in dst) {
        dst[i] = src[i];
    }
}

// feCSwap replaces (f,g) with (g,f) if b == 1; replaces (f,g) with (f,g) if b == 0.
//
// Preconditions: b in {0,1}.
private static void feCSwap(ptr<fieldElement> _addr_f, ptr<fieldElement> _addr_g, int b) {
    ref fieldElement f = ref _addr_f.val;
    ref fieldElement g = ref _addr_g.val;

    b = -b;
    foreach (var (i) in f) {
        var t = b & (f[i] ^ g[i]);
        f[i] ^= t;
        g[i] ^= t;
    }
}

// load3 reads a 24-bit, little-endian value from in.
private static long load3(slice<byte> @in) {
    long r = default;
    r = int64(in[0]);
    r |= int64(in[1]) << 8;
    r |= int64(in[2]) << 16;
    return r;
}

// load4 reads a 32-bit, little-endian value from in.
private static long load4(slice<byte> @in) {
    return int64(binary.LittleEndian.Uint32(in));
}

private static void feFromBytes(ptr<fieldElement> _addr_dst, ptr<array<byte>> _addr_src) {
    ref fieldElement dst = ref _addr_dst.val;
    ref array<byte> src = ref _addr_src.val;

    var h0 = load4(src[..]);
    var h1 = load3(src[(int)4..]) << 6;
    var h2 = load3(src[(int)7..]) << 5;
    var h3 = load3(src[(int)10..]) << 3;
    var h4 = load3(src[(int)13..]) << 2;
    var h5 = load4(src[(int)16..]);
    var h6 = load3(src[(int)20..]) << 7;
    var h7 = load3(src[(int)23..]) << 5;
    var h8 = load3(src[(int)26..]) << 4;
    var h9 = (load3(src[(int)29..]) & 0x7fffff) << 2;

    array<long> carry = new array<long>(10);
    carry[9] = (h9 + 1 << 24) >> 25;
    h0 += carry[9] * 19;
    h9 -= carry[9] << 25;
    carry[1] = (h1 + 1 << 24) >> 25;
    h2 += carry[1];
    h1 -= carry[1] << 25;
    carry[3] = (h3 + 1 << 24) >> 25;
    h4 += carry[3];
    h3 -= carry[3] << 25;
    carry[5] = (h5 + 1 << 24) >> 25;
    h6 += carry[5];
    h5 -= carry[5] << 25;
    carry[7] = (h7 + 1 << 24) >> 25;
    h8 += carry[7];
    h7 -= carry[7] << 25;

    carry[0] = (h0 + 1 << 25) >> 26;
    h1 += carry[0];
    h0 -= carry[0] << 26;
    carry[2] = (h2 + 1 << 25) >> 26;
    h3 += carry[2];
    h2 -= carry[2] << 26;
    carry[4] = (h4 + 1 << 25) >> 26;
    h5 += carry[4];
    h4 -= carry[4] << 26;
    carry[6] = (h6 + 1 << 25) >> 26;
    h7 += carry[6];
    h6 -= carry[6] << 26;
    carry[8] = (h8 + 1 << 25) >> 26;
    h9 += carry[8];
    h8 -= carry[8] << 26;

    dst[0] = int32(h0);
    dst[1] = int32(h1);
    dst[2] = int32(h2);
    dst[3] = int32(h3);
    dst[4] = int32(h4);
    dst[5] = int32(h5);
    dst[6] = int32(h6);
    dst[7] = int32(h7);
    dst[8] = int32(h8);
    dst[9] = int32(h9);
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
private static void feToBytes(ptr<array<byte>> _addr_s, ptr<fieldElement> _addr_h) {
    ref array<byte> s = ref _addr_s.val;
    ref fieldElement h = ref _addr_h.val;

    array<int> carry = new array<int>(10);

    nint q = (19 * h[9] + (1 << 24)) >> 25;
    q = (h[0] + q) >> 26;
    q = (h[1] + q) >> 25;
    q = (h[2] + q) >> 26;
    q = (h[3] + q) >> 25;
    q = (h[4] + q) >> 26;
    q = (h[5] + q) >> 25;
    q = (h[6] + q) >> 26;
    q = (h[7] + q) >> 25;
    q = (h[8] + q) >> 26;
    q = (h[9] + q) >> 25; 

    // Goal: Output h-(2^255-19)q, which is between 0 and 2^255-20.
    h[0] += 19 * q; 
    // Goal: Output h-2^255 q, which is between 0 and 2^255-20.

    carry[0] = h[0] >> 26;
    h[1] += carry[0];
    h[0] -= carry[0] << 26;
    carry[1] = h[1] >> 25;
    h[2] += carry[1];
    h[1] -= carry[1] << 25;
    carry[2] = h[2] >> 26;
    h[3] += carry[2];
    h[2] -= carry[2] << 26;
    carry[3] = h[3] >> 25;
    h[4] += carry[3];
    h[3] -= carry[3] << 25;
    carry[4] = h[4] >> 26;
    h[5] += carry[4];
    h[4] -= carry[4] << 26;
    carry[5] = h[5] >> 25;
    h[6] += carry[5];
    h[5] -= carry[5] << 25;
    carry[6] = h[6] >> 26;
    h[7] += carry[6];
    h[6] -= carry[6] << 26;
    carry[7] = h[7] >> 25;
    h[8] += carry[7];
    h[7] -= carry[7] << 25;
    carry[8] = h[8] >> 26;
    h[9] += carry[8];
    h[8] -= carry[8] << 26;
    carry[9] = h[9] >> 25;
    h[9] -= carry[9] << 25; 
    // h10 = carry9

    // Goal: Output h[0]+...+2^255 h10-2^255 q, which is between 0 and 2^255-20.
    // Have h[0]+...+2^230 h[9] between 0 and 2^255-1;
    // evidently 2^255 h10-2^255 q = 0.
    // Goal: Output h[0]+...+2^230 h[9].

    s[0] = byte(h[0] >> 0);
    s[1] = byte(h[0] >> 8);
    s[2] = byte(h[0] >> 16);
    s[3] = byte((h[0] >> 24) | (h[1] << 2));
    s[4] = byte(h[1] >> 6);
    s[5] = byte(h[1] >> 14);
    s[6] = byte((h[1] >> 22) | (h[2] << 3));
    s[7] = byte(h[2] >> 5);
    s[8] = byte(h[2] >> 13);
    s[9] = byte((h[2] >> 21) | (h[3] << 5));
    s[10] = byte(h[3] >> 3);
    s[11] = byte(h[3] >> 11);
    s[12] = byte((h[3] >> 19) | (h[4] << 6));
    s[13] = byte(h[4] >> 2);
    s[14] = byte(h[4] >> 10);
    s[15] = byte(h[4] >> 18);
    s[16] = byte(h[5] >> 0);
    s[17] = byte(h[5] >> 8);
    s[18] = byte(h[5] >> 16);
    s[19] = byte((h[5] >> 24) | (h[6] << 1));
    s[20] = byte(h[6] >> 7);
    s[21] = byte(h[6] >> 15);
    s[22] = byte((h[6] >> 23) | (h[7] << 3));
    s[23] = byte(h[7] >> 5);
    s[24] = byte(h[7] >> 13);
    s[25] = byte((h[7] >> 21) | (h[8] << 4));
    s[26] = byte(h[8] >> 4);
    s[27] = byte(h[8] >> 12);
    s[28] = byte((h[8] >> 20) | (h[9] << 6));
    s[29] = byte(h[9] >> 2);
    s[30] = byte(h[9] >> 10);
    s[31] = byte(h[9] >> 18);
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
private static void feMul(ptr<fieldElement> _addr_h, ptr<fieldElement> _addr_f, ptr<fieldElement> _addr_g) {
    ref fieldElement h = ref _addr_h.val;
    ref fieldElement f = ref _addr_f.val;
    ref fieldElement g = ref _addr_g.val;

    var f0 = f[0];
    var f1 = f[1];
    var f2 = f[2];
    var f3 = f[3];
    var f4 = f[4];
    var f5 = f[5];
    var f6 = f[6];
    var f7 = f[7];
    var f8 = f[8];
    var f9 = f[9];
    var g0 = g[0];
    var g1 = g[1];
    var g2 = g[2];
    var g3 = g[3];
    var g4 = g[4];
    var g5 = g[5];
    var g6 = g[6];
    var g7 = g[7];
    var g8 = g[8];
    var g9 = g[9];
    nint g1_19 = 19 * g1; // 1.4*2^29
    nint g2_19 = 19 * g2; // 1.4*2^30; still ok
    nint g3_19 = 19 * g3;
    nint g4_19 = 19 * g4;
    nint g5_19 = 19 * g5;
    nint g6_19 = 19 * g6;
    nint g7_19 = 19 * g7;
    nint g8_19 = 19 * g8;
    nint g9_19 = 19 * g9;
    nint f1_2 = 2 * f1;
    nint f3_2 = 2 * f3;
    nint f5_2 = 2 * f5;
    nint f7_2 = 2 * f7;
    nint f9_2 = 2 * f9;
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
    array<long> carry = new array<long>(10); 

    // |h0| <= (1.1*1.1*2^52*(1+19+19+19+19)+1.1*1.1*2^50*(38+38+38+38+38))
    //   i.e. |h0| <= 1.2*2^59; narrower ranges for h2, h4, h6, h8
    // |h1| <= (1.1*1.1*2^51*(1+1+19+19+19+19+19+19+19+19))
    //   i.e. |h1| <= 1.5*2^58; narrower ranges for h3, h5, h7, h9

    carry[0] = (h0 + (1 << 25)) >> 26;
    h1 += carry[0];
    h0 -= carry[0] << 26;
    carry[4] = (h4 + (1 << 25)) >> 26;
    h5 += carry[4];
    h4 -= carry[4] << 26; 
    // |h0| <= 2^25
    // |h4| <= 2^25
    // |h1| <= 1.51*2^58
    // |h5| <= 1.51*2^58

    carry[1] = (h1 + (1 << 24)) >> 25;
    h2 += carry[1];
    h1 -= carry[1] << 25;
    carry[5] = (h5 + (1 << 24)) >> 25;
    h6 += carry[5];
    h5 -= carry[5] << 25; 
    // |h1| <= 2^24; from now on fits into int32
    // |h5| <= 2^24; from now on fits into int32
    // |h2| <= 1.21*2^59
    // |h6| <= 1.21*2^59

    carry[2] = (h2 + (1 << 25)) >> 26;
    h3 += carry[2];
    h2 -= carry[2] << 26;
    carry[6] = (h6 + (1 << 25)) >> 26;
    h7 += carry[6];
    h6 -= carry[6] << 26; 
    // |h2| <= 2^25; from now on fits into int32 unchanged
    // |h6| <= 2^25; from now on fits into int32 unchanged
    // |h3| <= 1.51*2^58
    // |h7| <= 1.51*2^58

    carry[3] = (h3 + (1 << 24)) >> 25;
    h4 += carry[3];
    h3 -= carry[3] << 25;
    carry[7] = (h7 + (1 << 24)) >> 25;
    h8 += carry[7];
    h7 -= carry[7] << 25; 
    // |h3| <= 2^24; from now on fits into int32 unchanged
    // |h7| <= 2^24; from now on fits into int32 unchanged
    // |h4| <= 1.52*2^33
    // |h8| <= 1.52*2^33

    carry[4] = (h4 + (1 << 25)) >> 26;
    h5 += carry[4];
    h4 -= carry[4] << 26;
    carry[8] = (h8 + (1 << 25)) >> 26;
    h9 += carry[8];
    h8 -= carry[8] << 26; 
    // |h4| <= 2^25; from now on fits into int32 unchanged
    // |h8| <= 2^25; from now on fits into int32 unchanged
    // |h5| <= 1.01*2^24
    // |h9| <= 1.51*2^58

    carry[9] = (h9 + (1 << 24)) >> 25;
    h0 += carry[9] * 19;
    h9 -= carry[9] << 25; 
    // |h9| <= 2^24; from now on fits into int32 unchanged
    // |h0| <= 1.8*2^37

    carry[0] = (h0 + (1 << 25)) >> 26;
    h1 += carry[0];
    h0 -= carry[0] << 26; 
    // |h0| <= 2^25; from now on fits into int32 unchanged
    // |h1| <= 1.01*2^24

    h[0] = int32(h0);
    h[1] = int32(h1);
    h[2] = int32(h2);
    h[3] = int32(h3);
    h[4] = int32(h4);
    h[5] = int32(h5);
    h[6] = int32(h6);
    h[7] = int32(h7);
    h[8] = int32(h8);
    h[9] = int32(h9);
}

// feSquare calculates h = f*f. Can overlap h with f.
//
// Preconditions:
//    |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
//
// Postconditions:
//    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
private static void feSquare(ptr<fieldElement> _addr_h, ptr<fieldElement> _addr_f) {
    ref fieldElement h = ref _addr_h.val;
    ref fieldElement f = ref _addr_f.val;

    var f0 = f[0];
    var f1 = f[1];
    var f2 = f[2];
    var f3 = f[3];
    var f4 = f[4];
    var f5 = f[5];
    var f6 = f[6];
    var f7 = f[7];
    var f8 = f[8];
    var f9 = f[9];
    nint f0_2 = 2 * f0;
    nint f1_2 = 2 * f1;
    nint f2_2 = 2 * f2;
    nint f3_2 = 2 * f3;
    nint f4_2 = 2 * f4;
    nint f5_2 = 2 * f5;
    nint f6_2 = 2 * f6;
    nint f7_2 = 2 * f7;
    nint f5_38 = 38 * f5; // 1.31*2^30
    nint f6_19 = 19 * f6; // 1.31*2^30
    nint f7_38 = 38 * f7; // 1.31*2^30
    nint f8_19 = 19 * f8; // 1.31*2^30
    nint f9_38 = 38 * f9; // 1.31*2^30
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
    array<long> carry = new array<long>(10);

    carry[0] = (h0 + (1 << 25)) >> 26;
    h1 += carry[0];
    h0 -= carry[0] << 26;
    carry[4] = (h4 + (1 << 25)) >> 26;
    h5 += carry[4];
    h4 -= carry[4] << 26;

    carry[1] = (h1 + (1 << 24)) >> 25;
    h2 += carry[1];
    h1 -= carry[1] << 25;
    carry[5] = (h5 + (1 << 24)) >> 25;
    h6 += carry[5];
    h5 -= carry[5] << 25;

    carry[2] = (h2 + (1 << 25)) >> 26;
    h3 += carry[2];
    h2 -= carry[2] << 26;
    carry[6] = (h6 + (1 << 25)) >> 26;
    h7 += carry[6];
    h6 -= carry[6] << 26;

    carry[3] = (h3 + (1 << 24)) >> 25;
    h4 += carry[3];
    h3 -= carry[3] << 25;
    carry[7] = (h7 + (1 << 24)) >> 25;
    h8 += carry[7];
    h7 -= carry[7] << 25;

    carry[4] = (h4 + (1 << 25)) >> 26;
    h5 += carry[4];
    h4 -= carry[4] << 26;
    carry[8] = (h8 + (1 << 25)) >> 26;
    h9 += carry[8];
    h8 -= carry[8] << 26;

    carry[9] = (h9 + (1 << 24)) >> 25;
    h0 += carry[9] * 19;
    h9 -= carry[9] << 25;

    carry[0] = (h0 + (1 << 25)) >> 26;
    h1 += carry[0];
    h0 -= carry[0] << 26;

    h[0] = int32(h0);
    h[1] = int32(h1);
    h[2] = int32(h2);
    h[3] = int32(h3);
    h[4] = int32(h4);
    h[5] = int32(h5);
    h[6] = int32(h6);
    h[7] = int32(h7);
    h[8] = int32(h8);
    h[9] = int32(h9);
}

// feMul121666 calculates h = f * 121666. Can overlap h with f.
//
// Preconditions:
//    |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
//
// Postconditions:
//    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
private static void feMul121666(ptr<fieldElement> _addr_h, ptr<fieldElement> _addr_f) {
    ref fieldElement h = ref _addr_h.val;
    ref fieldElement f = ref _addr_f.val;

    var h0 = int64(f[0]) * 121666;
    var h1 = int64(f[1]) * 121666;
    var h2 = int64(f[2]) * 121666;
    var h3 = int64(f[3]) * 121666;
    var h4 = int64(f[4]) * 121666;
    var h5 = int64(f[5]) * 121666;
    var h6 = int64(f[6]) * 121666;
    var h7 = int64(f[7]) * 121666;
    var h8 = int64(f[8]) * 121666;
    var h9 = int64(f[9]) * 121666;
    array<long> carry = new array<long>(10);

    carry[9] = (h9 + (1 << 24)) >> 25;
    h0 += carry[9] * 19;
    h9 -= carry[9] << 25;
    carry[1] = (h1 + (1 << 24)) >> 25;
    h2 += carry[1];
    h1 -= carry[1] << 25;
    carry[3] = (h3 + (1 << 24)) >> 25;
    h4 += carry[3];
    h3 -= carry[3] << 25;
    carry[5] = (h5 + (1 << 24)) >> 25;
    h6 += carry[5];
    h5 -= carry[5] << 25;
    carry[7] = (h7 + (1 << 24)) >> 25;
    h8 += carry[7];
    h7 -= carry[7] << 25;

    carry[0] = (h0 + (1 << 25)) >> 26;
    h1 += carry[0];
    h0 -= carry[0] << 26;
    carry[2] = (h2 + (1 << 25)) >> 26;
    h3 += carry[2];
    h2 -= carry[2] << 26;
    carry[4] = (h4 + (1 << 25)) >> 26;
    h5 += carry[4];
    h4 -= carry[4] << 26;
    carry[6] = (h6 + (1 << 25)) >> 26;
    h7 += carry[6];
    h6 -= carry[6] << 26;
    carry[8] = (h8 + (1 << 25)) >> 26;
    h9 += carry[8];
    h8 -= carry[8] << 26;

    h[0] = int32(h0);
    h[1] = int32(h1);
    h[2] = int32(h2);
    h[3] = int32(h3);
    h[4] = int32(h4);
    h[5] = int32(h5);
    h[6] = int32(h6);
    h[7] = int32(h7);
    h[8] = int32(h8);
    h[9] = int32(h9);
}

// feInvert sets out = z^-1.
private static void feInvert(ptr<fieldElement> _addr_@out, ptr<fieldElement> _addr_z) {
    ref fieldElement @out = ref _addr_@out.val;
    ref fieldElement z = ref _addr_z.val;

    ref fieldElement t0 = ref heap(out ptr<fieldElement> _addr_t0);    ref fieldElement t1 = ref heap(out ptr<fieldElement> _addr_t1);    ref fieldElement t2 = ref heap(out ptr<fieldElement> _addr_t2);    ref fieldElement t3 = ref heap(out ptr<fieldElement> _addr_t3);

    nint i = default;

    feSquare(_addr_t0, _addr_z);
    for (i = 1; i < 1; i++) {
        feSquare(_addr_t0, _addr_t0);
    }
    feSquare(_addr_t1, _addr_t0);
    for (i = 1; i < 2; i++) {
        feSquare(_addr_t1, _addr_t1);
    }
    feMul(_addr_t1, _addr_z, _addr_t1);
    feMul(_addr_t0, _addr_t0, _addr_t1);
    feSquare(_addr_t2, _addr_t0);
    for (i = 1; i < 1; i++) {
        feSquare(_addr_t2, _addr_t2);
    }
    feMul(_addr_t1, _addr_t1, _addr_t2);
    feSquare(_addr_t2, _addr_t1);
    for (i = 1; i < 5; i++) {
        feSquare(_addr_t2, _addr_t2);
    }
    feMul(_addr_t1, _addr_t2, _addr_t1);
    feSquare(_addr_t2, _addr_t1);
    for (i = 1; i < 10; i++) {
        feSquare(_addr_t2, _addr_t2);
    }
    feMul(_addr_t2, _addr_t2, _addr_t1);
    feSquare(_addr_t3, _addr_t2);
    for (i = 1; i < 20; i++) {
        feSquare(_addr_t3, _addr_t3);
    }
    feMul(_addr_t2, _addr_t3, _addr_t2);
    feSquare(_addr_t2, _addr_t2);
    for (i = 1; i < 10; i++) {
        feSquare(_addr_t2, _addr_t2);
    }
    feMul(_addr_t1, _addr_t2, _addr_t1);
    feSquare(_addr_t2, _addr_t1);
    for (i = 1; i < 50; i++) {
        feSquare(_addr_t2, _addr_t2);
    }
    feMul(_addr_t2, _addr_t2, _addr_t1);
    feSquare(_addr_t3, _addr_t2);
    for (i = 1; i < 100; i++) {
        feSquare(_addr_t3, _addr_t3);
    }
    feMul(_addr_t2, _addr_t3, _addr_t2);
    feSquare(_addr_t2, _addr_t2);
    for (i = 1; i < 50; i++) {
        feSquare(_addr_t2, _addr_t2);
    }
    feMul(_addr_t1, _addr_t2, _addr_t1);
    feSquare(_addr_t1, _addr_t1);
    for (i = 1; i < 5; i++) {
        feSquare(_addr_t1, _addr_t1);
    }
    feMul(_addr_out, _addr_t1, _addr_t0);
}

private static void scalarMultGeneric(ptr<array<byte>> _addr_@out, ptr<array<byte>> _addr_@in, ptr<array<byte>> _addr_@base) {
    ref array<byte> @out = ref _addr_@out.val;
    ref array<byte> @in = ref _addr_@in.val;
    ref array<byte> @base = ref _addr_@base.val;

    array<byte> e = new array<byte>(32);

    copy(e[..], in[..]);
    e[0] &= 248;
    e[31] &= 127;
    e[31] |= 64;

    ref fieldElement x1 = ref heap(out ptr<fieldElement> _addr_x1);    ref fieldElement x2 = ref heap(out ptr<fieldElement> _addr_x2);    ref fieldElement z2 = ref heap(out ptr<fieldElement> _addr_z2);    ref fieldElement x3 = ref heap(out ptr<fieldElement> _addr_x3);    ref fieldElement z3 = ref heap(out ptr<fieldElement> _addr_z3);    ref fieldElement tmp0 = ref heap(out ptr<fieldElement> _addr_tmp0);    ref fieldElement tmp1 = ref heap(out ptr<fieldElement> _addr_tmp1);

    feFromBytes(_addr_x1, _addr_base);
    feOne(_addr_x2);
    feCopy(_addr_x3, _addr_x1);
    feOne(_addr_z3);

    var swap = int32(0);
    for (nint pos = 254; pos >= 0; pos--) {
        var b = e[pos / 8] >> (int)(uint(pos & 7));
        b &= 1;
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

} // end curve25519_package
