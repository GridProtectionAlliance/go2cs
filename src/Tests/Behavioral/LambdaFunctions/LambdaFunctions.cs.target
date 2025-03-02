namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate @string Stringy();

internal static @string foo() {
    return "Stringy function"u8;
}

internal static void takesAFunction(Stringy foo) {
    fmt.Printf("takesAFunction \u0049: %v\n"u8, foo());
}

internal static Stringy returnsAFunction() {
    return () => {
        fmt.Printf("Inner stringy function\n"u8);
        return "bar"u8;
    };
}

internal static void Main() {
    takesAFunction(foo);
    Stringy f = returnsAFunction();
    f();
    Stringy baz = () => "anonymous stringy\n"u8;
    fmt.Print(baz());
}

} // end main_package
