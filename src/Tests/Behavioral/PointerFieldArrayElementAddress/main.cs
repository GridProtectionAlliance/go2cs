namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct cycle {
    internal nint n;
}

[GoType] partial struct rec {
    internal array<cycle> future = new(3);
}

[GoType] partial struct holder {
    internal ж<rec> r;
}

internal static void bump(ж<cycle> Ꮡc) {
    ref var c = ref Ꮡc.val;

    c.n++;
}

internal static void viaParam(ж<rec> Ꮡp, nint i) {
    ref var p = ref Ꮡp.val;

    var c = Ꮡp.of(rec.Ꮡfuture).at<cycle>(i);
    bump(c);
}

internal static void viaLocal(ж<holder> Ꮡh, nint i) {
    ref var h = ref Ꮡh.val;

    var p = h.r;
    var c = p.of(rec.Ꮡfuture).at<cycle>(i);
    bump(c);
}

internal static void Main() {
    ref var r = ref heap<rec>(out var Ꮡr);
    r = new rec(future: new cycle[]{new(0), new(0), new(0)}.array());
    viaParam(Ꮡr, 0);
    viaParam(Ꮡr, 0);
    fmt.Println(r.future[0].n);
    var h = Ꮡ(new holder(r: Ꮡr));
    viaLocal(h, 1);
    fmt.Println(r.future[1].n);
}

} // end main_package
