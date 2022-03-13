// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package jpeg implements a JPEG image decoder and encoder.
//
// JPEG is defined in ITU-T T.81: https://www.w3.org/Graphics/JPEG/itu-t81.pdf.

// package jpeg -- go2cs converted at 2022 March 13 06:44:10 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Program Files\Go\src\image\jpeg\reader.go
namespace go.image;

using image = image_package;
using color = image.color_package;
using imageutil = image.@internal.imageutil_package;
using io = io_package;


// A FormatError reports that the input is not a valid JPEG.

public static partial class jpeg_package {

public partial struct FormatError { // : @string
}

public static @string Error(this FormatError e) {
    return "invalid JPEG format: " + string(e);
}

// An UnsupportedError reports that the input uses a valid but unimplemented JPEG feature.
public partial struct UnsupportedError { // : @string
}

public static @string Error(this UnsupportedError e) {
    return "unsupported JPEG feature: " + string(e);
}

private static var errUnsupportedSubsamplingRatio = UnsupportedError("luma/chroma subsampling ratio");

// Component specification, specified in section B.2.2.
private partial struct component {
    public nint h; // Horizontal sampling factor.
    public nint v; // Vertical sampling factor.
    public byte c; // Component identifier.
    public byte tq; // Quantization table destination selector.
}

private static readonly nint dcTable = 0;
private static readonly nint acTable = 1;
private static readonly nint maxTc = 1;
private static readonly nint maxTh = 3;
private static readonly nint maxTq = 3;

private static readonly nint maxComponents = 4;

private static readonly nuint sof0Marker = 0xc0; // Start Of Frame (Baseline Sequential).
private static readonly nuint sof1Marker = 0xc1; // Start Of Frame (Extended Sequential).
private static readonly nuint sof2Marker = 0xc2; // Start Of Frame (Progressive).
private static readonly nuint dhtMarker = 0xc4; // Define Huffman Table.
private static readonly nuint rst0Marker = 0xd0; // ReSTart (0).
private static readonly nuint rst7Marker = 0xd7; // ReSTart (7).
private static readonly nuint soiMarker = 0xd8; // Start Of Image.
private static readonly nuint eoiMarker = 0xd9; // End Of Image.
private static readonly nuint sosMarker = 0xda; // Start Of Scan.
private static readonly nuint dqtMarker = 0xdb; // Define Quantization Table.
private static readonly nuint driMarker = 0xdd; // Define Restart Interval.
private static readonly nuint comMarker = 0xfe; // COMment.
// "APPlication specific" markers aren't part of the JPEG spec per se,
// but in practice, their use is described at
// https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html
private static readonly nuint app0Marker = 0xe0;
private static readonly nuint app14Marker = 0xee;
private static readonly nuint app15Marker = 0xef;

// See https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
private static readonly nint adobeTransformUnknown = 0;
private static readonly nint adobeTransformYCbCr = 1;
private static readonly nint adobeTransformYCbCrK = 2;

// unzig maps from the zig-zag ordering to the natural ordering. For example,
// unzig[3] is the column and row of the fourth element in zig-zag order. The
// value is 16, which means first column (16%8 == 0) and third row (16/8 == 2).
private static array<nint> unzig = new array<nint>(new nint[] { 0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5, 12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28, 35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51, 58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63 });

// Deprecated: Reader is not used by the image/jpeg package and should
// not be used by others. It is kept for compatibility.
public partial interface Reader {
}

// bits holds the unprocessed bits that have been taken from the byte-stream.
// The n least significant bits of a form the unread bits, to be read in MSB to
// LSB order.
private partial struct bits {
    public uint a; // accumulator.
    public uint m; // mask. m==1<<(n-1) when n>0, with m==0 when n==0.
    public int n; // the number of unread bits in a.
}

private partial struct decoder {
    public io.Reader r;
    public bits bits; // bytes is a byte buffer, similar to a bufio.Reader, except that it
// has to be able to unread more than 1 byte, due to byte stuffing.
// Byte stuffing is specified in section F.1.2.3.
    public nint width;
    public nint height;
    public ptr<image.Gray> img1;
    public ptr<image.YCbCr> img3;
    public slice<byte> blackPix;
    public nint blackStride;
    public nint ri; // Restart Interval.
    public nint nComp; // As per section 4.5, there are four modes of operation (selected by the
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
private static error fill(this ptr<decoder> _addr_d) => func((_, panic, _) => {
    ref decoder d = ref _addr_d.val;

    if (d.bytes.i != d.bytes.j) {
        panic("jpeg: fill called when unread bytes exist");
    }
    if (d.bytes.j > 2) {
        d.bytes.buf[0] = d.bytes.buf[d.bytes.j - 2];
        d.bytes.buf[1] = d.bytes.buf[d.bytes.j - 1];
        (d.bytes.i, d.bytes.j) = (2, 2);
    }
    var (n, err) = d.r.Read(d.bytes.buf[(int)d.bytes.j..]);
    d.bytes.j += n;
    if (n > 0) {
        err = null;
    }
    return error.As(err)!;
});

// unreadByteStuffedByte undoes the most recent readByteStuffedByte call,
// giving a byte of data back from d.bits to d.bytes. The Huffman look-up table
// requires at least 8 bits for look-up, which means that Huffman decoding can
// sometimes overshoot and read one or two too many bytes. Two-byte overshoot
// can happen when expecting to read a 0xff 0x00 byte-stuffed byte.
private static void unreadByteStuffedByte(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;

    d.bytes.i -= d.bytes.nUnreadable;
    d.bytes.nUnreadable = 0;
    if (d.bits.n >= 8) {
        d.bits.a>>=8;
        d.bits.n -= 8;
        d.bits.m>>=8;
    }
}

// readByte returns the next byte, whether buffered or not buffered. It does
// not care about byte stuffing.
private static (byte, error) readByte(this ptr<decoder> _addr_d) {
    byte x = default;
    error err = default!;
    ref decoder d = ref _addr_d.val;

    while (d.bytes.i == d.bytes.j) {
        err = d.fill();

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    x = d.bytes.buf[d.bytes.i];
    d.bytes.i++;
    d.bytes.nUnreadable = 0;
    return (x, error.As(null!)!);
}

// errMissingFF00 means that readByteStuffedByte encountered an 0xff byte (a
// marker byte) that wasn't the expected byte-stuffed sequence 0xff, 0x00.
private static var errMissingFF00 = FormatError("missing 0xff00 sequence");

// readByteStuffedByte is like readByte but is for byte-stuffed Huffman data.
private static (byte, error) readByteStuffedByte(this ptr<decoder> _addr_d) {
    byte x = default;
    error err = default!;
    ref decoder d = ref _addr_d.val;
 
    // Take the fast path if d.bytes.buf contains at least two bytes.
    if (d.bytes.i + 2 <= d.bytes.j) {
        x = d.bytes.buf[d.bytes.i];
        d.bytes.i++;
        d.bytes.nUnreadable = 1;
        if (x != 0xff) {
            return (x, error.As(err)!);
        }
        if (d.bytes.buf[d.bytes.i] != 0x00) {
            return (0, error.As(errMissingFF00)!);
        }
        d.bytes.i++;
        d.bytes.nUnreadable = 2;
        return (0xff, error.As(null!)!);
    }
    d.bytes.nUnreadable = 0;

    x, err = d.readByte();
    if (err != null) {
        return (0, error.As(err)!);
    }
    d.bytes.nUnreadable = 1;
    if (x != 0xff) {
        return (x, error.As(null!)!);
    }
    x, err = d.readByte();
    if (err != null) {
        return (0, error.As(err)!);
    }
    d.bytes.nUnreadable = 2;
    if (x != 0x00) {
        return (0, error.As(errMissingFF00)!);
    }
    return (0xff, error.As(null!)!);
}

// readFull reads exactly len(p) bytes into p. It does not care about byte
// stuffing.
private static error readFull(this ptr<decoder> _addr_d, slice<byte> p) {
    ref decoder d = ref _addr_d.val;
 
    // Unread the overshot bytes, if any.
    if (d.bytes.nUnreadable != 0) {
        if (d.bits.n >= 8) {
            d.unreadByteStuffedByte();
        }
        d.bytes.nUnreadable = 0;
    }
    while (true) {
        var n = copy(p, d.bytes.buf[(int)d.bytes.i..(int)d.bytes.j]);
        p = p[(int)n..];
        d.bytes.i += n;
        if (len(p) == 0) {
            break;
        }
        {
            var err = d.fill();

            if (err != null) {
                if (err == io.EOF) {
                    err = io.ErrUnexpectedEOF;
                }
                return error.As(err)!;
            }

        }
    }
    return error.As(null!)!;
}

// ignore ignores the next n bytes.
private static error ignore(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;
 
    // Unread the overshot bytes, if any.
    if (d.bytes.nUnreadable != 0) {
        if (d.bits.n >= 8) {
            d.unreadByteStuffedByte();
        }
        d.bytes.nUnreadable = 0;
    }
    while (true) {
        var m = d.bytes.j - d.bytes.i;
        if (m > n) {
            m = n;
        }
        d.bytes.i += m;
        n -= m;
        if (n == 0) {
            break;
        }
        {
            var err = d.fill();

            if (err != null) {
                if (err == io.EOF) {
                    err = io.ErrUnexpectedEOF;
                }
                return error.As(err)!;
            }

        }
    }
    return error.As(null!)!;
}

// Specified in section B.2.2.
private static error processSOF(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;

    if (d.nComp != 0) {
        return error.As(FormatError("multiple SOF markers"))!;
    }
    switch (n) {
        case 6 + 3 * 1: // Grayscale image.
            d.nComp = 1;
            break;
        case 6 + 3 * 3: // YCbCr or RGB image.
            d.nComp = 3;
            break;
        case 6 + 3 * 4: // YCbCrK or CMYK image.
            d.nComp = 4;
            break;
        default: 
            return error.As(UnsupportedError("number of components"))!;
            break;
    }
    {
        var err = d.readFull(d.tmp[..(int)n]);

        if (err != null) {
            return error.As(err)!;
        }
    } 
    // We only support 8-bit precision.
    if (d.tmp[0] != 8) {
        return error.As(UnsupportedError("precision"))!;
    }
    d.height = int(d.tmp[1]) << 8 + int(d.tmp[2]);
    d.width = int(d.tmp[3]) << 8 + int(d.tmp[4]);
    if (int(d.tmp[5]) != d.nComp) {
        return error.As(FormatError("SOF has wrong length"))!;
    }
    for (nint i = 0; i < d.nComp; i++) {
        d.comp[i].c = d.tmp[6 + 3 * i]; 
        // Section B.2.2 states that "the value of C_i shall be different from
        // the values of C_1 through C_(i-1)".
        for (nint j = 0; j < i; j++) {
            if (d.comp[i].c == d.comp[j].c) {
                return error.As(FormatError("repeated component identifier"))!;
            }
        }

        d.comp[i].tq = d.tmp[8 + 3 * i];
        if (d.comp[i].tq > maxTq) {
            return error.As(FormatError("bad Tq value"))!;
        }
        var hv = d.tmp[7 + 3 * i];
        var h = int(hv >> 4);
        var v = int(hv & 0x0f);
        if (h < 1 || 4 < h || v < 1 || 4 < v) {
            return error.As(FormatError("luma/chroma subsampling ratio"))!;
        }
        if (h == 3 || v == 3) {
            return error.As(errUnsupportedSubsamplingRatio)!;
        }
        switch (d.nComp) {
            case 1: 
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
                (h, v) = (1, 1);
                break;
            case 3: 
                // For YCbCr images, we only support 4:4:4, 4:4:0, 4:2:2, 4:2:0,
                // 4:1:1 or 4:1:0 chroma subsampling ratios. This implies that the
                // (h, v) values for the Y component are either (1, 1), (1, 2),
                // (2, 1), (2, 2), (4, 1) or (4, 2), and the Y component's values
                // must be a multiple of the Cb and Cr component's values. We also
                // assume that the two chroma components have the same subsampling
                // ratio.
                switch (i) {
                    case 0: // Y.
                        // We have already verified, above, that h and v are both
                        // either 1, 2 or 4, so invalid (h, v) combinations are those
                        // with v == 4.
                        if (v == 4) {
                            return error.As(errUnsupportedSubsamplingRatio)!;
                        }
                        break;
                    case 1: // Cb.
                        if (d.comp[0].h % h != 0 || d.comp[0].v % v != 0) {
                            return error.As(errUnsupportedSubsamplingRatio)!;
                        }
                        break;
                    case 2: // Cr.
                        if (d.comp[1].h != h || d.comp[1].v != v) {
                            return error.As(errUnsupportedSubsamplingRatio)!;
                        }
                        break;
                }
                break;
            case 4: 
                // For 4-component images (either CMYK or YCbCrK), we only support two
                // hv vectors: [0x11 0x11 0x11 0x11] and [0x22 0x11 0x11 0x22].
                // Theoretically, 4-component JPEG images could mix and match hv values
                // but in practice, those two combinations are the only ones in use,
                // and it simplifies the applyBlack code below if we can assume that:
                //    - for CMYK, the C and K channels have full samples, and if the M
                //      and Y channels subsample, they subsample both horizontally and
                //      vertically.
                //    - for YCbCrK, the Y and K channels have full samples.
                switch (i) {
                    case 0: 
                        if (hv != 0x11 && hv != 0x22) {
                            return error.As(errUnsupportedSubsamplingRatio)!;
                        }
                        break;
                    case 1: 

                    case 2: 
                        if (hv != 0x11) {
                            return error.As(errUnsupportedSubsamplingRatio)!;
                        }
                        break;
                    case 3: 
                        if (d.comp[0].h != h || d.comp[0].v != v) {
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
private static error processDQT(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;

loop:
    while (n > 0) {
        n--;
        var (x, err) = d.readByte();
        if (err != null) {
            return error.As(err)!;
        }
        var tq = x & 0x0f;
        if (tq > maxTq) {
            return error.As(FormatError("bad Tq value"))!;
        }
        switch (x >> 4) {
            case 0: 
                if (n < blockSize) {
                    _breakloop = true;
                    break;
                }
                n -= blockSize;
                {
                    var err__prev1 = err;

                    var err = d.readFull(d.tmp[..(int)blockSize]);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }
                {
                    var i__prev2 = i;

                    foreach (var (__i) in d.quant[tq]) {
                        i = __i;
                        d.quant[tq][i] = int32(d.tmp[i]);
                    }

                    i = i__prev2;
                }
                break;
            case 1: 
                if (n < 2 * blockSize) {
                    _breakloop = true;
                    break;
                }
                n -= 2 * blockSize;
                {
                    var err__prev1 = err;

                    err = d.readFull(d.tmp[..(int)2 * blockSize]);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }
                {
                    var i__prev2 = i;

                    foreach (var (__i) in d.quant[tq]) {
                        i = __i;
                        d.quant[tq][i] = int32(d.tmp[2 * i]) << 8 | int32(d.tmp[2 * i + 1]);
                    }

                    i = i__prev2;
                }
                break;
            default: 
                return error.As(FormatError("bad Pq value"))!;
                break;
        }
    }
    if (n != 0) {
        return error.As(FormatError("DQT has wrong length"))!;
    }
    return error.As(null!)!;
}

// Specified in section B.2.4.4.
private static error processDRI(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;

    if (n != 2) {
        return error.As(FormatError("DRI has wrong length"))!;
    }
    {
        var err = d.readFull(d.tmp[..(int)2]);

        if (err != null) {
            return error.As(err)!;
        }
    }
    d.ri = int(d.tmp[0]) << 8 + int(d.tmp[1]);
    return error.As(null!)!;
}

private static error processApp0Marker(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;

    if (n < 5) {
        return error.As(d.ignore(n))!;
    }
    {
        var err = d.readFull(d.tmp[..(int)5]);

        if (err != null) {
            return error.As(err)!;
        }
    }
    n -= 5;

    d.jfif = d.tmp[0] == 'J' && d.tmp[1] == 'F' && d.tmp[2] == 'I' && d.tmp[3] == 'F' && d.tmp[4] == '\x00';

    if (n > 0) {
        return error.As(d.ignore(n))!;
    }
    return error.As(null!)!;
}

private static error processApp14Marker(this ptr<decoder> _addr_d, nint n) {
    ref decoder d = ref _addr_d.val;

    if (n < 12) {
        return error.As(d.ignore(n))!;
    }
    {
        var err = d.readFull(d.tmp[..(int)12]);

        if (err != null) {
            return error.As(err)!;
        }
    }
    n -= 12;

    if (d.tmp[0] == 'A' && d.tmp[1] == 'd' && d.tmp[2] == 'o' && d.tmp[3] == 'b' && d.tmp[4] == 'e') {
        d.adobeTransformValid = true;
        d.adobeTransform = d.tmp[11];
    }
    if (n > 0) {
        return error.As(d.ignore(n))!;
    }
    return error.As(null!)!;
}

// decode reads a JPEG image from r and returns it as an image.Image.
private static (image.Image, error) decode(this ptr<decoder> _addr_d, io.Reader r, bool configOnly) {
    image.Image _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    d.r = r; 

    // Check for the Start Of Image marker.
    {
        var err__prev1 = err;

        var err = d.readFull(d.tmp[..(int)2]);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }
    if (d.tmp[0] != 0xff || d.tmp[1] != soiMarker) {
        return (null, error.As(FormatError("missing SOI marker"))!);
    }
    while (true) {
        err = d.readFull(d.tmp[..(int)2]);
        if (err != null) {
            return (null, error.As(err)!);
        }
        while (d.tmp[0] != 0xff) { 
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
            d.tmp[0] = d.tmp[1];
            d.tmp[1], err = d.readByte();
            if (err != null) {
                return (null, error.As(err)!);
            }
        }
        var marker = d.tmp[1];
        if (marker == 0) { 
            // Treat "\xff\x00" as extraneous data.
            continue;
        }
        while (marker == 0xff) { 
            // Section B.1.1.2 says, "Any marker may optionally be preceded by any
            // number of fill bytes, which are bytes assigned code X'FF'".
            marker, err = d.readByte();
            if (err != null) {
                return (null, error.As(err)!);
            }
        }
        if (marker == eoiMarker) { // End Of Image.
            break;
        }
        if (rst0Marker <= marker && marker <= rst7Marker) { 
            // Figures B.2 and B.16 of the specification suggest that restart markers should
            // only occur between Entropy Coded Segments and not after the final ECS.
            // However, some encoders may generate incorrect JPEGs with a final restart
            // marker. That restart marker will be seen here instead of inside the processSOS
            // method, and is ignored as a harmless error. Restart markers have no extra data,
            // so we check for this before we read the 16-bit length of the segment.
            continue;
        }
        err = d.readFull(d.tmp[..(int)2]);

        if (err != null) {
            return (null, error.As(err)!);
        }
        var n = int(d.tmp[0]) << 8 + int(d.tmp[1]) - 2;
        if (n < 0) {
            return (null, error.As(FormatError("short segment length"))!);
        }

        if (marker == sof0Marker || marker == sof1Marker || marker == sof2Marker) 
            d.baseline = marker == sof0Marker;
            d.progressive = marker == sof2Marker;
            err = d.processSOF(n);
            if (configOnly && d.jfif) {
                return (null, error.As(err)!);
            }
        else if (marker == dhtMarker) 
            if (configOnly) {
                err = d.ignore(n);
            }
            else
 {
                err = d.processDHT(n);
            }
        else if (marker == dqtMarker) 
            if (configOnly) {
                err = d.ignore(n);
            }
            else
 {
                err = d.processDQT(n);
            }
        else if (marker == sosMarker) 
            if (configOnly) {
                return (null, error.As(null!)!);
            }
            err = d.processSOS(n);
        else if (marker == driMarker) 
            if (configOnly) {
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
            if (app0Marker <= marker && marker <= app15Marker || marker == comMarker) {
                err = d.ignore(n);
            }
            else if (marker < 0xc0) { // See Table B.1 "Marker code assignments".
                err = FormatError("unknown marker");
            }
            else
 {
                err = UnsupportedError("unknown marker");
            }
                if (err != null) {
            return (null, error.As(err)!);
        }
    }

    if (d.progressive) {
        {
            var err__prev2 = err;

            err = d.reconstructProgressiveImage();

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev2;

        }
    }
    if (d.img1 != null) {
        return (d.img1, error.As(null!)!);
    }
    if (d.img3 != null) {
        if (d.blackPix != null) {
            return d.applyBlack();
        }
        else if (d.isRGB()) {
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
private static (image.Image, error) applyBlack(this ptr<decoder> _addr_d) {
    image.Image _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    if (!d.adobeTransformValid) {
        return (null, error.As(UnsupportedError("unknown color model: 4-component JPEG doesn't have Adobe APP14 metadata"))!);
    }
    if (d.adobeTransform != adobeTransformUnknown) { 
        // Convert the YCbCr part of the YCbCrK to RGB, invert the RGB to get
        // CMY, and patch in the original K. The RGB to CMY inversion cancels
        // out the 'Adobe inversion' described in the applyBlack doc comment
        // above, so in practice, only the fourth channel (black) is inverted.
        var bounds = d.img3.Bounds();
        var img = image.NewRGBA(bounds);
        imageutil.DrawYCbCr(img, bounds, d.img3, bounds.Min);
        {
            nint iBase__prev1 = iBase;
            var y__prev1 = y;

            nint iBase = 0;
            var y = bounds.Min.Y;

            while (y < bounds.Max.Y) {
                {
                    var i__prev2 = i;
                    var x__prev2 = x;

                    var i = iBase + 3;
                    var x = bounds.Min.X;

                    while (x < bounds.Max.X) {
                        img.Pix[i] = 255 - d.blackPix[(y - bounds.Min.Y) * d.blackStride + (x - bounds.Min.X)];
                        (i, x) = (i + 4, x + 1);
                    }


                    i = i__prev2;
                    x = x__prev2;
                }
                (iBase, y) = (iBase + img.Stride, y + 1);
            }


            iBase = iBase__prev1;
            y = y__prev1;
        }
        return (addr(new image.CMYK(Pix:img.Pix,Stride:img.Stride,Rect:img.Rect,)), error.As(null!)!);
    }
    bounds = d.img3.Bounds();
    img = image.NewCMYK(bounds);

    foreach (var (t, translation) in translations) {
        var subsample = d.comp[t].h != d.comp[0].h || d.comp[t].v != d.comp[0].v;
        {
            nint iBase__prev2 = iBase;
            var y__prev2 = y;

            iBase = 0;
            y = bounds.Min.Y;

            while (y < bounds.Max.Y) {
                var sy = y - bounds.Min.Y;
                if (subsample) {
                    sy /= 2;
                (iBase, y) = (iBase + img.Stride, y + 1);
                }
                {
                    var i__prev3 = i;
                    var x__prev3 = x;

                    i = iBase + t;
                    x = bounds.Min.X;

                    while (x < bounds.Max.X) {
                        var sx = x - bounds.Min.X;
                        if (subsample) {
                            sx /= 2;
                        (i, x) = (i + 4, x + 1);
                        }
                        img.Pix[i] = 255 - translation.src[sy * translation.stride + sx];
                    }


                    i = i__prev3;
                    x = x__prev3;
                }
            }


            iBase = iBase__prev2;
            y = y__prev2;
        }
    }    return (img, error.As(null!)!);
}

private static bool isRGB(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;

    if (d.jfif) {
        return false;
    }
    if (d.adobeTransformValid && d.adobeTransform == adobeTransformUnknown) { 
        // https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
        // says that 0 means Unknown (and in practice RGB) and 1 means YCbCr.
        return true;
    }
    return d.comp[0].c == 'R' && d.comp[1].c == 'G' && d.comp[2].c == 'B';
}

private static (image.Image, error) convertToRGB(this ptr<decoder> _addr_d) {
    image.Image _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    var cScale = d.comp[0].h / d.comp[1].h;
    var bounds = d.img3.Bounds();
    var img = image.NewRGBA(bounds);
    for (var y = bounds.Min.Y; y < bounds.Max.Y; y++) {
        var po = img.PixOffset(bounds.Min.X, y);
        var yo = d.img3.YOffset(bounds.Min.X, y);
        var co = d.img3.COffset(bounds.Min.X, y);
        for (nint i = 0;
        var iMax = bounds.Max.X - bounds.Min.X; i < iMax; i++) {
            img.Pix[po + 4 * i + 0] = d.img3.Y[yo + i];
            img.Pix[po + 4 * i + 1] = d.img3.Cb[co + i / cScale];
            img.Pix[po + 4 * i + 2] = d.img3.Cr[co + i / cScale];
            img.Pix[po + 4 * i + 3] = 255;
        }
    }
    return (img, error.As(null!)!);
}

// Decode reads a JPEG image from r and returns it as an image.Image.
public static (image.Image, error) Decode(io.Reader r) {
    image.Image _p0 = default;
    error _p0 = default!;

    decoder d = default;
    return d.decode(r, false);
}

// DecodeConfig returns the color model and dimensions of a JPEG image without
// decoding the entire image.
public static (image.Config, error) DecodeConfig(io.Reader r) {
    image.Config _p0 = default;
    error _p0 = default!;

    decoder d = default;
    {
        var (_, err) = d.decode(r, true);

        if (err != null) {
            return (new image.Config(), error.As(err)!);
        }
    }
    switch (d.nComp) {
        case 1: 
            return (new image.Config(ColorModel:color.GrayModel,Width:d.width,Height:d.height,), error.As(null!)!);
            break;
        case 3: 
            var cm = color.YCbCrModel;
            if (d.isRGB()) {
                cm = color.RGBAModel;
            }
            return (new image.Config(ColorModel:cm,Width:d.width,Height:d.height,), error.As(null!)!);
            break;
        case 4: 
            return (new image.Config(ColorModel:color.CMYKModel,Width:d.width,Height:d.height,), error.As(null!)!);
            break;
    }
    return (new image.Config(), error.As(FormatError("missing SOF marker"))!);
}

private static void init() {
    image.RegisterFormat("jpeg", "\xff\xd8", Decode, DecodeConfig);
}

} // end jpeg_package
