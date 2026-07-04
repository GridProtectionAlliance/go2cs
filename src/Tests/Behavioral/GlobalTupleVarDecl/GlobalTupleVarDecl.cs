namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal nint v;
}

internal static nint calls;

internal static (ж<box>, error) makeBox(nint v) {
    return (Ꮡ(new box(v: v)), default!);
}

internal static (nint, @string) pair() {
    calls++;
    return (42, "hello");
}

internal static ж<box> defaultBox = makeBox(7).Item1;
internal static error _ᴛ1ʗ;

internal static (nint, @string) tupleᴛ1ʗ = pair();
internal static nint n = tupleᴛ1ʗ.Item1;
internal static @string s = tupleᴛ1ʗ.Item2;

internal static void Main() {
    fmt.Println((~defaultBox).v);
    fmt.Println(n, s);
    fmt.Println(calls);

    var (ln, ls) = pair();
    fmt.Println(ln, ls, calls);
}

} // end main_package
