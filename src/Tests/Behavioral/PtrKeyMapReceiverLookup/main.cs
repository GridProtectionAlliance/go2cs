namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct conn {
    internal nint id;
}

[GoType] partial struct tracker {
    internal map<ж<conn>, @string> m;
}

internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    ref var c = ref Ꮡc.Value;
    ref var t = ref Ꮡt.Value;

    {
        var (s, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
            return s;
        }
    }
    return "unknown"u8;
}

internal static void rename(this ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) {
    ref var c = ref Ꮡc.Value;
    ref var t = ref Ꮡt.Value;

    t.m[Ꮡc] = s;
}

internal static @string label(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
    ref var c = ref Ꮡc.Value;
    ref var t = ref Ꮡt.Value;

    return t.m[Ꮡc];
}

internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;
    ref var t = ref Ꮡt.Value;

    deferǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), "closed", c.id, defer);
    {
        var (_, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
            delete(t.m, Ꮡc);
        }
    }
});

internal static void Main() {
    var a = Ꮡ(new conn(id: 1));
    var b = Ꮡ(new conn(id: 2));
    var t = Ꮡ(new tracker(m: new map<ж<conn>, @string>{[a] = "idle"u8}));
    fmt.Println(a.status(t), b.status(t));
    a.rename(t, "busy"u8);
    fmt.Println(a.label(t), len((~t).m));
    b.rename(t, "new"u8);
    fmt.Println(b.label(t), a.label(t), len((~t).m));
    a.close(t);
    fmt.Println(len((~t).m), a.status(t), b.status(t));
}

} // end main_package
