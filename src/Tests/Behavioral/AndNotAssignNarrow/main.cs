namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt writing = 4;

[GoType] partial struct hmap {
    internal uint8 flags;
}

internal static void Main() {
    var h = Ꮡ(new hmap(flags: 7));
    h.val.flags &= unchecked((uint8)~writing);
    h.val.flags |= 8;
    fmt.Println((~h).flags);
    uint16 u = 65535;
    u &= unchecked((uint16)~(uint16)(240));
    fmt.Println(u);
    uint32 x = 4294967295U;
    x &= unchecked((uint32)~(uint32)(255));
    fmt.Println(x);
    nint i = 255;
    i &= ~(nint)(15);
    fmt.Println(i);
}

} // end main_package
