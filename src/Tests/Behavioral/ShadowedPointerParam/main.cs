namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct T {
    internal nint n;
}

internal static nint use(ж<T> Ꮡt) {
    ref var t = ref Ꮡt.val;

    return t.n;
}

internal static nint outer(ж<T> Ꮡt) {
    ref var t = ref Ꮡt.val;

    nint r = use(Ꮡt);
    switch (ᐧ) {
    default: {
        ж<T> tΔ2 = Ꮡ(new T(n: 5));
        r += use(tΔ2);
        r += (tΔ2.val).n;
        break;
    }}

    return r;
}

internal static void Main() {
    fmt.Println(outer(Ꮡ(new T(n: 1))));
}

} // end main_package
