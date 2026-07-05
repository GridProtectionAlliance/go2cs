namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string setFlag(bool @true) {
    if (@true) {
        return "on"u8;
    }
    return "off"u8;
}

internal static void Main() {
    fmt.Println(setFlag(true), setFlag(false));
    nint @false = 7;
    fmt.Println(@false + 1);
}

} // end main_package
