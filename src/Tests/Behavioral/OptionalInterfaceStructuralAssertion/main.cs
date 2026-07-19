namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Describer {
    @string Describe();
}

[GoType] partial interface Tagger {
    @string Describe();
    @string Tag();
}

[GoType] partial struct widget {
    internal @string name;
}

internal static @string Describe(this widget w) {
    return "describe:"u8 + w.name;
}

internal static @string Tag(this widget w) {
    return "tag:"u8 + w.name;
}

[GoType] partial struct plain {
}

internal static Describer newDescriber(@string name) {
    return new widget(name: name);
}

internal static void Main() {
    var d = newDescriber("alpha"u8);
    fmt.Println(d.Describe());
    var items = new any[]{new widget(name: "beta"u8), new plain(nil)}.slice();
    foreach (var (_, it) in items) {
        {
            var (t, ok) = it._<Tagger>(ᐧ); if (ok){
                fmt.Println(t.Describe(), t.Tag());
            } else {
                fmt.Println("not a Tagger");
            }
        }
    }
}

} // end main_package
