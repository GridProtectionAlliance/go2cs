// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// SHA256 block step.
// In its own file so that a faster assembly or C version
// can be substituted easily.

// package sha256 -- go2cs converted at 2020 August 29 08:31:01 UTC
// import "crypto/sha256" ==> using sha256 = go.crypto.sha256_package
// Original source: C:\Go\src\crypto\sha256\sha256block.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha256_package
    {
        private static uint _K = new slice<uint>(new uint[] { 0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5, 0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174, 0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da, 0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967, 0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85, 0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070, 0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3, 0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2 });

        private static void blockGeneric(ref digest dig, slice<byte> p)
        {
            array<uint> w = new array<uint>(64L);
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
                // Can interlace the computation of w with the
                // rounds below if needed for speed.
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < 16L; i++)
                    {
                        var j = i * 4L;
                        w[i] = uint32(p[j]) << (int)(24L) | uint32(p[j + 1L]) << (int)(16L) | uint32(p[j + 2L]) << (int)(8L) | uint32(p[j + 3L]);
                    }


                    i = i__prev2;
                }
                {
                    long i__prev2 = i;

                    for (i = 16L; i < 64L; i++)
                    {
                        var v1 = w[i - 2L];
                        var t1 = (v1 >> (int)(17L) | v1 << (int)((32L - 17L))) ^ (v1 >> (int)(19L) | v1 << (int)((32L - 19L))) ^ (v1 >> (int)(10L));
                        var v2 = w[i - 15L];
                        var t2 = (v2 >> (int)(7L) | v2 << (int)((32L - 7L))) ^ (v2 >> (int)(18L) | v2 << (int)((32L - 18L))) ^ (v2 >> (int)(3L));
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

                    for (i = 0L; i < 64L; i++)
                    {
                        t1 = h + ((e >> (int)(6L) | e << (int)((32L - 6L))) ^ (e >> (int)(11L) | e << (int)((32L - 11L))) ^ (e >> (int)(25L) | e << (int)((32L - 25L)))) + ((e & f) ^ (~e & g)) + _K[i] + w[i];

                        t2 = ((a >> (int)(2L) | a << (int)((32L - 2L))) ^ (a >> (int)(13L) | a << (int)((32L - 13L))) ^ (a >> (int)(22L) | a << (int)((32L - 22L)))) + ((a & b) ^ (a & c) ^ (b & c));

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
