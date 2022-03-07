// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 && gc && !purego
// +build amd64,gc,!purego

// package curve25519 -- go2cs converted at 2022 March 06 23:36:40 UTC
// import "vendor/golang.org/x/crypto/curve25519" ==> using curve25519 = go.vendor.golang.org.x.crypto.curve25519_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\curve25519\curve25519_amd64.go


namespace go.vendor.golang.org.x.crypto;

public static partial class curve25519_package {

    // These functions are implemented in the .s files. The names of the functions
    // in the rest of the file are also taken from the SUPERCOP sources to help
    // people following along.

    //go:noescape
private static void cswap(ptr<array<ulong>> inout, ulong v);

//go:noescape

private static void ladderstep(ptr<array<array<ulong>>> inout);

//go:noescape

private static void freeze(ptr<array<ulong>> inout);

//go:noescape

private static void mul(ptr<array<ulong>> dest, ptr<array<ulong>> a, ptr<array<ulong>> b);

//go:noescape

private static void square(ptr<array<ulong>> @out, ptr<array<ulong>> @in);

// mladder uses a Montgomery ladder to calculate (xr/zr) *= s.
private static void mladder(ptr<array<ulong>> _addr_xr, ptr<array<ulong>> _addr_zr, ptr<array<byte>> _addr_s) {
    ref array<ulong> xr = ref _addr_xr.val;
    ref array<ulong> zr = ref _addr_zr.val;
    ref array<byte> s = ref _addr_s.val;

    ref array<array<ulong>> work = ref heap(new array<array<ulong>>(5), out ptr<array<array<ulong>>> _addr_work);

    work[0] = xr;
    setint(_addr_work[1], 1);
    setint(_addr_work[2], 0);
    work[3] = xr;
    setint(_addr_work[4], 1);

    var j = uint(6);
    byte prevbit = default;

    for (nint i = 31; i >= 0; i--) {>>MARKER:FUNCTION_square_BLOCK_PREFIX<<
        while (j < 8) {>>MARKER:FUNCTION_mul_BLOCK_PREFIX<<
            array<array<byte>> bit = ((s)[i] >> (int)(j)) & 1;
            var swap = bit ^ prevbit;
            prevbit = bit;
            cswap(_addr_work[1], uint64(swap));
            ladderstep(_addr_work);
            j--;
        }
        j = 7;
    }

    xr = work[1];
    zr = work[2];
}

private static void scalarMult(ptr<array<byte>> _addr_@out, ptr<array<byte>> _addr_@in, ptr<array<byte>> _addr_@base) {
    ref array<byte> @out = ref _addr_@out.val;
    ref array<byte> @in = ref _addr_@in.val;
    ref array<byte> @base = ref _addr_@base.val;

    ref array<byte> e = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_e);
    copy(e[..], (in.val)[..]);
    e[0] &= 248;
    e[31] &= 127;
    e[31] |= 64;

    ref array<ulong> t = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_t);    ref array<ulong> z = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z);

    unpack(_addr_t, _addr_base);
    mladder(_addr_t, _addr_z, _addr_e);
    invert(_addr_z, _addr_z);
    mul(_addr_t, _addr_t, _addr_z);
    pack(_addr_out, _addr_t);
}

private static void setint(ptr<array<ulong>> _addr_r, ulong v) {
    ref array<ulong> r = ref _addr_r.val;

    r[0] = v;
    r[1] = 0;
    r[2] = 0;
    r[3] = 0;
    r[4] = 0;
}

// unpack sets r = x where r consists of 5, 51-bit limbs in little-endian
// order.
private static void unpack(ptr<array<ulong>> _addr_r, ptr<array<byte>> _addr_x) {
    ref array<ulong> r = ref _addr_r.val;
    ref array<byte> x = ref _addr_x.val;

    r[0] = uint64(x[0]) | uint64(x[1]) << 8 | uint64(x[2]) << 16 | uint64(x[3]) << 24 | uint64(x[4]) << 32 | uint64(x[5]) << 40 | uint64(x[6] & 7) << 48;

    r[1] = uint64(x[6]) >> 3 | uint64(x[7]) << 5 | uint64(x[8]) << 13 | uint64(x[9]) << 21 | uint64(x[10]) << 29 | uint64(x[11]) << 37 | uint64(x[12] & 63) << 45;

    r[2] = uint64(x[12]) >> 6 | uint64(x[13]) << 2 | uint64(x[14]) << 10 | uint64(x[15]) << 18 | uint64(x[16]) << 26 | uint64(x[17]) << 34 | uint64(x[18]) << 42 | uint64(x[19] & 1) << 50;

    r[3] = uint64(x[19]) >> 1 | uint64(x[20]) << 7 | uint64(x[21]) << 15 | uint64(x[22]) << 23 | uint64(x[23]) << 31 | uint64(x[24]) << 39 | uint64(x[25] & 15) << 47;

    r[4] = uint64(x[25]) >> 4 | uint64(x[26]) << 4 | uint64(x[27]) << 12 | uint64(x[28]) << 20 | uint64(x[29]) << 28 | uint64(x[30]) << 36 | uint64(x[31] & 127) << 44;
}

// pack sets out = x where out is the usual, little-endian form of the 5,
// 51-bit limbs in x.
private static void pack(ptr<array<byte>> _addr_@out, ptr<array<ulong>> _addr_x) {
    ref array<byte> @out = ref _addr_@out.val;
    ref array<ulong> x = ref _addr_x.val;

    ref array<array<ulong>> t = ref heap(x, out ptr<array<array<ulong>>> _addr_t);
    freeze(_addr_t);

    out[0] = byte(t[0]);
    out[1] = byte(t[0] >> 8);
    out[2] = byte(t[0] >> 16);
    out[3] = byte(t[0] >> 24);
    out[4] = byte(t[0] >> 32);
    out[5] = byte(t[0] >> 40);
    out[6] = byte(t[0] >> 48);

    out[6] ^= byte(t[1] << 3) & 0xf8;
    out[7] = byte(t[1] >> 5);
    out[8] = byte(t[1] >> 13);
    out[9] = byte(t[1] >> 21);
    out[10] = byte(t[1] >> 29);
    out[11] = byte(t[1] >> 37);
    out[12] = byte(t[1] >> 45);

    out[12] ^= byte(t[2] << 6) & 0xc0;
    out[13] = byte(t[2] >> 2);
    out[14] = byte(t[2] >> 10);
    out[15] = byte(t[2] >> 18);
    out[16] = byte(t[2] >> 26);
    out[17] = byte(t[2] >> 34);
    out[18] = byte(t[2] >> 42);
    out[19] = byte(t[2] >> 50);

    out[19] ^= byte(t[3] << 1) & 0xfe;
    out[20] = byte(t[3] >> 7);
    out[21] = byte(t[3] >> 15);
    out[22] = byte(t[3] >> 23);
    out[23] = byte(t[3] >> 31);
    out[24] = byte(t[3] >> 39);
    out[25] = byte(t[3] >> 47);

    out[25] ^= byte(t[4] << 4) & 0xf0;
    out[26] = byte(t[4] >> 4);
    out[27] = byte(t[4] >> 12);
    out[28] = byte(t[4] >> 20);
    out[29] = byte(t[4] >> 28);
    out[30] = byte(t[4] >> 36);
    out[31] = byte(t[4] >> 44);
}

// invert calculates r = x^-1 mod p using Fermat's little theorem.
private static void invert(ptr<array<ulong>> _addr_r, ptr<array<ulong>> _addr_x) {
    ref array<ulong> r = ref _addr_r.val;
    ref array<ulong> x = ref _addr_x.val;

    ref array<ulong> z2 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z2);    ref array<ulong> z9 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z9);    ref array<ulong> z11 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z11);    ref array<ulong> z2_5_0 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z2_5_0);    ref array<ulong> z2_10_0 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z2_10_0);    ref array<ulong> z2_20_0 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z2_20_0);    ref array<ulong> z2_50_0 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z2_50_0);    ref array<ulong> z2_100_0 = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_z2_100_0);    ref array<ulong> t = ref heap(new array<ulong>(5), out ptr<array<ulong>> _addr_t);



    square(_addr_z2, _addr_x); /* 2 */
    square(_addr_t, _addr_z2); /* 4 */
    square(_addr_t, _addr_t); /* 8 */
    mul(_addr_z9, _addr_t, _addr_x); /* 9 */
    mul(_addr_z11, _addr_z9, _addr_z2); /* 11 */
    square(_addr_t, _addr_z11); /* 22 */
    mul(_addr_z2_5_0, _addr_t, _addr_z9);    /* 2^5 - 2^0 = 31 */

    square(_addr_t, _addr_z2_5_0); /* 2^6 - 2^1 */
    {
        nint i__prev1 = i;

        for (nint i = 1; i < 5; i++) {>>MARKER:FUNCTION_freeze_BLOCK_PREFIX<< /* 2^20 - 2^10 */
            square(_addr_t, _addr_t);

        }

        i = i__prev1;
    }
    mul(_addr_z2_10_0, _addr_t, _addr_z2_5_0);    /* 2^10 - 2^0 */

    square(_addr_t, _addr_z2_10_0); /* 2^11 - 2^1 */
    {
        nint i__prev1 = i;

        for (i = 1; i < 10; i++) {>>MARKER:FUNCTION_ladderstep_BLOCK_PREFIX<< /* 2^20 - 2^10 */
            square(_addr_t, _addr_t);

        }

        i = i__prev1;
    }
    mul(_addr_z2_20_0, _addr_t, _addr_z2_10_0);    /* 2^20 - 2^0 */

    square(_addr_t, _addr_z2_20_0); /* 2^21 - 2^1 */
    {
        nint i__prev1 = i;

        for (i = 1; i < 20; i++) {>>MARKER:FUNCTION_cswap_BLOCK_PREFIX<< /* 2^40 - 2^20 */
            square(_addr_t, _addr_t);

        }

        i = i__prev1;
    }
    mul(_addr_t, _addr_t, _addr_z2_20_0);    /* 2^40 - 2^0 */

    square(_addr_t, _addr_t); /* 2^41 - 2^1 */
    {
        nint i__prev1 = i;

        for (i = 1; i < 10; i++) { /* 2^50 - 2^10 */
            square(_addr_t, _addr_t);

        }

        i = i__prev1;
    }
    mul(_addr_z2_50_0, _addr_t, _addr_z2_10_0);    /* 2^50 - 2^0 */

    square(_addr_t, _addr_z2_50_0); /* 2^51 - 2^1 */
    {
        nint i__prev1 = i;

        for (i = 1; i < 50; i++) { /* 2^100 - 2^50 */
            square(_addr_t, _addr_t);

        }

        i = i__prev1;
    }
    mul(_addr_z2_100_0, _addr_t, _addr_z2_50_0);    /* 2^100 - 2^0 */

    square(_addr_t, _addr_z2_100_0); /* 2^101 - 2^1 */
    {
        nint i__prev1 = i;

        for (i = 1; i < 100; i++) { /* 2^200 - 2^100 */
            square(_addr_t, _addr_t);

        }

        i = i__prev1;
    }
    mul(_addr_t, _addr_t, _addr_z2_100_0);    /* 2^200 - 2^0 */

    square(_addr_t, _addr_t); /* 2^201 - 2^1 */
    {
        nint i__prev1 = i;

        for (i = 1; i < 50; i++) { /* 2^250 - 2^50 */
            square(_addr_t, _addr_t);

        }

        i = i__prev1;
    }
    mul(_addr_t, _addr_t, _addr_z2_50_0);    /* 2^250 - 2^0 */

    square(_addr_t, _addr_t); /* 2^251 - 2^1 */
    square(_addr_t, _addr_t); /* 2^252 - 2^2 */
    square(_addr_t, _addr_t);    /* 2^253 - 2^3 */

    square(_addr_t, _addr_t);    /* 2^254 - 2^4 */

    square(_addr_t, _addr_t); /* 2^255 - 2^5 */
    mul(_addr_r, _addr_t, _addr_z11); /* 2^255 - 21 */
}

} // end curve25519_package
