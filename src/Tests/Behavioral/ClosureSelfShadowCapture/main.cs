namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct span {
    internal nint largeType;
    internal nint val;
}

internal static void onstack(Action f) {
    f();
}

internal static nint run() {
    var s = Ꮡ(new span(largeType: 5, val: 100));
    nint total = 0;
    var sʗ1 = s;
    onstack(() => {
        var sΔ1 = Ꮡ(new span(largeType: (~sʗ1).largeType * 2, val: (~sʗ1).val + 1));
        total += (~sΔ1).largeType + (~sΔ1).val;
    });
    total += s.Value.val;
    return total;
}

internal static void Main() {
    fmt.Println(run());
}

} // end main_package
