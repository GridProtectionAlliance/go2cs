// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package image -- go2cs converted at 2022 March 13 06:43:54 UTC
// import "image" ==> using image = go.image_package
// Original source: C:\Program Files\Go\src\image\names.go
namespace go;

using color = image.color_package;

public static partial class image_package {

 
// Black is an opaque black uniform image.
public static var Black = NewUniform(color.Black);public static var White = NewUniform(color.White);public static var Transparent = NewUniform(color.Transparent);public static var Opaque = NewUniform(color.Opaque);

// Uniform is an infinite-sized Image of uniform color.
// It implements the color.Color, color.Model, and Image interfaces.
public partial struct Uniform {
    public color.Color C;
}

private static (uint, uint, uint, uint) RGBA(this ptr<Uniform> _addr_c) {
    uint r = default;
    uint g = default;
    uint b = default;
    uint a = default;
    ref Uniform c = ref _addr_c.val;

    return c.C.RGBA();
}

private static color.Model ColorModel(this ptr<Uniform> _addr_c) {
    ref Uniform c = ref _addr_c.val;

    return c;
}

private static color.Color Convert(this ptr<Uniform> _addr_c, color.Color _p0) {
    ref Uniform c = ref _addr_c.val;

    return c.C;
}

private static Rectangle Bounds(this ptr<Uniform> _addr_c) {
    ref Uniform c = ref _addr_c.val;

    return new Rectangle(Point{-1e9,-1e9},Point{1e9,1e9});
}

private static color.Color At(this ptr<Uniform> _addr_c, nint x, nint y) {
    ref Uniform c = ref _addr_c.val;

    return c.C;
}

private static color.RGBA64 RGBA64At(this ptr<Uniform> _addr_c, nint x, nint y) {
    ref Uniform c = ref _addr_c.val;

    var (r, g, b, a) = c.C.RGBA();
    return new color.RGBA64(uint16(r),uint16(g),uint16(b),uint16(a));
}

// Opaque scans the entire image and reports whether it is fully opaque.
private static bool Opaque(this ptr<Uniform> _addr_c) {
    ref Uniform c = ref _addr_c.val;

    var (_, _, _, a) = c.C.RGBA();
    return a == 0xffff;
}

// NewUniform returns a new Uniform image of the given color.
public static ptr<Uniform> NewUniform(color.Color c) {
    return addr(new Uniform(c));
}

} // end image_package
