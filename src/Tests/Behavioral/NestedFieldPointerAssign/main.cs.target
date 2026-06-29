namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct inner {
    internal uintptr hi, lo;
}

[GoType] partial struct outer {
    internal inner stack;
    internal uintptr guard;
}

internal static ж<outer> get(ж<outer> Ꮡo) {
    ref var o = ref Ꮡo.val;

    return Ꮡo;
}

internal static void Main() {
    ref var @base = ref heap(new outer(), out var Ꮡbase);
    var o = get(Ꮡbase);
    o.val.stack.hi = 100;
    o.val.stack.lo = 50;
    o.val.guard = 5;
    fmt.Println(@base.stack.hi, @base.stack.lo, @base.guard);
}

} // end main_package
