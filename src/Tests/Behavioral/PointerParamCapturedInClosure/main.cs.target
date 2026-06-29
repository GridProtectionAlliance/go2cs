namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct config {
    internal nint size;
    internal nint tag;
}

internal static Func<nint, nint> scaler(ж<config> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return (nint x) => x * Ꮡc.val.size + Ꮡc.val.tag;
}

internal static Action mutate(ж<config> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return () => {
        Ꮡc.val.size++;
    };
}

internal static void Main() {
    var c = Ꮡ(new config(size: 10, tag: 3));
    var f = scaler(c);
    fmt.Println(f(5));
    var inc = mutate(c);
    inc();
    inc();
    fmt.Println((~c).size);
    fmt.Println(f(2));
}

} // end main_package
