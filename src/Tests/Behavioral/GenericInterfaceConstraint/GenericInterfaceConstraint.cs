namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Shape {
    float64 Area();
    @string Name();
}

[GoType] partial interface Round :
    Shape
{
    float64 Diameter();
}

[GoType] partial struct Circle {
    public float64 R;
}

[GoRecv] public static float64 Area(this ref Circle c) {
    return 3.0D * c.R * c.R;
}

[GoRecv] public static @string Name(this ref Circle c) {
    return "circle"u8;
}

[GoRecv] public static float64 Diameter(this ref Circle c) {
    return 2.0D * c.R;
}

[GoType] partial struct Square {
    public float64 S;
}

[GoRecv] public static float64 Area(this ref Square s) {
    return s.S * s.S;
}

[GoRecv] public static @string Name(this ref Square s) {
    return "square"u8;
}

internal static float64 totalArea<S>(slice<S> shapes)
    where S : Shape
{
    float64 sum = default!;
    foreach (var (_, s) in shapes) {
        sum += s.Area();
    }
    return sum;
}

internal static void walkAll<S>(slice<S> shapes)
    where S : Shape
{
    foreach (var (_, s) in shapes) {
        show(s);
    }
}

internal static void show(Shape s) {
    fmt.Printf("%s: %.2f\n"u8, s.Name(), s.Area());
}

internal static void Main() {
    var circles = new ж<Circle>[]{Ꮡ(new Circle(R: 1D)), Ꮡ(new Circle(R: 2D))}.slice();
    var squares = new ж<Square>[]{Ꮡ(new Square(S: 3D))}.slice();
    var shapes = new Shape[]{new CircleжShape(Ꮡ(new Circle(R: 1D))), new SquareжShape(Ꮡ(new Square(S: 2D)))}.slice();
    var rounds = new Round[]{new CircleжRound(Ꮡ(new Circle(R: 4D)))}.slice();
    fmt.Printf("circles: %.2f\n"u8, totalArea(widen<ж<Circle>, Shape>(circles, elemᴛ0 => new CircleжShape(elemᴛ0))));
    fmt.Printf("squares: %.2f\n"u8, totalArea(widen<ж<Square>, Shape>(squares, elemᴛ0 => new SquareжShape(elemᴛ0))));
    fmt.Printf("shapes: %.2f\n"u8, totalArea(shapes));
    fmt.Printf("rounds: %.2f\n"u8, totalArea(rounds));
    walkAll(widen<ж<Circle>, Shape>(circles, elemᴛ0 => new CircleжShape(elemᴛ0)));
    walkAll(shapes);
    walkAll(rounds);
}

} // end main_package
