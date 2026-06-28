global using alias = go.main_package.Inner;

namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Inner {
    public nint V;
    public nint W;
}

internal static alias keyed() {
    return new alias(V: 7, W: 3);
}

internal static alias empty() {
    return new alias();
}

internal static alias positional() {
    return new alias(1, 2);
}

internal static void Main() {
    var a = keyed();
    var b = empty();
    var c = positional();
    fmt.Println(a.V, a.W, b.V, c.V, c.W);
}

} // end main_package
