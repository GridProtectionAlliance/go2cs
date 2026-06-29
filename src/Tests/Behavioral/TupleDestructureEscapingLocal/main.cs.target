namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct gList {
    internal nint n;
}

internal static (gList, nint) makeList() {
    return (new gList(n: 7), 3);
}

internal static void use(ж<gList> Ꮡg) {
    ref var g = ref Ꮡg.val;

    g.n++;
}

internal static (nint, nint) run() {
    ref var list = ref heap<gList>(out var Ꮡlist);
    (list, var delta) = makeList();
    use(Ꮡlist);
    use(Ꮡlist);
    return (list.n, delta);
}

internal static void Main() {
    var (n, delta) = run();
    fmt.Println(n, delta);
}

} // end main_package
