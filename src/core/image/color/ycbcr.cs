// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

partial class color_package {

// RGBToYCbCr converts an RGB triple to a Y'CbCr triple.
public static (uint8, uint8, uint8) RGBToYCbCr(uint8 r, uint8 g, uint8 b) {
    // The JFIF specification says:
    //	Y' =  0.2990*R + 0.5870*G + 0.1140*B
    //	Cb = -0.1687*R - 0.3313*G + 0.5000*B + 128
    //	Cr =  0.5000*R - 0.4187*G - 0.0813*B + 128
    // https://www.w3.org/Graphics/JPEG/jfif3.pdf says Y but means Y'.
    var r1 = ((int32)r);
    var g1 = ((int32)g);
    var b1 = ((int32)b);
    // yy is in range [0,0xff].
    //
    // Note that 19595 + 38470 + 7471 equals 65536.
    var yy = (19595 * r1 + 38470 * g1 + 7471 * b1 + 1 << (int)(15)) >> (int)(16);
    // The bit twiddling below is equivalent to
    //
    // cb := (-11056*r1 - 21712*g1 + 32768*b1 + 257<<15) >> 16
    // if cb < 0 {
    //     cb = 0
    // } else if cb > 0xff {
    //     cb = ^int32(0)
    // }
    //
    // but uses fewer branches and is faster.
    // Note that the uint8 type conversion in the return
    // statement will convert ^int32(0) to 0xff.
    // The code below to compute cr uses a similar pattern.
    //
    // Note that -11056 - 21712 + 32768 equals 0.
    var cb = -11056 * r1 - 21712 * g1 + 32768 * b1 + 257 << (int)(15);
    if ((uint32)(((uint32)cb) & (nint)4278190080L) == 0){
        cb >>= (UntypedInt)(16);
    } else {
        cb = ~(cb >> (int)(31));
    }
    // Note that 32768 - 27440 - 5328 equals 0.
    var cr = 32768 * r1 - 27440 * g1 - 5328 * b1 + 257 << (int)(15);
    if ((uint32)(((uint32)cr) & (nint)4278190080L) == 0){
        cr >>= (UntypedInt)(16);
    } else {
        cr = ~(cr >> (int)(31));
    }
    return (((uint8)yy), ((uint8)cb), ((uint8)cr));
}

// YCbCrToRGB converts a Y'CbCr triple to an RGB triple.
public static (uint8, uint8, uint8) YCbCrToRGB(uint8 y, uint8 cb, uint8 cr) {
    // The JFIF specification says:
    //	R = Y' + 1.40200*(Cr-128)
    //	G = Y' - 0.34414*(Cb-128) - 0.71414*(Cr-128)
    //	B = Y' + 1.77200*(Cb-128)
    // https://www.w3.org/Graphics/JPEG/jfif3.pdf says Y but means Y'.
    //
    // Those formulae use non-integer multiplication factors. When computing,
    // integer math is generally faster than floating point math. We multiply
    // all of those factors by 1<<16 and round to the nearest integer:
    //	 91881 = roundToNearestInteger(1.40200 * 65536).
    //	 22554 = roundToNearestInteger(0.34414 * 65536).
    //	 46802 = roundToNearestInteger(0.71414 * 65536).
    //	116130 = roundToNearestInteger(1.77200 * 65536).
    //
    // Adding a rounding adjustment in the range [0, 1<<16-1] and then shifting
    // right by 16 gives us an integer math version of the original formulae.
    //	R = (65536*Y' +  91881 *(Cr-128)                  + adjustment) >> 16
    //	G = (65536*Y' -  22554 *(Cb-128) - 46802*(Cr-128) + adjustment) >> 16
    //	B = (65536*Y' + 116130 *(Cb-128)                  + adjustment) >> 16
    // A constant rounding adjustment of 1<<15, one half of 1<<16, would mean
    // round-to-nearest when dividing by 65536 (shifting right by 16).
    // Similarly, a constant rounding adjustment of 0 would mean round-down.
    //
    // Defining YY1 = 65536*Y' + adjustment simplifies the formulae and
    // requires fewer CPU operations:
    //	R = (YY1 +  91881 *(Cr-128)                 ) >> 16
    //	G = (YY1 -  22554 *(Cb-128) - 46802*(Cr-128)) >> 16
    //	B = (YY1 + 116130 *(Cb-128)                 ) >> 16
    //
    // The inputs (y, cb, cr) are 8 bit color, ranging in [0x00, 0xff]. In this
    // function, the output is also 8 bit color, but in the related YCbCr.RGBA
    // method, below, the output is 16 bit color, ranging in [0x0000, 0xffff].
    // Outputting 16 bit color simply requires changing the 16 to 8 in the "R =
    // etc >> 16" equation, and likewise for G and B.
    //
    // As mentioned above, a constant rounding adjustment of 1<<15 is a natural
    // choice, but there is an additional constraint: if c0 := YCbCr{Y: y, Cb:
    // 0x80, Cr: 0x80} and c1 := Gray{Y: y} then c0.RGBA() should equal
    // c1.RGBA(). Specifically, if y == 0 then "R = etc >> 8" should yield
    // 0x0000 and if y == 0xff then "R = etc >> 8" should yield 0xffff. If we
    // used a constant rounding adjustment of 1<<15, then it would yield 0x0080
    // and 0xff80 respectively.
    //
    // Note that when cb == 0x80 and cr == 0x80 then the formulae collapse to:
    //	R = YY1 >> n
    //	G = YY1 >> n
    //	B = YY1 >> n
    // where n is 16 for this function (8 bit color output) and 8 for the
    // YCbCr.RGBA method (16 bit color output).
    //
    // The solution is to make the rounding adjustment non-constant, and equal
    // to 257*Y', which ranges over [0, 1<<16-1] as Y' ranges over [0, 255].
    // YY1 is then defined as:
    //	YY1 = 65536*Y' + 257*Y'
    // or equivalently:
    //	YY1 = Y' * 0x10101
    var yy1 = ((int32)y) * 65793;
    var cb1 = ((int32)cb) - 128;
    var cr1 = ((int32)cr) - 128;
    // The bit twiddling below is equivalent to
    //
    // r := (yy1 + 91881*cr1) >> 16
    // if r < 0 {
    //     r = 0
    // } else if r > 0xff {
    //     r = ^int32(0)
    // }
    //
    // but uses fewer branches and is faster.
    // Note that the uint8 type conversion in the return
    // statement will convert ^int32(0) to 0xff.
    // The code below to compute g and b uses a similar pattern.
    var r = yy1 + 91881 * cr1;
    if ((uint32)(((uint32)r) & (nint)4278190080L) == 0){
        r >>= (UntypedInt)(16);
    } else {
        r = ~(r >> (int)(31));
    }
    var g = yy1 - 22554 * cb1 - 46802 * cr1;
    if ((uint32)(((uint32)g) & (nint)4278190080L) == 0){
        g >>= (UntypedInt)(16);
    } else {
        g = ~(g >> (int)(31));
    }
    var b = yy1 + 116130 * cb1;
    if ((uint32)(((uint32)b) & (nint)4278190080L) == 0){
        b >>= (UntypedInt)(16);
    } else {
        b = ~(b >> (int)(31));
    }
    return (((uint8)r), ((uint8)g), ((uint8)b));
}

// YCbCr represents a fully opaque 24-bit Y'CbCr color, having 8 bits each for
// one luma and two chroma components.
//
// JPEG, VP8, the MPEG family and other codecs use this color model. Such
// codecs often use the terms YUV and Y'CbCr interchangeably, but strictly
// speaking, the term YUV applies only to analog video signals, and Y' (luma)
// is Y (luminance) after applying gamma correction.
//
// Conversion between RGB and Y'CbCr is lossy and there are multiple, slightly
// different formulae for converting between the two. This package follows
// the JFIF specification at https://www.w3.org/Graphics/JPEG/jfif3.pdf.
[GoType] partial struct YCbCr {
    public uint8 Y;
    public uint8 Cb;
    public uint8 Cr;
}

public static (uint32, uint32, uint32, uint32) RGBA(this YCbCr c) {
    // This code is a copy of the YCbCrToRGB function above, except that it
    // returns values in the range [0, 0xffff] instead of [0, 0xff]. There is a
    // subtle difference between doing this and having YCbCr satisfy the Color
    // interface by first converting to an RGBA. The latter loses some
    // information by going to and from 8 bits per channel.
    //
    // For example, this code:
    //	const y, cb, cr = 0x7f, 0x7f, 0x7f
    //	r, g, b := color.YCbCrToRGB(y, cb, cr)
    //	r0, g0, b0, _ := color.YCbCr{y, cb, cr}.RGBA()
    //	r1, g1, b1, _ := color.RGBA{r, g, b, 0xff}.RGBA()
    //	fmt.Printf("0x%04x 0x%04x 0x%04x\n", r0, g0, b0)
    //	fmt.Printf("0x%04x 0x%04x 0x%04x\n", r1, g1, b1)
    // prints:
    //	0x7e18 0x808d 0x7db9
    //	0x7e7e 0x8080 0x7d7d
    var yy1 = ((int32)c.Y) * 65793;
    var cb1 = ((int32)c.Cb) - 128;
    var cr1 = ((int32)c.Cr) - 128;
    // The bit twiddling below is equivalent to
    //
    // r := (yy1 + 91881*cr1) >> 8
    // if r < 0 {
    //     r = 0
    // } else if r > 0xff {
    //     r = 0xffff
    // }
    //
    // but uses fewer branches and is faster.
    // The code below to compute g and b uses a similar pattern.
    var r = yy1 + 91881 * cr1;
    if ((uint32)(((uint32)r) & (nint)4278190080L) == 0){
        r >>= (UntypedInt)(8);
    } else {
        r = (int32)(~(r >> (int)(31)) & 65535);
    }
    var g = yy1 - 22554 * cb1 - 46802 * cr1;
    if ((uint32)(((uint32)g) & (nint)4278190080L) == 0){
        g >>= (UntypedInt)(8);
    } else {
        g = (int32)(~(g >> (int)(31)) & 65535);
    }
    var b = yy1 + 116130 * cb1;
    if ((uint32)(((uint32)b) & (nint)4278190080L) == 0){
        b >>= (UntypedInt)(8);
    } else {
        b = (int32)(~(b >> (int)(31)) & 65535);
    }
    return (((uint32)r), ((uint32)g), ((uint32)b), 65535);
}

// YCbCrModel is the [Model] for Y'CbCr colors.
public static Model YCbCrModel = ModelFunc(yCbCrModel);

internal static Color yCbCrModel(Color c) {
    {
        var (_, ok) = c._<YCbCr>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, _) = c.RGBA();
    var (y, u, v) = RGBToYCbCr(((uint8)(r >> (int)(8))), ((uint8)(g >> (int)(8))), ((uint8)(b >> (int)(8))));
    return new YCbCr(y, u, v);
}

// NYCbCrA represents a non-alpha-premultiplied Y'CbCr-with-alpha color, having
// 8 bits each for one luma, two chroma and one alpha component.
[GoType] partial struct NYCbCrA {
    public partial ref YCbCr YCbCr { get; }
    public uint8 A;
}

public static (uint32, uint32, uint32, uint32) RGBA(this NYCbCrA c) {
    // The first part of this method is the same as YCbCr.RGBA.
    var yy1 = ((int32)c.Y) * 65793;
    var cb1 = ((int32)c.Cb) - 128;
    var cr1 = ((int32)c.Cr) - 128;
    // The bit twiddling below is equivalent to
    //
    // r := (yy1 + 91881*cr1) >> 8
    // if r < 0 {
    //     r = 0
    // } else if r > 0xff {
    //     r = 0xffff
    // }
    //
    // but uses fewer branches and is faster.
    // The code below to compute g and b uses a similar pattern.
    var r = yy1 + 91881 * cr1;
    if ((uint32)(((uint32)r) & (nint)4278190080L) == 0){
        r >>= (UntypedInt)(8);
    } else {
        r = (int32)(~(r >> (int)(31)) & 65535);
    }
    var g = yy1 - 22554 * cb1 - 46802 * cr1;
    if ((uint32)(((uint32)g) & (nint)4278190080L) == 0){
        g >>= (UntypedInt)(8);
    } else {
        g = (int32)(~(g >> (int)(31)) & 65535);
    }
    var b = yy1 + 116130 * cb1;
    if ((uint32)(((uint32)b) & (nint)4278190080L) == 0){
        b >>= (UntypedInt)(8);
    } else {
        b = (int32)(~(b >> (int)(31)) & 65535);
    }
    // The second part of this method applies the alpha.
    var a = ((uint32)c.A) * 257;
    return (((uint32)r) * a / 65535, ((uint32)g) * a / 65535, ((uint32)b) * a / 65535, a);
}

// NYCbCrAModel is the [Model] for non-alpha-premultiplied Y'CbCr-with-alpha
// colors.
public static Model NYCbCrAModel = ModelFunc(nYCbCrAModel);

internal static Color nYCbCrAModel(Color c) {
    switch (c.type()) {
    case NYCbCrA c: {
        return c;
    }
    case YCbCr c: {
        return new NYCbCrA(c, 255);
    }}
    var (r, g, b, a) = c.RGBA();
    // Convert from alpha-premultiplied to non-alpha-premultiplied.
    if (a != 0) {
        r = (r * 65535) / a;
        g = (g * 65535) / a;
        b = (b * 65535) / a;
    }
    var (y, u, v) = RGBToYCbCr(((uint8)(r >> (int)(8))), ((uint8)(g >> (int)(8))), ((uint8)(b >> (int)(8))));
    return new NYCbCrA(new YCbCr(Y: y, Cb: u, Cr: v), ((uint8)(a >> (int)(8))));
}

// RGBToCMYK converts an RGB triple to a CMYK quadruple.
public static (uint8, uint8, uint8, uint8) RGBToCMYK(uint8 r, uint8 g, uint8 b) {
    var rr = ((uint32)r);
    var gg = ((uint32)g);
    var bb = ((uint32)b);
    var w = rr;
    if (w < gg) {
        w = gg;
    }
    if (w < bb) {
        w = bb;
    }
    if (w == 0) {
        return (0, 0, 0, 255);
    }
    var c = (w - rr) * 255 / w;
    var m = (w - gg) * 255 / w;
    var y = (w - bb) * 255 / w;
    return (((uint8)c), ((uint8)m), ((uint8)y), ((uint8)(255 - w)));
}

// CMYKToRGB converts a [CMYK] quadruple to an RGB triple.
public static (uint8, uint8, uint8) CMYKToRGB(uint8 c, uint8 m, uint8 y, uint8 k) {
    var w = 65535 - ((uint32)k) * 257;
    var r = (65535 - ((uint32)c) * 257) * w / 65535;
    var g = (65535 - ((uint32)m) * 257) * w / 65535;
    var b = (65535 - ((uint32)y) * 257) * w / 65535;
    return (((uint8)(r >> (int)(8))), ((uint8)(g >> (int)(8))), ((uint8)(b >> (int)(8))));
}

// CMYK represents a fully opaque CMYK color, having 8 bits for each of cyan,
// magenta, yellow and black.
//
// It is not associated with any particular color profile.
[GoType] partial struct CMYK {
    public uint8 C;
    public uint8 M;
    public uint8 Y;
    public uint8 K;
}

public static (uint32, uint32, uint32, uint32) RGBA(this CMYK c) {
    // This code is a copy of the CMYKToRGB function above, except that it
    // returns values in the range [0, 0xffff] instead of [0, 0xff].
    var w = 65535 - ((uint32)c.K) * 257;
    var r = (65535 - ((uint32)c.C) * 257) * w / 65535;
    var g = (65535 - ((uint32)c.M) * 257) * w / 65535;
    var b = (65535 - ((uint32)c.Y) * 257) * w / 65535;
    return (r, g, b, 65535);
}

// CMYKModel is the [Model] for CMYK colors.
public static Model CMYKModel = ModelFunc(cmykModel);

internal static Color cmykModel(Color c) {
    {
        var (_, ok) = c._<CMYK>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, _) = c.RGBA();
    var (cc, mm, yy, kk) = RGBToCMYK(((uint8)(r >> (int)(8))), ((uint8)(g >> (int)(8))), ((uint8)(b >> (int)(8))));
    return new CMYK(cc, mm, yy, kk);
}

} // end color_package
