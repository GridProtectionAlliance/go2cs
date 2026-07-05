namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string encComplex(nint i, complex128 c) {
    _ = i;
    if (c != 0 + builtin.i(0F)) {
        return "nonzero"u8;
    }
    return "zero"u8;
}

internal static void Main() {
    fmt.Println(encComplex(1, complex(0, 0)));
    fmt.Println(encComplex(2, complex(3, -4)));
    fmt.Println(encComplex(3, builtin.i(5F)));
}

} // end main_package
