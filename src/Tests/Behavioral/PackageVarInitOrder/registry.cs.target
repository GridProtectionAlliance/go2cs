namespace go;

partial class main_package {

[GoType] partial struct reg {
    internal slice<@string> entries;
    internal nint count;
}

internal static ж<reg> newReg() {
    return Ꮡ(new reg(nil));
}

[GoRecv] internal static @string add(this ref reg r, @string name) {
    r.entries = append(r.entries, name);
    r.count++;
    return name + "-added"u8;
}

internal static ж<reg> registry = newReg();

internal static slice<@string> names = new @string[]{"stdin", "stdout"}.slice();

} // end main_package
