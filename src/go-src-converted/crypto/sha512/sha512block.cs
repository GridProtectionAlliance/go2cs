// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// SHA512 block step.
// In its own file so that a faster assembly or C version
// can be substituted easily.

// package sha512 -- go2cs converted at 2020 August 29 08:29:50 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Go\src\crypto\sha512\sha512block.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha512_package
    {
        private static ulong _K = new slice<ulong>(new ulong[] { 0x428a2f98d728ae22, 0x7137449123ef65cd, 0xb5c0fbcfec4d3b2f, 0xe9b5dba58189dbbc, 0x3956c25bf348b538, 0x59f111f1b605d019, 0x923f82a4af194f9b, 0xab1c5ed5da6d8118, 0xd807aa98a3030242, 0x12835b0145706fbe, 0x243185be4ee4b28c, 0x550c7dc3d5ffb4e2, 0x72be5d74f27b896f, 0x80deb1fe3b1696b1, 0x9bdc06a725c71235, 0xc19bf174cf692694, 0xe49b69c19ef14ad2, 0xefbe4786384f25e3, 0x0fc19dc68b8cd5b5, 0x240ca1cc77ac9c65, 0x2de92c6f592b0275, 0x4a7484aa6ea6e483, 0x5cb0a9dcbd41fbd4, 0x76f988da831153b5, 0x983e5152ee66dfab, 0xa831c66d2db43210, 0xb00327c898fb213f, 0xbf597fc7beef0ee4, 0xc6e00bf33da88fc2, 0xd5a79147930aa725, 0x06ca6351e003826f, 0x142929670a0e6e70, 0x27b70a8546d22ffc, 0x2e1b21385c26c926, 0x4d2c6dfc5ac42aed, 0x53380d139d95b3df, 0x650a73548baf63de, 0x766a0abb3c77b2a8, 0x81c2c92e47edaee6, 0x92722c851482353b, 0xa2bfe8a14cf10364, 0xa81a664bbc423001, 0xc24b8b70d0f89791, 0xc76c51a30654be30, 0xd192e819d6ef5218, 0xd69906245565a910, 0xf40e35855771202a, 0x106aa07032bbd1b8, 0x19a4c116b8d2d0c8, 0x1e376c085141ab53, 0x2748774cdf8eeb99, 0x34b0bcb5e19b48a8, 0x391c0cb3c5c95a63, 0x4ed8aa4ae3418acb, 0x5b9cca4f7763e373, 0x682e6ff3d6b2b8a3, 0x748f82ee5defb2fc, 0x78a5636f43172f60, 0x84c87814a1f0ab72, 0x8cc702081a6439ec, 0x90befffa23631e28, 0xa4506cebde82bde9, 0xbef9a3f7b2c67915, 0xc67178f2e372532b, 0xca273eceea26619c, 0xd186b8c721c0c207, 0xeada7dd6cde0eb1e, 0xf57d4f7fee6ed178, 0x06f067aa72176fba, 0x0a637dc5a2c898a6, 0x113f9804bef90dae, 0x1b710b35131c471b, 0x28db77f523047d84, 0x32caab7b40c72493, 0x3c9ebe0a15c9bebc, 0x431d67c49c100d4c, 0x4cc5d4becb3e42b6, 0x597f299cfc657e2a, 0x5fcb6fab3ad6faec, 0x6c44198c4a475817 });

        private static void blockGeneric(ref digest dig, slice<byte> p)
        {
            array<ulong> w = new array<ulong>(80L);
            var h0 = dig.h[0L];
            var h1 = dig.h[1L];
            var h2 = dig.h[2L];
            var h3 = dig.h[3L];
            var h4 = dig.h[4L];
            var h5 = dig.h[5L];
            var h6 = dig.h[6L];
            var h7 = dig.h[7L];
            while (len(p) >= chunk)
            {
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < 16L; i++)
                    {
                        var j = i * 8L;
                        w[i] = uint64(p[j]) << (int)(56L) | uint64(p[j + 1L]) << (int)(48L) | uint64(p[j + 2L]) << (int)(40L) | uint64(p[j + 3L]) << (int)(32L) | uint64(p[j + 4L]) << (int)(24L) | uint64(p[j + 5L]) << (int)(16L) | uint64(p[j + 6L]) << (int)(8L) | uint64(p[j + 7L]);
                    }


                    i = i__prev2;
                }
                {
                    long i__prev2 = i;

                    for (i = 16L; i < 80L; i++)
                    {
                        var v1 = w[i - 2L];
                        var t1 = (v1 >> (int)(19L) | v1 << (int)((64L - 19L))) ^ (v1 >> (int)(61L) | v1 << (int)((64L - 61L))) ^ (v1 >> (int)(6L));
                        var v2 = w[i - 15L];
                        var t2 = (v2 >> (int)(1L) | v2 << (int)((64L - 1L))) ^ (v2 >> (int)(8L) | v2 << (int)((64L - 8L))) ^ (v2 >> (int)(7L));

                        w[i] = t1 + w[i - 7L] + t2 + w[i - 16L];
                    }


                    i = i__prev2;
                }

                var a = h0;
                var b = h1;
                var c = h2;
                var d = h3;
                var e = h4;
                var f = h5;
                var g = h6;
                var h = h7;

                {
                    long i__prev2 = i;

                    for (i = 0L; i < 80L; i++)
                    {
                        t1 = h + ((e >> (int)(14L) | e << (int)((64L - 14L))) ^ (e >> (int)(18L) | e << (int)((64L - 18L))) ^ (e >> (int)(41L) | e << (int)((64L - 41L)))) + ((e & f) ^ (~e & g)) + _K[i] + w[i];

                        t2 = ((a >> (int)(28L) | a << (int)((64L - 28L))) ^ (a >> (int)(34L) | a << (int)((64L - 34L))) ^ (a >> (int)(39L) | a << (int)((64L - 39L)))) + ((a & b) ^ (a & c) ^ (b & c));

                        h = g;
                        g = f;
                        f = e;
                        e = d + t1;
                        d = c;
                        c = b;
                        b = a;
                        a = t1 + t2;
                    }


                    i = i__prev2;
                }

                h0 += a;
                h1 += b;
                h2 += c;
                h3 += d;
                h4 += e;
                h5 += f;
                h6 += g;
                h7 += h;

                p = p[chunk..];
            }


            dig.h[0L] = h0;
            dig.h[1L] = h1;
            dig.h[2L] = h2;
            dig.h[3L] = h3;
            dig.h[4L] = h4;
            dig.h[5L] = h5;
            dig.h[6L] = h6;
            dig.h[7L] = h7;
        }
    }
}}
