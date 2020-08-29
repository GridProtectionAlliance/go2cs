// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package image -- go2cs converted at 2020 August 29 10:09:50 UTC
// import "image" ==> using image = go.image_package
// Original source: C:\Go\src\image\names.go
using color = go.image.color_package;
using static go.builtin;

namespace go
{
    public static partial class image_package
    {
 
        // Black is an opaque black uniform image.
        public static var Black = NewUniform(color.Black);        public static var White = NewUniform(color.White);        public static var Transparent = NewUniform(color.Transparent);        public static var Opaque = NewUniform(color.Opaque);

        // Uniform is an infinite-sized Image of uniform color.
        // It implements the color.Color, color.Model, and Image interfaces.
        public partial struct Uniform
        {
            public color.Color C;
        }

        private static (uint, uint, uint, uint) RGBA(this ref Uniform c)
        {
            return c.C.RGBA();
        }

        private static color.Model ColorModel(this ref Uniform c)
        {
            return c;
        }

        private static color.Color Convert(this ref Uniform c, color.Color _p0)
        {
            return c.C;
        }

        private static Rectangle Bounds(this ref Uniform c)
        {
            return new Rectangle(Point{-1e9,-1e9},Point{1e9,1e9});
        }

        private static color.Color At(this ref Uniform c, long x, long y)
        {
            return c.C;
        }

        // Opaque scans the entire image and reports whether it is fully opaque.
        private static bool Opaque(this ref Uniform c)
        {
            var (_, _, _, a) = c.C.RGBA();
            return a == 0xffffUL;
        }

        public static ref Uniform NewUniform(color.Color c)
        {
            return ref new Uniform(c);
        }
    }
}
