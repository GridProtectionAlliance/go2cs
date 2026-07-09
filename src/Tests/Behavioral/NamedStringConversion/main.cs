namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("@string")] partial struct errorString;

internal static @string Error(this errorString e) {
    return "err: "u8 + ((@string)e);
}

[GoType("@string")] partial struct label;

internal static void Main() {
    error e = ((errorString)(@string)"kaboom"u8);
    fmt.Println(e.Error());
    label l = ((label)(@string)"tag"u8);
    fmt.Println(l, len(l));
    label st = ((label)(@string)"json,omitempty"u8);
    fmt.Println(st[0], st[4]);
    label name = st[0..4];
    fmt.Println(name, name != ""u8, name == "json"u8);
}

} // end main_package
