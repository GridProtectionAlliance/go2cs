// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gif -- go2cs converted at 2020 August 29 10:10:08 UTC
// import "image/gif" ==> using gif = go.image.gif_package
// Original source: C:\Go\src\image\gif\writer.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using lzw = go.compress.lzw_package;
using errors = go.errors_package;
using image = go.image_package;
using color = go.image.color_package;
using palette = go.image.color.palette_package;
using draw = go.image.draw_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace image
{
    public static partial class gif_package
    {
        // Graphic control extension fields.
        private static readonly ulong gcLabel = 0xF9UL;
        private static readonly ulong gcBlockSize = 0x04UL;

        private static array<long> log2Lookup = new array<long>(new long[] { 2, 4, 8, 16, 32, 64, 128, 256 });

        private static long log2(long x)
        {
            foreach (var (i, v) in log2Lookup)
            {
                if (x <= v)
                {
                    return i;
                }
            }
            return -1L;
        }

        // Little-endian.
        private static void writeUint16(slice<byte> b, ushort u)
        {
            b[0L] = uint8(u);
            b[1L] = uint8(u >> (int)(8L));
        }

        // writer is a buffered writer.
        private partial interface writer : io.Writer, io.ByteWriter
        {
            error Flush();
        }

        // encoder encodes an image to the GIF format.
        private partial struct encoder
        {
            public writer w;
            public error err; // g is a reference to the data that is being encoded.
            public GIF g; // globalCT is the size in bytes of the global color table.
            public long globalCT; // buf is a scratch buffer. It must be at least 256 for the blockWriter.
            public array<byte> buf;
            public array<byte> globalColorTable;
            public array<byte> localColorTable;
        }

        // blockWriter writes the block structure of GIF image data, which
        // comprises (n, (n bytes)) blocks, with 1 <= n <= 255. It is the
        // writer given to the LZW encoder, which is thus immune to the
        // blocking.
        private partial struct blockWriter
        {
            public ptr<encoder> e;
        }

        private static void setup(this blockWriter b)
        {
            b.e.buf[0L] = 0L;
        }

        private static error Flush(this blockWriter b)
        {
            return error.As(b.e.err);
        }

        private static error WriteByte(this blockWriter b, byte c)
        {
            if (b.e.err != null)
            {
                return error.As(b.e.err);
            } 

            // Append c to buffered sub-block.
            b.e.buf[0L]++;
            b.e.buf[b.e.buf[0L]] = c;
            if (b.e.buf[0L] < 255L)
            {
                return error.As(null);
            } 

            // Flush block
            b.e.write(b.e.buf[..256L]);
            b.e.buf[0L] = 0L;
            return error.As(b.e.err);
        }

        // blockWriter must be an io.Writer for lzw.NewWriter, but this is never
        // actually called.
        private static (long, error) Write(this blockWriter b, slice<byte> data)
        {
            foreach (var (i, c) in data)
            {
                {
                    var err = b.WriteByte(c);

                    if (err != null)
                    {
                        return (i, err);
                    }

                }
            }
            return (len(data), null);
        }

        private static void close(this blockWriter b)
        { 
            // Write the block terminator (0x00), either by itself, or along with a
            // pending sub-block.
            if (b.e.buf[0L] == 0L)
            {
                b.e.writeByte(0L);
            }
            else
            {
                var n = uint(b.e.buf[0L]);
                b.e.buf[n + 1L] = 0L;
                b.e.write(b.e.buf[..n + 2L]);
            }
            b.e.flush();
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

        private static void writeHeader(this ref encoder e)
        {
            if (e.err != null)
            {
                return;
            }
            _, e.err = io.WriteString(e.w, "GIF89a");
            if (e.err != null)
            {
                return;
            } 

            // Logical screen width and height.
            writeUint16(e.buf[0L..2L], uint16(e.g.Config.Width));
            writeUint16(e.buf[2L..4L], uint16(e.g.Config.Height));
            e.write(e.buf[..4L]);

            {
                color.Palette (p, ok) = e.g.Config.ColorModel._<color.Palette>();

                if (ok && len(p) > 0L)
                {
                    var paddedSize = log2(len(p)); // Size of Global Color Table: 2^(1+n).
                    e.buf[0L] = fColorTable | uint8(paddedSize);
                    e.buf[1L] = e.g.BackgroundIndex;
                    e.buf[2L] = 0x00UL; // Pixel Aspect Ratio.
                    e.write(e.buf[..3L]);
                    error err = default;
                    e.globalCT, err = encodeColorTable(e.globalColorTable[..], p, paddedSize);
                    if (err != null && e.err == null)
                    {
                        e.err = err;
                        return;
                    }
                    e.write(e.globalColorTable[..e.globalCT]);
                }
                else
                { 
                    // All frames have a local color table, so a global color table
                    // is not needed.
                    e.buf[0L] = 0x00UL;
                    e.buf[1L] = 0x00UL; // Background Color Index.
                    e.buf[2L] = 0x00UL; // Pixel Aspect Ratio.
                    e.write(e.buf[..3L]);
                } 

                // Add animation info if necessary.

            } 

            // Add animation info if necessary.
            if (len(e.g.Image) > 1L)
            {
                e.buf[0L] = 0x21UL; // Extension Introducer.
                e.buf[1L] = 0xffUL; // Application Label.
                e.buf[2L] = 0x0bUL; // Block Size.
                e.write(e.buf[..3L]);
                var (_, err) = io.WriteString(e.w, "NETSCAPE2.0"); // Application Identifier.
                if (err != null && e.err == null)
                {
                    e.err = err;
                    return;
                }
                e.buf[0L] = 0x03UL; // Block Size.
                e.buf[1L] = 0x01UL; // Sub-block Index.
                writeUint16(e.buf[2L..4L], uint16(e.g.LoopCount));
                e.buf[4L] = 0x00UL; // Block Terminator.
                e.write(e.buf[..5L]);
            }
        }

        private static (long, error) encodeColorTable(slice<byte> dst, color.Palette p, long size)
        {
            if (uint(size) >= uint(len(log2Lookup)))
            {
                return (0L, errors.New("gif: cannot encode color table with more than 256 entries"));
            }
            {
                var i__prev1 = i;

                foreach (var (__i, __c) in p)
                {
                    i = __i;
                    c = __c;
                    if (c == null)
                    {
                        return (0L, errors.New("gif: cannot encode color table with nil entries"));
                    }
                    byte r = default;                    byte g = default;                    byte b = default; 
                    // It is most likely that the palette is full of color.RGBAs, so they
                    // get a fast path.
 
                    // It is most likely that the palette is full of color.RGBAs, so they
                    // get a fast path.
                    {
                        color.RGBA (rgba, ok) = c._<color.RGBA>();

                        if (ok)
                        {
                            r = rgba.R;
                            g = rgba.G;
                            b = rgba.B;
                        }
                        else
                        {
                            var (rr, gg, bb, _) = c.RGBA();
                            r = uint8(rr >> (int)(8L));
                            g = uint8(gg >> (int)(8L));
                            b = uint8(bb >> (int)(8L));
                        }

                    }
                    dst[3L * i + 0L] = r;
                    dst[3L * i + 1L] = g;
                    dst[3L * i + 2L] = b;
                }

                i = i__prev1;
            }

            var n = log2Lookup[size];
            if (n > len(p))
            { 
                // Pad with black.
                var fill = dst[3L * len(p)..3L * n];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in fill)
                    {
                        i = __i;
                        fill[i] = 0L;
                    }

                    i = i__prev1;
                }

            }
            return (3L * n, null);
        }

        private static bool colorTablesMatch(this ref encoder e, long localLen, long transparentIndex)
        {
            long localSize = 3L * localLen;
            if (transparentIndex >= 0L)
            {
                long trOff = 3L * transparentIndex;
                return bytes.Equal(e.globalColorTable[..trOff], e.localColorTable[..trOff]) && bytes.Equal(e.globalColorTable[trOff + 3L..localSize], e.localColorTable[trOff + 3L..localSize]);
            }
            return bytes.Equal(e.globalColorTable[..localSize], e.localColorTable[..localSize]);
        }

        private static void writeImageBlock(this ref encoder e, ref image.Paletted pm, long delay, byte disposal)
        {
            if (e.err != null)
            {
                return;
            }
            if (len(pm.Palette) == 0L)
            {
                e.err = errors.New("gif: cannot encode image block with empty palette");
                return;
            }
            var b = pm.Bounds();
            if (b.Min.X < 0L || b.Max.X >= 1L << (int)(16L) || b.Min.Y < 0L || b.Max.Y >= 1L << (int)(16L))
            {
                e.err = errors.New("gif: image block is too large to encode");
                return;
            }
            if (!b.In(new image.Rectangle(Max:image.Point{e.g.Config.Width,e.g.Config.Height})))
            {
                e.err = errors.New("gif: image block is out of bounds");
                return;
            }
            long transparentIndex = -1L;
            {
                var i__prev1 = i;

                foreach (var (__i, __c) in pm.Palette)
                {
                    i = __i;
                    c = __c;
                    if (c == null)
                    {
                        e.err = errors.New("gif: cannot encode color table with nil entries");
                        return;
                    }
                    {
                        var (_, _, _, a) = c.RGBA();

                        if (a == 0L)
                        {
                            transparentIndex = i;
                            break;
                        }

                    }
                }

                i = i__prev1;
            }

            if (delay > 0L || disposal != 0L || transparentIndex != -1L)
            {
                e.buf[0L] = sExtension; // Extension Introducer.
                e.buf[1L] = gcLabel; // Graphic Control Label.
                e.buf[2L] = gcBlockSize; // Block Size.
                if (transparentIndex != -1L)
                {
                    e.buf[3L] = 0x01UL | disposal << (int)(2L);
                }
                else
                {
                    e.buf[3L] = 0x00UL | disposal << (int)(2L);
                }
                writeUint16(e.buf[4L..6L], uint16(delay)); // Delay Time (1/100ths of a second)

                // Transparent color index.
                if (transparentIndex != -1L)
                {
                    e.buf[6L] = uint8(transparentIndex);
                }
                else
                {
                    e.buf[6L] = 0x00UL;
                }
                e.buf[7L] = 0x00UL; // Block Terminator.
                e.write(e.buf[..8L]);
            }
            e.buf[0L] = sImageDescriptor;
            writeUint16(e.buf[1L..3L], uint16(b.Min.X));
            writeUint16(e.buf[3L..5L], uint16(b.Min.Y));
            writeUint16(e.buf[5L..7L], uint16(b.Dx()));
            writeUint16(e.buf[7L..9L], uint16(b.Dy()));
            e.write(e.buf[..9L]); 

            // To determine whether or not this frame's palette is the same as the
            // global palette, we can check a couple things. First, do they actually
            // point to the same []color.Color? If so, they are equal so long as the
            // frame's palette is not longer than the global palette...
            var paddedSize = log2(len(pm.Palette)); // Size of Local Color Table: 2^(1+n).
            {
                color.Palette (gp, ok) = e.g.Config.ColorModel._<color.Palette>();

                if (ok && len(pm.Palette) <= len(gp) && ref gp[0L] == ref pm.Palette[0L])
                {
                    e.writeByte(0L); // Use the global color table.
                }
                else
                {
                    var (ct, err) = encodeColorTable(e.localColorTable[..], pm.Palette, paddedSize);
                    if (err != null)
                    {
                        if (e.err == null)
                        {
                            e.err = err;
                        }
                        return;
                    } 
                    // This frame's palette is not the very same slice as the global
                    // palette, but it might be a copy, possibly with one value turned into
                    // transparency by DecodeAll.
                    if (ct <= e.globalCT && e.colorTablesMatch(len(pm.Palette), transparentIndex))
                    {
                        e.writeByte(0L); // Use the global color table.
                    }
                    else
                    { 
                        // Use a local color table.
                        e.writeByte(fColorTable | uint8(paddedSize));
                        e.write(e.localColorTable[..ct]);
                    }
                }

            }

            var litWidth = paddedSize + 1L;
            if (litWidth < 2L)
            {
                litWidth = 2L;
            }
            e.writeByte(uint8(litWidth)); // LZW Minimum Code Size.

            blockWriter bw = new blockWriter(e:e);
            bw.setup();
            var lzww = lzw.NewWriter(bw, lzw.LSB, litWidth);
            {
                var dx = b.Dx();

                if (dx == pm.Stride)
                {
                    _, e.err = lzww.Write(pm.Pix[..dx * b.Dy()]);
                    if (e.err != null)
                    {
                        lzww.Close();
                        return;
                    }
                }
                else
                {
                    {
                        var i__prev1 = i;

                        long i = 0L;
                        var y = b.Min.Y;

                        while (y < b.Max.Y)
                        {
                            _, e.err = lzww.Write(pm.Pix[i..i + dx]);
                            if (e.err != null)
                            {
                                lzww.Close();
                                return;
                            i = i + pm.Stride;
                        y = y + 1L;
                            }
                        }


                        i = i__prev1;
                    }
                }

            }
            lzww.Close(); // flush to bw
            bw.close(); // flush to e.w
        }

        // Options are the encoding parameters.
        public partial struct Options
        {
            public long NumColors; // Quantizer is used to produce a palette with size NumColors.
// palette.Plan9 is used in place of a nil Quantizer.
            public draw.Quantizer Quantizer; // Drawer is used to convert the source image to the desired palette.
// draw.FloydSteinberg is used in place of a nil Drawer.
            public draw.Drawer Drawer;
        }

        // EncodeAll writes the images in g to w in GIF format with the
        // given loop count and delay between frames.
        public static error EncodeAll(io.Writer w, ref GIF g)
        {
            if (len(g.Image) == 0L)
            {
                return error.As(errors.New("gif: must provide at least one image"));
            }
            if (len(g.Image) != len(g.Delay))
            {
                return error.As(errors.New("gif: mismatched image and delay lengths"));
            }
            if (g.LoopCount < 0L)
            {
                g.LoopCount = 0L;
            }
            encoder e = new encoder(g:*g); 
            // The GIF.Disposal, GIF.Config and GIF.BackgroundIndex fields were added
            // in Go 1.5. Valid Go 1.4 code, such as when the Disposal field is omitted
            // in a GIF struct literal, should still produce valid GIFs.
            if (e.g.Disposal != null && len(e.g.Image) != len(e.g.Disposal))
            {
                return error.As(errors.New("gif: mismatched image and disposal lengths"));
            }
            if (e.g.Config == (new image.Config()))
            {
                var p = g.Image[0L].Bounds().Max;
                e.g.Config.Width = p.X;
                e.g.Config.Height = p.Y;
            }
            else if (e.g.Config.ColorModel != null)
            {
                {
                    color.Palette (_, ok) = e.g.Config.ColorModel._<color.Palette>();

                    if (!ok)
                    {
                        return error.As(errors.New("gif: GIF color model must be a color.Palette"));
                    }

                }
            }
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

            }

            e.writeHeader();
            foreach (var (i, pm) in g.Image)
            {
                var disposal = uint8(0L);
                if (g.Disposal != null)
                {
                    disposal = g.Disposal[i];
                }
                e.writeImageBlock(pm, g.Delay[i], disposal);
            }
            e.writeByte(sTrailer);
            e.flush();
            return error.As(e.err);
        }

        // Encode writes the Image m to w in GIF format.
        public static error Encode(io.Writer w, image.Image m, ref Options o)
        { 
            // Check for bounds and size restrictions.
            var b = m.Bounds();
            if (b.Dx() >= 1L << (int)(16L) || b.Dy() >= 1L << (int)(16L))
            {
                return error.As(errors.New("gif: image is too large to encode"));
            }
            Options opts = new Options();
            if (o != null)
            {
                opts = o.Value;
            }
            if (opts.NumColors < 1L || 256L < opts.NumColors)
            {
                opts.NumColors = 256L;
            }
            if (opts.Drawer == null)
            {
                opts.Drawer = draw.FloydSteinberg;
            }
            ref image.Paletted (pm, ok) = m._<ref image.Paletted>();
            if (!ok || len(pm.Palette) > opts.NumColors)
            { 
                // TODO: Pick a better sub-sample of the Plan 9 palette.
                pm = image.NewPaletted(b, palette.Plan9[..opts.NumColors]);
                if (opts.Quantizer != null)
                {
                    pm.Palette = opts.Quantizer.Quantize(make(color.Palette, 0L, opts.NumColors), m);
                }
                opts.Drawer.Draw(pm, b, m, image.ZP);
            } 

            // When calling Encode instead of EncodeAll, the single-frame image is
            // translated such that its top-left corner is (0, 0), so that the single
            // frame completely fills the overall GIF's bounds.
            if (pm.Rect.Min != (new image.Point()))
            {
                var dup = pm.Value;
                dup.Rect = dup.Rect.Sub(dup.Rect.Min);
                pm = ref dup;
            }
            return error.As(EncodeAll(w, ref new GIF(Image:[]*image.Paletted{pm},Delay:[]int{0},Config:image.Config{ColorModel:pm.Palette,Width:b.Dx(),Height:b.Dy(),},)));
        }
    }
}}
