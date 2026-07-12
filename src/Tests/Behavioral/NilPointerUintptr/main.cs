namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

[GoType] partial struct T {
    internal nint x;
}

internal static void Main() {
    ж<T> p = default!;
    fmt.Println((uintptr)new @unsafe.Pointer(p) == 0);
    var q = Ꮡ(new T(x: 5));
    fmt.Println((uintptr)new @unsafe.Pointer(q) != 0);
    fmt.Println((~q).x);
}

} // end main_package
