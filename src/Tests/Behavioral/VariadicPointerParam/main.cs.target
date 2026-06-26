namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal nint v;
}

internal static nint total(params Span<ж<box>> bsʗp) {
    var bs = bsʗp.slice();

    nint sum = 0;
    foreach (var (_, b) in bs) {
        sum += b.val.v;
    }
    return sum;
}

internal static void Main() {
    var a = Ꮡ(new box(v: 1));
    var b = Ꮡ(new box(v: 2));
    var c = Ꮡ(new box(v: 3));
    fmt.Println(total(a, b, c));
    fmt.Println(total());
    fmt.Println(total(a));
    var boxes = new ж<box>[]{a, b, c, Ꮡ(new box(v: 4))}.slice();
    fmt.Println(total(boxes.ꓸꓸꓸ));
}

} // end main_package
