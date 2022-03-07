// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha1 -- go2cs converted at 2022 March 06 22:19:26 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Program Files\Go\src\crypto\sha1\sha1block.go
using bits = go.math.bits_package;

namespace go.crypto;

public static partial class sha1_package {

private static readonly nuint _K0 = 0x5A827999;
private static readonly nuint _K1 = 0x6ED9EBA1;
private static readonly nuint _K2 = 0x8F1BBCDC;
private static readonly nuint _K3 = 0xCA62C1D6;


// blockGeneric is a portable, pure Go version of the SHA-1 block step.
// It's used by sha1block_generic.go and tests.
private static void blockGeneric(ptr<digest> _addr_dig, slice<byte> p) {
    ref digest dig = ref _addr_dig.val;

    array<uint> w = new array<uint>(16);

    var h0 = dig.h[0];
    var h1 = dig.h[1];
    var h2 = dig.h[2];
    var h3 = dig.h[3];
    var h4 = dig.h[4];
    while (len(p) >= chunk) { 
        // Can interlace the computation of w with the
        // rounds below if needed for speed.
        {
            nint i__prev2 = i;

            for (nint i = 0; i < 16; i++) {
                var j = i * 4;
                w[i] = uint32(p[j]) << 24 | uint32(p[j + 1]) << 16 | uint32(p[j + 2]) << 8 | uint32(p[j + 3]);
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
        i = 0;
        while (i < 16) {
            var f = b & c | (~b) & d;
            var t = bits.RotateLeft32(a, 5) + f + e + w[i & 0xf] + _K0;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);            i++;
        }
        while (i < 20) {
            var tmp = w[(i - 3) & 0xf] ^ w[(i - 8) & 0xf] ^ w[(i - 14) & 0xf] ^ w[(i) & 0xf];
            w[i & 0xf] = tmp << 1 | tmp >> (int)((32 - 1));

            f = b & c | (~b) & d;
            t = bits.RotateLeft32(a, 5) + f + e + w[i & 0xf] + _K0;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);            i++;
        }
        while (i < 40) {
            tmp = w[(i - 3) & 0xf] ^ w[(i - 8) & 0xf] ^ w[(i - 14) & 0xf] ^ w[(i) & 0xf];
            w[i & 0xf] = tmp << 1 | tmp >> (int)((32 - 1));
            f = b ^ c ^ d;
            t = bits.RotateLeft32(a, 5) + f + e + w[i & 0xf] + _K1;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);            i++;
        }
        while (i < 60) {
            tmp = w[(i - 3) & 0xf] ^ w[(i - 8) & 0xf] ^ w[(i - 14) & 0xf] ^ w[(i) & 0xf];
            w[i & 0xf] = tmp << 1 | tmp >> (int)((32 - 1));
            f = ((b | c) & d) | (b & c);
            t = bits.RotateLeft32(a, 5) + f + e + w[i & 0xf] + _K2;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);            i++;
        }
        while (i < 80) {
            tmp = w[(i - 3) & 0xf] ^ w[(i - 8) & 0xf] ^ w[(i - 14) & 0xf] ^ w[(i) & 0xf];
            w[i & 0xf] = tmp << 1 | tmp >> (int)((32 - 1));
            f = b ^ c ^ d;
            t = bits.RotateLeft32(a, 5) + f + e + w[i & 0xf] + _K3;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);            i++;
        }

        h0 += a;
        h1 += b;
        h2 += c;
        h3 += d;
        h4 += e;

        p = p[(int)chunk..];

    }

    (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4]) = (h0, h1, h2, h3, h4);
}

} // end sha1_package
