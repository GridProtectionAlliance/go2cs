namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt writing = 4;

[GoType] partial struct hmap {
    internal uint8 flags;
}

internal static void Main() {
    var h = Ꮡ(new hmap(flags: 7));
    h.Value.flags &= unchecked((uint8)~writing);
    h.Value.flags |= 8;
    fmt.Println((~h).flags);
    uint16 u = 0xFFFF;
    u &= unchecked((uint16)~(uint16)(0x0F0));
    fmt.Println(u);
    uint32 x = 0xFFFFFFFFU;
    x &= unchecked((uint32)~(uint32)(0xFF));
    fmt.Println(x);
    nint i = 0xFF;
    i &= ~(nint)(0x0F);
    fmt.Println(i);
}

} // end main_package
