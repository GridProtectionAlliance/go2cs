namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Δp {
    internal nint id;
}

[GoType] partial struct tagger {
}

internal static void p(this tagger _) {
}

[GoType] partial struct counter {
    internal nint n;
}

internal static void add(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.val;

    addInt(Ꮡc.of(counter.Ꮡn), d);
}

internal static void addInt(ж<nint> Ꮡx, nint d) {
    ref var x = ref Ꮡx.val;

    x += d;
}

internal static void bumpTwice(this ж<counter> Ꮡp) {
    ref var Δp = ref Ꮡp.val;

    Ꮡp.add(1);
    Ꮡp.add(1);
}

internal static void Main() {
    var c = Ꮡ(new counter(n: 0));
    c.bumpTwice();
    fmt.Println((~c).n);
    Δp pv = default!;
    pv.id = 7;
    tagger t = default!;
    t.p();
    fmt.Println(pv.id);
}

} // end main_package
