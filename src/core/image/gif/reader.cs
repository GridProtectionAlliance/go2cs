// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gif implements a GIF image decoder and encoder.
//
// The GIF specification is at https://www.w3.org/Graphics/GIF/spec-gif89a.txt.
namespace go.image;

using bufio = bufio_package;
using lzw = compress.lzw_package;
using errors = errors_package;
using fmt = fmt_package;
using image = image_package;
using color = image.color_package;
using io = io_package;
using compress;

partial class gif_package {

internal static error errNotEnough = errors.New("gif: not enough image data"u8);
internal static error errTooMuch = errors.New("gif: too much image data"u8);
internal static error errBadPixel = errors.New("gif: invalid pixel value"u8);

// If the io.Reader does not also have ReadByte, then decode will introduce its own buffering.
[GoType] partial interface reader :
    io.Reader,
    io.ByteReader
{
}

// Masks etc.
internal static readonly UntypedInt fColorTable = /* 1 << 7 */ 128;

internal static readonly UntypedInt fInterlace = /* 1 << 6 */ 64;

internal static readonly UntypedInt fColorTableBitsMask = 7;

internal static readonly UntypedInt gcTransparentColorSet = /* 1 << 0 */ 1;

internal static readonly UntypedInt gcDisposalMethodMask = /* 7 << 2 */ 28;

// Disposal Methods.
public static readonly UntypedInt DisposalNone = /* 0x01 */ 1;

public static readonly UntypedInt DisposalBackground = /* 0x02 */ 2;

public static readonly UntypedInt DisposalPrevious = /* 0x03 */ 3;

// Section indicators.
internal static readonly UntypedInt sExtension = /* 0x21 */ 33;

internal static readonly UntypedInt sImageDescriptor = /* 0x2C */ 44;

internal static readonly UntypedInt sTrailer = /* 0x3B */ 59;

// Extensions.
internal static readonly UntypedInt eText = /* 0x01 */ 1; // Plain Text

internal static readonly UntypedInt eGraphicControl = /* 0xF9 */ 249; // Graphic Control

internal static readonly UntypedInt eComment = /* 0xFE */ 254; // Comment

internal static readonly UntypedInt eApplication = /* 0xFF */ 255; // Application

internal static error readFull(io.Reader r, slice<byte> b) {
    var (_, err) = io.ReadFull(r, b);
    if (AreEqual(err, io.EOF)) {
        err = io.ErrUnexpectedEOF;
    }
    return err;
}

internal static (byte, error) readByte(io.ByteReader r) {
    var (b, err) = r.ReadByte();
    if (AreEqual(err, io.EOF)) {
        err = io.ErrUnexpectedEOF;
    }
    return (b, err);
}

// decoder is the type used to decode a GIF file.
[GoType] partial struct decoder {
    internal reader r;
    // From header.
    internal @string vers;
    internal nint width;
    internal nint height;
    internal nint loopCount;
    internal nint delayTime;
    internal byte backgroundIndex;
    internal byte disposalMethod;
    // From image descriptor.
    internal byte imageFields;
    // From graphics control.
    internal byte transparentIndex;
    internal bool hasTransparentIndex;
    // Computed.
    internal image.color_package.Palette globalColorTable;
    // Used when decoding.
    internal slice<nint> delay;
    internal slice<byte> disposal;
    internal slice<ж<image.Paletted>> image;
    internal array<byte> tmp = new(1024); // must be at least 768 so we can read color table
}

// blockReader parses the block structure of GIF image data, which comprises
// (n, (n bytes)) blocks, with 1 <= n <= 255. It is the reader given to the
// LZW decoder, which is thus immune to the blocking. After the LZW decoder
// completes, there will be a 0-byte block remaining (0, ()), which is
// consumed when checking that the blockReader is exhausted.
//
// To avoid the allocation of a bufio.Reader for the lzw Reader, blockReader
// implements io.ByteReader and buffers blocks into the decoder's "tmp" buffer.
[GoType] partial struct blockReader {
    internal ж<decoder> d;
    internal uint8 i; // d.tmp[i:j] contains the buffered bytes
    internal uint8 j;
    internal error err;
}

[GoRecv] internal static void fill(this ref blockReader b) {
    if (b.err != default!) {
        return;
    }
    (b.j, b.err) = readByte(b.d.r);
    if (b.j == 0 && b.err == default!) {
        b.err = io.EOF;
    }
    if (b.err != default!) {
        return;
    }
    b.i = 0;
    b.err = readFull(b.d.r, b.d.tmp[..(int)(b.j)]);
    if (b.err != default!) {
        b.j = 0;
    }
}

[GoRecv] internal static (byte, error) ReadByte(this ref blockReader b) {
    if (b.i == b.j) {
        b.fill();
        if (b.err != default!) {
            return (0, b.err);
        }
    }
    var c = b.d.tmp[b.i];
    b.i++;
    return (c, default!);
}

// blockReader must implement io.Reader, but its Read shouldn't ever actually
// be called in practice. The compress/lzw package will only call [blockReader.ReadByte].
[GoRecv] internal static (nint, error) Read(this ref blockReader b, slice<byte> p) {
    if (len(p) == 0 || b.err != default!) {
        return (0, b.err);
    }
    if (b.i == b.j) {
        b.fill();
        if (b.err != default!) {
            return (0, b.err);
        }
    }
    nint n = copy(p, b.d.tmp[(int)(b.i)..(int)(b.j)]);
    b.i += ((uint8)n);
    return (n, default!);
}

// close primarily detects whether or not a block terminator was encountered
// after reading a sequence of data sub-blocks. It allows at most one trailing
// sub-block worth of data. I.e., if some number of bytes exist in one sub-block
// following the end of LZW data, the very next sub-block must be the block
// terminator. If the very end of LZW data happened to fill one sub-block, at
// most one more sub-block of length 1 may exist before the block-terminator.
// These accommodations allow us to support GIFs created by less strict encoders.
// See https://golang.org/issue/16146.
[GoRecv] internal static error close(this ref blockReader b) {
    if (AreEqual(b.err, io.EOF)){
        // A clean block-sequence terminator was encountered while reading.
        return default!;
    } else 
    if (b.err != default!) {
        // Some other error was encountered while reading.
        return b.err;
    }
    if (b.i == b.j) {
        // We reached the end of a sub block reading LZW data. We'll allow at
        // most one more sub block of data with a length of 1 byte.
        b.fill();
        if (AreEqual(b.err, io.EOF)){
            return default!;
        } else 
        if (b.err != default!){
            return b.err;
        } else 
        if (b.j > 1) {
            return errTooMuch;
        }
    }
    // Part of a sub-block remains buffered. We expect that the next attempt to
    // buffer a sub-block will reach the block terminator.
    b.fill();
    if (AreEqual(b.err, io.EOF)){
        return default!;
    } else 
    if (b.err != default!) {
        return b.err;
    }
    return errTooMuch;
}

// decode reads a GIF image from r and stores the result in d.
[GoRecv] internal static error decode(this ref decoder d, io.Reader r, bool configOnly, bool keepAllFrames) {
    // Add buffering if r does not provide ReadByte.
    {
        var (rr, ok) = r._<reader>(ᐧ); if (ok){
            d.r = rr;
        } else {
            d.r = bufio.NewReader(r);
        }
    }
    d.loopCount = -1;
    var err = d.readHeaderAndScreenDescriptor();
    if (err != default!) {
        return err;
    }
    if (configOnly) {
        return default!;
    }
    while (ᐧ) {
        var (c, errΔ1) = readByte(d.r);
        if (errΔ1 != default!) {
            return fmt.Errorf("gif: reading frames: %v"u8, errΔ1);
        }
        switch (c) {
        case sExtension: {
            {
                errΔ1 = d.readExtension(); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            break;
        }
        case sImageDescriptor: {
            {
                errΔ1 = d.readImageDescriptor(keepAllFrames); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            if (!keepAllFrames && len(d.image) == 1) {
                return default!;
            }
            break;
        }
        case sTrailer: {
            if (len(d.image) == 0) {
                return fmt.Errorf("gif: missing image data"u8);
            }
            return default!;
        }
        default: {
            return fmt.Errorf("gif: unknown block type: 0x%.2x"u8, c);
        }}

    }
}

[GoRecv] internal static error readHeaderAndScreenDescriptor(this ref decoder d) {
    var err = readFull(d.r, d.tmp[..13]);
    if (err != default!) {
        return fmt.Errorf("gif: reading header: %v"u8, err);
    }
    d.vers = ((@string)(d.tmp[..6]));
    if (d.vers != "GIF87a"u8 && d.vers != "GIF89a"u8) {
        return fmt.Errorf("gif: can't recognize format %q"u8, d.vers);
    }
    d.width = ((nint)d.tmp[6]) + ((nint)d.tmp[7]) << (int)(8);
    d.height = ((nint)d.tmp[8]) + ((nint)d.tmp[9]) << (int)(8);
    {
        var fields = d.tmp[10]; if ((byte)(fields & fColorTable) != 0) {
            d.backgroundIndex = d.tmp[11];
            // readColorTable overwrites the contents of d.tmp, but that's OK.
            {
                var (d.globalColorTable, err) = d.readColorTable(fields); if (err != default!) {
                    return err;
                }
            }
        }
    }
    // d.tmp[12] is the Pixel Aspect Ratio, which is ignored.
    return default!;
}

[GoRecv] internal static (color.Palette, error) readColorTable(this ref decoder d, byte fields) {
    nint n = 1 << (int)((1 + ((nuint)((byte)(fields & fColorTableBitsMask)))));
    var err = readFull(d.r, d.tmp[..(int)(3 * n)]);
    if (err != default!) {
        return (default!, fmt.Errorf("gif: reading color table: %s"u8, err));
    }
    nint j = 0;
    var p = new color.Palette(n);
    foreach (var (i, _) in p) {
        p[i] = new colorꓸRGBA(d.tmp[j + 0], d.tmp[j + 1], d.tmp[j + 2], 255);
        j += 3;
    }
    return (p, default!);
}

[GoRecv] internal static error readExtension(this ref decoder d) {
    var (extension, err) = readByte(d.r);
    if (err != default!) {
        return fmt.Errorf("gif: reading extension: %v"u8, err);
    }
    nint size = 0;
    switch (extension) {
    case eText: {
        size = 13;
        break;
    }
    case eGraphicControl: {
        return d.readGraphicControl();
    }
    case eComment: {
    }
    case eApplication: {
        var (b, errΔ2) = readByte(d.r);
        if (errΔ2 != default!) {
            // nothing to do but read the data.
            return fmt.Errorf("gif: reading extension: %v"u8, errΔ2);
        }
        size = ((nint)b);
        break;
    }
    default: {
        return fmt.Errorf("gif: unknown extension 0x%.2x"u8, // The spec requires size be 11, but Adobe sometimes uses 10.
 extension);
    }}

    if (size > 0) {
        {
            var errΔ3 = readFull(d.r, d.tmp[..(int)(size)]); if (errΔ3 != default!) {
                return fmt.Errorf("gif: reading extension: %v"u8, errΔ3);
            }
        }
    }
    // Application Extension with "NETSCAPE2.0" as string and 1 in data means
    // this extension defines a loop count.
    if (extension == eApplication && ((@string)(d.tmp[..(int)(size)])) == "NETSCAPE2.0"u8) {
        var (n, errΔ4) = d.readBlock();
        if (errΔ4 != default!) {
            return fmt.Errorf("gif: reading extension: %v"u8, errΔ4);
        }
        if (n == 0) {
            return default!;
        }
        if (n == 3 && d.tmp[0] == 1) {
            d.loopCount = (nint)(((nint)d.tmp[1]) | ((nint)d.tmp[2]) << (int)(8));
        }
    }
    while (ᐧ) {
        var (n, errΔ5) = d.readBlock();
        if (errΔ5 != default!) {
            return fmt.Errorf("gif: reading extension: %v"u8, errΔ5);
        }
        if (n == 0) {
            return default!;
        }
    }
}

[GoRecv] internal static error readGraphicControl(this ref decoder d) {
    {
        var err = readFull(d.r, d.tmp[..6]); if (err != default!) {
            return fmt.Errorf("gif: can't read graphic control: %s"u8, err);
        }
    }
    if (d.tmp[0] != 4) {
        return fmt.Errorf("gif: invalid graphic control extension block size: %d"u8, d.tmp[0]);
    }
    var flags = d.tmp[1];
    d.disposalMethod = ((byte)(flags & gcDisposalMethodMask)) >> (int)(2);
    d.delayTime = (nint)(((nint)d.tmp[2]) | ((nint)d.tmp[3]) << (int)(8));
    if ((byte)(flags & gcTransparentColorSet) != 0) {
        d.transparentIndex = d.tmp[4];
        d.hasTransparentIndex = true;
    }
    if (d.tmp[5] != 0) {
        return fmt.Errorf("gif: invalid graphic control extension block terminator: %d"u8, d.tmp[5]);
    }
    return default!;
}

[GoRecv] internal static error readImageDescriptor(this ref decoder d, bool keepAllFrames) => func((defer, _) => {
    (m, err) = d.newImageFromDescriptor();
    if (err != default!) {
        return err;
    }
    var useLocalColorTable = (byte)(d.imageFields & fColorTable) != 0;
    if (useLocalColorTable){
        (m.val.Palette, err) = d.readColorTable(d.imageFields);
        if (err != default!) {
            return err;
        }
    } else {
        if (d.globalColorTable == default!) {
            return errors.New("gif: no color table"u8);
        }
        m.val.Palette = d.globalColorTable;
    }
    if (d.hasTransparentIndex) {
        if (!useLocalColorTable) {
            // Clone the global color table.
            m.val.Palette = append(((color.Palette)default!), d.globalColorTable.ꓸꓸꓸ);
        }
        {
            nint ti = ((nint)d.transparentIndex); if (ti < len((~m).Palette)){
                (~m).Palette[ti] = new colorꓸRGBA(nil);
            } else {
                // The transparentIndex is out of range, which is an error
                // according to the spec, but Firefox and Google Chrome
                // seem OK with this, so we enlarge the palette with
                // transparent colors. See golang.org/issue/15059.
                var p = new color.Palette(ti + 1);
                copy(p, (~m).Palette);
                for (nint i = len((~m).Palette); i < len(p); i++) {
                    p[i] = new colorꓸRGBA(nil);
                }
                m.val.Palette = p;
            }
        }
    }
    var (litWidth, err) = readByte(d.r);
    if (err != default!) {
        return fmt.Errorf("gif: reading image data: %v"u8, err);
    }
    if (litWidth < 2 || litWidth > 8) {
        return fmt.Errorf("gif: pixel size in decode out of range: %d"u8, litWidth);
    }
    // A wonderfully Go-like piece of magic.
    var br = Ꮡ(new blockReader(d: d));
    var lzwr = lzw.NewReader(~br, lzw.LSB, ((nint)litWidth));
    var lzwrʗ1 = lzwr;
    defer(lzwrʗ1.Close);
    {
        err = readFull(lzwr, (~m).Pix); if (err != default!) {
            if (!AreEqual(err, io.ErrUnexpectedEOF)) {
                return fmt.Errorf("gif: reading image data: %v"u8, err);
            }
            return errNotEnough;
        }
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
        var (n, errΔ1) = lzwr.Read(d.tmp[256..257]); if (n != 0 || (!AreEqual(errΔ1, io.EOF) && !AreEqual(errΔ1, io.ErrUnexpectedEOF))) {
            if (errΔ1 != default!) {
                return fmt.Errorf("gif: reading image data: %v"u8, errΔ1);
            }
            return errTooMuch;
        }
    }
    // In practice, some GIFs have an extra byte in the data sub-block
    // stream, which we ignore. See https://golang.org/issue/16146.
    {
        var errΔ2 = br.close(); if (AreEqual(errΔ2, errTooMuch)){
            return errTooMuch;
        } else 
        if (errΔ2 != default!) {
            return fmt.Errorf("gif: reading image data: %v"u8, errΔ2);
        }
    }
    // Check that the color indexes are inside the palette.
    if (len((~m).Palette) < 256) {
        foreach (var (_, pixel) in (~m).Pix) {
            if (((nint)pixel) >= len((~m).Palette)) {
                return errBadPixel;
            }
        }
    }
    // Undo the interlacing if necessary.
    if ((byte)(d.imageFields & fInterlace) != 0) {
        uninterlace(m);
    }
    if (keepAllFrames || len(d.image) == 0) {
        d.image = append(d.image, m);
        d.delay = append(d.delay, d.delayTime);
        d.disposal = append(d.disposal, d.disposalMethod);
    }
    // The GIF89a spec, Section 23 (Graphic Control Extension) says:
    // "The scope of this extension is the first graphic rendering block
    // to follow." We therefore reset the GCE fields to zero.
    d.delayTime = 0;
    d.hasTransparentIndex = false;
    return default!;
});

[GoRecv] internal static (ж<image.Paletted>, error) newImageFromDescriptor(this ref decoder d) {
    {
        var err = readFull(d.r, d.tmp[..9]); if (err != default!) {
            return (default!, fmt.Errorf("gif: can't read image descriptor: %s"u8, err));
        }
    }
    nint left = ((nint)d.tmp[0]) + ((nint)d.tmp[1]) << (int)(8);
    nint top = ((nint)d.tmp[2]) + ((nint)d.tmp[3]) << (int)(8);
    nint width = ((nint)d.tmp[4]) + ((nint)d.tmp[5]) << (int)(8);
    nint height = ((nint)d.tmp[6]) + ((nint)d.tmp[7]) << (int)(8);
    d.imageFields = d.tmp[8];
    // The GIF89a spec, Section 20 (Image Descriptor) says: "Each image must
    // fit within the boundaries of the Logical Screen, as defined in the
    // Logical Screen Descriptor."
    //
    // This is conceptually similar to testing
    //	frameBounds := image.Rect(left, top, left+width, top+height)
    //	imageBounds := image.Rect(0, 0, d.width, d.height)
    //	if !frameBounds.In(imageBounds) { etc }
    // but the semantics of the Go image.Rectangle type is that r.In(s) is true
    // whenever r is an empty rectangle, even if r.Min.X > s.Max.X. Here, we
    // want something stricter.
    //
    // Note that, by construction, left >= 0 && top >= 0, so we only have to
    // explicitly compare frameBounds.Max (left+width, top+height) against
    // imageBounds.Max (d.width, d.height) and not frameBounds.Min (left, top)
    // against imageBounds.Min (0, 0).
    if (left + width > d.width || top + height > d.height) {
        return (default!, errors.New("gif: frame bounds larger than image bounds"u8));
    }
    return (image.NewPaletted(new image.Rectangle(
        Min: new image.Point(left, top),
        Max: new image.Point(left + width, top + height)
    ), default!), default!);
}

[GoRecv] internal static (nint, error) readBlock(this ref decoder d) {
    var (n, err) = readByte(d.r);
    if (n == 0 || err != default!) {
        return (0, err);
    }
    {
        var errΔ1 = readFull(d.r, d.tmp[..(int)(n)]); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    return (((nint)n), default!);
}

// interlaceScan defines the ordering for a pass of the interlace algorithm.
[GoType] partial struct interlaceScan {
    internal nint skip;
    internal nint start;
}

// Group 1 : Every 8th. row, starting with row 0.
// Group 2 : Every 8th. row, starting with row 4.
// Group 3 : Every 4th. row, starting with row 2.
// Group 4 : Every 2nd. row, starting with row 1.
// interlacing represents the set of scans in an interlaced GIF image.
internal static slice<interlaceScan> interlacing = new interlaceScan[]{
    new(8, 0),
    new(8, 4),
    new(4, 2),
    new(2, 1)
}.slice();

// uninterlace rearranges the pixels in m to account for interlaced input.
internal static void uninterlace(ж<image.Paletted> Ꮡm) {
    ref var m = ref Ꮡm.val;

    slice<uint8> nPix = default!;
    nint dx = m.Bounds().Dx();
    nint dy = m.Bounds().Dy();
    nPix = new slice<uint8>(dx * dy);
    nint offset = 0;
    // steps through the input by sequential scan lines.
    foreach (var (_, pass) in interlacing) {
        nint nOffset = pass.start * dx;
        // steps through the output as defined by pass.
        for (nint y = pass.start; y < dy; y += pass.skip) {
            copy(nPix[(int)(nOffset)..(int)(nOffset + dx)], m.Pix[(int)(offset)..(int)(offset + dx)]);
            offset += dx;
            nOffset += dx * pass.skip;
        }
    }
    m.Pix = nPix;
}

// Decode reads a GIF image from r and returns the first embedded
// image as an [image.Image].
public static (image.Image, error) Decode(io.Reader r) {
    decoder d = default!;
    {
        var err = d.decode(r, false, false); if (err != default!) {
            return (default!, err);
        }
    }
    return (~d.image[0], default!);
}

// GIF represents the possibly multiple images stored in a GIF file.
[GoType] partial struct GIF {
    public slice<ж<image.Paletted>> Image; // The successive images.
    public slice<nint> Delay;       // The successive delay times, one per frame, in 100ths of a second.
    // LoopCount controls the number of times an animation will be
    // restarted during display.
    // A LoopCount of 0 means to loop forever.
    // A LoopCount of -1 means to show each frame only once.
    // Otherwise, the animation is looped LoopCount+1 times.
    public nint LoopCount;
    // Disposal is the successive disposal methods, one per frame. For
    // backwards compatibility, a nil Disposal is valid to pass to EncodeAll,
    // and implies that each frame's disposal method is 0 (no disposal
    // specified).
    public slice<byte> Disposal;
    // Config is the global color table (palette), width and height. A nil or
    // empty-color.Palette Config.ColorModel means that each frame has its own
    // color table and there is no global color table. Each frame's bounds must
    // be within the rectangle defined by the two points (0, 0) and
    // (Config.Width, Config.Height).
    //
    // For backwards compatibility, a zero-valued Config is valid to pass to
    // EncodeAll, and implies that the overall GIF's width and height equals
    // the first frame's bounds' Rectangle.Max point.
    public image_package.Config Config;
    // BackgroundIndex is the background index in the global color table, for
    // use with the DisposalBackground disposal method.
    public byte BackgroundIndex;
}

// DecodeAll reads a GIF image from r and returns the sequential frames
// and timing information.
public static (ж<GIF>, error) DecodeAll(io.Reader r) {
    decoder d = default!;
    {
        var err = d.decode(r, false, true); if (err != default!) {
            return (default!, err);
        }
    }
    var gif = Ꮡ(new GIF(
        Image: d.image,
        LoopCount: d.loopCount,
        Delay: d.delay,
        Disposal: d.disposal,
        Config: new image.Config(
            ColorModel: d.globalColorTable,
            Width: d.width,
            Height: d.height
        ),
        BackgroundIndex: d.backgroundIndex
    ));
    return (gif, default!);
}

// DecodeConfig returns the global color model and dimensions of a GIF image
// without decoding the entire image.
public static (image.Config, error) DecodeConfig(io.Reader r) {
    decoder d = default!;
    {
        var err = d.decode(r, true, false); if (err != default!) {
            return (new image.Config(nil), err);
        }
    }
    return (new image.Config(
        ColorModel: d.globalColorTable,
        Width: d.width,
        Height: d.height
    ), default!);
}

[GoInit] internal static void init() {
    image.RegisterFormat("gif"u8, "GIF8?a"u8, Decode, DecodeConfig);
}

} // end gif_package
