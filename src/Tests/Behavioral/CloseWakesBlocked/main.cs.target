namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void expectPanic(@string name, Action f) => func((defer, recover) => {
    defer(() => {
        fmt.Println(name, "->", recover());
    });
    f();
});

internal static void Main() {
    var ch = new channel<nint>(0);
    var res = new channel<@string>(3);
    for (nint i = 0; i < 3; i++) {
        var chʗ1 = ch;
        var resʗ1 = res;
        goǃ(() => {
            var (v, ok) = ᐸꟷ(chʗ1, ꟷ);
            resʗ1.ᐸꟷ(fmt.Sprintf("recv %d %t"u8, v, ok));
        });
    }
    close(ch);
    for (nint i = 0; i < 3; i++) {
        fmt.Println(ᐸꟷ(res));
    }
    var ch2 = new channel<nint>(0);
    var res2 = new channel<@string>(1);
    var ch2ʗ1 = ch2;
    var res2ʗ1 = res2;
    goǃ(() => func((defer, recover) => {
        var res2ʗ2 = res2ʗ1;
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    res2ʗ2.ᐸꟷ(fmt.Sprintf("sender panicked: %v"u8, r));
                }
            }
        });
        ch2ʗ1.ᐸꟷ(1);
        res2ʗ1.ᐸꟷ("sender completed (wrong)"u8);
    }));
    close(ch2);
    fmt.Println(ᐸꟷ(res2));
    var ch3 = new channel<nint>(0);
    var other = new channel<nint>(0);
    var res3 = new channel<@string>(1);
    var ch3ʗ1 = ch3;
    var otherʗ1 = other;
    var res3ʗ1 = res3;
    goǃ(() => {
        switch (select(ᐸꟷ(ch3ʗ1, ꓸꓸꓸ), ᐸꟷ(otherʗ1, ꓸꓸꓸ))) {
        case 0 when ch3ʗ1.ꟷᐳ(out var v, out var ok): {
            res3ʗ1.ᐸꟷ(fmt.Sprintf("select recv %d %t"u8, v, ok));
            break;
        }
        case 1 when otherʗ1.ꟷᐳ(out var v): {
            res3ʗ1.ᐸꟷ(fmt.Sprintf("other %d (wrong)"u8, v));
            break;
        }}
    });
    close(ch3);
    fmt.Println(ᐸꟷ(res3));
    var ch4 = new channel<nint>(0);
    var other2 = new channel<nint>(0);
    var res4 = new channel<@string>(1);
    var ch4ʗ1 = ch4;
    var other2ʗ1 = other2;
    var res4ʗ1 = res4;
    goǃ(() => func((defer, recover) => {
        var res4ʗ2 = res4ʗ1;
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    res4ʗ2.ᐸꟷ(fmt.Sprintf("select send panicked: %v"u8, r));
                }
            }
        });
        switch (select(ch4ʗ1.ᐸꟷ(99, ꓸꓸꓸ), ᐸꟷ(other2ʗ1, ꓸꓸꓸ))) {
        case 0: {
            res4ʗ1.ᐸꟷ("select send completed (wrong)"u8);
            break;
        }
        case 1 when other2ʗ1.ꟷᐳ(out var v): {
            res4ʗ1.ᐸꟷ(fmt.Sprintf("other %d (wrong)"u8, v));
            break;
        }}
    }));
    close(ch4);
    fmt.Println(ᐸꟷ(res4));
    expectPanic("close of closed"u8, () => {
        var cc = new channel<nint>(0);
        close(cc);
        close(cc);
    });
    expectPanic("close of nil"u8, () => {
        channel<nint> nc = default!;
        close(nc);
    });
    expectPanic("send on closed"u8, () => {
        var sc = new channel<nint>(1);
        close(sc);
        sc.ᐸꟷ(1);
    });
    expectPanic("select send on closed with default"u8, () => {
        var sd = new channel<nint>(1);
        close(sd);
        switch (trySelect(sd.ᐸꟷ(1, ꓸꓸꓸ))) {
        case 0: {
            fmt.Println("sent (wrong)");
            break;
        }
        default: {
            fmt.Println("default (wrong)");
            break;
        }}
    });
}

} // end main_package
