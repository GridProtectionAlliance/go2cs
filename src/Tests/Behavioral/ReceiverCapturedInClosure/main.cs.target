namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
}

internal static void addInt(ж<nint> Ꮡx, nint d) {
    ref var x = ref Ꮡx.val;

    x += d;
}

internal static nint addInClosure(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.val;

    var apply = () => {
        Ꮡc.val.n += d;
    };
    apply();
    return c.n;
}

internal static nint addViaFieldPtr(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.val;

    var apply = () => {
        addInt(Ꮡc.of(counter.Ꮡn), d);
    };
    apply();
    return c.n;
}

internal static Action<nint> makeAdder(this ж<counter> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return (nint d) => {
        Ꮡc.val.n += d;
    };
}

internal static void Main() {
    var c = Ꮡ(new counter(n: 0));
    fmt.Println(c.addInClosure(5));
    fmt.Println(c.addViaFieldPtr(3));
    var add = c.makeAdder();
    add(10);
    add(2);
    fmt.Println((~c).n);
}

} // end main_package
