namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt marker = /* 0xFFFD */ 65533;

[GoType("[]uint16")] partial struct words;

internal static void Main() {
    slice<uint16> a = default!;
    a = append(a, (uint16)(marker));
    a = append(a, (uint16)(7), (uint16)(8));
    uint16 v = 9;
    a = append(a, v);
    fmt.Println(len(a), a[0], a[1], a[2], a[3]);
    words w = default!;
    w = append(w, (uint16)(marker), (uint16)(1));
    fmt.Println(w[0], w[1]);
}

} // end main_package
