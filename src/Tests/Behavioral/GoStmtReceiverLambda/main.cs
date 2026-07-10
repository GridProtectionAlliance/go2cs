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
}

} // end main_package
