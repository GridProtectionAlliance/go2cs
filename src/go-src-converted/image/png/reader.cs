// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package png implements a PNG image decoder and encoder.
//
// The PNG specification is at https://www.w3.org/TR/PNG/.
// package png -- go2cs converted at 2020 October 09 06:06:02 UTC
// import "image/png" ==> using png = go.image.png_package
// Original source: C:\Go\src\image\png\reader.go
using zlib = go.compress.zlib_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using hash = go.hash_package;
using crc32 = go.hash.crc32_package;
using image = go.image_package;
using color = go.image.color_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace image
{
    public static partial class png_package
    {
        // Color type, as per the PNG spec.
        private static readonly long ctGrayscale = (long)0L;
        private static readonly long ctTrueColor = (long)2L;
        private static readonly long ctPaletted = (long)3L;
        private static readonly long ctGrayscaleAlpha = (long)4L;
        private static readonly long ctTrueColorAlpha = (long)6L;


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


        private static bool cbPaletted(long cb)
        {
            return cbP1 <= cb && cb <= cbP8;
        }

        // Filter type, as per the PNG spec.
        private static readonly long ftNone = (long)0L;
        private static readonly long ftSub = (long)1L;
        private static readonly long ftUp = (long)2L;
        private static readonly long ftAverage = (long)3L;
        private static readonly long ftPaeth = (long)4L;
        private static readonly long nFilter = (long)5L;


        // Interlace type.
        private static readonly long itNone = (long)0L;
        private static readonly long itAdam7 = (long)1L;


        // interlaceScan defines the placement and size of a pass for Adam7 interlacing.
        private partial struct interlaceScan
        {
            public long xFactor;
            public long yFactor;
            public long xOffset;
            public long yOffset;
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


        private static readonly @string pngHeader = (@string)"\x89PNG\r\n\x1a\n";



        private partial struct decoder
        {
            public io.Reader r;
            public image.Image img;
            public hash.Hash32 crc;
            public long width;
            public long height;
            public long depth;
            public color.Palette palette;
            public long cb;
            public long stage;
            public uint idatLength;
            public array<byte> tmp;
            public long interlace; // useTransparent and transparent are used for grayscale and truecolor
// transparency, as opposed to palette transparency.
            public bool useTransparent;
            public array<byte> transparent;
        }

        // A FormatError reports that the input is not a valid PNG.
        public partial struct FormatError // : @string
        {
        }

        public static @string Error(this FormatError e)
        {
            return "png: invalid format: " + string(e);
        }

        private static var chunkOrderError = FormatError("chunk out of order");

        // An UnsupportedError reports that the input uses a valid but unimplemented PNG feature.
        public partial struct UnsupportedError // : @string
        {
        }

        public static @string Error(this UnsupportedError e)
        {
            return "png: unsupported feature: " + string(e);
        }

        private static long min(long a, long b)
        {
            if (a < b)
            {
                return a;
            }

            return b;

        }

        private static error parseIHDR(this ptr<decoder> _addr_d, uint length)
        {
            ref decoder d = ref _addr_d.val;

            if (length != 13L)
            {
                return error.As(FormatError("bad IHDR length"))!;
            }

            {
                var (_, err) = io.ReadFull(d.r, d.tmp[..13L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            d.crc.Write(d.tmp[..13L]);
            if (d.tmp[10L] != 0L)
            {
                return error.As(UnsupportedError("compression method"))!;
            }

            if (d.tmp[11L] != 0L)
            {
                return error.As(UnsupportedError("filter method"))!;
            }

            if (d.tmp[12L] != itNone && d.tmp[12L] != itAdam7)
            {
                return error.As(FormatError("invalid interlace method"))!;
            }

            d.interlace = int(d.tmp[12L]);

            var w = int32(binary.BigEndian.Uint32(d.tmp[0L..4L]));
            var h = int32(binary.BigEndian.Uint32(d.tmp[4L..8L]));
            if (w <= 0L || h <= 0L)
            {
                return error.As(FormatError("non-positive dimension"))!;
            }

            var nPixels64 = int64(w) * int64(h);
            var nPixels = int(nPixels64);
            if (nPixels64 != int64(nPixels))
            {
                return error.As(UnsupportedError("dimension overflow"))!;
            } 
            // There can be up to 8 bytes per pixel, for 16 bits per channel RGBA.
            if (nPixels != (nPixels * 8L) / 8L)
            {
                return error.As(UnsupportedError("dimension overflow"))!;
            }

            d.cb = cbInvalid;
            d.depth = int(d.tmp[8L]);
            switch (d.depth)
            {
                case 1L: 

                    if (d.tmp[9L] == ctGrayscale) 
                        d.cb = cbG1;
                    else if (d.tmp[9L] == ctPaletted) 
                        d.cb = cbP1;
                    break;
                case 2L: 

                    if (d.tmp[9L] == ctGrayscale) 
                        d.cb = cbG2;
                    else if (d.tmp[9L] == ctPaletted) 
                        d.cb = cbP2;
                    break;
                case 4L: 

                    if (d.tmp[9L] == ctGrayscale) 
                        d.cb = cbG4;
                    else if (d.tmp[9L] == ctPaletted) 
                        d.cb = cbP4;
                    break;
                case 8L: 

                    if (d.tmp[9L] == ctGrayscale) 
                        d.cb = cbG8;
                    else if (d.tmp[9L] == ctTrueColor) 
                        d.cb = cbTC8;
                    else if (d.tmp[9L] == ctPaletted) 
                        d.cb = cbP8;
                    else if (d.tmp[9L] == ctGrayscaleAlpha) 
                        d.cb = cbGA8;
                    else if (d.tmp[9L] == ctTrueColorAlpha) 
                        d.cb = cbTCA8;
                    break;
                case 16L: 

                    if (d.tmp[9L] == ctGrayscale) 
                        d.cb = cbG16;
                    else if (d.tmp[9L] == ctTrueColor) 
                        d.cb = cbTC16;
                    else if (d.tmp[9L] == ctGrayscaleAlpha) 
                        d.cb = cbGA16;
                    else if (d.tmp[9L] == ctTrueColorAlpha) 
                        d.cb = cbTCA16;
                    break;
            }
            if (d.cb == cbInvalid)
            {
                return error.As(UnsupportedError(fmt.Sprintf("bit depth %d, color type %d", d.tmp[8L], d.tmp[9L])))!;
            }

            d.width = int(w);
            d.height = int(h);
            return error.As(d.verifyChecksum())!;

        }

        private static error parsePLTE(this ptr<decoder> _addr_d, uint length)
        {
            ref decoder d = ref _addr_d.val;

            var np = int(length / 3L); // The number of palette entries.
            if (length % 3L != 0L || np <= 0L || np > 256L || np > 1L << (int)(uint(d.depth)))
            {
                return error.As(FormatError("bad PLTE length"))!;
            }

            var (n, err) = io.ReadFull(d.r, d.tmp[..3L * np]);
            if (err != null)
            {
                return error.As(err)!;
            }

            d.crc.Write(d.tmp[..n]);

            if (d.cb == cbP1 || d.cb == cbP2 || d.cb == cbP4 || d.cb == cbP8) 
                d.palette = make(color.Palette, 256L);
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < np; i++)
                    {
                        d.palette[i] = new color.RGBA(d.tmp[3*i+0],d.tmp[3*i+1],d.tmp[3*i+2],0xff);
                    }


                    i = i__prev1;
                }
                {
                    long i__prev1 = i;

                    for (i = np; i < 256L; i++)
                    { 
                        // Initialize the rest of the palette to opaque black. The spec (section
                        // 11.2.3) says that "any out-of-range pixel value found in the image data
                        // is an error", but some real-world PNG files have out-of-range pixel
                        // values. We fall back to opaque black, the same as libpng 1.5.13;
                        // ImageMagick 6.5.7 returns an error.
                        d.palette[i] = new color.RGBA(0x00,0x00,0x00,0xff);

                    }


                    i = i__prev1;
                }
                d.palette = d.palette[..np];
            else if (d.cb == cbTC8 || d.cb == cbTCA8 || d.cb == cbTC16 || d.cb == cbTCA16)             else 
                return error.As(FormatError("PLTE, color type mismatch"))!;
                        return error.As(d.verifyChecksum())!;

        }

        private static error parsetRNS(this ptr<decoder> _addr_d, uint length)
        {
            ref decoder d = ref _addr_d.val;


            if (d.cb == cbG1 || d.cb == cbG2 || d.cb == cbG4 || d.cb == cbG8 || d.cb == cbG16) 
                if (length != 2L)
                {
                    return error.As(FormatError("bad tRNS length"))!;
                }

                var (n, err) = io.ReadFull(d.r, d.tmp[..length]);
                if (err != null)
                {
                    return error.As(err)!;
                }

                d.crc.Write(d.tmp[..n]);

                copy(d.transparent[..], d.tmp[..length]);

                if (d.cb == cbG1) 
                    d.transparent[1L] *= 0xffUL;
                else if (d.cb == cbG2) 
                    d.transparent[1L] *= 0x55UL;
                else if (d.cb == cbG4) 
                    d.transparent[1L] *= 0x11UL;
                                d.useTransparent = true;
            else if (d.cb == cbTC8 || d.cb == cbTC16) 
                if (length != 6L)
                {
                    return error.As(FormatError("bad tRNS length"))!;
                }

                (n, err) = io.ReadFull(d.r, d.tmp[..length]);
                if (err != null)
                {
                    return error.As(err)!;
                }

                d.crc.Write(d.tmp[..n]);

                copy(d.transparent[..], d.tmp[..length]);
                d.useTransparent = true;
            else if (d.cb == cbP1 || d.cb == cbP2 || d.cb == cbP4 || d.cb == cbP8) 
                if (length > 256L)
                {
                    return error.As(FormatError("bad tRNS length"))!;
                }

                (n, err) = io.ReadFull(d.r, d.tmp[..length]);
                if (err != null)
                {
                    return error.As(err)!;
                }

                d.crc.Write(d.tmp[..n]);

                if (len(d.palette) < n)
                {
                    d.palette = d.palette[..n];
                }

                for (long i = 0L; i < n; i++)
                {
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
        private static (long, error) Read(this ptr<decoder> _addr_d, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            if (len(p) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            while (d.idatLength == 0L)
            { 
                // We have exhausted an IDAT chunk. Verify the checksum of that chunk.
                {
                    var err = d.verifyChecksum();

                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    } 
                    // Read the length and chunk type of the next chunk, and check that
                    // it is an IDAT chunk.

                } 
                // Read the length and chunk type of the next chunk, and check that
                // it is an IDAT chunk.
                {
                    var (_, err) = io.ReadFull(d.r, d.tmp[..8L]);

                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    }

                }

                d.idatLength = binary.BigEndian.Uint32(d.tmp[..4L]);
                if (string(d.tmp[4L..8L]) != "IDAT")
                {
                    return (0L, error.As(FormatError("not enough pixel data"))!);
                }

                d.crc.Reset();
                d.crc.Write(d.tmp[4L..8L]);

            }

            if (int(d.idatLength) < 0L)
            {
                return (0L, error.As(UnsupportedError("IDAT chunk length overflow"))!);
            }

            var (n, err) = d.r.Read(p[..min(len(p), int(d.idatLength))]);
            d.crc.Write(p[..n]);
            d.idatLength -= uint32(n);
            return (n, error.As(err)!);

        }

        // decode decodes the IDAT data into an image.
        private static (image.Image, error) decode(this ptr<decoder> _addr_d) => func((defer, _, __) =>
        {
            image.Image _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            var (r, err) = zlib.NewReader(d);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(r.Close());
            image.Image img = default;
            if (d.interlace == itNone)
            {
                img, err = d.readImagePass(r, 0L, false);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }
            else if (d.interlace == itAdam7)
            { 
                // Allocate a blank image of the full size.
                img, err = d.readImagePass(null, 0L, true);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                for (long pass = 0L; pass < 7L; pass++)
                {
                    var (imagePass, err) = d.readImagePass(r, pass, false);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    if (imagePass != null)
                    {
                        d.mergePassInto(img, imagePass, pass);
                    }

                }


            } 

            // Check for EOF, to verify the zlib checksum.
            long n = 0L;
            for (long i = 0L; n == 0L && err == null; i++)
            {
                if (i == 100L)
                {
                    return (null, error.As(io.ErrNoProgress)!);
                }

                n, err = r.Read(d.tmp[..1L]);

            }

            if (err != null && err != io.EOF)
            {
                return (null, error.As(FormatError(err.Error()))!);
            }

            if (n != 0L || d.idatLength != 0L)
            {
                return (null, error.As(FormatError("too much pixel data"))!);
            }

            return (img, error.As(null!)!);

        });

        // readImagePass reads a single image pass, sized according to the pass number.
        private static (image.Image, error) readImagePass(this ptr<decoder> _addr_d, io.Reader r, long pass, bool allocateOnly)
        {
            image.Image _p0 = default;
            error _p0 = default!;
            ref decoder d = ref _addr_d.val;

            long bitsPerPixel = 0L;
            long pixOffset = 0L;
            ptr<image.Gray> gray;            ptr<image.RGBA> rgba;            ptr<image.Paletted> paletted;            ptr<image.NRGBA> nrgba;            ptr<image.Gray16> gray16;            ptr<image.RGBA64> rgba64;            ptr<image.NRGBA64> nrgba64;            image.Image img = default;
            var width = d.width;
            var height = d.height;
            if (d.interlace == itAdam7 && !allocateOnly)
            {
                var p = interlacing[pass]; 
                // Add the multiplication factor and subtract one, effectively rounding up.
                width = (width - p.xOffset + p.xFactor - 1L) / p.xFactor;
                height = (height - p.yOffset + p.yFactor - 1L) / p.yFactor; 
                // A PNG image can't have zero width or height, but for an interlaced
                // image, an individual pass might have zero width or height. If so, we
                // shouldn't even read a per-row filter type byte, so return early.
                if (width == 0L || height == 0L)
                {
                    return (null, error.As(null!)!);
                }

            }


            if (d.cb == cbG1 || d.cb == cbG2 || d.cb == cbG4 || d.cb == cbG8) 
                bitsPerPixel = d.depth;
                if (d.useTransparent)
                {
                    nrgba = image.NewNRGBA(image.Rect(0L, 0L, width, height));
                    img = nrgba;
                }
                else
                {
                    gray = image.NewGray(image.Rect(0L, 0L, width, height));
                    img = gray;
                }

            else if (d.cb == cbGA8) 
                bitsPerPixel = 16L;
                nrgba = image.NewNRGBA(image.Rect(0L, 0L, width, height));
                img = nrgba;
            else if (d.cb == cbTC8) 
                bitsPerPixel = 24L;
                if (d.useTransparent)
                {
                    nrgba = image.NewNRGBA(image.Rect(0L, 0L, width, height));
                    img = nrgba;
                }
                else
                {
                    rgba = image.NewRGBA(image.Rect(0L, 0L, width, height));
                    img = rgba;
                }

            else if (d.cb == cbP1 || d.cb == cbP2 || d.cb == cbP4 || d.cb == cbP8) 
                bitsPerPixel = d.depth;
                paletted = image.NewPaletted(image.Rect(0L, 0L, width, height), d.palette);
                img = paletted;
            else if (d.cb == cbTCA8) 
                bitsPerPixel = 32L;
                nrgba = image.NewNRGBA(image.Rect(0L, 0L, width, height));
                img = nrgba;
            else if (d.cb == cbG16) 
                bitsPerPixel = 16L;
                if (d.useTransparent)
                {
                    nrgba64 = image.NewNRGBA64(image.Rect(0L, 0L, width, height));
                    img = nrgba64;
                }
                else
                {
                    gray16 = image.NewGray16(image.Rect(0L, 0L, width, height));
                    img = gray16;
                }

            else if (d.cb == cbGA16) 
                bitsPerPixel = 32L;
                nrgba64 = image.NewNRGBA64(image.Rect(0L, 0L, width, height));
                img = nrgba64;
            else if (d.cb == cbTC16) 
                bitsPerPixel = 48L;
                if (d.useTransparent)
                {
                    nrgba64 = image.NewNRGBA64(image.Rect(0L, 0L, width, height));
                    img = nrgba64;
                }
                else
                {
                    rgba64 = image.NewRGBA64(image.Rect(0L, 0L, width, height));
                    img = rgba64;
                }

            else if (d.cb == cbTCA16) 
                bitsPerPixel = 64L;
                nrgba64 = image.NewNRGBA64(image.Rect(0L, 0L, width, height));
                img = nrgba64;
                        if (allocateOnly)
            {
                return (img, error.As(null!)!);
            }

            var bytesPerPixel = (bitsPerPixel + 7L) / 8L; 

            // The +1 is for the per-row filter type, which is at cr[0].
            long rowSize = 1L + (int64(bitsPerPixel) * int64(width) + 7L) / 8L;
            if (rowSize != int64(int(rowSize)))
            {
                return (null, error.As(UnsupportedError("dimension overflow"))!);
            } 
            // cr and pr are the bytes for the current and previous row.
            var cr = make_slice<byte>(rowSize);
            var pr = make_slice<byte>(rowSize);

            for (long y = 0L; y < height; y++)
            { 
                // Read the decompressed bytes.
                var (_, err) = io.ReadFull(r, cr);
                if (err != null)
                {
                    if (err == io.EOF || err == io.ErrUnexpectedEOF)
                    {
                        return (null, error.As(FormatError("not enough pixel data"))!);
                    }

                    return (null, error.As(err)!);

                } 

                // Apply the filter.
                var cdat = cr[1L..];
                var pdat = pr[1L..];

                if (cr[0L] == ftNone)                 else if (cr[0L] == ftSub) 
                    {
                        var i__prev2 = i;

                        for (var i = bytesPerPixel; i < len(cdat); i++)
                        {
                            cdat[i] += cdat[i - bytesPerPixel];
                        }


                        i = i__prev2;
                    }
                else if (cr[0L] == ftUp) 
                    {
                        var i__prev2 = i;
                        var p__prev2 = p;

                        foreach (var (__i, __p) in pdat)
                        {
                            i = __i;
                            p = __p;
                            cdat[i] += p;
                        }

                        i = i__prev2;
                        p = p__prev2;
                    }
                else if (cr[0L] == ftAverage) 
                    // The first column has no column to the left of it, so it is a
                    // special case. We know that the first column exists because we
                    // check above that width != 0, and so len(cdat) != 0.
                    {
                        var i__prev2 = i;

                        for (i = 0L; i < bytesPerPixel; i++)
                        {
                            cdat[i] += pdat[i] / 2L;
                        }


                        i = i__prev2;
                    }
                    {
                        var i__prev2 = i;

                        for (i = bytesPerPixel; i < len(cdat); i++)
                        {
                            cdat[i] += uint8((int(cdat[i - bytesPerPixel]) + int(pdat[i])) / 2L);
                        }


                        i = i__prev2;
                    }
                else if (cr[0L] == ftPaeth) 
                    filterPaeth(cdat, pdat, bytesPerPixel);
                else 
                    return (null, error.As(FormatError("bad filter type"))!);
                // Convert from bytes to colors.

                if (d.cb == cbG1) 
                    if (d.useTransparent)
                    {
                        var ty = d.transparent[1L];
                        {
                            long x__prev2 = x;

                            long x = 0L;

                            while (x < width)
                            {
                                var b = cdat[x / 8L];
                                {
                                    long x2__prev3 = x2;

                                    for (long x2 = 0L; x2 < 8L && x + x2 < width; x2++)
                                    {
                                        var ycol = (b >> (int)(7L)) * 0xffUL;
                                        var acol = uint8(0xffUL);
                                        if (ycol == ty)
                                        {
                                            acol = 0x00UL;
                                        }

                                        nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol,ycol,ycol,acol));
                                        b <<= 1L;
                                x += 8L;
                                    }
                    else


                                    x2 = x2__prev3;
                                }

                            }


                            x = x__prev2;
                        }

                    }                    {
                        {
                            long x__prev2 = x;

                            x = 0L;

                            while (x < width)
                            {
                                b = cdat[x / 8L];
                                {
                                    long x2__prev3 = x2;

                                    for (x2 = 0L; x2 < 8L && x + x2 < width; x2++)
                                    {
                                        gray.SetGray(x + x2, y, new color.Gray((b>>7)*0xff));
                                        b <<= 1L;
                                    }


                                    x2 = x2__prev3;
                                }
                                x += 8L;
                            }


                            x = x__prev2;
                        }

                    }

                else if (d.cb == cbG2) 
                    if (d.useTransparent)
                    {
                        ty = d.transparent[1L];
                        {
                            long x__prev2 = x;

                            x = 0L;

                            while (x < width)
                            {
                                b = cdat[x / 4L];
                                {
                                    long x2__prev3 = x2;

                                    for (x2 = 0L; x2 < 4L && x + x2 < width; x2++)
                                    {
                                        ycol = (b >> (int)(6L)) * 0x55UL;
                                        acol = uint8(0xffUL);
                                        if (ycol == ty)
                                        {
                                            acol = 0x00UL;
                                        }

                                        nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol,ycol,ycol,acol));
                                        b <<= 2L;
                                x += 4L;
                                    }
                    else


                                    x2 = x2__prev3;
                                }

                            }


                            x = x__prev2;
                        }

                    }                    {
                        {
                            long x__prev2 = x;

                            x = 0L;

                            while (x < width)
                            {
                                b = cdat[x / 4L];
                                {
                                    long x2__prev3 = x2;

                                    for (x2 = 0L; x2 < 4L && x + x2 < width; x2++)
                                    {
                                        gray.SetGray(x + x2, y, new color.Gray((b>>6)*0x55));
                                        b <<= 2L;
                                    }


                                    x2 = x2__prev3;
                                }
                                x += 4L;
                            }


                            x = x__prev2;
                        }

                    }

                else if (d.cb == cbG4) 
                    if (d.useTransparent)
                    {
                        ty = d.transparent[1L];
                        {
                            long x__prev2 = x;

                            x = 0L;

                            while (x < width)
                            {
                                b = cdat[x / 2L];
                                {
                                    long x2__prev3 = x2;

                                    for (x2 = 0L; x2 < 2L && x + x2 < width; x2++)
                                    {
                                        ycol = (b >> (int)(4L)) * 0x11UL;
                                        acol = uint8(0xffUL);
                                        if (ycol == ty)
                                        {
                                            acol = 0x00UL;
                                        }

                                        nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol,ycol,ycol,acol));
                                        b <<= 4L;
                                x += 2L;
                                    }
                    else


                                    x2 = x2__prev3;
                                }

                            }


                            x = x__prev2;
                        }

                    }                    {
                        {
                            long x__prev2 = x;

                            x = 0L;

                            while (x < width)
                            {
                                b = cdat[x / 2L];
                                {
                                    long x2__prev3 = x2;

                                    for (x2 = 0L; x2 < 2L && x + x2 < width; x2++)
                                    {
                                        gray.SetGray(x + x2, y, new color.Gray((b>>4)*0x11));
                                        b <<= 4L;
                                    }


                                    x2 = x2__prev3;
                                }
                                x += 2L;
                            }


                            x = x__prev2;
                        }

                    }

                else if (d.cb == cbG8) 
                    if (d.useTransparent)
                    {
                        ty = d.transparent[1L];
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                ycol = cdat[x];
                                acol = uint8(0xffUL);
                                if (ycol == ty)
                                {
                                    acol = 0x00UL;
                                }

                                nrgba.SetNRGBA(x, y, new color.NRGBA(ycol,ycol,ycol,acol));

                            }
                    else


                            x = x__prev2;
                        }

                    }                    {
                        copy(gray.Pix[pixOffset..], cdat);
                        pixOffset += gray.Stride;
                    }

                else if (d.cb == cbGA8) 
                    {
                        long x__prev2 = x;

                        for (x = 0L; x < width; x++)
                        {
                            ycol = cdat[2L * x + 0L];
                            nrgba.SetNRGBA(x, y, new color.NRGBA(ycol,ycol,ycol,cdat[2*x+1]));
                        }


                        x = x__prev2;
                    }
                else if (d.cb == cbTC8) 
                    if (d.useTransparent)
                    {
                        var pix = nrgba.Pix;
                        i = pixOffset;
                        long j = 0L;
                        var tr = d.transparent[1L];
                        var tg = d.transparent[3L];
                        var tb = d.transparent[5L];
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                var r = cdat[j + 0L];
                                var g = cdat[j + 1L];
                                b = cdat[j + 2L];
                                var a = uint8(0xffUL);
                                if (r == tr && g == tg && b == tb)
                                {
                                    a = 0x00UL;
                                }

                                pix[i + 0L] = r;
                                pix[i + 1L] = g;
                                pix[i + 2L] = b;
                                pix[i + 3L] = a;
                                i += 4L;
                                j += 3L;

                            }
                    else


                            x = x__prev2;
                        }
                        pixOffset += nrgba.Stride;

                    }                    {
                        pix = rgba.Pix;
                        i = pixOffset;
                        j = 0L;
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                pix[i + 0L] = cdat[j + 0L];
                                pix[i + 1L] = cdat[j + 1L];
                                pix[i + 2L] = cdat[j + 2L];
                                pix[i + 3L] = 0xffUL;
                                i += 4L;
                                j += 3L;
                            }


                            x = x__prev2;
                        }
                        pixOffset += rgba.Stride;

                    }

                else if (d.cb == cbP1) 
                    {
                        long x__prev2 = x;

                        x = 0L;

                        while (x < width)
                        {
                            b = cdat[x / 8L];
                            {
                                long x2__prev3 = x2;

                                for (x2 = 0L; x2 < 8L && x + x2 < width; x2++)
                                {
                                    var idx = b >> (int)(7L);
                                    if (len(paletted.Palette) <= int(idx))
                                    {
                                        paletted.Palette = paletted.Palette[..int(idx) + 1L];
                                    }

                                    paletted.SetColorIndex(x + x2, y, idx);
                                    b <<= 1L;
                            x += 8L;
                                }


                                x2 = x2__prev3;
                            }

                        }


                        x = x__prev2;
                    }
                else if (d.cb == cbP2) 
                    {
                        long x__prev2 = x;

                        x = 0L;

                        while (x < width)
                        {
                            b = cdat[x / 4L];
                            {
                                long x2__prev3 = x2;

                                for (x2 = 0L; x2 < 4L && x + x2 < width; x2++)
                                {
                                    idx = b >> (int)(6L);
                                    if (len(paletted.Palette) <= int(idx))
                                    {
                                        paletted.Palette = paletted.Palette[..int(idx) + 1L];
                                    }

                                    paletted.SetColorIndex(x + x2, y, idx);
                                    b <<= 2L;
                            x += 4L;
                                }


                                x2 = x2__prev3;
                            }

                        }


                        x = x__prev2;
                    }
                else if (d.cb == cbP4) 
                    {
                        long x__prev2 = x;

                        x = 0L;

                        while (x < width)
                        {
                            b = cdat[x / 2L];
                            {
                                long x2__prev3 = x2;

                                for (x2 = 0L; x2 < 2L && x + x2 < width; x2++)
                                {
                                    idx = b >> (int)(4L);
                                    if (len(paletted.Palette) <= int(idx))
                                    {
                                        paletted.Palette = paletted.Palette[..int(idx) + 1L];
                                    }

                                    paletted.SetColorIndex(x + x2, y, idx);
                                    b <<= 4L;
                            x += 2L;
                                }


                                x2 = x2__prev3;
                            }

                        }


                        x = x__prev2;
                    }
                else if (d.cb == cbP8) 
                    if (len(paletted.Palette) != 256L)
                    {
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                if (len(paletted.Palette) <= int(cdat[x]))
                                {
                                    paletted.Palette = paletted.Palette[..int(cdat[x]) + 1L];
                                }

                            }


                            x = x__prev2;
                        }

                    }

                    copy(paletted.Pix[pixOffset..], cdat);
                    pixOffset += paletted.Stride;
                else if (d.cb == cbTCA8) 
                    copy(nrgba.Pix[pixOffset..], cdat);
                    pixOffset += nrgba.Stride;
                else if (d.cb == cbG16) 
                    if (d.useTransparent)
                    {
                        ty = uint16(d.transparent[0L]) << (int)(8L) | uint16(d.transparent[1L]);
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                ycol = uint16(cdat[2L * x + 0L]) << (int)(8L) | uint16(cdat[2L * x + 1L]);
                                acol = uint16(0xffffUL);
                                if (ycol == ty)
                                {
                                    acol = 0x0000UL;
                                }

                                nrgba64.SetNRGBA64(x, y, new color.NRGBA64(ycol,ycol,ycol,acol));

                            }
                    else


                            x = x__prev2;
                        }

                    }                    {
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                ycol = uint16(cdat[2L * x + 0L]) << (int)(8L) | uint16(cdat[2L * x + 1L]);
                                gray16.SetGray16(x, y, new color.Gray16(ycol));
                            }


                            x = x__prev2;
                        }

                    }

                else if (d.cb == cbGA16) 
                    {
                        long x__prev2 = x;

                        for (x = 0L; x < width; x++)
                        {
                            ycol = uint16(cdat[4L * x + 0L]) << (int)(8L) | uint16(cdat[4L * x + 1L]);
                            acol = uint16(cdat[4L * x + 2L]) << (int)(8L) | uint16(cdat[4L * x + 3L]);
                            nrgba64.SetNRGBA64(x, y, new color.NRGBA64(ycol,ycol,ycol,acol));
                        }


                        x = x__prev2;
                    }
                else if (d.cb == cbTC16) 
                    if (d.useTransparent)
                    {
                        tr = uint16(d.transparent[0L]) << (int)(8L) | uint16(d.transparent[1L]);
                        tg = uint16(d.transparent[2L]) << (int)(8L) | uint16(d.transparent[3L]);
                        tb = uint16(d.transparent[4L]) << (int)(8L) | uint16(d.transparent[5L]);
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                var rcol = uint16(cdat[6L * x + 0L]) << (int)(8L) | uint16(cdat[6L * x + 1L]);
                                var gcol = uint16(cdat[6L * x + 2L]) << (int)(8L) | uint16(cdat[6L * x + 3L]);
                                var bcol = uint16(cdat[6L * x + 4L]) << (int)(8L) | uint16(cdat[6L * x + 5L]);
                                acol = uint16(0xffffUL);
                                if (rcol == tr && gcol == tg && bcol == tb)
                                {
                                    acol = 0x0000UL;
                                }

                                nrgba64.SetNRGBA64(x, y, new color.NRGBA64(rcol,gcol,bcol,acol));

                            }
                    else


                            x = x__prev2;
                        }

                    }                    {
                        {
                            long x__prev2 = x;

                            for (x = 0L; x < width; x++)
                            {
                                rcol = uint16(cdat[6L * x + 0L]) << (int)(8L) | uint16(cdat[6L * x + 1L]);
                                gcol = uint16(cdat[6L * x + 2L]) << (int)(8L) | uint16(cdat[6L * x + 3L]);
                                bcol = uint16(cdat[6L * x + 4L]) << (int)(8L) | uint16(cdat[6L * x + 5L]);
                                rgba64.SetRGBA64(x, y, new color.RGBA64(rcol,gcol,bcol,0xffff));
                            }


                            x = x__prev2;
                        }

                    }

                else if (d.cb == cbTCA16) 
                    {
                        long x__prev2 = x;

                        for (x = 0L; x < width; x++)
                        {
                            rcol = uint16(cdat[8L * x + 0L]) << (int)(8L) | uint16(cdat[8L * x + 1L]);
                            gcol = uint16(cdat[8L * x + 2L]) << (int)(8L) | uint16(cdat[8L * x + 3L]);
                            bcol = uint16(cdat[8L * x + 4L]) << (int)(8L) | uint16(cdat[8L * x + 5L]);
                            acol = uint16(cdat[8L * x + 6L]) << (int)(8L) | uint16(cdat[8L * x + 7L]);
                            nrgba64.SetNRGBA64(x, y, new color.NRGBA64(rcol,gcol,bcol,acol));
                        }


                        x = x__prev2;
                    }
                // The current row for y is the previous row for y+1.
                pr = cr;
                cr = pr;

            }


            return (img, error.As(null!)!);

        }

        // mergePassInto merges a single pass into a full sized image.
        private static void mergePassInto(this ptr<decoder> _addr_d, image.Image dst, image.Image src, long pass)
        {
            ref decoder d = ref _addr_d.val;

            var p = interlacing[pass];
            slice<byte> srcPix = default;            slice<byte> dstPix = default;            long stride = default;            image.Rectangle rect = default;            long bytesPerPixel = default;
            switch (dst.type())
            {
                case ptr<image.Alpha> target:
                    srcPix = src._<ptr<image.Alpha>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 1L;
                    break;
                case ptr<image.Alpha16> target:
                    srcPix = src._<ptr<image.Alpha16>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 2L;
                    break;
                case ptr<image.Gray> target:
                    srcPix = src._<ptr<image.Gray>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 1L;
                    break;
                case ptr<image.Gray16> target:
                    srcPix = src._<ptr<image.Gray16>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 2L;
                    break;
                case ptr<image.NRGBA> target:
                    srcPix = src._<ptr<image.NRGBA>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 4L;
                    break;
                case ptr<image.NRGBA64> target:
                    srcPix = src._<ptr<image.NRGBA64>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 8L;
                    break;
                case ptr<image.Paletted> target:
                    srcPix = src._<ptr<image.Paletted>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 1L;
                    break;
                case ptr<image.RGBA> target:
                    srcPix = src._<ptr<image.RGBA>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 4L;
                    break;
                case ptr<image.RGBA64> target:
                    srcPix = src._<ptr<image.RGBA64>>().Pix;
                    dstPix = target.Pix;
                    stride = target.Stride;
                    rect = target.Rect;
                    bytesPerPixel = 8L;
                    break;
            }
            long s = 0L;
            var bounds = src.Bounds();
            for (var y = bounds.Min.Y; y < bounds.Max.Y; y++)
            {
                var dBase = (y * p.yFactor + p.yOffset - rect.Min.Y) * stride + (p.xOffset - rect.Min.X) * bytesPerPixel;
                for (var x = bounds.Min.X; x < bounds.Max.X; x++)
                {
                    var d = dBase + x * p.xFactor * bytesPerPixel;
                    copy(dstPix[d..], srcPix[s..s + bytesPerPixel]);
                    s += bytesPerPixel;
                }


            }


        }

        private static error parseIDAT(this ptr<decoder> _addr_d, uint length)
        {
            error err = default!;
            ref decoder d = ref _addr_d.val;

            d.idatLength = length;
            d.img, err = d.decode();
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(d.verifyChecksum())!;

        }

        private static error parseIEND(this ptr<decoder> _addr_d, uint length)
        {
            ref decoder d = ref _addr_d.val;

            if (length != 0L)
            {
                return error.As(FormatError("bad IEND length"))!;
            }

            return error.As(d.verifyChecksum())!;

        }

        private static error parseChunk(this ptr<decoder> _addr_d)
        {
            ref decoder d = ref _addr_d.val;
 
            // Read the length and chunk type.
            {
                var (_, err) = io.ReadFull(d.r, d.tmp[..8L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            var length = binary.BigEndian.Uint32(d.tmp[..4L]);
            d.crc.Reset();
            d.crc.Write(d.tmp[4L..8L]); 

            // Read the chunk data.
            switch (string(d.tmp[4L..8L]))
            {
                case "IHDR": 
                    if (d.stage != dsStart)
                    {
                        return error.As(chunkOrderError)!;
                    }

                    d.stage = dsSeenIHDR;
                    return error.As(d.parseIHDR(length))!;
                    break;
                case "PLTE": 
                    if (d.stage != dsSeenIHDR)
                    {
                        return error.As(chunkOrderError)!;
                    }

                    d.stage = dsSeenPLTE;
                    return error.As(d.parsePLTE(length))!;
                    break;
                case "tRNS": 
                    if (cbPaletted(d.cb))
                    {
                        if (d.stage != dsSeenPLTE)
                        {
                            return error.As(chunkOrderError)!;
                        }

                    }
                    else if (d.stage != dsSeenIHDR)
                    {
                        return error.As(chunkOrderError)!;
                    }

                    d.stage = dsSeentRNS;
                    return error.As(d.parsetRNS(length))!;
                    break;
                case "IDAT": 
                    if (d.stage < dsSeenIHDR || d.stage > dsSeenIDAT || (d.stage == dsSeenIHDR && cbPaletted(d.cb)))
                    {
                        return error.As(chunkOrderError)!;
                    }
                    else if (d.stage == dsSeenIDAT)
                    { 
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
                    if (d.stage != dsSeenIDAT)
                    {
                        return error.As(chunkOrderError)!;
                    }

                    d.stage = dsSeenIEND;
                    return error.As(d.parseIEND(length))!;
                    break;
            }
            if (length > 0x7fffffffUL)
            {
                return error.As(FormatError(fmt.Sprintf("Bad chunk length: %d", length)))!;
            } 
            // Ignore this chunk (of a known length).
            array<byte> ignored = new array<byte>(4096L);
            while (length > 0L)
            {
                var (n, err) = io.ReadFull(d.r, ignored[..min(len(ignored), int(length))]);
                if (err != null)
                {
                    return error.As(err)!;
                }

                d.crc.Write(ignored[..n]);
                length -= uint32(n);

            }

            return error.As(d.verifyChecksum())!;

        }

        private static error verifyChecksum(this ptr<decoder> _addr_d)
        {
            ref decoder d = ref _addr_d.val;

            {
                var (_, err) = io.ReadFull(d.r, d.tmp[..4L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            if (binary.BigEndian.Uint32(d.tmp[..4L]) != d.crc.Sum32())
            {
                return error.As(FormatError("invalid checksum"))!;
            }

            return error.As(null!)!;

        }

        private static error checkHeader(this ptr<decoder> _addr_d)
        {
            ref decoder d = ref _addr_d.val;

            var (_, err) = io.ReadFull(d.r, d.tmp[..len(pngHeader)]);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (string(d.tmp[..len(pngHeader)]) != pngHeader)
            {
                return error.As(FormatError("not a PNG file"))!;
            }

            return error.As(null!)!;

        }

        // Decode reads a PNG image from r and returns it as an image.Image.
        // The type of Image returned depends on the PNG contents.
        public static (image.Image, error) Decode(io.Reader r)
        {
            image.Image _p0 = default;
            error _p0 = default!;

            ptr<decoder> d = addr(new decoder(r:r,crc:crc32.NewIEEE(),));
            {
                var err__prev1 = err;

                var err = d.checkHeader();

                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = io.ErrUnexpectedEOF;
                    }

                    return (null, error.As(err)!);

                }

                err = err__prev1;

            }

            while (d.stage != dsSeenIEND)
            {
                {
                    var err__prev1 = err;

                    err = d.parseChunk();

                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
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
        public static (image.Config, error) DecodeConfig(io.Reader r)
        {
            image.Config _p0 = default;
            error _p0 = default!;

            ptr<decoder> d = addr(new decoder(r:r,crc:crc32.NewIEEE(),));
            {
                var err__prev1 = err;

                var err = d.checkHeader();

                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = io.ErrUnexpectedEOF;
                    }

                    return (new image.Config(), error.As(err)!);

                }

                err = err__prev1;

            }

            while (true)
            {
                {
                    var err__prev1 = err;

                    err = d.parseChunk();

                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
                            err = io.ErrUnexpectedEOF;
                        }

                        return (new image.Config(), error.As(err)!);

                    }

                    err = err__prev1;

                }

                var paletted = cbPaletted(d.cb);
                if (d.stage == dsSeenIHDR && !paletted)
                {
                    break;
                }

                if (d.stage == dsSeenPLTE && paletted)
                {
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

        private static void init()
        {
            image.RegisterFormat("png", pngHeader, Decode, DecodeConfig);
        }
    }
}}
