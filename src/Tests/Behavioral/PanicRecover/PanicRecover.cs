namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    f();
    fmt.Println("Returned normally from f.");
}

internal static void f() => func((defer, recover) => {
    defer(() => {
        {
            var r = recover(); if (r != default!) {
                fmt.Println("Recovered in f", r);
            }
        }
    });
    fmt.Println("Calling g.");
    g(0);
    fmt.Println("Returned normally from g.");
});

internal static void g(nint i) => func((defer, _) => {
    if (i > 3) {
        fmt.Println("Panicking!");
        throw panic(fmt.Sprintf("%v"u8, i));
    }
    deferǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), "Defer in g", i, defer);
    fmt.Println("Printing in g", i);
    g(i + 1);
});

} // end main_package
