namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[]byte")] partial struct Buf;

internal static @string fillVia(ж<slice<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.Value;

    var p = Ꮡ(new Buf(@out));
    p.ValueSlot = append(p.ValueSlot, (byte)((rune)'A'), (byte)((rune)'B'), (byte)((rune)'C'));
    return ((@string)(slice<byte>)p.ValueSlot);
}

internal static void Main() {
    ref var b = ref heap<slice<byte>>(out var Ꮡb);
    b = new slice<byte>(0, 8);
    var p = Ꮡ(new Buf(b));
    p.ValueSlot = append(p.ValueSlot, (byte)((rune)'h'), (byte)((rune)'i'));
    p.ValueSlot = append(p.ValueSlot, (byte)((rune)'!'));
    fmt.Println(((@string)(slice<byte>)p.ValueSlot));
    fmt.Println(len(p.ValueSlot));
    var makeBuf = () => {
        ref var s = ref heap<slice<byte>>(out var Ꮡs);
        s = new slice<byte>(0, 4);
        return Ꮡ(new Buf(s));
    };
    var q = makeBuf();
    q.ValueSlot = append(q.ValueSlot, (byte)((rune)'x'), (byte)((rune)'y'));
    fmt.Println(((@string)(slice<byte>)q.ValueSlot));
    ref var src = ref heap<slice<byte>>(out var Ꮡsrc);
    src = new slice<byte>(0, 4);
    fmt.Println(fillVia(Ꮡsrc));
}

} // end main_package
