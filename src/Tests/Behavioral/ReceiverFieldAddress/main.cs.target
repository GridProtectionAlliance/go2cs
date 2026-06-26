namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Counter {
    internal int32 n;
}

internal static int32 bump(ж<int32> Ꮡp, int32 delta) {
    ref var p = ref Ꮡp.val;

    p += delta;
    return p;
}

[GoRecv("capture")] public static int32 Add(this ref Counter c, int32 delta) {
    return bump(Add_CounterꓸᏑc.of(Counter.Ꮡn), delta);
}

[GoRecv("capture")] public static void Set(this ref Counter c, int32 v) {
    (Set_CounterꓸᏑc.of(Counter.Ꮡn)).val = v;
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
