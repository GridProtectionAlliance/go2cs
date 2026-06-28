namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct note {
    internal nint x;
}

internal static nint count;

internal static void setup(ж<note> _) {
    count += 1;
}

internal static void discard(nint _) {
    count += 10;
}

internal static void Main() {
    ref var n = ref heap(new note(), out var Ꮡn);
    setup(Ꮡn);
    discard(5);
    fmt.Println(count);
}

} // end main_package
