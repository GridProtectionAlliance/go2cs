// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64,!gccgo,!appengine

// package curve25519 -- go2cs converted at 2020 August 29 10:11:36 UTC
// import "vendor/golang_org/x/crypto/curve25519" ==> using curve25519 = go.vendor.golang_org.x.crypto.curve25519_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\curve25519\mont25519_amd64.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class curve25519_package
    {
        // These functions are implemented in the .s files. The names of the functions
        // in the rest of the file are also taken from the SUPERCOP sources to help
        // people following along.

        //go:noescape
        private static void cswap(ref array<ulong> inout, ulong v)
;

        //go:noescape

        private static void ladderstep(ref array<array<ulong>> inout)
;

        //go:noescape

        private static void freeze(ref array<ulong> inout)
;

        //go:noescape

        private static void mul(ref array<ulong> dest, ref array<ulong> a, ref array<ulong> b)
;

        //go:noescape

        private static void square(ref array<ulong> @out, ref array<ulong> @in)
;

        // mladder uses a Montgomery ladder to calculate (xr/zr) *= s.
        private static void mladder(ref array<ulong> xr, ref array<ulong> zr, ref array<byte> s)
        {
            array<array<ulong>> work = new array<array<ulong>>(5L);

            work[0L] = xr.Value;
            setint(ref work[1L], 1L);
            setint(ref work[2L], 0L);
            work[3L] = xr.Value;
            setint(ref work[4L], 1L);

            var j = uint(6L);
            byte prevbit = default;

            for (long i = 31L; i >= 0L; i--)
            {>>MARKER:FUNCTION_square_BLOCK_PREFIX<<
                while (j < 8L)
                {>>MARKER:FUNCTION_mul_BLOCK_PREFIX<<
                    var bit = ((s.Value)[i] >> (int)(j)) & 1L;
                    var swap = bit ^ prevbit;
                    prevbit = bit;
                    cswap(ref work[1L], uint64(swap));
                    ladderstep(ref work);
                    j--;
                }

                j = 7L;
            }


            xr.Value = work[1L];
            zr.Value = work[2L];
        }

        private static void scalarMult(ref array<byte> @out, ref array<byte> @in, ref array<byte> @base)
        {
            array<byte> e = new array<byte>(32L);
            copy(e[..], (in.Value)[..]);
            e[0L] &= 248L;
            e[31L] &= 127L;
            e[31L] |= 64L;

            array<ulong> t = new array<ulong>(5L);            array<ulong> z = new array<ulong>(5L);

            unpack(ref t, base);
            mladder(ref t, ref z, ref e);
            invert(ref z, ref z);
            mul(ref t, ref t, ref z);
            pack(out, ref t);
        }

        private static void setint(ref array<ulong> r, ulong v)
        {
            r[0L] = v;
            r[1L] = 0L;
            r[2L] = 0L;
            r[3L] = 0L;
            r[4L] = 0L;
        }

        // unpack sets r = x where r consists of 5, 51-bit limbs in little-endian
        // order.
        private static void unpack(ref array<ulong> r, ref array<byte> x)
        {
            r[0L] = uint64(x[0L]) | uint64(x[1L]) << (int)(8L) | uint64(x[2L]) << (int)(16L) | uint64(x[3L]) << (int)(24L) | uint64(x[4L]) << (int)(32L) | uint64(x[5L]) << (int)(40L) | uint64(x[6L] & 7L) << (int)(48L);

            r[1L] = uint64(x[6L]) >> (int)(3L) | uint64(x[7L]) << (int)(5L) | uint64(x[8L]) << (int)(13L) | uint64(x[9L]) << (int)(21L) | uint64(x[10L]) << (int)(29L) | uint64(x[11L]) << (int)(37L) | uint64(x[12L] & 63L) << (int)(45L);

            r[2L] = uint64(x[12L]) >> (int)(6L) | uint64(x[13L]) << (int)(2L) | uint64(x[14L]) << (int)(10L) | uint64(x[15L]) << (int)(18L) | uint64(x[16L]) << (int)(26L) | uint64(x[17L]) << (int)(34L) | uint64(x[18L]) << (int)(42L) | uint64(x[19L] & 1L) << (int)(50L);

            r[3L] = uint64(x[19L]) >> (int)(1L) | uint64(x[20L]) << (int)(7L) | uint64(x[21L]) << (int)(15L) | uint64(x[22L]) << (int)(23L) | uint64(x[23L]) << (int)(31L) | uint64(x[24L]) << (int)(39L) | uint64(x[25L] & 15L) << (int)(47L);

            r[4L] = uint64(x[25L]) >> (int)(4L) | uint64(x[26L]) << (int)(4L) | uint64(x[27L]) << (int)(12L) | uint64(x[28L]) << (int)(20L) | uint64(x[29L]) << (int)(28L) | uint64(x[30L]) << (int)(36L) | uint64(x[31L] & 127L) << (int)(44L);
        }

        // pack sets out = x where out is the usual, little-endian form of the 5,
        // 51-bit limbs in x.
        private static void pack(ref array<byte> @out, ref array<ulong> x)
        {
            var t = x.Value;
            freeze(ref t);

            out[0L] = byte(t[0L]);
            out[1L] = byte(t[0L] >> (int)(8L));
            out[2L] = byte(t[0L] >> (int)(16L));
            out[3L] = byte(t[0L] >> (int)(24L));
            out[4L] = byte(t[0L] >> (int)(32L));
            out[5L] = byte(t[0L] >> (int)(40L));
            out[6L] = byte(t[0L] >> (int)(48L));

            out[6L] ^= byte(t[1L] << (int)(3L)) & 0xf8UL;
            out[7L] = byte(t[1L] >> (int)(5L));
            out[8L] = byte(t[1L] >> (int)(13L));
            out[9L] = byte(t[1L] >> (int)(21L));
            out[10L] = byte(t[1L] >> (int)(29L));
            out[11L] = byte(t[1L] >> (int)(37L));
            out[12L] = byte(t[1L] >> (int)(45L));

            out[12L] ^= byte(t[2L] << (int)(6L)) & 0xc0UL;
            out[13L] = byte(t[2L] >> (int)(2L));
            out[14L] = byte(t[2L] >> (int)(10L));
            out[15L] = byte(t[2L] >> (int)(18L));
            out[16L] = byte(t[2L] >> (int)(26L));
            out[17L] = byte(t[2L] >> (int)(34L));
            out[18L] = byte(t[2L] >> (int)(42L));
            out[19L] = byte(t[2L] >> (int)(50L));

            out[19L] ^= byte(t[3L] << (int)(1L)) & 0xfeUL;
            out[20L] = byte(t[3L] >> (int)(7L));
            out[21L] = byte(t[3L] >> (int)(15L));
            out[22L] = byte(t[3L] >> (int)(23L));
            out[23L] = byte(t[3L] >> (int)(31L));
            out[24L] = byte(t[3L] >> (int)(39L));
            out[25L] = byte(t[3L] >> (int)(47L));

            out[25L] ^= byte(t[4L] << (int)(4L)) & 0xf0UL;
            out[26L] = byte(t[4L] >> (int)(4L));
            out[27L] = byte(t[4L] >> (int)(12L));
            out[28L] = byte(t[4L] >> (int)(20L));
            out[29L] = byte(t[4L] >> (int)(28L));
            out[30L] = byte(t[4L] >> (int)(36L));
            out[31L] = byte(t[4L] >> (int)(44L));
        }

        // invert calculates r = x^-1 mod p using Fermat's little theorem.
        private static void invert(ref array<ulong> r, ref array<ulong> x)
        {
            array<ulong> z2 = new array<ulong>(5L);            array<ulong> z9 = new array<ulong>(5L);            array<ulong> z11 = new array<ulong>(5L);            array<ulong> z2_5_0 = new array<ulong>(5L);            array<ulong> z2_10_0 = new array<ulong>(5L);            array<ulong> z2_20_0 = new array<ulong>(5L);            array<ulong> z2_50_0 = new array<ulong>(5L);            array<ulong> z2_100_0 = new array<ulong>(5L);            array<ulong> t = new array<ulong>(5L);



            square(ref z2, x); /* 2 */
            square(ref t, ref z2); /* 4 */
            square(ref t, ref t); /* 8 */
            mul(ref z9, ref t, x); /* 9 */
            mul(ref z11, ref z9, ref z2); /* 11 */
            square(ref t, ref z11); /* 22 */
            mul(ref z2_5_0, ref t, ref z9);            /* 2^5 - 2^0 = 31 */

            square(ref t, ref z2_5_0); /* 2^6 - 2^1 */
            {
                long i__prev1 = i;

                for (long i = 1L; i < 5L; i++)
                {>>MARKER:FUNCTION_freeze_BLOCK_PREFIX<< /* 2^20 - 2^10 */
                    square(ref t, ref t);
                }


                i = i__prev1;
            }
            mul(ref z2_10_0, ref t, ref z2_5_0);            /* 2^10 - 2^0 */

            square(ref t, ref z2_10_0); /* 2^11 - 2^1 */
            {
                long i__prev1 = i;

                for (i = 1L; i < 10L; i++)
                {>>MARKER:FUNCTION_ladderstep_BLOCK_PREFIX<< /* 2^20 - 2^10 */
                    square(ref t, ref t);
                }


                i = i__prev1;
            }
            mul(ref z2_20_0, ref t, ref z2_10_0);            /* 2^20 - 2^0 */

            square(ref t, ref z2_20_0); /* 2^21 - 2^1 */
            {
                long i__prev1 = i;

                for (i = 1L; i < 20L; i++)
                {>>MARKER:FUNCTION_cswap_BLOCK_PREFIX<< /* 2^40 - 2^20 */
                    square(ref t, ref t);
                }


                i = i__prev1;
            }
            mul(ref t, ref t, ref z2_20_0);            /* 2^40 - 2^0 */

            square(ref t, ref t); /* 2^41 - 2^1 */
            {
                long i__prev1 = i;

                for (i = 1L; i < 10L; i++)
                { /* 2^50 - 2^10 */
                    square(ref t, ref t);
                }


                i = i__prev1;
            }
            mul(ref z2_50_0, ref t, ref z2_10_0);            /* 2^50 - 2^0 */

            square(ref t, ref z2_50_0); /* 2^51 - 2^1 */
            {
                long i__prev1 = i;

                for (i = 1L; i < 50L; i++)
                { /* 2^100 - 2^50 */
                    square(ref t, ref t);
                }


                i = i__prev1;
            }
            mul(ref z2_100_0, ref t, ref z2_50_0);            /* 2^100 - 2^0 */

            square(ref t, ref z2_100_0); /* 2^101 - 2^1 */
            {
                long i__prev1 = i;

                for (i = 1L; i < 100L; i++)
                { /* 2^200 - 2^100 */
                    square(ref t, ref t);
                }


                i = i__prev1;
            }
            mul(ref t, ref t, ref z2_100_0);            /* 2^200 - 2^0 */

            square(ref t, ref t); /* 2^201 - 2^1 */
            {
                long i__prev1 = i;

                for (i = 1L; i < 50L; i++)
                { /* 2^250 - 2^50 */
                    square(ref t, ref t);
                }


                i = i__prev1;
            }
            mul(ref t, ref t, ref z2_50_0);            /* 2^250 - 2^0 */

            square(ref t, ref t); /* 2^251 - 2^1 */
            square(ref t, ref t); /* 2^252 - 2^2 */
            square(ref t, ref t);            /* 2^253 - 2^3 */

            square(ref t, ref t);            /* 2^254 - 2^4 */

            square(ref t, ref t); /* 2^255 - 2^5 */
            mul(r, ref t, ref z11); /* 2^255 - 21 */
        }
    }
}}}}}
