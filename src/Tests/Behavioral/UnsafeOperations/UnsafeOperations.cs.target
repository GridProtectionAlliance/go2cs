namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    var b = new byte[]{}.slice();
    for (nint ch = 32; ch < 80; ch++) {
        b = append(b, ((@string)((rune)ch)).ꓸꓸꓸ);
    }
    @string str = @unsafe.String(Ꮡ(b, 0), len(b));
    fmt.Println(str);
    var ptr = @unsafe.StringData(str);
    fmt.Println("ptr =", ptr);
    fmt.Println(@unsafe.String(ptr, len(str)));
}

} // end main_package
