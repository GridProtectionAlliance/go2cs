namespace go;

using fmt = fmt_package;

partial class main_package {

internal static any store(any v) {
    return v;
}

internal static any ret() {
    return (nint)(42);
}

[GoType] partial struct holder {
    internal any v;
}

internal static @string classify(any v) {
    switch (v.type()) {
    case nint: {
        return "int"u8;
    }
    case int32: {
        return "int32"u8;
    }
    default: {
        return "other"u8;
    }}

}

internal static void Main() {
    fmt.Println(store((nint)(42))._<nint>());
    any a = (nint)(7);
    fmt.Println(a._<nint>());
    any b = default!;
    b = (nint)(8);
    fmt.Println(b._<nint>());
    fmt.Println(ret()._<nint>());
    var s = new any[]{(nint)(5)}.slice();
    fmt.Println(s[0]._<nint>());
    var m = new map<@string, any>{["k"u8] = (nint)(9)};
    fmt.Println(m["k"u8]._<nint>());
    var h = new holder(v: (nint)(3));
    fmt.Println(h.v._<nint>());
    fmt.Println(store((nint)(-5))._<nint>());
    fmt.Println(store((nint)(1 + 2))._<nint>());
    fmt.Println(classify((nint)(1)), classify((int32)1));
    {
        var (n, ok) = store((nint)(11))._<nint>(ᐧ); if (ok) {
            fmt.Println("ok", n);
        }
    }
}

} // end main_package
