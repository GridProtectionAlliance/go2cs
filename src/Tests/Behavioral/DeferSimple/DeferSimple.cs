namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() => func((defer, _) => {
    fmt.Println("Open file");
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "Close file", defer);
    fmt.Println("Write data to file");
});

} // end main_package
