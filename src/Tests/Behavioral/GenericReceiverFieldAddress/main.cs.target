namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Box<T> {
    internal T v;
}

internal static void setT<T>(ж<T> Ꮡp, T val) {
    ref var p = ref Ꮡp.val;

    p = val;
}

internal static T getT<T>(ж<T> Ꮡp) {
    ref var p = ref Ꮡp.val;

    return p;
}

public static void Set<T>(this ж<Box<T>> Ꮡb, T val) {
    ref var b = ref Ꮡb.val;

    setT(Ꮡb.of(Box<T>.Ꮡv), val);
}

public static T Get<T>(this ж<Box<T>> Ꮡb) {
    ref var b = ref Ꮡb.val;

    return getT(Ꮡb.of(Box<T>.Ꮡv));
}

internal static void Main() {
    ref var bi = ref heap(new Box<nint>(), out var Ꮡbi);
    Ꮡbi.Set(42);
    Ꮡbi.Set(Ꮡbi.Get() + 1);
    fmt.Println("int:", Ꮡbi.Get());
    ref var bs = ref heap(new Box<@string>(), out var Ꮡbs);
    Ꮡbs.Set("hello"u8);
    fmt.Println("string:", Ꮡbs.Get());
}

} // end main_package
