// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package image implements a basic 2-D image library.
//
// The fundamental interface is called Image. An Image contains colors, which
// are described in the image/color package.
//
// Values of the Image interface are created either by calling functions such
// as NewRGBA and NewPaletted, or by calling Decode on an io.Reader containing
// image data in a format such as GIF, JPEG or PNG. Decoding any particular
// image format requires the prior registration of a decoder function.
// Registration is typically automatic as a side effect of initializing that
// format's package so that, to decode a PNG image, it suffices to have
//    import _ "image/png"
// in a program's main package. The _ means to import a package purely for its
// initialization side effects.
//
// See "The Go image package" for more details:
// https://golang.org/doc/articles/image_package.html
// package image -- go2cs converted at 2020 August 29 10:09:50 UTC
// import "image" ==> using image = go.image_package
// Original source: C:\Go\src\image\image.go
using color = go.image.color_package;
using static go.builtin;

namespace go
{
    public static partial class image_package
    {
        // Config holds an image's color model and dimensions.
        public partial struct Config
        {
            public color.Model ColorModel;
            public long Width;
            public long Height;
        }

        // Image is a finite rectangular grid of color.Color values taken from a color
        // model.
        public partial interface Image
        {
            color.Color ColorModel(); // Bounds returns the domain for which At can return non-zero color.
// The bounds do not necessarily contain the point (0, 0).
            color.Color Bounds(); // At returns the color of the pixel at (x, y).
// At(Bounds().Min.X, Bounds().Min.Y) returns the upper-left pixel of the grid.
// At(Bounds().Max.X-1, Bounds().Max.Y-1) returns the lower-right one.
            color.Color At(long x, long y);
        }

        // PalettedImage is an image whose colors may come from a limited palette.
        // If m is a PalettedImage and m.ColorModel() returns a color.Palette p,
        // then m.At(x, y) should be equivalent to p[m.ColorIndexAt(x, y)]. If m's
        // color model is not a color.Palette, then ColorIndexAt's behavior is
        // undefined.
        public partial interface PalettedImage : Image
        {
            byte ColorIndexAt(long x, long y);
        }

        // RGBA is an in-memory image whose At method returns color.RGBA values.
        public partial struct RGBA
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref RGBA p)
        {
            return color.RGBAModel;
        }

        private static Rectangle Bounds(this ref RGBA p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref RGBA p, long x, long y)
        {
            return p.RGBAAt(x, y);
        }

        private static color.RGBA RGBAAt(this ref RGBA p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.RGBA();
            }
            var i = p.PixOffset(x, y);
            return new color.RGBA(p.Pix[i+0],p.Pix[i+1],p.Pix[i+2],p.Pix[i+3]);
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref RGBA p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4L;
        }

        private static void Set(this ref RGBA p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            color.RGBA c1 = color.RGBAModel.Convert(c)._<color.RGBA>();
            p.Pix[i + 0L] = c1.R;
            p.Pix[i + 1L] = c1.G;
            p.Pix[i + 2L] = c1.B;
            p.Pix[i + 3L] = c1.A;
        }

        private static void SetRGBA(this ref RGBA p, long x, long y, color.RGBA c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i + 0L] = c.R;
            p.Pix[i + 1L] = c.G;
            p.Pix[i + 2L] = c.B;
            p.Pix[i + 3L] = c.A;
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref RGBA p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new RGBA();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new RGBA(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref RGBA p)
        {
            if (p.Rect.Empty())
            {
                return true;
            }
            long i0 = 3L;
            var i1 = p.Rect.Dx() * 4L;
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                {
                    var i = i0;

                    while (i < i1)
                    {
                        if (p.Pix[i] != 0xffUL)
                        {
                            return false;
                        i += 4L;
                        }
                    }

                }
                i0 += p.Stride;
                i1 += p.Stride;
            }

            return true;
        }

        // NewRGBA returns a new RGBA image with the given bounds.
        public static ref RGBA NewRGBA(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var buf = make_slice<byte>(4L * w * h);
            return ref new RGBA(buf,4*w,r);
        }

        // RGBA64 is an in-memory image whose At method returns color.RGBA64 values.
        public partial struct RGBA64
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref RGBA64 p)
        {
            return color.RGBA64Model;
        }

        private static Rectangle Bounds(this ref RGBA64 p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref RGBA64 p, long x, long y)
        {
            return p.RGBA64At(x, y);
        }

        private static color.RGBA64 RGBA64At(this ref RGBA64 p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.RGBA64();
            }
            var i = p.PixOffset(x, y);
            return new color.RGBA64(uint16(p.Pix[i+0])<<8|uint16(p.Pix[i+1]),uint16(p.Pix[i+2])<<8|uint16(p.Pix[i+3]),uint16(p.Pix[i+4])<<8|uint16(p.Pix[i+5]),uint16(p.Pix[i+6])<<8|uint16(p.Pix[i+7]),);
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref RGBA64 p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 8L;
        }

        private static void Set(this ref RGBA64 p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            color.RGBA64 c1 = color.RGBA64Model.Convert(c)._<color.RGBA64>();
            p.Pix[i + 0L] = uint8(c1.R >> (int)(8L));
            p.Pix[i + 1L] = uint8(c1.R);
            p.Pix[i + 2L] = uint8(c1.G >> (int)(8L));
            p.Pix[i + 3L] = uint8(c1.G);
            p.Pix[i + 4L] = uint8(c1.B >> (int)(8L));
            p.Pix[i + 5L] = uint8(c1.B);
            p.Pix[i + 6L] = uint8(c1.A >> (int)(8L));
            p.Pix[i + 7L] = uint8(c1.A);
        }

        private static void SetRGBA64(this ref RGBA64 p, long x, long y, color.RGBA64 c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i + 0L] = uint8(c.R >> (int)(8L));
            p.Pix[i + 1L] = uint8(c.R);
            p.Pix[i + 2L] = uint8(c.G >> (int)(8L));
            p.Pix[i + 3L] = uint8(c.G);
            p.Pix[i + 4L] = uint8(c.B >> (int)(8L));
            p.Pix[i + 5L] = uint8(c.B);
            p.Pix[i + 6L] = uint8(c.A >> (int)(8L));
            p.Pix[i + 7L] = uint8(c.A);
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref RGBA64 p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new RGBA64();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new RGBA64(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref RGBA64 p)
        {
            if (p.Rect.Empty())
            {
                return true;
            }
            long i0 = 6L;
            var i1 = p.Rect.Dx() * 8L;
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                {
                    var i = i0;

                    while (i < i1)
                    {
                        if (p.Pix[i + 0L] != 0xffUL || p.Pix[i + 1L] != 0xffUL)
                        {
                            return false;
                        i += 8L;
                        }
                    }

                }
                i0 += p.Stride;
                i1 += p.Stride;
            }

            return true;
        }

        // NewRGBA64 returns a new RGBA64 image with the given bounds.
        public static ref RGBA64 NewRGBA64(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(8L * w * h);
            return ref new RGBA64(pix,8*w,r);
        }

        // NRGBA is an in-memory image whose At method returns color.NRGBA values.
        public partial struct NRGBA
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref NRGBA p)
        {
            return color.NRGBAModel;
        }

        private static Rectangle Bounds(this ref NRGBA p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref NRGBA p, long x, long y)
        {
            return p.NRGBAAt(x, y);
        }

        private static color.NRGBA NRGBAAt(this ref NRGBA p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.NRGBA();
            }
            var i = p.PixOffset(x, y);
            return new color.NRGBA(p.Pix[i+0],p.Pix[i+1],p.Pix[i+2],p.Pix[i+3]);
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref NRGBA p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4L;
        }

        private static void Set(this ref NRGBA p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            color.NRGBA c1 = color.NRGBAModel.Convert(c)._<color.NRGBA>();
            p.Pix[i + 0L] = c1.R;
            p.Pix[i + 1L] = c1.G;
            p.Pix[i + 2L] = c1.B;
            p.Pix[i + 3L] = c1.A;
        }

        private static void SetNRGBA(this ref NRGBA p, long x, long y, color.NRGBA c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i + 0L] = c.R;
            p.Pix[i + 1L] = c.G;
            p.Pix[i + 2L] = c.B;
            p.Pix[i + 3L] = c.A;
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref NRGBA p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new NRGBA();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new NRGBA(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref NRGBA p)
        {
            if (p.Rect.Empty())
            {
                return true;
            }
            long i0 = 3L;
            var i1 = p.Rect.Dx() * 4L;
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                {
                    var i = i0;

                    while (i < i1)
                    {
                        if (p.Pix[i] != 0xffUL)
                        {
                            return false;
                        i += 4L;
                        }
                    }

                }
                i0 += p.Stride;
                i1 += p.Stride;
            }

            return true;
        }

        // NewNRGBA returns a new NRGBA image with the given bounds.
        public static ref NRGBA NewNRGBA(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(4L * w * h);
            return ref new NRGBA(pix,4*w,r);
        }

        // NRGBA64 is an in-memory image whose At method returns color.NRGBA64 values.
        public partial struct NRGBA64
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref NRGBA64 p)
        {
            return color.NRGBA64Model;
        }

        private static Rectangle Bounds(this ref NRGBA64 p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref NRGBA64 p, long x, long y)
        {
            return p.NRGBA64At(x, y);
        }

        private static color.NRGBA64 NRGBA64At(this ref NRGBA64 p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.NRGBA64();
            }
            var i = p.PixOffset(x, y);
            return new color.NRGBA64(uint16(p.Pix[i+0])<<8|uint16(p.Pix[i+1]),uint16(p.Pix[i+2])<<8|uint16(p.Pix[i+3]),uint16(p.Pix[i+4])<<8|uint16(p.Pix[i+5]),uint16(p.Pix[i+6])<<8|uint16(p.Pix[i+7]),);
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref NRGBA64 p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 8L;
        }

        private static void Set(this ref NRGBA64 p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            color.NRGBA64 c1 = color.NRGBA64Model.Convert(c)._<color.NRGBA64>();
            p.Pix[i + 0L] = uint8(c1.R >> (int)(8L));
            p.Pix[i + 1L] = uint8(c1.R);
            p.Pix[i + 2L] = uint8(c1.G >> (int)(8L));
            p.Pix[i + 3L] = uint8(c1.G);
            p.Pix[i + 4L] = uint8(c1.B >> (int)(8L));
            p.Pix[i + 5L] = uint8(c1.B);
            p.Pix[i + 6L] = uint8(c1.A >> (int)(8L));
            p.Pix[i + 7L] = uint8(c1.A);
        }

        private static void SetNRGBA64(this ref NRGBA64 p, long x, long y, color.NRGBA64 c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i + 0L] = uint8(c.R >> (int)(8L));
            p.Pix[i + 1L] = uint8(c.R);
            p.Pix[i + 2L] = uint8(c.G >> (int)(8L));
            p.Pix[i + 3L] = uint8(c.G);
            p.Pix[i + 4L] = uint8(c.B >> (int)(8L));
            p.Pix[i + 5L] = uint8(c.B);
            p.Pix[i + 6L] = uint8(c.A >> (int)(8L));
            p.Pix[i + 7L] = uint8(c.A);
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref NRGBA64 p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new NRGBA64();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new NRGBA64(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref NRGBA64 p)
        {
            if (p.Rect.Empty())
            {
                return true;
            }
            long i0 = 6L;
            var i1 = p.Rect.Dx() * 8L;
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                {
                    var i = i0;

                    while (i < i1)
                    {
                        if (p.Pix[i + 0L] != 0xffUL || p.Pix[i + 1L] != 0xffUL)
                        {
                            return false;
                        i += 8L;
                        }
                    }

                }
                i0 += p.Stride;
                i1 += p.Stride;
            }

            return true;
        }

        // NewNRGBA64 returns a new NRGBA64 image with the given bounds.
        public static ref NRGBA64 NewNRGBA64(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(8L * w * h);
            return ref new NRGBA64(pix,8*w,r);
        }

        // Alpha is an in-memory image whose At method returns color.Alpha values.
        public partial struct Alpha
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref Alpha p)
        {
            return color.AlphaModel;
        }

        private static Rectangle Bounds(this ref Alpha p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref Alpha p, long x, long y)
        {
            return p.AlphaAt(x, y);
        }

        private static color.Alpha AlphaAt(this ref Alpha p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.Alpha();
            }
            var i = p.PixOffset(x, y);
            return new color.Alpha(p.Pix[i]);
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref Alpha p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 1L;
        }

        private static void Set(this ref Alpha p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i] = color.AlphaModel.Convert(c)._<color.Alpha>().A;
        }

        private static void SetAlpha(this ref Alpha p, long x, long y, color.Alpha c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i] = c.A;
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref Alpha p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new Alpha();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new Alpha(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref Alpha p)
        {
            if (p.Rect.Empty())
            {
                return true;
            }
            long i0 = 0L;
            var i1 = p.Rect.Dx();
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                for (var i = i0; i < i1; i++)
                {
                    if (p.Pix[i] != 0xffUL)
                    {
                        return false;
                    }
                }

                i0 += p.Stride;
                i1 += p.Stride;
            }

            return true;
        }

        // NewAlpha returns a new Alpha image with the given bounds.
        public static ref Alpha NewAlpha(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(1L * w * h);
            return ref new Alpha(pix,1*w,r);
        }

        // Alpha16 is an in-memory image whose At method returns color.Alpha16 values.
        public partial struct Alpha16
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref Alpha16 p)
        {
            return color.Alpha16Model;
        }

        private static Rectangle Bounds(this ref Alpha16 p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref Alpha16 p, long x, long y)
        {
            return p.Alpha16At(x, y);
        }

        private static color.Alpha16 Alpha16At(this ref Alpha16 p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.Alpha16();
            }
            var i = p.PixOffset(x, y);
            return new color.Alpha16(uint16(p.Pix[i+0])<<8|uint16(p.Pix[i+1]));
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref Alpha16 p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 2L;
        }

        private static void Set(this ref Alpha16 p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            color.Alpha16 c1 = color.Alpha16Model.Convert(c)._<color.Alpha16>();
            p.Pix[i + 0L] = uint8(c1.A >> (int)(8L));
            p.Pix[i + 1L] = uint8(c1.A);
        }

        private static void SetAlpha16(this ref Alpha16 p, long x, long y, color.Alpha16 c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i + 0L] = uint8(c.A >> (int)(8L));
            p.Pix[i + 1L] = uint8(c.A);
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref Alpha16 p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new Alpha16();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new Alpha16(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref Alpha16 p)
        {
            if (p.Rect.Empty())
            {
                return true;
            }
            long i0 = 0L;
            var i1 = p.Rect.Dx() * 2L;
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                {
                    var i = i0;

                    while (i < i1)
                    {
                        if (p.Pix[i + 0L] != 0xffUL || p.Pix[i + 1L] != 0xffUL)
                        {
                            return false;
                        i += 2L;
                        }
                    }

                }
                i0 += p.Stride;
                i1 += p.Stride;
            }

            return true;
        }

        // NewAlpha16 returns a new Alpha16 image with the given bounds.
        public static ref Alpha16 NewAlpha16(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(2L * w * h);
            return ref new Alpha16(pix,2*w,r);
        }

        // Gray is an in-memory image whose At method returns color.Gray values.
        public partial struct Gray
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref Gray p)
        {
            return color.GrayModel;
        }

        private static Rectangle Bounds(this ref Gray p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref Gray p, long x, long y)
        {
            return p.GrayAt(x, y);
        }

        private static color.Gray GrayAt(this ref Gray p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.Gray();
            }
            var i = p.PixOffset(x, y);
            return new color.Gray(p.Pix[i]);
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref Gray p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 1L;
        }

        private static void Set(this ref Gray p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i] = color.GrayModel.Convert(c)._<color.Gray>().Y;
        }

        private static void SetGray(this ref Gray p, long x, long y, color.Gray c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i] = c.Y;
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref Gray p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new Gray();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new Gray(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref Gray p)
        {
            return true;
        }

        // NewGray returns a new Gray image with the given bounds.
        public static ref Gray NewGray(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(1L * w * h);
            return ref new Gray(pix,1*w,r);
        }

        // Gray16 is an in-memory image whose At method returns color.Gray16 values.
        public partial struct Gray16
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref Gray16 p)
        {
            return color.Gray16Model;
        }

        private static Rectangle Bounds(this ref Gray16 p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref Gray16 p, long x, long y)
        {
            return p.Gray16At(x, y);
        }

        private static color.Gray16 Gray16At(this ref Gray16 p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.Gray16();
            }
            var i = p.PixOffset(x, y);
            return new color.Gray16(uint16(p.Pix[i+0])<<8|uint16(p.Pix[i+1]));
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref Gray16 p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 2L;
        }

        private static void Set(this ref Gray16 p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            color.Gray16 c1 = color.Gray16Model.Convert(c)._<color.Gray16>();
            p.Pix[i + 0L] = uint8(c1.Y >> (int)(8L));
            p.Pix[i + 1L] = uint8(c1.Y);
        }

        private static void SetGray16(this ref Gray16 p, long x, long y, color.Gray16 c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i + 0L] = uint8(c.Y >> (int)(8L));
            p.Pix[i + 1L] = uint8(c.Y);
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref Gray16 p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new Gray16();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new Gray16(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref Gray16 p)
        {
            return true;
        }

        // NewGray16 returns a new Gray16 image with the given bounds.
        public static ref Gray16 NewGray16(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(2L * w * h);
            return ref new Gray16(pix,2*w,r);
        }

        // CMYK is an in-memory image whose At method returns color.CMYK values.
        public partial struct CMYK
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect;
        }

        private static color.Model ColorModel(this ref CMYK p)
        {
            return color.CMYKModel;
        }

        private static Rectangle Bounds(this ref CMYK p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref CMYK p, long x, long y)
        {
            return p.CMYKAt(x, y);
        }

        private static color.CMYK CMYKAt(this ref CMYK p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return new color.CMYK();
            }
            var i = p.PixOffset(x, y);
            return new color.CMYK(p.Pix[i+0],p.Pix[i+1],p.Pix[i+2],p.Pix[i+3]);
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref CMYK p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4L;
        }

        private static void Set(this ref CMYK p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            color.CMYK c1 = color.CMYKModel.Convert(c)._<color.CMYK>();
            p.Pix[i + 0L] = c1.C;
            p.Pix[i + 1L] = c1.M;
            p.Pix[i + 2L] = c1.Y;
            p.Pix[i + 3L] = c1.K;
        }

        private static void SetCMYK(this ref CMYK p, long x, long y, color.CMYK c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i + 0L] = c.C;
            p.Pix[i + 1L] = c.M;
            p.Pix[i + 2L] = c.Y;
            p.Pix[i + 3L] = c.K;
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref CMYK p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new CMYK();
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new CMYK(Pix:p.Pix[i:],Stride:p.Stride,Rect:r,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref CMYK p)
        {
            return true;
        }

        // NewCMYK returns a new CMYK image with the given bounds.
        public static ref CMYK NewCMYK(Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            var buf = make_slice<byte>(4L * w * h);
            return ref new CMYK(buf,4*w,r);
        }

        // Paletted is an in-memory image of uint8 indices into a given palette.
        public partial struct Paletted
        {
            public slice<byte> Pix; // Stride is the Pix stride (in bytes) between vertically adjacent pixels.
            public long Stride; // Rect is the image's bounds.
            public Rectangle Rect; // Palette is the image's palette.
            public color.Palette Palette;
        }

        private static color.Model ColorModel(this ref Paletted p)
        {
            return p.Palette;
        }

        private static Rectangle Bounds(this ref Paletted p)
        {
            return p.Rect;
        }

        private static color.Color At(this ref Paletted p, long x, long y)
        {
            if (len(p.Palette) == 0L)
            {
                return null;
            }
            if (!(new Point(x,y).In(p.Rect)))
            {
                return p.Palette[0L];
            }
            var i = p.PixOffset(x, y);
            return p.Palette[p.Pix[i]];
        }

        // PixOffset returns the index of the first element of Pix that corresponds to
        // the pixel at (x, y).
        private static long PixOffset(this ref Paletted p, long x, long y)
        {
            return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 1L;
        }

        private static void Set(this ref Paletted p, long x, long y, color.Color c)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i] = uint8(p.Palette.Index(c));
        }

        private static byte ColorIndexAt(this ref Paletted p, long x, long y)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return 0L;
            }
            var i = p.PixOffset(x, y);
            return p.Pix[i];
        }

        private static void SetColorIndex(this ref Paletted p, long x, long y, byte index)
        {
            if (!(new Point(x,y).In(p.Rect)))
            {
                return;
            }
            var i = p.PixOffset(x, y);
            p.Pix[i] = index;
        }

        // SubImage returns an image representing the portion of the image p visible
        // through r. The returned value shares pixels with the original image.
        private static Image SubImage(this ref Paletted p, Rectangle r)
        {
            r = r.Intersect(p.Rect); 
            // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
            // either r1 or r2 if the intersection is empty. Without explicitly checking for
            // this, the Pix[i:] expression below can panic.
            if (r.Empty())
            {
                return ref new Paletted(Palette:p.Palette,);
            }
            var i = p.PixOffset(r.Min.X, r.Min.Y);
            return ref new Paletted(Pix:p.Pix[i:],Stride:p.Stride,Rect:p.Rect.Intersect(r),Palette:p.Palette,);
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref Paletted p)
        {
            array<bool> present = new array<bool>(256L);
            long i0 = 0L;
            var i1 = p.Rect.Dx();
            for (var y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++)
            {
                {
                    var c__prev2 = c;

                    foreach (var (_, __c) in p.Pix[i0..i1])
                    {
                        c = __c;
                        present[c] = true;
                    }

                    c = c__prev2;
                }

                i0 += p.Stride;
                i1 += p.Stride;
            }

            {
                var c__prev1 = c;

                foreach (var (__i, __c) in p.Palette)
                {
                    i = __i;
                    c = __c;
                    if (!present[i])
                    {
                        continue;
                    }
                    var (_, _, _, a) = c.RGBA();
                    if (a != 0xffffUL)
                    {
                        return false;
                    }
                }

                c = c__prev1;
            }

            return true;
        }

        // NewPaletted returns a new Paletted image with the given width, height and
        // palette.
        public static ref Paletted NewPaletted(Rectangle r, color.Palette p)
        {
            var w = r.Dx();
            var h = r.Dy();
            var pix = make_slice<byte>(1L * w * h);
            return ref new Paletted(pix,1*w,r,p);
        }
    }
}
