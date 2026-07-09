namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Tally {
    internal nint total;
    internal @string log;
}

public static void Add(this ж<Tally> Ꮡt, nint n) => func((defer, recover) => {
    ref var t = ref Ꮡt.Value;

    defer(() => {
        Ꮡt.Value.log = fmt.Sprintf("%s+%d"u8, Ꮡt.Value.log, n);
    });
    t.total += n;
});

public static void AddTwice(this ж<Tally> Ꮡt, nint n) {
    ref var t = ref Ꮡt.Value;

    Ꮡt.Add(n);
    Ꮡt.Add(n);
}

internal static (nint, @string) bump(Tally tʗp, nint n) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    t.total++;
    Ꮡt.Add(n);
    Ꮡt.AddTwice(n * 10);
    return (t.total, t.log);
}

internal static void Main() {
    var t = new Tally(total: 5, log: "start"u8);
    var (total, log) = bump(t, 3);
    fmt.Println("bumped:", total, log);
    fmt.Println("caller copy untouched:", t.total, t.log);
}

} // end main_package
