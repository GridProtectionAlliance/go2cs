namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("@string")] partial struct errorString;

internal static @string Error(this errorString e) {
    return "err: "u8 + ((@string)e);
}

[GoType("@string")] partial struct label;

internal static void Main() {
    errorString e = ((errorString)(@string)"kaboom"u8);
    fmt.Println(e.Error());
    @string l = ((label)(@string)"tag"u8);
    fmt.Println(l, len(l));
}

} // end main_package
