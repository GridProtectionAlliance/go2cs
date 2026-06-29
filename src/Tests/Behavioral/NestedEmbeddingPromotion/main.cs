namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct inner {
    internal nint n;
    internal nint p;
}

[GoType] partial struct mid {
    internal partial ref inner inner { get; }
    internal nint m;
}

[GoType] partial struct top {
    internal partial ref mid mid { get; }
    internal nint t;
}

internal static void Main() {
    var x = new top(nil);
    x.n = 1;
    x.p = 9;
    x.m = 2;
    x.t = 3;
    fmt.Println(x.n, x.p, x.m, x.t);
    fmt.Println(x.inner.n, x.mid.m);
    x.n = 42;
    fmt.Println(x.n, x.inner.n);
}

} // end main_package
