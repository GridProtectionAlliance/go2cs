namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate void stopFn();

internal static stopFn makeStop(@string tag, channel/*<-*/<@string> @out) {
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
    deferǃ(GetPrintLn(), (@string)"Fifth", defer);
    var c = Ꮡ(new acc(nil));
    var (s1, e1) = c.add(5);
    fmt.Println(s1, e1);
    var (s2, e2) = c.add(-1);
    fmt.Println(s2, e2, (~c).total);
    fmt.Println("Main function");
});

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
