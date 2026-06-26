// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using color = image.color_package;
using image;

partial class image_package {

public static ж<Uniform> Black = NewUniform(color.Black);
public static ж<Uniform> White = NewUniform(color.White);
public static ж<Uniform> Transparent = NewUniform(color.Transparent);
public static ж<Uniform> ΔOpaque = NewUniform(color.Opaque);

// Uniform is an infinite-sized [Image] of uniform color.
// It implements the [color.Color], [color.Model], and [Image] interfaces.
[GoType] partial struct Uniform {
    public image.color_package.Color C;
}

[GoRecv] public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this ref Uniform c) {
    uint32 r = default!;
    uint32 g = default!;
    uint32 b = default!;
    uint32 a = default!;

    return c.C.RGBA();
}

[GoRecv("capture")] public static color.Model ColorModel(this ref Uniform c) {
    return ~c;
}

[GoRecv] public static color.Color Convert(this ref Uniform c, color.Color _) {
    return c.C;
}

[GoRecv] public static Rectangle Bounds(this ref Uniform c) {
    return new Rectangle(new Point(-1e9F, -1e9F), new Point(1e9F, 1e9F));
}

[GoRecv] public static color.Color At(this ref Uniform c, nint x, nint y) {
    return c.C;
}

[GoRecv] public static color.RGBA64 RGBA64At(this ref Uniform c, nint x, nint y) {
    var (r, g, b, a) = c.C.RGBA();
    return new color.RGBA64(((uint16)r), ((uint16)g), ((uint16)b), ((uint16)a));
}

// Opaque scans the entire image and reports whether it is fully opaque.
[GoRecv] public static bool Opaque(this ref Uniform c) {
    var (_, _, _, a) = c.C.RGBA();
    return a == 65535;
}

// NewUniform returns a new [Uniform] image of the given color.
public static ж<Uniform> NewUniform(color.Color c) {
    return Ꮡ(new Uniform(c));
}

} // end image_package
