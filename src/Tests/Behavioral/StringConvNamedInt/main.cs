namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:rune")] partial struct Delim;

[GoType("num:nint")] partial struct Code;

public static @string String(this Delim d) {
    return ((@string)(rune)d);
}

internal static void Main() {
    var d = ((Delim)(rune)'{');
    fmt.Println(d.String());
    Delim d2 = 65;
    fmt.Println(((@string)(rune)d2));
    Code c = 0x4E2D;
    fmt.Println(((@string)(nint)c));
    fmt.Println(len(((@string)(nint)c)));
}

} // end main_package
