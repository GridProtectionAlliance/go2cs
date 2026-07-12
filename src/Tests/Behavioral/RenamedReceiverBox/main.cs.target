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
    addInt(Ꮡc.of(counter.Ꮡn), d);
}

internal static void addInt(ж<nint> Ꮡx, nint d) {
    ref var x = ref Ꮡx.Value;

    x += d;
}

internal static void bumpTwice(this ж<counter> Ꮡp) {
    Ꮡp.add(1);
    Ꮡp.add(1);
}

internal static nint addInClosure(ж<counter> Ꮡp, nint d) {
    ref var Δp = ref Ꮡp.Value;

    var apply = () => {
        Ꮡp.Value.n += d;
        addInt(Ꮡp.of(counter.Ꮡn), d);
    };
    apply();
    return Δp.n;
}

internal static void Main() {
    var c = Ꮡ(new counter(n: 0));
    c.bumpTwice();
    fmt.Println((~c).n);
    fmt.Println(addInClosure(c, 5));
    fmt.Println((~c).n);
    Δp pv = default!;
    pv.id = 7;
    tagger t = default!;
    t.p();
    fmt.Println(pv.id);
}

} // end main_package
