namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface I {
    @string m();
}

[GoType] partial struct T {
    public @string name;
}

private static @string m(this T @ref) {
    return @ref.name;
}

[GoType] partial struct S {
    public @string name;
}

private static @string m(this S @ref) {
    return "Am I an I?"u8;
}

private static void Main() {
    var whatAmI = (object i) => {
        switch (i.type()) {
        case bool t:
            fmt.Println("I'm a bool");
            break;
        case nint t:
            fmt.Println("I'm an int");
            break;
        case int32 t:
            fmt.Println("I'm an int");
            break;
        default: {
            var t = i.type();
            fmt.Printf("Don't know type %T\n"u8, t);
            break;
        }}
    };
    whatAmI(true);
    whatAmI(1);
    whatAmI("hey");
    nint x;
    nint i = x._<nint>();
    fmt.Println(i);
    T y = default!;
    y.name = "Me"u8;
    f(y);
    object s = new S("you");
    switch (s.type()) {
    default:
        fmt.Println("S is not an I");
        break;
    case I :
        fmt.Println("S is an I!!");
        break;
    case default! :
        fmt.Println("S is nil or an int");
        break;
    case nint :
        fmt.Println("S is nil or an int");
        break;
    case int32 :
        fmt.Println("S is nil or an int");
        break;
    }
}

private static void f(I y) {
    fmt.Println(y.m());
}

} // end main_package
