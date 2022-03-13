// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package color implements a basic color library.

// package color -- go2cs converted at 2022 March 13 06:43:45 UTC
// import "image/color" ==> using color = go.image.color_package
// Original source: C:\Program Files\Go\src\image\color\color.go
namespace go.image;

using System;
public static partial class color_package {

// Color can convert itself to alpha-premultiplied 16-bits per channel RGBA.
// The conversion may be lossy.
public partial interface Color {
    (uint, uint, uint, uint) RGBA();
}

// RGBA represents a traditional 32-bit alpha-premultiplied color, having 8
// bits for each of red, green, blue and alpha.
//
// An alpha-premultiplied color component C has been scaled by alpha (A), so
// has valid values 0 <= C <= A.
public partial struct RGBA {
    public byte R;
    public byte G;
    public byte B;
    public byte A;
}

public static (uint, uint, uint, uint) RGBA(this RGBA c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    r = uint32(c.R);
    r |= r << 8;
    g = uint32(c.G);
    g |= g << 8;
    b = uint32(c.B);
    b |= b << 8;
    a = uint32(c.A);
    a |= a << 8;
    return ;
}

// RGBA64 represents a 64-bit alpha-premultiplied color, having 16 bits for
// each of red, green, blue and alpha.
//
// An alpha-premultiplied color component C has been scaled by alpha (A), so
// has valid values 0 <= C <= A.
public partial struct RGBA64 {
    public ushort R;
    public ushort G;
    public ushort B;
    public ushort A;
}

public static (uint, uint, uint, uint) RGBA(this RGBA64 c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    return (uint32(c.R), uint32(c.G), uint32(c.B), uint32(c.A));
}

// NRGBA represents a non-alpha-premultiplied 32-bit color.
public partial struct NRGBA {
    public byte R;
    public byte G;
    public byte B;
    public byte A;
}

public static (uint, uint, uint, uint) RGBA(this NRGBA c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    r = uint32(c.R);
    r |= r << 8;
    r *= uint32(c.A);
    r /= 0xff;
    g = uint32(c.G);
    g |= g << 8;
    g *= uint32(c.A);
    g /= 0xff;
    b = uint32(c.B);
    b |= b << 8;
    b *= uint32(c.A);
    b /= 0xff;
    a = uint32(c.A);
    a |= a << 8;
    return ;
}

// NRGBA64 represents a non-alpha-premultiplied 64-bit color,
// having 16 bits for each of red, green, blue and alpha.
public partial struct NRGBA64 {
    public ushort R;
    public ushort G;
    public ushort B;
    public ushort A;
}

public static (uint, uint, uint, uint) RGBA(this NRGBA64 c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    r = uint32(c.R);
    r *= uint32(c.A);
    r /= 0xffff;
    g = uint32(c.G);
    g *= uint32(c.A);
    g /= 0xffff;
    b = uint32(c.B);
    b *= uint32(c.A);
    b /= 0xffff;
    a = uint32(c.A);
    return ;
}

// Alpha represents an 8-bit alpha color.
public partial struct Alpha {
    public byte A;
}

public static (uint, uint, uint, uint) RGBA(this Alpha c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    a = uint32(c.A);
    a |= a << 8;
    return (a, a, a, a);
}

// Alpha16 represents a 16-bit alpha color.
public partial struct Alpha16 {
    public ushort A;
}

public static (uint, uint, uint, uint) RGBA(this Alpha16 c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    a = uint32(c.A);
    return (a, a, a, a);
}

// Gray represents an 8-bit grayscale color.
public partial struct Gray {
    public byte Y;
}

public static (uint, uint, uint, uint) RGBA(this Gray c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    var y = uint32(c.Y);
    y |= y << 8;
    return (y, y, y, 0xffff);
}

// Gray16 represents a 16-bit grayscale color.
public partial struct Gray16 {
    public ushort Y;
}

public static (uint, uint, uint, uint) RGBA(this Gray16 c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;

    var y = uint32(c.Y);
    return (y, y, y, 0xffff);
}

// Model can convert any Color to one from its own color model. The conversion
// may be lossy.
public partial interface Model {
    Color Convert(Color c);
}

// ModelFunc returns a Model that invokes f to implement the conversion.
public static Model ModelFunc(Func<Color, Color> f) { 
    // Note: using *modelFunc as the implementation
    // means that callers can still use comparisons
    // like m == RGBAModel. This is not possible if
    // we use the func value directly, because funcs
    // are no longer comparable.
    return addr(new modelFunc(f));
}

private partial struct modelFunc {
    public Func<Color, Color> f;
}

private static Color Convert(this ptr<modelFunc> _addr_m, Color c) {
    ref modelFunc m = ref _addr_m.val;

    return m.f(c);
}

// Models for the standard color types.
public static Model RGBAModel = Model.As(ModelFunc(rgbaModel))!;public static Model RGBA64Model = Model.As(ModelFunc(rgba64Model))!;public static Model NRGBAModel = Model.As(ModelFunc(nrgbaModel))!;public static Model NRGBA64Model = Model.As(ModelFunc(nrgba64Model))!;public static Model AlphaModel = Model.As(ModelFunc(alphaModel))!;public static Model Alpha16Model = Model.As(ModelFunc(alpha16Model))!;public static Model GrayModel = Model.As(ModelFunc(grayModel))!;public static Model Gray16Model = Model.As(ModelFunc(gray16Model))!;

private static Color rgbaModel(Color c) {
    {
        RGBA (_, ok) = c._<RGBA>();

        if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    return new RGBA(uint8(r>>8),uint8(g>>8),uint8(b>>8),uint8(a>>8));
}

private static Color rgba64Model(Color c) {
    {
        RGBA64 (_, ok) = c._<RGBA64>();

        if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    return new RGBA64(uint16(r),uint16(g),uint16(b),uint16(a));
}

private static Color nrgbaModel(Color c) {
    {
        NRGBA (_, ok) = c._<NRGBA>();

        if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    if (a == 0xffff) {
        return new NRGBA(uint8(r>>8),uint8(g>>8),uint8(b>>8),0xff);
    }
    if (a == 0) {
        return new NRGBA(0,0,0,0);
    }
    r = (r * 0xffff) / a;
    g = (g * 0xffff) / a;
    b = (b * 0xffff) / a;
    return new NRGBA(uint8(r>>8),uint8(g>>8),uint8(b>>8),uint8(a>>8));
}

private static Color nrgba64Model(Color c) {
    {
        NRGBA64 (_, ok) = c._<NRGBA64>();

        if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    if (a == 0xffff) {
        return new NRGBA64(uint16(r),uint16(g),uint16(b),0xffff);
    }
    if (a == 0) {
        return new NRGBA64(0,0,0,0);
    }
    r = (r * 0xffff) / a;
    g = (g * 0xffff) / a;
    b = (b * 0xffff) / a;
    return new NRGBA64(uint16(r),uint16(g),uint16(b),uint16(a));
}

private static Color alphaModel(Color c) {
    {
        Alpha (_, ok) = c._<Alpha>();

        if (ok) {
            return c;
        }
    }
    var (_, _, _, a) = c.RGBA();
    return new Alpha(uint8(a>>8));
}

private static Color alpha16Model(Color c) {
    {
        Alpha16 (_, ok) = c._<Alpha16>();

        if (ok) {
            return c;
        }
    }
    var (_, _, _, a) = c.RGBA();
    return new Alpha16(uint16(a));
}

private static Color grayModel(Color c) {
    {
        Gray (_, ok) = c._<Gray>();

        if (ok) {
            return c;
        }
    }
    var (r, g, b, _) = c.RGBA(); 

    // These coefficients (the fractions 0.299, 0.587 and 0.114) are the same
    // as those given by the JFIF specification and used by func RGBToYCbCr in
    // ycbcr.go.
    //
    // Note that 19595 + 38470 + 7471 equals 65536.
    //
    // The 24 is 16 + 8. The 16 is the same as used in RGBToYCbCr. The 8 is
    // because the return value is 8 bit color, not 16 bit color.
    nint y = (19595 * r + 38470 * g + 7471 * b + 1 << 15) >> 24;

    return new Gray(uint8(y));
}

private static Color gray16Model(Color c) {
    {
        Gray16 (_, ok) = c._<Gray16>();

        if (ok) {
            return c;
        }
    }
    var (r, g, b, _) = c.RGBA(); 

    // These coefficients (the fractions 0.299, 0.587 and 0.114) are the same
    // as those given by the JFIF specification and used by func RGBToYCbCr in
    // ycbcr.go.
    //
    // Note that 19595 + 38470 + 7471 equals 65536.
    nint y = (19595 * r + 38470 * g + 7471 * b + 1 << 15) >> 16;

    return new Gray16(uint16(y));
}

// Palette is a palette of colors.
public partial struct Palette { // : slice<Color>
}

// Convert returns the palette color closest to c in Euclidean R,G,B space.
public static Color Convert(this Palette p, Color c) {
    if (len(p) == 0) {
        return null;
    }
    return p[p.Index(c)];
}

// Index returns the index of the palette color closest to c in Euclidean
// R,G,B,A space.
public static nint Index(this Palette p, Color c) { 
    // A batch version of this computation is in image/draw/draw.go.

    var (cr, cg, cb, ca) = c.RGBA();
    nint ret = 0;
    var bestSum = uint32(1 << 32 - 1);
    foreach (var (i, v) in p) {
        var (vr, vg, vb, va) = v.RGBA();
        var sum = sqDiff(cr, vr) + sqDiff(cg, vg) + sqDiff(cb, vb) + sqDiff(ca, va);
        if (sum < bestSum) {
            if (sum == 0) {
                return i;
            }
            (ret, bestSum) = (i, sum);
        }
    }    return ret;
}

// sqDiff returns the squared-difference of x and y, shifted by 2 so that
// adding four of those won't overflow a uint32.
//
// x and y are both assumed to be in the range [0, 0xffff].
private static uint sqDiff(uint x, uint y) { 
    // The canonical code of this function looks as follows:
    //
    //    var d uint32
    //    if x > y {
    //        d = x - y
    //    } else {
    //        d = y - x
    //    }
    //    return (d * d) >> 2
    //
    // Language spec guarantees the following properties of unsigned integer
    // values operations with respect to overflow/wrap around:
    //
    // > For unsigned integer values, the operations +, -, *, and << are
    // > computed modulo 2n, where n is the bit width of the unsigned
    // > integer's type. Loosely speaking, these unsigned integer operations
    // > discard high bits upon overflow, and programs may rely on ``wrap
    // > around''.
    //
    // Considering these properties and the fact that this function is
    // called in the hot paths (x,y loops), it is reduced to the below code
    // which is slightly faster. See TestSqDiff for correctness check.
    var d = x - y;
    return (d * d) >> 2;
}

// Standard colors.
public static Gray16 Black = new Gray16(0);public static Gray16 White = new Gray16(0xffff);public static Alpha16 Transparent = new Alpha16(0);public static Alpha16 Opaque = new Alpha16(0xffff);

} // end color_package
