// Tests of call chaining f(g()) when g has multiple return values (MRVs).
// See https://code.google.com/p/go/issues/detail?id=4573.

// package main -- go2cs converted at 2022 March 06 23:33:45 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\mrvchain.go


namespace go;

public static partial class main_package {

private static void assert(nint actual, nint expected) => func((_, panic, _) => {
    if (actual != expected) {
        panic(actual);
    }
});

private static (nint, nint) g() {
    nint _p0 = default;
    nint _p0 = default;

    return (5, 7);
}

private static (double, double) g2() {
    double _p0 = default;
    double _p0 = default;

    return (5, 7);
}

private static void f1v(nint x, params nint[] v) {
    v = v.Clone();

    assert(x, 5);
    assert(v[0], 7);
}

private static void f2(nint x, nint y) {
    assert(x, 5);
    assert(y, 7);
}

private static void f2v(nint x, nint y, params nint[] v) {
    v = v.Clone();

    assert(x, 5);
    assert(y, 7);
    assert(len(v), 0);
}

private static (double, double) complexArgs() {
    double _p0 = default;
    double _p0 = default;

    return (5, 7);
}

private static (slice<@string>, @string) appendArgs() {
    slice<@string> _p0 = default;
    @string _p0 = default;

    return (new slice<@string>(new @string[] { "foo" }), "bar");
}

private static (object, bool) h() {
    object i = default;
    bool ok = default;

    map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, @string>{1:"hi"};
    i, ok = m[1]; // string->interface{} conversion within multi-valued expression
    return ;

}

private static (object, bool) h2() {
    object i = default;
    bool ok = default;

    var ch = make_channel<@string>(1);
    ch.Send("hi");
    i, ok = ch.Receive(); // string->interface{} conversion within multi-valued expression
    return ;

}

private static void Main() => func((_, panic, _) => {
    f1v(g());
    f2(g());
    f2v(g());
    {
        var c = complex(complexArgs());

        if (c != 5 + 7i) {
            panic(c);
        }
    }

    {
        var s = append(appendArgs());

        if (len(s) != 2 || s[0] != "foo" || s[1] != "bar") {
            panic(s);
        }
    }

    var (i, ok) = h();
    if (!ok || i._<@string>() != "hi") {
        panic(i);
    }
    i, ok = h2();
    if (!ok || i._<@string>() != "hi") {
        panic(i);
    }
});

} // end main_package
