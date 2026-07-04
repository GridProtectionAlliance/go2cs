namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Δp {
    internal nint id;
}

[GoType] partial struct tagger {
}

internal static void p(this tagger _) {
}

[GoType] partial struct box {
    internal nint n;
}

internal static void setN(ж<box> Ꮡb, nint v) {
    ref var b = ref Ꮡb.Value;

    b.n = v;
}

internal static void bump(ж<nint> Ꮡnp) {
    ref var np = ref Ꮡnp.Value;

    np += 100;
}

internal static nint usesTypeP() {
    Δp pv = default!;
    pv.id = 1;
    tagger t = default!;
    t.p();
    return pv.id;
}

internal static void Main() {
    ref var Δp = ref heap<box>(out var Ꮡp);
    Δp = new box(n: 0);
    setN(Ꮡp, 7);
    setN(Ꮡp, Δp.n + 3);
    bump(Ꮡp.of(box.Ꮡn));
    fmt.Println(Δp.n, usesTypeP());
}

} // end main_package
