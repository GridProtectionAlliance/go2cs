namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct inner {
    internal nint n;
}

[GoRecv] internal static void bump(this ref inner i, nint d) {
    i.n += d;
}

[GoRecv] internal static void reset0(this ref inner i) {
    i.n = 0;
}

[GoRecv] internal static nint total(this ref inner i) {
    return i.n;
}

[GoType] partial struct outer {
    internal nint tag;
    internal partial ref inner inner { get; }
}

internal static void viaParam(ж<outer> Ꮡo) {
    ref var o = ref Ꮡo.val;

    Ꮡo.of(outer.Ꮡinner).bump(100);
}

internal static void Main() {
    var o = Ꮡ(new outer(tag: 7));
    o.of(outer.Ꮡinner).bump(5);
    o.of(outer.Ꮡinner).bump(3);
    viaParam(o);
    fmt.Println(o.of(outer.Ꮡinner).total());
    fmt.Println((~o).n);
    fmt.Println((~o).inner.n);
    fmt.Println((~o).tag);
    o.of(outer.Ꮡinner).reset0();
    fmt.Println((~o).n);
}

} // end main_package
