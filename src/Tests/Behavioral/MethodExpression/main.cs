namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static nint add(this ref counter c, nint d) {
    c.n += d;
    return c.n;
}

internal static nint get(this counter c) {
    return c.n;
}

internal static void Main() {
    var f = (Func<ж<counter>, nint, nint>)(add);
    var c = Ꮡ(new counter(n: 10));
    fmt.Println(f(c, 5));
    fmt.Println(f(c, 2));
    var cv = c.val;
    fmt.Println(cv.get());
    var g = (Func<counter, nint>)(get);
    fmt.Println(g(c.val));
    var apply = (Func<ж<counter>, nint, nint> fn, ж<counter> @base, nint d) => fn(@base, d);
    fmt.Println(apply((Func<ж<counter>, nint, nint>)(add), c, 3));
    fmt.Println(g(c.val));
}

} // end main_package
