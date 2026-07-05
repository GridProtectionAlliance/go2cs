namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("map[@string, nint]")] partial struct A;

[GoType("map[@string, nint]")] partial struct B;

[GoType("[]nint")] partial struct S1;

[GoType("[]nint")] partial struct S2;

internal static void Main() {
    var a = new A(new map<@string, nint>{["x"u8] = 1, ["y"u8] = 2});
    var b = ((B)(map<@string, nint>)a);
    fmt.Println(b["x"u8], b["y"u8], len(b));
    var s = new S1(new nint[]{10, 20, 30}.slice());
    var s2 = ((S2)(slice<nint>)(s));
    fmt.Println(s2[0], s2[2], len(s2));
}

} // end main_package
