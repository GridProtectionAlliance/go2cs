// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jpeg -- go2cs converted at 2020 August 29 10:10:20 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Go\src\image\jpeg\writer.go
using bufio = go.bufio_package;
using errors = go.errors_package;
using image = go.image_package;
using color = go.image.color_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace image
{
    public static partial class jpeg_package
    {
        // min returns the minimum of two integers.
        private static long min(long x, long y)
        {
            if (x < y)
            {
                return x;
            }
            return y;
        }

        // div returns a/b rounded to the nearest integer, instead of rounded to zero.
        private static int div(int a, int b)
        {
            if (a >= 0L)
            {
                return (a + (b >> (int)(1L))) / b;
            }
            return -((-a + (b >> (int)(1L))) / b);
        }

        // bitCount counts the number of bits needed to hold an integer.
        private static array<byte> bitCount = new array<byte>(new byte[] { 0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 });

        private partial struct quantIndex // : long
        {
        }

        private static readonly quantIndex quantIndexLuminance = iota;
        private static readonly var quantIndexChrominance = 0;
        private static readonly var nQuantIndex = 1;

        // unscaledQuant are the unscaled quantization tables in zig-zag order. Each
        // encoder copies and scales the tables according to its quality parameter.
        // The values are derived from section K.1 after converting from natural to
        // zig-zag order.
        private static array<array<byte>> unscaledQuant = new array<array<byte>>(new array<byte>[] { {16,11,12,14,12,10,16,14,13,14,18,17,16,19,24,40,26,24,22,22,24,49,35,37,29,40,58,51,61,60,57,51,56,55,64,72,92,78,64,68,87,69,55,56,80,109,81,87,95,98,103,104,103,62,77,113,121,112,100,120,92,101,103,99,}, {17,18,18,24,21,24,47,26,26,47,99,66,56,66,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,} });

        private partial struct huffIndex // : long
        {
        }

        private static readonly huffIndex huffIndexLuminanceDC = iota;
        private static readonly var huffIndexLuminanceAC = 0;
        private static readonly var huffIndexChrominanceDC = 1;
        private static readonly var huffIndexChrominanceAC = 2;
        private static readonly var nHuffIndex = 3;

        // huffmanSpec specifies a Huffman encoding.
        private partial struct huffmanSpec
        {
            public array<byte> count; // value[i] is the decoded value of the i'th codeword.
            public slice<byte> value;
        }

        // theHuffmanSpec is the Huffman encoding specifications.
        // This encoder uses the same Huffman encoding for all images.
        private static array<huffmanSpec> theHuffmanSpec = new array<huffmanSpec>(new huffmanSpec[] { {[16]byte{0,1,5,1,1,1,1,1,1,0,0,0,0,0,0,0},[]byte{0,1,2,3,4,5,6,7,8,9,10,11},}, {[16]byte{0,2,1,3,3,2,4,3,5,5,4,4,0,0,1,125},[]byte{0x01,0x02,0x03,0x00,0x04,0x11,0x05,0x12,0x21,0x31,0x41,0x06,0x13,0x51,0x61,0x07,0x22,0x71,0x14,0x32,0x81,0x91,0xa1,0x08,0x23,0x42,0xb1,0xc1,0x15,0x52,0xd1,0xf0,0x24,0x33,0x62,0x72,0x82,0x09,0x0a,0x16,0x17,0x18,0x19,0x1a,0x25,0x26,0x27,0x28,0x29,0x2a,0x34,0x35,0x36,0x37,0x38,0x39,0x3a,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x4a,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5a,0x63,0x64,0x65,0x66,0x67,0x68,0x69,0x6a,0x73,0x74,0x75,0x76,0x77,0x78,0x79,0x7a,0x83,0x84,0x85,0x86,0x87,0x88,0x89,0x8a,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9a,0xa2,0xa3,0xa4,0xa5,0xa6,0xa7,0xa8,0xa9,0xaa,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,0xc2,0xc3,0xc4,0xc5,0xc6,0xc7,0xc8,0xc9,0xca,0xd2,0xd3,0xd4,0xd5,0xd6,0xd7,0xd8,0xd9,0xda,0xe1,0xe2,0xe3,0xe4,0xe5,0xe6,0xe7,0xe8,0xe9,0xea,0xf1,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf8,0xf9,0xfa,},}, {[16]byte{0,3,1,1,1,1,1,1,1,1,1,0,0,0,0,0},[]byte{0,1,2,3,4,5,6,7,8,9,10,11},}, {[16]byte{0,2,1,2,4,4,3,4,7,5,4,4,0,1,2,119},[]byte{0x00,0x01,0x02,0x03,0x11,0x04,0x05,0x21,0x31,0x06,0x12,0x41,0x51,0x07,0x61,0x71,0x13,0x22,0x32,0x81,0x08,0x14,0x42,0x91,0xa1,0xb1,0xc1,0x09,0x23,0x33,0x52,0xf0,0x15,0x62,0x72,0xd1,0x0a,0x16,0x24,0x34,0xe1,0x25,0xf1,0x17,0x18,0x19,0x1a,0x26,0x27,0x28,0x29,0x2a,0x35,0x36,0x37,0x38,0x39,0x3a,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x4a,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5a,0x63,0x64,0x65,0x66,0x67,0x68,0x69,0x6a,0x73,0x74,0x75,0x76,0x77,0x78,0x79,0x7a,0x82,0x83,0x84,0x85,0x86,0x87,0x88,0x89,0x8a,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9a,0xa2,0xa3,0xa4,0xa5,0xa6,0xa7,0xa8,0xa9,0xaa,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,0xc2,0xc3,0xc4,0xc5,0xc6,0xc7,0xc8,0xc9,0xca,0xd2,0xd3,0xd4,0xd5,0xd6,0xd7,0xd8,0xd9,0xda,0xe2,0xe3,0xe4,0xe5,0xe6,0xe7,0xe8,0xe9,0xea,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf8,0xf9,0xfa,},} });

        // huffmanLUT is a compiled look-up table representation of a huffmanSpec.
        // Each value maps to a uint32 of which the 8 most significant bits hold the
        // codeword size in bits and the 24 least significant bits hold the codeword.
        // The maximum codeword size is 16 bits.
        private partial struct huffmanLUT // : slice<uint>
        {
        }

        private static void init(this ref huffmanLUT h, huffmanSpec s)
        {
            long maxValue = 0L;
            foreach (var (_, v) in s.value)
            {
                if (int(v) > maxValue)
                {
                    maxValue = int(v);
                }
            }
            h.Value = make_slice<uint>(maxValue + 1L);
            var code = uint32(0L);
            long k = 0L;
            for (long i = 0L; i < len(s.count); i++)
            {
                var nBits = uint32(i + 1L) << (int)(24L);
                for (var j = uint8(0L); j < s.count[i]; j++)
                {
                    (h.Value)[s.value[k]] = nBits | code;
                    code++;
                    k++;
                }

                code <<= 1L;
            }

        }

        // theHuffmanLUT are compiled representations of theHuffmanSpec.
        private static array<huffmanLUT> theHuffmanLUT = new array<huffmanLUT>(4L);

        private static void init()
        {
            foreach (var (i, s) in theHuffmanSpec)
            {
                theHuffmanLUT[i].init(s);
            }
        }

        // writer is a buffered writer.
        private partial interface writer : io.Writer, io.ByteWriter
        {
            error Flush();
        }

        // encoder encodes an image to the JPEG format.
        private partial struct encoder
        {
            public writer w;
            public error err; // buf is a scratch buffer.
            public array<byte> buf; // bits and nBits are accumulated bits to write to w.
            public uint bits; // quant is the scaled quantization tables, in zig-zag order.
            public uint nBits; // quant is the scaled quantization tables, in zig-zag order.
            public array<array<byte>> quant;
        }

        private static void flush(this ref encoder e)
        {
            if (e.err != null)
            {
                return;
            }
            e.err = e.w.Flush();
        }

        private static void write(this ref encoder e, slice<byte> p)
        {
            if (e.err != null)
            {
                return;
            }
            _, e.err = e.w.Write(p);
        }

        private static void writeByte(this ref encoder e, byte b)
        {
            if (e.err != null)
            {
                return;
            }
            e.err = e.w.WriteByte(b);
        }

        // emit emits the least significant nBits bits of bits to the bit-stream.
        // The precondition is bits < 1<<nBits && nBits <= 16.
        private static void emit(this ref encoder e, uint bits, uint nBits)
        {
            nBits += e.nBits;
            bits <<= 32L - nBits;
            bits |= e.bits;
            while (nBits >= 8L)
            {
                var b = uint8(bits >> (int)(24L));
                e.writeByte(b);
                if (b == 0xffUL)
                {
                    e.writeByte(0x00UL);
                }
                bits <<= 8L;
                nBits -= 8L;
            }

            e.bits = bits;
            e.nBits = nBits;
        }

        // emitHuff emits the given value with the given Huffman encoder.
        private static void emitHuff(this ref encoder e, huffIndex h, int value)
        {
            var x = theHuffmanLUT[h][value];
            e.emit(x & (1L << (int)(24L) - 1L), x >> (int)(24L));
        }

        // emitHuffRLE emits a run of runLength copies of value encoded with the given
        // Huffman encoder.
        private static void emitHuffRLE(this ref encoder e, huffIndex h, int runLength, int value)
        {
            var a = value;
            var b = value;
            if (a < 0L)
            {
                a = -value;
                b = value - 1L;
            }
            uint nBits = default;
            if (a < 0x100UL)
            {
                nBits = uint32(bitCount[a]);
            }
            else
            {
                nBits = 8L + uint32(bitCount[a >> (int)(8L)]);
            }
            e.emitHuff(h, runLength << (int)(4L) | int32(nBits));
            if (nBits > 0L)
            {
                e.emit(uint32(b) & (1L << (int)(nBits) - 1L), nBits);
            }
        }

        // writeMarkerHeader writes the header for a marker with the given length.
        private static void writeMarkerHeader(this ref encoder e, byte marker, long markerlen)
        {
            e.buf[0L] = 0xffUL;
            e.buf[1L] = marker;
            e.buf[2L] = uint8(markerlen >> (int)(8L));
            e.buf[3L] = uint8(markerlen & 0xffUL);
            e.write(e.buf[..4L]);
        }

        // writeDQT writes the Define Quantization Table marker.
        private static void writeDQT(this ref encoder e)
        {
            const long markerlen = 2L + int(nQuantIndex) * (1L + blockSize);

            e.writeMarkerHeader(dqtMarker, markerlen);
            foreach (var (i) in e.quant)
            {
                e.writeByte(uint8(i));
                e.write(e.quant[i][..]);
            }
        }

        // writeSOF0 writes the Start Of Frame (Baseline Sequential) marker.
        private static void writeSOF0(this ref encoder e, image.Point size, long nComponent)
        {
            long markerlen = 8L + 3L * nComponent;
            e.writeMarkerHeader(sof0Marker, markerlen);
            e.buf[0L] = 8L; // 8-bit color.
            e.buf[1L] = uint8(size.Y >> (int)(8L));
            e.buf[2L] = uint8(size.Y & 0xffUL);
            e.buf[3L] = uint8(size.X >> (int)(8L));
            e.buf[4L] = uint8(size.X & 0xffUL);
            e.buf[5L] = uint8(nComponent);
            if (nComponent == 1L)
            {
                e.buf[6L] = 1L; 
                // No subsampling for grayscale image.
                e.buf[7L] = 0x11UL;
                e.buf[8L] = 0x00UL;
            }
            else
            {
                for (long i = 0L; i < nComponent; i++)
                {
                    e.buf[3L * i + 6L] = uint8(i + 1L); 
                    // We use 4:2:0 chroma subsampling.
                    e.buf[3L * i + 7L] = "\x22\x11\x11"[i];
                    e.buf[3L * i + 8L] = "\x00\x01\x01"[i];
                }

            }
            e.write(e.buf[..3L * (nComponent - 1L) + 9L]);
        }

        // writeDHT writes the Define Huffman Table marker.
        private static void writeDHT(this ref encoder e, long nComponent)
        {
            long markerlen = 2L;
            var specs = theHuffmanSpec[..];
            if (nComponent == 1L)
            { 
                // Drop the Chrominance tables.
                specs = specs[..2L];
            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in specs)
                {
                    s = __s;
                    markerlen += 1L + 16L + len(s.value);
                }

                s = s__prev1;
            }

            e.writeMarkerHeader(dhtMarker, markerlen);
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in specs)
                {
                    i = __i;
                    s = __s;
                    e.writeByte("\x00\x10\x01\x11"[i]);
                    e.write(s.count[..]);
                    e.write(s.value);
                }

                s = s__prev1;
            }

        }

        // writeBlock writes a block of pixel data using the given quantization table,
        // returning the post-quantized DC value of the DCT-transformed block. b is in
        // natural (not zig-zag) order.
        private static int writeBlock(this ref encoder e, ref block b, quantIndex q, int prevDC)
        {
            fdct(b); 
            // Emit the DC delta.
            var dc = div(b[0L], 8L * int32(e.quant[q][0L]));
            e.emitHuffRLE(huffIndex(2L * q + 0L), 0L, dc - prevDC); 
            // Emit the AC components.
            var h = huffIndex(2L * q + 1L);
            var runLength = int32(0L);
            for (long zig = 1L; zig < blockSize; zig++)
            {
                var ac = div(b[unzig[zig]], 8L * int32(e.quant[q][zig]));
                if (ac == 0L)
                {
                    runLength++;
                }
                else
                {
                    while (runLength > 15L)
                    {
                        e.emitHuff(h, 0xf0UL);
                        runLength -= 16L;
                    }

                    e.emitHuffRLE(h, runLength, ac);
                    runLength = 0L;
                }
            }

            if (runLength > 0L)
            {
                e.emitHuff(h, 0x00UL);
            }
            return dc;
        }

        // toYCbCr converts the 8x8 region of m whose top-left corner is p to its
        // YCbCr values.
        private static void toYCbCr(image.Image m, image.Point p, ref block yBlock, ref block cbBlock, ref block crBlock)
        {
            var b = m.Bounds();
            var xmax = b.Max.X - 1L;
            var ymax = b.Max.Y - 1L;
            for (long j = 0L; j < 8L; j++)
            {
                for (long i = 0L; i < 8L; i++)
                {
                    var (r, g, b, _) = m.At(min(p.X + i, xmax), min(p.Y + j, ymax)).RGBA();
                    var (yy, cb, cr) = color.RGBToYCbCr(uint8(r >> (int)(8L)), uint8(g >> (int)(8L)), uint8(b >> (int)(8L)));
                    yBlock[8L * j + i] = int32(yy);
                    cbBlock[8L * j + i] = int32(cb);
                    crBlock[8L * j + i] = int32(cr);
                }

            }

        }

        // grayToY stores the 8x8 region of m whose top-left corner is p in yBlock.
        private static void grayToY(ref image.Gray m, image.Point p, ref block yBlock)
        {
            var b = m.Bounds();
            var xmax = b.Max.X - 1L;
            var ymax = b.Max.Y - 1L;
            var pix = m.Pix;
            for (long j = 0L; j < 8L; j++)
            {
                for (long i = 0L; i < 8L; i++)
                {
                    var idx = m.PixOffset(min(p.X + i, xmax), min(p.Y + j, ymax));
                    yBlock[8L * j + i] = int32(pix[idx]);
                }

            }

        }

        // rgbaToYCbCr is a specialized version of toYCbCr for image.RGBA images.
        private static void rgbaToYCbCr(ref image.RGBA m, image.Point p, ref block yBlock, ref block cbBlock, ref block crBlock)
        {
            var b = m.Bounds();
            var xmax = b.Max.X - 1L;
            var ymax = b.Max.Y - 1L;
            for (long j = 0L; j < 8L; j++)
            {
                var sj = p.Y + j;
                if (sj > ymax)
                {
                    sj = ymax;
                }
                var offset = (sj - b.Min.Y) * m.Stride - b.Min.X * 4L;
                for (long i = 0L; i < 8L; i++)
                {
                    var sx = p.X + i;
                    if (sx > xmax)
                    {
                        sx = xmax;
                    }
                    var pix = m.Pix[offset + sx * 4L..];
                    var (yy, cb, cr) = color.RGBToYCbCr(pix[0L], pix[1L], pix[2L]);
                    yBlock[8L * j + i] = int32(yy);
                    cbBlock[8L * j + i] = int32(cb);
                    crBlock[8L * j + i] = int32(cr);
                }

            }

        }

        // yCbCrToYCbCr is a specialized version of toYCbCr for image.YCbCr images.
        private static void yCbCrToYCbCr(ref image.YCbCr m, image.Point p, ref block yBlock, ref block cbBlock, ref block crBlock)
        {
            var b = m.Bounds();
            var xmax = b.Max.X - 1L;
            var ymax = b.Max.Y - 1L;
            for (long j = 0L; j < 8L; j++)
            {
                var sy = p.Y + j;
                if (sy > ymax)
                {
                    sy = ymax;
                }
                for (long i = 0L; i < 8L; i++)
                {
                    var sx = p.X + i;
                    if (sx > xmax)
                    {
                        sx = xmax;
                    }
                    var yi = m.YOffset(sx, sy);
                    var ci = m.COffset(sx, sy);
                    yBlock[8L * j + i] = int32(m.Y[yi]);
                    cbBlock[8L * j + i] = int32(m.Cb[ci]);
                    crBlock[8L * j + i] = int32(m.Cr[ci]);
                }

            }

        }

        // scale scales the 16x16 region represented by the 4 src blocks to the 8x8
        // dst block.
        private static void scale(ref block dst, ref array<block> src)
        {
            for (long i = 0L; i < 4L; i++)
            {
                var dstOff = (i & 2L) << (int)(4L) | (i & 1L) << (int)(2L);
                for (long y = 0L; y < 4L; y++)
                {
                    for (long x = 0L; x < 4L; x++)
                    {
                        long j = 16L * y + 2L * x;
                        var sum = src[i][j] + src[i][j + 1L] + src[i][j + 8L] + src[i][j + 9L];
                        dst[8L * y + x + dstOff] = (sum + 2L) >> (int)(2L);
                    }

                }

            }

        }

        // sosHeaderY is the SOS marker "\xff\xda" followed by 8 bytes:
        //    - the marker length "\x00\x08",
        //    - the number of components "\x01",
        //    - component 1 uses DC table 0 and AC table 0 "\x01\x00",
        //    - the bytes "\x00\x3f\x00". Section B.2.3 of the spec says that for
        //      sequential DCTs, those bytes (8-bit Ss, 8-bit Se, 4-bit Ah, 4-bit Al)
        //      should be 0x00, 0x3f, 0x00<<4 | 0x00.
        private static byte sosHeaderY = new slice<byte>(new byte[] { 0xff, 0xda, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3f, 0x00 });

        // sosHeaderYCbCr is the SOS marker "\xff\xda" followed by 12 bytes:
        //    - the marker length "\x00\x0c",
        //    - the number of components "\x03",
        //    - component 1 uses DC table 0 and AC table 0 "\x01\x00",
        //    - component 2 uses DC table 1 and AC table 1 "\x02\x11",
        //    - component 3 uses DC table 1 and AC table 1 "\x03\x11",
        //    - the bytes "\x00\x3f\x00". Section B.2.3 of the spec says that for
        //      sequential DCTs, those bytes (8-bit Ss, 8-bit Se, 4-bit Ah, 4-bit Al)
        //      should be 0x00, 0x3f, 0x00<<4 | 0x00.
        private static byte sosHeaderYCbCr = new slice<byte>(new byte[] { 0xff, 0xda, 0x00, 0x0c, 0x03, 0x01, 0x00, 0x02, 0x11, 0x03, 0x11, 0x00, 0x3f, 0x00 });

        // writeSOS writes the StartOfScan marker.
        private static void writeSOS(this ref encoder e, image.Image m)
        {
            switch (m.type())
            {
                case ref image.Gray _:
                    e.write(sosHeaderY);
                    break;
                default:
                {
                    e.write(sosHeaderYCbCr);
                    break;
                }
            }
 
            // Scratch buffers to hold the YCbCr values.
            // The blocks are in natural (not zig-zag) order.
            block b = default;            array<block> cb = new array<block>(4L);            array<block> cr = new array<block>(4L); 
            // DC components are delta-encoded.
            int prevDCY = default;            int prevDCCb = default;            int prevDCCr = default;
            var bounds = m.Bounds();
            switch (m.type())
            {
                case ref image.Gray m:
                    {
                        var y__prev1 = y;

                        var y = bounds.Min.Y;

                        while (y < bounds.Max.Y)
                        {
                            {
                                var x__prev2 = x;

                                var x = bounds.Min.X;

                                while (x < bounds.Max.X)
                                {
                                    var p = image.Pt(x, y);
                                    grayToY(m, p, ref b);
                                    prevDCY = e.writeBlock(ref b, 0L, prevDCY);
                                    x += 8L;
                                }


                                x = x__prev2;
                            }
                            y += 8L;
                        }


                        y = y__prev1;
                    }
                    break;
                default:
                {
                    var m = m.type();
                    ref image.RGBA (rgba, _) = m._<ref image.RGBA>();
                    ref image.YCbCr (ycbcr, _) = m._<ref image.YCbCr>();
                    {
                        var y__prev1 = y;

                        y = bounds.Min.Y;

                        while (y < bounds.Max.Y)
                        {
                            {
                                var x__prev2 = x;

                                x = bounds.Min.X;

                                while (x < bounds.Max.X)
                                {
                                    for (long i = 0L; i < 4L; i++)
                                    {
                                        var xOff = (i & 1L) * 8L;
                                        var yOff = (i & 2L) * 4L;
                                        p = image.Pt(x + xOff, y + yOff);
                                        if (rgba != null)
                                        {
                                            rgbaToYCbCr(rgba, p, ref b, ref cb[i], ref cr[i]);
                                        }
                                        else if (ycbcr != null)
                                        {
                                            yCbCrToYCbCr(ycbcr, p, ref b, ref cb[i], ref cr[i]);
                                    x += 16L;
                                        }
                                        else
                                        {
                                            toYCbCr(m, p, ref b, ref cb[i], ref cr[i]);
                            y += 16L;
                                        }
                                        prevDCY = e.writeBlock(ref b, 0L, prevDCY);
                                    }

                                    scale(ref b, ref cb);
                                    prevDCCb = e.writeBlock(ref b, 1L, prevDCCb);
                                    scale(ref b, ref cr);
                                    prevDCCr = e.writeBlock(ref b, 1L, prevDCCr);
                                }


                                x = x__prev2;
                            }
                        }


                        y = y__prev1;
                    }
                    break;
                } 
                // Pad the last byte with 1's.
            } 
            // Pad the last byte with 1's.
            e.emit(0x7fUL, 7L);
        }

        // DefaultQuality is the default quality encoding parameter.
        public static readonly long DefaultQuality = 75L;

        // Options are the encoding parameters.
        // Quality ranges from 1 to 100 inclusive, higher is better.


        // Options are the encoding parameters.
        // Quality ranges from 1 to 100 inclusive, higher is better.
        public partial struct Options
        {
            public long Quality;
        }

        // Encode writes the Image m to w in JPEG 4:2:0 baseline format with the given
        // options. Default parameters are used if a nil *Options is passed.
        public static error Encode(io.Writer w, image.Image m, ref Options o)
        {
            var b = m.Bounds();
            if (b.Dx() >= 1L << (int)(16L) || b.Dy() >= 1L << (int)(16L))
            {
                return error.As(errors.New("jpeg: image is too large to encode"));
            }
            encoder e = default;
            {
                writer (ww, ok) = w._<writer>();

                if (ok)
                {
                    e.w = ww;
                }
                else
                {
                    e.w = bufio.NewWriter(w);
                } 
                // Clip quality to [1, 100].

            } 
            // Clip quality to [1, 100].
            var quality = DefaultQuality;
            if (o != null)
            {
                quality = o.Quality;
                if (quality < 1L)
                {
                    quality = 1L;
                }
                else if (quality > 100L)
                {
                    quality = 100L;
                }
            } 
            // Convert from a quality rating to a scaling factor.
            long scale = default;
            if (quality < 50L)
            {
                scale = 5000L / quality;
            }
            else
            {
                scale = 200L - quality * 2L;
            } 
            // Initialize the quantization tables.
            foreach (var (i) in e.quant)
            {
                foreach (var (j) in e.quant[i])
                {
                    var x = int(unscaledQuant[i][j]);
                    x = (x * scale + 50L) / 100L;
                    if (x < 1L)
                    {
                        x = 1L;
                    }
                    else if (x > 255L)
                    {
                        x = 255L;
                    }
                    e.quant[i][j] = uint8(x);
                }
            } 
            // Compute number of components based on input image type.
            long nComponent = 3L;
            switch (m.type())
            {
                case ref image.Gray _:
                    nComponent = 1L;
                    break; 
                // Write the Start Of Image marker.
            } 
            // Write the Start Of Image marker.
            e.buf[0L] = 0xffUL;
            e.buf[1L] = 0xd8UL;
            e.write(e.buf[..2L]); 
            // Write the quantization tables.
            e.writeDQT(); 
            // Write the image dimensions.
            e.writeSOF0(b.Size(), nComponent); 
            // Write the Huffman tables.
            e.writeDHT(nComponent); 
            // Write the image data.
            e.writeSOS(m); 
            // Write the End Of Image marker.
            e.buf[0L] = 0xffUL;
            e.buf[1L] = 0xd9UL;
            e.write(e.buf[..2L]);
            e.flush();
            return error.As(e.err);
        }
    }
}}
