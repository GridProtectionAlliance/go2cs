namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct @base {
    internal int64 a;
    internal int64 b;
}

[GoType("@base")] partial struct view;

internal static ж<@base> makeBase() {
    ref var x = ref heap<@base>(out var Ꮡx);
    x = new @base(a: 10, b: 20);
    return Ꮡx;
}

internal static void Main() {
    var pb = makeBase();
    var pv = Ꮡ((view)(~pb));
    var bb = ((@base)(pv.Value));
    fmt.Println(bb.a, bb.b);
}

} // end main_package
