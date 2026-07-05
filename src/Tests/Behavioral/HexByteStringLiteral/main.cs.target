namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly @string data = ((@string)(new byte[]{0x50, 0x4b, 0x03, 0x04, 0xdb, 0x35, 0x30, 0xff, 0x92, 0x00, 0x4c, 0x4d, 0x54}));

internal static nint get4(@string s, nint i) {
    return (nint)((nint)((nint)((nint)s[i] | ((nint)s[i + 1] << (int)(8))) | ((nint)s[i + 2] << (int)(16))) | ((nint)s[i + 3] << (int)(24)));
}

internal static void Main() {
    fmt.Println(len(data));
    for (nint i = 0; i < len(data); i++) {
        fmt.Printf("%d "u8, data[i]);
    }
    fmt.Println();
    fmt.Println(get4(data, 0));
}

} // end main_package
