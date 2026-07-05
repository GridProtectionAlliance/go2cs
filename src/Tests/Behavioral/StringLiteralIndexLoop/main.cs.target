namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint sumTable(nint n) {
    nint total = 0;
    for (nint i = 0; i < n; i++) {
        total += (nint)"\x01\x02\x03\x04"u8[(int)(i)];
    }
    return total;
}

internal static void Main() {
    fmt.Println(sumTable(4));
    fmt.Println("abcd"u8[2]);
}

} // end main_package
