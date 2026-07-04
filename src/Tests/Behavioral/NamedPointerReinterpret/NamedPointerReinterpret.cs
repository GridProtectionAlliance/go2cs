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

[GoType("ж<int64>")] partial class intRef;

[GoType("@string")] partial struct tail;

[GoRecv] internal static byte chop(this ref tail t) {
    var b = (t)[0];
    t = (t)[1..];
    return b;
}

internal static @string consume(@string s) {
    var t = Ꮡ((tail)(s));
    var b1 = t.chop();
    var b2 = t.chop();
    return ((@string)new byte[]{b1, b2}.slice()) + ((@string)(t.Value));
}

internal static @string classify(any v) {
    if (v == ((intRef)default!)) {
        return "nilref"u8;
    }
    return "other"u8;
}

internal static void Main() {
    var pb = makeBase();
    var pv = Ꮡ((view)(~pb));
    var bb = ((@base)(pv.Value));
    fmt.Println(bb.a, bb.b);
    any boxed = ((intRef)default!);
    fmt.Println(classify(boxed));
    ref var n = ref heap<int64>(out var Ꮡn);
    n = (int64)5;
    intRef r = Ꮡn;
    fmt.Println(r.Value, classify(r));
    fmt.Println(consume("abcd"u8));
}

} // end main_package
