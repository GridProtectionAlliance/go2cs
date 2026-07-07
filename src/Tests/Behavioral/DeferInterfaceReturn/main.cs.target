namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Shape {
    nint Area();
}

[GoType] partial struct Circle {
    public nint R;
}

public static nint Area(this Circle c) {
    return 3 * c.R * c.R;
}

[GoType] partial struct Square {
    public nint S;
}

public static nint Area(this Square s) {
    return s.S * s.S;
}

internal static Shape pick(nint kind) => func<Shape>((defer, recover) => {
    defer(() => {
        _ = recover();
    });
    if (kind == 0) {
        return new Circle(R: 2);
    }
    return new Square(S: 5);
});

internal static (Shape, bool) classify(nint kind) => func<(Shape, bool)>((defer, recover) => {
    defer(() => {
        _ = recover();
    });
    if (kind == 0) {
        return (new Circle(R: 3), true);
    }
    return (new Square(S: 4), false);
});

internal static void Main() {
    fmt.Println(pick(0).Area());
    fmt.Println(pick(1).Area());
    var (s0, ok0) = classify(0);
    fmt.Println(s0.Area(), ok0);
    var (s1, ok1) = classify(1);
    fmt.Println(s1.Area(), ok1);
}

} // end main_package
