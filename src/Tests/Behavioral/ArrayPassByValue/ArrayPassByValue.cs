namespace go;

using fmt = fmt_package;

public static partial class main_package {

private static void Main() {
    ref var a = ref heap(new array<@string>(2), out var Ꮡa);

    a[0] = "Hello"u8;
    a[1] = "World"u8;
    var p = Ꮡa.at<@string>(0);
    test(a);
    fmt.Println(a[0], a[1]);
    fmt.Println();
    a[0] = "Hello"u8;
    test2(Ꮡa);
    fmt.Println(a[0], a[1]);
    fmt.Println();
    a[0] = "Hello"u8;
    test3(a[..]);
    fmt.Println(a[0], a[1]);
    fmt.Println();
    var primes = new nint[] {2, 3, 5, 7, 11, 13 };
    fmt.Println(primes);
    fmt.Println(a[0]);
    stest(p);
    fmt.Println(a[0]);
}

private static void stest(ptr<@string> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = "hello"u8;
}

// Arrays are passed by value (a full copy)
private static void test(array<@string> a) {
    a = a.Clone();

    // Update to array will be local
    fmt.Println(a[0], a[1]);
    a[0] = "Goodbye"u8;
    fmt.Println(a[0], a[1]);
}

private static void test2(ptr<array<@string>> Ꮡa) {
    ref var a = ref Ꮡa.val;

    fmt.Println(a[0], a[1]);
    a[0] = "Goodbye"u8;
    fmt.Println(a[0], a[1]);
}

private static void test3(slice<@string> a) {
    fmt.Println(a[0], a[1]);
    a[0] = "Goodbye"u8;
    fmt.Println(a[0], a[1]);
}

} // end main_package
