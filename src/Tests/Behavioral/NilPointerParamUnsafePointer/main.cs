namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

[GoType("num:uintptr")] partial struct Handle;

internal static uintptr addrOfNamed(ж<Handle> Ꮡh) {
    return (uintptr)new @unsafe.Pointer(Ꮡh);
}

internal static uintptr addrOfPtrPtr(ж<ж<uint16>> Ꮡpp) {
    return (uintptr)new @unsafe.Pointer(Ꮡpp);
}

internal static uintptr addrOfBasic(ж<uint16> Ꮡb) {
    return (uintptr)new @unsafe.Pointer(Ꮡb);
}

internal static (uintptr, Handle) liveAlias(ж<Handle> Ꮡh) {
    ref var h = ref Ꮡh.Value;

    h = h + 7;
    return ((uintptr)new @unsafe.Pointer(Ꮡh), h);
}

internal static void Main() {
    fmt.Println("named nil  :", addrOfNamed(nil));
    fmt.Println("ptrptr nil :", addrOfPtrPtr(nil));
    fmt.Println("basic nil  :", addrOfBasic(nil));
    ref var h = ref heap(new Handle(), out var Ꮡh);
    h = 3;
    fmt.Println("named nonnil nonzero :", addrOfNamed(Ꮡh) != 0);
    ref var u = ref heap(new uint16(), out var Ꮡu);
    u = 9;
    ref var pu = ref heap<ж<uint16>>(out var Ꮡpu);
    pu = Ꮡu;
    fmt.Println("ptrptr nonnil nonzero:", addrOfPtrPtr(Ꮡpu) != 0);
    fmt.Println("basic nonnil nonzero :", addrOfBasic(Ꮡu) != 0);
    var (addr, val) = liveAlias(Ꮡh);
    fmt.Println("live alias nonzero:", addr != 0, "value:", val, "readback:", h);
}

} // end main_package
