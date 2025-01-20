namespace go;

using fmt = fmt_package;

partial class main_package {

private static void Main() {
    @string a = default!;
    a = "Hello World"u8;
    test(a);
    fmt.Println(a);
    fmt.Println();
    a = "Hello World"u8;
    test2(Ꮡa);
    fmt.Println(a);
}

private static void test(@string a) {
    fmt.Println(a);
    a = "Goodbye World"u8;
    fmt.Println(a);
}

private static void test2(ptr<@string> Ꮡa) {
    ref var a = ref Ꮡa.val;

    fmt.Println(a);
    a = "Goodbye World"u8;
    fmt.Println(a);
}

} // end main_package
