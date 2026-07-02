namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct inner {
    internal int64 a;
    internal int64 b;
}

[GoType("inner")] partial struct wrapper;

[GoType] partial struct box {
    internal wrapper w;
}

internal static void fill(ж<box> Ꮡb) {
    ref var b = ref Ꮡb.val;

    var c = Ꮡb.of(box.Ꮡw);
    c.val.a = 10;
    c.val.b = 20;
}

internal static int64 readBack(ж<box> Ꮡb) {
    ref var b = ref Ꮡb.val;

    var c = Ꮡb.of(box.Ꮡw);
    return (~c).a + (~c).b;
}

internal static void bump(ж<int64> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = p + 7;
}

internal static void Main() {
    ref var b = ref heap(new box(), out var Ꮡb);
    fill(Ꮡb);
    fmt.Println(b.w.a, b.w.b);
    fmt.Println(readBack(Ꮡb));
    var c = Ꮡb.of(box.Ꮡw);
    bump(c.of(wrapper.Ꮡa));
    fmt.Println(b.w.a, readBack(Ꮡb));
}

} // end main_package
