namespace go;

using fmt = fmt_package;
using ꓸꓸꓸShape = Span<main_package.Shape>;

partial class main_package {

[GoType] partial interface Shape {
    nint Area();
}

[GoType] partial struct Rect {
    internal nint w, h;
}

[GoRecv] public static nint Area(this ref Rect r) {
    return r.w * r.h;
}

[GoType] partial struct Circle {
    internal nint r;
}

[GoRecv] public static nint Area(this ref Circle c) {
    return 3 * c.r * c.r;
}

internal static ж<Rect> newRect(nint w, nint h) {
    return Ꮡ(new Rect(w: w, h: h));
}

internal static nint totalArea(nint scale, params ꓸꓸꓸShape shapesʗp) {
    var shapes = shapesʗp.slice();

    nint sum = 0;
    foreach (var (_, s) in shapes) {
        sum += s.Area();
    }
    return sum * scale;
}

internal static void Main() {
    fmt.Println(totalArea(2, new RectжShape(newRect(3, 4)), new CircleжShape(Ꮡ(new Circle(r: 2))), new RectжShape(newRect(1, 5))));
    var r = Ꮡ(new Rect(w: 4, h: 2));
    Shape s = new CircleжShape(Ꮡ(new Circle(r: 3)));
    fmt.Println(totalArea(1, s, new RectжShape(r)));
    fmt.Println(totalArea(3));
    var shapes = new Shape[]{new RectжShape(Ꮡ(new Rect(w: 2, h: 2))), new CircleжShape(Ꮡ(new Circle(r: 1)))}.slice();
    fmt.Println(totalArea(1, shapes.ꓸꓸꓸ));
}

} // end main_package
