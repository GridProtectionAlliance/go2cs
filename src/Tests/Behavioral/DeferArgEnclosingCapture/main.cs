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

internal static void report(@string prefix, Tally tʗp, nint n) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    Ꮡt.Add(n);
    fmt.Println(prefix, t.total, t.log);
}

internal static void Main() {
    ref var @base = ref heap<Tally>(out var Ꮡbase);
    @base = new Tally(total: 2, log: "d"u8);
    var baseʗ1 = @base;

    ((Action)(() => func((defer, recover) => {
        deferǃ((Tally t) => {
            report("deferred:"u8, t, 4);
        }, baseʗ1, defer);
    })))();
    var baseʗ2 = @base;
    ((Action)(() => func((defer, recover) => {
        deferǃ(report, (@string)"named:", baseʗ2, (nint)(5), defer);
    })))();
    var done = new channel<bool>(0);
    var baseʗ3 = @base;
    var doneʗ1 = done;

        var doneʗ2 = doneʗ1;
    ((Action)(() => {
        var doneʗ3 = doneʗ1;
        goǃ((Tally t) => {
            report("goroutine:"u8, t, 7);
            doneʗ3.ᐸꟷ(true);
        }, baseʗ3);
    }))();
    ᐸꟷ(done);
    fmt.Println("source untouched:", @base.total, @base.log);
}

} // end main_package
