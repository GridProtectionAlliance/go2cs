namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface point<T> {
    @string label();
    T combine(T _);
    (T, error) restore(slice<byte> _);
}

[GoType] partial struct p224 {
    internal nint v;
}

[GoRecv] internal static @string label(this ref p224 p) {
    return "p224"u8;
}

[GoRecv] internal static ж<p224> combine(this ref p224 p, ж<p224> Ꮡo) {
    ref var o = ref Ꮡo.Value;

    return Ꮡ(new p224(v: p.v + o.v));
}

[GoRecv] internal static (ж<p224>, error) restore(this ref p224 p, slice<byte> b) {
    return (Ꮡ(new p224(v: (nint)b[0])), default!);
}

internal static ж<p224> newP224() {
    return Ꮡ(new p224(v: 1));
}

[GoType] partial struct p384 {
    internal nint v;
}

[GoRecv] internal static @string label(this ref p384 p) {
    return "p384"u8;
}

[GoRecv] internal static ж<p384> combine(this ref p384 p, ж<p384> Ꮡo) {
    ref var o = ref Ꮡo.Value;

    return Ꮡ(new p384(v: p.v * o.v));
}

[GoRecv] internal static (ж<p384>, error) restore(this ref p384 p, slice<byte> b) {
    return (Ꮡ(new p384(v: (nint)b[0])), default!);
}

internal static ж<p384> newP384() {
    return Ꮡ(new p384(v: 2));
}

[GoType] partial struct curve<Point>
    where Point : point<Point>
{
    internal @string name;
    internal Func<Point> newPoint;
    internal Point @base;
}

[GoType] partial interface Curve {
    @string Name();
    @string BaseLabel();
    @string Combined();
    @string Fresh();
}

[GoRecv] internal static @string Name<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.name;
}

[GoRecv] internal static @string BaseLabel<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.@base.label();
}

[GoRecv] internal static @string Combined<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.@base.combine(c.@base).label();
}

[GoRecv] internal static @string Fresh<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    var p = c.newPoint();
    var (r, _) = p.restore(new byte[]{7}.slice());
    return r.label();
}

internal static void Main() {
    Curve c1 = new curveжCurve<p224жpoint>(Ꮡ(new curve<p224жpoint>(name: "c224"u8, newPoint: () => newP224(), @base: Ꮡ(new p224(v: 3)))));
    Curve c2 = new curveжCurve<p384жpoint>(Ꮡ(new curve<p384жpoint>(name: "c384"u8, newPoint: () => newP384(), @base: Ꮡ(new p384(v: 5)))));
    fmt.Println(c1.Name(), c1.BaseLabel(), c1.Combined(), c1.Fresh());
    fmt.Println(c2.Name(), c2.BaseLabel(), c2.Combined(), c2.Fresh());
    foreach (var (_, c) in new Curve[]{c1, c2}.slice()) {
        fmt.Println(c.Name(), "->", c.BaseLabel(), "->", c.Combined(), "->", c.Fresh());
    }
}

} // end main_package
