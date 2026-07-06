namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

// type stopFn is a methodless func type — rendered inline as its base delegate

internal static Action makeStop(@string tag, channel/*<-*/<@string> @out) {
    var outʗ1 = @out;
    return () => {
        outʗ1.ᐸꟷ(tag);
    };
}

internal static void Main() => func((defer, recover) => {
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "First", defer);
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "Second", defer);
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "Third", defer);
    var f1 = fmt.Println;
    var f1ʗ1 = f1;
    deferǃ(ᴛ1 => f1ʗ1(ᴛ1), "Fourth", defer);
    var msgs = new channel<@string>(2);
    var cancel = makeStop("stopped"u8, msgs);
    var cancelʗ1 = cancel;
    defer(() => cancelʗ1());
    var msgsʗ1 = msgs;
    goǃ(() => makeStop("go-stopped"u8, msgsʗ1)());
    fmt.Println(ᐸꟷ(msgs));
    var drained = new channel<nint>(1);
    var drainedʗ1 = drained;
    defer(() => {
        var (v, open) = ᐸꟷ(drainedʗ1, ꟷ);
        fmt.Println("after close:", v, open);
    });
    deferǃ(ᴛ1 => close(ᴛ1), drained, defer);
    deferǃ(GetPrintLn(), (@string)"Fifth", defer);
    var c = Ꮡ(new acc(nil));
    var (s1, e1) = c.add(5);
    fmt.Println(s1, e1);
    var (s2, e2) = c.add(-1);
    fmt.Println(s2, e2, (~c).total);
    var sm = Ꮡ(new sema(nil));
    acquireAndWork(sm);
    fmt.Println("after:", (~sm).held);
    watchAndSend(sm);
    fmt.Println("sent:", ᐸꟷ((~sm).@out), "| held:", (~sm).held);
    fmt.Println("notify:", notifyAll(1, 2, 3));
    fmt.Println("Main function");
});

[GoType] partial struct sema {
    internal bool held;
    internal channel<nint> @out;
}

[GoRecv] internal static void release(this ref sema s) {
    s.held = false;
    fmt.Println("sema released");
}

[GoRecv] internal static void send(this ref sema s, nint n) {
    s.@out.ᐸꟷ(n);
}

internal static void watchAndSend(ж<sema> Ꮡs) => func((defer, recover) => {
    ref var s = ref Ꮡs.Value;

    s.@out = new channel<nint>(2);
    s.held = true;
    goǃ(Ꮡs.send, (nint)(7));
    fmt.Println("sent-first:", ᐸꟷ(s.@out));
    deferǃ(Ꮡs.send, (nint)(9), defer);
    defer(Ꮡs.release);
    fmt.Println("watching, held:", s.held);
});

internal static void acquireAndWork(ж<sema> Ꮡs) => func((defer, recover) => {
    ref var s = ref Ꮡs.Value;

    s.held = true;
    defer(Ꮡs.release);
    fmt.Println("working, held:", s.held);
});

internal static nint notifyAll(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.slice();
    return func((defer, recover) => {
        deferǃ(ᴛ1 => fmt.Println(ᴛ1), "notified", defer);
        nint total = 0;
        foreach (var (_, v) in vals) {
            total += v;
        }
        return total;
    });
}

[GoType] partial struct acc {
    internal nint total;
}

internal static (nint sum, error err) add(this ж<acc> Ꮡa, nint n) {
    nint sum = default!;
    error err = default!;
    func((defer, recover) => {
    ref var a = ref Ꮡa.Value;

        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    err = fmt.Errorf("boom"u8);
                }
            }
        });
        a.total += n;
        if (n < 0) {
            throw panic("negative");
        }
        sum = a.total;
    });
    return (sum, err);
}

public static Action<@string> GetPrintLn() {
    return (@string src) => {
        fmt.Println(src);
    };
}

} // end main_package
