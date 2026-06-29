namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Δsliceᴛ {
    internal nint lo, hi;
}

internal static nint span(this Δsliceᴛ s) {
    return s.hi - s.lo;
}

[GoType] partial struct builder {
    internal nint @base;
}

[GoRecv] internal static Δsliceᴛ Δslice(this ref builder b, nint n) {
    return new Δsliceᴛ(lo: b.@base, hi: b.@base + n);
}

internal static void Main() {
    var b = Ꮡ(new builder(@base: 10));
    var s = b.Δslice(5);
    fmt.Println(s.lo, s.hi);
    fmt.Println(s.span());
    fmt.Println(s);
}

} // end main_package
