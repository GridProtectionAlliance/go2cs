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
    ref var c = ref Ꮡc.Value;

    return bump(Ꮡc.of(Counter.Ꮡn), delta);
}

public static void Set(this ж<Counter> Ꮡc, int32 v) {
    ref var c = ref Ꮡc.Value;

    (Ꮡc.of(Counter.Ꮡn)).Value = v;
}

[GoRecv] public static int32 Get(this ref Counter c) {
    return c.n;
}

[GoType] partial struct Flag {
    internal Counter c;
    internal @string label;
}

public static int32 Incr(this ж<Flag> Ꮡf) {
    ref var f = ref Ꮡf.Value;

    return Ꮡf.of(Flag.Ꮡc).Add(1);
}

public static int32 AddN(this ж<Flag> Ꮡf, int32 d) {
    ref var f = ref Ꮡf.Value;

    return Ꮡf.of(Flag.Ꮡc).Add(d);
}

public static void Reset(this ж<Flag> Ꮡf, int32 v) {
    ref var f = ref Ꮡf.Value;

    Ꮡf.of(Flag.Ꮡc).Set(v);
}

[GoRecv] public static int32 Value(this ref Flag f) {
    return f.c.Get();
}

[GoRecv] public static @string Label(this ref Flag f) {
    return f.label;
}

internal static int32 applyTwice(Func<int32, int32> f, int32 d) {
    f(d);
    return f(d);
}

internal static int32 readVia(Func<int32> get) {
    return get();
}

public static int32 AddTwice(this ж<Counter> Ꮡc, int32 d) {
    ref var c = ref Ꮡc.Value;

    return applyTwice(Ꮡc.Add, d);
}

public static int32 AddViaValue(this ж<Flag> Ꮡf, int32 d) {
    ref var f = ref Ꮡf.Value;

    return applyTwice(Ꮡf.of(Flag.Ꮡc).Add, d);
}

public static int32 ReadViaValue(this ж<Flag> Ꮡf) {
    ref var f = ref Ꮡf.Value;

    return readVia(Ꮡf.of(Flag.Ꮡc).Get);
}

internal static void Main() {
    ref var fl = ref heap(new Flag(), out var Ꮡfl);
    fl.label = "hits"u8;
    Ꮡfl.Reset(10);
    fmt.Println(fl.Label(), "start:", fl.Value());
    fmt.Println("Incr:", Ꮡfl.Incr());
    fmt.Println("Incr:", Ꮡfl.Incr());
    fmt.Println("AddN 5:", Ꮡfl.AddN(5));
    fmt.Println("final:", fl.Value());
    fmt.Println("AddTwice 3:", Ꮡfl.of(Flag.Ꮡc).AddTwice(3));
    fmt.Println("AddViaValue 2:", Ꮡfl.AddViaValue(2));
    fmt.Println("ReadViaValue:", Ꮡfl.ReadViaValue());
    fmt.Println("local value:", applyTwice(Ꮡfl.of(Flag.Ꮡc).Add, 1));
}

} // end main_package
