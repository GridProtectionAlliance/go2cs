// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// SHA512 block step.
// In its own file so that a faster assembly or C version
// can be substituted easily.
namespace go.crypto;

using bits = math.bits_package;
using math;

partial class sha512_package {

internal static slice<uint64> _K = new uint64[]{
    (nint)4794697086780616226L,
    (nint)8158064640168781261L,
    (nuint)13096744586834688815UL,
    (nuint)16840607885511220156UL,
    (nint)4131703408338449720L,
    (nint)6480981068601479193L,
    (nuint)10538285296894168987UL,
    (nuint)12329834152419229976UL,
    (nuint)15566598209576043074UL,
    (nint)1334009975649890238L,
    (nint)2608012711638119052L,
    (nint)6128411473006802146L,
    (nint)8268148722764581231L,
    (nuint)9286055187155687089UL,
    (nuint)11230858885718282805UL,
    (nuint)13951009754708518548UL,
    (nuint)16472876342353939154UL,
    (nuint)17275323862435702243UL,
    (nint)1135362057144423861L,
    (nint)2597628984639134821L,
    (nint)3308224258029322869L,
    (nint)5365058923640841347L,
    (nint)6679025012923562964L,
    (nint)8573033837759648693L,
    (nuint)10970295158949994411UL,
    (nuint)12119686244451234320UL,
    (nuint)12683024718118986047UL,
    (nuint)13788192230050041572UL,
    (nuint)14330467153632333762UL,
    (nuint)15395433587784984357UL,
    (nint)489312712824947311L,
    (nint)1452737877330783856L,
    (nint)2861767655752347644L,
    (nint)3322285676063803686L,
    (nint)5560940570517711597L,
    (nint)5996557281743188959L,
    (nint)7280758554555802590L,
    (nint)8532644243296465576L,
    (nuint)9350256976987008742UL,
    (nuint)10552545826968843579UL,
    (nuint)11727347734174303076UL,
    (nuint)12113106623233404929UL,
    (nuint)14000437183269869457UL,
    (nuint)14369950271660146224UL,
    (nuint)15101387698204529176UL,
    (nuint)15463397548674623760UL,
    (nuint)17586052441742319658UL,
    (nint)1182934255886127544L,
    (nint)1847814050463011016L,
    (nint)2177327727835720531L,
    (nint)2830643537854262169L,
    (nint)3796741975233480872L,
    (nint)4115178125766777443L,
    (nint)5681478168544905931L,
    (nint)6601373596472566643L,
    (nint)7507060721942968483L,
    (nint)8399075790359081724L,
    (nint)8693463985226723168L,
    (nuint)9568029438360202098UL,
    (nuint)10144078919501101548UL,
    (nuint)10430055236837252648UL,
    (nuint)11840083180663258601UL,
    (nuint)13761210420658862357UL,
    (nuint)14299343276471374635UL,
    (nuint)14566680578165727644UL,
    (nuint)15097957966210449927UL,
    (nuint)16922976911328602910UL,
    (nuint)17689382322260857208UL,
    (nint)500013540394364858L,
    (nint)748580250866718886L,
    (nint)1242879168328830382L,
    (nint)1977374033974150939L,
    (nint)2944078676154940804L,
    (nint)3659926193048069267L,
    (nint)4368137639120453308L,
    (nint)4836135668995329356L,
    (nint)5532061633213252278L,
    (nint)6448918945643986474L,
    (nint)6902733635092675308L,
    (nint)7801388544844847127L
}.slice();

internal static void blockGeneric(ж<digest> Ꮡdig, slice<byte> p) {
    ref var dig = ref Ꮡdig.val;

    array<uint64> w = new(80);
    var (h0, h1, h2, h3, h4, h5, h6, h7) = (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4], dig.h[5], dig.h[6], dig.h[7]);
    while (len(p) >= chunk) {
        for (nint i = 0; i < 16; i++) {
            nint j = i * 8;
            w[i] = (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)p[j]) << (int)(56) | ((uint64)p[j + 1]) << (int)(48)) | ((uint64)p[j + 2]) << (int)(40)) | ((uint64)p[j + 3]) << (int)(32)) | ((uint64)p[j + 4]) << (int)(24)) | ((uint64)p[j + 5]) << (int)(16)) | ((uint64)p[j + 6]) << (int)(8)) | ((uint64)p[j + 7]));
        }
        for (nint i = 16; i < 80; i++) {
            var v1 = w[i - 2];
            var t1 = (uint64)((uint64)(bits.RotateLeft64(v1, -19) ^ bits.RotateLeft64(v1, -61)) ^ (v1 >> (int)(6)));
            var v2 = w[i - 15];
            var t2 = (uint64)((uint64)(bits.RotateLeft64(v2, -1) ^ bits.RotateLeft64(v2, -8)) ^ (v2 >> (int)(7)));
            w[i] = t1 + w[i - 7] + t2 + w[i - 16];
        }
        var (a, b, c, d, e, f, g, h) = (h0, h1, h2, h3, h4, h5, h6, h7);
        for (nint i = 0; i < 80; i++) {
            var t1 = h + ((uint64)((uint64)(bits.RotateLeft64(e, -14) ^ bits.RotateLeft64(e, -18)) ^ bits.RotateLeft64(e, -41))) + ((uint64)(((uint64)(e & f)) ^ ((uint64)(^e & g)))) + _K[i] + w[i];
            var t2 = ((uint64)((uint64)(bits.RotateLeft64(a, -28) ^ bits.RotateLeft64(a, -34)) ^ bits.RotateLeft64(a, -39))) + ((uint64)((uint64)(((uint64)(a & b)) ^ ((uint64)(a & c))) ^ ((uint64)(b & c))));
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

} // end sha512_package
