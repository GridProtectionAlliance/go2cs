namespace go;

using fmt = fmt_package;

partial class main_package {

internal static byte lower(byte c) {
    if ((rune)'A' <= c && c <= (rune)'Z') {
        return (byte)(c + ((rune)'a' - (rune)'A'));
    }
    return c;
}

internal static byte wrapRet(byte x) {
    return (byte)(x + x + 1);
}

internal static void Main() {
    fmt.Println(lower((rune)'A'), lower((rune)'Z'), lower((rune)'a'));
    fmt.Println(wrapRet(200));
}

} // end main_package
