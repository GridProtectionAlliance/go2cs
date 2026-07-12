namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Counter {
    internal int32 n;
}

internal static int32 bump(ж<int32> Ꮡp, int32 delta) {
    ref var p = ref Ꮡp.Value;

    p += delta;
    return p;
}

public static int32 Add(this ж<Counter> Ꮡc, int32 delta) {
    return bump(Ꮡc.of(Counter.Ꮡn), delta);
}

public static void Set(this ж<Counter> Ꮡc, int32 v) {
    (Ꮡc.of(Counter.Ꮡn)).Value = v;
}

[GoRecv] public static int32 Get(this ref Counter c) {
    return c.n;
}

internal static void Main() {
    ref var c = ref heap(new Counter(), out var Ꮡc);
    Ꮡc.Set(100);
    fmt.Println("after Set:", c.Get());
    fmt.Println("Add 10:", Ꮡc.Add(10));
    fmt.Println("Add 5:", Ꮡc.Add(5));
    fmt.Println("final:", c.Get());
}

} // end main_package
