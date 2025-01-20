namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate @string Stringy();

private static @string foo() {
    return "Stringy function"u8;
}

private static void takesAFunction(Stringy foo) {
    fmt.Printf("takesAFunction \u0049: %v\n"u8, foo());
}

private static Stringy returnsAFunction() {
    return () => {
        fmt.Printf("Inner stringy function\n"u8);
        return "bar"u8;
    };
}

private static void Main() {
    takesAFunction(foo);
    Stringy f = returnsAFunction();
    f();
    Stringy baz = () => "anonymous stringy\n"u8;
    fmt.Printf(baz());
}

} // end main_package
