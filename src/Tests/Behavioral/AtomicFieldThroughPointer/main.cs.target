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

[GoType] partial struct profile {
    internal atom wait;
}

[GoType] partial struct holder {
    internal atom wait;
    internal profile prof;
}

[GoType] partial struct owner {
    internal ж<holder> h;
    internal ж<holder> deep;
}

internal static void bump(ж<owner> Ꮡo, int64 d) {
    ref var o = ref Ꮡo.val;

    o.h.of(holder.Ꮡwait).add(d);
    o.deep.of(holder.Ꮡprof).of(profile.Ꮡwait).add(d);
}

internal static int64 viaPointerLocal(ж<owner> Ꮡo, int64 d) {
    ref var o = ref Ꮡo.val;

    var mp = o.h;
    mp.of(holder.Ꮡprof).of(profile.Ꮡwait).add(d);
    mp.of(holder.Ꮡprof).of(profile.Ꮡwait).add(d);
    return mp.of(holder.Ꮡprof).of(profile.Ꮡwait).get();
}

internal static void Main() {
    var o = Ꮡ(new owner(h: Ꮡ(new holder(nil)), deep: Ꮡ(new holder(nil))));
    bump(o, 5);
    bump(o, 5);
    fmt.Println((~o).h.of(holder.Ꮡwait).get());
    fmt.Println((~o).deep.of(holder.Ꮡprof).of(profile.Ꮡwait).get());
    var o2 = Ꮡ(new owner(h: Ꮡ(new holder(nil))));
    fmt.Println(viaPointerLocal(o2, 4));
}

} // end main_package
