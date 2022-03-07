// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package png implements a PNG image decoder and encoder.
//
// The PNG specification is at https://www.w3.org/TR/PNG/.
// package png -- go2cs converted at 2022 March 06 23:36:19 UTC
// import "image/png" ==> using png = go.image.png_package
// Original source: C:\Program Files\Go\src\image\png\reader.go
using zlib = go.compress.zlib_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using hash = go.hash_package;
using crc32 = go.hash.crc32_package;
using image = go.image_package;
using color = go.image.color_package;
using io = go.io_package;

namespace go.image;

public static partial class png_package {

    // Color type, as per the PNG spec.
private static readonly nint ctGrayscale = 0;
private static readonly nint ctTrueColor = 2;
private static readonly nint ctPaletted = 3;
private static readonly nint ctGrayscaleAlpha = 4;
private static readonly nint ctTrueColorAlpha = 6;


// A cb is a combination of color type and bit depth.
private static readonly var cbInvalid = iota;
private static readonly var cbG1 = 0;
private static readonly var cbG2 = 1;
private static readonly var cbG4 = 2;
private static readonly var cbG8 = 3;
private static readonly var cbGA8 = 4;
private static readonly var cbTC8 = 5;
private static readonly var cbP1 = 6;
private static readonly var cbP2 = 7;
private static readonly var cbP4 = 8;
private static readonly var cbP8 = 9;
private static readonly var cbTCA8 = 10;
private static readonly var cbG16 = 11;
private static readonly var cbGA16 = 12;
private static readonly var cbTC16 = 13;
private static readonly var cbTCA16 = 14;


private static bool cbPaletted(nint cb) {
    return cbP1 <= cb && cb <= cbP8;
}

// Filter type, as per the PNG spec.
private static readonly nint ftNone = 0;
private static readonly nint ftSub = 1;
private static readonly nint ftUp = 2;
private static readonly nint ftAverage = 3;
private static readonly nint ftPaeth = 4;
private static readonly nint nFilter = 5;


// Interlace type.
private static readonly nint itNone = 0;
private static readonly nint itAdam7 = 1;


// interlaceScan defines the placement and size of a pass for Adam7 interlacing.
private partial struct interlaceScan {
    public nint xFactor;
    public nint yFactor;
    public nint xOffset;
    public nint yOffset;
}

// interlacing defines Adam7 interlacing, with 7 passes of reduced images.
// See https://www.w3.org/TR/PNG/#8Interlace
private static interlaceScan interlacing = new slice<interlaceScan>(new interlaceScan[] { {8,8,0,0}, {8,8,4,0}, {4,8,0,4}, {4,4,2,0}, {2,4,0,2}, {2,2,1,0}, {1,2,0,1} });

// Decoding stage.
// The PNG specification says that the IHDR, PLTE (if present), tRNS (if
// present), IDAT and IEND chunks must appear in that order. There may be
// multiple IDAT chunks, and IDAT chunks must be sequential (i.e. they may not
// have any other chunks between them).
// https://www.w3.org/TR/PNG/#5ChunkOrdering
private static readonly var dsStart = iota;
private static readonly var dsSeenIHDR = 0;
private static readonly var dsSeenPLTE = 1;
private static readonly var dsSeentRNS = 2;
private static readonly var dsSeenIDAT = 3;
private static readonly var dsSeenIEND = 4;


private static readonly @string pngHeader = "\x89PNG\r\n\x1a\n";



private partial struct decoder {
    public io.Reader r;
    public image.Image img;
    public hash.Hash32 crc;
    public nint width;
    public nint height;
    public nint depth;
    public color.Palette palette;
    public nint cb;
    public nint stage;
    public uint idatLength;
    public array<byte> tmp;
    public nint interlace; // useTransparent and transparent are used for grayscale and truecolor
// transparency, as opposed to palette transparency.
    public bool useTransparent;
    public array<byte> transparent;
}

// A FormatError reports that the input is not a valid PNG.
public partial struct FormatError { // : @string
}

public static @string Error(this FormatError e) {
    return "png: invalid format: " + string(e);
}

private static var chunkOrderError = FormatError("chunk out of order");

// An UnsupportedError reports that the input uses a valid but unimplemented PNG feature.
public partial struct UnsupportedError { // : @string
}

public static @string Error(this UnsupportedError e) {
    return "png: unsupported feature: " + string(e);
}

private static nint min(nint a, nint b) {
    if (a < b) {
        return a;
    }
    return b;

}

private static error parseIHDR(this ptr<decoder> _addr_d, uint length) {
    ref decoder d = ref _addr_d.val;

    if (length != 13) {
        return error.As(FormatError("bad IHDR length"))!;
    }
    {
        var (_, err) = io.ReadFull(d.r, d.tmp[..(int)13]);

        if (err != null) {
            return error.As(err)!;
        }
    }

    d.crc.Write(d.tmp[..(int)13]);
    if (d.tmp[10] != 0) {
        return error.As(UnsupportedError("compression method"))!;
    }
    if (d.tmp[11] != 0) {
        return error.As(UnsupportedError("filter method"))!;
    }
    if (d.tmp[12] != itNone && d.tmp[12] != itAdam7) {
        return error.As(FormatError("invalid interlace method"))!;
    }
    d.interlace = int(d.tmp[12]);

    var w = int32(binary.BigEndian.Uint32(d.tmp[(int)0..(int)4]));
    var h = int32(binary.BigEndian.Uint32(d.tmp[(int)4..(int)8]));
    if (w <= 0 || h <= 0) {
        return error.As(FormatError("non-positive dimension"))!;
    }
    var nPixels64 = int64(w) * int64(h);
    var nPixels = int(nPixels64);
    if (nPixels64 != int64(nPixels)) {
        return error.As(UnsupportedError("dimension overflow"))!;
    }
    if (nPixels != (nPixels * 8) / 8) {
        return error.As(UnsupportedError("dimension overflow"))!;
    }
    d.cb = cbInvalid;
    d.depth = int(d.tmp[8]);
    switch (d.depth) {
        case 1: 

            if (d.tmp[9] == ctGrayscale) 
                d.cb = cbG1;
            else if (d.tmp[9] == ctPaletted) 
                d.cb = cbP1;

            break;
        case 2: 

            if (d.tmp[9] == ctGrayscale) 
                d.cb = cbG2;
            else if (d.tmp[9] == ctPaletted) 
                d.cb = cbP2;

            break;
        case 4: 

            if (d.tmp[9] == ctGrayscale) 
                d.cb = cbG4;
            else if (d.tmp[9] == ctPaletted) 
                d.cb = cbP4;

            break;
        case 8: 

            if (d.tmp[9] == ctGrayscale) 
                d.cb = cbG8;
            else if (d.tmp[9] == ctTrueColor) 
                d.cb = cbTC8;
            else if (d.tmp[9] == ctPaletted) 
                d.cb = cbP8;
            else if (d.tmp[9] == ctGrayscaleAlpha) 
                d.cb = cbGA8;
            else if (d.tmp[9] == ctTrueColorAlpha) 
                d.cb = cbTCA8;

            break;
        case 16: 

            if (d.tmp[9] == ctGrayscale) 
                d.cb = cbG16;
            else if (d.tmp[9] == ctTrueColor) 
                d.cb = cbTC16;
            else if (d.tmp[9] == ctGrayscaleAlpha) 
                d.cb = cbGA16;
            else if (d.tmp[9] == ctTrueColorAlpha) 
                d.cb = cbTCA16;

            break;
    }
    if (d.cb == cbInvalid) {
        return error.As(UnsupportedError(fmt.Sprintf("bit depth %d, color type %d", d.tmp[8], d.tmp[9])))!;
    }
    (d.width, d.height) = (int(w), int(h));    return error.As(d.verifyChecksum())!;

}

private static error parsePLTE(this ptr<decoder> _addr_d, uint length) {
    ref decoder d = ref _addr_d.val;

    var np = int(length / 3); // The number of palette entries.
    if (length % 3 != 0 || np <= 0 || np > 256 || np > 1 << (int)(uint(d.depth))) {
        return error.As(FormatError("bad PLTE length"))!;
    }
    var (n, err) = io.ReadFull(d.r, d.tmp[..(int)3 * np]);
    if (err != null) {
        return error.As(err)!;
    }
    d.crc.Write(d.tmp[..(int)n]);

    if (d.cb == cbP1 || d.cb == cbP2 || d.cb == cbP4 || d.cb == cbP8) 
        d.palette = make(color.Palette, 256);
        {
            nint i__prev1 = i;

            for (nint i = 0; i < np; i++) {
                d.palette[i] = new color.RGBA(d.tmp[3*i+0],d.tmp[3*i+1],d.tmp[3*i+2],0xff);
            }


            i = i__prev1;
        }
        {
            nint i__prev1 = i;

            for (i = np; i < 256; i++) { 
                // Initialize the rest of the palette to opaque black. The spec (section
                // 11.2.3) says that "any out-of-range pixel value found in the image data
                // is an error", but some real-world PNG files have out-of-range pixel
                // values. We fall back to opaque black, the same as libpng 1.5.13;
                // ImageMagick 6.5.7 returns an error.
                d.palette[i] = new color.RGBA(0x00,0x00,0x00,0xff);

            }


            i = i__prev1;
        }
        d.palette = d.palette[..(int)np];
    else if (d.cb == cbTC8 || d.cb == cbTCA8 || d.cb == cbTC16 || d.cb == cbTCA16)     else 
        return error.As(FormatError("PLTE, color type mismatch"))!;
        return error.As(d.verifyChecksum())!;

}

private static error parsetRNS(this ptr<decoder> _addr_d, uint length) {
    ref decoder d = ref _addr_d.val;


    if (d.cb == cbG1 || d.cb == cbG2 || d.cb == cbG4 || d.cb == cbG8 || d.cb == cbG16) 
        if (length != 2) {
            return error.As(FormatError("bad tRNS length"))!;
        }
        var (n, err) = io.ReadFull(d.r, d.tmp[..(int)length]);
        if (err != null) {
            return error.As(err)!;
        }
        d.crc.Write(d.tmp[..(int)n]);

        copy(d.transparent[..], d.tmp[..(int)length]);

        if (d.cb == cbG1) 
            d.transparent[1] *= 0xff;
        else if (d.cb == cbG2) 
            d.transparent[1] *= 0x55;
        else if (d.cb == cbG4) 
            d.transparent[1] *= 0x11;
                d.useTransparent = true;
    else if (d.cb == cbTC8 || d.cb == cbTC16) 
        if (length != 6) {
            return error.As(FormatError("bad tRNS length"))!;
        }
        (n, err) = io.ReadFull(d.r, d.tmp[..(int)length]);
        if (err != null) {
            return error.As(err)!;
        }
        d.crc.Write(d.tmp[..(int)n]);

        copy(d.transparent[..], d.tmp[..(int)length]);
        d.useTransparent = true;
    else if (d.cb == cbP1 || d.cb == cbP2 || d.cb == cbP4 || d.cb == cbP8) 
        if (length > 256) {
            return error.As(FormatError("bad tRNS length"))!;
        }
        (n, err) = io.ReadFull(d.r, d.tmp[..(int)length]);
        if (err != null) {
            return error.As(err)!;
        }
        d.crc.Write(d.tmp[..(int)n]);

        if (len(d.palette) < n) {
            d.palette = d.palette[..(int)n];
        }
        for (nint i = 0; i < n; i++) {
            color.RGBA rgba = d.palette[i]._<color.RGBA>();
            d.palette[i] = new color.NRGBA(rgba.R,rgba.G,rgba.B,d.tmp[i]);
        }
    else 
        return error.As(FormatError("tRNS, color type mismatch"))!;
        return error.As(d.verifyChecksum())!;

}

// Read presents one or more IDAT chunks as one continuous stream (minus the
// intermediate chunk headers and footers). If the PNG data looked like:
//   ... len0 IDAT xxx crc0 len1 IDAT yy crc1 len2 IEND crc2
// then this reader presents xxxyy. For well-formed PNG data, the decoder state
// immediately before the first Read call is that d.r is positioned between the
// first IDAT and xxx, and the decoder state immediately after the last Read
// call is that d.r is positioned between yy and crc1.
private static (nint, error) Read(this ptr<decoder> _addr_d, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    while (d.idatLength == 0) { 
        // We have exhausted an IDAT chunk. Verify the checksum of that chunk.
        {
            var err = d.verifyChecksum();

            if (err != null) {
                return (0, error.As(err)!);
            } 
            // Read the length and chunk type of the next chunk, and check that
            // it is an IDAT chunk.

        } 
        // Read the length and chunk type of the next chunk, and check that
        // it is an IDAT chunk.
        {
            var (_, err) = io.ReadFull(d.r, d.tmp[..(int)8]);

            if (err != null) {
                return (0, error.As(err)!);
            }

        }

        d.idatLength = binary.BigEndian.Uint32(d.tmp[..(int)4]);
        if (string(d.tmp[(int)4..(int)8]) != "IDAT") {
            return (0, error.As(FormatError("not enough pixel data"))!);
        }
        d.crc.Reset();
        d.crc.Write(d.tmp[(int)4..(int)8]);

    }
    if (int(d.idatLength) < 0) {
        return (0, error.As(UnsupportedError("IDAT chunk length overflow"))!);
    }
    var (n, err) = d.r.Read(p[..(int)min(len(p), int(d.idatLength))]);
    d.crc.Write(p[..(int)n]);
    d.idatLength -= uint32(n);
    return (n, error.As(err)!);

}

// decode decodes the IDAT data into an image.
private static (image.Image, error) decode(this ptr<decoder> _addr_d) => func((defer, _, _) => {
    image.Image _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    var (r, err) = zlib.NewReader(d);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(r.Close());
    image.Image img = default;
    if (d.interlace == itNone) {
        img, err = d.readImagePass(r, 0, false);
        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    else if (d.interlace == itAdam7) { 
        // Allocate a blank image of the full size.
        img, err = d.readImagePass(null, 0, true);
        if (err != null) {
            return (null, error.As(err)!);
        }
        for (nint pass = 0; pass < 7; pass++) {
            var (imagePass, err) = d.readImagePass(r, pass, false);
            if (err != null) {
                return (null, error.As(err)!);
            }
            if (imagePass != null) {
                d.mergePassInto(img, imagePass, pass);
            }
        }

    }
    nint n = 0;
    for (nint i = 0; n == 0 && err == null; i++) {
        if (i == 100) {
            return (null, error.As(io.ErrNoProgress)!);
        }
        n, err = r.Read(d.tmp[..(int)1]);

    }
    if (err != null && err != io.EOF) {
        return (null, error.As(FormatError(err.Error()))!);
    }
    if (n != 0 || d.idatLength != 0) {
        return (null, error.As(FormatError("too much pixel data"))!);
    }
    return (img, error.As(null!)!);

});

// readImagePass reads a single image pass, sized according to the pass number.
private static (image.Image, error) readImagePass(this ptr<decoder> _addr_d, io.Reader r, nint pass, bool allocateOnly) {
    image.Image _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    nint bitsPerPixel = 0;
    nint pixOffset = 0;
    ptr<image.Gray> gray;    ptr<image.RGBA> rgba;    ptr<image.Paletted> paletted;    ptr<image.NRGBA> nrgba;    ptr<image.Gray16> gray16;    ptr<image.RGBA64> rgba64;    ptr<image.NRGBA64> nrgba64;    image.Image img = default;
    var width = d.width;
    var height = d.height;
    if (d.interlace == itAdam7 && !allocateOnly) {
        var p = interlacing[pass]; 
        // Add the multiplication factor and subtract one, effectively rounding up.
        width = (width - p.xOffset + p.xFactor - 1) / p.xFactor;
        height = (height - p.yOffset + p.yFactor - 1) / p.yFactor; 
        // A PNG image can't have zero width or height, but for an interlaced
        // image, an individual pass might have zero width or height. If so, we
        // shouldn't even read a per-row filter type byte, so return early.
        if (width == 0 || height == 0) {
            return (null, error.As(null!)!);
        }
    }

    if (d.cb == cbG1 || d.cb == cbG2 || d.cb == cbG4 || d.cb == cbG8) 
        bitsPerPixel = d.depth;
        if (d.useTransparent) {
            nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
            img = nrgba;
        }
        else
 {
            gray = image.NewGray(image.Rect(0, 0, width, height));
            img = gray;
        }
    else if (d.cb == cbGA8) 
        bitsPerPixel = 16;
        nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
        img = nrgba;
    else if (d.cb == cbTC8) 
        bitsPerPixel = 24;
        if (d.useTransparent) {
            nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
            img = nrgba;
        }
        else
 {
            rgba = image.NewRGBA(image.Rect(0, 0, width, height));
            img = rgba;
        }
    else if (d.cb == cbP1 || d.cb == cbP2 || d.cb == cbP4 || d.cb == cbP8) 
        bitsPerPixel = d.depth;
        paletted = image.NewPaletted(image.Rect(0, 0, width, height), d.palette);
        img = paletted;
    else if (d.cb == cbTCA8) 
        bitsPerPixel = 32;
        nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
        img = nrgba;
    else if (d.cb == cbG16) 
        bitsPerPixel = 16;
        if (d.useTransparent) {
            nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
            img = nrgba64;
        }
        else
 {
            gray16 = image.NewGray16(image.Rect(0, 0, width, height));
            img = gray16;
        }
    else if (d.cb == cbGA16) 
        bitsPerPixel = 32;
        nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
        img = nrgba64;
    else if (d.cb == cbTC16) 
        bitsPerPixel = 48;
        if (d.useTransparent) {
            nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
            img = nrgba64;
        }
        else
 {
            rgba64 = image.NewRGBA64(image.Rect(0, 0, width, height));
            img = rgba64;
        }
    else if (d.cb == cbTCA16) 
        bitsPerPixel = 64;
        nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
        img = nrgba64;
        if (allocateOnly) {
        return (img, error.As(null!)!);
    }
    var bytesPerPixel = (bitsPerPixel + 7) / 8; 

    // The +1 is for the per-row filter type, which is at cr[0].
    nint rowSize = 1 + (int64(bitsPerPixel) * int64(width) + 7) / 8;
    if (rowSize != int64(int(rowSize))) {
        return (null, error.As(UnsupportedError("dimension overflow"))!);
    }
    var cr = make_slice<byte>(rowSize);
    var pr = make_slice<byte>(rowSize);

    for (nint y = 0; y < height; y++) { 
        // Read the decompressed bytes.
        var (_, err) = io.ReadFull(r, cr);
        if (err != null) {
            if (err == io.EOF || err == io.ErrUnexpectedEOF) {
                return (null, error.As(FormatError("not enough pixel data"))!);
            }
            return (null, error.As(err)!);
        }
        var cdat = cr[(int)1..];
        var pdat = pr[(int)1..];

        if (cr[0] == ftNone)         else if (cr[0] == ftSub) 
            {
                var i__prev2 = i;

                for (var i = bytesPerPixel; i < len(cdat); i++) {
                    cdat[i] += cdat[i - bytesPerPixel];
                }


                i = i__prev2;
            }
        else if (cr[0] == ftUp) 
            {
                var i__prev2 = i;
                var p__prev2 = p;

                foreach (var (__i, __p) in pdat) {
                    i = __i;
                    p = __p;
                    cdat[i] += p;
                }

                i = i__prev2;
                p = p__prev2;
            }
        else if (cr[0] == ftAverage) 
            // The first column has no column to the left of it, so it is a
            // special case. We know that the first column exists because we
            // check above that width != 0, and so len(cdat) != 0.
            {
                var i__prev2 = i;

                for (i = 0; i < bytesPerPixel; i++) {
                    cdat[i] += pdat[i] / 2;
                }


                i = i__prev2;
            }
            {
                var i__prev2 = i;

                for (i = bytesPerPixel; i < len(cdat); i++) {
                    cdat[i] += uint8((int(cdat[i - bytesPerPixel]) + int(pdat[i])) / 2);
                }


                i = i__prev2;
            }
        else if (cr[0] == ftPaeth) 
            filterPaeth(cdat, pdat, bytesPerPixel);
        else 
            return (null, error.As(FormatError("bad filter type"))!);
        // Convert from bytes to colors.

        if (d.cb == cbG1) 
            if (d.useTransparent) {
                var ty = d.transparent[1];
                {
                    nint x__prev2 = x;

                    nint x = 0;

                    while (x < width) {
                        var b = cdat[x / 8];
                        {
                            nint x2__prev3 = x2;

                            for (nint x2 = 0; x2 < 8 && x + x2 < width; x2++) {
                                var ycol = (b >> 7) * 0xff;
                                var acol = uint8(0xff);
                                if (ycol == ty) {
                                    acol = 0x00;
                                }
                                nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol,ycol,ycol,acol));
                                b<<=1;
                        x += 8;
                            }
            else


                            x2 = x2__prev3;
                        }

                    }


                    x = x__prev2;
                }

            } {
                {
                    nint x__prev2 = x;

                    x = 0;

                    while (x < width) {
                        b = cdat[x / 8];
                        {
                            nint x2__prev3 = x2;

                            for (x2 = 0; x2 < 8 && x + x2 < width; x2++) {
                                gray.SetGray(x + x2, y, new color.Gray((b>>7)*0xff));
                                b<<=1;
                            }


                            x2 = x2__prev3;
                        }
                        x += 8;
                    }


                    x = x__prev2;
                }

            }

        else if (d.cb == cbG2) 
            if (d.useTransparent) {
                ty = d.transparent[1];
                {
                    nint x__prev2 = x;

                    x = 0;

                    while (x < width) {
                        b = cdat[x / 4];
                        {
                            nint x2__prev3 = x2;

                            for (x2 = 0; x2 < 4 && x + x2 < width; x2++) {
                                ycol = (b >> 6) * 0x55;
                                acol = uint8(0xff);
                                if (ycol == ty) {
                                    acol = 0x00;
                                }
                                nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol,ycol,ycol,acol));
                                b<<=2;
                        x += 4;
                            }
            else


                            x2 = x2__prev3;
                        }

                    }


                    x = x__prev2;
                }

            } {
                {
                    nint x__prev2 = x;

                    x = 0;

                    while (x < width) {
                        b = cdat[x / 4];
                        {
                            nint x2__prev3 = x2;

                            for (x2 = 0; x2 < 4 && x + x2 < width; x2++) {
                                gray.SetGray(x + x2, y, new color.Gray((b>>6)*0x55));
                                b<<=2;
                            }


                            x2 = x2__prev3;
                        }
                        x += 4;
                    }


                    x = x__prev2;
                }

            }

        else if (d.cb == cbG4) 
            if (d.useTransparent) {
                ty = d.transparent[1];
                {
                    nint x__prev2 = x;

                    x = 0;

                    while (x < width) {
                        b = cdat[x / 2];
                        {
                            nint x2__prev3 = x2;

                            for (x2 = 0; x2 < 2 && x + x2 < width; x2++) {
                                ycol = (b >> 4) * 0x11;
                                acol = uint8(0xff);
                                if (ycol == ty) {
                                    acol = 0x00;
                                }
                                nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol,ycol,ycol,acol));
                                b<<=4;
                        x += 2;
                            }
            else


                            x2 = x2__prev3;
                        }

                    }


                    x = x__prev2;
                }

            } {
                {
                    nint x__prev2 = x;

                    x = 0;

                    while (x < width) {
                        b = cdat[x / 2];
                        {
                            nint x2__prev3 = x2;

                            for (x2 = 0; x2 < 2 && x + x2 < width; x2++) {
                                gray.SetGray(x + x2, y, new color.Gray((b>>4)*0x11));
                                b<<=4;
                            }


                            x2 = x2__prev3;
                        }
                        x += 2;
                    }


                    x = x__prev2;
                }

            }

        else if (d.cb == cbG8) 
            if (d.useTransparent) {
                ty = d.transparent[1];
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        ycol = cdat[x];
                        acol = uint8(0xff);
                        if (ycol == ty) {
                            acol = 0x00;
                        }
                        nrgba.SetNRGBA(x, y, new color.NRGBA(ycol,ycol,ycol,acol));
                    }
            else


                    x = x__prev2;
                }

            } {
                copy(gray.Pix[(int)pixOffset..], cdat);
                pixOffset += gray.Stride;
            }

        else if (d.cb == cbGA8) 
            {
                nint x__prev2 = x;

                for (x = 0; x < width; x++) {
                    ycol = cdat[2 * x + 0];
                    nrgba.SetNRGBA(x, y, new color.NRGBA(ycol,ycol,ycol,cdat[2*x+1]));
                }


                x = x__prev2;
            }
        else if (d.cb == cbTC8) 
            if (d.useTransparent) {
                var pix = nrgba.Pix;
                i = pixOffset;
                nint j = 0;
                var tr = d.transparent[1];
                var tg = d.transparent[3];
                var tb = d.transparent[5];
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        var r = cdat[j + 0];
                        var g = cdat[j + 1];
                        b = cdat[j + 2];
                        var a = uint8(0xff);
                        if (r == tr && g == tg && b == tb) {
                            a = 0x00;
                        }
                        pix[i + 0] = r;
                        pix[i + 1] = g;
                        pix[i + 2] = b;
                        pix[i + 3] = a;
                        i += 4;
                        j += 3;
                    }
            else


                    x = x__prev2;
                }
                pixOffset += nrgba.Stride;

            } {
                pix = rgba.Pix;
                i = pixOffset;
                j = 0;
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        pix[i + 0] = cdat[j + 0];
                        pix[i + 1] = cdat[j + 1];
                        pix[i + 2] = cdat[j + 2];
                        pix[i + 3] = 0xff;
                        i += 4;
                        j += 3;
                    }


                    x = x__prev2;
                }
                pixOffset += rgba.Stride;

            }

        else if (d.cb == cbP1) 
            {
                nint x__prev2 = x;

                x = 0;

                while (x < width) {
                    b = cdat[x / 8];
                    {
                        nint x2__prev3 = x2;

                        for (x2 = 0; x2 < 8 && x + x2 < width; x2++) {
                            var idx = b >> 7;
                            if (len(paletted.Palette) <= int(idx)) {
                                paletted.Palette = paletted.Palette[..(int)int(idx) + 1];
                            }
                            paletted.SetColorIndex(x + x2, y, idx);
                            b<<=1;
                    x += 8;
                        }


                        x2 = x2__prev3;
                    }

                }


                x = x__prev2;
            }
        else if (d.cb == cbP2) 
            {
                nint x__prev2 = x;

                x = 0;

                while (x < width) {
                    b = cdat[x / 4];
                    {
                        nint x2__prev3 = x2;

                        for (x2 = 0; x2 < 4 && x + x2 < width; x2++) {
                            idx = b >> 6;
                            if (len(paletted.Palette) <= int(idx)) {
                                paletted.Palette = paletted.Palette[..(int)int(idx) + 1];
                            }
                            paletted.SetColorIndex(x + x2, y, idx);
                            b<<=2;
                    x += 4;
                        }


                        x2 = x2__prev3;
                    }

                }


                x = x__prev2;
            }
        else if (d.cb == cbP4) 
            {
                nint x__prev2 = x;

                x = 0;

                while (x < width) {
                    b = cdat[x / 2];
                    {
                        nint x2__prev3 = x2;

                        for (x2 = 0; x2 < 2 && x + x2 < width; x2++) {
                            idx = b >> 4;
                            if (len(paletted.Palette) <= int(idx)) {
                                paletted.Palette = paletted.Palette[..(int)int(idx) + 1];
                            }
                            paletted.SetColorIndex(x + x2, y, idx);
                            b<<=4;
                    x += 2;
                        }


                        x2 = x2__prev3;
                    }

                }


                x = x__prev2;
            }
        else if (d.cb == cbP8) 
            if (len(paletted.Palette) != 256) {
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        if (len(paletted.Palette) <= int(cdat[x])) {
                            paletted.Palette = paletted.Palette[..(int)int(cdat[x]) + 1];
                        }
                    }


                    x = x__prev2;
                }

            }

            copy(paletted.Pix[(int)pixOffset..], cdat);
            pixOffset += paletted.Stride;
        else if (d.cb == cbTCA8) 
            copy(nrgba.Pix[(int)pixOffset..], cdat);
            pixOffset += nrgba.Stride;
        else if (d.cb == cbG16) 
            if (d.useTransparent) {
                ty = uint16(d.transparent[0]) << 8 | uint16(d.transparent[1]);
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        ycol = uint16(cdat[2 * x + 0]) << 8 | uint16(cdat[2 * x + 1]);
                        acol = uint16(0xffff);
                        if (ycol == ty) {
                            acol = 0x0000;
                        }
                        nrgba64.SetNRGBA64(x, y, new color.NRGBA64(ycol,ycol,ycol,acol));
                    }
            else


                    x = x__prev2;
                }

            } {
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        ycol = uint16(cdat[2 * x + 0]) << 8 | uint16(cdat[2 * x + 1]);
                        gray16.SetGray16(x, y, new color.Gray16(ycol));
                    }


                    x = x__prev2;
                }

            }

        else if (d.cb == cbGA16) 
            {
                nint x__prev2 = x;

                for (x = 0; x < width; x++) {
                    ycol = uint16(cdat[4 * x + 0]) << 8 | uint16(cdat[4 * x + 1]);
                    acol = uint16(cdat[4 * x + 2]) << 8 | uint16(cdat[4 * x + 3]);
                    nrgba64.SetNRGBA64(x, y, new color.NRGBA64(ycol,ycol,ycol,acol));
                }


                x = x__prev2;
            }
        else if (d.cb == cbTC16) 
            if (d.useTransparent) {
                tr = uint16(d.transparent[0]) << 8 | uint16(d.transparent[1]);
                tg = uint16(d.transparent[2]) << 8 | uint16(d.transparent[3]);
                tb = uint16(d.transparent[4]) << 8 | uint16(d.transparent[5]);
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        var rcol = uint16(cdat[6 * x + 0]) << 8 | uint16(cdat[6 * x + 1]);
                        var gcol = uint16(cdat[6 * x + 2]) << 8 | uint16(cdat[6 * x + 3]);
                        var bcol = uint16(cdat[6 * x + 4]) << 8 | uint16(cdat[6 * x + 5]);
                        acol = uint16(0xffff);
                        if (rcol == tr && gcol == tg && bcol == tb) {
                            acol = 0x0000;
                        }
                        nrgba64.SetNRGBA64(x, y, new color.NRGBA64(rcol,gcol,bcol,acol));
                    }
            else


                    x = x__prev2;
                }

            } {
                {
                    nint x__prev2 = x;

                    for (x = 0; x < width; x++) {
                        rcol = uint16(cdat[6 * x + 0]) << 8 | uint16(cdat[6 * x + 1]);
                        gcol = uint16(cdat[6 * x + 2]) << 8 | uint16(cdat[6 * x + 3]);
                        bcol = uint16(cdat[6 * x + 4]) << 8 | uint16(cdat[6 * x + 5]);
                        rgba64.SetRGBA64(x, y, new color.RGBA64(rcol,gcol,bcol,0xffff));
                    }


                    x = x__prev2;
                }

            }

        else if (d.cb == cbTCA16) 
            {
                nint x__prev2 = x;

                for (x = 0; x < width; x++) {
                    rcol = uint16(cdat[8 * x + 0]) << 8 | uint16(cdat[8 * x + 1]);
                    gcol = uint16(cdat[8 * x + 2]) << 8 | uint16(cdat[8 * x + 3]);
                    bcol = uint16(cdat[8 * x + 4]) << 8 | uint16(cdat[8 * x + 5]);
                    acol = uint16(cdat[8 * x + 6]) << 8 | uint16(cdat[8 * x + 7]);
                    nrgba64.SetNRGBA64(x, y, new color.NRGBA64(rcol,gcol,bcol,acol));
                }


                x = x__prev2;
            }
        // The current row for y is the previous row for y+1.
        (pr, cr) = (cr, pr);
    }

    return (img, error.As(null!)!);

}

// mergePassInto merges a single pass into a full sized image.
private static void mergePassInto(this ptr<decoder> _addr_d, image.Image dst, image.Image src, nint pass) {
    ref decoder d = ref _addr_d.val;

    var p = interlacing[pass];
    slice<byte> srcPix = default;    slice<byte> dstPix = default;    nint stride = default;    image.Rectangle rect = default;    nint bytesPerPixel = default;
    switch (dst.type()) {
        case ptr<image.Alpha> target:
            srcPix = src._<ptr<image.Alpha>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 1;
            break;
        case ptr<image.Alpha16> target:
            srcPix = src._<ptr<image.Alpha16>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 2;
            break;
        case ptr<image.Gray> target:
            srcPix = src._<ptr<image.Gray>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 1;
            break;
        case ptr<image.Gray16> target:
            srcPix = src._<ptr<image.Gray16>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 2;
            break;
        case ptr<image.NRGBA> target:
            srcPix = src._<ptr<image.NRGBA>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 4;
            break;
        case ptr<image.NRGBA64> target:
            srcPix = src._<ptr<image.NRGBA64>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 8;
            break;
        case ptr<image.Paletted> target:
            srcPix = src._<ptr<image.Paletted>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 1;
            break;
        case ptr<image.RGBA> target:
            srcPix = src._<ptr<image.RGBA>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 4;
            break;
        case ptr<image.RGBA64> target:
            srcPix = src._<ptr<image.RGBA64>>().Pix;
            (dstPix, stride, rect) = (target.Pix, target.Stride, target.Rect);            bytesPerPixel = 8;
            break;
    }
    nint s = 0;
    var bounds = src.Bounds();
    for (var y = bounds.Min.Y; y < bounds.Max.Y; y++) {
        var dBase = (y * p.yFactor + p.yOffset - rect.Min.Y) * stride + (p.xOffset - rect.Min.X) * bytesPerPixel;
        for (var x = bounds.Min.X; x < bounds.Max.X; x++) {
            var d = dBase + x * p.xFactor * bytesPerPixel;
            copy(dstPix[(int)d..], srcPix[(int)s..(int)s + bytesPerPixel]);
            s += bytesPerPixel;
        }
    }

}

private static error parseIDAT(this ptr<decoder> _addr_d, uint length) {
    error err = default!;
    ref decoder d = ref _addr_d.val;

    d.idatLength = length;
    d.img, err = d.decode();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(d.verifyChecksum())!;

}

private static error parseIEND(this ptr<decoder> _addr_d, uint length) {
    ref decoder d = ref _addr_d.val;

    if (length != 0) {
        return error.As(FormatError("bad IEND length"))!;
    }
    return error.As(d.verifyChecksum())!;

}

private static error parseChunk(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;
 
    // Read the length and chunk type.
    {
        var (_, err) = io.ReadFull(d.r, d.tmp[..(int)8]);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var length = binary.BigEndian.Uint32(d.tmp[..(int)4]);
    d.crc.Reset();
    d.crc.Write(d.tmp[(int)4..(int)8]); 

    // Read the chunk data.
    switch (string(d.tmp[(int)4..(int)8])) {
        case "IHDR": 
            if (d.stage != dsStart) {
                return error.As(chunkOrderError)!;
            }
            d.stage = dsSeenIHDR;
            return error.As(d.parseIHDR(length))!;

            break;
        case "PLTE": 
            if (d.stage != dsSeenIHDR) {
                return error.As(chunkOrderError)!;
            }
            d.stage = dsSeenPLTE;
            return error.As(d.parsePLTE(length))!;

            break;
        case "tRNS": 
            if (cbPaletted(d.cb)) {
                if (d.stage != dsSeenPLTE) {
                    return error.As(chunkOrderError)!;
                }
            }
            else if (d.stage != dsSeenIHDR) {
                return error.As(chunkOrderError)!;
            }
            d.stage = dsSeentRNS;
            return error.As(d.parsetRNS(length))!;

            break;
        case "IDAT": 
            if (d.stage < dsSeenIHDR || d.stage > dsSeenIDAT || (d.stage == dsSeenIHDR && cbPaletted(d.cb))) {
                return error.As(chunkOrderError)!;
            }
            else if (d.stage == dsSeenIDAT) { 
                // Ignore trailing zero-length or garbage IDAT chunks.
                //
                // This does not affect valid PNG images that contain multiple IDAT
                // chunks, since the first call to parseIDAT below will consume all
                // consecutive IDAT chunks required for decoding the image.
                break;

            }
            d.stage = dsSeenIDAT;
            return error.As(d.parseIDAT(length))!;

            break;
        case "IEND": 
            if (d.stage != dsSeenIDAT) {
                return error.As(chunkOrderError)!;
            }
            d.stage = dsSeenIEND;
            return error.As(d.parseIEND(length))!;

            break;
    }
    if (length > 0x7fffffff) {
        return error.As(FormatError(fmt.Sprintf("Bad chunk length: %d", length)))!;
    }
    array<byte> ignored = new array<byte>(4096);
    while (length > 0) {
        var (n, err) = io.ReadFull(d.r, ignored[..(int)min(len(ignored), int(length))]);
        if (err != null) {
            return error.As(err)!;
        }
        d.crc.Write(ignored[..(int)n]);
        length -= uint32(n);

    }
    return error.As(d.verifyChecksum())!;

}

private static error verifyChecksum(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;

    {
        var (_, err) = io.ReadFull(d.r, d.tmp[..(int)4]);

        if (err != null) {
            return error.As(err)!;
        }
    }

    if (binary.BigEndian.Uint32(d.tmp[..(int)4]) != d.crc.Sum32()) {
        return error.As(FormatError("invalid checksum"))!;
    }
    return error.As(null!)!;

}

private static error checkHeader(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;

    var (_, err) = io.ReadFull(d.r, d.tmp[..(int)len(pngHeader)]);
    if (err != null) {
        return error.As(err)!;
    }
    if (string(d.tmp[..(int)len(pngHeader)]) != pngHeader) {
        return error.As(FormatError("not a PNG file"))!;
    }
    return error.As(null!)!;

}

// Decode reads a PNG image from r and returns it as an image.Image.
// The type of Image returned depends on the PNG contents.
public static (image.Image, error) Decode(io.Reader r) {
    image.Image _p0 = default;
    error _p0 = default!;

    ptr<decoder> d = addr(new decoder(r:r,crc:crc32.NewIEEE(),));
    {
        var err__prev1 = err;

        var err = d.checkHeader();

        if (err != null) {
            if (err == io.EOF) {
                err = io.ErrUnexpectedEOF;
            }
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    while (d.stage != dsSeenIEND) {
        {
            var err__prev1 = err;

            err = d.parseChunk();

            if (err != null) {
                if (err == io.EOF) {
                    err = io.ErrUnexpectedEOF;
                }
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

    }
    return (d.img, error.As(null!)!);

}

// DecodeConfig returns the color model and dimensions of a PNG image without
// decoding the entire image.
public static (image.Config, error) DecodeConfig(io.Reader r) {
    image.Config _p0 = default;
    error _p0 = default!;

    ptr<decoder> d = addr(new decoder(r:r,crc:crc32.NewIEEE(),));
    {
        var err__prev1 = err;

        var err = d.checkHeader();

        if (err != null) {
            if (err == io.EOF) {
                err = io.ErrUnexpectedEOF;
            }
            return (new image.Config(), error.As(err)!);
        }
        err = err__prev1;

    }

    while (true) {
        {
            var err__prev1 = err;

            err = d.parseChunk();

            if (err != null) {
                if (err == io.EOF) {
                    err = io.ErrUnexpectedEOF;
                }
                return (new image.Config(), error.As(err)!);
            }

            err = err__prev1;

        }

        var paletted = cbPaletted(d.cb);
        if (d.stage == dsSeenIHDR && !paletted) {
            break;
        }
        if (d.stage == dsSeenPLTE && paletted) {
            break;
        }
    }
    color.Model cm = default;

    if (d.cb == cbG1 || d.cb == cbG2 || d.cb == cbG4 || d.cb == cbG8) 
        cm = color.GrayModel;
    else if (d.cb == cbGA8) 
        cm = color.NRGBAModel;
    else if (d.cb == cbTC8) 
        cm = color.RGBAModel;
    else if (d.cb == cbP1 || d.cb == cbP2 || d.cb == cbP4 || d.cb == cbP8) 
        cm = d.palette;
    else if (d.cb == cbTCA8) 
        cm = color.NRGBAModel;
    else if (d.cb == cbG16) 
        cm = color.Gray16Model;
    else if (d.cb == cbGA16) 
        cm = color.NRGBA64Model;
    else if (d.cb == cbTC16) 
        cm = color.RGBA64Model;
    else if (d.cb == cbTCA16) 
        cm = color.NRGBA64Model;
        return (new image.Config(ColorModel:cm,Width:d.width,Height:d.height,), error.As(null!)!);

}

private static void init() {
    image.RegisterFormat("png", pngHeader, Decode, DecodeConfig);
}

} // end png_package
