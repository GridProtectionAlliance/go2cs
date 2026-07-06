namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Node {
    internal nint a, b;
}

internal static (nint, nint) parts() {
    return (3, 4);
}

internal static ж<Node> makeNode(nint a, nint b) {
    return Ꮡ(new Node(a: a, b: b));
}

internal static nint combine(nint a, nint b) {
    return a * 10 + b;
}

internal static ж<Node> build() {
    var (ᴛ1, ᴛ2) = parts();
    var r = makeNode(ᴛ1, ᴛ2);
    return r;
}

internal static void Main() {
    var n = build();
    var (ᴛ3, ᴛ4) = parts();
    nint s = combine(ᴛ3, ᴛ4);
    fmt.Println((~n).a, (~n).b, s);
}

} // end main_package
