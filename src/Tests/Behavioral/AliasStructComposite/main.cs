global using alias = go.main_package.Inner;
global using words = go.array<ulong>;

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

internal static uint64 sum(words w) {
    uint64 s = default!;
    for (nint i = 0; i < len(w); i++) {
        s += w[i];
    }
    return s;
}

internal static void Main() {
    var a = keyed();
    var b = empty();
    var c = positional();
    fmt.Println(a.V, a.W, b.V, c.V, c.W);
    var w = new uint64[]{10, 20, 30, 40}.array();
    fmt.Println(sum(w), len(w));
}

} // end main_package
