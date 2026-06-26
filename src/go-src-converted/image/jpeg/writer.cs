// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bufio = bufio_package;
using errors = errors_package;
using image = image_package;
using color = image.color_package;
using io = io_package;

partial class jpeg_package {

// div returns a/b rounded to the nearest integer, instead of rounded to zero.
internal static int32 div(int32 a, int32 b) {
    if (a >= 0) {
        return (a + (b >> (int)(1))) / b;
    }
    return -((-a + (b >> (int)(1))) / b);
}

// bitCount counts the number of bits needed to hold an integer.
internal static array<byte> bitCount = new byte[]{
    0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
    5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
}.array();

[GoType("num:nint")] partial struct quantIndex;

internal static readonly quantIndex quantIndexLuminance = /* iota */ 0;
internal static readonly quantIndex quantIndexChrominance = 1;
internal static readonly quantIndex nQuantIndex = 2;

// Luminance.
// Chrominance.
// unscaledQuant are the unscaled quantization tables in zig-zag order. Each
// encoder copies and scales the tables according to its quality parameter.
// The values are derived from section K.1 after converting from natural to
// zig-zag order.
internal static array<array<byte>> unscaledQuant = new array<byte>[]{
    new(
        16, 11, 12, 14, 12, 10, 16, 14,
        13, 14, 18, 17, 16, 19, 24, 40,
        26, 24, 22, 22, 24, 49, 35, 37,
        29, 40, 58, 51, 61, 60, 57, 51,
        56, 55, 64, 72, 92, 78, 64, 68,
        87, 69, 55, 56, 80, 109, 81, 87,
        95, 98, 103, 104, 103, 62, 77, 113,
        121, 112, 100, 120, 92, 101, 103, 99
    ),
    new(
        17, 18, 18, 24, 21, 24, 47, 26,
        26, 47, 99, 66, 56, 66, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99
    )
}.array();

[GoType("num:nint")] partial struct huffIndex;

internal static readonly huffIndex huffIndexLuminanceDC = /* iota */ 0;
internal static readonly huffIndex huffIndexLuminanceAC = 1;
internal static readonly huffIndex huffIndexChrominanceDC = 2;
internal static readonly huffIndex huffIndexChrominanceAC = 3;
internal static readonly huffIndex nHuffIndex = 4;

// huffmanSpec specifies a Huffman encoding.
[GoType] partial struct huffmanSpec {
    // count[i] is the number of codes of length i bits.
    internal array<byte> count = new(16);
    // value[i] is the decoded value of the i'th codeword.
    internal slice<byte> value;
}

// Luminance DC.
// Luminance AC.
// Chrominance DC.
// Chrominance AC.
// theHuffmanSpec is the Huffman encoding specifications.
// This encoder uses the same Huffman encoding for all images.
internal static array<huffmanSpec> theHuffmanSpec = new huffmanSpec[]{
    new(
        new byte[]{0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0}.array(),
        new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}.slice()
    ),
    new(
        new byte[]{0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 125}.array(),
        new byte[]{
            1, 2, 3, 0, 4, 17, 5, 18,
            33, 49, 65, 6, 19, 81, 97, 7,
            34, 113, 20, 50, 129, 145, 161, 8,
            35, 66, 177, 193, 21, 82, 209, 240,
            36, 51, 98, 114, 130, 9, 10, 22,
            23, 24, 25, 26, 37, 38, 39, 40,
            41, 42, 52, 53, 54, 55, 56, 57,
            58, 67, 68, 69, 70, 71, 72, 73,
            74, 83, 84, 85, 86, 87, 88, 89,
            90, 99, 100, 101, 102, 103, 104, 105,
            106, 115, 116, 117, 118, 119, 120, 121,
            122, 131, 132, 133, 134, 135, 136, 137,
            138, 146, 147, 148, 149, 150, 151, 152,
            153, 154, 162, 163, 164, 165, 166, 167,
            168, 169, 170, 178, 179, 180, 181, 182,
            183, 184, 185, 186, 194, 195, 196, 197,
            198, 199, 200, 201, 202, 210, 211, 212,
            213, 214, 215, 216, 217, 218, 225, 226,
            227, 228, 229, 230, 231, 232, 233, 234,
            241, 242, 243, 244, 245, 246, 247, 248,
            249, 250
        }.slice()
    ),
    new(
        new byte[]{0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0}.array(),
        new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}.slice()
    ),
    new(
        new byte[]{0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 119}.array(),
        new byte[]{
            0, 1, 2, 3, 17, 4, 5, 33,
            49, 6, 18, 65, 81, 7, 97, 113,
            19, 34, 50, 129, 8, 20, 66, 145,
            161, 177, 193, 9, 35, 51, 82, 240,
            21, 98, 114, 209, 10, 22, 36, 52,
            225, 37, 241, 23, 24, 25, 26, 38,
            39, 40, 41, 42, 53, 54, 55, 56,
            57, 58, 67, 68, 69, 70, 71, 72,
            73, 74, 83, 84, 85, 86, 87, 88,
            89, 90, 99, 100, 101, 102, 103, 104,
            105, 106, 115, 116, 117, 118, 119, 120,
            121, 122, 130, 131, 132, 133, 134, 135,
            136, 137, 138, 146, 147, 148, 149, 150,
            151, 152, 153, 154, 162, 163, 164, 165,
            166, 167, 168, 169, 170, 178, 179, 180,
            181, 182, 183, 184, 185, 186, 194, 195,
            196, 197, 198, 199, 200, 201, 202, 210,
            211, 212, 213, 214, 215, 216, 217, 218,
            226, 227, 228, 229, 230, 231, 232, 233,
            234, 242, 243, 244, 245, 246, 247, 248,
            249, 250
        }.slice()
    )
}.array();

[GoType("[]uint32")] partial struct huffmanLUT;

[GoRecv] internal static void init(this ref huffmanLUT h, huffmanSpec s) {
    nint maxValue = 0;
    foreach (var (_, v) in s.value) {
        if (((nint)v) > maxValue) {
            maxValue = ((nint)v);
        }
    }
    h = new slice<uint32>(maxValue + 1);
    var code = ((uint32)0);
    nint k = 0;
    for (nint i = 0; i < len(s.count); i++) {
        var nBits = ((uint32)(i + 1)) << (int)(24);
        for (var j = ((uint8)0); j < s.count[i]; j++) {
            (ж<ж<huffmanLUT>>)[s.value[k]] = (uint32)(nBits | code);
            code++;
            k++;
        }
        code <<= (UntypedInt)(1);
    }
}

// theHuffmanLUT are compiled representations of theHuffmanSpec.
internal static array<huffmanLUT> theHuffmanLUT;

[GoInit] internal static void init() {
    foreach (var (i, s) in theHuffmanSpec) {
        theHuffmanLUT[i].init(s);
    }
}

// writer is a buffered writer.
[GoType] partial interface writer :
    io.Writer,
    io.ByteWriter
{
    error Flush();
}

// encoder encodes an image to the JPEG format.
[GoType] partial struct encoder {
    // w is the writer to write to. err is the first error encountered during
    // writing. All attempted writes after the first error become no-ops.
    internal writer w;
    internal error err;
    // buf is a scratch buffer.
    internal array<byte> buf = new(16);
    // bits and nBits are accumulated bits to write to w.
    internal uint32 bits;
    internal uint32 nBits;
    // quant is the scaled quantization tables, in zig-zag order.
    internal array<array<byte>> quant = new(nQuantIndex);
}

[GoRecv] internal static void flush(this ref encoder e) {
    if (e.err != default!) {
        return;
    }
    e.err = e.w.Flush();
}

[GoRecv] internal static void write(this ref encoder e, slice<byte> p) {
    if (e.err != default!) {
        return;
    }
    (_, e.err) = e.w.Write(p);
}

[GoRecv] internal static void writeByte(this ref encoder e, byte b) {
    if (e.err != default!) {
        return;
    }
    e.err = e.w.WriteByte(b);
}

// emit emits the least significant nBits bits of bits to the bit-stream.
// The precondition is bits < 1<<nBits && nBits <= 16.
[GoRecv] internal static void emit(this ref encoder e, uint32 bits, uint32 nBits) {
    nBits += e.nBits;
    bits <<= (uint32)(32 - nBits);
    bits |= (uint32)(e.bits);
    while (nBits >= 8) {
        var b = ((uint8)(bits >> (int)(24)));
        e.writeByte(b);
        if (b == 255) {
            e.writeByte(0);
        }
        bits <<= (UntypedInt)(8);
        nBits -= 8;
    }
    (e.bits, e.nBits) = (bits, nBits);
}

// emitHuff emits the given value with the given Huffman encoder.
[GoRecv] internal static void emitHuff(this ref encoder e, huffIndex h, int32 value) {
    var x = theHuffmanLUT[h][value];
    e.emit((uint32)(x & (1 << (int)(24) - 1)), x >> (int)(24));
}

// emitHuffRLE emits a run of runLength copies of value encoded with the given
// Huffman encoder.
[GoRecv] internal static void emitHuffRLE(this ref encoder e, huffIndex h, int32 runLength, int32 value) {
    var (a, b) = (value, value);
    if (a < 0) {
        (a, b) = (-value, value - 1);
    }
    uint32 nBits = default!;
    if (a < 256){
        nBits = ((uint32)bitCount[a]);
    } else {
        nBits = 8 + ((uint32)bitCount[a >> (int)(8)]);
    }
    e.emitHuff(h, (int32)(runLength << (int)(4) | ((int32)nBits)));
    if (nBits > 0) {
        e.emit((uint32)(((uint32)b) & (1 << (int)(nBits) - 1)), nBits);
    }
}

// writeMarkerHeader writes the header for a marker with the given length.
[GoRecv] internal static void writeMarkerHeader(this ref encoder e, uint8 marker, nint markerlen) {
    e.buf[0] = 255;
    e.buf[1] = marker;
    e.buf[2] = ((uint8)(markerlen >> (int)(8)));
    e.buf[3] = ((uint8)((nint)(markerlen & 255)));
    e.write(e.buf[..4]);
}

// writeDQT writes the Define Quantization Table marker.
[GoRecv] internal static void writeDQT(this ref encoder e) {
    const nint markerlen = /* 2 + int(nQuantIndex)*(1+blockSize) */ 132;
    e.writeMarkerHeader(dqtMarker, markerlen);
    foreach (var (i, _) in e.quant) {
        e.writeByte(((uint8)i));
        e.write(e.quant[i][..]);
    }
}

// writeSOF0 writes the Start Of Frame (Baseline Sequential) marker.
[GoRecv] internal static void writeSOF0(this ref encoder e, image.Point size, nint nComponent) {
    nint markerlen = 8 + 3 * nComponent;
    e.writeMarkerHeader(sof0Marker, markerlen);
    e.buf[0] = 8;
    // 8-bit color.
    e.buf[1] = ((uint8)(size.Y >> (int)(8)));
    e.buf[2] = ((uint8)((nint)(size.Y & 255)));
    e.buf[3] = ((uint8)(size.X >> (int)(8)));
    e.buf[4] = ((uint8)((nint)(size.X & 255)));
    e.buf[5] = ((uint8)nComponent);
    if (nComponent == 1){
        e.buf[6] = 1;
        // No subsampling for grayscale image.
        e.buf[7] = 17;
        e.buf[8] = 0;
    } else {
        for (nint i = 0; i < nComponent; i++) {
            e.buf[3 * i + 6] = ((uint8)(i + 1));
            // We use 4:2:0 chroma subsampling.
            e.buf[3 * i + 7] = "\x22\x11\x11"u8[i];
            e.buf[3 * i + 8] = "\x00\x01\x01"u8[i];
        }
    }
    e.write(e.buf[..(int)(3 * (nComponent - 1) + 9)]);
}

// writeDHT writes the Define Huffman Table marker.
[GoRecv] internal static void writeDHT(this ref encoder e, nint nComponent) {
    nint markerlen = 2;
    var specs = theHuffmanSpec[..];
    if (nComponent == 1) {
        // Drop the Chrominance tables.
        specs = specs[..2];
    }
    foreach (var (_, s) in specs) {
        markerlen += 1 + 16 + len(s.value);
    }
    e.writeMarkerHeader(dhtMarker, markerlen);
    foreach (var (i, s) in specs) {
        e.writeByte("\x00\x10\x01\x11"u8[i]);
        e.write(s.count[..]);
        e.write(s.value);
    }
}

// writeBlock writes a block of pixel data using the given quantization table,
// returning the post-quantized DC value of the DCT-transformed block. b is in
// natural (not zig-zag) order.
[GoRecv] internal static int32 writeBlock(this ref encoder e, ж<block> Ꮡb, quantIndex q, int32 prevDC) {
    ref var b = ref Ꮡb.val;

    fdct(Ꮡb);
    // Emit the DC delta.
    var dc = div(b[0], 8 * ((int32)e.quant[q][0]));
    e.emitHuffRLE(((huffIndex)(2 * q + 0)), 0, dc - prevDC);
    // Emit the AC components.
    huffIndex h = ((huffIndex)(2 * q + 1));
    var runLength = ((int32)0);
    for (nint zig = 1; zig < blockSize; zig++) {
        var ac = div(b[unzig[zig]], 8 * ((int32)e.quant[q][zig]));
        if (ac == 0){
            runLength++;
        } else {
            while (runLength > 15) {
                e.emitHuff(h, 240);
                runLength -= 16;
            }
            e.emitHuffRLE(h, runLength, ac);
            runLength = 0;
        }
    }
    if (runLength > 0) {
        e.emitHuff(h, 0);
    }
    return dc;
}

// toYCbCr converts the 8x8 region of m whose top-left corner is p to its
// YCbCr values.
internal static void toYCbCr(image.Image m, image.Point p, ж<block> ᏑyBlock, ж<block> ᏑcbBlock, ж<block> ᏑcrBlock) {
    ref var yBlock = ref ᏑyBlock.val;
    ref var cbBlock = ref ᏑcbBlock.val;
    ref var crBlock = ref ᏑcrBlock.val;

    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    for (nint j = 0; j < 8; j++) {
        for (nint i = 0; i < 8; i++) {
            var (r, g, bΔ1, _) = m.At(min(p.X + i, xmax), min(p.Y + j, ymax)).RGBA();
            var (yy, cb, cr) = color.RGBToYCbCr(((uint8)(r >> (int)(8))), ((uint8)(g >> (int)(8))), ((uint8)(bΔ1 >> (int)(8))));
            yBlock[8 * j + i] = ((int32)yy);
            cbBlock[8 * j + i] = ((int32)cb);
            crBlock[8 * j + i] = ((int32)cr);
        }
    }
}

// grayToY stores the 8x8 region of m whose top-left corner is p in yBlock.
internal static void grayToY(ж<image.Gray> Ꮡm, image.Point p, ж<block> ᏑyBlock) {
    ref var m = ref Ꮡm.val;
    ref var yBlock = ref ᏑyBlock.val;

    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    var pix = m.Pix;
    for (nint j = 0; j < 8; j++) {
        for (nint i = 0; i < 8; i++) {
            nint idx = m.PixOffset(min(p.X + i, xmax), min(p.Y + j, ymax));
            yBlock[8 * j + i] = ((int32)pix[idx]);
        }
    }
}

// rgbaToYCbCr is a specialized version of toYCbCr for image.RGBA images.
internal static void rgbaToYCbCr(ж<imageꓸRGBA> Ꮡm, image.Point p, ж<block> ᏑyBlock, ж<block> ᏑcbBlock, ж<block> ᏑcrBlock) {
    ref var m = ref Ꮡm.val;
    ref var yBlock = ref ᏑyBlock.val;
    ref var cbBlock = ref ᏑcbBlock.val;
    ref var crBlock = ref ᏑcrBlock.val;

    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    for (nint j = 0; j < 8; j++) {
        nint sj = p.Y + j;
        if (sj > ymax) {
            sj = ymax;
        }
        nint offset = (sj - b.Min.Y) * m.Stride - b.Min.X * 4;
        for (nint i = 0; i < 8; i++) {
            nint sx = p.X + i;
            if (sx > xmax) {
                sx = xmax;
            }
            var pix = m.Pix[(int)(offset + sx * 4)..];
            var (yy, cb, cr) = color.RGBToYCbCr(pix[0], pix[1], pix[2]);
            yBlock[8 * j + i] = ((int32)yy);
            cbBlock[8 * j + i] = ((int32)cb);
            crBlock[8 * j + i] = ((int32)cr);
        }
    }
}

// yCbCrToYCbCr is a specialized version of toYCbCr for image.YCbCr images.
internal static void yCbCrToYCbCr(ж<image.YCbCr> Ꮡm, image.Point p, ж<block> ᏑyBlock, ж<block> ᏑcbBlock, ж<block> ᏑcrBlock) {
    ref var m = ref Ꮡm.val;
    ref var yBlock = ref ᏑyBlock.val;
    ref var cbBlock = ref ᏑcbBlock.val;
    ref var crBlock = ref ᏑcrBlock.val;

    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    for (nint j = 0; j < 8; j++) {
        nint sy = p.Y + j;
        if (sy > ymax) {
            sy = ymax;
        }
        for (nint i = 0; i < 8; i++) {
            nint sx = p.X + i;
            if (sx > xmax) {
                sx = xmax;
            }
            nint yi = m.YOffset(sx, sy);
            nint ci = m.COffset(sx, sy);
            yBlock[8 * j + i] = ((int32)m.Y[yi]);
            cbBlock[8 * j + i] = ((int32)m.Cb[ci]);
            crBlock[8 * j + i] = ((int32)m.Cr[ci]);
        }
    }
}

// scale scales the 16x16 region represented by the 4 src blocks to the 8x8
// dst block.
internal static void scale(ж<block> Ꮡdst, ж<array<block>> Ꮡsrc) {
    ref var dst = ref Ꮡdst.val;
    ref var src = ref Ꮡsrc.val;

    for (nint i = 0; i < 4; i++) {
        nint dstOff = (nint)(((nint)(i & 2)) << (int)(4) | ((nint)(i & 1)) << (int)(2));
        for (nint y = 0; y < 4; y++) {
            for (nint x = 0; x < 4; x++) {
                nint j = 16 * y + 2 * x;
                var sum = src[i][j] + src[i][j + 1] + src[i][j + 8] + src[i][j + 9];
                dst[8 * y + x + dstOff] = (sum + 2) >> (int)(2);
            }
        }
    }
}

// sosHeaderY is the SOS marker "\xff\xda" followed by 8 bytes:
//   - the marker length "\x00\x08",
//   - the number of components "\x01",
//   - component 1 uses DC table 0 and AC table 0 "\x01\x00",
//   - the bytes "\x00\x3f\x00". Section B.2.3 of the spec says that for
//     sequential DCTs, those bytes (8-bit Ss, 8-bit Se, 4-bit Ah, 4-bit Al)
//     should be 0x00, 0x3f, 0x00<<4 | 0x00.
internal static slice<byte> sosHeaderY = new byte[]{
    255, 218, 0, 8, 1, 1, 0, 0, 63, 0
}.slice();

// sosHeaderYCbCr is the SOS marker "\xff\xda" followed by 12 bytes:
//   - the marker length "\x00\x0c",
//   - the number of components "\x03",
//   - component 1 uses DC table 0 and AC table 0 "\x01\x00",
//   - component 2 uses DC table 1 and AC table 1 "\x02\x11",
//   - component 3 uses DC table 1 and AC table 1 "\x03\x11",
//   - the bytes "\x00\x3f\x00". Section B.2.3 of the spec says that for
//     sequential DCTs, those bytes (8-bit Ss, 8-bit Se, 4-bit Ah, 4-bit Al)
//     should be 0x00, 0x3f, 0x00<<4 | 0x00.
internal static slice<byte> sosHeaderYCbCr = new byte[]{
    255, 218, 0, 12, 3, 1, 0, 2,
    17, 3, 17, 0, 63, 0
}.slice();

// writeSOS writes the StartOfScan marker.
[GoRecv] internal static void writeSOS(this ref encoder e, image.Image m) {
    switch (m.type()) {
    case ж<image.Gray> : {
        e.write(sosHeaderY);
        break;
    }
    default: {

        e.write(sosHeaderYCbCr);
        break;
    }}

    ref var b = ref heap(new block(), out var Ꮡb);
    ref var cb = ref heap(new array<block>(4), out var Ꮡcb);
    ref var cr = ref heap(new array<block>(4), out var Ꮡcr);
    int32 prevDCY = default!;
    int32 prevDCCb = default!;
    int32 prevDCCr = default!;
    var bounds = m.Bounds();
    switch (m.type()) {
    case ж<image.Gray> m: {
        for (nint y = bounds.Min.Y; y < bounds.Max.Y; y += 8) {
            // TODO(wathiede): switch on m.ColorModel() instead of type.
            for (nint x = bounds.Min.X; x < bounds.Max.X; x += 8) {
                var p = image.Pt(x, y);
                grayToY(Ꮡm, p, Ꮡb);
                prevDCY = e.writeBlock(Ꮡb, 0, prevDCY);
            }
        }
        break;
    }
    default: {
        var m = m.type();
        var (rgba, _) = m._<ж<imageꓸRGBA>>(ᐧ);
        var (ycbcr, _) = m._<ж<image.YCbCr>>(ᐧ);
        for (nint y = bounds.Min.Y; y < bounds.Max.Y; y += 16) {
            for (nint x = bounds.Min.X; x < bounds.Max.X; x += 16) {
                ref var i = ref heap<nint>(out var Ꮡi);
                for (i = 0; i < 4; i++) {
                    nint xOff = ((nint)(i & 1)) * 8;
                    nint yOff = ((nint)(i & 2)) * 4;
                    var p = image.Pt(x + xOff, y + yOff);
                    if (rgba != nil){
                        rgbaToYCbCr(rgba, p, Ꮡb, Ꮡcb.at<block>(i), Ꮡcr.at<block>(i));
                    } else 
                    if (ycbcr != nil){
                        yCbCrToYCbCr(ycbcr, p, Ꮡb, Ꮡcb.at<block>(i), Ꮡcr.at<block>(i));
                    } else {
                        toYCbCr(m, p, Ꮡb, Ꮡcb.at<block>(i), Ꮡcr.at<block>(i));
                    }
                    prevDCY = e.writeBlock(Ꮡb, 0, prevDCY);
                }
                scale(Ꮡb, Ꮡcb);
                prevDCCb = e.writeBlock(Ꮡb, 1, prevDCCb);
                scale(Ꮡb, Ꮡcr);
                prevDCCr = e.writeBlock(Ꮡb, 1, prevDCCr);
            }
        }
        break;
    }}
    // Pad the last byte with 1's.
    e.emit(127, 7);
}

// DefaultQuality is the default quality encoding parameter.
public static readonly UntypedInt DefaultQuality = 75;

// Options are the encoding parameters.
// Quality ranges from 1 to 100 inclusive, higher is better.
[GoType] partial struct Options {
    public nint Quality;
}

// Encode writes the Image m to w in JPEG 4:2:0 baseline format with the given
// options. Default parameters are used if a nil *[Options] is passed.
public static error Encode(io.Writer w, image.Image m, ж<Options> Ꮡo) {
    ref var o = ref Ꮡo.val;

    var b = m.Bounds();
    if (b.Dx() >= 1 << (int)(16) || b.Dy() >= 1 << (int)(16)) {
        return errors.New("jpeg: image is too large to encode"u8);
    }
    encoder e = default!;
    {
        var (ww, ok) = w._<writer>(ᐧ); if (ok){
            e.w = ww;
        } else {
            e.w = bufio.NewWriter(w);
        }
    }
    // Clip quality to [1, 100].
    nint quality = DefaultQuality;
    if (o != nil) {
        quality = o.Quality;
        if (quality < 1){
            quality = 1;
        } else 
        if (quality > 100) {
            quality = 100;
        }
    }
    // Convert from a quality rating to a scaling factor.
    nint scale = default!;
    if (quality < 50){
        scale = 5000 / quality;
    } else {
        scale = 200 - quality * 2;
    }
    // Initialize the quantization tables.
    foreach (var (i, _) in e.quant) {
        foreach (var (j, _) in e.quant[i]) {
            nint x = ((nint)unscaledQuant[i][j]);
            x = (x * scale + 50) / 100;
            if (x < 1){
                x = 1;
            } else 
            if (x > 255) {
                x = 255;
            }
            e.quant[i][j] = ((uint8)x);
        }
    }
    // Compute number of components based on input image type.
    nint nComponent = 3;
    switch (m.type()) {
    case ж<image.Gray> : {
        nComponent = 1;
        break;
    }}

    // TODO(wathiede): switch on m.ColorModel() instead of type.
    // Write the Start Of Image marker.
    e.buf[0] = 255;
    e.buf[1] = 216;
    e.write(e.buf[..2]);
    // Write the quantization tables.
    e.writeDQT();
    // Write the image dimensions.
    e.writeSOF0(b.Size(), nComponent);
    // Write the Huffman tables.
    e.writeDHT(nComponent);
    // Write the image data.
    e.writeSOS(m);
    // Write the End Of Image marker.
    e.buf[0] = 255;
    e.buf[1] = 217;
    e.write(e.buf[..2]);
    e.flush();
    return e.err;
}

} // end jpeg_package
