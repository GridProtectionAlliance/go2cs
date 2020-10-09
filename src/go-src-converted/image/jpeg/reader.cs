// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package jpeg implements a JPEG image decoder and encoder.
//
// JPEG is defined in ITU-T T.81: https://www.w3.org/Graphics/JPEG/itu-t81.pdf.
// package jpeg -- go2cs converted at 2020 October 09 06:05:54 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Go\src\image\jpeg\reader.go
using image = go.image_package;
using color = go.image.color_package;
using imageutil = go.image.@internal.imageutil_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace image
{
    public static partial class jpeg_package
    {
        // TODO(nigeltao): fix up the doc comment style so that sentences start with
        // the name of the type or function that they annotate.

        // A FormatError reports that the input is not a valid JPEG.
        public partial struct FormatError // : @string
        {
        }

        public static @string Error(this FormatError e)
        {
            return "invalid JPEG format: " + string(e);
        }

        // An UnsupportedError reports that the input uses a valid but unimplemented JPEG feature.
        public partial struct UnsupportedError // : @string
        {
        }

        public static @string Error(this UnsupportedError e)
        {
            return "unsupported JPEG feature: " + string(e);
        }

        private static var errUnsupportedSubsamplingRatio = UnsupportedError("luma/chroma subsampling ratio");

        // Component specification, specified in section B.2.2.
        private partial struct component
        {
            public long h; // Horizontal sampling factor.
            public long v; // Vertical sampling factor.
            public byte c; // Component identifier.
            public byte tq; // Quantization table destination selector.
        }

        private static readonly long dcTable = (long)0L;
        private static readonly long acTable = (long)1L;
        private static readonly long maxTc = (long)1L;
        private static readonly long maxTh = (long)3L;
        private static readonly long maxTq = (long)3L;

        private static readonly long maxComponents = (long)4L;


        private static readonly ulong sof0Marker = (ulong)0xc0UL; // Start Of Frame (Baseline Sequential).
        private static readonly ulong sof1Marker = (ulong)0xc1UL; // Start Of Frame (Extended Sequential).
        private static readonly ulong sof2Marker = (ulong)0xc2UL; // Start Of Frame (Progressive).
        private static readonly ulong dhtMarker = (ulong)0xc4UL; // Define Huffman Table.
        private static readonly ulong rst0Marker = (ulong)0xd0UL; // ReSTart (0).
        private static readonly ulong rst7Marker = (ulong)0xd7UL; // ReSTart (7).
        private static readonly ulong soiMarker = (ulong)0xd8UL; // Start Of Image.
        private static readonly ulong eoiMarker = (ulong)0xd9UL; // End Of Image.
        private static readonly ulong sosMarker = (ulong)0xdaUL; // Start Of Scan.
        private static readonly ulong dqtMarker = (ulong)0xdbUL; // Define Quantization Table.
        private static readonly ulong driMarker = (ulong)0xddUL; // Define Restart Interval.
        private static readonly ulong comMarker = (ulong)0xfeUL; // COMment.
        // "APPlication specific" markers aren't part of the JPEG spec per se,
        // but in practice, their use is described at
        // https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html
        private static readonly ulong app0Marker = (ulong)0xe0UL;
        private static readonly ulong app14Marker = (ulong)0xeeUL;
        private static readonly ulong app15Marker = (ulong)0xefUL;


        // See https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
        private static readonly long adobeTransformUnknown = (long)0L;
        private static readonly long adobeTransformYCbCr = (long)1L;
        private static readonly long adobeTransformYCbCrK = (long)2L;


        // unzig maps from the zig-zag ordering to the natural ordering. For example,
        // unzig[3] is the column and row of the fourth element in zig-zag order. The
        // value is 16, which means first column (16%8 == 0) and third row (16/8 == 2).
        private static array<long> unzig = new array<long>(new long[] { 0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5, 12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28, 35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51, 58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63 });

        // Deprecated: Reader is not used by the image/jpeg package and should
        // not be used by others. It is kept for compatibility.
        public partial interface Reader : io.ByteReader, io.Reader
        {
        }

        // bits holds the unprocessed bits that have been taken from the byte-stream.
        // The n least significant bits of a form the unread bits, to be read in MSB to
        // LSB order.
        private partial struct bits
        {
            public uint a; // accumulator.
            public uint m; // mask. m==1<<(n-1) when n>0, with m==0 when n==0.
            public int n; // the number of unread bits in a.
        }

        private partial struct decoder
        {
            public io.Reader r;
            public bits bits; // bytes is a byte buffer, similar to a bufio.Reader, except that it
// has to be able to unread more than 1 byte, due to byte stuffing.
// Byte stuffing is specified in section F.1.2.3.
            public long width;
            public long height;
            public ptr<image.Gray> img1;
            public ptr<image.YCbCr> img3;
            public slice<byte> blackPix;
            public long blackStride;
            public long ri; // Restart Interval.
            public long nComp; // As per section 4.5, there are four modes of operation (selected by the
// SOF? markers): sequential DCT, progressive DCT, lossless and
// hierarchical, although this implementation does not support the latter
// two non-DCT modes. Sequential DCT is further split into baseline and
// extended, as per section 4.11.
            public bool baseline;
            public bool progressive;
            public bool jfif;
            public bool adobeTransformValid;
            public byte adobeTransform;
            public ushort eobRun; // End-of-Band run, specified in section G.1.2.2.

            public array<component> comp;
            public array<slice<block>> progCoeffs; // Saved state between progressive-mode scans.
            public array<array<huffman>> huff;
            public array<block> quant; // Quantization tables, in zig-zag order.
            public array<byte> tmp;
        }

        // fill fills up the d.bytes.buf buffer from the underlying io.Reader. It
        // should only be called when there are no unread bytes in d.bytes.
        private static error fill(this ptr<decoder> _addr_d) => func((_, panic, __) =>
        {
            ref decoder d = ref _addr_d.val;

            if (d.bytes.i != d.bytes.j)
            {
                panic("jpeg: fill called when unread bytes exist");
            } 
            // Move the last 2 bytes to the start of the buffer, in case we need
            // to call unreadByteStuffedByte.
            if (d.bytes.j > 2L)
            {
                d.bytes.buf[0L] = d.bytes.buf[d.bytes.j - 2L];
                d.bytes.buf[1L] = d.bytes.buf[d.bytes.j - 1L];
                d.bytes.i = 2L;
                d.bytes.j = 2L;

            } 
            // Fill in the rest of the buffer.
            var (n, err) = d.r.Read(d.bytes.buf[d.bytes.j..]);
            d.bytes.j += n;
            if (n > 0L)
            {
                err = null;
            }

            return error.As(err)!;

        });

        // unreadByteStuffedByte undoes the most recent readByteStuffedByte call,
        // giving a byte of data back from d.bits to d.bytes. The Huffman look-up table
        // requires at least 8 bits for look-up, which means that Huffman decoding can
        // sometimes overshoot and read one or two too many bytes. Two-byte overshoot
        // can happen when expecting to read a 0xff 0x00 byte-stuffed byte.
        private static void unreadByteStuffedByte(this ptr<decoder> _addr_d)
        {
            ref decoder d = ref _addr_d.val;

            d.bytes.i -= d.bytes.nUnreadable;
            d.bytes.nUnreadable = 0L;
            if (d.bits.n >= 8L)
            {
                d.bits.a >>= 8L;
                d.bits.n -= 8L;
                d.bits.m >>= 8L;
            }

        }

        // readByte returns the next byte, whether buffered or not buffered. It does
        // not care about byte stuffing.
        private static (byte, error) readByte(this ptr<decoder> _addr_d)
        {
            byte x = default;
            error err = default!;
            ref decoder d = ref _addr_d.val;

            while (d.bytes.i == d.bytes.j)
            {
                err = d.fill();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            x = d.bytes.buf[d.bytes.i];
            d.bytes.i++;
            d.bytes.nUnreadable = 0L;
            return (x, error.As(null!)!);

        }

        // errMissingFF00 means that readByteStuffedByte encountered an 0xff byte (a
        // marker byte) that wasn't the expected byte-stuffed sequence 0xff, 0x00.
        private static var errMissingFF00 = FormatError("missing 0xff00 sequence");

        // readByteStuffedByte is like readByte but is for byte-stuffed Huffman data.
        private static (byte, error) readByteStuffedByte(this ptr<decoder> _addr_d)
        {
            byte x = default;
            error err = default!;
            ref decoder d = ref _addr_d.val;
 
            // Take the fast path if d.bytes.buf contains at least two bytes.
            if (d.bytes.i + 2L <= d.bytes.j)
            {
                x = d.bytes.buf[d.bytes.i];
                d.bytes.i++;
                d.bytes.nUnreadable = 1L;
                if (x != 0xffUL)
                {
                    return (x, error.As(err)!);
                }

                if (d.bytes.buf[d.bytes.i] != 0x00UL)
                {
                    return (0L, error.As(errMissingFF00)!);
                }

                d.bytes.i++;
                d.bytes.nUnreadable = 2L;
                return (0xffUL, error.As(null!)!);

            }

            d.bytes.nUnreadable = 0L;

            x, err = d.readByte();
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            d.bytes.nUnreadable = 1L;
            if (x != 0xffUL)
            {
                return (x, error.As(null!)!);
            }

            x, err = d.readByte();
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            d.bytes.nUnreadable = 2L;
            if (x != 0x00UL)
            {
                return (0L, error.As(errMissingFF00)!);
            }

            return (0xffUL, error.As(null!)!);

        }

        // readFull reads exactly len(p) bytes into p. It does not care about byte
        // stuffing.
        private static error readFull(this ptr<decoder> _addr_d, slice<byte> p)
        {
            ref decoder d = ref _addr_d.val;
 
            // Unread the overshot bytes, if any.
            if (d.bytes.nUnreadable != 0L)
            {
                if (d.bits.n >= 8L)
                {
                    d.unreadByteStuffedByte();
                }

                d.bytes.nUnreadable = 0L;

            }

            while (true)
            {
                var n = copy(p, d.bytes.buf[d.bytes.i..d.bytes.j]);
                p = p[n..];
                d.bytes.i += n;
                if (len(p) == 0L)
                {
                    break;
                }

                {
                    var err = d.fill();

                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
                            err = io.ErrUnexpectedEOF;
                        }

                        return error.As(err)!;

                    }

                }

            }

            return error.As(null!)!;

        }

        // ignore ignores the next n bytes.
        private static error ignore(this ptr<decoder> _addr_d, long n)
        {
            ref decoder d = ref _addr_d.val;
 
            // Unread the overshot bytes, if any.
            if (d.bytes.nUnreadable != 0L)
            {
                if (d.bits.n >= 8L)
                {
                    d.unreadByteStuffedByte();
                }

                d.bytes.nUnreadable = 0L;

            }

            while (true)
            {
                var m = d.bytes.j - d.bytes.i;
                if (m > n)
                {
                    m = n;
                }

                d.bytes.i += m;
                n -= m;
                if (n == 0L)
                {
                    break;
                }

                {
                    var err = d.fill();

                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
                            err = io.ErrUnexpectedEOF;
                        }

                        return error.As(err)!;

                    }

                }

            }

            return error.As(null!)!;

        }

        // Specified in section B.2.2.
        private static error processSOF(this ptr<decoder> _addr_d, long n)
        {
            ref decoder d = ref _addr_d.val;

            if (d.nComp != 0L)
            {
                return error.As(FormatError("multiple SOF markers"))!;
            }

            switch (n)
            {
                case 6L + 3L * 1L: // Grayscale image.
                    d.nComp = 1L;
                    break;
                case 6L + 3L * 3L: // YCbCr or RGB image.
                    d.nComp = 3L;
                    break;
                case 6L + 3L * 4L: // YCbCrK or CMYK image.
                    d.nComp = 4L;
                    break;
                default: 
                    return error.As(UnsupportedError("number of components"))!;
                    break;
            }
            {
                var err = d.readFull(d.tmp[..n]);

                if (err != null)
                {
                    return error.As(err)!;
                } 
                // We only support 8-bit precision.

            } 
            // We only support 8-bit precision.
            if (d.tmp[0L] != 8L)
            {
                return error.As(UnsupportedError("precision"))!;
            }

            d.height = int(d.tmp[1L]) << (int)(8L) + int(d.tmp[2L]);
            d.width = int(d.tmp[3L]) << (int)(8L) + int(d.tmp[4L]);
            if (int(d.tmp[5L]) != d.nComp)
            {
                return error.As(FormatError("SOF has wrong length"))!;
            }

            for (long i = 0L; i < d.nComp; i++)
            {
                d.comp[i].c = d.tmp[6L + 3L * i]; 
                // Section B.2.2 states that "the value of C_i shall be different from
                // the values of C_1 through C_(i-1)".
                for (long j = 0L; j < i; j++)
                {
                    if (d.comp[i].c == d.comp[j].c)
                    {
                        return error.As(FormatError("repeated component identifier"))!;
                    }

                }


                d.comp[i].tq = d.tmp[8L + 3L * i];
                if (d.comp[i].tq > maxTq)
                {
                    return error.As(FormatError("bad Tq value"))!;
                }

                var hv = d.tmp[7L + 3L * i];
                var h = int(hv >> (int)(4L));
                var v = int(hv & 0x0fUL);
                if (h < 1L || 4L < h || v < 1L || 4L < v)
                {
                    return error.As(FormatError("luma/chroma subsampling ratio"))!;
                }

                if (h == 3L || v == 3L)
                {
                    return error.As(errUnsupportedSubsamplingRatio)!;
                }

                switch (d.nComp)
                {
                    case 1L: 
                        // If a JPEG image has only one component, section A.2 says "this data
                        // is non-interleaved by definition" and section A.2.2 says "[in this
                        // case...] the order of data units within a scan shall be left-to-right
                        // and top-to-bottom... regardless of the values of H_1 and V_1". Section
                        // 4.8.2 also says "[for non-interleaved data], the MCU is defined to be
                        // one data unit". Similarly, section A.1.1 explains that it is the ratio
                        // of H_i to max_j(H_j) that matters, and similarly for V. For grayscale
                        // images, H_1 is the maximum H_j for all components j, so that ratio is
                        // always 1. The component's (h, v) is effectively always (1, 1): even if
                        // the nominal (h, v) is (2, 1), a 20x5 image is encoded in three 8x8
                        // MCUs, not two 16x8 MCUs.
                        h = 1L;
                        v = 1L;
                        break;
                    case 3L: 
                        // For YCbCr images, we only support 4:4:4, 4:4:0, 4:2:2, 4:2:0,
                        // 4:1:1 or 4:1:0 chroma subsampling ratios. This implies that the
                        // (h, v) values for the Y component are either (1, 1), (1, 2),
                        // (2, 1), (2, 2), (4, 1) or (4, 2), and the Y component's values
                        // must be a multiple of the Cb and Cr component's values. We also
                        // assume that the two chroma components have the same subsampling
                        // ratio.
                        switch (i)
                        {
                            case 0L: // Y.
                                // We have already verified, above, that h and v are both
                                // either 1, 2 or 4, so invalid (h, v) combinations are those
                                // with v == 4.
                                if (v == 4L)
                                {
                                    return error.As(errUnsupportedSubsamplingRatio)!;
                                }

                                break;
                            case 1L: // Cb.
                                if (d.comp[0L].h % h != 0L || d.comp[0L].v % v != 0L)
                                {
                                    return error.As(errUnsupportedSubsamplingRatio)!;
                                }

                                break;
                            case 2L: // Cr.
                                if (d.comp[1L].h != h || d.comp[1L].v != v)
                                {
                                    return error.As(errUnsupportedSubsamplingRatio)!;
                                }

                                break;
                        }
                        break;
                    case 4L: 
                        // For 4-component images (either CMYK or YCbCrK), we only support two
                        // hv vectors: [0x11 0x11 0x11 0x11] and [0x22 0x11 0x11 0x22].
                        // Theoretically, 4-component JPEG images could mix and match hv values
                        // but in practice, those two combinations are the only ones in use,
                        // and it simplifies the applyBlack code below if we can assume that:
                        //    - for CMYK, the C and K channels have full samples, and if the M
                        //      and Y channels subsample, they subsample both horizontally and
                        //      vertically.
                        //    - for YCbCrK, the Y and K channels have full samples.
                        switch (i)
                        {
                            case 0L: 
                                if (hv != 0x11UL && hv != 0x22UL)
                                {
                                    return error.As(errUnsupportedSubsamplingRatio)!;
                                }

                                break;
                            case 1L: 

                            case 2L: 
                                if (hv != 0x11UL)
                                {
                                    return error.As(errUnsupportedSubsamplingRatio)!;
                                }

                                break;
                            case 3L: 
                                if (d.comp[0L].h != h || d.comp[0L].v != v)
                                {
                                    return error.As(errUnsupportedSubsamplingRatio)!;
                                }

                                break;
                        }
                        break;
                }

                d.comp[i].h = h;
                d.comp[i].v = v;

            }

            return error.As(null!)!;

        }

        // Specified in section B.2.4.1.
        private static error processDQT(this ptr<decoder> _addr_d, long n)
        {
            ref decoder d = ref _addr_d.val;

loop:
            while (n > 0L)
            {
                n--;
                var (x, err) = d.readByte();
                if (err != null)
                {
                    return error.As(err)!;
                }

                var tq = x & 0x0fUL;
                if (tq > maxTq)
                {
                    return error.As(FormatError("bad Tq value"))!;
                }

                switch (x >> (int)(4L))
                {
                    case 0L: 
                        if (n < blockSize)
                        {
                            _breakloop = true;
                            break;
                        }

                        n -= blockSize;
                        {
                            var err__prev1 = err;

                            var err = d.readFull(d.tmp[..blockSize]);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev1;

                        }

                        {
                            var i__prev2 = i;

                            foreach (var (__i) in d.quant[tq])
                            {
                                i = __i;
                                d.quant[tq][i] = int32(d.tmp[i]);
                            }

                            i = i__prev2;
                        }
                        break;
                    case 1L: 
                        if (n < 2L * blockSize)
                        {
                            _breakloop = true;
                            break;
                        }

                        n -= 2L * blockSize;
                        {
                            var err__prev1 = err;

                            err = d.readFull(d.tmp[..2L * blockSize]);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev1;

                        }

                        {
                            var i__prev2 = i;

                            foreach (var (__i) in d.quant[tq])
                            {
                                i = __i;
                                d.quant[tq][i] = int32(d.tmp[2L * i]) << (int)(8L) | int32(d.tmp[2L * i + 1L]);
                            }

                            i = i__prev2;
                        }
                        break;
                    default: 
                        return error.As(FormatError("bad Pq value"))!;
                        break;
                }

            }
            if (n != 0L)
            {
                return error.As(FormatError("DQT has wrong length"))!;
            }

            return error.As(null!)!;

        }

        // Specified in section B.2.4.4.
        private static error processDRI(this ptr<decoder> _addr_d, long n)
        {
            ref decoder d = ref _addr_d.val;

            if (n != 2L)
            {
                return error.As(FormatError("DRI has wrong length"))!;
            }

            {
                var err = d.readFull(d.tmp[..2L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            d.ri = int(d.tmp[0L]) << (int)(8L) + int(d.tmp[1L]);
            return error.As(null!)!;

        }

        private static error processApp0Marker(this ptr<decoder> _addr_d, long n)
        {
            ref decoder d = ref _addr_d.val;

            if (n < 5L)
            {
                return error.As(d.ignore(n))!;
            }

            {
                var err = d.readFull(d.tmp[..5L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            n -= 5L;

            d.jfif = d.tmp[0L] == 'J' && d.tmp[1L] == 'F' && d.tmp[2L] == 'I' && d.tmp[3L] == 'F' && d.tmp[4L] == '\x00';

            if (n > 0L)
            {
                return error.As(d.ignore(n))!;
            }

            return error.As(null!)!;

        }

        private static error processApp14Marker(this ptr<decoder> _addr_d, long n)
        {
            ref decoder d = ref _addr_d.val;

            if (n < 12L)
            {
                return error.As(d.ignore(n))!;
            }

            {
                var err = d.readFull(d.tmp[..12L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            n -= 12L;

            if (d.tmp[0L] == 'A' && d.tmp[1L] == 'd' && d.tmp[2L] == 'o' && d.tmp[3L] == 'b' && d.tmp[4L] == 'e')
            {
                d.adobeTransformValid = true;
                d.adobeTransform = d.tmp[11L];
            }

            if (n > 0L)
            {
                return error.As(d.ignore(n))!;
            }

            return error.As(null!)!;

        }

        // decode reads a JPEG image from r and returns it as an image.Image.
        private static (image.Image, error) decode(this ptr<decoder> _addr_d, io.Reader r, bool configOnly)
        {
            image.Image _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            d.r = r; 

            // Check for the Start Of Image marker.
            {
                var err__prev1 = err;

                var err = d.readFull(d.tmp[..2L]);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            if (d.tmp[0L] != 0xffUL || d.tmp[1L] != soiMarker)
            {
                return (null, error.As(FormatError("missing SOI marker"))!);
            } 

            // Process the remaining segments until the End Of Image marker.
            while (true)
            {
                err = d.readFull(d.tmp[..2L]);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                while (d.tmp[0L] != 0xffUL)
                { 
                    // Strictly speaking, this is a format error. However, libjpeg is
                    // liberal in what it accepts. As of version 9, next_marker in
                    // jdmarker.c treats this as a warning (JWRN_EXTRANEOUS_DATA) and
                    // continues to decode the stream. Even before next_marker sees
                    // extraneous data, jpeg_fill_bit_buffer in jdhuff.c reads as many
                    // bytes as it can, possibly past the end of a scan's data. It
                    // effectively puts back any markers that it overscanned (e.g. an
                    // "\xff\xd9" EOI marker), but it does not put back non-marker data,
                    // and thus it can silently ignore a small number of extraneous
                    // non-marker bytes before next_marker has a chance to see them (and
                    // print a warning).
                    //
                    // We are therefore also liberal in what we accept. Extraneous data
                    // is silently ignored.
                    //
                    // This is similar to, but not exactly the same as, the restart
                    // mechanism within a scan (the RST[0-7] markers).
                    //
                    // Note that extraneous 0xff bytes in e.g. SOS data are escaped as
                    // "\xff\x00", and so are detected a little further down below.
                    d.tmp[0L] = d.tmp[1L];
                    d.tmp[1L], err = d.readByte();
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

                var marker = d.tmp[1L];
                if (marker == 0L)
                { 
                    // Treat "\xff\x00" as extraneous data.
                    continue;

                }

                while (marker == 0xffUL)
                { 
                    // Section B.1.1.2 says, "Any marker may optionally be preceded by any
                    // number of fill bytes, which are bytes assigned code X'FF'".
                    marker, err = d.readByte();
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

                if (marker == eoiMarker)
                { // End Of Image.
                    break;

                }

                if (rst0Marker <= marker && marker <= rst7Marker)
                { 
                    // Figures B.2 and B.16 of the specification suggest that restart markers should
                    // only occur between Entropy Coded Segments and not after the final ECS.
                    // However, some encoders may generate incorrect JPEGs with a final restart
                    // marker. That restart marker will be seen here instead of inside the processSOS
                    // method, and is ignored as a harmless error. Restart markers have no extra data,
                    // so we check for this before we read the 16-bit length of the segment.
                    continue;

                } 

                // Read the 16-bit length of the segment. The value includes the 2 bytes for the
                // length itself, so we subtract 2 to get the number of remaining bytes.
                err = d.readFull(d.tmp[..2L]);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                var n = int(d.tmp[0L]) << (int)(8L) + int(d.tmp[1L]) - 2L;
                if (n < 0L)
                {
                    return (null, error.As(FormatError("short segment length"))!);
                }


                if (marker == sof0Marker || marker == sof1Marker || marker == sof2Marker) 
                    d.baseline = marker == sof0Marker;
                    d.progressive = marker == sof2Marker;
                    err = d.processSOF(n);
                    if (configOnly && d.jfif)
                    {
                        return (null, error.As(err)!);
                    }

                else if (marker == dhtMarker) 
                    if (configOnly)
                    {
                        err = d.ignore(n);
                    }
                    else
                    {
                        err = d.processDHT(n);
                    }

                else if (marker == dqtMarker) 
                    if (configOnly)
                    {
                        err = d.ignore(n);
                    }
                    else
                    {
                        err = d.processDQT(n);
                    }

                else if (marker == sosMarker) 
                    if (configOnly)
                    {
                        return (null, error.As(null!)!);
                    }

                    err = d.processSOS(n);
                else if (marker == driMarker) 
                    if (configOnly)
                    {
                        err = d.ignore(n);
                    }
                    else
                    {
                        err = d.processDRI(n);
                    }

                else if (marker == app0Marker) 
                    err = d.processApp0Marker(n);
                else if (marker == app14Marker) 
                    err = d.processApp14Marker(n);
                else 
                    if (app0Marker <= marker && marker <= app15Marker || marker == comMarker)
                    {
                        err = d.ignore(n);
                    }
                    else if (marker < 0xc0UL)
                    { // See Table B.1 "Marker code assignments".
                        err = FormatError("unknown marker");

                    }
                    else
                    {
                        err = UnsupportedError("unknown marker");
                    }

                                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }


            if (d.progressive)
            {
                {
                    var err__prev2 = err;

                    err = d.reconstructProgressiveImage();

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            if (d.img1 != null)
            {
                return (d.img1, error.As(null!)!);
            }

            if (d.img3 != null)
            {
                if (d.blackPix != null)
                {
                    return d.applyBlack();
                }
                else if (d.isRGB())
                {
                    return d.convertToRGB();
                }

                return (d.img3, error.As(null!)!);

            }

            return (null, error.As(FormatError("missing SOS marker"))!);

        }

        // applyBlack combines d.img3 and d.blackPix into a CMYK image. The formula
        // used depends on whether the JPEG image is stored as CMYK or YCbCrK,
        // indicated by the APP14 (Adobe) metadata.
        //
        // Adobe CMYK JPEG images are inverted, where 255 means no ink instead of full
        // ink, so we apply "v = 255 - v" at various points. Note that a double
        // inversion is a no-op, so inversions might be implicit in the code below.
        private static (image.Image, error) applyBlack(this ptr<decoder> _addr_d)
        {
            image.Image _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            if (!d.adobeTransformValid)
            {
                return (null, error.As(UnsupportedError("unknown color model: 4-component JPEG doesn't have Adobe APP14 metadata"))!);
            } 

            // If the 4-component JPEG image isn't explicitly marked as "Unknown (RGB
            // or CMYK)" as per
            // https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
            // we assume that it is YCbCrK. This matches libjpeg's jdapimin.c.
            if (d.adobeTransform != adobeTransformUnknown)
            { 
                // Convert the YCbCr part of the YCbCrK to RGB, invert the RGB to get
                // CMY, and patch in the original K. The RGB to CMY inversion cancels
                // out the 'Adobe inversion' described in the applyBlack doc comment
                // above, so in practice, only the fourth channel (black) is inverted.
                var bounds = d.img3.Bounds();
                var img = image.NewRGBA(bounds);
                imageutil.DrawYCbCr(img, bounds, d.img3, bounds.Min);
                {
                    long iBase__prev1 = iBase;
                    var y__prev1 = y;

                    long iBase = 0L;
                    var y = bounds.Min.Y;

                    while (y < bounds.Max.Y)
                    {
                        {
                            var i__prev2 = i;
                            var x__prev2 = x;

                            var i = iBase + 3L;
                            var x = bounds.Min.X;

                            while (x < bounds.Max.X)
                            {
                                img.Pix[i] = 255L - d.blackPix[(y - bounds.Min.Y) * d.blackStride + (x - bounds.Min.X)];
                                i = i + 4L;
                            x = x + 1L;
                            }


                            i = i__prev2;
                            x = x__prev2;
                        }
                        iBase = iBase + img.Stride;
                    y = y + 1L;
                    }


                    iBase = iBase__prev1;
                    y = y__prev1;
                }
                return (addr(new image.CMYK(Pix:img.Pix,Stride:img.Stride,Rect:img.Rect,)), error.As(null!)!);

            } 

            // The first three channels (cyan, magenta, yellow) of the CMYK
            // were decoded into d.img3, but each channel was decoded into a separate
            // []byte slice, and some channels may be subsampled. We interleave the
            // separate channels into an image.CMYK's single []byte slice containing 4
            // contiguous bytes per pixel.
            bounds = d.img3.Bounds();
            img = image.NewCMYK(bounds);

            foreach (var (t, translation) in translations)
            {
                var subsample = d.comp[t].h != d.comp[0L].h || d.comp[t].v != d.comp[0L].v;
                {
                    long iBase__prev2 = iBase;
                    var y__prev2 = y;

                    iBase = 0L;
                    y = bounds.Min.Y;

                    while (y < bounds.Max.Y)
                    {
                        var sy = y - bounds.Min.Y;
                        if (subsample)
                        {
                            sy /= 2L;
                        iBase = iBase + img.Stride;
                    y = y + 1L;
                        }

                        {
                            var i__prev3 = i;
                            var x__prev3 = x;

                            i = iBase + t;
                            x = bounds.Min.X;

                            while (x < bounds.Max.X)
                            {
                                var sx = x - bounds.Min.X;
                                if (subsample)
                                {
                                    sx /= 2L;
                                i = i + 4L;
                            x = x + 1L;
                                }

                                img.Pix[i] = 255L - translation.src[sy * translation.stride + sx];

                            }


                            i = i__prev3;
                            x = x__prev3;
                        }

                    }


                    iBase = iBase__prev2;
                    y = y__prev2;
                }

            }
            return (img, error.As(null!)!);

        }

        private static bool isRGB(this ptr<decoder> _addr_d)
        {
            ref decoder d = ref _addr_d.val;

            if (d.jfif)
            {
                return false;
            }

            if (d.adobeTransformValid && d.adobeTransform == adobeTransformUnknown)
            { 
                // https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
                // says that 0 means Unknown (and in practice RGB) and 1 means YCbCr.
                return true;

            }

            return d.comp[0L].c == 'R' && d.comp[1L].c == 'G' && d.comp[2L].c == 'B';

        }

        private static (image.Image, error) convertToRGB(this ptr<decoder> _addr_d)
        {
            image.Image _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            var cScale = d.comp[0L].h / d.comp[1L].h;
            var bounds = d.img3.Bounds();
            var img = image.NewRGBA(bounds);
            for (var y = bounds.Min.Y; y < bounds.Max.Y; y++)
            {
                var po = img.PixOffset(bounds.Min.X, y);
                var yo = d.img3.YOffset(bounds.Min.X, y);
                var co = d.img3.COffset(bounds.Min.X, y);
                for (long i = 0L;
                var iMax = bounds.Max.X - bounds.Min.X; i < iMax; i++)
                {
                    img.Pix[po + 4L * i + 0L] = d.img3.Y[yo + i];
                    img.Pix[po + 4L * i + 1L] = d.img3.Cb[co + i / cScale];
                    img.Pix[po + 4L * i + 2L] = d.img3.Cr[co + i / cScale];
                    img.Pix[po + 4L * i + 3L] = 255L;
                }


            }

            return (img, error.As(null!)!);

        }

        // Decode reads a JPEG image from r and returns it as an image.Image.
        public static (image.Image, error) Decode(io.Reader r)
        {
            image.Image _p0 = default;
            error _p0 = default!;

            decoder d = default;
            return d.decode(r, false);
        }

        // DecodeConfig returns the color model and dimensions of a JPEG image without
        // decoding the entire image.
        public static (image.Config, error) DecodeConfig(io.Reader r)
        {
            image.Config _p0 = default;
            error _p0 = default!;

            decoder d = default;
            {
                var (_, err) = d.decode(r, true);

                if (err != null)
                {
                    return (new image.Config(), error.As(err)!);
                }

            }

            switch (d.nComp)
            {
                case 1L: 
                    return (new image.Config(ColorModel:color.GrayModel,Width:d.width,Height:d.height,), error.As(null!)!);
                    break;
                case 3L: 
                    var cm = color.YCbCrModel;
                    if (d.isRGB())
                    {
                        cm = color.RGBAModel;
                    }

                    return (new image.Config(ColorModel:cm,Width:d.width,Height:d.height,), error.As(null!)!);
                    break;
                case 4L: 
                    return (new image.Config(ColorModel:color.CMYKModel,Width:d.width,Height:d.height,), error.As(null!)!);
                    break;
            }
            return (new image.Config(), error.As(FormatError("missing SOF marker"))!);

        }

        private static void init()
        {
            image.RegisterFormat("jpeg", "\xff\xd8", Decode, DecodeConfig);
        }
    }
}}
