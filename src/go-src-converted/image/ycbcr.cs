// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package image -- go2cs converted at 2022 March 13 06:43:56 UTC
// import "image" ==> using image = go.image_package
// Original source: C:\Program Files\Go\src\image\ycbcr.go
namespace go;

using color = image.color_package;


// YCbCrSubsampleRatio is the chroma subsample ratio used in a YCbCr image.

public static partial class image_package {

public partial struct YCbCrSubsampleRatio { // : nint
}

public static readonly YCbCrSubsampleRatio YCbCrSubsampleRatio444 = iota;
public static readonly var YCbCrSubsampleRatio422 = 0;
public static readonly var YCbCrSubsampleRatio420 = 1;
public static readonly var YCbCrSubsampleRatio440 = 2;
public static readonly var YCbCrSubsampleRatio411 = 3;
public static readonly var YCbCrSubsampleRatio410 = 4;

public static @string String(this YCbCrSubsampleRatio s) {

    if (s == YCbCrSubsampleRatio444) 
        return "YCbCrSubsampleRatio444";
    else if (s == YCbCrSubsampleRatio422) 
        return "YCbCrSubsampleRatio422";
    else if (s == YCbCrSubsampleRatio420) 
        return "YCbCrSubsampleRatio420";
    else if (s == YCbCrSubsampleRatio440) 
        return "YCbCrSubsampleRatio440";
    else if (s == YCbCrSubsampleRatio411) 
        return "YCbCrSubsampleRatio411";
    else if (s == YCbCrSubsampleRatio410) 
        return "YCbCrSubsampleRatio410";
        return "YCbCrSubsampleRatioUnknown";
}

// YCbCr is an in-memory image of Y'CbCr colors. There is one Y sample per
// pixel, but each Cb and Cr sample can span one or more pixels.
// YStride is the Y slice index delta between vertically adjacent pixels.
// CStride is the Cb and Cr slice index delta between vertically adjacent pixels
// that map to separate chroma samples.
// It is not an absolute requirement, but YStride and len(Y) are typically
// multiples of 8, and:
//    For 4:4:4, CStride == YStride/1 && len(Cb) == len(Cr) == len(Y)/1.
//    For 4:2:2, CStride == YStride/2 && len(Cb) == len(Cr) == len(Y)/2.
//    For 4:2:0, CStride == YStride/2 && len(Cb) == len(Cr) == len(Y)/4.
//    For 4:4:0, CStride == YStride/1 && len(Cb) == len(Cr) == len(Y)/2.
//    For 4:1:1, CStride == YStride/4 && len(Cb) == len(Cr) == len(Y)/4.
//    For 4:1:0, CStride == YStride/4 && len(Cb) == len(Cr) == len(Y)/8.
public partial struct YCbCr {
    public slice<byte> Y;
    public slice<byte> Cb;
    public slice<byte> Cr;
    public nint YStride;
    public nint CStride;
    public YCbCrSubsampleRatio SubsampleRatio;
    public Rectangle Rect;
}

private static color.Model ColorModel(this ptr<YCbCr> _addr_p) {
    ref YCbCr p = ref _addr_p.val;

    return color.YCbCrModel;
}

private static Rectangle Bounds(this ptr<YCbCr> _addr_p) {
    ref YCbCr p = ref _addr_p.val;

    return p.Rect;
}

private static color.Color At(this ptr<YCbCr> _addr_p, nint x, nint y) {
    ref YCbCr p = ref _addr_p.val;

    return p.YCbCrAt(x, y);
}

private static color.RGBA64 RGBA64At(this ptr<YCbCr> _addr_p, nint x, nint y) {
    ref YCbCr p = ref _addr_p.val;

    var (r, g, b, a) = p.YCbCrAt(x, y).RGBA();
    return new color.RGBA64(uint16(r),uint16(g),uint16(b),uint16(a));
}

private static color.YCbCr YCbCrAt(this ptr<YCbCr> _addr_p, nint x, nint y) {
    ref YCbCr p = ref _addr_p.val;

    if (!(new Point(x,y).In(p.Rect))) {
        return new color.YCbCr();
    }
    var yi = p.YOffset(x, y);
    var ci = p.COffset(x, y);
    return new color.YCbCr(p.Y[yi],p.Cb[ci],p.Cr[ci],);
}

// YOffset returns the index of the first element of Y that corresponds to
// the pixel at (x, y).
private static nint YOffset(this ptr<YCbCr> _addr_p, nint x, nint y) {
    ref YCbCr p = ref _addr_p.val;

    return (y - p.Rect.Min.Y) * p.YStride + (x - p.Rect.Min.X);
}

// COffset returns the index of the first element of Cb or Cr that corresponds
// to the pixel at (x, y).
private static nint COffset(this ptr<YCbCr> _addr_p, nint x, nint y) {
    ref YCbCr p = ref _addr_p.val;


    if (p.SubsampleRatio == YCbCrSubsampleRatio422) 
        return (y - p.Rect.Min.Y) * p.CStride + (x / 2 - p.Rect.Min.X / 2);
    else if (p.SubsampleRatio == YCbCrSubsampleRatio420) 
        return (y / 2 - p.Rect.Min.Y / 2) * p.CStride + (x / 2 - p.Rect.Min.X / 2);
    else if (p.SubsampleRatio == YCbCrSubsampleRatio440) 
        return (y / 2 - p.Rect.Min.Y / 2) * p.CStride + (x - p.Rect.Min.X);
    else if (p.SubsampleRatio == YCbCrSubsampleRatio411) 
        return (y - p.Rect.Min.Y) * p.CStride + (x / 4 - p.Rect.Min.X / 4);
    else if (p.SubsampleRatio == YCbCrSubsampleRatio410) 
        return (y / 2 - p.Rect.Min.Y / 2) * p.CStride + (x / 4 - p.Rect.Min.X / 4);
    // Default to 4:4:4 subsampling.
    return (y - p.Rect.Min.Y) * p.CStride + (x - p.Rect.Min.X);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
private static Image SubImage(this ptr<YCbCr> _addr_p, Rectangle r) {
    ref YCbCr p = ref _addr_p.val;

    r = r.Intersect(p.Rect); 
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return addr(new YCbCr(SubsampleRatio:p.SubsampleRatio,));
    }
    var yi = p.YOffset(r.Min.X, r.Min.Y);
    var ci = p.COffset(r.Min.X, r.Min.Y);
    return addr(new YCbCr(Y:p.Y[yi:],Cb:p.Cb[ci:],Cr:p.Cr[ci:],SubsampleRatio:p.SubsampleRatio,YStride:p.YStride,CStride:p.CStride,Rect:r,));
}

private static bool Opaque(this ptr<YCbCr> _addr_p) {
    ref YCbCr p = ref _addr_p.val;

    return true;
}

private static (nint, nint, nint, nint) yCbCrSize(Rectangle r, YCbCrSubsampleRatio subsampleRatio) {
    nint w = default;
    nint h = default;
    nint cw = default;
    nint ch = default;

    (w, h) = (r.Dx(), r.Dy());
    if (subsampleRatio == YCbCrSubsampleRatio422) 
        cw = (r.Max.X + 1) / 2 - r.Min.X / 2;
        ch = h;
    else if (subsampleRatio == YCbCrSubsampleRatio420) 
        cw = (r.Max.X + 1) / 2 - r.Min.X / 2;
        ch = (r.Max.Y + 1) / 2 - r.Min.Y / 2;
    else if (subsampleRatio == YCbCrSubsampleRatio440) 
        cw = w;
        ch = (r.Max.Y + 1) / 2 - r.Min.Y / 2;
    else if (subsampleRatio == YCbCrSubsampleRatio411) 
        cw = (r.Max.X + 3) / 4 - r.Min.X / 4;
        ch = h;
    else if (subsampleRatio == YCbCrSubsampleRatio410) 
        cw = (r.Max.X + 3) / 4 - r.Min.X / 4;
        ch = (r.Max.Y + 1) / 2 - r.Min.Y / 2;
    else 
        // Default to 4:4:4 subsampling.
        cw = w;
        ch = h;
        return ;
}

// NewYCbCr returns a new YCbCr image with the given bounds and subsample
// ratio.
public static ptr<YCbCr> NewYCbCr(Rectangle r, YCbCrSubsampleRatio subsampleRatio) => func((_, panic, _) => {
    var (w, h, cw, ch) = yCbCrSize(r, subsampleRatio); 

    // totalLength should be the same as i2, below, for a valid Rectangle r.
    var totalLength = add2NonNeg(mul3NonNeg(1, w, h), mul3NonNeg(2, cw, ch));
    if (totalLength < 0) {
        panic("image: NewYCbCr Rectangle has huge or negative dimensions");
    }
    var i0 = w * h + 0 * cw * ch;
    var i1 = w * h + 1 * cw * ch;
    var i2 = w * h + 2 * cw * ch;
    var b = make_slice<byte>(i2);
    return addr(new YCbCr(Y:b[:i0:i0],Cb:b[i0:i1:i1],Cr:b[i1:i2:i2],SubsampleRatio:subsampleRatio,YStride:w,CStride:cw,Rect:r,));
});

// NYCbCrA is an in-memory image of non-alpha-premultiplied Y'CbCr-with-alpha
// colors. A and AStride are analogous to the Y and YStride fields of the
// embedded YCbCr.
public partial struct NYCbCrA {
    public ref YCbCr YCbCr => ref YCbCr_val;
    public slice<byte> A;
    public nint AStride;
}

private static color.Model ColorModel(this ptr<NYCbCrA> _addr_p) {
    ref NYCbCrA p = ref _addr_p.val;

    return color.NYCbCrAModel;
}

private static color.Color At(this ptr<NYCbCrA> _addr_p, nint x, nint y) {
    ref NYCbCrA p = ref _addr_p.val;

    return p.NYCbCrAAt(x, y);
}

private static color.RGBA64 RGBA64At(this ptr<NYCbCrA> _addr_p, nint x, nint y) {
    ref NYCbCrA p = ref _addr_p.val;

    var (r, g, b, a) = p.NYCbCrAAt(x, y).RGBA();
    return new color.RGBA64(uint16(r),uint16(g),uint16(b),uint16(a));
}

private static color.NYCbCrA NYCbCrAAt(this ptr<NYCbCrA> _addr_p, nint x, nint y) {
    ref NYCbCrA p = ref _addr_p.val;

    if (!(new Point(X:x,Y:y).In(p.Rect))) {
        return new color.NYCbCrA();
    }
    var yi = p.YOffset(x, y);
    var ci = p.COffset(x, y);
    var ai = p.AOffset(x, y);
    return new color.NYCbCrA(color.YCbCr{Y:p.Y[yi],Cb:p.Cb[ci],Cr:p.Cr[ci],},p.A[ai],);
}

// AOffset returns the index of the first element of A that corresponds to the
// pixel at (x, y).
private static nint AOffset(this ptr<NYCbCrA> _addr_p, nint x, nint y) {
    ref NYCbCrA p = ref _addr_p.val;

    return (y - p.Rect.Min.Y) * p.AStride + (x - p.Rect.Min.X);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
private static Image SubImage(this ptr<NYCbCrA> _addr_p, Rectangle r) {
    ref NYCbCrA p = ref _addr_p.val;

    r = r.Intersect(p.Rect); 
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return addr(new NYCbCrA(YCbCr:YCbCr{SubsampleRatio:p.SubsampleRatio,},));
    }
    var yi = p.YOffset(r.Min.X, r.Min.Y);
    var ci = p.COffset(r.Min.X, r.Min.Y);
    var ai = p.AOffset(r.Min.X, r.Min.Y);
    return addr(new NYCbCrA(YCbCr:YCbCr{Y:p.Y[yi:],Cb:p.Cb[ci:],Cr:p.Cr[ci:],SubsampleRatio:p.SubsampleRatio,YStride:p.YStride,CStride:p.CStride,Rect:r,},A:p.A[ai:],AStride:p.AStride,));
}

// Opaque scans the entire image and reports whether it is fully opaque.
private static bool Opaque(this ptr<NYCbCrA> _addr_p) {
    ref NYCbCrA p = ref _addr_p.val;

    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 0;
    var i1 = p.Rect.Dx();
    for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        foreach (var (_, a) in p.A[(int)i0..(int)i1]) {
            if (a != 0xff) {
                return false;
            }
        }        i0 += p.AStride;
        i1 += p.AStride;
    }
    return true;
}

// NewNYCbCrA returns a new NYCbCrA image with the given bounds and subsample
// ratio.
public static ptr<NYCbCrA> NewNYCbCrA(Rectangle r, YCbCrSubsampleRatio subsampleRatio) => func((_, panic, _) => {
    var (w, h, cw, ch) = yCbCrSize(r, subsampleRatio); 

    // totalLength should be the same as i3, below, for a valid Rectangle r.
    var totalLength = add2NonNeg(mul3NonNeg(2, w, h), mul3NonNeg(2, cw, ch));
    if (totalLength < 0) {
        panic("image: NewNYCbCrA Rectangle has huge or negative dimension");
    }
    nint i0 = 1 * w * h + 0 * cw * ch;
    nint i1 = 1 * w * h + 1 * cw * ch;
    nint i2 = 1 * w * h + 2 * cw * ch;
    nint i3 = 2 * w * h + 2 * cw * ch;
    var b = make_slice<byte>(i3);
    return addr(new NYCbCrA(YCbCr:YCbCr{Y:b[:i0:i0],Cb:b[i0:i1:i1],Cr:b[i1:i2:i2],SubsampleRatio:subsampleRatio,YStride:w,CStride:cw,Rect:r,},A:b[i2:],AStride:w,));
});

} // end image_package
