// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bits = math.bits_package;
using math;

partial class sha1_package {

internal static readonly UntypedInt _K0 = /* 0x5A827999 */ 1518500249;
internal static readonly UntypedInt _K1 = /* 0x6ED9EBA1 */ 1859775393;
internal static readonly UntypedInt _K2 = /* 0x8F1BBCDC */ 2400959708;
internal static readonly UntypedInt _K3 = /* 0xCA62C1D6 */ 3395469782;

// blockGeneric is a portable, pure Go version of the SHA-1 block step.
// It's used by sha1block_generic.go and tests.
internal static void blockGeneric(ж<digest> Ꮡdig, slice<byte> p) {
    ref var dig = ref Ꮡdig.val;

    array<uint32> w = new(16);
    var (h0, h1, h2, h3, h4) = (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4]);
    while (len(p) >= chunk) {
        // Can interlace the computation of w with the
        // rounds below if needed for speed.
        for (nint i = 0; i < 16; i++) {
            nint j = i * 4;
            w[i] = (uint32)((uint32)((uint32)(((uint32)p[j]) << (int)(24) | ((uint32)p[j + 1]) << (int)(16)) | ((uint32)p[j + 2]) << (int)(8)) | ((uint32)p[j + 3]));
        }
        var (a, b, c, d, e) = (h0, h1, h2, h3, h4);
        // Each of the four 20-iteration rounds
        // differs only in the computation of f and
        // the choice of K (_K0, _K1, etc).
        nint i = 0;
        for (; i < 16; i++) {
            var f = (uint32)((uint32)(b & c) | (uint32)((^b) & d));
            var t = bits.RotateLeft32(a, 5) + f + e + w[(nint)(i & 15)] + _K0;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 20; i++) {
            var tmp = (uint32)((uint32)((uint32)(w[(nint)((i - 3) & 15)] ^ w[(nint)((i - 8) & 15)]) ^ w[(nint)((i - 14) & 15)]) ^ w[(nint)((i) & 15)]);
            w[(nint)(i & 15)] = bits.RotateLeft32(tmp, 1);
            var f = (uint32)((uint32)(b & c) | (uint32)((^b) & d));
            var t = bits.RotateLeft32(a, 5) + f + e + w[(nint)(i & 15)] + _K0;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 40; i++) {
            var tmp = (uint32)((uint32)((uint32)(w[(nint)((i - 3) & 15)] ^ w[(nint)((i - 8) & 15)]) ^ w[(nint)((i - 14) & 15)]) ^ w[(nint)((i) & 15)]);
            w[(nint)(i & 15)] = bits.RotateLeft32(tmp, 1);
            var f = (uint32)((uint32)(b ^ c) ^ d);
            var t = bits.RotateLeft32(a, 5) + f + e + w[(nint)(i & 15)] + _K1;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 60; i++) {
            var tmp = (uint32)((uint32)((uint32)(w[(nint)((i - 3) & 15)] ^ w[(nint)((i - 8) & 15)]) ^ w[(nint)((i - 14) & 15)]) ^ w[(nint)((i) & 15)]);
            w[(nint)(i & 15)] = bits.RotateLeft32(tmp, 1);
            var f = (uint32)(((uint32)(((uint32)(b | c)) & d)) | ((uint32)(b & c)));
            var t = bits.RotateLeft32(a, 5) + f + e + w[(nint)(i & 15)] + _K2;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 80; i++) {
            var tmp = (uint32)((uint32)((uint32)(w[(nint)((i - 3) & 15)] ^ w[(nint)((i - 8) & 15)]) ^ w[(nint)((i - 14) & 15)]) ^ w[(nint)((i) & 15)]);
            w[(nint)(i & 15)] = bits.RotateLeft32(tmp, 1);
            var f = (uint32)((uint32)(b ^ c) ^ d);
            var t = bits.RotateLeft32(a, 5) + f + e + w[(nint)(i & 15)] + _K3;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        h0 += a;
        h1 += b;
        h2 += c;
        h3 += d;
        h4 += e;
        p = p[(int)(chunk)..];
    }
    (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4]) = (h0, h1, h2, h3, h4);
}

} // end sha1_package
