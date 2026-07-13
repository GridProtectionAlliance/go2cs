namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    ref var buf = ref heap(new array<uint16>(8), out var Ꮡbuf);
    buf[0] = 0x3412;
    buf[1] = 0x7856;
    var l = ~(ж<uint32>)(uintptr)(new @unsafe.Pointer(Ꮡbuf));
    fmt.Println(l);
    fmt.Println(buf[0], buf[2]);
    var a = (uintptr)new @unsafe.Pointer(Ꮡbuf);
    var b = (uintptr)new @unsafe.Pointer(Ꮡbuf);
    fmt.Println(a == b);
}

} // end main_package
