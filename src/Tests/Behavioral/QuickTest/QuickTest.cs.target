global using P = go.ж<bool>;
global using M = go.map<nint, nint>;

namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct test_R0 {
    public @string @string;
    public ж<nint> @int;
    public P P;
    public M M;
}

[GoType] partial struct test_x {
    public @string @string;
    public ж<nint> @int;
    public P P;
    public M M;
}

internal static test_R0 test() {
    test_x x = default!;
    x.@string = "Go"u8;
    x.@int = @new<nint>();
    x.P = @new<bool>();
    x.M = new M();
    return x;
}

internal static void Main() {
    var x = test();
    fmt.Println(x);
}

} // end main_package
