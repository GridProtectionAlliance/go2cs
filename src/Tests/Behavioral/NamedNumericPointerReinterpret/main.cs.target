namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint64 load64(ж<uint64> Ꮡp) {
    ref var p = ref Ꮡp.val;

    return p;
}

internal static uint32 load32(ж<uint32> Ꮡp) {
    ref var p = ref Ꮡp.val;

    return p;
}

[GoType("num:uint64")] partial struct lfstack;

[GoType("num:uint32")] partial struct sweepClass;

[GoRecv] internal static uint64 peek(this ref lfstack head) {
    return load64(Ꮡ((uint64)(head)));
}

[GoRecv] internal static uint32 peek(this ref sweepClass sc) {
    return load32(Ꮡ((uint32)(sc)));
}

internal static uint64 peekVia(ж<lfstack> Ꮡp) {
    ref var p = ref Ꮡp.val;

    return load64(Ꮡ((uint64)(p)));
}

[GoType("num:uint64")] partial struct hexval;

internal static uint64 describe(hexval v) {
    return (uint64)v;
}

internal static void Main() {
    ref var a = ref heap(new lfstack(), out var Ꮡa);
    a = 956397711105UL;
    sweepClass b = 1234;
    fmt.Println(a.peek(), b.peek());
    fmt.Println(peekVia(Ꮡa));
    lfstack c = 42;
    fmt.Println(c.peek());
    var h = ((hexval)(uint64)a);
    fmt.Println(describe(h), (uint64)((hexval)(uint64)c));
}

} // end main_package
