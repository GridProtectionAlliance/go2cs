namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ref var a = ref heap(new @string(), out var Ꮡa);
    a = "Hello World"u8;
    test(a);
    fmt.Println(a);
    fmt.Println();
    a = "Hello World"u8;
    test2(Ꮡa);
    fmt.Println(a);
}

internal static void test(@string a) {
    fmt.Println(a);
    a = "Goodbye World"u8;
    fmt.Println(a);
}

internal static void test2(ж<@string> Ꮡa) {
    ref var a = ref Ꮡa.val;

    fmt.Println(a);
    a = "Goodbye World"u8;
    fmt.Println(a);
}

} // end main_package
