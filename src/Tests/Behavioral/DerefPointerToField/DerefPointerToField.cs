namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct holder {
    internal ж<slice<nint>> xs;
    internal ж<nint> cnt;
}

internal static nint sumRange(ж<holder> Ꮡh) {
    ref var h = ref Ꮡh.Value;

    nint s = 0;
    foreach (var (_, x) in h.xs.ValueSlot) {
        s += x;
    }
    return s;
}

internal static nint readVal(ж<holder> Ꮡh) {
    ref var h = ref Ꮡh.Value;

    return h.cnt.Value;
}

internal static void Main() {
    var xs = new nint[]{10, 20, 30}.slice();
    ref var c = ref heap<nint>(out var Ꮡc);
    c = 7;
    var h = Ꮡ(new holder(xs: Ꮡ(xs), cnt: Ꮡc));
    fmt.Println(sumRange(h));
    fmt.Println(readVal(h));
}

} // end main_package
