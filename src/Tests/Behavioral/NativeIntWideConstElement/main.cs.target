namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt maxInt = /* 1<<63 - 1 */ 9223372036854775807;

[GoType] partial struct splitTest {
    internal @string name;
    internal nint n;
}

internal static slice<splitTest> tests = new splitTest[]{
    new("quarter"u8, (nint)(2305843009213693951L)),
    new("eighth"u8, (nint)(1152921504606846976L))
}.slice();

internal static nint half(nint n) {
    return n / 2;
}

internal static void Main() {
    foreach (var (_, tt) in tests) {
        fmt.Println(tt.name, tt.n);
    }
    fmt.Println(half((nint)(4611686018427387903L)));
    fmt.Println(half((nint)(2305843009213693948L)));
    nint local = (nint)(576460752303423487L);
    fmt.Println(local);
}

} // end main_package
