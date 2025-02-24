namespace go;

using fmt = fmt_package;

partial class main_package {

private static void Main() {
    map<any, @string> tbl = default!;
    tbl = new map<any, @string>();
    // Ensure object-based map key parameters as strings are not u8
    tbl[@string("12"u8)] = "1"u8;
    tbl[12] = "2"u8;
    tbl[12.0F] = "3"u8;
    fmt.Println(tbl["12"u8]);
    fmt.Println(tbl[12]);
    fmt.Println(tbl[12.0F]);
}

} // end main_package
