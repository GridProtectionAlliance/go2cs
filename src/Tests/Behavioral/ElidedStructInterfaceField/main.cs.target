namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct pointerErr {
    internal @string msg;
}

[GoRecv] internal static @string Error(this ref pointerErr p) {
    return p.msg;
}

[GoType] partial struct valueErr {
    internal @string msg;
}

internal static @string Error(this valueErr v) {
    return v.msg;
}

[GoType("dyn")] partial struct main_cases {
    internal error err;
    internal @string want;
}

internal static void Main() {
    var cases = new main_cases[]{
        new(new pointerErrжerror(Ꮡ(new pointerErr("pointer-receiver"))), "pointer-receiver"u8),
        new(new valueErr("value-receiver"), "value-receiver"u8)
    }.slice();
    foreach (var (_, c) in cases) {
        fmt.Printf("%s | %s\n"u8, c.err.Error(), c.want);
    }
}

} // end main_package
