namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal int64 n;
}

internal static ж<counter> ptr(this ж<counter> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return Ꮡc;
}

internal static void inc(this ж<counter> Ꮡc) {
    ref var c = ref Ꮡc.val;

    Ꮡc.ptr().val.n++;
}

[GoType] partial struct holder {
    internal counter c;
}

[GoType] partial struct box {
    internal holder h;
}

internal static void viaParam(ж<box> Ꮡb) {
    ref var b = ref Ꮡb.val;

    Ꮡb.of(box.Ꮡh).of(holder.Ꮡc).inc();
}

[GoType] partial struct wrapper {
    internal box b;
}

internal static ж<wrapper> self(this ж<wrapper> Ꮡw) {
    ref var w = ref Ꮡw.val;

    return Ꮡw;
}

internal static void bump(this ж<wrapper> Ꮡw) {
    ref var w = ref Ꮡw.val;

    _ = Ꮡw.self();
    Ꮡw.of(wrapper.Ꮡb).of(box.Ꮡh).of(holder.Ꮡc).inc();
}

internal static void Main() {
    var b = Ꮡ(new box(nil));
    viaParam(b);
    viaParam(b);
    fmt.Println((~b).h.c.n);
    var w = Ꮡ(new wrapper(nil));
    w.bump();
    w.bump();
    w.bump();
    fmt.Println((~w).b.h.c.n);
}

} // end main_package
