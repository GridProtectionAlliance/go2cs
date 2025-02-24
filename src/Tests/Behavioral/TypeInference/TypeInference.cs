namespace go;

using fmt = fmt_package;

partial class main_package {

private static void Main() {
    const bool c = /* 3 < 4 */ true;
    [GoType("bool")]     partial struct MyBool {}

    nint x = default!;
    nint y = default!;
    bool b3 = x == y;
    bool b4 = x == y;
    MyBool b5 = x == y;
    fmt.Println(c);
    fmt.Println(b3);
    fmt.Println(b4);
    fmt.Println(b5);
}

} // end main_package
