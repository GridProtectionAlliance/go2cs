// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package image -- go2cs converted at 2020 August 29 10:09:39 UTC
// import "image" ==> using image = go.image_package
// Original source: C:\Go\src\image\geom.go
using color = go.image.color_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go
{
    public static partial class image_package
    {
        // A Point is an X, Y coordinate pair. The axes increase right and down.
        public partial struct Point
        {
            public long X;
            public long Y;
        }

        // String returns a string representation of p like "(3,4)".
        public static @string String(this Point p)
        {
            return "(" + strconv.Itoa(p.X) + "," + strconv.Itoa(p.Y) + ")";
        }

        // Add returns the vector p+q.
        public static Point Add(this Point p, Point q)
        {
            return new Point(p.X+q.X,p.Y+q.Y);
        }

        // Sub returns the vector p-q.
        public static Point Sub(this Point p, Point q)
        {
            return new Point(p.X-q.X,p.Y-q.Y);
        }

        // Mul returns the vector p*k.
        public static Point Mul(this Point p, long k)
        {
            return new Point(p.X*k,p.Y*k);
        }

        // Div returns the vector p/k.
        public static Point Div(this Point p, long k)
        {
            return new Point(p.X/k,p.Y/k);
        }

        // In reports whether p is in r.
        public static bool In(this Point p, Rectangle r)
        {
            return r.Min.X <= p.X && p.X < r.Max.X && r.Min.Y <= p.Y && p.Y < r.Max.Y;
        }

        // Mod returns the point q in r such that p.X-q.X is a multiple of r's width
        // and p.Y-q.Y is a multiple of r's height.
        public static Point Mod(this Point p, Rectangle r)
        {
            var w = r.Dx();
            var h = r.Dy();
            p = p.Sub(r.Min);
            p.X = p.X % w;
            if (p.X < 0L)
            {
                p.X += w;
            }
            p.Y = p.Y % h;
            if (p.Y < 0L)
            {
                p.Y += h;
            }
            return p.Add(r.Min);
        }

        // Eq reports whether p and q are equal.
        public static bool Eq(this Point p, Point q)
        {
            return p == q;
        }

        // ZP is the zero Point.
        public static Point ZP = default;

        // Pt is shorthand for Point{X, Y}.
        public static Point Pt(long X, long Y)
        {
            return new Point(X,Y);
        }

        // A Rectangle contains the points with Min.X <= X < Max.X, Min.Y <= Y < Max.Y.
        // It is well-formed if Min.X <= Max.X and likewise for Y. Points are always
        // well-formed. A rectangle's methods always return well-formed outputs for
        // well-formed inputs.
        //
        // A Rectangle is also an Image whose bounds are the rectangle itself. At
        // returns color.Opaque for points in the rectangle and color.Transparent
        // otherwise.
        public partial struct Rectangle
        {
            public Point Min;
            public Point Max;
        }

        // String returns a string representation of r like "(3,4)-(6,5)".
        public static @string String(this Rectangle r)
        {
            return r.Min.String() + "-" + r.Max.String();
        }

        // Dx returns r's width.
        public static long Dx(this Rectangle r)
        {
            return r.Max.X - r.Min.X;
        }

        // Dy returns r's height.
        public static long Dy(this Rectangle r)
        {
            return r.Max.Y - r.Min.Y;
        }

        // Size returns r's width and height.
        public static Point Size(this Rectangle r)
        {
            return new Point(r.Max.X-r.Min.X,r.Max.Y-r.Min.Y,);
        }

        // Add returns the rectangle r translated by p.
        public static Rectangle Add(this Rectangle r, Point p)
        {
            return new Rectangle(Point{r.Min.X+p.X,r.Min.Y+p.Y},Point{r.Max.X+p.X,r.Max.Y+p.Y},);
        }

        // Sub returns the rectangle r translated by -p.
        public static Rectangle Sub(this Rectangle r, Point p)
        {
            return new Rectangle(Point{r.Min.X-p.X,r.Min.Y-p.Y},Point{r.Max.X-p.X,r.Max.Y-p.Y},);
        }

        // Inset returns the rectangle r inset by n, which may be negative. If either
        // of r's dimensions is less than 2*n then an empty rectangle near the center
        // of r will be returned.
        public static Rectangle Inset(this Rectangle r, long n)
        {
            if (r.Dx() < 2L * n)
            {
                r.Min.X = (r.Min.X + r.Max.X) / 2L;
                r.Max.X = r.Min.X;
            }
            else
            {
                r.Min.X += n;
                r.Max.X -= n;
            }
            if (r.Dy() < 2L * n)
            {
                r.Min.Y = (r.Min.Y + r.Max.Y) / 2L;
                r.Max.Y = r.Min.Y;
            }
            else
            {
                r.Min.Y += n;
                r.Max.Y -= n;
            }
            return r;
        }

        // Intersect returns the largest rectangle contained by both r and s. If the
        // two rectangles do not overlap then the zero rectangle will be returned.
        public static Rectangle Intersect(this Rectangle r, Rectangle s)
        {
            if (r.Min.X < s.Min.X)
            {
                r.Min.X = s.Min.X;
            }
            if (r.Min.Y < s.Min.Y)
            {
                r.Min.Y = s.Min.Y;
            }
            if (r.Max.X > s.Max.X)
            {
                r.Max.X = s.Max.X;
            }
            if (r.Max.Y > s.Max.Y)
            {
                r.Max.Y = s.Max.Y;
            } 
            // Letting r0 and s0 be the values of r and s at the time that the method
            // is called, this next line is equivalent to:
            //
            // if max(r0.Min.X, s0.Min.X) >= min(r0.Max.X, s0.Max.X) || likewiseForY { etc }
            if (r.Empty())
            {
                return ZR;
            }
            return r;
        }

        // Union returns the smallest rectangle that contains both r and s.
        public static Rectangle Union(this Rectangle r, Rectangle s)
        {
            if (r.Empty())
            {
                return s;
            }
            if (s.Empty())
            {
                return r;
            }
            if (r.Min.X > s.Min.X)
            {
                r.Min.X = s.Min.X;
            }
            if (r.Min.Y > s.Min.Y)
            {
                r.Min.Y = s.Min.Y;
            }
            if (r.Max.X < s.Max.X)
            {
                r.Max.X = s.Max.X;
            }
            if (r.Max.Y < s.Max.Y)
            {
                r.Max.Y = s.Max.Y;
            }
            return r;
        }

        // Empty reports whether the rectangle contains no points.
        public static bool Empty(this Rectangle r)
        {
            return r.Min.X >= r.Max.X || r.Min.Y >= r.Max.Y;
        }

        // Eq reports whether r and s contain the same set of points. All empty
        // rectangles are considered equal.
        public static bool Eq(this Rectangle r, Rectangle s)
        {
            return r == s || r.Empty() && s.Empty();
        }

        // Overlaps reports whether r and s have a non-empty intersection.
        public static bool Overlaps(this Rectangle r, Rectangle s)
        {
            return !r.Empty() && !s.Empty() && r.Min.X < s.Max.X && s.Min.X < r.Max.X && r.Min.Y < s.Max.Y && s.Min.Y < r.Max.Y;
        }

        // In reports whether every point in r is in s.
        public static bool In(this Rectangle r, Rectangle s)
        {
            if (r.Empty())
            {
                return true;
            } 
            // Note that r.Max is an exclusive bound for r, so that r.In(s)
            // does not require that r.Max.In(s).
            return s.Min.X <= r.Min.X && r.Max.X <= s.Max.X && s.Min.Y <= r.Min.Y && r.Max.Y <= s.Max.Y;
        }

        // Canon returns the canonical version of r. The returned rectangle has minimum
        // and maximum coordinates swapped if necessary so that it is well-formed.
        public static Rectangle Canon(this Rectangle r)
        {
            if (r.Max.X < r.Min.X)
            {
                r.Min.X = r.Max.X;
                r.Max.X = r.Min.X;
            }
            if (r.Max.Y < r.Min.Y)
            {
                r.Min.Y = r.Max.Y;
                r.Max.Y = r.Min.Y;
            }
            return r;
        }

        // At implements the Image interface.
        public static color.Color At(this Rectangle r, long x, long y)
        {
            if ((new Point(x,y)).In(r))
            {
                return color.Opaque;
            }
            return color.Transparent;
        }

        // Bounds implements the Image interface.
        public static Rectangle Bounds(this Rectangle r)
        {
            return r;
        }

        // ColorModel implements the Image interface.
        public static color.Model ColorModel(this Rectangle r)
        {
            return color.Alpha16Model;
        }

        // ZR is the zero Rectangle.
        public static Rectangle ZR = default;

        // Rect is shorthand for Rectangle{Pt(x0, y0), Pt(x1, y1)}. The returned
        // rectangle has minimum and maximum coordinates swapped if necessary so that
        // it is well-formed.
        public static Rectangle Rect(long x0, long y0, long x1, long y1)
        {
            if (x0 > x1)
            {
                x0 = x1;
                x1 = x0;
            }
            if (y0 > y1)
            {
                y0 = y1;
                y1 = y0;
            }
            return new Rectangle(Point{x0,y0},Point{x1,y1});
        }
    }
}
