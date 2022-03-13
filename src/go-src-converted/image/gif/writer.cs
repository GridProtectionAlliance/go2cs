// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gif -- go2cs converted at 2022 March 13 06:44:06 UTC
// import "image/gif" ==> using gif = go.image.gif_package
// Original source: C:\Program Files\Go\src\image\gif\writer.go
namespace go.image;

using bufio = bufio_package;
using bytes = bytes_package;
using lzw = compress.lzw_package;
using errors = errors_package;
using image = image_package;
using color = image.color_package;
using palette = image.color.palette_package;
using draw = image.draw_package;
using io = io_package;


// Graphic control extension fields.

public static partial class gif_package {

private static readonly nuint gcLabel = 0xF9;
private static readonly nuint gcBlockSize = 0x04;

private static array<nint> log2Lookup = new array<nint>(new nint[] { 2, 4, 8, 16, 32, 64, 128, 256 });

private static nint log2(nint x) {
    foreach (var (i, v) in log2Lookup) {
        if (x <= v) {
            return i;
        }
    }    return -1;
}

// Little-endian.
private static void writeUint16(slice<byte> b, ushort u) {
    b[0] = uint8(u);
    b[1] = uint8(u >> 8);
}

// writer is a buffered writer.
private partial interface writer {
    error Flush();
}

// encoder encodes an image to the GIF format.
private partial struct encoder {
    public writer w;
    public error err; // g is a reference to the data that is being encoded.
    public GIF g; // globalCT is the size in bytes of the global color table.
    public nint globalCT; // buf is a scratch buffer. It must be at least 256 for the blockWriter.
    public array<byte> buf;
    public array<byte> globalColorTable;
    public array<byte> localColorTable;
}

// blockWriter writes the block structure of GIF image data, which
// comprises (n, (n bytes)) blocks, with 1 <= n <= 255. It is the
// writer given to the LZW encoder, which is thus immune to the
// blocking.
private partial struct blockWriter {
    public ptr<encoder> e;
}

private static void setup(this blockWriter b) {
    b.e.buf[0] = 0;
}

private static error Flush(this blockWriter b) {
    return error.As(b.e.err)!;
}

private static error WriteByte(this blockWriter b, byte c) {
    if (b.e.err != null) {
        return error.As(b.e.err)!;
    }
    b.e.buf[0]++;
    b.e.buf[b.e.buf[0]] = c;
    if (b.e.buf[0] < 255) {
        return error.As(null!)!;
    }
    b.e.write(b.e.buf[..(int)256]);
    b.e.buf[0] = 0;
    return error.As(b.e.err)!;
}

// blockWriter must be an io.Writer for lzw.NewWriter, but this is never
// actually called.
private static (nint, error) Write(this blockWriter b, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;

    foreach (var (i, c) in data) {
        {
            var err = b.WriteByte(c);

            if (err != null) {
                return (i, error.As(err)!);
            }

        }
    }    return (len(data), error.As(null!)!);
}

private static void close(this blockWriter b) { 
    // Write the block terminator (0x00), either by itself, or along with a
    // pending sub-block.
    if (b.e.buf[0] == 0) {
        b.e.writeByte(0);
    }
    else
 {
        var n = uint(b.e.buf[0]);
        b.e.buf[n + 1] = 0;
        b.e.write(b.e.buf[..(int)n + 2]);
    }
    b.e.flush();
}

private static void flush(this ptr<encoder> _addr_e) {
    ref encoder e = ref _addr_e.val;

    if (e.err != null) {
        return ;
    }
    e.err = e.w.Flush();
}

private static void write(this ptr<encoder> _addr_e, slice<byte> p) {
    ref encoder e = ref _addr_e.val;

    if (e.err != null) {
        return ;
    }
    _, e.err = e.w.Write(p);
}

private static void writeByte(this ptr<encoder> _addr_e, byte b) {
    ref encoder e = ref _addr_e.val;

    if (e.err != null) {
        return ;
    }
    e.err = e.w.WriteByte(b);
}

private static void writeHeader(this ptr<encoder> _addr_e) {
    ref encoder e = ref _addr_e.val;

    if (e.err != null) {
        return ;
    }
    _, e.err = io.WriteString(e.w, "GIF89a");
    if (e.err != null) {
        return ;
    }
    writeUint16(e.buf[(int)0..(int)2], uint16(e.g.Config.Width));
    writeUint16(e.buf[(int)2..(int)4], uint16(e.g.Config.Height));
    e.write(e.buf[..(int)4]);

    {
        color.Palette (p, ok) = e.g.Config.ColorModel._<color.Palette>();

        if (ok && len(p) > 0) {
            var paddedSize = log2(len(p)); // Size of Global Color Table: 2^(1+n).
            e.buf[0] = fColorTable | uint8(paddedSize);
            e.buf[1] = e.g.BackgroundIndex;
            e.buf[2] = 0x00; // Pixel Aspect Ratio.
            e.write(e.buf[..(int)3]);
            error err = default!;
            e.globalCT, err = encodeColorTable(e.globalColorTable[..], p, paddedSize);
            if (err != null && e.err == null) {
                e.err = err;
                return ;
            }
            e.write(e.globalColorTable[..(int)e.globalCT]);
        }
        else
 { 
            // All frames have a local color table, so a global color table
            // is not needed.
            e.buf[0] = 0x00;
            e.buf[1] = 0x00; // Background Color Index.
            e.buf[2] = 0x00; // Pixel Aspect Ratio.
            e.write(e.buf[..(int)3]);
        }
    } 

    // Add animation info if necessary.
    if (len(e.g.Image) > 1 && e.g.LoopCount >= 0) {
        e.buf[0] = 0x21; // Extension Introducer.
        e.buf[1] = 0xff; // Application Label.
        e.buf[2] = 0x0b; // Block Size.
        e.write(e.buf[..(int)3]);
        var (_, err) = io.WriteString(e.w, "NETSCAPE2.0"); // Application Identifier.
        if (err != null && e.err == null) {
            e.err = err;
            return ;
        }
        e.buf[0] = 0x03; // Block Size.
        e.buf[1] = 0x01; // Sub-block Index.
        writeUint16(e.buf[(int)2..(int)4], uint16(e.g.LoopCount));
        e.buf[4] = 0x00; // Block Terminator.
        e.write(e.buf[..(int)5]);
    }
}

private static (nint, error) encodeColorTable(slice<byte> dst, color.Palette p, nint size) {
    nint _p0 = default;
    error _p0 = default!;

    if (uint(size) >= uint(len(log2Lookup))) {
        return (0, error.As(errors.New("gif: cannot encode color table with more than 256 entries"))!);
    }
    {
        var i__prev1 = i;

        foreach (var (__i, __c) in p) {
            i = __i;
            c = __c;
            if (c == null) {
                return (0, error.As(errors.New("gif: cannot encode color table with nil entries"))!);
            }
            byte r = default;            byte g = default;            byte b = default; 
            // It is most likely that the palette is full of color.RGBAs, so they
            // get a fast path.
 
            // It is most likely that the palette is full of color.RGBAs, so they
            // get a fast path.
            {
                color.RGBA (rgba, ok) = c._<color.RGBA>();

                if (ok) {
                    (r, g, b) = (rgba.R, rgba.G, rgba.B);
                }
                else
 {
                    var (rr, gg, bb, _) = c.RGBA();
                    (r, g, b) = (uint8(rr >> 8), uint8(gg >> 8), uint8(bb >> 8));
                }

            }
            dst[3 * i + 0] = r;
            dst[3 * i + 1] = g;
            dst[3 * i + 2] = b;
        }
        i = i__prev1;
    }

    var n = log2Lookup[size];
    if (n > len(p)) { 
        // Pad with black.
        var fill = dst[(int)3 * len(p)..(int)3 * n];
        {
            var i__prev1 = i;

            foreach (var (__i) in fill) {
                i = __i;
                fill[i] = 0;
            }

            i = i__prev1;
        }
    }
    return (3 * n, error.As(null!)!);
}

private static bool colorTablesMatch(this ptr<encoder> _addr_e, nint localLen, nint transparentIndex) {
    ref encoder e = ref _addr_e.val;

    nint localSize = 3 * localLen;
    if (transparentIndex >= 0) {
        nint trOff = 3 * transparentIndex;
        return bytes.Equal(e.globalColorTable[..(int)trOff], e.localColorTable[..(int)trOff]) && bytes.Equal(e.globalColorTable[(int)trOff + 3..(int)localSize], e.localColorTable[(int)trOff + 3..(int)localSize]);
    }
    return bytes.Equal(e.globalColorTable[..(int)localSize], e.localColorTable[..(int)localSize]);
}

private static void writeImageBlock(this ptr<encoder> _addr_e, ptr<image.Paletted> _addr_pm, nint delay, byte disposal) {
    ref encoder e = ref _addr_e.val;
    ref image.Paletted pm = ref _addr_pm.val;

    if (e.err != null) {
        return ;
    }
    if (len(pm.Palette) == 0) {
        e.err = errors.New("gif: cannot encode image block with empty palette");
        return ;
    }
    var b = pm.Bounds();
    if (b.Min.X < 0 || b.Max.X >= 1 << 16 || b.Min.Y < 0 || b.Max.Y >= 1 << 16) {
        e.err = errors.New("gif: image block is too large to encode");
        return ;
    }
    if (!b.In(new image.Rectangle(Max:image.Point{e.g.Config.Width,e.g.Config.Height}))) {
        e.err = errors.New("gif: image block is out of bounds");
        return ;
    }
    nint transparentIndex = -1;
    {
        var i__prev1 = i;

        foreach (var (__i, __c) in pm.Palette) {
            i = __i;
            c = __c;
            if (c == null) {
                e.err = errors.New("gif: cannot encode color table with nil entries");
                return ;
            }
            {
                var (_, _, _, a) = c.RGBA();

                if (a == 0) {
                    transparentIndex = i;
                    break;
                }

            }
        }
        i = i__prev1;
    }

    if (delay > 0 || disposal != 0 || transparentIndex != -1) {
        e.buf[0] = sExtension; // Extension Introducer.
        e.buf[1] = gcLabel; // Graphic Control Label.
        e.buf[2] = gcBlockSize; // Block Size.
        if (transparentIndex != -1) {
            e.buf[3] = 0x01 | disposal << 2;
        }
        else
 {
            e.buf[3] = 0x00 | disposal << 2;
        }
        writeUint16(e.buf[(int)4..(int)6], uint16(delay)); // Delay Time (1/100ths of a second)

        // Transparent color index.
        if (transparentIndex != -1) {
            e.buf[6] = uint8(transparentIndex);
        }
        else
 {
            e.buf[6] = 0x00;
        }
        e.buf[7] = 0x00; // Block Terminator.
        e.write(e.buf[..(int)8]);
    }
    e.buf[0] = sImageDescriptor;
    writeUint16(e.buf[(int)1..(int)3], uint16(b.Min.X));
    writeUint16(e.buf[(int)3..(int)5], uint16(b.Min.Y));
    writeUint16(e.buf[(int)5..(int)7], uint16(b.Dx()));
    writeUint16(e.buf[(int)7..(int)9], uint16(b.Dy()));
    e.write(e.buf[..(int)9]); 

    // To determine whether or not this frame's palette is the same as the
    // global palette, we can check a couple things. First, do they actually
    // point to the same []color.Color? If so, they are equal so long as the
    // frame's palette is not longer than the global palette...
    var paddedSize = log2(len(pm.Palette)); // Size of Local Color Table: 2^(1+n).
    {
        color.Palette (gp, ok) = e.g.Config.ColorModel._<color.Palette>();

        if (ok && len(pm.Palette) <= len(gp) && _addr_gp[0] == _addr_pm.Palette[0]) {
            e.writeByte(0); // Use the global color table.
        }
        else
 {
            var (ct, err) = encodeColorTable(e.localColorTable[..], pm.Palette, paddedSize);
            if (err != null) {
                if (e.err == null) {
                    e.err = err;
                }
                return ;
            } 
            // This frame's palette is not the very same slice as the global
            // palette, but it might be a copy, possibly with one value turned into
            // transparency by DecodeAll.
            if (ct <= e.globalCT && e.colorTablesMatch(len(pm.Palette), transparentIndex)) {
                e.writeByte(0); // Use the global color table.
            }
            else
 { 
                // Use a local color table.
                e.writeByte(fColorTable | uint8(paddedSize));
                e.write(e.localColorTable[..(int)ct]);
            }
        }
    }

    var litWidth = paddedSize + 1;
    if (litWidth < 2) {
        litWidth = 2;
    }
    e.writeByte(uint8(litWidth)); // LZW Minimum Code Size.

    blockWriter bw = new blockWriter(e:e);
    bw.setup();
    var lzww = lzw.NewWriter(bw, lzw.LSB, litWidth);
    {
        var dx = b.Dx();

        if (dx == pm.Stride) {
            _, e.err = lzww.Write(pm.Pix[..(int)dx * b.Dy()]);
            if (e.err != null) {
                lzww.Close();
                return ;
            }
        }
        else
 {
            {
                var i__prev1 = i;

                nint i = 0;
                var y = b.Min.Y;

                while (y < b.Max.Y) {
                    _, e.err = lzww.Write(pm.Pix[(int)i..(int)i + dx]);
                    if (e.err != null) {
                        lzww.Close();
                        return ;
                    (i, y) = (i + pm.Stride, y + 1);
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
public partial struct Options {
    public nint NumColors; // Quantizer is used to produce a palette with size NumColors.
// palette.Plan9 is used in place of a nil Quantizer.
    public draw.Quantizer Quantizer; // Drawer is used to convert the source image to the desired palette.
// draw.FloydSteinberg is used in place of a nil Drawer.
    public draw.Drawer Drawer;
}

// EncodeAll writes the images in g to w in GIF format with the
// given loop count and delay between frames.
public static error EncodeAll(io.Writer w, ptr<GIF> _addr_g) {
    ref GIF g = ref _addr_g.val;

    if (len(g.Image) == 0) {
        return error.As(errors.New("gif: must provide at least one image"))!;
    }
    if (len(g.Image) != len(g.Delay)) {
        return error.As(errors.New("gif: mismatched image and delay lengths"))!;
    }
    encoder e = new encoder(g:*g); 
    // The GIF.Disposal, GIF.Config and GIF.BackgroundIndex fields were added
    // in Go 1.5. Valid Go 1.4 code, such as when the Disposal field is omitted
    // in a GIF struct literal, should still produce valid GIFs.
    if (e.g.Disposal != null && len(e.g.Image) != len(e.g.Disposal)) {
        return error.As(errors.New("gif: mismatched image and disposal lengths"))!;
    }
    if (e.g.Config == (new image.Config())) {
        var p = g.Image[0].Bounds().Max;
        e.g.Config.Width = p.X;
        e.g.Config.Height = p.Y;
    }
    else if (e.g.Config.ColorModel != null) {
        {
            color.Palette (_, ok) = e.g.Config.ColorModel._<color.Palette>();

            if (!ok) {
                return error.As(errors.New("gif: GIF color model must be a color.Palette"))!;
            }

        }
    }
    {
        writer (ww, ok) = writer.As(w._<writer>())!;

        if (ok) {
            e.w = ww;
        }
        else
 {
            e.w = bufio.NewWriter(w);
        }
    }

    e.writeHeader();
    foreach (var (i, pm) in g.Image) {
        var disposal = uint8(0);
        if (g.Disposal != null) {
            disposal = g.Disposal[i];
        }
        e.writeImageBlock(pm, g.Delay[i], disposal);
    }    e.writeByte(sTrailer);
    e.flush();
    return error.As(e.err)!;
}

// Encode writes the Image m to w in GIF format.
public static error Encode(io.Writer w, image.Image m, ptr<Options> _addr_o) {
    ref Options o = ref _addr_o.val;
 
    // Check for bounds and size restrictions.
    var b = m.Bounds();
    if (b.Dx() >= 1 << 16 || b.Dy() >= 1 << 16) {
        return error.As(errors.New("gif: image is too large to encode"))!;
    }
    Options opts = new Options();
    if (o != null) {
        opts = o;
    }
    if (opts.NumColors < 1 || 256 < opts.NumColors) {
        opts.NumColors = 256;
    }
    if (opts.Drawer == null) {
        opts.Drawer = draw.FloydSteinberg;
    }
    ptr<image.Paletted> (pm, _) = m._<ptr<image.Paletted>>();
    if (pm == null) {
        {
            color.Palette (cp, ok) = m.ColorModel()._<color.Palette>();

            if (ok) {
                pm = image.NewPaletted(b, cp);
                for (var y = b.Min.Y; y < b.Max.Y; y++) {
                    for (var x = b.Min.X; x < b.Max.X; x++) {
                        pm.Set(x, y, cp.Convert(m.At(x, y)));
                    }
                }
            }

        }
    }
    if (pm == null || len(pm.Palette) > opts.NumColors) { 
        // Set pm to be a palettedized copy of m, including its bounds, which
        // might not start at (0, 0).
        //
        // TODO: Pick a better sub-sample of the Plan 9 palette.
        pm = image.NewPaletted(b, palette.Plan9[..(int)opts.NumColors]);
        if (opts.Quantizer != null) {
            pm.Palette = opts.Quantizer.Quantize(make(color.Palette, 0, opts.NumColors), m);
        }
        opts.Drawer.Draw(pm, b, m, b.Min);
    }
    if (pm.Rect.Min != (new image.Point())) {
        ref var dup = ref heap(pm.val, out ptr<var> _addr_dup);
        dup.Rect = dup.Rect.Sub(dup.Rect.Min);
        _addr_pm = _addr_dup;
        pm = ref _addr_pm.val;
    }
    return error.As(EncodeAll(w, addr(new GIF(Image:[]*image.Paletted{pm},Delay:[]int{0},Config:image.Config{ColorModel:pm.Palette,Width:b.Dx(),Height:b.Dy(),},))))!;
}

} // end gif_package
