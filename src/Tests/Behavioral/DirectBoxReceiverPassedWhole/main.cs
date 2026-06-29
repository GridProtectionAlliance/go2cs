namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct mc {
    internal nint n;
}

internal static void clearViaPtr(ж<mc> Ꮡc) {
    ref var c = ref Ꮡc.val;

    c.n = 0;
}

internal static void bump(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p++;
}

internal static void reset(this ж<mc> Ꮡc) {
    ref var c = ref Ꮡc.val;

    bump(Ꮡc.of(mc.Ꮡn));
    clearViaPtr(Ꮡc);
}

internal static void Main() {
    var c = Ꮡ(new mc(n: 9));
    c.reset();
    fmt.Println((~c).n);
}

} // end main_package
