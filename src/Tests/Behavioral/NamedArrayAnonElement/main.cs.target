namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct semaRoot {
    internal nint n;
}

[GoType("dyn")] partial struct semTableᴛ1 {
    internal semaRoot root;
    internal array<byte> pad = new(40);
}

[GoType("[4]semTableᴛ1")] partial struct semTable;

[GoRecv] internal static ж<semaRoot> rootFor(this ref semTable t, nint i) {
    return Ꮡ(t.Value[i]).of(semTableᴛ1.Ꮡroot);
}

internal static void Main() {
    fmt.Println("named-array anon element compiles");
}

} // end main_package
