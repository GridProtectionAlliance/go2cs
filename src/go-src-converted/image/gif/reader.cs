// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gif implements a GIF image decoder and encoder.
//
// The GIF specification is at https://www.w3.org/Graphics/GIF/spec-gif89a.txt.
// package gif -- go2cs converted at 2022 March 06 23:36:03 UTC
// import "image/gif" ==> using gif = go.image.gif_package
// Original source: C:\Program Files\Go\src\image\gif\reader.go
using bufio = go.bufio_package;
using lzw = go.compress.lzw_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using image = go.image_package;
using color = go.image.color_package;
using io = go.io_package;

namespace go.image;

public static partial class gif_package {

private static var errNotEnough = errors.New("gif: not enough image data");private static var errTooMuch = errors.New("gif: too much image data");private static var errBadPixel = errors.New("gif: invalid pixel value");

// If the io.Reader does not also have ReadByte, then decode will introduce its own buffering.
private partial interface reader {
}

// Masks etc.
 
// Fields.
private static readonly nint fColorTable = 1 << 7;
private static readonly nint fInterlace = 1 << 6;
private static readonly nint fColorTableBitsMask = 7; 

// Graphic control flags.
private static readonly nint gcTransparentColorSet = 1 << 0;
private static readonly nint gcDisposalMethodMask = 7 << 2;


// Disposal Methods.
public static readonly nuint DisposalNone = 0x01;
public static readonly nuint DisposalBackground = 0x02;
public static readonly nuint DisposalPrevious = 0x03;


// Section indicators.
private static readonly nuint sExtension = 0x21;
private static readonly nuint sImageDescriptor = 0x2C;
private static readonly nuint sTrailer = 0x3B;


// Extensions.
private static readonly nuint eText = 0x01; // Plain Text
private static readonly nuint eGraphicControl = 0xF9; // Graphic Control
private static readonly nuint eComment = 0xFE; // Comment
private static readonly nuint eApplication = 0xFF; // Application

private static error readFull(io.Reader r, slice<byte> b) {
    var (_, err) = io.ReadFull(r, b);
    if (err == io.EOF) {
        err = io.ErrUnexpectedEOF;
    }
    return error.As(err)!;

}

private static (byte, error) readByte(io.ByteReader r) {
    byte _p0 = default;
    error _p0 = default!;

    var (b, err) = r.ReadByte();
    if (err == io.EOF) {
        err = io.ErrUnexpectedEOF;
    }
    return (b, error.As(err)!);

}

// decoder is the type used to decode a GIF file.
private partial struct decoder {
    public reader r; // From header.
    public @string vers;
    public nint width;
    public nint height;
    public nint loopCount;
    public nint delayTime;
    public byte backgroundIndex;
    public byte disposalMethod; // From image descriptor.
    public byte imageFields; // From graphics control.
    public byte transparentIndex;
    public bool hasTransparentIndex; // Computed.
    public color.Palette globalColorTable; // Used when decoding.
    public slice<nint> delay;
    public slice<byte> disposal;
    public slice<ptr<image.Paletted>> image;
    public array<byte> tmp; // must be at least 768 so we can read color table
}

// blockReader parses the block structure of GIF image data, which comprises
// (n, (n bytes)) blocks, with 1 <= n <= 255. It is the reader given to the
// LZW decoder, which is thus immune to the blocking. After the LZW decoder
// completes, there will be a 0-byte block remaining (0, ()), which is
// consumed when checking that the blockReader is exhausted.
//
// To avoid the allocation of a bufio.Reader for the lzw Reader, blockReader
// implements io.ByteReader and buffers blocks into the decoder's "tmp" buffer.
private partial struct blockReader {
    public ptr<decoder> d;
    public byte i; // d.tmp[i:j] contains the buffered bytes
    public byte j; // d.tmp[i:j] contains the buffered bytes
    public error err;
}

private static void fill(this ptr<blockReader> _addr_b) {
    ref blockReader b = ref _addr_b.val;

    if (b.err != null) {
        return ;
    }
    b.j, b.err = readByte(b.d.r);
    if (b.j == 0 && b.err == null) {
        b.err = io.EOF;
    }
    if (b.err != null) {
        return ;
    }
    b.i = 0;
    b.err = readFull(b.d.r, b.d.tmp[..(int)b.j]);
    if (b.err != null) {
        b.j = 0;
    }
}

private static (byte, error) ReadByte(this ptr<blockReader> _addr_b) {
    byte _p0 = default;
    error _p0 = default!;
    ref blockReader b = ref _addr_b.val;

    if (b.i == b.j) {
        b.fill();
        if (b.err != null) {
            return (0, error.As(b.err)!);
        }
    }
    var c = b.d.tmp[b.i];
    b.i++;
    return (c, error.As(null!)!);

}

// blockReader must implement io.Reader, but its Read shouldn't ever actually
// be called in practice. The compress/lzw package will only call ReadByte.
private static (nint, error) Read(this ptr<blockReader> _addr_b, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref blockReader b = ref _addr_b.val;

    if (len(p) == 0 || b.err != null) {
        return (0, error.As(b.err)!);
    }
    if (b.i == b.j) {
        b.fill();
        if (b.err != null) {
            return (0, error.As(b.err)!);
        }
    }
    var n = copy(p, b.d.tmp[(int)b.i..(int)b.j]);
    b.i += uint8(n);
    return (n, error.As(null!)!);

}

// close primarily detects whether or not a block terminator was encountered
// after reading a sequence of data sub-blocks. It allows at most one trailing
// sub-block worth of data. I.e., if some number of bytes exist in one sub-block
// following the end of LZW data, the very next sub-block must be the block
// terminator. If the very end of LZW data happened to fill one sub-block, at
// most one more sub-block of length 1 may exist before the block-terminator.
// These accommodations allow us to support GIFs created by less strict encoders.
// See https://golang.org/issue/16146.
private static error close(this ptr<blockReader> _addr_b) {
    ref blockReader b = ref _addr_b.val;

    if (b.err == io.EOF) { 
        // A clean block-sequence terminator was encountered while reading.
        return error.As(null!)!;

    }
    else if (b.err != null) { 
        // Some other error was encountered while reading.
        return error.As(b.err)!;

    }
    if (b.i == b.j) { 
        // We reached the end of a sub block reading LZW data. We'll allow at
        // most one more sub block of data with a length of 1 byte.
        b.fill();
        if (b.err == io.EOF) {
            return error.As(null!)!;
        }
        else if (b.err != null) {
            return error.As(b.err)!;
        }
        else if (b.j > 1) {
            return error.As(errTooMuch)!;
        }
    }
    b.fill();
    if (b.err == io.EOF) {
        return error.As(null!)!;
    }
    else if (b.err != null) {
        return error.As(b.err)!;
    }
    return error.As(errTooMuch)!;

}

// decode reads a GIF image from r and stores the result in d.
private static error decode(this ptr<decoder> _addr_d, io.Reader r, bool configOnly, bool keepAllFrames) {
    ref decoder d = ref _addr_d.val;
 
    // Add buffering if r does not provide ReadByte.
    {
        reader (rr, ok) = reader.As(r._<reader>())!;

        if (ok) {
            d.r = rr;
        }
        else
 {
            d.r = bufio.NewReader(r);
        }
    }


    d.loopCount = -1;

    var err = d.readHeaderAndScreenDescriptor();
    if (err != null) {
        return error.As(err)!;
    }
    if (configOnly) {
        return error.As(null!)!;
    }
    while (true) {
        var (c, err) = readByte(d.r);
        if (err != null) {
            return error.As(fmt.Errorf("gif: reading frames: %v", err))!;
        }

        if (c == sExtension) 
            err = d.readExtension();

            if (err != null) {
                return error.As(err)!;
            }

        else if (c == sImageDescriptor) 
            err = d.readImageDescriptor(keepAllFrames);

            if (err != null) {
                return error.As(err)!;
            }

        else if (c == sTrailer) 
            if (len(d.image) == 0) {
                return error.As(fmt.Errorf("gif: missing image data"))!;
            }
            return error.As(null!)!;
        else 
            return error.As(fmt.Errorf("gif: unknown block type: 0x%.2x", c))!;
        
    }

}

private static error readHeaderAndScreenDescriptor(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;

    var err = readFull(d.r, d.tmp[..(int)13]);
    if (err != null) {
        return error.As(fmt.Errorf("gif: reading header: %v", err))!;
    }
    d.vers = string(d.tmp[..(int)6]);
    if (d.vers != "GIF87a" && d.vers != "GIF89a") {
        return error.As(fmt.Errorf("gif: can't recognize format %q", d.vers))!;
    }
    d.width = int(d.tmp[6]) + int(d.tmp[7]) << 8;
    d.height = int(d.tmp[8]) + int(d.tmp[9]) << 8;
    {
        var fields = d.tmp[10];

        if (fields & fColorTable != 0) {
            d.backgroundIndex = d.tmp[11]; 
            // readColorTable overwrites the contents of d.tmp, but that's OK.
            d.globalColorTable, err = d.readColorTable(fields);

            if (err != null) {
                return error.As(err)!;
            }

        }
    } 
    // d.tmp[12] is the Pixel Aspect Ratio, which is ignored.
    return error.As(null!)!;

}

private static (color.Palette, error) readColorTable(this ptr<decoder> _addr_d, byte fields) {
    color.Palette _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    nint n = 1 << (int)((1 + uint(fields & fColorTableBitsMask)));
    var err = readFull(d.r, d.tmp[..(int)3 * n]);
    if (err != null) {
        return (null, error.As(fmt.Errorf("gif: reading color table: %s", err))!);
    }
    nint j = 0;
    var p = make(color.Palette, n);
    foreach (var (i) in p) {
        p[i] = new color.RGBA(d.tmp[j+0],d.tmp[j+1],d.tmp[j+2],0xFF);
        j += 3;
    }    return (p, error.As(null!)!);

}

private static error readExtension(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;

    var (extension, err) = readByte(d.r);
    if (err != null) {
        return error.As(fmt.Errorf("gif: reading extension: %v", err))!;
    }
    nint size = 0;

    if (extension == eText) 
        size = 13;
    else if (extension == eGraphicControl) 
        return error.As(d.readGraphicControl())!;
    else if (extension == eComment)     else if (extension == eApplication) 
        var (b, err) = readByte(d.r);
        if (err != null) {
            return error.As(fmt.Errorf("gif: reading extension: %v", err))!;
        }
        size = int(b);
    else 
        return error.As(fmt.Errorf("gif: unknown extension 0x%.2x", extension))!;
        if (size > 0) {
        {
            var err = readFull(d.r, d.tmp[..(int)size]);

            if (err != null) {
                return error.As(fmt.Errorf("gif: reading extension: %v", err))!;
            }

        }

    }
    if (extension == eApplication && string(d.tmp[..(int)size]) == "NETSCAPE2.0") {
        var (n, err) = d.readBlock();
        if (err != null) {
            return error.As(fmt.Errorf("gif: reading extension: %v", err))!;
        }
        if (n == 0) {
            return error.As(null!)!;
        }
        if (n == 3 && d.tmp[0] == 1) {
            d.loopCount = int(d.tmp[1]) | int(d.tmp[2]) << 8;
        }
    }
    while (true) {
        (n, err) = d.readBlock();
        if (err != null) {
            return error.As(fmt.Errorf("gif: reading extension: %v", err))!;
        }
        if (n == 0) {
            return error.As(null!)!;
        }
    }

}

private static error readGraphicControl(this ptr<decoder> _addr_d) {
    ref decoder d = ref _addr_d.val;

    {
        var err = readFull(d.r, d.tmp[..(int)6]);

        if (err != null) {
            return error.As(fmt.Errorf("gif: can't read graphic control: %s", err))!;
        }
    }

    if (d.tmp[0] != 4) {
        return error.As(fmt.Errorf("gif: invalid graphic control extension block size: %d", d.tmp[0]))!;
    }
    var flags = d.tmp[1];
    d.disposalMethod = (flags & gcDisposalMethodMask) >> 2;
    d.delayTime = int(d.tmp[2]) | int(d.tmp[3]) << 8;
    if (flags & gcTransparentColorSet != 0) {
        d.transparentIndex = d.tmp[4];
        d.hasTransparentIndex = true;
    }
    if (d.tmp[5] != 0) {
        return error.As(fmt.Errorf("gif: invalid graphic control extension block terminator: %d", d.tmp[5]))!;
    }
    return error.As(null!)!;

}

private static error readImageDescriptor(this ptr<decoder> _addr_d, bool keepAllFrames) => func((defer, _, _) => {
    ref decoder d = ref _addr_d.val;

    var (m, err) = d.newImageFromDescriptor();
    if (err != null) {
        return error.As(err)!;
    }
    var useLocalColorTable = d.imageFields & fColorTable != 0;
    if (useLocalColorTable) {
        m.Palette, err = d.readColorTable(d.imageFields);
        if (err != null) {
            return error.As(err)!;
        }
    }
    else
 {
        if (d.globalColorTable == null) {
            return error.As(errors.New("gif: no color table"))!;
        }
        m.Palette = d.globalColorTable;

    }
    if (d.hasTransparentIndex) {
        if (!useLocalColorTable) { 
            // Clone the global color table.
            m.Palette = append(color.Palette(null), d.globalColorTable);

        }
        {
            var ti = int(d.transparentIndex);

            if (ti < len(m.Palette)) {
                m.Palette[ti] = new color.RGBA();
            }
            else
 { 
                // The transparentIndex is out of range, which is an error
                // according to the spec, but Firefox and Google Chrome
                // seem OK with this, so we enlarge the palette with
                // transparent colors. See golang.org/issue/15059.
                var p = make(color.Palette, ti + 1);
                copy(p, m.Palette);
                for (var i = len(m.Palette); i < len(p); i++) {
                    p[i] = new color.RGBA();
                }

                m.Palette = p;

            }

        }

    }
    var (litWidth, err) = readByte(d.r);
    if (err != null) {
        return error.As(fmt.Errorf("gif: reading image data: %v", err))!;
    }
    if (litWidth < 2 || litWidth > 8) {
        return error.As(fmt.Errorf("gif: pixel size in decode out of range: %d", litWidth))!;
    }
    ptr<blockReader> br = addr(new blockReader(d:d));
    var lzwr = lzw.NewReader(br, lzw.LSB, int(litWidth));
    defer(lzwr.Close());
    err = readFull(lzwr, m.Pix);

    if (err != null) {
        if (err != io.ErrUnexpectedEOF) {
            return error.As(fmt.Errorf("gif: reading image data: %v", err))!;
        }
        return error.As(errNotEnough)!;

    }
    {
        var (n, err) = lzwr.Read(d.tmp[(int)256..(int)257]);

        if (n != 0 || (err != io.EOF && err != io.ErrUnexpectedEOF)) {
            if (err != null) {
                return error.As(fmt.Errorf("gif: reading image data: %v", err))!;
            }
            return error.As(errTooMuch)!;
        }
    } 

    // In practice, some GIFs have an extra byte in the data sub-block
    // stream, which we ignore. See https://golang.org/issue/16146.
    {
        var err = br.close();

        if (err == errTooMuch) {
            return error.As(errTooMuch)!;
        }
        else if (err != null) {
            return error.As(fmt.Errorf("gif: reading image data: %v", err))!;
        }

    } 

    // Check that the color indexes are inside the palette.
    if (len(m.Palette) < 256) {
        foreach (var (_, pixel) in m.Pix) {
            if (int(pixel) >= len(m.Palette)) {
                return error.As(errBadPixel)!;
            }
        }
    }
    if (d.imageFields & fInterlace != 0) {
        uninterlace(_addr_m);
    }
    if (keepAllFrames || len(d.image) == 0) {
        d.image = append(d.image, m);
        d.delay = append(d.delay, d.delayTime);
        d.disposal = append(d.disposal, d.disposalMethod);
    }
    d.delayTime = 0;
    d.hasTransparentIndex = false;
    return error.As(null!)!;

});

private static (ptr<image.Paletted>, error) newImageFromDescriptor(this ptr<decoder> _addr_d) {
    ptr<image.Paletted> _p0 = default!;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    {
        var err = readFull(d.r, d.tmp[..(int)9]);

        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("gif: can't read image descriptor: %s", err))!);
        }
    }

    var left = int(d.tmp[0]) + int(d.tmp[1]) << 8;
    var top = int(d.tmp[2]) + int(d.tmp[3]) << 8;
    var width = int(d.tmp[4]) + int(d.tmp[5]) << 8;
    var height = int(d.tmp[6]) + int(d.tmp[7]) << 8;
    d.imageFields = d.tmp[8]; 

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
    if (left + width > d.width || top + height > d.height) {
        return (_addr_null!, error.As(errors.New("gif: frame bounds larger than image bounds"))!);
    }
    return (_addr_image.NewPaletted(new image.Rectangle(Min:image.Point{left,top},Max:image.Point{left+width,top+height},), null)!, error.As(null!)!);

}

private static (nint, error) readBlock(this ptr<decoder> _addr_d) {
    nint _p0 = default;
    error _p0 = default!;
    ref decoder d = ref _addr_d.val;

    var (n, err) = readByte(d.r);
    if (n == 0 || err != null) {
        return (0, error.As(err)!);
    }
    {
        var err = readFull(d.r, d.tmp[..(int)n]);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }

    return (int(n), error.As(null!)!);

}

// interlaceScan defines the ordering for a pass of the interlace algorithm.
private partial struct interlaceScan {
    public nint skip;
    public nint start;
}

// interlacing represents the set of scans in an interlaced GIF image.
private static interlaceScan interlacing = new slice<interlaceScan>(new interlaceScan[] { {8,0}, {8,4}, {4,2}, {2,1} });

// uninterlace rearranges the pixels in m to account for interlaced input.
private static void uninterlace(ptr<image.Paletted> _addr_m) {
    ref image.Paletted m = ref _addr_m.val;

    slice<byte> nPix = default;
    var dx = m.Bounds().Dx();
    var dy = m.Bounds().Dy();
    nPix = make_slice<byte>(dx * dy);
    nint offset = 0; // steps through the input by sequential scan lines.
    foreach (var (_, pass) in interlacing) {
        var nOffset = pass.start * dx; // steps through the output as defined by pass.
        {
            var y = pass.start;

            while (y < dy) {
                copy(nPix[(int)nOffset..(int)nOffset + dx], m.Pix[(int)offset..(int)offset + dx]);
                offset += dx;
                nOffset += dx * pass.skip;
                y += pass.skip;
            }

        }

    }    m.Pix = nPix;

}

// Decode reads a GIF image from r and returns the first embedded
// image as an image.Image.
public static (image.Image, error) Decode(io.Reader r) {
    image.Image _p0 = default;
    error _p0 = default!;

    decoder d = default;
    {
        var err = d.decode(r, false, false);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    return (d.image[0], error.As(null!)!);

}

// GIF represents the possibly multiple images stored in a GIF file.
public partial struct GIF {
    public slice<ptr<image.Paletted>> Image; // The successive images.
    public slice<nint> Delay; // The successive delay times, one per frame, in 100ths of a second.
// LoopCount controls the number of times an animation will be
// restarted during display.
// A LoopCount of 0 means to loop forever.
// A LoopCount of -1 means to show each frame only once.
// Otherwise, the animation is looped LoopCount+1 times.
    public nint LoopCount; // Disposal is the successive disposal methods, one per frame. For
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
public static (ptr<GIF>, error) DecodeAll(io.Reader r) {
    ptr<GIF> _p0 = default!;
    error _p0 = default!;

    decoder d = default;
    {
        var err = d.decode(r, false, true);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    ptr<GIF> gif = addr(new GIF(Image:d.image,LoopCount:d.loopCount,Delay:d.delay,Disposal:d.disposal,Config:image.Config{ColorModel:d.globalColorTable,Width:d.width,Height:d.height,},BackgroundIndex:d.backgroundIndex,));
    return (_addr_gif!, error.As(null!)!);

}

// DecodeConfig returns the global color model and dimensions of a GIF image
// without decoding the entire image.
public static (image.Config, error) DecodeConfig(io.Reader r) {
    image.Config _p0 = default;
    error _p0 = default!;

    decoder d = default;
    {
        var err = d.decode(r, true, false);

        if (err != null) {
            return (new image.Config(), error.As(err)!);
        }
    }

    return (new image.Config(ColorModel:d.globalColorTable,Width:d.width,Height:d.height,), error.As(null!)!);

}

private static void init() {
    image.RegisterFormat("gif", "GIF8?a", Decode, DecodeConfig);
}

} // end gif_package
