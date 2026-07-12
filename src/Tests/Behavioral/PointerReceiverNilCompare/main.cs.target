namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal nint val;
}

internal static bool isNil(this ж<box> Ꮡb) {
    return Ꮡb == nil;
}

internal static bool notNil(this ж<box> Ꮡb) {
    return Ꮡb != nil;
}

internal static bool same(this ж<box> Ꮡb, ж<box> Ꮡother) {
    return Ꮡb == Ꮡother;
}

[GoType] partial struct embedder {
    internal partial ref box box { get; }
    internal nint tag;
}

internal static bool isNil(this ж<embedder> Ꮡe) {
    return Ꮡe == nil;
}

internal static @string checkGuard(this ж<embedder> Ꮡe) {
    ref var e = ref Ꮡe.Value;

    if (Ꮡe == nil) {
        return "nil"u8;
    }
    return fmt.Sprintf("val=%d tag=%d"u8, e.val, e.tag);
}

internal static void Main() {
    var b = Ꮡ(new box(nil));
    fmt.Println(b.isNil(), b.notNil());
    fmt.Println(b.same(b), b.same(Ꮡ(new box(nil))));
    var e = Ꮡ(new embedder(nil));
    e.Value.val = 7;
    e.Value.tag = 9;
    fmt.Println(e.isNil(), e.checkGuard());
}

} // end main_package
