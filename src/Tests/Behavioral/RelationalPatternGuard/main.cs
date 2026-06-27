namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string classify(nint x, nint y) {
    switch (ᐧ) {
    case {} when x is < 0: {
        return "neg"u8;
    }
    case {} when x == y: {
        return "equal"u8;
    }
    case {} when x > y: {
        return "greater"u8;
    }}

    return "less"u8;
}

internal static void Main() {
    fmt.Println(classify(-1, 5));
    fmt.Println(classify(5, 5));
    fmt.Println(classify(7, 3));
    fmt.Println(classify(2, 8));
}

} // end main_package
