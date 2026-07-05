namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

[GoType] partial struct box {
    internal nint n;
}

internal static nint helper(ж<box> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    return b.n;
}

internal static nint consume(ж<box> Ꮡio, Δio.Writer w) {
    ref var ioΔ1 = ref Ꮡio.Value;

    _ = w;
    return ioΔ1.n + helper(Ꮡio);
}

internal static nint combine(Δio.Writer ioΔ1, ж<box> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    _ = ioΔ1;
    return p.n * 2;
}

internal static void Main() {
    ref var b = ref heap<box>(out var Ꮡb);
    b = new box(n: 7);
    fmt.Println(consume(Ꮡb, default!));
    fmt.Println(combine(default!, Ꮡb));
}

} // end main_package
