namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal nint val;
}

internal static @string onlyNilCheck(ж<node> Ꮡp) {
    if (Ꮡp == nil) {
        return "nil"u8;
    }
    return "set"u8;
}

internal static bool inner(ж<node> Ꮡp) {
    return Ꮡp == nil;
}

internal static bool passThrough(ж<node> Ꮡp) {
    return inner(Ꮡp);
}

internal static nint usesValue(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    return p.val;
}

internal static void Main() {
    fmt.Println(onlyNilCheck(nil));
    fmt.Println(onlyNilCheck(Ꮡ(new node(nil))));
    fmt.Println(passThrough(nil));
    fmt.Println(passThrough(Ꮡ(new node(nil))));
    fmt.Println(usesValue(Ꮡ(new node(val: 9))));
}

} // end main_package
