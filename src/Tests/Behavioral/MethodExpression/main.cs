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
    var cv = c.Value;
    fmt.Println(cv.get());
    var g = (Func<counter, nint>)(get);
    fmt.Println(g(c.Value));
    var apply = (Func<ж<counter>, nint, nint> fn, ж<counter> @base, nint dΔ1) => fn(@base, dΔ1);
    fmt.Println(apply((Func<ж<counter>, nint, nint>)(add), c, 3));
    fmt.Println(g(c.Value));
    Func<nint, nint> bump = c.add;
    fmt.Println(bump(4));
    fmt.Println(bump(1));
    fmt.Println(g(c.Value));
    dispatcher d = default!;
    d.compute = (nint p1) => new reader(readTen).sum(p1);
    fmt.Println(d.compute(3));
    var sh = new shifter(delta: 2);
    fmt.Println(((@string)mapRunes((rune p1) => sh.shift(p1), slice<rune>((@string)"AB"))));
}

[GoType] partial struct shifter {
    internal rune delta;
}

internal static rune shift(this shifter s, rune r) {
    return r + s.delta;
}

internal static slice<rune> mapRunes(Func<rune, rune> f, slice<rune> rs) {
    var @out = new slice<rune>(len(rs));
    foreach (var (i, r) in rs) {
        @out[i] = f(r);
    }
    return @out;
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
