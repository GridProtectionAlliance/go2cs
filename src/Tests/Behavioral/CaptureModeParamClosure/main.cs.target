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

internal static (nint, nint, @string) closureRead(Tally tʗp, nint n) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    var get = () => Ꮡt.Value.total;
    nint before = get();
    Ꮡt.Add(n);
    nint after = get();
    return (before, after, t.log);
}

internal static (nint, @string) closureWrite(Tally tʗp, nint n) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    var bump = () => {
        Ꮡt.Value.total += 100;
    };
    bump();
    Ꮡt.Add(n);
    bump();
    return (t.total, t.log);
}

internal static (nint result, @string log) deferClosure(Tally tʗp, nint n) {
    nint result = default!;
    @string log = default!;
    func((defer, recover) => {
    ref var t = ref heap(tʗp, out var Ꮡt);

        defer(() => {
            (result, log) = (Ꮡt.Value.total, Ꮡt.Value.log);
        });
        Ꮡt.Add(n);
        t.total += 7;
        (result, log) = (0, "");
    });
    return (result, log);
}

internal static (nint total, @string log) deferMethodValue(Tally tʗp, nint n) {
    nint total = default!;
    @string log = default!;
    func((defer, recover) => {
    ref var t = ref heap(tʗp, out var Ꮡt);

        defer(() => {
            (total, log) = (Ꮡt.Value.total, Ꮡt.Value.log);
        });
        deferǃ(Ꮡt.Add, n, defer);
        t.total++;
        (total, log) = (0, "");
    });
    return (total, log);
}

internal static void Main() {
    var t = new Tally(total: 5, log: "start"u8);
    var (before, after, log) = closureRead(t, 3);
    fmt.Println("closureRead:", before, after, log);
    (var total, log) = closureWrite(t, 3);
    fmt.Println("closureWrite:", total, log);
    (total, log) = deferClosure(t, 3);
    fmt.Println("deferClosure:", total, log);
    (total, log) = deferMethodValue(t, 3);
    fmt.Println("deferMethodValue:", total, log);
    fmt.Println("caller copy untouched:", t.total, t.log);
}

} // end main_package
