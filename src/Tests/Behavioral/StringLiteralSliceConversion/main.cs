namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var bs = slice<byte>((@string)"hello");
    var rs = slice<rune>((@string)"héllo");
    fmt.Println(len(bs), ((@string)bs));
    fmt.Println(len(rs), ((@string)rs));
    fmt.Println(slice<byte>((@string)"hi"));
    fmt.Println(slice<rune>((@string)"aΩ"));
    @string s = "hello"u8;
    fmt.Println(((sstring)slice<byte>(s)) == ((sstring)bs));
}

} // end main_package
