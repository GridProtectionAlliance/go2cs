namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static void inc(this ref counter c) {
    c.n++;
}

[GoType] partial struct holder {
    internal partial ref counter counter { get; }
    internal @string name;
}

[GoType] partial struct slotBox {
    internal nint id;
    internal holder slot;
}

internal static void Main() {
    var b = new slotBox(id: 3);
    Ꮡ(b.slot).of(holder.Ꮡcounter).inc();
    Ꮡ(b.slot).of(holder.Ꮡcounter).inc();
    Ꮡ(b.slot).of(holder.Ꮡcounter).inc();
    fmt.Println("id:", b.id, "count:", b.slot.n, "name-empty:", b.slot.name == ""u8);
}

} // end main_package
