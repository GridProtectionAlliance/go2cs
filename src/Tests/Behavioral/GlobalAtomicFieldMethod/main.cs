namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

[GoType] partial struct controller {
    internal atomic.Int64 total;
}

internal static ж<controller> Ꮡctrl = new(default(controller));
internal static ref controller ctrl => ref Ꮡctrl.val;

internal static void keep(ж<controller> Ꮡc) {
    ref var c = ref Ꮡc.val;

    _ = Ꮡc;
}

internal static void bump() {
    Ꮡctrl.of(controller.Ꮡtotal).Add(5);
}

internal static void Main() {
    keep(Ꮡctrl);
    bump();
    bump();
    fmt.Println(Ꮡctrl.of(controller.Ꮡtotal).Load());
}

} // end main_package
