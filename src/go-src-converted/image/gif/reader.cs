// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gif implements a GIF image decoder and encoder.
//
// The GIF specification is at http://www.w3.org/Graphics/GIF/spec-gif89a.txt.
// package gif -- go2cs converted at 2020 August 29 10:10:02 UTC
// import "image/gif" ==> using gif = go.image.gif_package
// Original source: C:\Go\src\image\gif\reader.go
using bufio = go.bufio_package;
using lzw = go.compress.lzw_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using image = go.image_package;
using color = go.image.color_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace image
{
    public static partial class gif_package
    {
        private static var errNotEnough = errors.New("gif: not enough image data");        private static var errTooMuch = errors.New("gif: too much image data");        private static var errBadPixel = errors.New("gif: invalid pixel value");

        // If the io.Reader does not also have ReadByte, then decode will introduce its own buffering.
        private partial interface reader : io.Reader, io.ByteReader
        {
        }

        // Masks etc.
 
        // Fields.
        private static readonly long fColorTable = 1L << (int)(7L);
        private static readonly long fInterlace = 1L << (int)(6L);
        private static readonly long fColorTableBitsMask = 7L; 

        // Graphic control flags.
        private static readonly long gcTransparentColorSet = 1L << (int)(0L);
        private static readonly long gcDisposalMethodMask = 7L << (int)(2L);

        // Disposal Methods.
        public static readonly ulong DisposalNone = 0x01UL;
        public static readonly ulong DisposalBackground = 0x02UL;
        public static readonly ulong DisposalPrevious = 0x03UL;

        // Section indicators.
        private static readonly ulong sExtension = 0x21UL;
        private static readonly ulong sImageDescriptor = 0x2CUL;
        private static readonly ulong sTrailer = 0x3BUL;

        // Extensions.
        private static readonly ulong eText = 0x01UL; // Plain Text
        private static readonly ulong eGraphicControl = 0xF9UL; // Graphic Control
        private static readonly ulong eComment = 0xFEUL; // Comment
        private static readonly ulong eApplication = 0xFFUL; // Application

        private static error readFull(io.Reader r, slice<byte> b)
        {
            var (_, err) = io.ReadFull(r, b);
            if (err == io.EOF)
            {
                err = io.ErrUnexpectedEOF;
            }
            return error.As(err);
        }

        private static (byte, error) readByte(io.ByteReader r)
        {
            var (b, err) = r.ReadByte();
            if (err == io.EOF)
            {
                err = io.ErrUnexpectedEOF;
            }
            return (b, err);
        }

        // decoder is the type used to decode a GIF file.
        private partial struct decoder
        {
            public reader r; // From header.
            public @string vers;
            public long width;
            public long height;
            public long loopCount;
            public long delayTime;
            public byte backgroundIndex;
            public byte disposalMethod; // From image descriptor.
            public byte imageFields; // From graphics control.
            public byte transparentIndex;
            public bool hasTransparentIndex; // Computed.
            public color.Palette globalColorTable; // Used when decoding.
            public slice<long> delay;
            public slice<byte> disposal;
            public slice<ref image.Paletted> image;
            public array<byte> tmp; // must be at least 768 so we can read color table
        }

        // blockReader parses the block structure of GIF image data, which comprises
        // (n, (n bytes)) blocks, with 1 <= n <= 255. It is the reader given to the
        // LZW decoder, which is thus immune to the blocking. After the LZW decoder
        // completes, there will be a 0-byte block remaining (0, ()), which is
        // consumed when checking that the blockReader is exhausted.
        //
        // To avoid the allocation of a bufio.Reader for the lzw Reader, blockReader
        // implements io.ReadByte and buffers blocks into the decoder's "tmp" buffer.
        private partial struct blockReader
        {
            public ptr<decoder> d;
            public byte i; // d.tmp[i:j] contains the buffered bytes
            public byte j; // d.tmp[i:j] contains the buffered bytes
            public error err;
        }

        private static void fill(this ref blockReader b)
        {
            if (b.err != null)
            {
                return;
            }
            b.j, b.err = readByte(b.d.r);
            if (b.j == 0L && b.err == null)
            {
                b.err = io.EOF;
            }
            if (b.err != null)
            {
                return;
            }
            b.i = 0L;
            b.err = readFull(b.d.r, b.d.tmp[..b.j]);
            if (b.err != null)
            {
                b.j = 0L;
            }
        }

        private static (byte, error) ReadByte(this ref blockReader b)
        {
            if (b.i == b.j)
            {
                b.fill();
                if (b.err != null)
                {
                    return (0L, b.err);
                }
            }
            var c = b.d.tmp[b.i];
            b.i++;
            return (c, null);
        }

        // blockReader must implement io.Reader, but its Read shouldn't ever actually
        // be called in practice. The compress/lzw package will only call ReadByte.
        private static (long, error) Read(this ref blockReader b, slice<byte> p)
        {
            if (len(p) == 0L || b.err != null)
            {
                return (0L, b.err);
            }
            if (b.i == b.j)
            {
                b.fill();
                if (b.err != null)
                {
                    return (0L, b.err);
                }
            }
            var n = copy(p, b.d.tmp[b.i..b.j]);
            b.i += uint8(n);
            return (n, null);
        }

        // close primarily detects whether or not a block terminator was encountered
        // after reading a sequence of data sub-blocks. It allows at most one trailing
        // sub-block worth of data. I.e., if some number of bytes exist in one sub-block
        // following the end of LZW data, the very next sub-block must be the block
        // terminator. If the very end of LZW data happened to fill one sub-block, at
        // most one more sub-block of length 1 may exist before the block-terminator.
        // These accomodations allow us to support GIFs created by less strict encoders.
        // See https://golang.org/issue/16146.
        private static error close(this ref blockReader b)
        {
            if (b.err == io.EOF)
            { 
                // A clean block-sequence terminator was encountered while reading.
                return error.As(null);
            }
            else if (b.err != null)
            { 
                // Some other error was encountered while reading.
                return error.As(b.err);
            }
            if (b.i == b.j)
            { 
                // We reached the end of a sub block reading LZW data. We'll allow at
                // most one more sub block of data with a length of 1 byte.
                b.fill();
                if (b.err == io.EOF)
                {
                    return error.As(null);
                }
                else if (b.err != null)
                {
                    return error.As(b.err);
                }
                else if (b.j > 1L)
                {
                    return error.As(errTooMuch);
                }
            } 

            // Part of a sub-block remains buffered. We expect that the next attempt to
            // buffer a sub-block will reach the block terminator.
            b.fill();
            if (b.err == io.EOF)
            {
                return error.As(null);
            }
            else if (b.err != null)
            {
                return error.As(b.err);
            }
            return error.As(errTooMuch);
        }

        // decode reads a GIF image from r and stores the result in d.
        private static error decode(this ref decoder d, io.Reader r, bool configOnly, bool keepAllFrames)
        { 
            // Add buffering if r does not provide ReadByte.
            {
                reader (rr, ok) = r._<reader>();

                if (ok)
                {
                    d.r = rr;
                }
                else
                {
                    d.r = bufio.NewReader(r);
                }

            }

            var err = d.readHeaderAndScreenDescriptor();
            if (err != null)
            {
                return error.As(err);
            }
            if (configOnly)
            {
                return error.As(null);
            }
            while (true)
            {
                var (c, err) = readByte(d.r);
                if (err != null)
                {
                    return error.As(fmt.Errorf("gif: reading frames: %v", err));
                }

                if (c == sExtension) 
                    err = d.readExtension();

                    if (err != null)
                    {
                        return error.As(err);
                    }
                else if (c == sImageDescriptor) 
                    err = d.readImageDescriptor(keepAllFrames);

                    if (err != null)
                    {
                        return error.As(err);
                    }
                else if (c == sTrailer) 
                    if (len(d.image) == 0L)
                    {
                        return error.As(fmt.Errorf("gif: missing image data"));
                    }
                    return error.As(null);
                else 
                    return error.As(fmt.Errorf("gif: unknown block type: 0x%.2x", c));
                            }

        }

        private static error readHeaderAndScreenDescriptor(this ref decoder d)
        {
            var err = readFull(d.r, d.tmp[..13L]);
            if (err != null)
            {
                return error.As(fmt.Errorf("gif: reading header: %v", err));
            }
            d.vers = string(d.tmp[..6L]);
            if (d.vers != "GIF87a" && d.vers != "GIF89a")
            {
                return error.As(fmt.Errorf("gif: can't recognize format %q", d.vers));
            }
            d.width = int(d.tmp[6L]) + int(d.tmp[7L]) << (int)(8L);
            d.height = int(d.tmp[8L]) + int(d.tmp[9L]) << (int)(8L);
            {
                var fields = d.tmp[10L];

                if (fields & fColorTable != 0L)
                {
                    d.backgroundIndex = d.tmp[11L]; 
                    // readColorTable overwrites the contents of d.tmp, but that's OK.
                    d.globalColorTable, err = d.readColorTable(fields);

                    if (err != null)
                    {
                        return error.As(err);
                    }
                } 
                // d.tmp[12] is the Pixel Aspect Ratio, which is ignored.

            } 
            // d.tmp[12] is the Pixel Aspect Ratio, which is ignored.
            return error.As(null);
        }

        private static (color.Palette, error) readColorTable(this ref decoder d, byte fields)
        {
            long n = 1L << (int)((1L + uint(fields & fColorTableBitsMask)));
            var err = readFull(d.r, d.tmp[..3L * n]);
            if (err != null)
            {
                return (null, fmt.Errorf("gif: reading color table: %s", err));
            }
            long j = 0L;
            var p = make(color.Palette, n);
            foreach (var (i) in p)
            {
                p[i] = new color.RGBA(d.tmp[j+0],d.tmp[j+1],d.tmp[j+2],0xFF);
                j += 3L;
            }
            return (p, null);
        }

        private static error readExtension(this ref decoder d)
        {
            var (extension, err) = readByte(d.r);
            if (err != null)
            {
                return error.As(fmt.Errorf("gif: reading extension: %v", err));
            }
            long size = 0L;

            if (extension == eText) 
                size = 13L;
            else if (extension == eGraphicControl) 
                return error.As(d.readGraphicControl());
            else if (extension == eComment)             else if (extension == eApplication) 
                var (b, err) = readByte(d.r);
                if (err != null)
                {
                    return error.As(fmt.Errorf("gif: reading extension: %v", err));
                } 
                // The spec requires size be 11, but Adobe sometimes uses 10.
                size = int(b);
            else 
                return error.As(fmt.Errorf("gif: unknown extension 0x%.2x", extension));
                        if (size > 0L)
            {
                {
                    var err = readFull(d.r, d.tmp[..size]);

                    if (err != null)
                    {
                        return error.As(fmt.Errorf("gif: reading extension: %v", err));
                    }

                }
            } 

            // Application Extension with "NETSCAPE2.0" as string and 1 in data means
            // this extension defines a loop count.
            if (extension == eApplication && string(d.tmp[..size]) == "NETSCAPE2.0")
            {
                var (n, err) = d.readBlock();
                if (err != null)
                {
                    return error.As(fmt.Errorf("gif: reading extension: %v", err));
                }
                if (n == 0L)
                {
                    return error.As(null);
                }
                if (n == 3L && d.tmp[0L] == 1L)
                {
                    d.loopCount = int(d.tmp[1L]) | int(d.tmp[2L]) << (int)(8L);
                }
            }
            while (true)
            {
                (n, err) = d.readBlock();
                if (err != null)
                {
                    return error.As(fmt.Errorf("gif: reading extension: %v", err));
                }
                if (n == 0L)
                {
                    return error.As(null);
                }
            }

        }

        private static error readGraphicControl(this ref decoder d)
        {
            {
                var err = readFull(d.r, d.tmp[..6L]);

                if (err != null)
                {
                    return error.As(fmt.Errorf("gif: can't read graphic control: %s", err));
                }

            }
            if (d.tmp[0L] != 4L)
            {
                return error.As(fmt.Errorf("gif: invalid graphic control extension block size: %d", d.tmp[0L]));
            }
            var flags = d.tmp[1L];
            d.disposalMethod = (flags & gcDisposalMethodMask) >> (int)(2L);
            d.delayTime = int(d.tmp[2L]) | int(d.tmp[3L]) << (int)(8L);
            if (flags & gcTransparentColorSet != 0L)
            {
                d.transparentIndex = d.tmp[4L];
                d.hasTransparentIndex = true;
            }
            if (d.tmp[5L] != 0L)
            {
                return error.As(fmt.Errorf("gif: invalid graphic control extension block terminator: %d", d.tmp[5L]));
            }
            return error.As(null);
        }

        private static error readImageDescriptor(this ref decoder _d, bool keepAllFrames) => func(_d, (ref decoder d, Defer defer, Panic _, Recover __) =>
        {
            var (m, err) = d.newImageFromDescriptor();
            if (err != null)
            {
                return error.As(err);
            }
            var useLocalColorTable = d.imageFields & fColorTable != 0L;
            if (useLocalColorTable)
            {
                m.Palette, err = d.readColorTable(d.imageFields);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            else
            {
                if (d.globalColorTable == null)
                {
                    return error.As(errors.New("gif: no color table"));
                }
                m.Palette = d.globalColorTable;
            }
            if (d.hasTransparentIndex)
            {
                if (!useLocalColorTable)
                { 
                    // Clone the global color table.
                    m.Palette = append(color.Palette(null), d.globalColorTable);
                }
                {
                    var ti = int(d.transparentIndex);

                    if (ti < len(m.Palette))
                    {
                        m.Palette[ti] = new color.RGBA();
                    }
                    else
                    { 
                        // The transparentIndex is out of range, which is an error
                        // according to the spec, but Firefox and Google Chrome
                        // seem OK with this, so we enlarge the palette with
                        // transparent colors. See golang.org/issue/15059.
                        var p = make(color.Palette, ti + 1L);
                        copy(p, m.Palette);
                        for (var i = len(m.Palette); i < len(p); i++)
                        {
                            p[i] = new color.RGBA();
                        }

                        m.Palette = p;
                    }

                }
            }
            var (litWidth, err) = readByte(d.r);
            if (err != null)
            {
                return error.As(fmt.Errorf("gif: reading image data: %v", err));
            }
            if (litWidth < 2L || litWidth > 8L)
            {
                return error.As(fmt.Errorf("gif: pixel size in decode out of range: %d", litWidth));
            } 
            // A wonderfully Go-like piece of magic.
            blockReader br = ref new blockReader(d:d);
            var lzwr = lzw.NewReader(br, lzw.LSB, int(litWidth));
            defer(lzwr.Close());
            err = readFull(lzwr, m.Pix);

            if (err != null)
            {
                if (err != io.ErrUnexpectedEOF)
                {
                    return error.As(fmt.Errorf("gif: reading image data: %v", err));
                }
                return error.As(errNotEnough);
            } 
            // In theory, both lzwr and br should be exhausted. Reading from them
            // should yield (0, io.EOF).
            //
            // The spec (Appendix F - Compression), says that "An End of
            // Information code... must be the last code output by the encoder
            // for an image". In practice, though, giflib (a widely used C
            // library) does not enforce this, so we also accept lzwr returning
            // io.ErrUnexpectedEOF (meaning that the encoded stream hit io.EOF
            // before the LZW decoder saw an explicit end code), provided that
            // the io.ReadFull call above successfully read len(m.Pix) bytes.
            // See https://golang.org/issue/9856 for an example GIF.
            {
                var (n, err) = lzwr.Read(d.tmp[256L..257L]);

                if (n != 0L || (err != io.EOF && err != io.ErrUnexpectedEOF))
                {
                    if (err != null)
                    {
                        return error.As(fmt.Errorf("gif: reading image data: %v", err));
                    }
                    return error.As(errTooMuch);
                } 

                // In practice, some GIFs have an extra byte in the data sub-block
                // stream, which we ignore. See https://golang.org/issue/16146.

            } 

            // In practice, some GIFs have an extra byte in the data sub-block
            // stream, which we ignore. See https://golang.org/issue/16146.
            {
                var err = br.close();

                if (err == errTooMuch)
                {
                    return error.As(errTooMuch);
                }
                else if (err != null)
                {
                    return error.As(fmt.Errorf("gif: reading image data: %v", err));
                } 

                // Check that the color indexes are inside the palette.

            } 

            // Check that the color indexes are inside the palette.
            if (len(m.Palette) < 256L)
            {
                foreach (var (_, pixel) in m.Pix)
                {
                    if (int(pixel) >= len(m.Palette))
                    {
                        return error.As(errBadPixel);
                    }
                }
            } 

            // Undo the interlacing if necessary.
            if (d.imageFields & fInterlace != 0L)
            {
                uninterlace(m);
            }
            if (keepAllFrames || len(d.image) == 0L)
            {
                d.image = append(d.image, m);
                d.delay = append(d.delay, d.delayTime);
                d.disposal = append(d.disposal, d.disposalMethod);
            } 
            // The GIF89a spec, Section 23 (Graphic Control Extension) says:
            // "The scope of this extension is the first graphic rendering block
            // to follow." We therefore reset the GCE fields to zero.
            d.delayTime = 0L;
            d.hasTransparentIndex = false;
            return error.As(null);
        });

        private static (ref image.Paletted, error) newImageFromDescriptor(this ref decoder d)
        {
            {
                var err = readFull(d.r, d.tmp[..9L]);

                if (err != null)
                {
                    return (null, fmt.Errorf("gif: can't read image descriptor: %s", err));
                }

            }
            var left = int(d.tmp[0L]) + int(d.tmp[1L]) << (int)(8L);
            var top = int(d.tmp[2L]) + int(d.tmp[3L]) << (int)(8L);
            var width = int(d.tmp[4L]) + int(d.tmp[5L]) << (int)(8L);
            var height = int(d.tmp[6L]) + int(d.tmp[7L]) << (int)(8L);
            d.imageFields = d.tmp[8L]; 

            // The GIF89a spec, Section 20 (Image Descriptor) says: "Each image must
            // fit within the boundaries of the Logical Screen, as defined in the
            // Logical Screen Descriptor."
            //
            // This is conceptually similar to testing
            //    frameBounds := image.Rect(left, top, left+width, top+height)
            //    imageBounds := image.Rect(0, 0, d.width, d.height)
            //    if !frameBounds.In(imageBounds) { etc }
            // but the semantics of the Go image.Rectangle type is that r.In(s) is true
            // whenever r is an empty rectangle, even if r.Min.X > s.Max.X. Here, we
            // want something stricter.
            //
            // Note that, by construction, left >= 0 && top >= 0, so we only have to
            // explicitly compare frameBounds.Max (left+width, top+height) against
            // imageBounds.Max (d.width, d.height) and not frameBounds.Min (left, top)
            // against imageBounds.Min (0, 0).
            if (left + width > d.width || top + height > d.height)
            {
                return (null, errors.New("gif: frame bounds larger than image bounds"));
            }
            return (image.NewPaletted(new image.Rectangle(Min:image.Point{left,top},Max:image.Point{left+width,top+height},), null), null);
        }

        private static (long, error) readBlock(this ref decoder d)
        {
            var (n, err) = readByte(d.r);
            if (n == 0L || err != null)
            {
                return (0L, err);
            }
            {
                var err = readFull(d.r, d.tmp[..n]);

                if (err != null)
                {
                    return (0L, err);
                }

            }
            return (int(n), null);
        }

        // interlaceScan defines the ordering for a pass of the interlace algorithm.
        private partial struct interlaceScan
        {
            public long skip;
            public long start;
        }

        // interlacing represents the set of scans in an interlaced GIF image.
        private static interlaceScan interlacing = new slice<interlaceScan>(new interlaceScan[] { {8,0}, {8,4}, {4,2}, {2,1} });

        // uninterlace rearranges the pixels in m to account for interlaced input.
        private static void uninterlace(ref image.Paletted m)
        {
            slice<byte> nPix = default;
            var dx = m.Bounds().Dx();
            var dy = m.Bounds().Dy();
            nPix = make_slice<byte>(dx * dy);
            long offset = 0L; // steps through the input by sequential scan lines.
            foreach (var (_, pass) in interlacing)
            {
                var nOffset = pass.start * dx; // steps through the output as defined by pass.
                {
                    var y = pass.start;

                    while (y < dy)
                    {
                        copy(nPix[nOffset..nOffset + dx], m.Pix[offset..offset + dx]);
                        offset += dx;
                        nOffset += dx * pass.skip;
                        y += pass.skip;
                    }

                }
            }
            m.Pix = nPix;
        }

        // Decode reads a GIF image from r and returns the first embedded
        // image as an image.Image.
        public static (image.Image, error) Decode(io.Reader r)
        {
            decoder d = default;
            {
                var err = d.decode(r, false, false);

                if (err != null)
                {
                    return (null, err);
                }

            }
            return (d.image[0L], null);
        }

        // GIF represents the possibly multiple images stored in a GIF file.
        public partial struct GIF
        {
            public slice<ref image.Paletted> Image; // The successive images.
            public slice<long> Delay; // The successive delay times, one per frame, in 100ths of a second.
            public long LoopCount; // The loop count.
// Disposal is the successive disposal methods, one per frame. For
// backwards compatibility, a nil Disposal is valid to pass to EncodeAll,
// and implies that each frame's disposal method is 0 (no disposal
// specified).
            public slice<byte> Disposal; // Config is the global color table (palette), width and height. A nil or
// empty-color.Palette Config.ColorModel means that each frame has its own
// color table and there is no global color table. Each frame's bounds must
// be within the rectangle defined by the two points (0, 0) and
// (Config.Width, Config.Height).
//
// For backwards compatibility, a zero-valued Config is valid to pass to
// EncodeAll, and implies that the overall GIF's width and height equals
// the first frame's bounds' Rectangle.Max point.
            public image.Config Config; // BackgroundIndex is the background index in the global color table, for
// use with the DisposalBackground disposal method.
            public byte BackgroundIndex;
        }

        // DecodeAll reads a GIF image from r and returns the sequential frames
        // and timing information.
        public static (ref GIF, error) DecodeAll(io.Reader r)
        {
            decoder d = default;
            {
                var err = d.decode(r, false, true);

                if (err != null)
                {
                    return (null, err);
                }

            }
            GIF gif = ref new GIF(Image:d.image,LoopCount:d.loopCount,Delay:d.delay,Disposal:d.disposal,Config:image.Config{ColorModel:d.globalColorTable,Width:d.width,Height:d.height,},BackgroundIndex:d.backgroundIndex,);
            return (gif, null);
        }

        // DecodeConfig returns the global color model and dimensions of a GIF image
        // without decoding the entire image.
        public static (image.Config, error) DecodeConfig(io.Reader r)
        {
            decoder d = default;
            {
                var err = d.decode(r, true, false);

                if (err != null)
                {
                    return (new image.Config(), err);
                }

            }
            return (new image.Config(ColorModel:d.globalColorTable,Width:d.width,Height:d.height,), null);
        }

        private static void init()
        {
            image.RegisterFormat("gif", "GIF8?a", Decode, DecodeConfig);
        }
    }
}}
