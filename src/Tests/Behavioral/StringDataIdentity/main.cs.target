namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    @string s = "identity probe string"u8;
    @string t = s;
    fmt.Println(@unsafe.StringData(s) == @unsafe.StringData(t));
    fmt.Println(@unsafe.StringData(s) == @unsafe.StringData(s));
    @string u = ((@string)append(slice<byte>(default!), s.ꓸꓸꓸ));
    fmt.Println(@unsafe.StringData(s) == @unsafe.StringData(u));
    fmt.Println(s == u);
}

} // end main_package
