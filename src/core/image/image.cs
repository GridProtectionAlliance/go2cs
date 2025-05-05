// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package image implements a basic 2-D image library.
//
// The fundamental interface is called [Image]. An [Image] contains colors, which
// are described in the image/color package.
//
// Values of the [Image] interface are created either by calling functions such
// as [NewRGBA] and [NewPaletted], or by calling [Decode] on an [io.Reader] containing
// image data in a format such as GIF, JPEG or PNG. Decoding any particular
// image format requires the prior registration of a decoder function.
// Registration is typically automatic as a side effect of initializing that
// format's package so that, to decode a PNG image, it suffices to have
//
//	import _ "image/png"
//
// in a program's main package. The _ means to import a package purely for its
// initialization side effects.
//
// See "The Go image package" for more details:
// https://golang.org/doc/articles/image_package.html
//
// # Security Considerations
//
// The image package can be used to parse arbitrarily large images, which can
// cause resource exhaustion on machines which do not have enough memory to
// store them. When operating on arbitrary images, [DecodeConfig] should be called
// before [Decode], so that the program can decide whether the image, as defined
// in the returned header, can be safely decoded with the available resources. A
// call to [Decode] which produces an extremely large image, as defined in the
// header returned by [DecodeConfig], is not considered a security issue,
// regardless of whether the image is itself malformed or not. A call to
// [DecodeConfig] which returns a header which does not match the image returned
// by [Decode] may be considered a security issue, and should be reported per the
// [Go Security Policy](https://go.dev/security/policy).
namespace go;

using color = image.color_package;
using image;

partial class image_package {

// Config holds an image's color model and dimensions.
[GoType] partial struct Config {
    public image.color_package.Model ColorModel;
    public nint Width;
    public nint Height;
}

// Image is a finite rectangular grid of [color.Color] values taken from a color
// model.
[GoType] partial interface Image {
    // ColorModel returns the Image's color model.
    color.Model ColorModel();
    // Bounds returns the domain for which At can return non-zero color.
    // The bounds do not necessarily contain the point (0, 0).
    Rectangle Bounds();
    // At returns the color of the pixel at (x, y).
    // At(Bounds().Min.X, Bounds().Min.Y) returns the upper-left pixel of the grid.
    // At(Bounds().Max.X-1, Bounds().Max.Y-1) returns the lower-right one.
    color.Color At(nint x, nint y);
}

// RGBA64Image is an [Image] whose pixels can be converted directly to a
// color.RGBA64.
[GoType] partial interface RGBA64Image :
    Image
{
    // RGBA64At returns the RGBA64 color of the pixel at (x, y). It is
    // equivalent to calling At(x, y).RGBA() and converting the resulting
    // 32-bit return values to a color.RGBA64, but it can avoid allocations
    // from converting concrete color types to the color.Color interface type.
    color.RGBA64 RGBA64At(nint x, nint y);
}

// PalettedImage is an image whose colors may come from a limited palette.
// If m is a PalettedImage and m.ColorModel() returns a [color.Palette] p,
// then m.At(x, y) should be equivalent to p[m.ColorIndexAt(x, y)]. If m's
// color model is not a color.Palette, then ColorIndexAt's behavior is
// undefined.
[GoType] partial interface PalettedImage :
    Image
{
    // ColorIndexAt returns the palette index of the pixel at (x, y).
    uint8 ColorIndexAt(nint x, nint y);
}

// pixelBufferLength returns the length of the []uint8 typed Pix slice field
// for the NewXxx functions. Conceptually, this is just (bpp * width * height),
// but this function panics if at least one of those is negative or if the
// computation would overflow the int type.
//
// This panics instead of returning an error because of backwards
// compatibility. The NewXxx functions do not return an error.
internal static nint pixelBufferLength(nint bytesPerPixel, Rectangle r, @string imageTypeName) {
    nint totalLength = mul3NonNeg(bytesPerPixel, r.Dx(), r.Dy());
    if (totalLength < 0) {
        throw panic("image: New"u8 + imageTypeName + " Rectangle has huge or negative dimensions"u8);
    }
    return totalLength;
}

// RGBA is an in-memory image whose At method returns [color.RGBA] values.
[GoType] partial struct ΔRGBA {
    // Pix holds the image's pixels, in R, G, B, A order. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*4].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref ΔRGBA p) {
    return color.RGBAModel;
}

[GoRecv] public static Rectangle Bounds(this ref ΔRGBA p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref ΔRGBA p, nint x, nint y) {
    return p.RGBAAt(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref ΔRGBA p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.RGBA64(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    var r = ((uint16)s[0]);
    var g = ((uint16)s[1]);
    var b = ((uint16)s[2]);
    var a = ((uint16)s[3]);
    return new color.RGBA64(
        (uint16)((r << (int)(8)) | r),
        (uint16)((g << (int)(8)) | g),
        (uint16)((b << (int)(8)) | b),
        (uint16)((a << (int)(8)) | a)
    );
}

[GoRecv] public static colorꓸRGBA RGBAAt(this ref ΔRGBA p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new colorꓸRGBA(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    return new colorꓸRGBA(s[0], s[1], s[2], s[3]);
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref ΔRGBA p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4;
}

[GoRecv] public static void Set(this ref ΔRGBA p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.RGBAModel.Convert(c)._<colorꓸRGBA>();
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c1.R;
    s[1] = c1.G;
    s[2] = c1.B;
    s[3] = c1.A;
}

[GoRecv] public static void SetRGBA64(this ref ΔRGBA p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = ((uint8)(c.R >> (int)(8)));
    s[1] = ((uint8)(c.G >> (int)(8)));
    s[2] = ((uint8)(c.B >> (int)(8)));
    s[3] = ((uint8)(c.A >> (int)(8)));
}

[GoRecv] public static void SetRGBA(this ref ΔRGBA p, nint x, nint y, colorꓸRGBA c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c.R;
    s[1] = c.G;
    s[2] = c.B;
    s[3] = c.A;
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref ΔRGBA p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new ΔRGBA(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new ΔRGBA(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref ΔRGBA p) {
    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 3;
    nint i1 = p.Rect.Dx() * 4;
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        for (nint i = i0; i < i1; i += 4) {
            if (p.Pix[i] != 255) {
                return false;
            }
        }
        i0 += p.Stride;
        i1 += p.Stride;
    }
    return true;
}

// NewRGBA returns a new [RGBA] image with the given bounds.
public static ж<ΔRGBA> NewRGBA(Rectangle r) {
    return Ꮡ(new ΔRGBA(
        Pix: new slice<uint8>(pixelBufferLength(4, r, "RGBA"u8)),
        Stride: 4 * r.Dx(),
        Rect: r
    ));
}

// RGBA64 is an in-memory image whose At method returns [color.RGBA64] values.
[GoType] partial struct RGBA64 {
    // Pix holds the image's pixels, in R, G, B, A order and big-endian format. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*8].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref RGBA64 p) {
    return color.RGBA64Model;
}

[GoRecv] public static Rectangle Bounds(this ref RGBA64 p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref RGBA64 p, nint x, nint y) {
    return p.RGBA64At(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref RGBA64 p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.RGBA64(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 8, i + 8);
    // Small cap improves performance, see https://golang.org/issue/27857
    return new color.RGBA64(
        (uint16)(((uint16)s[0]) << (int)(8) | ((uint16)s[1])),
        (uint16)(((uint16)s[2]) << (int)(8) | ((uint16)s[3])),
        (uint16)(((uint16)s[4]) << (int)(8) | ((uint16)s[5])),
        (uint16)(((uint16)s[6]) << (int)(8) | ((uint16)s[7]))
    );
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref RGBA64 p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 8;
}

[GoRecv] public static void Set(this ref RGBA64 p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.RGBA64Model.Convert(c)._<color.RGBA64>();
    var s = p.Pix.slice(i, i + 8, i + 8);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = ((uint8)(c1.R >> (int)(8)));
    s[1] = ((uint8)c1.R);
    s[2] = ((uint8)(c1.G >> (int)(8)));
    s[3] = ((uint8)c1.G);
    s[4] = ((uint8)(c1.B >> (int)(8)));
    s[5] = ((uint8)c1.B);
    s[6] = ((uint8)(c1.A >> (int)(8)));
    s[7] = ((uint8)c1.A);
}

[GoRecv] public static void SetRGBA64(this ref RGBA64 p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 8, i + 8);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = ((uint8)(c.R >> (int)(8)));
    s[1] = ((uint8)c.R);
    s[2] = ((uint8)(c.G >> (int)(8)));
    s[3] = ((uint8)c.G);
    s[4] = ((uint8)(c.B >> (int)(8)));
    s[5] = ((uint8)c.B);
    s[6] = ((uint8)(c.A >> (int)(8)));
    s[7] = ((uint8)c.A);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref RGBA64 p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new RGBA64(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new RGBA64(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref RGBA64 p) {
    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 6;
    nint i1 = p.Rect.Dx() * 8;
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        for (nint i = i0; i < i1; i += 8) {
            if (p.Pix[i + 0] != 255 || p.Pix[i + 1] != 255) {
                return false;
            }
        }
        i0 += p.Stride;
        i1 += p.Stride;
    }
    return true;
}

// NewRGBA64 returns a new [RGBA64] image with the given bounds.
public static ж<RGBA64> NewRGBA64(Rectangle r) {
    return Ꮡ(new RGBA64(
        Pix: new slice<uint8>(pixelBufferLength(8, r, "RGBA64"u8)),
        Stride: 8 * r.Dx(),
        Rect: r
    ));
}

// NRGBA is an in-memory image whose At method returns [color.NRGBA] values.
[GoType] partial struct NRGBA {
    // Pix holds the image's pixels, in R, G, B, A order. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*4].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref NRGBA p) {
    return color.NRGBAModel;
}

[GoRecv] public static Rectangle Bounds(this ref NRGBA p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref NRGBA p, nint x, nint y) {
    return p.NRGBAAt(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref NRGBA p, nint x, nint y) {
    var (r, g, b, a) = p.NRGBAAt(x, y).RGBA();
    return new color.RGBA64(((uint16)r), ((uint16)g), ((uint16)b), ((uint16)a));
}

[GoRecv] public static color.NRGBA NRGBAAt(this ref NRGBA p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.NRGBA(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    return new color.NRGBA(s[0], s[1], s[2], s[3]);
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref NRGBA p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4;
}

[GoRecv] public static void Set(this ref NRGBA p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.NRGBAModel.Convert(c)._<color.NRGBA>();
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c1.R;
    s[1] = c1.G;
    s[2] = c1.B;
    s[3] = c1.A;
}

[GoRecv] public static void SetRGBA64(this ref NRGBA p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    var (r, g, b, a) = (((uint32)c.R), ((uint32)c.G), ((uint32)c.B), ((uint32)c.A));
    if ((a != 0) && (a != 65535)) {
        r = (r * 65535) / a;
        g = (g * 65535) / a;
        b = (b * 65535) / a;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = ((uint8)(r >> (int)(8)));
    s[1] = ((uint8)(g >> (int)(8)));
    s[2] = ((uint8)(b >> (int)(8)));
    s[3] = ((uint8)(a >> (int)(8)));
}

[GoRecv] public static void SetNRGBA(this ref NRGBA p, nint x, nint y, color.NRGBA c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c.R;
    s[1] = c.G;
    s[2] = c.B;
    s[3] = c.A;
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref NRGBA p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new NRGBA(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new NRGBA(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref NRGBA p) {
    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 3;
    nint i1 = p.Rect.Dx() * 4;
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        for (nint i = i0; i < i1; i += 4) {
            if (p.Pix[i] != 255) {
                return false;
            }
        }
        i0 += p.Stride;
        i1 += p.Stride;
    }
    return true;
}

// NewNRGBA returns a new [NRGBA] image with the given bounds.
public static ж<NRGBA> NewNRGBA(Rectangle r) {
    return Ꮡ(new NRGBA(
        Pix: new slice<uint8>(pixelBufferLength(4, r, "NRGBA"u8)),
        Stride: 4 * r.Dx(),
        Rect: r
    ));
}

// NRGBA64 is an in-memory image whose At method returns [color.NRGBA64] values.
[GoType] partial struct NRGBA64 {
    // Pix holds the image's pixels, in R, G, B, A order and big-endian format. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*8].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref NRGBA64 p) {
    return color.NRGBA64Model;
}

[GoRecv] public static Rectangle Bounds(this ref NRGBA64 p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref NRGBA64 p, nint x, nint y) {
    return p.NRGBA64At(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref NRGBA64 p, nint x, nint y) {
    var (r, g, b, a) = p.NRGBA64At(x, y).RGBA();
    return new color.RGBA64(((uint16)r), ((uint16)g), ((uint16)b), ((uint16)a));
}

[GoRecv] public static color.NRGBA64 NRGBA64At(this ref NRGBA64 p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.NRGBA64(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 8, i + 8);
    // Small cap improves performance, see https://golang.org/issue/27857
    return new color.NRGBA64(
        (uint16)(((uint16)s[0]) << (int)(8) | ((uint16)s[1])),
        (uint16)(((uint16)s[2]) << (int)(8) | ((uint16)s[3])),
        (uint16)(((uint16)s[4]) << (int)(8) | ((uint16)s[5])),
        (uint16)(((uint16)s[6]) << (int)(8) | ((uint16)s[7]))
    );
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref NRGBA64 p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 8;
}

[GoRecv] public static void Set(this ref NRGBA64 p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.NRGBA64Model.Convert(c)._<color.NRGBA64>();
    var s = p.Pix.slice(i, i + 8, i + 8);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = ((uint8)(c1.R >> (int)(8)));
    s[1] = ((uint8)c1.R);
    s[2] = ((uint8)(c1.G >> (int)(8)));
    s[3] = ((uint8)c1.G);
    s[4] = ((uint8)(c1.B >> (int)(8)));
    s[5] = ((uint8)c1.B);
    s[6] = ((uint8)(c1.A >> (int)(8)));
    s[7] = ((uint8)c1.A);
}

[GoRecv] public static void SetRGBA64(this ref NRGBA64 p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    var (r, g, b, a) = (((uint32)c.R), ((uint32)c.G), ((uint32)c.B), ((uint32)c.A));
    if ((a != 0) && (a != 65535)) {
        r = (r * 65535) / a;
        g = (g * 65535) / a;
        b = (b * 65535) / a;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 8, i + 8);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = ((uint8)(r >> (int)(8)));
    s[1] = ((uint8)r);
    s[2] = ((uint8)(g >> (int)(8)));
    s[3] = ((uint8)g);
    s[4] = ((uint8)(b >> (int)(8)));
    s[5] = ((uint8)b);
    s[6] = ((uint8)(a >> (int)(8)));
    s[7] = ((uint8)a);
}

[GoRecv] public static void SetNRGBA64(this ref NRGBA64 p, nint x, nint y, color.NRGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 8, i + 8);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = ((uint8)(c.R >> (int)(8)));
    s[1] = ((uint8)c.R);
    s[2] = ((uint8)(c.G >> (int)(8)));
    s[3] = ((uint8)c.G);
    s[4] = ((uint8)(c.B >> (int)(8)));
    s[5] = ((uint8)c.B);
    s[6] = ((uint8)(c.A >> (int)(8)));
    s[7] = ((uint8)c.A);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref NRGBA64 p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new NRGBA64(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new NRGBA64(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref NRGBA64 p) {
    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 6;
    nint i1 = p.Rect.Dx() * 8;
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        for (nint i = i0; i < i1; i += 8) {
            if (p.Pix[i + 0] != 255 || p.Pix[i + 1] != 255) {
                return false;
            }
        }
        i0 += p.Stride;
        i1 += p.Stride;
    }
    return true;
}

// NewNRGBA64 returns a new [NRGBA64] image with the given bounds.
public static ж<NRGBA64> NewNRGBA64(Rectangle r) {
    return Ꮡ(new NRGBA64(
        Pix: new slice<uint8>(pixelBufferLength(8, r, "NRGBA64"u8)),
        Stride: 8 * r.Dx(),
        Rect: r
    ));
}

// Alpha is an in-memory image whose At method returns [color.Alpha] values.
[GoType] partial struct Alpha {
    // Pix holds the image's pixels, as alpha values. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*1].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref Alpha p) {
    return color.AlphaModel;
}

[GoRecv] public static Rectangle Bounds(this ref Alpha p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref Alpha p, nint x, nint y) {
    return p.AlphaAt(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref Alpha p, nint x, nint y) {
    var a = ((uint16)p.AlphaAt(x, y).A);
    a |= (uint16)(a << (int)(8));
    return new color.RGBA64(a, a, a, a);
}

[GoRecv] public static color.Alpha AlphaAt(this ref Alpha p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.Alpha(nil);
    }
    nint i = p.PixOffset(x, y);
    return new color.Alpha(p.Pix[i]);
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref Alpha p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 1;
}

[GoRecv] public static void Set(this ref Alpha p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = color.AlphaModel.Convert(c)._<color.Alpha>().A;
}

[GoRecv] public static void SetRGBA64(this ref Alpha p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = ((uint8)(c.A >> (int)(8)));
}

[GoRecv] public static void SetAlpha(this ref Alpha p, nint x, nint y, color.Alpha c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = c.A;
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref Alpha p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new Alpha(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new Alpha(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref Alpha p) {
    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 0;
    nint i1 = p.Rect.Dx();
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        for (nint i = i0; i < i1; i++) {
            if (p.Pix[i] != 255) {
                return false;
            }
        }
        i0 += p.Stride;
        i1 += p.Stride;
    }
    return true;
}

// NewAlpha returns a new [Alpha] image with the given bounds.
public static ж<Alpha> NewAlpha(Rectangle r) {
    return Ꮡ(new Alpha(
        Pix: new slice<uint8>(pixelBufferLength(1, r, "Alpha"u8)),
        Stride: 1 * r.Dx(),
        Rect: r
    ));
}

// Alpha16 is an in-memory image whose At method returns [color.Alpha16] values.
[GoType] partial struct Alpha16 {
    // Pix holds the image's pixels, as alpha values in big-endian format. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*2].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref Alpha16 p) {
    return color.Alpha16Model;
}

[GoRecv] public static Rectangle Bounds(this ref Alpha16 p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref Alpha16 p, nint x, nint y) {
    return p.Alpha16At(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref Alpha16 p, nint x, nint y) {
    var a = p.Alpha16At(x, y).A;
    return new color.RGBA64(a, a, a, a);
}

[GoRecv] public static color.Alpha16 Alpha16At(this ref Alpha16 p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.Alpha16(nil);
    }
    nint i = p.PixOffset(x, y);
    return new color.Alpha16((uint16)(((uint16)p.Pix[i + 0]) << (int)(8) | ((uint16)p.Pix[i + 1])));
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref Alpha16 p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 2;
}

[GoRecv] public static void Set(this ref Alpha16 p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.Alpha16Model.Convert(c)._<color.Alpha16>();
    p.Pix[i + 0] = ((uint8)(c1.A >> (int)(8)));
    p.Pix[i + 1] = ((uint8)c1.A);
}

[GoRecv] public static void SetRGBA64(this ref Alpha16 p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i + 0] = ((uint8)(c.A >> (int)(8)));
    p.Pix[i + 1] = ((uint8)c.A);
}

[GoRecv] public static void SetAlpha16(this ref Alpha16 p, nint x, nint y, color.Alpha16 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i + 0] = ((uint8)(c.A >> (int)(8)));
    p.Pix[i + 1] = ((uint8)c.A);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref Alpha16 p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new Alpha16(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new Alpha16(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref Alpha16 p) {
    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 0;
    nint i1 = p.Rect.Dx() * 2;
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        for (nint i = i0; i < i1; i += 2) {
            if (p.Pix[i + 0] != 255 || p.Pix[i + 1] != 255) {
                return false;
            }
        }
        i0 += p.Stride;
        i1 += p.Stride;
    }
    return true;
}

// NewAlpha16 returns a new [Alpha16] image with the given bounds.
public static ж<Alpha16> NewAlpha16(Rectangle r) {
    return Ꮡ(new Alpha16(
        Pix: new slice<uint8>(pixelBufferLength(2, r, "Alpha16"u8)),
        Stride: 2 * r.Dx(),
        Rect: r
    ));
}

// Gray is an in-memory image whose At method returns [color.Gray] values.
[GoType] partial struct Gray {
    // Pix holds the image's pixels, as gray values. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*1].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref Gray p) {
    return color.GrayModel;
}

[GoRecv] public static Rectangle Bounds(this ref Gray p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref Gray p, nint x, nint y) {
    return p.GrayAt(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref Gray p, nint x, nint y) {
    var gray = ((uint16)p.GrayAt(x, y).Y);
    gray |= (uint16)(gray << (int)(8));
    return new color.RGBA64(gray, gray, gray, 65535);
}

[GoRecv] public static color.Gray GrayAt(this ref Gray p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.Gray(nil);
    }
    nint i = p.PixOffset(x, y);
    return new color.Gray(p.Pix[i]);
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref Gray p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 1;
}

[GoRecv] public static void Set(this ref Gray p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = color.GrayModel.Convert(c)._<color.Gray>().Y;
}

[GoRecv] public static void SetRGBA64(this ref Gray p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    // This formula is the same as in color.grayModel.
    var gray = (19595 * ((uint32)c.R) + 38470 * ((uint32)c.G) + 7471 * ((uint32)c.B) + 1 << (int)(15)) >> (int)(24);
    nint i = p.PixOffset(x, y);
    p.Pix[i] = ((uint8)gray);
}

[GoRecv] public static void SetGray(this ref Gray p, nint x, nint y, color.Gray c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = c.Y;
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref Gray p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new Gray(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new Gray(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref Gray p) {
    return true;
}

// NewGray returns a new [Gray] image with the given bounds.
public static ж<Gray> NewGray(Rectangle r) {
    return Ꮡ(new Gray(
        Pix: new slice<uint8>(pixelBufferLength(1, r, "Gray"u8)),
        Stride: 1 * r.Dx(),
        Rect: r
    ));
}

// Gray16 is an in-memory image whose At method returns [color.Gray16] values.
[GoType] partial struct Gray16 {
    // Pix holds the image's pixels, as gray values in big-endian format. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*2].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref Gray16 p) {
    return color.Gray16Model;
}

[GoRecv] public static Rectangle Bounds(this ref Gray16 p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref Gray16 p, nint x, nint y) {
    return p.Gray16At(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref Gray16 p, nint x, nint y) {
    var gray = p.Gray16At(x, y).Y;
    return new color.RGBA64(gray, gray, gray, 65535);
}

[GoRecv] public static color.Gray16 Gray16At(this ref Gray16 p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.Gray16(nil);
    }
    nint i = p.PixOffset(x, y);
    return new color.Gray16((uint16)(((uint16)p.Pix[i + 0]) << (int)(8) | ((uint16)p.Pix[i + 1])));
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref Gray16 p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 2;
}

[GoRecv] public static void Set(this ref Gray16 p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.Gray16Model.Convert(c)._<color.Gray16>();
    p.Pix[i + 0] = ((uint8)(c1.Y >> (int)(8)));
    p.Pix[i + 1] = ((uint8)c1.Y);
}

[GoRecv] public static void SetRGBA64(this ref Gray16 p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    // This formula is the same as in color.gray16Model.
    var gray = (19595 * ((uint32)c.R) + 38470 * ((uint32)c.G) + 7471 * ((uint32)c.B) + 1 << (int)(15)) >> (int)(16);
    nint i = p.PixOffset(x, y);
    p.Pix[i + 0] = ((uint8)(gray >> (int)(8)));
    p.Pix[i + 1] = ((uint8)gray);
}

[GoRecv] public static void SetGray16(this ref Gray16 p, nint x, nint y, color.Gray16 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i + 0] = ((uint8)(c.Y >> (int)(8)));
    p.Pix[i + 1] = ((uint8)c.Y);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref Gray16 p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new Gray16(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new Gray16(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref Gray16 p) {
    return true;
}

// NewGray16 returns a new [Gray16] image with the given bounds.
public static ж<Gray16> NewGray16(Rectangle r) {
    return Ꮡ(new Gray16(
        Pix: new slice<uint8>(pixelBufferLength(2, r, "Gray16"u8)),
        Stride: 2 * r.Dx(),
        Rect: r
    ));
}

// CMYK is an in-memory image whose At method returns [color.CMYK] values.
[GoType] partial struct CMYK {
    // Pix holds the image's pixels, in C, M, Y, K order. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*4].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
}

[GoRecv] public static color.Model ColorModel(this ref CMYK p) {
    return color.CMYKModel;
}

[GoRecv] public static Rectangle Bounds(this ref CMYK p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref CMYK p, nint x, nint y) {
    return p.CMYKAt(x, y);
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref CMYK p, nint x, nint y) {
    var (r, g, b, a) = p.CMYKAt(x, y).RGBA();
    return new color.RGBA64(((uint16)r), ((uint16)g), ((uint16)b), ((uint16)a));
}

[GoRecv] public static color.CMYK CMYKAt(this ref CMYK p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.CMYK(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    return new color.CMYK(s[0], s[1], s[2], s[3]);
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref CMYK p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4;
}

[GoRecv] public static void Set(this ref CMYK p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.CMYKModel.Convert(c)._<color.CMYK>();
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c1.C;
    s[1] = c1.M;
    s[2] = c1.Y;
    s[3] = c1.K;
}

[GoRecv] public static void SetRGBA64(this ref CMYK p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    var (cc, mm, yy, kk) = color.RGBToCMYK(((uint8)(c.R >> (int)(8))), ((uint8)(c.G >> (int)(8))), ((uint8)(c.B >> (int)(8))));
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = cc;
    s[1] = mm;
    s[2] = yy;
    s[3] = kk;
}

[GoRecv] public static void SetCMYK(this ref CMYK p, nint x, nint y, color.CMYK c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4);
    // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c.C;
    s[1] = c.M;
    s[2] = c.Y;
    s[3] = c.K;
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref CMYK p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new CMYK(nil);
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new CMYK(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: r
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref CMYK p) {
    return true;
}

// NewCMYK returns a new CMYK image with the given bounds.
public static ж<CMYK> NewCMYK(Rectangle r) {
    return Ꮡ(new CMYK(
        Pix: new slice<uint8>(pixelBufferLength(4, r, "CMYK"u8)),
        Stride: 4 * r.Dx(),
        Rect: r
    ));
}

// Paletted is an in-memory image of uint8 indices into a given palette.
[GoType] partial struct Paletted {
    // Pix holds the image's pixels, as palette indices. The pixel at
    // (x, y) starts at Pix[(y-Rect.Min.Y)*Stride + (x-Rect.Min.X)*1].
    public slice<uint8> Pix;
    // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
    public nint Stride;
    // Rect is the image's bounds.
    public Rectangle Rect;
    // Palette is the image's palette.
    public image.color_package.Palette Palette;
}

[GoRecv] public static color.Model ColorModel(this ref Paletted p) {
    return p.Palette;
}

[GoRecv] public static Rectangle Bounds(this ref Paletted p) {
    return p.Rect;
}

[GoRecv] public static color.Color At(this ref Paletted p, nint x, nint y) {
    if (len(p.Palette) == 0) {
        return default!;
    }
    if (!(new Point(x, y).In(p.Rect))) {
        return p.Palette[0];
    }
    nint i = p.PixOffset(x, y);
    return p.Palette[p.Pix[i]];
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref Paletted p, nint x, nint y) {
    if (len(p.Palette) == 0) {
        return new color.RGBA64(nil);
    }
    var c = ((color.Color)default!);
    if (!(new Point(x, y).In(p.Rect))){
        c = p.Palette[0];
    } else {
        nint i = p.PixOffset(x, y);
        c = p.Palette[p.Pix[i]];
    }
    var (r, g, b, a) = c.RGBA();
    return new color.RGBA64(
        ((uint16)r),
        ((uint16)g),
        ((uint16)b),
        ((uint16)a)
    );
}

// PixOffset returns the index of the first element of Pix that corresponds to
// the pixel at (x, y).
[GoRecv] public static nint PixOffset(this ref Paletted p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 1;
}

[GoRecv] public static void Set(this ref Paletted p, nint x, nint y, color.Color c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = ((uint8)p.Palette.Index(c));
}

[GoRecv] public static void SetRGBA64(this ref Paletted p, nint x, nint y, color.RGBA64 c) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = ((uint8)p.Palette.Index(c));
}

[GoRecv] public static uint8 ColorIndexAt(this ref Paletted p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return 0;
    }
    nint i = p.PixOffset(x, y);
    return p.Pix[i];
}

[GoRecv] public static void SetColorIndex(this ref Paletted p, nint x, nint y, uint8 index) {
    if (!(new Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    p.Pix[i] = index;
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
[GoRecv] public static Image SubImage(this ref Paletted p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new Paletted(
            Palette: p.Palette
        );
    }
    nint i = p.PixOffset(r.Min.X, r.Min.Y);
    return new Paletted(
        Pix: p.Pix[(int)(i)..],
        Stride: p.Stride,
        Rect: p.Rect.Intersect(r),
        Palette: p.Palette
    );
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref Paletted p) {
    array<bool> present = new(256);
    nint i0 = 0;
    nint i1 = p.Rect.Dx();
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        foreach (var (_, c) in p.Pix[(int)(i0)..(int)(i1)]) {
            present[c] = true;
        }
        i0 += p.Stride;
        i1 += p.Stride;
    }
    foreach (var (i, c) in p.Palette) {
        if (!present[i]) {
            continue;
        }
        var (_, _, _, a) = c.RGBA();
        if (a != 65535) {
            return false;
        }
    }
    return true;
}

// NewPaletted returns a new [Paletted] image with the given width, height and
// palette.
public static ж<Paletted> NewPaletted(Rectangle r, color.Palette p) {
    return Ꮡ(new Paletted(
        Pix: new slice<uint8>(pixelBufferLength(1, r, "Paletted"u8)),
        Stride: 1 * r.Dx(),
        Rect: r,
        Palette: p
    ));
}

} // end image_package
