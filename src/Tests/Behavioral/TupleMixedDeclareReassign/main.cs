namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (ж<int64>, int64) next(int64 now) {
    ref var v = ref heap<int64>(out var Ꮡv);
    v = now * 2;
    return (Ꮡv, now + 1);
}

internal static (ж<int64>, int64) step(int64 now) {
    (var pp, now) = next(now);
    if (pp == nil) {
        return (default!, now);
    }
    return (pp, now);
}

internal static (ж<nint>, ж<nint>) pair() {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 7;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 9;
    return (Ꮡa, Ꮡb);
}

internal static (ж<nint>, ж<nint>) shadow() {
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 100;
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 200;
    var (px, py) = (Ꮡx, Ꮡy);
    if (px.Value > 0) {
        var (pxΔ1, pyΔ1) = pair();
        return (pxΔ1, pyΔ1);
    }
    return (px, py);
}

internal static void Main() {
    var (a, b) = step(5);
    fmt.Println(a.Value, b);
    var (c, d) = shadow();
    fmt.Println(c.Value, d.Value);
}

} // end main_package
