namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void setViaDefer(ж<nint> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    defer(() => {
        Ꮡp.Value = 42;
    });
});

internal static void bumpInClosure(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    var add = () => {
        Ꮡp.Value = Ꮡp.Value + 1;
    };
    add();
    add();
}

internal static void mixed(ж<nint> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    p = 5;
    defer(() => {
        Ꮡp.Value = Ꮡp.Value * 10;
    });
});

internal static void Main() {
    ref var a = ref heap(new nint(), out var Ꮡa);
    setViaDefer(Ꮡa);
    fmt.Println(a);
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 10;
    bumpInClosure(Ꮡb);
    fmt.Println(b);
    ref var c = ref heap<nint>(out var Ꮡc);
    c = 0;
    mixed(Ꮡc);
    fmt.Println(c);
}

} // end main_package
