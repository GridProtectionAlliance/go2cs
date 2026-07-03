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
    fmt.Println(cached(), cached());
    loader = (@string name) => (slice<byte>(name), default!);
    var (b, err) = loader("zone"u8);
    fmt.Println(len(b), err == default!);
}

internal static Func<@string, (slice<byte>, error)> loader;

internal static Func<nint> cached = memo(() => {
    nint n = default!;
    for (nint i = 1; i <= 4; i++) {
        n += i;
    }
    return n;
});

internal static Func<nint> memo(Func<nint> f) {
    var done = false;
    nint v = 0;
    return () => {
        if (!done) {
            v = f();
            done = true;
        }
        return v;
    };
}

} // end main_package
