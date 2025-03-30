namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() => func((defer, _) => {
    deferǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), "Deferred result:", add(3, 4), defer);
    fmt.Println("Doing something else");
});

internal static nint add(nint x, nint y) {
    nint result = x + y;
    fmt.Println("Calculate:", result);
    return result;
}

} // end main_package
