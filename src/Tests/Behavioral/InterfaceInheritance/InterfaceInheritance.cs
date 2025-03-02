namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct T1 {
    public @string name;
}

public static void M(this T1 t) {
}

public static void N(this T1 t) {
}

public static @string String(this T1 t) {
    return ""u8;
}

public static @string Error(this T1 t) {
    return ""u8;
}

[GoType] partial struct T2 {
    public @string name;
}

public static void M(this T2 t) {
}

public static void N(this T2 t) {
}

public static @string String(this T2 t) {
    return ""u8;
}

public static @string Error(this T2 t) {
    return ""u8;
}

[GoType] partial interface I {
    void M();
}

[GoType] partial interface V :
    I,
    fmt.Stringer,
    error
{
    void N();
}

internal static void Main() {
    var m = new map<I, nint>();
    I i1 = new T1("foo");
    I i2 = new T2("bar");
    m[i1] = 1;
    m[i2] = 2;
    fmt.Println(m);
    var n = new map<V, nint>();
    V v1 = new T1("foo");
    V v2 = new T2("bar");
    v1.N();
    v2.M();
    fmt.Print(v1.String());
    fmt.Print(v2.Error());
    n[v1] = 3;
    n[v2] = 4;
    fmt.Println(n);
}

} // end main_package
