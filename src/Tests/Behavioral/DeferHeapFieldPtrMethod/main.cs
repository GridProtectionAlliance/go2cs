namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct tracker {
    internal slice<@string> lines;
}

[GoRecv] internal static void flush(this ref tracker t) {
    fmt.Printf("flush: %d lines\n"u8, len(t.lines));
    foreach (var (_, l) in t.lines) {
        fmt.Println("line:", l);
    }
}

[GoType] partial struct parser {
    internal @string name;
    internal tracker trk;
}

internal static void seed(ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p.trk.lines = append(p.trk.lines, "seed"u8);
}

internal static void run() => func((defer, recover) => {
    ref var p = ref heap<parser>(out var Ꮡp);
    p = new parser(name: "p1"u8);
    seed(Ꮡp);
    defer(Ꮡp.of(parser.Ꮡtrk).flush);
    p.trk.lines = append(p.trk.lines, "after-defer"u8);
    p.trk.lines = append(p.trk.lines, "final"u8);
    fmt.Println("run done:", p.name);
});

internal static void Main() {
    run();
}

} // end main_package
