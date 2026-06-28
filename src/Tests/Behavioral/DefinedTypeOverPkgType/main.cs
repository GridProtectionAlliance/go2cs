namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

[GoType("unsafe_package.Pointer")] partial struct stdFunction;

internal static stdFunction handler;

internal static stdFunction other;

internal static void Main() {
    handler = other;
    fmt.Println(handler == other);
}

} // end main_package
