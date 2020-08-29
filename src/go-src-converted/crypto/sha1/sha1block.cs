// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha1 -- go2cs converted at 2020 August 29 08:31:01 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Go\src\crypto\sha1\sha1block.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha1_package
    {
        private static readonly ulong _K0 = 0x5A827999UL;
        private static readonly ulong _K1 = 0x6ED9EBA1UL;
        private static readonly ulong _K2 = 0x8F1BBCDCUL;
        private static readonly ulong _K3 = 0xCA62C1D6UL;

        // blockGeneric is a portable, pure Go version of the SHA-1 block step.
        // It's used by sha1block_generic.go and tests.
        private static void blockGeneric(ref digest dig, slice<byte> p)
        {
            array<uint> w = new array<uint>(16L);

            var h0 = dig.h[0L];
            var h1 = dig.h[1L];
            var h2 = dig.h[2L];
            var h3 = dig.h[3L];
            var h4 = dig.h[4L];
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

                var a = h0;
                var b = h1;
                var c = h2;
                var d = h3;
                var e = h4; 

                // Each of the four 20-iteration rounds
                // differs only in the computation of f and
                // the choice of K (_K0, _K1, etc).
                i = 0L;
                while (i < 16L)
                {
                    var f = b & c | (~b) & d;
                    var a5 = a << (int)(5L) | a >> (int)((32L - 5L));
                    var b30 = b << (int)(30L) | b >> (int)((32L - 30L));
                    var t = a5 + f + e + w[i & 0xfUL] + _K0;
                    a = t;
                    b = a;
                    c = b30;
                    d = c;
                    e = d;
                    i++;
                }

                while (i < 20L)
                {
                    var tmp = w[(i - 3L) & 0xfUL] ^ w[(i - 8L) & 0xfUL] ^ w[(i - 14L) & 0xfUL] ^ w[(i) & 0xfUL];
                    w[i & 0xfUL] = tmp << (int)(1L) | tmp >> (int)((32L - 1L));

                    f = b & c | (~b) & d;
                    a5 = a << (int)(5L) | a >> (int)((32L - 5L));
                    b30 = b << (int)(30L) | b >> (int)((32L - 30L));
                    t = a5 + f + e + w[i & 0xfUL] + _K0;
                    a = t;
                    b = a;
                    c = b30;
                    d = c;
                    e = d;
                    i++;
                }

                while (i < 40L)
                {
                    tmp = w[(i - 3L) & 0xfUL] ^ w[(i - 8L) & 0xfUL] ^ w[(i - 14L) & 0xfUL] ^ w[(i) & 0xfUL];
                    w[i & 0xfUL] = tmp << (int)(1L) | tmp >> (int)((32L - 1L));
                    f = b ^ c ^ d;
                    a5 = a << (int)(5L) | a >> (int)((32L - 5L));
                    b30 = b << (int)(30L) | b >> (int)((32L - 30L));
                    t = a5 + f + e + w[i & 0xfUL] + _K1;
                    a = t;
                    b = a;
                    c = b30;
                    d = c;
                    e = d;
                    i++;
                }

                while (i < 60L)
                {
                    tmp = w[(i - 3L) & 0xfUL] ^ w[(i - 8L) & 0xfUL] ^ w[(i - 14L) & 0xfUL] ^ w[(i) & 0xfUL];
                    w[i & 0xfUL] = tmp << (int)(1L) | tmp >> (int)((32L - 1L));
                    f = ((b | c) & d) | (b & c);

                    a5 = a << (int)(5L) | a >> (int)((32L - 5L));
                    b30 = b << (int)(30L) | b >> (int)((32L - 30L));
                    t = a5 + f + e + w[i & 0xfUL] + _K2;
                    a = t;
                    b = a;
                    c = b30;
                    d = c;
                    e = d;
                    i++;
                }

                while (i < 80L)
                {
                    tmp = w[(i - 3L) & 0xfUL] ^ w[(i - 8L) & 0xfUL] ^ w[(i - 14L) & 0xfUL] ^ w[(i) & 0xfUL];
                    w[i & 0xfUL] = tmp << (int)(1L) | tmp >> (int)((32L - 1L));
                    f = b ^ c ^ d;
                    a5 = a << (int)(5L) | a >> (int)((32L - 5L));
                    b30 = b << (int)(30L) | b >> (int)((32L - 30L));
                    t = a5 + f + e + w[i & 0xfUL] + _K3;
                    a = t;
                    b = a;
                    c = b30;
                    d = c;
                    e = d;
                    i++;
                }


                h0 += a;
                h1 += b;
                h2 += c;
                h3 += d;
                h4 += e;

                p = p[chunk..];
            }


            dig.h[0L] = h0;
            dig.h[1L] = h1;
            dig.h[2L] = h2;
            dig.h[3L] = h3;
            dig.h[4L] = h4;
        }
    }
}}
