namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct atom {
    internal int64 v;
}

[GoRecv] internal static void add(this ref atom a, int64 d) {
    a.v += d;
}

[GoRecv] internal static int64 get(this ref atom a) {
    return a.v;
}

[GoType] partial struct holder {
    internal atom wait;
}

[GoType] partial struct owner {
    internal ж<holder> h;
}

internal static void bump(ж<owner> Ꮡo, int64 d) {
    ref var o = ref Ꮡo.val;

    o.h.of(holder.Ꮡwait).add(d);
}

internal static void Main() {
    var o = Ꮡ(new owner(h: Ꮡ(new holder(nil))));
    bump(o, 5);
    bump(o, 5);
    fmt.Println((~o).h.of(holder.Ꮡwait).get());
}

} // end main_package
