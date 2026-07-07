namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static void reset(this ref counter c) {
    c.n = 0;
}

[GoRecv] internal static void inc(this ref counter c) {
    c.n++;
}

[GoType] partial struct builder {
    internal counter c;
}

internal static void run(this ж<builder> Ꮡb) => func((defer, recover) => {
    ref var b = ref Ꮡb.Value;

    defer(Ꮡb.of(builder.Ꮡc).reset);
    b.c.inc();
    b.c.inc();
    fmt.Println("inside:", b.c.n);
});

internal static void Main() {
    var b = Ꮡ(new builder(nil));
    b.run();
    fmt.Println("after:", (~b).c.n);
    var x = Ꮡ(new builder(nil));
    var xʗ1 = x;
    ((Action)(() => func((defer, recover) => {
        var xʗ2 = xʗ1;
        defer(xʗ2.of(builder.Ꮡc).reset);
        xʗ1.of(builder.Ꮡc).inc();
        fmt.Println("inside2:", (~xʗ1).c.n);
    })))();
    fmt.Println("after2:", (~x).c.n);
}

} // end main_package
