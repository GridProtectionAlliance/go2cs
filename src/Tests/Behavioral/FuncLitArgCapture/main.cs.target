namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal nint x;
    internal nint y;
}

internal static void run(Action f) {
    f();
}

internal static void set(ж<box> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p.x = 42;
}

internal static void Main() {
    ref var m = ref heap(new box(), out var Ꮡm);
    run(() => {
        set(Ꮡm);
    });
    fmt.Println("1:", m.x);
    ref var n = ref heap(new box(), out var Ꮡn);
    run(() => {
        set(Ꮡn);
        Ꮡn.val.y = Ꮡn.val.x + 1;
    });
    fmt.Println("2:", n.x, n.y);
    ref var c = ref heap(new box(), out var Ꮡc);
    run(() => {
        var p = Ꮡc.of(box.Ꮡx);
        p.val = 99;
    });
    fmt.Println("3:", c.x);
    ref var d = ref heap(new box(), out var Ꮡd);
    var f = () => {
        set(Ꮡd);
    };
    f();
    fmt.Println("4:", d.x);
}

} // end main_package
