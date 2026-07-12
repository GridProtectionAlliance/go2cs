namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface shape {
    @string name();
}

[GoType] partial struct circle {
    internal nint r;
}

[GoType] partial struct square {
    internal nint s;
}

[GoType] partial struct dot {
}

internal static @string name(this circle c) {
    return "circle"u8;
}

internal static @string name(this square s) {
    return "square"u8;
}

internal static @string name(this dot d) {
    return "dot"u8;
}

internal static @string describe(shape v) {
    switch (v.type()) {
    case circle _:
    case square _: {
        var t = v;
        return "both:"u8 + t.name();
    }
    case dot t: {
        return "val:"u8 + t.name();
    }
    default: {
        var t = v;
        return "none"u8;
    }}
}

internal static @string classify(any x) {
    switch (x.type()) {
    case nint _:
    case int32 _:
    case int64 _: {
        var v = x;
        return fmt.Sprintf("integer %v"u8, v);
    }
    case @string _:
    case bool _: {
        var v = x;
        return fmt.Sprintf("text-or-flag %v"u8, v);
    }}
    return "unknown"u8;
}

internal static @string ptrKind(any x) {
    switch (x.type()) {
    case ж<circle> _:
    case ж<square> _: {
        var t = x;
        _ = t;
        return "shape-ptr"u8;
    }
    case ж<dot> t: {
        return "dot-ptr"u8;
    }}
    return "other"u8;
}

internal static @string kind(shape v) {
    switch (v.type()) {
    case null:
    case dot _: {
        var t = v;
        _ = t;
        return "nil-or-dot"u8;
    }
    case circle t: {
        return "circle-val"u8;
    }
    default: {
        var t = v;
        return "boxed"u8;
    }}
}

internal static @string tag(any x) {
    switch (x.type()) {
    case nint _:
    case int32 _:
    case @string _: {
        return "common"u8;
    }
    case float64: {
        return "float"u8;
    }}

    return "rare"u8;
}

internal static void Main() {
    fmt.Println(describe(new circle(1)));
    fmt.Println(describe(new square(2)));
    fmt.Println(describe(new dot(nil)));
    fmt.Println(classify((nint)(1)));
    fmt.Println(classify((int64)2));
    fmt.Println(classify("s"));
    fmt.Println(classify(false));
    fmt.Println(classify(3.5D));
    fmt.Println(ptrKind(Ꮡ(new circle(1))));
    fmt.Println(ptrKind(Ꮡ(new square(2))));
    fmt.Println(ptrKind(Ꮡ(new dot(nil))));
    fmt.Println(ptrKind((nint)(7)));
    fmt.Println(kind(default!));
    fmt.Println(kind(new dot(nil)));
    fmt.Println(kind(new circle(3)));
    fmt.Println(kind(new square(4)));
    fmt.Println(tag((nint)(7)));
    fmt.Println(tag("x"));
    fmt.Println(tag(1.5D));
    fmt.Println(tag(true));
}

} // end main_package
