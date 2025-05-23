// Code generated by go run gen.go; DO NOT EDIT.
namespace go.image.@internal;

using image = image_package;

partial class imageutil_package {

// DrawYCbCr draws the YCbCr source image on the RGBA destination image with
// r.Min in dst aligned with sp in src. It reports whether the draw was
// successful. If it returns false, no dst pixels were changed.
//
// This function assumes that r is entirely within dst's bounds and the
// translation of r from dst coordinate space to src coordinate space is
// entirely within src's bounds.
public static bool /*ok*/ DrawYCbCr(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, ж<image.YCbCr> Ꮡsrc, image.Point sp) {
    bool ok = default!;

    ref var dst = ref Ꮡdst.val;
    ref var src = ref Ꮡsrc.val;
    // This function exists in the image/internal/imageutil package because it
    // is needed by both the image/draw and image/jpeg packages, but it doesn't
    // seem right for one of those two to depend on the other.
    //
    // Another option is to have this code be exported in the image package,
    // but we'd need to make sure we're totally happy with the API (for the
    // rest of Go 1 compatibility), and decide if we want to have a more
    // general purpose DrawToRGBA method for other image types. One possibility
    // is:
    //
    // func (src *YCbCr) CopyToRGBA(dst *RGBA, dr, sr Rectangle) (effectiveDr, effectiveSr Rectangle)
    //
    // in the spirit of the built-in copy function for 1-dimensional slices,
    // that also allowed a CopyFromRGBA method if needed.
    nint x0 = (r.Min.X - dst.Rect.Min.X) * 4;
    nint x1 = (r.Max.X - dst.Rect.Min.X) * 4;
    nint y0 = r.Min.Y - dst.Rect.Min.Y;
    nint y1 = r.Max.Y - dst.Rect.Min.Y;
    var exprᴛ1 = src.SubsampleRatio;
    if (exprᴛ1 == image.YCbCrSubsampleRatio444) {
        for (nint y = y0;nint sy = sp.Y; y != y1; (y, sy) = (y + 1, sy + 1)) {
            var dpix = dst.Pix[(int)(y * dst.Stride)..];
            nint yi = (sy - src.Rect.Min.Y) * src.YStride + (sp.X - src.Rect.Min.X);
            nint ci = (sy - src.Rect.Min.Y) * src.CStride + (sp.X - src.Rect.Min.X);
            for (nint x = x0; x != x1; (x, yi, ci) = (x + 4, yi + 1, ci + 1)) {
                // This is an inline version of image/color/ycbcr.go's func YCbCrToRGB.
                var yy1 = ((int32)src.Y[yi]) * 65793;
                var cb1 = ((int32)src.Cb[ci]) - 128;
                var cr1 = ((int32)src.Cr[ci]) - 128;
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
                var rΔ5 = yy1 + 91881 * cr1;
                if ((uint32)(((uint32)rΔ5) & (nint)4278190080L) == 0){
                    rΔ5 >>= (UntypedInt)(16);
                } else {
                    rΔ5 = ~(rΔ5 >> (int)(31));
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
                // use a temp slice to hint to the compiler that a single bounds check suffices
                var rgba = dpix.slice(x, x + 4, len(dpix));
                rgba[0] = ((uint8)rΔ5);
                rgba[1] = ((uint8)g);
                rgba[2] = ((uint8)b);
                rgba[3] = 255;
            }
        }
    }
    else if (exprᴛ1 == image.YCbCrSubsampleRatio422) {
        for (nint y = y0;nint sy = sp.Y; y != y1; (y, sy) = (y + 1, sy + 1)) {
            var dpix = dst.Pix[(int)(y * dst.Stride)..];
            nint yi = (sy - src.Rect.Min.Y) * src.YStride + (sp.X - src.Rect.Min.X);
            nint ciBase = (sy - src.Rect.Min.Y) * src.CStride - src.Rect.Min.X / 2;
            for (nint x = x0;nint sx = sp.X; x != x1; (x, sx, yi) = (x + 4, sx + 1, yi + 1)) {
                nint ci = ciBase + sx / 2;
                // This is an inline version of image/color/ycbcr.go's func YCbCrToRGB.
                var yy1 = ((int32)src.Y[yi]) * 65793;
                var cb1 = ((int32)src.Cb[ci]) - 128;
                var cr1 = ((int32)src.Cr[ci]) - 128;
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
                var rΔ6 = yy1 + 91881 * cr1;
                if ((uint32)(((uint32)rΔ6) & (nint)4278190080L) == 0){
                    rΔ6 >>= (UntypedInt)(16);
                } else {
                    rΔ6 = ~(rΔ6 >> (int)(31));
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
                // use a temp slice to hint to the compiler that a single bounds check suffices
                var rgba = dpix.slice(x, x + 4, len(dpix));
                rgba[0] = ((uint8)rΔ6);
                rgba[1] = ((uint8)g);
                rgba[2] = ((uint8)b);
                rgba[3] = 255;
            }
        }
    }
    else if (exprᴛ1 == image.YCbCrSubsampleRatio420) {
        for (nint y = y0;nint sy = sp.Y; y != y1; (y, sy) = (y + 1, sy + 1)) {
            var dpix = dst.Pix[(int)(y * dst.Stride)..];
            nint yi = (sy - src.Rect.Min.Y) * src.YStride + (sp.X - src.Rect.Min.X);
            nint ciBase = (sy / 2 - src.Rect.Min.Y / 2) * src.CStride - src.Rect.Min.X / 2;
            for (nint x = x0;nint sx = sp.X; x != x1; (x, sx, yi) = (x + 4, sx + 1, yi + 1)) {
                nint ci = ciBase + sx / 2;
                // This is an inline version of image/color/ycbcr.go's func YCbCrToRGB.
                var yy1 = ((int32)src.Y[yi]) * 65793;
                var cb1 = ((int32)src.Cb[ci]) - 128;
                var cr1 = ((int32)src.Cr[ci]) - 128;
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
                var rΔ7 = yy1 + 91881 * cr1;
                if ((uint32)(((uint32)rΔ7) & (nint)4278190080L) == 0){
                    rΔ7 >>= (UntypedInt)(16);
                } else {
                    rΔ7 = ~(rΔ7 >> (int)(31));
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
                // use a temp slice to hint to the compiler that a single bounds check suffices
                var rgba = dpix.slice(x, x + 4, len(dpix));
                rgba[0] = ((uint8)rΔ7);
                rgba[1] = ((uint8)g);
                rgba[2] = ((uint8)b);
                rgba[3] = 255;
            }
        }
    }
    else if (exprᴛ1 == image.YCbCrSubsampleRatio440) {
        for (nint y = y0;nint sy = sp.Y; y != y1; (y, sy) = (y + 1, sy + 1)) {
            var dpix = dst.Pix[(int)(y * dst.Stride)..];
            nint yi = (sy - src.Rect.Min.Y) * src.YStride + (sp.X - src.Rect.Min.X);
            nint ci = (sy / 2 - src.Rect.Min.Y / 2) * src.CStride + (sp.X - src.Rect.Min.X);
            for (nint x = x0; x != x1; (x, yi, ci) = (x + 4, yi + 1, ci + 1)) {
                // This is an inline version of image/color/ycbcr.go's func YCbCrToRGB.
                var yy1 = ((int32)src.Y[yi]) * 65793;
                var cb1 = ((int32)src.Cb[ci]) - 128;
                var cr1 = ((int32)src.Cr[ci]) - 128;
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
                var rΔ8 = yy1 + 91881 * cr1;
                if ((uint32)(((uint32)rΔ8) & (nint)4278190080L) == 0){
                    rΔ8 >>= (UntypedInt)(16);
                } else {
                    rΔ8 = ~(rΔ8 >> (int)(31));
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
                // use a temp slice to hint to the compiler that a single bounds check suffices
                var rgba = dpix.slice(x, x + 4, len(dpix));
                rgba[0] = ((uint8)rΔ8);
                rgba[1] = ((uint8)g);
                rgba[2] = ((uint8)b);
                rgba[3] = 255;
            }
        }
    }
    else { /* default: */
        return false;
    }

    return true;
}

} // end imageutil_package
