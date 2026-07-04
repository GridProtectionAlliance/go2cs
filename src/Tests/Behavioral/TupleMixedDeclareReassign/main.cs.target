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

[GoType] partial struct cursor {
    internal nint pos;
    internal @string tag;
}

internal static (cursor, error) makeCursor(nint pos, @string tag) {
    return (new cursor(pos: pos, tag: tag), default!);
}

internal static void bump(ж<cursor> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    c.pos++;
}

internal static (nint, nint) streams() {
    ref var rbr1 = ref heap<cursor>(out var Ꮡrbr1);
    (rbr1, var err) = makeCursor(10, "one"u8);
    if (err != default!) {
        return (0, 0);
    }
    ref var rbr2 = ref heap<cursor>(out var Ꮡrbr2);
    (rbr2, err) = makeCursor(20, "two"u8);
    if (err != default!) {
        return (0, 0);
    }
    bump(Ꮡrbr1);
    bump(Ꮡrbr2);
    bump(Ꮡrbr2);
    return (rbr1.pos, rbr2.pos);
}

internal static void Main() {
    var (a, b) = step(5);
    fmt.Println(a.Value, b);
    var (c, d) = shadow();
    fmt.Println(c.Value, d.Value);
    var (e, f) = streams();
    fmt.Println(e, f);
}

} // end main_package
