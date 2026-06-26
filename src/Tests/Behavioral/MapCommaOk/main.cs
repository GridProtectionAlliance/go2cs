namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    var (v, ok) = m["a"u8, ꟷ];
    fmt.Println(v, ok);
    var (v2, ok2) = m["z"u8, ꟷ];
    fmt.Println(v2, ok2);
    var (_, ok3) = m["b"u8, ꟷ];
    fmt.Println(ok3);
    {
        var (_, ok4) = m["z"u8, ꟷ]; if (!ok4) {
            fmt.Println("z absent");
        }
    }
    nint w = default!;
    bool present = default!;
    (w, present) = m["b"u8, ꟷ];
    fmt.Println(w, present);
}

} // end main_package
