namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Stringish {
    @string Str();
}

[GoType] partial interface Marshaler {
    @string Marshal();
}

[GoType] partial struct widget {
    internal nint n;
}

[GoRecv] internal static @string Str(this ref widget w) {
    return fmt.Sprintf("widget(%d)"u8, w.n);
}

[GoRecv] internal static @string Marshal(this ref widget w) {
    return fmt.Sprintf("<%d>"u8, w.n);
}

[GoType] partial struct other {
}

[GoRecv] internal static @string Str(this ref other o) {
    return "other"u8;
}

internal static Stringish newStringish(nint n) {
    return new widgetжStringish(Ꮡ(new widget(n)));
}

internal static void Main() {
    var s = newStringish(7);
    fmt.Println(s.Str());
    var m = s._<Marshaler>();
    fmt.Println(m.Marshal());
    {
        var (m2, ok) = s._<Marshaler>(ᐧ); if (ok) {
            fmt.Println("ok", m2.Marshal());
        }
    }
    Stringish s2 = new otherжStringish(Ꮡ(new other(nil)));
    {
        var (_, ok) = s2._<Marshaler>(ᐧ); if (ok){
            fmt.Println("unexpected: other is Marshaler");
        } else {
            fmt.Println("other is not Marshaler");
        }
    }
}

} // end main_package
