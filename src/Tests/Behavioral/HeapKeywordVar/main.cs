namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void set(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p = 42;
}

[GoType] partial struct @decimal {
    internal nint d;
}

[GoRecv] internal static @string String(this ref @decimal a) {
    return fmt.Sprint(a.d);
}

[GoRecv] internal static void Assign(this ref @decimal a, nint v) {
    a.d = v;
}

internal static void Main() {
    ref var @base = ref heap(new nint(), out var Ꮡbase);
    ref var @as = ref heap(new nint(), out var Ꮡas);
    ref var @event = ref heap(new nint(), out var Ꮡevent);
    set(Ꮡbase);
    set(Ꮡas);
    set(Ꮡevent);
    @base += 1;
    fmt.Println(@base, @as, @event);
    @decimal dec = default!;
    dec.Assign(7);
    fmt.Println(dec.String());
}

} // end main_package
