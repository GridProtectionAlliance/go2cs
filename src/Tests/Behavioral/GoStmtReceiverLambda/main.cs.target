namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
    internal channel<bool> done;
}

[GoRecv] internal static error bump(this ref counter c, nint delta) {
    c.n += delta;
    c.done.ᐸꟷ(true);
    return default!;
}

[GoRecv] internal static error report(this ref counter c) {
    c.done.ᐸꟷ(true);
    return default!;
}

[GoType] partial struct engine {
    internal ж<counter> tally;
}

internal static void start(this ж<engine> Ꮡe, nint delta) {
    ref var e = ref Ꮡe.Value;

    goǃ(ᴛ1 => Ꮡe.Value.tally.bump(ᴛ1), delta);
}

internal static void ping(this ж<engine> Ꮡe) {
    ref var e = ref Ꮡe.Value;

    goǃ(() => Ꮡe.Value.tally.report());
}

[GoType] partial struct valueSender {
    internal channel<nint> c;
}

internal static void send(this valueSender v, nint n) {
    v.c.ᐸꟷ(n);
}

internal static void ping(this valueSender v) {
    v.c.ᐸꟷ(99);
}

internal static void Main() {
    var e = Ꮡ(new engine(tally: Ꮡ(new counter(done: new channel<bool>(1)))));
    e.start(5);
    ᐸꟷ((~(~e).tally).done);
    fmt.Println("after start:", (~(~e).tally).n);
    e.start(7);
    ᐸꟷ((~(~e).tally).done);
    fmt.Println("after second start:", (~(~e).tally).n);
    e.ping();
    ᐸꟷ((~(~e).tally).done);
    fmt.Println("pinged:", (~(~e).tally).n);
    ref var vs = ref heap<valueSender>(out var Ꮡvs);
    vs = new valueSender(c: new channel<nint>(1));
    var vsʗ1 = vs;
    goǃ(ᴛ1 => vsʗ1.send(ᴛ1), 7);
    fmt.Println("value-recv go:", ᐸꟷ(vs.c));
    var vsʗ2 = vs;
    goǃ(() => vsʗ2.ping());
    fmt.Println("value-recv nullary go:", ᐸꟷ(vs.c));
}

} // end main_package
