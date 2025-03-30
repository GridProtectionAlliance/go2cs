namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() => func((defer, _) => {
    nint count = 0;
    defer(() => {
        fmt.Println("Deferred count (closure):", count);
    });
    count = 10;
    fmt.Println("Count before defer:", count);
});

} // end main_package
