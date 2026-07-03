namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct mc {
    internal nint n;
}

internal static void clearViaPtr(ж<mc> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    c.n = 0;
}

internal static void bump(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p++;
}

internal static void reset(this ж<mc> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    bump(Ꮡc.of(mc.Ꮡn));
    clearViaPtr(Ꮡc);
}

[GoType] partial struct wrap {
    internal ж<mc> p;
    internal nint tag;
}

internal static wrap wrapped(this ж<mc> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    return new wrap(Ꮡc, 7);
}

internal static wrap keyed(this ж<mc> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    return new wrap(tag: 8, p: Ꮡc);
}

internal static void Main() {
    var c = Ꮡ(new mc(n: 9));
    c.reset();
    fmt.Println((~c).n);
    var w = c.wrapped();
    w.p.Value.n = 42;
    fmt.Println((~c).n, w.tag);
    var k = c.keyed();
    k.p.Value.n++;
    fmt.Println((~c).n, k.tag);
}

} // end main_package
