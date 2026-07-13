namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var bs = slice<byte>("hello"u8);
    var rs = slice<rune>((@string)"héllo");
    fmt.Println(len(bs), ((@string)bs));
    fmt.Println(len(rs), ((@string)rs));
    fmt.Println(slice<byte>("hi"u8));
    fmt.Println(slice<rune>((@string)"aΩ"));
    fmt.Println(slice<byte>(@"ab"u8));
    fmt.Println(slice<byte>(((@string)(new byte[]{0xff, 0xfe}))));
    @string s = "hello"u8;
    fmt.Println(((sstring)slice<byte>(s)) == ((sstring)bs));
}

} // end main_package
