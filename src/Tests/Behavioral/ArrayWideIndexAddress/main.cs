namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ref var a = ref heap<array<nint>>(out var Ꮡa);
    a = new nint[]{10, 20, 30, 40}.array();
    uintptr i = 2;
    var pi = Ꮡa.at<nint>((nint)(i));
    pi.val = 99;
    nuint u = 1;
    var pu = Ꮡa.at<nint>((nint)(u));
    pu.val = 88;
    uint64 w = 3;
    var pw = Ꮡa.at<nint>((nint)(w));
    pw.val = 77;
    nuint g = 5;
    var pe = Ꮡa.at<nint>((nint)(g % 2));
    pe.val = 66;
    fmt.Println(a[0], a[1], a[2], a[3]);
    uintptr pc = 171;
    fmt.Println("0123456789abcdef"u8[(int)((uintptr)(pc & 15))]);
    fmt.Println("0123456789abcdef"u8[(int)((uintptr)((pc >> (int)(4)) & 15))]);
    @string hex = "0123456789abcdef"u8;
    uint64 k = 12;
    fmt.Println(hex[(int)(k)]);
}

} // end main_package
