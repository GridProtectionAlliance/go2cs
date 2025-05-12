// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// SHA256 block step.
// In its own file so that a faster assembly or C version
// can be substituted easily.
namespace go.crypto;

using bits = math.bits_package;
using math;

partial class sha256_package {

internal static slice<uint32> _K = new uint32[]{
    1116352408,
    1899447441,
    (nint)3049323471L,
    (nint)3921009573L,
    961987163,
    1508970993,
    (nint)2453635748L,
    (nint)2870763221L,
    (nint)3624381080L,
    310598401,
    607225278,
    1426881987,
    1925078388,
    (nint)2162078206L,
    (nint)2614888103L,
    (nint)3248222580L,
    (nint)3835390401L,
    (nint)4022224774L,
    264347078,
    604807628,
    770255983,
    1249150122,
    1555081692,
    1996064986,
    (nint)2554220882L,
    (nint)2821834349L,
    (nint)2952996808L,
    (nint)3210313671L,
    (nint)3336571891L,
    (nint)3584528711L,
    113926993,
    338241895,
    666307205,
    773529912,
    1294757372,
    1396182291,
    1695183700,
    1986661051,
    (nint)2177026350L,
    (nint)2456956037L,
    (nint)2730485921L,
    (nint)2820302411L,
    (nint)3259730800L,
    (nint)3345764771L,
    (nint)3516065817L,
    (nint)3600352804L,
    (nint)4094571909L,
    275423344,
    430227734,
    506948616,
    659060556,
    883997877,
    958139571,
    1322822218,
    1537002063,
    1747873779,
    1955562222,
    2024104815,
    (nint)2227730452L,
    (nint)2361852424L,
    (nint)2428436474L,
    (nint)2756734187L,
    (nint)3204031479L,
    (nint)3329325298L
}.slice();

internal static void blockGeneric(ж<digest> Ꮡdig, slice<byte> p) {
    ref var dig = ref Ꮡdig.val;

    array<uint32> w = new(64);
    var (h0, h1, h2, h3, h4, h5, h6, h7) = (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4], dig.h[5], dig.h[6], dig.h[7]);
    while (len(p) >= chunk) {
        // Can interlace the computation of w with the
        // rounds below if needed for speed.
        for (nint i = 0; i < 16; i++) {
            nint j = i * 4;
            w[i] = (uint32)((uint32)((uint32)(((uint32)p[j]) << (int)(24) | ((uint32)p[j + 1]) << (int)(16)) | ((uint32)p[j + 2]) << (int)(8)) | ((uint32)p[j + 3]));
        }
        for (nint i = 16; i < 64; i++) {
            var v1 = w[i - 2];
            var t1 = (uint32)((uint32)((bits.RotateLeft32(v1, -17)) ^ (bits.RotateLeft32(v1, -19))) ^ (v1 >> (int)(10)));
            var v2 = w[i - 15];
            var t2 = (uint32)((uint32)((bits.RotateLeft32(v2, -7)) ^ (bits.RotateLeft32(v2, -18))) ^ (v2 >> (int)(3)));
            w[i] = t1 + w[i - 7] + t2 + w[i - 16];
        }
        var (a, b, c, d, e, f, g, h) = (h0, h1, h2, h3, h4, h5, h6, h7);
        for (nint i = 0; i < 64; i++) {
            var t1 = h + ((uint32)((uint32)((bits.RotateLeft32(e, -6)) ^ (bits.RotateLeft32(e, -11))) ^ (bits.RotateLeft32(e, -25)))) + ((uint32)(((uint32)(e & f)) ^ ((uint32)(~e & g)))) + _K[i] + w[i];
            var t2 = ((uint32)((uint32)((bits.RotateLeft32(a, -2)) ^ (bits.RotateLeft32(a, -13))) ^ (bits.RotateLeft32(a, -22)))) + ((uint32)((uint32)(((uint32)(a & b)) ^ ((uint32)(a & c))) ^ ((uint32)(b & c))));
            h = g;
            g = f;
            f = e;
            e = d + t1;
            d = c;
            c = b;
            b = a;
            a = t1 + t2;
        }
        h0 += a;
        h1 += b;
        h2 += c;
        h3 += d;
        h4 += e;
        h5 += f;
        h6 += g;
        h7 += h;
        p = p[(int)(chunk)..];
    }
    (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4], dig.h[5], dig.h[6], dig.h[7]) = (h0, h1, h2, h3, h4, h5, h6, h7);
}

} // end sha256_package
