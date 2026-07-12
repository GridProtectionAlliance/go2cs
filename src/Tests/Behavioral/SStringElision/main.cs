namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var name = slice<byte>((@string)"go2cs");
    sstring s = ((sstring)name);
    if (s == "go2cs") {
        fmt.Println("match");
    }
    var digits = slice<byte>((@string)"2468");
    sstring d = ((sstring)digits);
    fmt.Println((nint)d[0] + (nint)d[3] + len(d));
    var scratch = slice<byte>((@string)"AB");
    @string t = ((@string)scratch);
    scratch[0] = (rune)'X';
    if (t == "AB"u8) {
        fmt.Println("copy-safe");
    }
    @string u = ((@string)slice<byte>((@string)"printed"));
    fmt.Println(u);
    fmt.Println(returnedString());
    var tag = slice<byte>((@string)"v2");
    if (((sstring)tag) == "v2") {
        fmt.Println("tagged");
    }
    @string want = "v2"u8;
    if (((@string)tag) == want) {
        fmt.Println("wanted");
    }
}

internal static @string returnedString() {
    var b = slice<byte>((@string)"returned");
    @string r = ((@string)b);
    return r;
}

} // end main_package
