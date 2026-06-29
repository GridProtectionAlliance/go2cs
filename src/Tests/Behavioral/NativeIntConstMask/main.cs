namespace go;

using fmt = fmt_package;

partial class main_package {

internal const uintptr shift = 3;

internal const uintptr blockSize = 16;

internal static uintptr low(uintptr i) {
    return (uintptr)(i & (uintptr)(((1 << (int)(shift))) - 1));
}

internal static uintptr align(uintptr i) {
    return (uintptr)(i & ~(uintptr)(blockSize - 1));
}

internal static void Main() {
    fmt.Println(((uint64)low(11)));
    fmt.Println(((uint64)align(11)));
    fmt.Println(((uint64)align(27)));
}

} // end main_package
