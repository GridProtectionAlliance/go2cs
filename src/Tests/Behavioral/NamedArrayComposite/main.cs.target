namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[3]rune")] partial struct triple;

[GoType("[]nint")] partial struct nums;

internal static void Main() {
    var t = new triple(new rune[]{7, 32, 9}.array());
    fmt.Println(t[0], t[1], t[2]);
    var u = new triple(new rune[]{0, 305 - 73, 0}.array());
    fmt.Println(u[1]);
    var n = new nums(new nint[]{1, 2, 3, 4}.slice());
    fmt.Println(len(n), n[3]);
    fmt.Println(first(new triple(new rune[]{11, 22, 33}.array())));
}

internal static rune first(triple t) {
    return t[0];
}

} // end main_package
