namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[]nint")] partial struct ints;

[GoRecv] internal static void swap(this ref ints p, nint i, nint j) {
    ((p)[i], (p)[j]) = ((p)[j], (p)[i]);
}

[GoRecv] internal static void set(this ref ints p, nint i, nint v) {
    (p)[i] = v;
}

internal static void show(ж<ints> Ꮡp) {
    ref var p = ref Ꮡp.ValueSlot;

    foreach (var (i, x) in p) {
        if (i > 0) {
            fmt.Print(" ");
        }
        fmt.Print(x);
    }
    fmt.Println();
}

internal static void Main() {
    var p = Ꮡ(new ints(new nint[]{10, 20, 30, 40}.slice()));
    p.swap(0, 3);
    p.swap(1, 2);
    p.set(0, 99);
    show(p);
    var q = Ꮡ(new ints(new nint[]{1, 2, 3, 4, 5}.slice()));
    for ((nint i, nint j) = (0, len(q.ValueSlot) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        q.swap(i, j);
    }
    show(q);
}

} // end main_package
