namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface shape {
    @string name();
}

[GoType] partial struct circle {
    internal nint r;
}

[GoRecv] internal static @string name(this ref circle c) {
    return "circle"u8;
}

[GoType] partial struct square {
    internal nint s;
}

[GoRecv] internal static @string name(this ref square q) {
    return "square"u8;
}

[GoType] partial struct dot {
    internal nint tag;
}

internal static @string name(this dot d) {
    return "dot"u8;
}

internal static @string classify(shape v) {
    switch (v.type()) {
    case ж<circle> t: {
        return "ptr "u8 + t.name();
    }
    case ж<square> t: {
        return "ptr "u8 + t.name();
    }
    case dot t: {
        return "val "u8 + t.name();
    }
    case null: {
        return "nil shape"u8;
    }
    default: {
        var t = v;
        return "unknown"u8;
    }}
}

internal static @string classifyMulti(shape v) {
    switch (v.type()) {
    case ж<circle> _:
    case ж<square> _: {
        var t = v;
        return "multi ptr "u8 + t.name();
    }
    case dot t: {
        return "multi val "u8 + t.name();
    }
    default: {
        var t = v;
        return "multi other"u8;
    }}
}

internal static void grow(shape v) {
    switch (v.type()) {
    case ж<circle> t: {
        t.Value.r += 10;
        break;
    }
    case ж<square> t: {
        t.Value.s += 10;
        break;
    }}
}

internal static void Main() {
    var c = Ꮡ(new circle(r: 1));
    var q = Ꮡ(new square(s: 2));
    var d = new dot(tag: 3);
    fmt.Println(classify(new circleжshape(c)));
    fmt.Println(classify(new squareжshape(q)));
    fmt.Println(classify(d));
    fmt.Println(classify(default!));
    fmt.Println(classifyMulti(new circleжshape(c)));
    fmt.Println(classifyMulti(new squareжshape(q)));
    fmt.Println(classifyMulti(d));
    grow(new circleжshape(c));
    grow(new squareжshape(q));
    fmt.Println((~c).r, (~q).s);
    any x = c;
    switch (x.type()) {
    case ж<circle>: {
        fmt.Println("any holds *circle");
        break;
    }
    default: {
        fmt.Println("any miss");
        break;
    }}

    shape v = new squareжshape(q);
    switch (v.type()) {
    case ж<square>: {
        fmt.Println("shape holds *square");
        break;
    }
    default: {
        fmt.Println("shape miss");
        break;
    }}

}

} // end main_package
