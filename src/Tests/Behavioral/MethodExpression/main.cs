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
    var apply = (Func<ж<counter>, nint, nint> fn, ж<counter> @base, nint dΔ1) => fn(@base, dΔ1);
    fmt.Println(apply((Func<ж<counter>, nint, nint>)(add), c, 3));
    fmt.Println(g(c.val));
    Func<nint, nint> bump = c.add;
    fmt.Println(bump(4));
    fmt.Println(bump(1));
    fmt.Println(g(c.val));
    dispatcher d = default!;
    d.compute = (nint p1) => new reader(readTen).sum(p1);
    fmt.Println(d.compute(3));
}

internal delegate nint reader();

internal static nint sum(this reader f, nint extra) {
    return f() + extra;
}

[GoType] partial struct dispatcher {
    internal Func<nint, nint> compute;
}

internal static nint readTen() {
    return 10;
}

} // end main_package
