namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint64 load64(ж<uint64> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    return p;
}

internal static uint32 load32(ж<uint32> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    return p;
}

[GoType("num:uint64")] partial struct lfstack;

[GoType("num:uint32")] partial struct sweepClass;

internal static uint64 peek(this ж<lfstack> Ꮡhead) {
    ref var head = ref Ꮡhead.Value;

    return load64(Ꮡ((uint64)(head)));
}

internal static uint32 peek(this ж<sweepClass> Ꮡsc) {
    ref var sc = ref Ꮡsc.Value;

    return load32(Ꮡ((uint32)(sc)));
}

internal static uint64 peekVia(ж<lfstack> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    return load64(Ꮡ((uint64)(p)));
}

[GoType("num:uint64")] partial struct hexval;

internal static uint64 describe(hexval v) {
    return (uint64)v;
}

internal static void Main() {
    ref var a = ref heap(new lfstack(), out var Ꮡa);
    a = 0xDEADBEEF01UL;
    ref var b = ref heap(new sweepClass(), out var Ꮡb);
    b = 1234;
    fmt.Println(Ꮡa.peek(), Ꮡb.peek());
    fmt.Println(peekVia(Ꮡa));
    ref var c = ref heap(new lfstack(), out var Ꮡc);
    c = 42;
    fmt.Println(Ꮡc.peek());
    var h = ((hexval)(uint64)a);
    fmt.Println(describe(h), (uint64)((hexval)(uint64)c));
}

} // end main_package
