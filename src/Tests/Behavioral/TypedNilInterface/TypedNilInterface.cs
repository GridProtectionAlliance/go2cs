namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct AErr {
    internal nint n;
}

[GoRecv] public static @string Error(this ref AErr e) {
    return "a"u8;
}

[GoType] partial struct BErr {
    internal nint n;
}

[GoRecv] public static @string Error(this ref BErr e) {
    return "b"u8;
}

internal static void Main() {
    any x = ((ж<nint>)nil);
    fmt.Printf("%T\n"u8, x);
    fmt.Println("x==nil", x == default!);
    any y = ((ж<nint>)nil);
    fmt.Println("x==y", AreEqual(x, y));
    ж<nint> p = default!;
    fmt.Println("p==typednil", p == ((ж<nint>)nil));
    error e1 = new AErrжerror(((ж<AErr>)nil));
    error e2 = new BErrжerror(((ж<BErr>)nil));
    fmt.Println("e1==nil", e1 == default!);
    fmt.Println("e2==nil", e2 == default!);
    fmt.Println("e1==e2", AreEqual(e1, e2));
    any ea = ((ж<AErr>)nil);
    fmt.Println("ea==e1", AreEqual(ea, e1));
    switch (e1.type()) {
    case ж<AErr> v: {
        fmt.Println("switch-AErr", v == nil);
        break;
    }
    default: {
        var v = e1;
        fmt.Println("switch-other");
        break;
    }}
}

} // end main_package
