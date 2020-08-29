// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package image -- go2cs converted at 2020 August 29 10:09:52 UTC
// import "image" ==> using image = go.image_package
// Original source: C:\Go\src\image\ycbcr.go
using color = go.image.color_package;
using static go.builtin;

namespace go
{
    public static partial class image_package
    {
        // YCbCrSubsampleRatio is the chroma subsample ratio used in a YCbCr image.
        public partial struct YCbCrSubsampleRatio // : long
        {
        }

        public static readonly YCbCrSubsampleRatio YCbCrSubsampleRatio444 = iota;
        public static readonly var YCbCrSubsampleRatio422 = 0;
        public static readonly var YCbCrSubsampleRatio420 = 1;
        public static readonly var YCbCrSubsampleRatio440 = 2;
        public static readonly var YCbCrSubsampleRatio411 = 3;
        public static readonly var YCbCrSubsampleRatio410 = 4;

        public static @string String(this YCbCrSubsampleRatio s)
        {

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
        public partial struct YCbCr
        {
            public slice<byte> Y;
            public slice<byte> Cb;
            public slice<byte> Cr;
            public long YStride;
            public long CStride;
            public YCbCrSubsampleRatio SubsampleRatio;
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref YCbCr p)
        {
            return color.YCbCrModel;
        }

        private static Rectangle Bounds(this ref YCbCr p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref YCbCr p, long x, long y)
        {
            return p.YCbCrAt(x, y);
        }

        private static color.YCbCr YCbCrAt(this ref YCbCr p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.YCbCr();
            }
            var yi = p.YOffset(x, y);
            var ci = p.COffset(x, y);
            return new color.YCbCr(p.Y[yi],p.Cb[ci],p.Cr[ci],);
        }

        // YOffset returns the index of the first element of Y that corresponds to
        // the pixel at (x, y).
        private static long YOffset(this ref YCbCr p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.YStride + (x - p.Rect.Min.X);
        }

        // COffset returns the index of the first element of Cb or Cr that corresponds
        // to the pixel at (x, y).
        private static long COffset(this ref YCbCr p, long x, long y)
        {

            if (p.SubsampleRatio == YCbCrSubsampleRatio422) 
                return (y - p.Rect.Min.Y) * p.CStride + (x / 2L - p.Rect.Min.X / 2L);
            else if (p.SubsampleRatio == YCbCrSubsampleRatio420) 
                return (y / 2L - p.Rect.Min.Y / 2L) * p.CStride + (x / 2L - p.Rect.Min.X / 2L);
            else if (p.SubsampleRatio == YCbCrSubsampleRatio440) 
                return (y / 2L - p.Rect.Min.Y / 2L) * p.CStride + (x - p.Rect.Min.X);
            else if (p.SubsampleRatio == YCbCrSubsampleRatio411) 
                return (y - p.Rect.Min.Y) * p.CStride + (x / 4L - p.Rect.Min.X / 4L);
            else if (p.SubsampleRatio == YCbCrSubsampleRatio410) 
                return (y / 2L - p.Rect.Min.Y / 2L) * p.CStride + (x / 4L - p.Rect.Min.X / 4L);
            // Default to 4:4:4 subsampling.
            return (y - p.Rect.Min.Y) * p.CStride + (x - p.Rect.Min.X);
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref YCbCr p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new YCbCr(SubsampleRatio:p.SubsampleRatio,);
            }
            var yi = p.YOffset(r.Min.X, r.Min.Y);
            var ci = p.COffset(r.Min.X, r.Min.Y);
            return ref new YCbCr(Y:p.Y[yi:],Cb:p.Cb[ci:],Cr:p.Cr[ci:],SubsampleRatio:p.SubsampleRatio,YStride:p.YStride,CStride:p.CStride,Rect:r,);
        }

        private static bool Opaque(this ref YCbCr p)
        {
            return true;
        }

        private static (long, long, long, long) yCbCrSize(Rectangle r, YCbCrSubsampleRatio subsampleRatio)
        {
            w = r.Dx();
            h = r.Dy();

            if (subsampleRatio == YCbCrSubsampleRatio422) 
                cw = (r.Max.X + 1L) / 2L - r.Min.X / 2L;
                ch = h;
            else if (subsampleRatio == YCbCrSubsampleRatio420) 
                cw = (r.Max.X + 1L) / 2L - r.Min.X / 2L;
                ch = (r.Max.Y + 1L) / 2L - r.Min.Y / 2L;
            else if (subsampleRatio == YCbCrSubsampleRatio440) 
                cw = w;
                ch = (r.Max.Y + 1L) / 2L - r.Min.Y / 2L;
            else if (subsampleRatio == YCbCrSubsampleRatio411) 
                cw = (r.Max.X + 3L) / 4L - r.Min.X / 4L;
                ch = h;
            else if (subsampleRatio == YCbCrSubsampleRatio410) 
                cw = (r.Max.X + 3L) / 4L - r.Min.X / 4L;
                ch = (r.Max.Y + 1L) / 2L - r.Min.Y / 2L;
            else 
                // Default to 4:4:4 subsampling.
                cw = w;
                ch = h;
                        return;
        }

        // NewYCbCr returns a new YCbCr image with the given bounds and subsample
        // ratio.
        public static ref YCbCr NewYCbCr(Rectangle r, YCbCrSubsampleRatio subsampleRatio)
        {
            var (w, h, cw, ch) = yCbCrSize(r, subsampleRatio);
            var i0 = w * h + 0L * cw * ch;
            var i1 = w * h + 1L * cw * ch;
            var i2 = w * h + 2L * cw * ch;
            var b = make_slice<byte>(i2);
            return ref new YCbCr(Y:b[:i0:i0],Cb:b[i0:i1:i1],Cr:b[i1:i2:i2],SubsampleRatio:subsampleRatio,YStride:w,CStride:cw,Rect:r,);
        }

        // NYCbCrA is an in-memory image of non-alpha-premultiplied Y'CbCr-with-alpha
        // colors. A and AStride are analogous to the Y and YStride fields of the
        // embedded YCbCr.
        public partial struct NYCbCrA
        {
            public ref YCbCr YCbCr => ref YCbCr_val;
            public slice<byte> A;
            public long AStride;
        }

        private static color.Model ColorModel(this ref NYCbCrA p)
        {
            return color.NYCbCrAModel;
        }

        private static color.Color At(this ref NYCbCrA p, long x, long y)
        {
            return p.NYCbCrAAt(x, y);
        }

        private static color.NYCbCrA NYCbCrAAt(this ref NYCbCrA p, long x, long y)
        {
            if (!(new Point(X:x,Y:y).In(p.Rect)))
            {
                return new color.NYCbCrA();
            }
            var yi = p.YOffset(x, y);
            var ci = p.COffset(x, y);
            var ai = p.AOffset(x, y);
            return new color.NYCbCrA(color.YCbCr{Y:p.Y[yi],Cb:p.Cb[ci],Cr:p.Cr[ci],},p.A[ai],);
        }

        // AOffset returns the index of the first element of A that corresponds to the
        // pixel at (x, y).
        private static long AOffset(this ref NYCbCrA p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.AStride + (x - p.Rect.Min.X);
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref NYCbCrA p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new NYCbCrA(YCbCr:YCbCr{SubsampleRatio:p.SubsampleRatio,},);
            }
            var yi = p.YOffset(r.Min.X, r.Min.Y);
            var ci = p.COffset(r.Min.X, r.Min.Y);
            var ai = p.AOffset(r.Min.X, r.Min.Y);
            return ref new NYCbCrA(YCbCr:YCbCr{Y:p.Y[yi:],Cb:p.Cb[ci:],Cr:p.Cr[ci:],SubsampleRatio:p.SubsampleRatio,YStride:p.YStride,CStride:p.CStride,Rect:r,},A:p.A[ai:],AStride:p.AStride,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref NYCbCrA p)
        {
            if (p.Rect.Empty())
            {
                return true;
            }
            long i0 = 0L;
            var i1 = p.Rect.Dx();
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                foreach (var (_, a) in p.A[i0..i1])
                {
                    if (a != 0xffUL)
                    {
                        return false;
                    }
                }
                i0 += p.AStride;
                i1 += p.AStride;
            }

            return true;
        }

        // NewNYCbCrA returns a new NYCbCrA image with the given bounds and subsample
        // ratio.
        public static ref NYCbCrA NewNYCbCrA(Rectangle r, YCbCrSubsampleRatio subsampleRatio)
        {
            var (w, h, cw, ch) = yCbCrSize(r, subsampleRatio);
            long i0 = 1L * w * h + 0L * cw * ch;
            long i1 = 1L * w * h + 1L * cw * ch;
            long i2 = 1L * w * h + 2L * cw * ch;
            long i3 = 2L * w * h + 2L * cw * ch;
            var b = make_slice<byte>(i3);
            return ref new NYCbCrA(YCbCr:YCbCr{Y:b[:i0:i0],Cb:b[i0:i1:i1],Cr:b[i1:i2:i2],SubsampleRatio:subsampleRatio,YStride:w,CStride:cw,Rect:r,},A:b[i2:],AStride:w,);
        }
    }
}
