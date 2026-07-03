namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct inner {
    internal nint a, b;
}

[GoType] partial struct outer {
    internal inner mid;
    internal nint tag;
}

internal static ж<outer> Ꮡg = new(default(outer));
internal static ref outer g => ref Ꮡg.Value;

internal static void keep(ж<outer> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    _ = Ꮡp;
}

internal static void mutate() {
    var p = Ꮡg.of(outer.Ꮡmid).of(inner.Ꮡa);
    p.Value = 42;
    var q = Ꮡg.of(outer.Ꮡmid);
    q.Value.b = 7;
}

internal static void Main() {
    keep(Ꮡg);
    mutate();
    fmt.Println(g.mid.a, g.mid.b, g.tag);
}

} // end main_package
