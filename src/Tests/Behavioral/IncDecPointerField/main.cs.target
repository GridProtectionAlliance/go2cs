namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct inner {
    internal nint k;
}

[GoType] partial struct counter {
    internal nint n;
    internal inner sub;
}

internal static ж<counter> get(ж<counter> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return Ꮡc;
}

internal static void Main() {
    var @base = Ꮡ(new counter(n: 5));
    @base.val.sub.k = 3;
    var c = get(@base);
    c.val.n++;
    c.val.n++;
    c.val.sub.k--;
    fmt.Println((~@base).n, (~@base).sub.k);
}

} // end main_package
