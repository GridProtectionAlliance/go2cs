namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    foreach (var (_, v) in new nint[]{5, 99, 999, 1234, -5, -999}.slice()) {
        fmt.Printf("k =% 4d;\n"u8, v);
    }
    fmt.Printf("[% d] [% d] [%+d] [%+d]\n"u8, 7, -7, 7, -7);
    fmt.Printf("[%4d] [%-4d] [%04d] [%04d] [%+4d]\n"u8, 42, 42, 42, -42, 7);
    fmt.Printf("[% 04d] [%+04d]\n"u8, 5, 5);
    fmt.Printf("[%6s] [%-6s] [%6v]\n"u8, "ab", "ab", 12);
    fmt.Printf("[%.1f] [%0.1f] [%6.2f] [%-7.2f] [%.0f]\n"u8, 45.678D, 45.678D, 3.14159D, 3.14159D, 2.71D);
    fmt.Printf("[%2d] [%2s]\n"u8, 12345, "hello");
    fmt.Printf("[% d] [% 6s]\n"u8, -3, "ab");
}

} // end main_package
