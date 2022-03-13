// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package edwards25519 -- go2cs converted at 2022 March 13 06:40:47 UTC
// import "cmd/vendor/golang.org/x/crypto/ed25519/internal/edwards25519" ==> using edwards25519 = go.cmd.vendor.golang.org.x.crypto.ed25519.@internal.edwards25519_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\crypto\ed25519\internal\edwards25519\edwards25519.go
namespace go.cmd.vendor.golang.org.x.crypto.ed25519.@internal;

using binary = encoding.binary_package;

public static partial class edwards25519_package {

// This code is a port of the public domain, “ref10” implementation of ed25519
// from SUPERCOP.

// FieldElement represents an element of the field GF(2^255 - 19).  An element
// t, entries t[0]...t[9], represents the integer t[0]+2^26 t[1]+2^51 t[2]+2^77
// t[3]+2^102 t[4]+...+2^230 t[9].  Bounds on each t[i] vary depending on
// context.
public partial struct FieldElement { // : array<int>
}

private static FieldElement zero = default;

public static void FeZero(ptr<FieldElement> _addr_fe) {
    ref FieldElement fe = ref _addr_fe.val;

    copy(fe[..], zero[..]);
}

public static void FeOne(ptr<FieldElement> _addr_fe) {
    ref FieldElement fe = ref _addr_fe.val;

    FeZero(_addr_fe);
    fe[0] = 1;
}

public static void FeAdd(ptr<FieldElement> _addr_dst, ptr<FieldElement> _addr_a, ptr<FieldElement> _addr_b) {
    ref FieldElement dst = ref _addr_dst.val;
    ref FieldElement a = ref _addr_a.val;
    ref FieldElement b = ref _addr_b.val;

    dst[0] = a[0] + b[0];
    dst[1] = a[1] + b[1];
    dst[2] = a[2] + b[2];
    dst[3] = a[3] + b[3];
    dst[4] = a[4] + b[4];
    dst[5] = a[5] + b[5];
    dst[6] = a[6] + b[6];
    dst[7] = a[7] + b[7];
    dst[8] = a[8] + b[8];
    dst[9] = a[9] + b[9];
}

public static void FeSub(ptr<FieldElement> _addr_dst, ptr<FieldElement> _addr_a, ptr<FieldElement> _addr_b) {
    ref FieldElement dst = ref _addr_dst.val;
    ref FieldElement a = ref _addr_a.val;
    ref FieldElement b = ref _addr_b.val;

    dst[0] = a[0] - b[0];
    dst[1] = a[1] - b[1];
    dst[2] = a[2] - b[2];
    dst[3] = a[3] - b[3];
    dst[4] = a[4] - b[4];
    dst[5] = a[5] - b[5];
    dst[6] = a[6] - b[6];
    dst[7] = a[7] - b[7];
    dst[8] = a[8] - b[8];
    dst[9] = a[9] - b[9];
}

public static void FeCopy(ptr<FieldElement> _addr_dst, ptr<FieldElement> _addr_src) {
    ref FieldElement dst = ref _addr_dst.val;
    ref FieldElement src = ref _addr_src.val;

    copy(dst[..], src[..]);
}

// Replace (f,g) with (g,g) if b == 1;
// replace (f,g) with (f,g) if b == 0.
//
// Preconditions: b in {0,1}.
public static void FeCMove(ptr<FieldElement> _addr_f, ptr<FieldElement> _addr_g, int b) {
    ref FieldElement f = ref _addr_f.val;
    ref FieldElement g = ref _addr_g.val;

    b = -b;
    f[0] ^= b & (f[0] ^ g[0]);
    f[1] ^= b & (f[1] ^ g[1]);
    f[2] ^= b & (f[2] ^ g[2]);
    f[3] ^= b & (f[3] ^ g[3]);
    f[4] ^= b & (f[4] ^ g[4]);
    f[5] ^= b & (f[5] ^ g[5]);
    f[6] ^= b & (f[6] ^ g[6]);
    f[7] ^= b & (f[7] ^ g[7]);
    f[8] ^= b & (f[8] ^ g[8]);
    f[9] ^= b & (f[9] ^ g[9]);
}

private static long load3(slice<byte> @in) {
    long r = default;
    r = int64(in[0]);
    r |= int64(in[1]) << 8;
    r |= int64(in[2]) << 16;
    return r;
}

private static long load4(slice<byte> @in) {
    long r = default;
    r = int64(in[0]);
    r |= int64(in[1]) << 8;
    r |= int64(in[2]) << 16;
    r |= int64(in[3]) << 24;
    return r;
}

public static void FeFromBytes(ptr<FieldElement> _addr_dst, ptr<array<byte>> _addr_src) {
    ref FieldElement dst = ref _addr_dst.val;
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
    var h9 = (load3(src[(int)29..]) & 8388607) << 2;

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
public static void FeToBytes(ptr<array<byte>> _addr_s, ptr<FieldElement> _addr_h) {
    ref array<byte> s = ref _addr_s.val;
    ref FieldElement h = ref _addr_h.val;

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

public static byte FeIsNegative(ptr<FieldElement> _addr_f) {
    ref FieldElement f = ref _addr_f.val;

    ref array<byte> s = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_s);
    FeToBytes(_addr_s, _addr_f);
    return s[0] & 1;
}

public static int FeIsNonZero(ptr<FieldElement> _addr_f) {
    ref FieldElement f = ref _addr_f.val;

    ref array<byte> s = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_s);
    FeToBytes(_addr_s, _addr_f);
    byte x = default;
    foreach (var (_, b) in s) {
        x |= b;
    }    x |= x >> 4;
    x |= x >> 2;
    x |= x >> 1;
    return int32(x & 1);
}

// FeNeg sets h = -f
//
// Preconditions:
//    |f| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
//
// Postconditions:
//    |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
public static void FeNeg(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f) {
    ref FieldElement h = ref _addr_h.val;
    ref FieldElement f = ref _addr_f.val;

    h[0] = -f[0];
    h[1] = -f[1];
    h[2] = -f[2];
    h[3] = -f[3];
    h[4] = -f[4];
    h[5] = -f[5];
    h[6] = -f[6];
    h[7] = -f[7];
    h[8] = -f[8];
    h[9] = -f[9];
}

public static void FeCombine(ptr<FieldElement> _addr_h, long h0, long h1, long h2, long h3, long h4, long h5, long h6, long h7, long h8, long h9) {
    ref FieldElement h = ref _addr_h.val;

    long c0 = default;    long c1 = default;    long c2 = default;    long c3 = default;    long c4 = default;    long c5 = default;    long c6 = default;    long c7 = default;    long c8 = default;    long c9 = default;

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

    c0 = (h0 + (1 << 25)) >> 26;
    h1 += c0;
    h0 -= c0 << 26;
    c4 = (h4 + (1 << 25)) >> 26;
    h5 += c4;
    h4 -= c4 << 26;
    /* |h0| <= 2^25 */
    /* |h4| <= 2^25 */
    /* |h1| <= 1.51*2^58 */
    /* |h5| <= 1.51*2^58 */

    c1 = (h1 + (1 << 24)) >> 25;
    h2 += c1;
    h1 -= c1 << 25;
    c5 = (h5 + (1 << 24)) >> 25;
    h6 += c5;
    h5 -= c5 << 25;
    /* |h1| <= 2^24; from now on fits into int32 */
    /* |h5| <= 2^24; from now on fits into int32 */
    /* |h2| <= 1.21*2^59 */
    /* |h6| <= 1.21*2^59 */

    c2 = (h2 + (1 << 25)) >> 26;
    h3 += c2;
    h2 -= c2 << 26;
    c6 = (h6 + (1 << 25)) >> 26;
    h7 += c6;
    h6 -= c6 << 26;
    /* |h2| <= 2^25; from now on fits into int32 unchanged */
    /* |h6| <= 2^25; from now on fits into int32 unchanged */
    /* |h3| <= 1.51*2^58 */
    /* |h7| <= 1.51*2^58 */

    c3 = (h3 + (1 << 24)) >> 25;
    h4 += c3;
    h3 -= c3 << 25;
    c7 = (h7 + (1 << 24)) >> 25;
    h8 += c7;
    h7 -= c7 << 25;
    /* |h3| <= 2^24; from now on fits into int32 unchanged */
    /* |h7| <= 2^24; from now on fits into int32 unchanged */
    /* |h4| <= 1.52*2^33 */
    /* |h8| <= 1.52*2^33 */

    c4 = (h4 + (1 << 25)) >> 26;
    h5 += c4;
    h4 -= c4 << 26;
    c8 = (h8 + (1 << 25)) >> 26;
    h9 += c8;
    h8 -= c8 << 26;
    /* |h4| <= 2^25; from now on fits into int32 unchanged */
    /* |h8| <= 2^25; from now on fits into int32 unchanged */
    /* |h5| <= 1.01*2^24 */
    /* |h9| <= 1.51*2^58 */

    c9 = (h9 + (1 << 24)) >> 25;
    h0 += c9 * 19;
    h9 -= c9 << 25;
    /* |h9| <= 2^24; from now on fits into int32 unchanged */
    /* |h0| <= 1.8*2^37 */

    c0 = (h0 + (1 << 25)) >> 26;
    h1 += c0;
    h0 -= c0 << 26;
    /* |h0| <= 2^25; from now on fits into int32 unchanged */
    /* |h1| <= 1.01*2^24 */

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
public static void FeMul(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f, ptr<FieldElement> _addr_g) {
    ref FieldElement h = ref _addr_h.val;
    ref FieldElement f = ref _addr_f.val;
    ref FieldElement g = ref _addr_g.val;

    var f0 = int64(f[0]);
    var f1 = int64(f[1]);
    var f2 = int64(f[2]);
    var f3 = int64(f[3]);
    var f4 = int64(f[4]);
    var f5 = int64(f[5]);
    var f6 = int64(f[6]);
    var f7 = int64(f[7]);
    var f8 = int64(f[8]);
    var f9 = int64(f[9]);

    var f1_2 = int64(2 * f[1]);
    var f3_2 = int64(2 * f[3]);
    var f5_2 = int64(2 * f[5]);
    var f7_2 = int64(2 * f[7]);
    var f9_2 = int64(2 * f[9]);

    var g0 = int64(g[0]);
    var g1 = int64(g[1]);
    var g2 = int64(g[2]);
    var g3 = int64(g[3]);
    var g4 = int64(g[4]);
    var g5 = int64(g[5]);
    var g6 = int64(g[6]);
    var g7 = int64(g[7]);
    var g8 = int64(g[8]);
    var g9 = int64(g[9]);

    var g1_19 = int64(19 * g[1]); /* 1.4*2^29 */
    var g2_19 = int64(19 * g[2]); /* 1.4*2^30; still ok */
    var g3_19 = int64(19 * g[3]);
    var g4_19 = int64(19 * g[4]);
    var g5_19 = int64(19 * g[5]);
    var g6_19 = int64(19 * g[6]);
    var g7_19 = int64(19 * g[7]);
    var g8_19 = int64(19 * g[8]);
    var g9_19 = int64(19 * g[9]);

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

private static (long, long, long, long, long, long, long, long, long, long) feSquare(ptr<FieldElement> _addr_f) {
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

    var f0 = int64(f[0]);
    var f1 = int64(f[1]);
    var f2 = int64(f[2]);
    var f3 = int64(f[3]);
    var f4 = int64(f[4]);
    var f5 = int64(f[5]);
    var f6 = int64(f[6]);
    var f7 = int64(f[7]);
    var f8 = int64(f[8]);
    var f9 = int64(f[9]);
    var f0_2 = int64(2 * f[0]);
    var f1_2 = int64(2 * f[1]);
    var f2_2 = int64(2 * f[2]);
    var f3_2 = int64(2 * f[3]);
    var f4_2 = int64(2 * f[4]);
    var f5_2 = int64(2 * f[5]);
    var f6_2 = int64(2 * f[6]);
    var f7_2 = int64(2 * f[7]);
    nint f5_38 = 38 * f5; // 1.31*2^30
    nint f6_19 = 19 * f6; // 1.31*2^30
    nint f7_38 = 38 * f7; // 1.31*2^30
    nint f8_19 = 19 * f8; // 1.31*2^30
    nint f9_38 = 38 * f9; // 1.31*2^30

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
public static void FeSquare(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f) {
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
public static void FeSquare2(ptr<FieldElement> _addr_h, ptr<FieldElement> _addr_f) {
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

public static void FeInvert(ptr<FieldElement> _addr_@out, ptr<FieldElement> _addr_z) {
    ref FieldElement @out = ref _addr_@out.val;
    ref FieldElement z = ref _addr_z.val;

    ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);    ref FieldElement t1 = ref heap(out ptr<FieldElement> _addr_t1);    ref FieldElement t2 = ref heap(out ptr<FieldElement> _addr_t2);    ref FieldElement t3 = ref heap(out ptr<FieldElement> _addr_t3);

    nint i = default;

    FeSquare(_addr_t0, _addr_z); // 2^1
    FeSquare(_addr_t1, _addr_t0); // 2^2
    for (i = 1; i < 2; i++) { // 2^3
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_t1, _addr_z, _addr_t1); // 2^3 + 2^0
    FeMul(_addr_t0, _addr_t0, _addr_t1); // 2^3 + 2^1 + 2^0
    FeSquare(_addr_t2, _addr_t0); // 2^4 + 2^2 + 2^1
    FeMul(_addr_t1, _addr_t1, _addr_t2); // 2^4 + 2^3 + 2^2 + 2^1 + 2^0
    FeSquare(_addr_t2, _addr_t1); // 5,4,3,2,1
    for (i = 1; i < 5; i++) { // 9,8,7,6,5
        FeSquare(_addr_t2, _addr_t2);
    }
    FeMul(_addr_t1, _addr_t2, _addr_t1); // 9,8,7,6,5,4,3,2,1,0
    FeSquare(_addr_t2, _addr_t1); // 10..1
    for (i = 1; i < 10; i++) { // 19..10
        FeSquare(_addr_t2, _addr_t2);
    }
    FeMul(_addr_t2, _addr_t2, _addr_t1); // 19..0
    FeSquare(_addr_t3, _addr_t2); // 20..1
    for (i = 1; i < 20; i++) { // 39..20
        FeSquare(_addr_t3, _addr_t3);
    }
    FeMul(_addr_t2, _addr_t3, _addr_t2); // 39..0
    FeSquare(_addr_t2, _addr_t2); // 40..1
    for (i = 1; i < 10; i++) { // 49..10
        FeSquare(_addr_t2, _addr_t2);
    }
    FeMul(_addr_t1, _addr_t2, _addr_t1); // 49..0
    FeSquare(_addr_t2, _addr_t1); // 50..1
    for (i = 1; i < 50; i++) { // 99..50
        FeSquare(_addr_t2, _addr_t2);
    }
    FeMul(_addr_t2, _addr_t2, _addr_t1); // 99..0
    FeSquare(_addr_t3, _addr_t2); // 100..1
    for (i = 1; i < 100; i++) { // 199..100
        FeSquare(_addr_t3, _addr_t3);
    }
    FeMul(_addr_t2, _addr_t3, _addr_t2); // 199..0
    FeSquare(_addr_t2, _addr_t2); // 200..1
    for (i = 1; i < 50; i++) { // 249..50
        FeSquare(_addr_t2, _addr_t2);
    }
    FeMul(_addr_t1, _addr_t2, _addr_t1); // 249..0
    FeSquare(_addr_t1, _addr_t1); // 250..1
    for (i = 1; i < 5; i++) { // 254..5
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_out, _addr_t1, _addr_t0); // 254..5,3,1,0
}

private static void fePow22523(ptr<FieldElement> _addr_@out, ptr<FieldElement> _addr_z) {
    ref FieldElement @out = ref _addr_@out.val;
    ref FieldElement z = ref _addr_z.val;

    ref FieldElement t0 = ref heap(out ptr<FieldElement> _addr_t0);    ref FieldElement t1 = ref heap(out ptr<FieldElement> _addr_t1);    ref FieldElement t2 = ref heap(out ptr<FieldElement> _addr_t2);

    nint i = default;

    FeSquare(_addr_t0, _addr_z);
    for (i = 1; i < 1; i++) {
        FeSquare(_addr_t0, _addr_t0);
    }
    FeSquare(_addr_t1, _addr_t0);
    for (i = 1; i < 2; i++) {
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_t1, _addr_z, _addr_t1);
    FeMul(_addr_t0, _addr_t0, _addr_t1);
    FeSquare(_addr_t0, _addr_t0);
    for (i = 1; i < 1; i++) {
        FeSquare(_addr_t0, _addr_t0);
    }
    FeMul(_addr_t0, _addr_t1, _addr_t0);
    FeSquare(_addr_t1, _addr_t0);
    for (i = 1; i < 5; i++) {
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_t0, _addr_t1, _addr_t0);
    FeSquare(_addr_t1, _addr_t0);
    for (i = 1; i < 10; i++) {
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_t1, _addr_t1, _addr_t0);
    FeSquare(_addr_t2, _addr_t1);
    for (i = 1; i < 20; i++) {
        FeSquare(_addr_t2, _addr_t2);
    }
    FeMul(_addr_t1, _addr_t2, _addr_t1);
    FeSquare(_addr_t1, _addr_t1);
    for (i = 1; i < 10; i++) {
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_t0, _addr_t1, _addr_t0);
    FeSquare(_addr_t1, _addr_t0);
    for (i = 1; i < 50; i++) {
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_t1, _addr_t1, _addr_t0);
    FeSquare(_addr_t2, _addr_t1);
    for (i = 1; i < 100; i++) {
        FeSquare(_addr_t2, _addr_t2);
    }
    FeMul(_addr_t1, _addr_t2, _addr_t1);
    FeSquare(_addr_t1, _addr_t1);
    for (i = 1; i < 50; i++) {
        FeSquare(_addr_t1, _addr_t1);
    }
    FeMul(_addr_t0, _addr_t1, _addr_t0);
    FeSquare(_addr_t0, _addr_t0);
    for (i = 1; i < 2; i++) {
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

public partial struct ProjectiveGroupElement {
    public FieldElement X;
    public FieldElement Y;
    public FieldElement Z;
}

public partial struct ExtendedGroupElement {
    public FieldElement X;
    public FieldElement Y;
    public FieldElement Z;
    public FieldElement T;
}

public partial struct CompletedGroupElement {
    public FieldElement X;
    public FieldElement Y;
    public FieldElement Z;
    public FieldElement T;
}

public partial struct PreComputedGroupElement {
    public FieldElement yPlusX;
    public FieldElement yMinusX;
    public FieldElement xy2d;
}

public partial struct CachedGroupElement {
    public FieldElement yPlusX;
    public FieldElement yMinusX;
    public FieldElement Z;
    public FieldElement T2d;
}

private static void Zero(this ptr<ProjectiveGroupElement> _addr_p) {
    ref ProjectiveGroupElement p = ref _addr_p.val;

    FeZero(_addr_p.X);
    FeOne(_addr_p.Y);
    FeOne(_addr_p.Z);
}

private static void Double(this ptr<ProjectiveGroupElement> _addr_p, ptr<CompletedGroupElement> _addr_r) {
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

private static void ToBytes(this ptr<ProjectiveGroupElement> _addr_p, ptr<array<byte>> _addr_s) {
    ref ProjectiveGroupElement p = ref _addr_p.val;
    ref array<byte> s = ref _addr_s.val;

    ref FieldElement recip = ref heap(out ptr<FieldElement> _addr_recip);    ref FieldElement x = ref heap(out ptr<FieldElement> _addr_x);    ref FieldElement y = ref heap(out ptr<FieldElement> _addr_y);



    FeInvert(_addr_recip, _addr_p.Z);
    FeMul(_addr_x, _addr_p.X, _addr_recip);
    FeMul(_addr_y, _addr_p.Y, _addr_recip);
    FeToBytes(_addr_s, _addr_y);
    s[31] ^= FeIsNegative(_addr_x) << 7;
}

private static void Zero(this ptr<ExtendedGroupElement> _addr_p) {
    ref ExtendedGroupElement p = ref _addr_p.val;

    FeZero(_addr_p.X);
    FeOne(_addr_p.Y);
    FeOne(_addr_p.Z);
    FeZero(_addr_p.T);
}

private static void Double(this ptr<ExtendedGroupElement> _addr_p, ptr<CompletedGroupElement> _addr_r) {
    ref ExtendedGroupElement p = ref _addr_p.val;
    ref CompletedGroupElement r = ref _addr_r.val;

    ref ProjectiveGroupElement q = ref heap(out ptr<ProjectiveGroupElement> _addr_q);
    p.ToProjective(_addr_q);
    q.Double(r);
}

private static void ToCached(this ptr<ExtendedGroupElement> _addr_p, ptr<CachedGroupElement> _addr_r) {
    ref ExtendedGroupElement p = ref _addr_p.val;
    ref CachedGroupElement r = ref _addr_r.val;

    FeAdd(_addr_r.yPlusX, _addr_p.Y, _addr_p.X);
    FeSub(_addr_r.yMinusX, _addr_p.Y, _addr_p.X);
    FeCopy(_addr_r.Z, _addr_p.Z);
    FeMul(_addr_r.T2d, _addr_p.T, _addr_d2);
}

private static void ToProjective(this ptr<ExtendedGroupElement> _addr_p, ptr<ProjectiveGroupElement> _addr_r) {
    ref ExtendedGroupElement p = ref _addr_p.val;
    ref ProjectiveGroupElement r = ref _addr_r.val;

    FeCopy(_addr_r.X, _addr_p.X);
    FeCopy(_addr_r.Y, _addr_p.Y);
    FeCopy(_addr_r.Z, _addr_p.Z);
}

private static void ToBytes(this ptr<ExtendedGroupElement> _addr_p, ptr<array<byte>> _addr_s) {
    ref ExtendedGroupElement p = ref _addr_p.val;
    ref array<byte> s = ref _addr_s.val;

    ref FieldElement recip = ref heap(out ptr<FieldElement> _addr_recip);    ref FieldElement x = ref heap(out ptr<FieldElement> _addr_x);    ref FieldElement y = ref heap(out ptr<FieldElement> _addr_y);



    FeInvert(_addr_recip, _addr_p.Z);
    FeMul(_addr_x, _addr_p.X, _addr_recip);
    FeMul(_addr_y, _addr_p.Y, _addr_recip);
    FeToBytes(_addr_s, _addr_y);
    s[31] ^= FeIsNegative(_addr_x) << 7;
}

private static bool FromBytes(this ptr<ExtendedGroupElement> _addr_p, ptr<array<byte>> _addr_s) {
    ref ExtendedGroupElement p = ref _addr_p.val;
    ref array<byte> s = ref _addr_s.val;

    ref FieldElement u = ref heap(out ptr<FieldElement> _addr_u);    ref FieldElement v = ref heap(out ptr<FieldElement> _addr_v);    ref FieldElement v3 = ref heap(out ptr<FieldElement> _addr_v3);    ref FieldElement vxx = ref heap(out ptr<FieldElement> _addr_vxx);    ref FieldElement check = ref heap(out ptr<FieldElement> _addr_check);



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

    ref array<byte> tmpX = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_tmpX);    array<byte> tmp2 = new array<byte>(32);



    FeSquare(_addr_vxx, _addr_p.X);
    FeMul(_addr_vxx, _addr_vxx, _addr_v);
    FeSub(_addr_check, _addr_vxx, _addr_u); // vx^2-u
    if (FeIsNonZero(_addr_check) == 1) {
        FeAdd(_addr_check, _addr_vxx, _addr_u); // vx^2+u
        if (FeIsNonZero(_addr_check) == 1) {
            return false;
        }
        FeMul(_addr_p.X, _addr_p.X, _addr_SqrtM1);

        FeToBytes(_addr_tmpX, _addr_p.X);
        {
            FieldElement v__prev1 = v;

            foreach (var (__i, __v) in tmpX) {
                i = __i;
                v = __v;
                tmp2[31 - i] = v;
            }

            v = v__prev1;
        }
    }
    if (FeIsNegative(_addr_p.X) != (s[31] >> 7)) {
        FeNeg(_addr_p.X, _addr_p.X);
    }
    FeMul(_addr_p.T, _addr_p.X, _addr_p.Y);
    return true;
}

private static void ToProjective(this ptr<CompletedGroupElement> _addr_p, ptr<ProjectiveGroupElement> _addr_r) {
    ref CompletedGroupElement p = ref _addr_p.val;
    ref ProjectiveGroupElement r = ref _addr_r.val;

    FeMul(_addr_r.X, _addr_p.X, _addr_p.T);
    FeMul(_addr_r.Y, _addr_p.Y, _addr_p.Z);
    FeMul(_addr_r.Z, _addr_p.Z, _addr_p.T);
}

private static void ToExtended(this ptr<CompletedGroupElement> _addr_p, ptr<ExtendedGroupElement> _addr_r) {
    ref CompletedGroupElement p = ref _addr_p.val;
    ref ExtendedGroupElement r = ref _addr_r.val;

    FeMul(_addr_r.X, _addr_p.X, _addr_p.T);
    FeMul(_addr_r.Y, _addr_p.Y, _addr_p.Z);
    FeMul(_addr_r.Z, _addr_p.Z, _addr_p.T);
    FeMul(_addr_r.T, _addr_p.X, _addr_p.Y);
}

private static void Zero(this ptr<PreComputedGroupElement> _addr_p) {
    ref PreComputedGroupElement p = ref _addr_p.val;

    FeOne(_addr_p.yPlusX);
    FeOne(_addr_p.yMinusX);
    FeZero(_addr_p.xy2d);
}

private static void geAdd(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<CachedGroupElement> _addr_q) {
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

private static void geSub(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<CachedGroupElement> _addr_q) {
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

private static void geMixedAdd(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<PreComputedGroupElement> _addr_q) {
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

private static void geMixedSub(ptr<CompletedGroupElement> _addr_r, ptr<ExtendedGroupElement> _addr_p, ptr<PreComputedGroupElement> _addr_q) {
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

private static void slide(ptr<array<sbyte>> _addr_r, ptr<array<byte>> _addr_a) {
    ref array<sbyte> r = ref _addr_r.val;
    ref array<byte> a = ref _addr_a.val;

    {
        var i__prev1 = i;

        foreach (var (__i) in r) {
            i = __i;
            r[i] = int8(1 & (a[i >> 3] >> (int)(uint(i & 7))));
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in r) {
            i = __i;
            if (r[i] != 0) {
                for (nint b = 1; b <= 6 && i + b < 256; b++) {
                    if (r[i + b] != 0) {
                        if (r[i] + (r[i + b] << (int)(uint(b))) <= 15) {
                            r[i] += r[i + b] << (int)(uint(b));
                            r[i + b] = 0;
                        }
                        else if (r[i] - (r[i + b] << (int)(uint(b))) >= -15) {
                            r[i] -= r[i + b] << (int)(uint(b));
                            for (var k = i + b; k < 256; k++) {
                                if (r[k] == 0) {
                                    r[k] = 1;
                                    break;
                                }
                                r[k] = 0;
                            }
                        else
                        } {
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
public static void GeDoubleScalarMultVartime(ptr<ProjectiveGroupElement> _addr_r, ptr<array<byte>> _addr_a, ptr<ExtendedGroupElement> _addr_A, ptr<array<byte>> _addr_b) {
    ref ProjectiveGroupElement r = ref _addr_r.val;
    ref array<byte> a = ref _addr_a.val;
    ref ExtendedGroupElement A = ref _addr_A.val;
    ref array<byte> b = ref _addr_b.val;

    ref array<sbyte> aSlide = ref heap(new array<sbyte>(256), out ptr<array<sbyte>> _addr_aSlide);    ref array<sbyte> bSlide = ref heap(new array<sbyte>(256), out ptr<array<sbyte>> _addr_bSlide);

    array<CachedGroupElement> Ai = new array<CachedGroupElement>(8); // A,3A,5A,7A,9A,11A,13A,15A
    ref CompletedGroupElement t = ref heap(out ptr<CompletedGroupElement> _addr_t);
    ref ExtendedGroupElement u = ref heap(out ptr<ExtendedGroupElement> _addr_u);    ref ExtendedGroupElement A2 = ref heap(out ptr<ExtendedGroupElement> _addr_A2);

    nint i = default;

    slide(_addr_aSlide, _addr_a);
    slide(_addr_bSlide, _addr_b);

    A.ToCached(_addr_Ai[0]);
    A.Double(_addr_t);
    t.ToExtended(_addr_A2);

    {
        nint i__prev1 = i;

        for (i = 0; i < 7; i++) {
            geAdd(_addr_t, _addr_A2, _addr_Ai[i]);
            t.ToExtended(_addr_u);
            u.ToCached(_addr_Ai[i + 1]);
        }

        i = i__prev1;
    }

    r.Zero();

    for (i = 255; i >= 0; i--) {
        if (aSlide[i] != 0 || bSlide[i] != 0) {
            break;
        }
    }

    while (i >= 0) {
        r.Double(_addr_t);

        if (aSlide[i] > 0) {
            t.ToExtended(_addr_u);
            geAdd(_addr_t, _addr_u, _addr_Ai[aSlide[i] / 2]);
        i--;
        }
        else if (aSlide[i] < 0) {
            t.ToExtended(_addr_u);
            geSub(_addr_t, _addr_u, _addr_Ai[(-aSlide[i]) / 2]);
        }
        if (bSlide[i] > 0) {
            t.ToExtended(_addr_u);
            geMixedAdd(_addr_t, _addr_u, _addr_bi[bSlide[i] / 2]);
        }
        else if (bSlide[i] < 0) {
            t.ToExtended(_addr_u);
            geMixedSub(_addr_t, _addr_u, _addr_bi[(-bSlide[i]) / 2]);
        }
        t.ToProjective(r);
    }
}

// equal returns 1 if b == c and 0 otherwise, assuming that b and c are
// non-negative.
private static int equal(int b, int c) {
    var x = uint32(b ^ c);
    x--;
    return int32(x >> 31);
}

// negative returns 1 if b < 0 and 0 otherwise.
private static int negative(int b) {
    return (b >> 31) & 1;
}

public static void PreComputedGroupElementCMove(ptr<PreComputedGroupElement> _addr_t, ptr<PreComputedGroupElement> _addr_u, int b) {
    ref PreComputedGroupElement t = ref _addr_t.val;
    ref PreComputedGroupElement u = ref _addr_u.val;

    FeCMove(_addr_t.yPlusX, _addr_u.yPlusX, b);
    FeCMove(_addr_t.yMinusX, _addr_u.yMinusX, b);
    FeCMove(_addr_t.xy2d, _addr_u.xy2d, b);
}

private static void selectPoint(ptr<PreComputedGroupElement> _addr_t, int pos, int b) {
    ref PreComputedGroupElement t = ref _addr_t.val;

    ref PreComputedGroupElement minusT = ref heap(out ptr<PreComputedGroupElement> _addr_minusT);
    var bNegative = negative(b);
    var bAbs = b - (((-bNegative) & b) << 1);

    t.Zero();
    for (var i = int32(0); i < 8; i++) {
        PreComputedGroupElementCMove(_addr_t, _addr_base[pos][i], equal(bAbs, i + 1));
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
public static void GeScalarMultBase(ptr<ExtendedGroupElement> _addr_h, ptr<array<byte>> _addr_a) {
    ref ExtendedGroupElement h = ref _addr_h.val;
    ref array<byte> a = ref _addr_a.val;

    array<sbyte> e = new array<sbyte>(64);

    {
        var i__prev1 = i;

        foreach (var (__i, __v) in a) {
            i = __i;
            v = __v;
            e[2 * i] = int8(v & 15);
            e[2 * i + 1] = int8((v >> 4) & 15);
        }
        i = i__prev1;
    }

    var carry = int8(0);
    {
        var i__prev1 = i;

        for (nint i = 0; i < 63; i++) {
            e[i] += carry;
            carry = (e[i] + 8) >> 4;
            e[i] -= carry << 4;
        }

        i = i__prev1;
    }
    e[63] += carry; 
    // each e[i] is between -8 and 8.

    h.Zero();
    ref PreComputedGroupElement t = ref heap(out ptr<PreComputedGroupElement> _addr_t);
    ref CompletedGroupElement r = ref heap(out ptr<CompletedGroupElement> _addr_r);
    {
        var i__prev1 = i;

        i = int32(1);

        while (i < 64) {
            selectPoint(_addr_t, i / 2, int32(e[i]));
            geMixedAdd(_addr_r, _addr_h, _addr_t);
            r.ToExtended(h);
            i += 2;
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

        i = int32(0);

        while (i < 64) {
            selectPoint(_addr_t, i / 2, int32(e[i]));
            geMixedAdd(_addr_r, _addr_h, _addr_t);
            r.ToExtended(h);
            i += 2;
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
public static void ScMulAdd(ptr<array<byte>> _addr_s, ptr<array<byte>> _addr_a, ptr<array<byte>> _addr_b, ptr<array<byte>> _addr_c) {
    ref array<byte> s = ref _addr_s.val;
    ref array<byte> a = ref _addr_a.val;
    ref array<byte> b = ref _addr_b.val;
    ref array<byte> c = ref _addr_c.val;

    nint a0 = 2097151 & load3(a[..]);
    nint a1 = 2097151 & (load4(a[(int)2..]) >> 5);
    nint a2 = 2097151 & (load3(a[(int)5..]) >> 2);
    nint a3 = 2097151 & (load4(a[(int)7..]) >> 7);
    nint a4 = 2097151 & (load4(a[(int)10..]) >> 4);
    nint a5 = 2097151 & (load3(a[(int)13..]) >> 1);
    nint a6 = 2097151 & (load4(a[(int)15..]) >> 6);
    nint a7 = 2097151 & (load3(a[(int)18..]) >> 3);
    nint a8 = 2097151 & load3(a[(int)21..]);
    nint a9 = 2097151 & (load4(a[(int)23..]) >> 5);
    nint a10 = 2097151 & (load3(a[(int)26..]) >> 2);
    var a11 = (load4(a[(int)28..]) >> 7);
    nint b0 = 2097151 & load3(b[..]);
    nint b1 = 2097151 & (load4(b[(int)2..]) >> 5);
    nint b2 = 2097151 & (load3(b[(int)5..]) >> 2);
    nint b3 = 2097151 & (load4(b[(int)7..]) >> 7);
    nint b4 = 2097151 & (load4(b[(int)10..]) >> 4);
    nint b5 = 2097151 & (load3(b[(int)13..]) >> 1);
    nint b6 = 2097151 & (load4(b[(int)15..]) >> 6);
    nint b7 = 2097151 & (load3(b[(int)18..]) >> 3);
    nint b8 = 2097151 & load3(b[(int)21..]);
    nint b9 = 2097151 & (load4(b[(int)23..]) >> 5);
    nint b10 = 2097151 & (load3(b[(int)26..]) >> 2);
    var b11 = (load4(b[(int)28..]) >> 7);
    nint c0 = 2097151 & load3(c[..]);
    nint c1 = 2097151 & (load4(c[(int)2..]) >> 5);
    nint c2 = 2097151 & (load3(c[(int)5..]) >> 2);
    nint c3 = 2097151 & (load4(c[(int)7..]) >> 7);
    nint c4 = 2097151 & (load4(c[(int)10..]) >> 4);
    nint c5 = 2097151 & (load3(c[(int)13..]) >> 1);
    nint c6 = 2097151 & (load4(c[(int)15..]) >> 6);
    nint c7 = 2097151 & (load3(c[(int)18..]) >> 3);
    nint c8 = 2097151 & load3(c[(int)21..]);
    nint c9 = 2097151 & (load4(c[(int)23..]) >> 5);
    nint c10 = 2097151 & (load3(c[(int)26..]) >> 2);
    var c11 = (load4(c[(int)28..]) >> 7);
    array<long> carry = new array<long>(23);

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
    var s23 = int64(0);

    carry[0] = (s0 + (1 << 20)) >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[2] = (s2 + (1 << 20)) >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[4] = (s4 + (1 << 20)) >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[12] = (s12 + (1 << 20)) >> 21;
    s13 += carry[12];
    s12 -= carry[12] << 21;
    carry[14] = (s14 + (1 << 20)) >> 21;
    s15 += carry[14];
    s14 -= carry[14] << 21;
    carry[16] = (s16 + (1 << 20)) >> 21;
    s17 += carry[16];
    s16 -= carry[16] << 21;
    carry[18] = (s18 + (1 << 20)) >> 21;
    s19 += carry[18];
    s18 -= carry[18] << 21;
    carry[20] = (s20 + (1 << 20)) >> 21;
    s21 += carry[20];
    s20 -= carry[20] << 21;
    carry[22] = (s22 + (1 << 20)) >> 21;
    s23 += carry[22];
    s22 -= carry[22] << 21;

    carry[1] = (s1 + (1 << 20)) >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[3] = (s3 + (1 << 20)) >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[5] = (s5 + (1 << 20)) >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;
    carry[13] = (s13 + (1 << 20)) >> 21;
    s14 += carry[13];
    s13 -= carry[13] << 21;
    carry[15] = (s15 + (1 << 20)) >> 21;
    s16 += carry[15];
    s15 -= carry[15] << 21;
    carry[17] = (s17 + (1 << 20)) >> 21;
    s18 += carry[17];
    s17 -= carry[17] << 21;
    carry[19] = (s19 + (1 << 20)) >> 21;
    s20 += carry[19];
    s19 -= carry[19] << 21;
    carry[21] = (s21 + (1 << 20)) >> 21;
    s22 += carry[21];
    s21 -= carry[21] << 21;

    s11 += s23 * 666643;
    s12 += s23 * 470296;
    s13 += s23 * 654183;
    s14 -= s23 * 997805;
    s15 += s23 * 136657;
    s16 -= s23 * 683901;
    s23 = 0;

    s10 += s22 * 666643;
    s11 += s22 * 470296;
    s12 += s22 * 654183;
    s13 -= s22 * 997805;
    s14 += s22 * 136657;
    s15 -= s22 * 683901;
    s22 = 0;

    s9 += s21 * 666643;
    s10 += s21 * 470296;
    s11 += s21 * 654183;
    s12 -= s21 * 997805;
    s13 += s21 * 136657;
    s14 -= s21 * 683901;
    s21 = 0;

    s8 += s20 * 666643;
    s9 += s20 * 470296;
    s10 += s20 * 654183;
    s11 -= s20 * 997805;
    s12 += s20 * 136657;
    s13 -= s20 * 683901;
    s20 = 0;

    s7 += s19 * 666643;
    s8 += s19 * 470296;
    s9 += s19 * 654183;
    s10 -= s19 * 997805;
    s11 += s19 * 136657;
    s12 -= s19 * 683901;
    s19 = 0;

    s6 += s18 * 666643;
    s7 += s18 * 470296;
    s8 += s18 * 654183;
    s9 -= s18 * 997805;
    s10 += s18 * 136657;
    s11 -= s18 * 683901;
    s18 = 0;

    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[12] = (s12 + (1 << 20)) >> 21;
    s13 += carry[12];
    s12 -= carry[12] << 21;
    carry[14] = (s14 + (1 << 20)) >> 21;
    s15 += carry[14];
    s14 -= carry[14] << 21;
    carry[16] = (s16 + (1 << 20)) >> 21;
    s17 += carry[16];
    s16 -= carry[16] << 21;

    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;
    carry[13] = (s13 + (1 << 20)) >> 21;
    s14 += carry[13];
    s13 -= carry[13] << 21;
    carry[15] = (s15 + (1 << 20)) >> 21;
    s16 += carry[15];
    s15 -= carry[15] << 21;

    s5 += s17 * 666643;
    s6 += s17 * 470296;
    s7 += s17 * 654183;
    s8 -= s17 * 997805;
    s9 += s17 * 136657;
    s10 -= s17 * 683901;
    s17 = 0;

    s4 += s16 * 666643;
    s5 += s16 * 470296;
    s6 += s16 * 654183;
    s7 -= s16 * 997805;
    s8 += s16 * 136657;
    s9 -= s16 * 683901;
    s16 = 0;

    s3 += s15 * 666643;
    s4 += s15 * 470296;
    s5 += s15 * 654183;
    s6 -= s15 * 997805;
    s7 += s15 * 136657;
    s8 -= s15 * 683901;
    s15 = 0;

    s2 += s14 * 666643;
    s3 += s14 * 470296;
    s4 += s14 * 654183;
    s5 -= s14 * 997805;
    s6 += s14 * 136657;
    s7 -= s14 * 683901;
    s14 = 0;

    s1 += s13 * 666643;
    s2 += s13 * 470296;
    s3 += s13 * 654183;
    s4 -= s13 * 997805;
    s5 += s13 * 136657;
    s6 -= s13 * 683901;
    s13 = 0;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = (s0 + (1 << 20)) >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[2] = (s2 + (1 << 20)) >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[4] = (s4 + (1 << 20)) >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    carry[1] = (s1 + (1 << 20)) >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[3] = (s3 + (1 << 20)) >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[5] = (s5 + (1 << 20)) >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[11] = s11 >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    s[0] = byte(s0 >> 0);
    s[1] = byte(s0 >> 8);
    s[2] = byte((s0 >> 16) | (s1 << 5));
    s[3] = byte(s1 >> 3);
    s[4] = byte(s1 >> 11);
    s[5] = byte((s1 >> 19) | (s2 << 2));
    s[6] = byte(s2 >> 6);
    s[7] = byte((s2 >> 14) | (s3 << 7));
    s[8] = byte(s3 >> 1);
    s[9] = byte(s3 >> 9);
    s[10] = byte((s3 >> 17) | (s4 << 4));
    s[11] = byte(s4 >> 4);
    s[12] = byte(s4 >> 12);
    s[13] = byte((s4 >> 20) | (s5 << 1));
    s[14] = byte(s5 >> 7);
    s[15] = byte((s5 >> 15) | (s6 << 6));
    s[16] = byte(s6 >> 2);
    s[17] = byte(s6 >> 10);
    s[18] = byte((s6 >> 18) | (s7 << 3));
    s[19] = byte(s7 >> 5);
    s[20] = byte(s7 >> 13);
    s[21] = byte(s8 >> 0);
    s[22] = byte(s8 >> 8);
    s[23] = byte((s8 >> 16) | (s9 << 5));
    s[24] = byte(s9 >> 3);
    s[25] = byte(s9 >> 11);
    s[26] = byte((s9 >> 19) | (s10 << 2));
    s[27] = byte(s10 >> 6);
    s[28] = byte((s10 >> 14) | (s11 << 7));
    s[29] = byte(s11 >> 1);
    s[30] = byte(s11 >> 9);
    s[31] = byte(s11 >> 17);
}

// Input:
//   s[0]+256*s[1]+...+256^63*s[63] = s
//
// Output:
//   s[0]+256*s[1]+...+256^31*s[31] = s mod l
//   where l = 2^252 + 27742317777372353535851937790883648493.
public static void ScReduce(ptr<array<byte>> _addr_@out, ptr<array<byte>> _addr_s) {
    ref array<byte> @out = ref _addr_@out.val;
    ref array<byte> s = ref _addr_s.val;

    nint s0 = 2097151 & load3(s[..]);
    nint s1 = 2097151 & (load4(s[(int)2..]) >> 5);
    nint s2 = 2097151 & (load3(s[(int)5..]) >> 2);
    nint s3 = 2097151 & (load4(s[(int)7..]) >> 7);
    nint s4 = 2097151 & (load4(s[(int)10..]) >> 4);
    nint s5 = 2097151 & (load3(s[(int)13..]) >> 1);
    nint s6 = 2097151 & (load4(s[(int)15..]) >> 6);
    nint s7 = 2097151 & (load3(s[(int)18..]) >> 3);
    nint s8 = 2097151 & load3(s[(int)21..]);
    nint s9 = 2097151 & (load4(s[(int)23..]) >> 5);
    nint s10 = 2097151 & (load3(s[(int)26..]) >> 2);
    nint s11 = 2097151 & (load4(s[(int)28..]) >> 7);
    nint s12 = 2097151 & (load4(s[(int)31..]) >> 4);
    nint s13 = 2097151 & (load3(s[(int)34..]) >> 1);
    nint s14 = 2097151 & (load4(s[(int)36..]) >> 6);
    nint s15 = 2097151 & (load3(s[(int)39..]) >> 3);
    nint s16 = 2097151 & load3(s[(int)42..]);
    nint s17 = 2097151 & (load4(s[(int)44..]) >> 5);
    nint s18 = 2097151 & (load3(s[(int)47..]) >> 2);
    nint s19 = 2097151 & (load4(s[(int)49..]) >> 7);
    nint s20 = 2097151 & (load4(s[(int)52..]) >> 4);
    nint s21 = 2097151 & (load3(s[(int)55..]) >> 1);
    nint s22 = 2097151 & (load4(s[(int)57..]) >> 6);
    var s23 = (load4(s[(int)60..]) >> 3);

    s11 += s23 * 666643;
    s12 += s23 * 470296;
    s13 += s23 * 654183;
    s14 -= s23 * 997805;
    s15 += s23 * 136657;
    s16 -= s23 * 683901;
    s23 = 0;

    s10 += s22 * 666643;
    s11 += s22 * 470296;
    s12 += s22 * 654183;
    s13 -= s22 * 997805;
    s14 += s22 * 136657;
    s15 -= s22 * 683901;
    s22 = 0;

    s9 += s21 * 666643;
    s10 += s21 * 470296;
    s11 += s21 * 654183;
    s12 -= s21 * 997805;
    s13 += s21 * 136657;
    s14 -= s21 * 683901;
    s21 = 0;

    s8 += s20 * 666643;
    s9 += s20 * 470296;
    s10 += s20 * 654183;
    s11 -= s20 * 997805;
    s12 += s20 * 136657;
    s13 -= s20 * 683901;
    s20 = 0;

    s7 += s19 * 666643;
    s8 += s19 * 470296;
    s9 += s19 * 654183;
    s10 -= s19 * 997805;
    s11 += s19 * 136657;
    s12 -= s19 * 683901;
    s19 = 0;

    s6 += s18 * 666643;
    s7 += s18 * 470296;
    s8 += s18 * 654183;
    s9 -= s18 * 997805;
    s10 += s18 * 136657;
    s11 -= s18 * 683901;
    s18 = 0;

    array<long> carry = new array<long>(17);

    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[12] = (s12 + (1 << 20)) >> 21;
    s13 += carry[12];
    s12 -= carry[12] << 21;
    carry[14] = (s14 + (1 << 20)) >> 21;
    s15 += carry[14];
    s14 -= carry[14] << 21;
    carry[16] = (s16 + (1 << 20)) >> 21;
    s17 += carry[16];
    s16 -= carry[16] << 21;

    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;
    carry[13] = (s13 + (1 << 20)) >> 21;
    s14 += carry[13];
    s13 -= carry[13] << 21;
    carry[15] = (s15 + (1 << 20)) >> 21;
    s16 += carry[15];
    s15 -= carry[15] << 21;

    s5 += s17 * 666643;
    s6 += s17 * 470296;
    s7 += s17 * 654183;
    s8 -= s17 * 997805;
    s9 += s17 * 136657;
    s10 -= s17 * 683901;
    s17 = 0;

    s4 += s16 * 666643;
    s5 += s16 * 470296;
    s6 += s16 * 654183;
    s7 -= s16 * 997805;
    s8 += s16 * 136657;
    s9 -= s16 * 683901;
    s16 = 0;

    s3 += s15 * 666643;
    s4 += s15 * 470296;
    s5 += s15 * 654183;
    s6 -= s15 * 997805;
    s7 += s15 * 136657;
    s8 -= s15 * 683901;
    s15 = 0;

    s2 += s14 * 666643;
    s3 += s14 * 470296;
    s4 += s14 * 654183;
    s5 -= s14 * 997805;
    s6 += s14 * 136657;
    s7 -= s14 * 683901;
    s14 = 0;

    s1 += s13 * 666643;
    s2 += s13 * 470296;
    s3 += s13 * 654183;
    s4 -= s13 * 997805;
    s5 += s13 * 136657;
    s6 -= s13 * 683901;
    s13 = 0;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = (s0 + (1 << 20)) >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[2] = (s2 + (1 << 20)) >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[4] = (s4 + (1 << 20)) >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[6] = (s6 + (1 << 20)) >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[8] = (s8 + (1 << 20)) >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[10] = (s10 + (1 << 20)) >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    carry[1] = (s1 + (1 << 20)) >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[3] = (s3 + (1 << 20)) >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[5] = (s5 + (1 << 20)) >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[7] = (s7 + (1 << 20)) >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[9] = (s9 + (1 << 20)) >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[11] = (s11 + (1 << 20)) >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;
    carry[11] = s11 >> 21;
    s12 += carry[11];
    s11 -= carry[11] << 21;

    s0 += s12 * 666643;
    s1 += s12 * 470296;
    s2 += s12 * 654183;
    s3 -= s12 * 997805;
    s4 += s12 * 136657;
    s5 -= s12 * 683901;
    s12 = 0;

    carry[0] = s0 >> 21;
    s1 += carry[0];
    s0 -= carry[0] << 21;
    carry[1] = s1 >> 21;
    s2 += carry[1];
    s1 -= carry[1] << 21;
    carry[2] = s2 >> 21;
    s3 += carry[2];
    s2 -= carry[2] << 21;
    carry[3] = s3 >> 21;
    s4 += carry[3];
    s3 -= carry[3] << 21;
    carry[4] = s4 >> 21;
    s5 += carry[4];
    s4 -= carry[4] << 21;
    carry[5] = s5 >> 21;
    s6 += carry[5];
    s5 -= carry[5] << 21;
    carry[6] = s6 >> 21;
    s7 += carry[6];
    s6 -= carry[6] << 21;
    carry[7] = s7 >> 21;
    s8 += carry[7];
    s7 -= carry[7] << 21;
    carry[8] = s8 >> 21;
    s9 += carry[8];
    s8 -= carry[8] << 21;
    carry[9] = s9 >> 21;
    s10 += carry[9];
    s9 -= carry[9] << 21;
    carry[10] = s10 >> 21;
    s11 += carry[10];
    s10 -= carry[10] << 21;

    out[0] = byte(s0 >> 0);
    out[1] = byte(s0 >> 8);
    out[2] = byte((s0 >> 16) | (s1 << 5));
    out[3] = byte(s1 >> 3);
    out[4] = byte(s1 >> 11);
    out[5] = byte((s1 >> 19) | (s2 << 2));
    out[6] = byte(s2 >> 6);
    out[7] = byte((s2 >> 14) | (s3 << 7));
    out[8] = byte(s3 >> 1);
    out[9] = byte(s3 >> 9);
    out[10] = byte((s3 >> 17) | (s4 << 4));
    out[11] = byte(s4 >> 4);
    out[12] = byte(s4 >> 12);
    out[13] = byte((s4 >> 20) | (s5 << 1));
    out[14] = byte(s5 >> 7);
    out[15] = byte((s5 >> 15) | (s6 << 6));
    out[16] = byte(s6 >> 2);
    out[17] = byte(s6 >> 10);
    out[18] = byte((s6 >> 18) | (s7 << 3));
    out[19] = byte(s7 >> 5);
    out[20] = byte(s7 >> 13);
    out[21] = byte(s8 >> 0);
    out[22] = byte(s8 >> 8);
    out[23] = byte((s8 >> 16) | (s9 << 5));
    out[24] = byte(s9 >> 3);
    out[25] = byte(s9 >> 11);
    out[26] = byte((s9 >> 19) | (s10 << 2));
    out[27] = byte(s10 >> 6);
    out[28] = byte((s10 >> 14) | (s11 << 7));
    out[29] = byte(s11 >> 1);
    out[30] = byte(s11 >> 9);
    out[31] = byte(s11 >> 17);
}

// order is the order of Curve25519 in little-endian form.
private static array<ulong> order = new array<ulong>(new ulong[] { 0x5812631a5cf5d3ed, 0x14def9dea2f79cd6, 0, 0x1000000000000000 });

// ScMinimal returns true if the given scalar is less than the order of the
// curve.
public static bool ScMinimal(ptr<array<byte>> _addr_scalar) {
    ref array<byte> scalar = ref _addr_scalar.val;

    for (nint i = 3; ; i--) {
        var v = binary.LittleEndian.Uint64(scalar[(int)i * 8..]);
        if (v > order[i]) {
            return false;
        }
        else if (v < order[i]) {
            break;
        }
        else if (i == 0) {
            return false;
        }
    }

    return true;
}

} // end edwards25519_package
