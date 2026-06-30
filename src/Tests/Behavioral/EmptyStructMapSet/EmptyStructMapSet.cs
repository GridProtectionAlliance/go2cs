namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void add(map<nint, EmptyStruct> seen, nint k) {
    seen[k] = new EmptyStruct();
}

internal static bool contains(map<nint, EmptyStruct> seen, nint k) {
    var (_, ok) = seen[k, ꟷ];
    return ok;
}

internal static void Main() {
    var seen = new map<nint, EmptyStruct>();
    add(seen, 3);
    add(seen, 7);
    add(seen, 3);
    fmt.Println("len:", len(seen));
    foreach (var (_, k) in new nint[]{1, 3, 5, 7}.slice()) {
        fmt.Printf("contains(%d) = %t\n"u8, k, contains(seen, k));
    }
    var lit = new map<@string, EmptyStruct>{
        ["a"u8] = new(),
        ["b"u8] = new()
    };
    lit["c"u8] = new EmptyStruct();
    fmt.Println("lit len:", len(lit));
    foreach (var (_, s) in new @string[]{"a", "b", "c", "d"}.slice()) {
        var (_, ok) = lit[s, ꟷ];
        fmt.Printf("lit[%s] = %t\n"u8, s, ok);
    }
}

} // end main_package
