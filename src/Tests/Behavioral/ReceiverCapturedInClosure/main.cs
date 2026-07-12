namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
}

internal static void addInt(ж<nint> Ꮡx, nint d) {
    ref var x = ref Ꮡx.Value;

    x += d;
}

internal static nint addInClosure(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.Value;

    var apply = () => {
        Ꮡc.Value.n += d;
    };
    apply();
    return c.n;
}

internal static nint addViaFieldPtr(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.Value;

    var apply = () => {
        addInt(Ꮡc.of(counter.Ꮡn), d);
    };
    apply();
    return c.n;
}

internal static Action<nint> makeAdder(this ж<counter> Ꮡc) {
    return (nint d) => {
        Ꮡc.Value.n += d;
    };
}

[GoType("num:nint")] partial struct label;

internal static @string render(this label l) {
    return fmt.Sprintf("L%d"u8, (nint)l);
}

[GoType] partial struct widget {
    internal label id;
}

internal static @string tag(this widget w) {
    return fmt.Sprintf("W%d"u8, (nint)w.id);
}

internal static @string call(Func<@string> f) {
    return f();
}

internal static @string viaFieldMethodValue(this ж<widget> Ꮡw) {
    return call(() => Ꮡw.Value.id.render());
}

internal static @string viaBareMethodValue(this ж<widget> Ꮡw) {
    return call(() => Ꮡw.Value.tag());
}

internal static void Main() {
    var c = Ꮡ(new counter(n: 0));
    fmt.Println(c.addInClosure(5));
    fmt.Println(c.addViaFieldPtr(3));
    var add = c.makeAdder();
    add(10);
    add(2);
    fmt.Println((~c).n);
    var w = Ꮡ(new widget(id: 42));
    fmt.Println(w.viaFieldMethodValue());
    fmt.Println(w.viaBareMethodValue());
}

} // end main_package
