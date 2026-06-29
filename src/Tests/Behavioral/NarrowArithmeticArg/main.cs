namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint8 takeU8(uint8 x) {
    return x;
}

internal static int8 takeI8(int8 x) {
    return x;
}

internal static uint16 takeU16(uint16 x) {
    return x;
}

internal static void Main() {
    uint8 a = 200;
    uint8 b = 100;
    fmt.Println(takeU8((uint8)(a + b)));
    fmt.Println(takeU8((uint8)(~a)));
    int8 c = 100;
    int8 d = 100;
    fmt.Println(takeI8((int8)(c + d)));
    uint16 e = 60000;
    uint16 f = 10000;
    fmt.Println(takeU16((uint16)(e + f)));
    fmt.Println(takeU8((uint8)(a * 2 + b)));
}

} // end main_package
