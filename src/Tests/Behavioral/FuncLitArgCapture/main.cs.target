namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal nint x;
    internal nint y;
}

internal static ж<box> gPtr;

internal static box gVal = new box(x: 7);

internal static void run(Action f) {
    f();
}

internal static void set(ж<box> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p.x = 42;
}

[GoType] partial struct payload {
    internal slice<nint> vals;
}

internal static nint sum(this payload p) {
    nint t = 0;
    foreach (var (_, v) in p.vals) {
        t += v;
    }
    return t;
}

internal static nint nestedStructCapture() {
    ref var p = ref heap<payload>(out var Ꮡp);
    p = new payload(vals: new nint[]{1, 2, 3, 4}.slice());
    var @out = new channel<nint>(1);
    var outʗ1 = @out;
    var pʗ1 = p;
    var outer = () => {
        var outʗ2 = outʗ1;
        var pʗ2 = pʗ1;
        goǃ(() => {
            outʗ2.ᐸꟷ(pʗ2.sum());
        });
    };
    outer();
    return ᐸꟷ(@out);
}

internal static nint selfRefCapture() {
    var done = new channel<nint>(1);
    var doneʗ1 = done;
    var worker = (Action cb) => {
        cb();
        doneʗ1.ᐸꟷ(5);
    };
    var workerʗ1 = worker;
    goǃ(workerʗ1, () => {
    });
    return ᐸꟷ(done);
}

internal static void deferArgCapture(ж<box> Ꮡout) => func((defer, recover) => {
    ref var @out = ref Ꮡout.Value;

    var pf = Ꮡout;
    var pfʗ1 = pf;
    deferǃ(run, () => {
        pfʗ1.Value.x = 77;
    }, defer);
    pf.Value.x = 5;
});

internal static void Main() {
    ref var m = ref heap(new box(), out var Ꮡm);
    run(() => {
        set(Ꮡm);
    });
    fmt.Println("1:", m.x);
    ref var n = ref heap(new box(), out var Ꮡn);
    run(() => {
        set(Ꮡn);
        Ꮡn.Value.y = Ꮡn.Value.x + 1;
    });
    fmt.Println("2:", n.x, n.y);
    ref var c = ref heap(new box(), out var Ꮡc);
    run(() => {
        var p = Ꮡc.of(box.Ꮡx);
        p.Value = 99;
    });
    fmt.Println("3:", c.x);
    ref var d = ref heap(new box(), out var Ꮡd);
    var f = () => {
        set(Ꮡd);
    };
    f();
    fmt.Println("4:", d.x);
    ref var e = ref heap(new box(), out var Ꮡe);
    var pe = Ꮡe;
    var peʗ1 = pe;
    run(() => {
        peʗ1.Value.x = 11;
        peʗ1.Value.y = (~peʗ1).x + 1;
    });
    fmt.Println("5:", e.x, e.y);
    gPtr = Ꮡe;
    run(() => {
        gPtr.Value.x = gVal.x;
    });
    fmt.Println("6:", e.x);
    var vals = new nint[]{5}.slice();
    var valsʗ1 = vals;
    var adder = (nint k) => k + valsʗ1[0];
    fmt.Println("7:", adder(10));
    var @base = new nint[]{3}.slice();
        var baseʗ1 = @base;
    var handlers = new Func<nint, nint>[]{
        (nint k) => k + baseʗ1[0]
    }.slice();
    fmt.Println("8:", handlers[0](100));
    var seed = new nint[]{2}.slice();

    var seedʗ1 = seed;
    Func<nint, nint> mul = (nint k) => k * seedʗ1[0];
    fmt.Println("9:", mul(21));
    var pick = (nint sel) => {
        nint x = default!;
        bool ok = default!;
        if (sel > 0) {
            x = sel * 7;
            ok = true;
            return (x, ok);
        }
        return (x, ok);
    };
    var (a, b) = pick(3);
    var (zx, zok) = pick(-1);
    fmt.Println("10:", a, b, zx, zok);
    var zero = () => {
        nint nΔ1 = default!;
        @string s = default!;
        return (nΔ1, s);
    };
    var (n0, s0) = zero();
    fmt.Println("11:", n0, s0 == ""u8);
    fmt.Println("12:", nestedStructCapture());
    fmt.Println("13:", selfRefCapture());
    ref var g = ref heap(new box(), out var Ꮡg);
    deferArgCapture(Ꮡg);
    fmt.Println("14:", g.x);
}

} // end main_package
