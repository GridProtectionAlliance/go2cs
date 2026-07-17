namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    @string s = "a"u8 + ((sstring)new byte[]{0x80, 0x80}.slice()) + "☺"u8;
    foreach (var (i, rΔ1) in s) {
        fmt.Println(i, (int32)rΔ1);
    }
    @string t = "abc"u8;
    var b = slice<byte>(t);
    b[0] = (rune)'X';
    fmt.Println(t, ((@string)b));
    var r = slice<rune>(((@string)(new byte[]{0x61, 0x80, 0x80, 0x62})));
    fmt.Println(len(r), (int32)r[1], (int32)r[2], (int32)r[3]);
}

} // end main_package
