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

internal static uintptr maskAddr(uintptr i) {
    return (uintptr)(i & (uintptr)1099511627775UL);
}

internal static void Main() {
    fmt.Println(((uint64)low(11)));
    fmt.Println(((uint64)align(11)));
    fmt.Println(((uint64)align(27)));
    uintptr addr = 1;
    addr <<= (int)(47);
    addr |= (uintptr)(43981);
    fmt.Println(((uint64)maskAddr(addr)));
}

} // end main_package
