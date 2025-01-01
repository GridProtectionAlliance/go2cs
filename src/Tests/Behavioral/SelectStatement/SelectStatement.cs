namespace go;

using fmt = fmt_package;

public static partial class main_package {

[GoType("[]nint")]
public partial struct IntSlice {}

private static void g1(channel<nint> ch) {
    ch.ᐸꟷ(12);
}

private static void g2(channel<nint> ch) {
    ch.ᐸꟷ(32);
}

private static void sum(slice<nint> s, channel<nint> c) {
    nint sum = 0;
    foreach (var (_, v) in s) {
        sum += v;
    }
    c.ᐸꟷ(sum);
}

private static void fibonacci(channel<nint> f, channel<nint> quit) {
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

private static void sendOnly(channel/*<-*/<@string> s) {
    s.ᐸꟷ("output"u8);
}

public static Action<Func<nint, bool>> All(this IntSlice s) {
    return (Func<nint, bool> yield) => {
        foreach (var (_, v) in s) {
            if (!yield(v)) {
                return;
            }
        }
    };
}

private static void generate(channel/*<-*/<nint> ch) {
    for (nint i = 2; i < 100; i++) {
        ch.ᐸꟷ(i);
    }
}

private static void filter(/*<-*/channel<nint> src, channel/*<-*/<nint> dst, nint prime) {
    foreach (var i in src) {
        if (i % prime != 0) {
            dst.ᐸꟷ(i);
        }
    }
}

private static void sieve() {
    var ch = new channel<nint>(1);
    goǃ(_ => generate(ch));
    while (ᐧ) {
        nint prime = ᐸꟷ(ch);
        fmt.Print(prime, "\n");
        var ch1 = new channel<nint>(1);
        goǃ(_ => filter(ch, ch1, prime));
        ch = ch1;
        if (prime > 95) {
            break;
        }
    }
}

private static void Main() {
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(1);
    ch.ᐸꟷ(2);
    fmt.Println(ᐸꟷ(ch));
    fmt.Println(ᐸꟷ(ch));
    var ch1 = new channel<nint>(1);
    var ch2 = new channel<nint>(1);
    var ch3 = new channel<nint>(1);
    goǃ(_ => g1(ch1));
    goǃ(_ => g2(ch2));
    goǃ(_ => g1(ch3));
    for (nint i = 0; i < 3; i++) {
        switch (select(ᐸꟷ(ch1, ꓸꓸꓸ), ᐸꟷ(ch2, ꓸꓸꓸ), ᐸꟷ(ch3, ꓸꓸꓸ))) {
        case 0 when ch1.ꟷᐳ(out var v1):
            fmt.Println("Got: ", v1);
            break;
        case 1 when ch2.ꟷᐳ(out var v1):
            fmt.Println("Got: ", v1);
            break;
        case 2 when ch3.ꟷᐳ(out var v1, out var okΔ1):
            fmt.Println("OK: ", okΔ1, " -- got: ", v1);
            break;
        }
    }
    var s = new nint[]{7, 2, 8, -9, 4, 0}.slice();
    var c = new channel<nint>(1);
    goǃ(_ => sum(s[..(int)(len(s) / 2)], c));
    goǃ(_ => sum(s[(int)(len(s) / 2)..], c));
    goǃ(_ => sum(s[2..5], c));
    nint x = ᐸꟷ(c);
    nint y = ᐸꟷ(c);
    nint z = ᐸꟷ(c);
    fmt.Println(x, y, x + y, z);
    var f = new channel<nint>(1);
    var quit = new channel<nint>(1);
    goǃ(() => {
        for (nint i = 0; i < 10; i++) {
            fmt.Println(ᐸꟷ(f));
        }
        quit.ᐸꟷ(0);
    });
    fibonacci(f, quit);
    var mychanl = new channel<@string>(1);
    goǃ(_ => sendOnly(mychanl));
    var (result, ok) = ᐸꟷ(mychanl, ꟷ);
    fmt.Println(result, ok);
    foreach (var v in range(((IntSlice)(s)).All())) {
        fmt.Println(v);
    }
    //sieve();
}

} // end main_package
