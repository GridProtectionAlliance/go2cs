namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Tally {
    internal nint total;
    internal @string log;
}

[GoRecv] public static void Add(this ref Tally t, nint n) {
    t.total += n;
    t.log += "+"u8;
}

internal static void probeA1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var bump = () => {
        Ꮡt.Value.total += 100;
    };
    bump();
    t.total++;
    fmt.Println("A1:", t.total, t.log);
}

internal static void probeA2() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var get = () => Ꮡt.Value.total;
    t.total += 10;
    fmt.Println("A2:", get());
}

internal static void probeA3() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    t.total += 2;
    var tʗ1 = t;
    var get = () => tʗ1.total;
    fmt.Println("A3:", get());
}

internal static void probeB1(Tally t) {
    var bump = () => {
        t.total += 100;
    };
    bump();
    t.total++;
    fmt.Println("B1:", t.total);
}

internal static void probeB2(Tally t) {
    var get = () => t.total;
    t.total += 10;
    fmt.Println("B2:", get());
}

internal static void probeC1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var bump = () => {
        Ꮡt.Value.total += 100;
    };
    bump();
    t.Add(3);
    bump();
    fmt.Println("C1:", t.total, t.log);
}

internal static void probeC2() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var get = () => Ꮡt.Value.total;
    t.Add(3);
    fmt.Println("C2:", get());
}

internal static void probeD1(Tally t) {
    var bump = () => {
        t.total += 100;
    };
    bump();
    t.Add(3);
    bump();
    fmt.Println("D1:", t.total, t.log);
}

internal static void probeE1() => func((defer, recover) => {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    defer(() => {
        fmt.Println("E1:", Ꮡt.Value.total);
    });
    t.total = 42;
});

internal static void probeE2() => func((defer, recover) => {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    deferǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), "E2:", t.total, defer);
    t.total = 42;
});

internal static void probeF1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var done = new channel<nint>(0);
    var doneʗ1 = done;
    goǃ(() => {
        Ꮡt.Value.total += 100;
        doneʗ1.ᐸꟷ(1);
    });
    ᐸꟷ(done);
    fmt.Println("F1:", t.total);
}

internal static void probeG1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    slice<Action> fs = default!;
    for (nint i = 0; i < 2; i++) {
        fs = append(fs, () => {
            Ꮡt.Value.total += 10;
        });
    }
    fs[0]();
    fs[1]();
    fmt.Println("G1:", t.total);
}

internal static void probeG3() {
    slice<Func<nint>> fs = default!;
    foreach (var (_, x) in new nint[]{10, 20, 30}.slice()) {
        fs = append(fs, () => x);
    }
    fmt.Println("G3:", fs[0](), fs[1](), fs[2]());
}

internal static void probeH1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var inc = () => {
        Ꮡt.Value.total++;
    };
    var get = () => Ꮡt.Value.total;
    inc();
    inc();
    fmt.Println("H1:", get());
}

internal static void probeI1() {
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    s = new nint[]{1}.slice();
    var app = () => {
        Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, (nint)(2));
    };
    app();
    fmt.Println("I1:", len(s), s[0]);
}

internal static void probeJ1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    ((Action)(() => {
        Ꮡt.Value.total += 100;
    }))();
    fmt.Println("J1:", t.total);
}

internal static void probeK1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var p = Ꮡt;
    var get = () => Ꮡt.Value.total;
    p.Value.total = 50;
    fmt.Println("K1:", get());
}

internal static void probeL1() {
    ref var m = ref heap<map<nint, nint>>(out var Ꮡm);
    m = new map<nint, nint>{};
    var set = () => {
        Ꮡm.ValueSlot = new map<nint, nint>{[1] = 1};
    };
    set();
    fmt.Println("L1:", len(m));
}

internal static void probeM1() {
    ref var t = ref heap<Tally>(out var Ꮡt);
    t = new Tally(5, "s");
    var bump = () => {
        Ꮡt.Value.total += 100;
    };
    var get = () => Ꮡt.Value.total;
    bump();
    fmt.Println("M1:", get(), t.total);
}

internal static void probeN1() {
    nint n = 0;
    var inc = () => {
        n++;
    };
    inc();
    fmt.Println("N1:", n);
}

internal static void probeN2() {
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 0;
    var p = Ꮡn;
    var inc = () => {
        Ꮡn.Value += 5;
    };
    inc();
    p.Value += 2;
    fmt.Println("N2:", n);
}

internal static (V, nint) probeP1<V>(slice<V> seq) {
    ref var v = ref heap<V>(out var Ꮡv);
    nint n = 0;
    var p = Ꮡv;
    var set = (V x) => {
        (Ꮡv.ValueSlot, n) = (x, n + 1);
    };
    foreach (var (_, x) in seq) {
        set(x);
    }
    return (p.ValueSlot, n);
}

internal static void Main() {
    probeA1();
    probeA2();
    probeA3();
    probeB1(new Tally(5, "s"));
    probeB2(new Tally(5, "s"));
    probeC1();
    probeC2();
    probeD1(new Tally(5, "s"));
    probeE1();
    probeE2();
    probeF1();
    probeG1();
    probeG3();
    probeH1();
    probeI1();
    probeJ1();
    probeK1();
    probeL1();
    probeM1();
    probeN1();
    probeN2();
    var (v, n) = probeP1(new nint[]{10, 20, 30}.slice());
    fmt.Println("P1:", v, n);
}

} // end main_package
