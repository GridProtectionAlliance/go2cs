// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package png -- go2cs converted at 2022 March 06 23:36:21 UTC
// import "image/png" ==> using png = go.image.png_package
// Original source: C:\Program Files\Go\src\image\png\writer.go
using bufio = go.bufio_package;
using zlib = go.compress.zlib_package;
using binary = go.encoding.binary_package;
using crc32 = go.hash.crc32_package;
using image = go.image_package;
using color = go.image.color_package;
using io = go.io_package;
using strconv = go.strconv_package;

namespace go.image;

public static partial class png_package {

    // Encoder configures encoding PNG images.
public partial struct Encoder {
    public CompressionLevel CompressionLevel; // BufferPool optionally specifies a buffer pool to get temporary
// EncoderBuffers when encoding an image.
    public EncoderBufferPool BufferPool;
}

// EncoderBufferPool is an interface for getting and returning temporary
// instances of the EncoderBuffer struct. This can be used to reuse buffers
// when encoding multiple images.
public partial interface EncoderBufferPool {
    ptr<EncoderBuffer> Get();
    ptr<EncoderBuffer> Put(ptr<EncoderBuffer> _p0);
}

// EncoderBuffer holds the buffers used for encoding PNG images.
public partial struct EncoderBuffer { // : encoder
}

private partial struct encoder {
    public ptr<Encoder> enc;
    public io.Writer w;
    public image.Image m;
    public nint cb;
    public error err;
    public array<byte> header;
    public array<byte> footer;
    public array<byte> tmp;
    public array<slice<byte>> cr;
    public slice<byte> pr;
    public ptr<zlib.Writer> zw;
    public nint zwLevel;
    public ptr<bufio.Writer> bw;
}

// CompressionLevel indicates the compression level.
public partial struct CompressionLevel { // : nint
}

public static readonly CompressionLevel DefaultCompression = 0;
public static readonly CompressionLevel NoCompression = -1;
public static readonly CompressionLevel BestSpeed = -2;
public static readonly CompressionLevel BestCompression = -3; 

// Positive CompressionLevel values are reserved to mean a numeric zlib
// compression level, although that is not implemented yet.

private partial interface opaquer {
    bool Opaque();
}

// Returns whether or not the image is fully opaque.
private static bool opaque(image.Image m) {
    {
        opaquer (o, ok) = opaquer.As(m._<opaquer>())!;

        if (ok) {
            return o.Opaque();
        }
    }

    var b = m.Bounds();
    for (var y = b.Min.Y; y < b.Max.Y; y++) {
        for (var x = b.Min.X; x < b.Max.X; x++) {
            var (_, _, _, a) = m.At(x, y).RGBA();
            if (a != 0xffff) {
                return false;
            }
        }
    }
    return true;

}

// The absolute value of a byte interpreted as a signed int8.
private static nint abs8(byte d) {
    if (d < 128) {
        return int(d);
    }
    return 256 - int(d);

}

private static void writeChunk(this ptr<encoder> _addr_e, slice<byte> b, @string name) {
    ref encoder e = ref _addr_e.val;

    if (e.err != null) {
        return ;
    }
    var n = uint32(len(b));
    if (int(n) != len(b)) {
        e.err = UnsupportedError(name + " chunk is too large: " + strconv.Itoa(len(b)));
        return ;
    }
    binary.BigEndian.PutUint32(e.header[..(int)4], n);
    e.header[4] = name[0];
    e.header[5] = name[1];
    e.header[6] = name[2];
    e.header[7] = name[3];
    var crc = crc32.NewIEEE();
    crc.Write(e.header[(int)4..(int)8]);
    crc.Write(b);
    binary.BigEndian.PutUint32(e.footer[..(int)4], crc.Sum32());

    _, e.err = e.w.Write(e.header[..(int)8]);
    if (e.err != null) {
        return ;
    }
    _, e.err = e.w.Write(b);
    if (e.err != null) {
        return ;
    }
    _, e.err = e.w.Write(e.footer[..(int)4]);

}

private static void writeIHDR(this ptr<encoder> _addr_e) {
    ref encoder e = ref _addr_e.val;

    var b = e.m.Bounds();
    binary.BigEndian.PutUint32(e.tmp[(int)0..(int)4], uint32(b.Dx()));
    binary.BigEndian.PutUint32(e.tmp[(int)4..(int)8], uint32(b.Dy())); 
    // Set bit depth and color type.

    if (e.cb == cbG8) 
        e.tmp[8] = 8;
        e.tmp[9] = ctGrayscale;
    else if (e.cb == cbTC8) 
        e.tmp[8] = 8;
        e.tmp[9] = ctTrueColor;
    else if (e.cb == cbP8) 
        e.tmp[8] = 8;
        e.tmp[9] = ctPaletted;
    else if (e.cb == cbP4) 
        e.tmp[8] = 4;
        e.tmp[9] = ctPaletted;
    else if (e.cb == cbP2) 
        e.tmp[8] = 2;
        e.tmp[9] = ctPaletted;
    else if (e.cb == cbP1) 
        e.tmp[8] = 1;
        e.tmp[9] = ctPaletted;
    else if (e.cb == cbTCA8) 
        e.tmp[8] = 8;
        e.tmp[9] = ctTrueColorAlpha;
    else if (e.cb == cbG16) 
        e.tmp[8] = 16;
        e.tmp[9] = ctGrayscale;
    else if (e.cb == cbTC16) 
        e.tmp[8] = 16;
        e.tmp[9] = ctTrueColor;
    else if (e.cb == cbTCA16) 
        e.tmp[8] = 16;
        e.tmp[9] = ctTrueColorAlpha;
        e.tmp[10] = 0; // default compression method
    e.tmp[11] = 0; // default filter method
    e.tmp[12] = 0; // non-interlaced
    e.writeChunk(e.tmp[..(int)13], "IHDR");

}

private static void writePLTEAndTRNS(this ptr<encoder> _addr_e, color.Palette p) {
    ref encoder e = ref _addr_e.val;

    if (len(p) < 1 || len(p) > 256) {
        e.err = FormatError("bad palette length: " + strconv.Itoa(len(p)));
        return ;
    }
    nint last = -1;
    foreach (var (i, c) in p) {
        color.NRGBA c1 = color.NRGBAModel.Convert(c)._<color.NRGBA>();
        e.tmp[3 * i + 0] = c1.R;
        e.tmp[3 * i + 1] = c1.G;
        e.tmp[3 * i + 2] = c1.B;
        if (c1.A != 0xff) {
            last = i;
        }
        e.tmp[3 * 256 + i] = c1.A;

    }    e.writeChunk(e.tmp[..(int)3 * len(p)], "PLTE");
    if (last != -1) {
        e.writeChunk(e.tmp[(int)3 * 256..(int)3 * 256 + 1 + last], "tRNS");
    }
}

// An encoder is an io.Writer that satisfies writes by writing PNG IDAT chunks,
// including an 8-byte header and 4-byte CRC checksum per Write call. Such calls
// should be relatively infrequent, since writeIDATs uses a bufio.Writer.
//
// This method should only be called from writeIDATs (via writeImage).
// No other code should treat an encoder as an io.Writer.
private static (nint, error) Write(this ptr<encoder> _addr_e, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref encoder e = ref _addr_e.val;

    e.writeChunk(b, "IDAT");
    if (e.err != null) {
        return (0, error.As(e.err)!);
    }
    return (len(b), error.As(null!)!);

}

// Chooses the filter to use for encoding the current row, and applies it.
// The return value is the index of the filter and also of the row in cr that has had it applied.
private static nint filter(ptr<array<slice<byte>>> _addr_cr, slice<byte> pr, nint bpp) {
    ref array<slice<byte>> cr = ref _addr_cr.val;
 
    // We try all five filter types, and pick the one that minimizes the sum of absolute differences.
    // This is the same heuristic that libpng uses, although the filters are attempted in order of
    // estimated most likely to be minimal (ftUp, ftPaeth, ftNone, ftSub, ftAverage), rather than
    // in their enumeration order (ftNone, ftSub, ftUp, ftAverage, ftPaeth).
    var cdat0 = cr[0][(int)1..];
    var cdat1 = cr[1][(int)1..];
    var cdat2 = cr[2][(int)1..];
    var cdat3 = cr[3][(int)1..];
    var cdat4 = cr[4][(int)1..];
    var pdat = pr[(int)1..];
    var n = len(cdat0); 

    // The up filter.
    nint sum = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < n; i++) {
            cdat2[i] = cdat0[i] - pdat[i];
            sum += abs8(cdat2[i]);
        }

        i = i__prev1;
    }
    var best = sum;
    var filter = ftUp; 

    // The Paeth filter.
    sum = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < bpp; i++) {
            cdat4[i] = cdat0[i] - pdat[i];
            sum += abs8(cdat4[i]);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = bpp; i < n; i++) {
            cdat4[i] = cdat0[i] - paeth(cdat0[i - bpp], pdat[i], pdat[i - bpp]);
            sum += abs8(cdat4[i]);
            if (sum >= best) {
                break;
            }
        }

        i = i__prev1;
    }
    if (sum < best) {
        best = sum;
        filter = ftPaeth;
    }
    sum = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < n; i++) {
            sum += abs8(cdat0[i]);
            if (sum >= best) {
                break;
            }
        }

        i = i__prev1;
    }
    if (sum < best) {
        best = sum;
        filter = ftNone;
    }
    sum = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < bpp; i++) {
            cdat1[i] = cdat0[i];
            sum += abs8(cdat1[i]);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = bpp; i < n; i++) {
            cdat1[i] = cdat0[i] - cdat0[i - bpp];
            sum += abs8(cdat1[i]);
            if (sum >= best) {
                break;
            }
        }

        i = i__prev1;
    }
    if (sum < best) {
        best = sum;
        filter = ftSub;
    }
    sum = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < bpp; i++) {
            cdat3[i] = cdat0[i] - pdat[i] / 2;
            sum += abs8(cdat3[i]);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = bpp; i < n; i++) {
            cdat3[i] = cdat0[i] - uint8((int(cdat0[i - bpp]) + int(pdat[i])) / 2);
            sum += abs8(cdat3[i]);
            if (sum >= best) {
                break;
            }
        }

        i = i__prev1;
    }
    if (sum < best) {
        filter = ftAverage;
    }
    return filter;

}

private static void zeroMemory(slice<byte> v) {
    foreach (var (i) in v) {
        v[i] = 0;
    }
}

private static error writeImage(this ptr<encoder> _addr_e, io.Writer w, image.Image m, nint cb, nint level) => func((defer, _, _) => {
    ref encoder e = ref _addr_e.val;

    if (e.zw == null || e.zwLevel != level) {
        var (zw, err) = zlib.NewWriterLevel(w, level);
        if (err != null) {
            return error.As(err)!;
        }
        e.zw = zw;
        e.zwLevel = level;

    }
    else
 {
        e.zw.Reset(w);
    }
    defer(e.zw.Close());

    nint bitsPerPixel = 0;


    if (cb == cbG8) 
        bitsPerPixel = 8;
    else if (cb == cbTC8) 
        bitsPerPixel = 24;
    else if (cb == cbP8) 
        bitsPerPixel = 8;
    else if (cb == cbP4) 
        bitsPerPixel = 4;
    else if (cb == cbP2) 
        bitsPerPixel = 2;
    else if (cb == cbP1) 
        bitsPerPixel = 1;
    else if (cb == cbTCA8) 
        bitsPerPixel = 32;
    else if (cb == cbTC16) 
        bitsPerPixel = 48;
    else if (cb == cbTCA16) 
        bitsPerPixel = 64;
    else if (cb == cbG16) 
        bitsPerPixel = 16;
    // cr[*] and pr are the bytes for the current and previous row.
    // cr[0] is unfiltered (or equivalently, filtered with the ftNone filter).
    // cr[ft], for non-zero filter types ft, are buffers for transforming cr[0] under the
    // other PNG filter types. These buffers are allocated once and re-used for each row.
    // The +1 is for the per-row filter type, which is at cr[*][0].
    var b = m.Bounds();
    nint sz = 1 + (bitsPerPixel * b.Dx() + 7) / 8;
    {
        var i__prev1 = i;

        foreach (var (__i) in e.cr) {
            i = __i;
            if (cap(e.cr[i]) < sz) {
                e.cr[i] = make_slice<byte>(sz);
            }
            else
 {
                e.cr[i] = e.cr[i][..(int)sz];
            }

            e.cr[i][0] = uint8(i);

        }
        i = i__prev1;
    }

    ref var cr = ref heap(e.cr, out ptr<var> _addr_cr);
    if (cap(e.pr) < sz) {
        e.pr = make_slice<byte>(sz);
    }
    else
 {
        e.pr = e.pr[..(int)sz];
        zeroMemory(e.pr);
    }
    var pr = e.pr;

    ptr<image.Gray> (gray, _) = m._<ptr<image.Gray>>();
    ptr<image.RGBA> (rgba, _) = m._<ptr<image.RGBA>>();
    ptr<image.Paletted> (paletted, _) = m._<ptr<image.Paletted>>();
    ptr<image.NRGBA> (nrgba, _) = m._<ptr<image.NRGBA>>();

    for (var y = b.Min.Y; y < b.Max.Y; y++) { 
        // Convert from colors to bytes.
        nint i = 1;

        if (cb == cbG8) 
            if (gray != null) {
                var offset = (y - b.Min.Y) * gray.Stride;
                copy(cr[0][(int)1..], gray.Pix[(int)offset..(int)offset + b.Dx()]);
            }
            else
 {
                {
                    var x__prev2 = x;

                    for (var x = b.Min.X; x < b.Max.X; x++) {
                        color.Gray c = color.GrayModel.Convert(m.At(x, y))._<color.Gray>();
                        cr[0][i] = c.Y;
                        i++;
                    }


                    x = x__prev2;
                }

            }

        else if (cb == cbTC8) 
            // We have previously verified that the alpha value is fully opaque.
            var cr0 = cr[0];
            nint stride = 0;
            slice<byte> pix = (slice<byte>)null;
            if (rgba != null) {
                (stride, pix) = (rgba.Stride, rgba.Pix);
            }
            else if (nrgba != null) {
                (stride, pix) = (nrgba.Stride, nrgba.Pix);
            }

            if (stride != 0) {
                var j0 = (y - b.Min.Y) * stride;
                var j1 = j0 + b.Dx() * 4;
                {
                    var j = j0;

                    while (j < j1) {
                        cr0[i + 0] = pix[j + 0];
                        cr0[i + 1] = pix[j + 1];
                        cr0[i + 2] = pix[j + 2];
                        i += 3;
                        j += 4;
                    }
            else

                }

            } {
                {
                    var x__prev2 = x;

                    for (x = b.Min.X; x < b.Max.X; x++) {
                        var (r, g, b, _) = m.At(x, y).RGBA();
                        cr0[i + 0] = uint8(r >> 8);
                        cr0[i + 1] = uint8(g >> 8);
                        cr0[i + 2] = uint8(b >> 8);
                        i += 3;
                    }


                    x = x__prev2;
                }

            }

        else if (cb == cbP8) 
            if (paletted != null) {
                offset = (y - b.Min.Y) * paletted.Stride;
                copy(cr[0][(int)1..], paletted.Pix[(int)offset..(int)offset + b.Dx()]);
            }
            else
 {
                image.PalettedImage pi = m._<image.PalettedImage>();
                {
                    var x__prev2 = x;

                    for (x = b.Min.X; x < b.Max.X; x++) {
                        cr[0][i] = pi.ColorIndexAt(x, y);
                        i += 1;
                    }


                    x = x__prev2;
                }

            }

        else if (cb == cbP4 || cb == cbP2 || cb == cbP1) 
            pi = m._<image.PalettedImage>();

            byte a = default;
            c = default;
            nint pixelsPerByte = 8 / bitsPerPixel;
            {
                var x__prev2 = x;

                for (x = b.Min.X; x < b.Max.X; x++) {
                    a = a << (int)(uint(bitsPerPixel)) | pi.ColorIndexAt(x, y);
                    c++;
                    if (c == pixelsPerByte) {
                        cr[0][i] = a;
                        i += 1;
                        a = 0;
                        c = 0;
                    }
                }


                x = x__prev2;
            }
            if (c != 0) {
                while (c != pixelsPerByte) {
                    a = a << (int)(uint(bitsPerPixel));
                    c++;
                }

                cr[0][i] = a;
            }

        else if (cb == cbTCA8) 
            if (nrgba != null) {
                offset = (y - b.Min.Y) * nrgba.Stride;
                copy(cr[0][(int)1..], nrgba.Pix[(int)offset..(int)offset + b.Dx() * 4]);
            }
            else
 { 
                // Convert from image.Image (which is alpha-premultiplied) to PNG's non-alpha-premultiplied.
                {
                    var x__prev2 = x;

                    for (x = b.Min.X; x < b.Max.X; x++) {
                        c = color.NRGBAModel.Convert(m.At(x, y))._<color.NRGBA>();
                        cr[0][i + 0] = c.R;
                        cr[0][i + 1] = c.G;
                        cr[0][i + 2] = c.B;
                        cr[0][i + 3] = c.A;
                        i += 4;
                    }


                    x = x__prev2;
                }

            }

        else if (cb == cbG16) 
            {
                var x__prev2 = x;

                for (x = b.Min.X; x < b.Max.X; x++) {
                    c = color.Gray16Model.Convert(m.At(x, y))._<color.Gray16>();
                    cr[0][i + 0] = uint8(c.Y >> 8);
                    cr[0][i + 1] = uint8(c.Y);
                    i += 2;
                }


                x = x__prev2;
            }
        else if (cb == cbTC16) 
            // We have previously verified that the alpha value is fully opaque.
            {
                var x__prev2 = x;

                for (x = b.Min.X; x < b.Max.X; x++) {
                    (r, g, b, _) = m.At(x, y).RGBA();
                    cr[0][i + 0] = uint8(r >> 8);
                    cr[0][i + 1] = uint8(r);
                    cr[0][i + 2] = uint8(g >> 8);
                    cr[0][i + 3] = uint8(g);
                    cr[0][i + 4] = uint8(b >> 8);
                    cr[0][i + 5] = uint8(b);
                    i += 6;
                }


                x = x__prev2;
            }
        else if (cb == cbTCA16) 
            // Convert from image.Image (which is alpha-premultiplied) to PNG's non-alpha-premultiplied.
            {
                var x__prev2 = x;

                for (x = b.Min.X; x < b.Max.X; x++) {
                    c = color.NRGBA64Model.Convert(m.At(x, y))._<color.NRGBA64>();
                    cr[0][i + 0] = uint8(c.R >> 8);
                    cr[0][i + 1] = uint8(c.R);
                    cr[0][i + 2] = uint8(c.G >> 8);
                    cr[0][i + 3] = uint8(c.G);
                    cr[0][i + 4] = uint8(c.B >> 8);
                    cr[0][i + 5] = uint8(c.B);
                    cr[0][i + 6] = uint8(c.A >> 8);
                    cr[0][i + 7] = uint8(c.A);
                    i += 8;
                }


                x = x__prev2;
            }
        // Apply the filter.
        // Skip filter for NoCompression and paletted images (cbP8) as
        // "filters are rarely useful on palette images" and will result
        // in larger files (see http://www.libpng.org/pub/png/book/chapter09.html).
        var f = ftNone;
        if (level != zlib.NoCompression && cb != cbP8 && cb != cbP4 && cb != cbP2 && cb != cbP1) { 
            // Since we skip paletted images we don't have to worry about
            // bitsPerPixel not being a multiple of 8
            var bpp = bitsPerPixel / 8;
            f = filter(_addr_cr, pr, bpp);

        }
        {
            var (_, err) = e.zw.Write(cr[f]);

            if (err != null) {
                return error.As(err)!;
            } 

            // The current row for y is the previous row for y+1.

        } 

        // The current row for y is the previous row for y+1.
        (pr, cr[0]) = (cr[0], pr);
    }
    return error.As(null!)!;

});

// Write the actual image data to one or more IDAT chunks.
private static void writeIDATs(this ptr<encoder> _addr_e) {
    ref encoder e = ref _addr_e.val;

    if (e.err != null) {
        return ;
    }
    if (e.bw == null) {
        e.bw = bufio.NewWriterSize(e, 1 << 15);
    }
    else
 {
        e.bw.Reset(e);
    }
    e.err = e.writeImage(e.bw, e.m, e.cb, levelToZlib(e.enc.CompressionLevel));
    if (e.err != null) {
        return ;
    }
    e.err = e.bw.Flush();

}

// This function is required because we want the zero value of
// Encoder.CompressionLevel to map to zlib.DefaultCompression.
private static nint levelToZlib(CompressionLevel l) {

    if (l == DefaultCompression) 
        return zlib.DefaultCompression;
    else if (l == NoCompression) 
        return zlib.NoCompression;
    else if (l == BestSpeed) 
        return zlib.BestSpeed;
    else if (l == BestCompression) 
        return zlib.BestCompression;
    else 
        return zlib.DefaultCompression;
    
}

private static void writeIEND(this ptr<encoder> _addr_e) {
    ref encoder e = ref _addr_e.val;

    e.writeChunk(null, "IEND");
}

// Encode writes the Image m to w in PNG format. Any Image may be
// encoded, but images that are not image.NRGBA might be encoded lossily.
public static error Encode(io.Writer w, image.Image m) {
    Encoder e = default;
    return error.As(e.Encode(w, m))!;
}

// Encode writes the Image m to w in PNG format.
private static error Encode(this ptr<Encoder> _addr_enc, io.Writer w, image.Image m) => func((defer, _, _) => {
    ref Encoder enc = ref _addr_enc.val;
 
    // Obviously, negative widths and heights are invalid. Furthermore, the PNG
    // spec section 11.2.2 says that zero is invalid. Excessively large images are
    // also rejected.
    var mw = int64(m.Bounds().Dx());
    var mh = int64(m.Bounds().Dy());
    if (mw <= 0 || mh <= 0 || mw >= 1 << 32 || mh >= 1 << 32) {
        return error.As(FormatError("invalid image size: " + strconv.FormatInt(mw, 10) + "x" + strconv.FormatInt(mh, 10)))!;
    }
    ptr<encoder> e;
    if (enc.BufferPool != null) {
        var buffer = enc.BufferPool.Get();
        e = (encoder.val)(buffer);
    }
    if (e == null) {
        e = addr(new encoder());
    }
    if (enc.BufferPool != null) {
        defer(enc.BufferPool.Put((EncoderBuffer.val)(e)));
    }
    e.enc = enc;
    e.w = w;
    e.m = m;

    color.Palette pal = default; 
    // cbP8 encoding needs PalettedImage's ColorIndexAt method.
    {
        image.PalettedImage (_, ok) = m._<image.PalettedImage>();

        if (ok) {
            pal, _ = m.ColorModel()._<color.Palette>();
        }
    }

    if (pal != null) {
        if (len(pal) <= 2) {
            e.cb = cbP1;
        }
        else if (len(pal) <= 4) {
            e.cb = cbP2;
        }
        else if (len(pal) <= 16) {
            e.cb = cbP4;
        }
        else
 {
            e.cb = cbP8;
        }
    }
    else
 {

        if (m.ColorModel() == color.GrayModel) 
            e.cb = cbG8;
        else if (m.ColorModel() == color.Gray16Model) 
            e.cb = cbG16;
        else if (m.ColorModel() == color.RGBAModel || m.ColorModel() == color.NRGBAModel || m.ColorModel() == color.AlphaModel) 
            if (opaque(m)) {
                e.cb = cbTC8;
            }
            else
 {
                e.cb = cbTCA8;
            }

        else 
            if (opaque(m)) {
                e.cb = cbTC16;
            }
            else
 {
                e.cb = cbTCA16;
            }

            }
    _, e.err = io.WriteString(w, pngHeader);
    e.writeIHDR();
    if (pal != null) {
        e.writePLTEAndTRNS(pal);
    }
    e.writeIDATs();
    e.writeIEND();
    return error.As(e.err)!;

});

} // end png_package
