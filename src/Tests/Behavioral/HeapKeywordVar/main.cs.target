namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void set(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = 42;
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
}

} // end main_package
