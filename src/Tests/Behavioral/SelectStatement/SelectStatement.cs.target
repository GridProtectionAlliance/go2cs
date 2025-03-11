namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[]nint")] partial struct IntSlice;

internal static void g1(channel<nint> ch) {
    ch.ᐸꟷ(12);
}

internal static void g2(channel<nint> ch) {
    ch.ᐸꟷ(32);
}

internal static void sum(slice<nint> s, channel<nint> c) {
    nint sum = 0;
    foreach (var (_, v) in s) {
        sum += v;
    }
    c.ᐸꟷ(sum);
}

internal static void fibonacci(channel<nint> f, channel<nint> quit) {
    nint x = 0;
    nint y = 1;
    while (ᐧ) {
        switch (select(f.ᐸꟷ(x, ꓸꓸꓸ), ᐸꟷ(quit, ꓸꓸꓸ))) {
        case 0:
            (x, y) = (y, x + y);
            break;
        case 1 when quit.ꟷᐳ(out _):
            fmt.Println("quit");
            return;
        }
    }
}

internal static void sendOnly(channel/*<-*/<@string> s) {
    s.ᐸꟷ("output"u8);
}

public static Action<Func<nint, bool>> All(this IntSlice s) {
    var sʗ1 = s;
    return (Func<nint, bool> yield) => {
        foreach (var (_, v) in sʗ1) {
            if (!yield(v)) {
                return;
            }
        }
    };
}

internal static void generate(channel/*<-*/<nint> ch) {
    for (nint i = 2; ᐧ ; i++) {
        ch.ᐸꟷ(i);
    }
}

internal static void filter(/*<-*/channel<nint> src, channel/*<-*/<nint> dst, nint prime) {
    foreach (var i in src) {
        if (i % prime != 0) {
            dst.ᐸꟷ(i);
        }
    }
}

internal static void sieve() {
    var ch = new channel<nint>(1);
    var chʗ1 = ch;
    goǃ(_ => generate(chʗ1));
    while (ᐧ) {
        nint prime = ᐸꟷ(ch);
        fmt.Print(prime, "\n");
        var ch1 = new channel<nint>(1);
        var chʗ2 = ch;
        var ch1ʗ1 = ch1;
        goǃ(_ => filter(chʗ2, ch1ʗ1, prime));
        ch = ch1;
        if (prime > 40) {
            break;
        }
    }
}

internal static nint f() {
    return 0;
}

internal static void Main() {
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(1);
    ch.ᐸꟷ(2);
    fmt.Println(ᐸꟷ(ch));
    fmt.Println(ᐸꟷ(ch));
    var a = new slice<nint>(2);
    var ch1 = new channel<nint>(1);
    var ch2 = new channel<nint>(1);
    var ch3 = new channel<nint>(1);
    var ch4 = new channel<nint>(1);
    var ch1ʗ1 = ch1;
    goǃ(_ => g1(ch1ʗ1));
    var ch2ʗ1 = ch2;
    goǃ(_ => g2(ch2ʗ1));
    var ch3ʗ1 = ch3;
    goǃ(_ => g1(ch3ʗ1));
    var ch4ʗ1 = ch4;
    goǃ(_ => g2(ch4ʗ1));
    for (nint i = 0; i < 4; i++) {
        switch (select(ᐸꟷ(ch1, ꓸꓸꓸ), ᐸꟷ(ch2, ꓸꓸꓸ), ᐸꟷ(ch3, ꓸꓸꓸ), ᐸꟷ(ch4, ꓸꓸꓸ))) {
        case 0 when ch1.ꟷᐳ(out var v1):
            fmt.Println("Got: ", v1);
            break;
        case 1 when ch2.ꟷᐳ(out var v1):
            fmt.Println("Got: ", v1);
            break;
        case 2 when ch3.ꟷᐳ(out var v1, out var okΔ1):
            fmt.Println("OK: ", okΔ1, " -- got: ", v1);
            break;
        case 3 when ch4.ꟷᐳ(out a[f()]):
            fmt.Println("Got: ", a[f()]);
            break;
        }
    }
    ch1 = default!;
    close(ch2);
    switch (select(ch1.ᐸꟷ(1, ꓸꓸꓸ), ᐸꟷ(ch1, ꓸꓸꓸ), ᐸꟷ(ch2, ꓸꓸꓸ), ᐸꟷ(ch3, ꓸꓸꓸ), ᐸꟷ(ch4, ꓸꓸꓸ))) {
    case 0:
        fmt.Println("unexpected send to nil channel");
        break;
    case 1 when ch1.ꟷᐳ(out var v1):
        fmt.Println("unexpected received from nil channel: ", v1);
        break;
    case 2 when ch2.ꟷᐳ(out var v1):
        fmt.Println("closed channel 2 selected immediately: ", v1);
        break;
    case 3 when ch3.ꟷᐳ(out var v1, out var okΔ2):
        fmt.Println("unexpected: OK: ", okΔ2, " -- got: ", v1);
        break;
    case 4 when ch4.ꟷᐳ(out a[f()]):
        fmt.Println("unexpected: ", a[f()]);
        break;
    }
    var s = new nint[]{7, 2, 8, -9, 4, 0}.slice();
    var c = new channel<nint>(1);
    var cʗ1 = c;
    var sʗ1 = s;
    goǃ(_ => sum(sʗ1[..(int)(len(sʗ1) / 2)], cʗ1));
    var cʗ2 = c;
    var sʗ2 = s;
    goǃ(_ => sum(sʗ2[(int)(len(sʗ2) / 2)..], cʗ2));
    var cʗ3 = c;
    var sʗ3 = s;
    goǃ(_ => sum(sʗ3[2..5], cʗ3));
    nint x = ᐸꟷ(c);
    nint y = ᐸꟷ(c);
    nint z = ᐸꟷ(c);
    fmt.Println(x, y, x + y, z);
    var fΔ1 = new channel<nint>(1);
    var quit = new channel<nint>(1);
    var fʗ1 = fΔ1;
    var quitʗ1 = quit;
    goǃ(() => {
        for (nint i = 0; i < 10; i++) {
            fmt.Println(ᐸꟷ(fʗ1));
        }
        quitʗ1.ᐸꟷ(0);
    });
    fibonacci(fΔ1, quit);
    var mychanl = new channel<@string>(1);
    var mychanlʗ1 = mychanl;
    goǃ(_ => sendOnly(mychanlʗ1));
    var (result, ok) = ᐸꟷ(mychanl, ꟷ);
    fmt.Println(result, ok);
    foreach (var v in range(((IntSlice)s).All())) {
        fmt.Println(v);
    }
    sieve();
}

} // end main_package
