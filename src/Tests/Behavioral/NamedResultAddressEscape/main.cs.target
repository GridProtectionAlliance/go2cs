namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void setErr(ж<error> Ꮡerr) {
    ref var err = ref Ꮡerr.ValueSlot;

    err = fmt.Errorf("written via pointer"u8);
}

internal static void addOne(ж<nint> Ꮡn) {
    ref var n = ref Ꮡn.Value;

    n++;
}

internal static void handlePanic(ж<error> Ꮡerr) => func((defer, recover) => {
    ref var err = ref Ꮡerr.ValueSlot;

    {
        var e = recover(); if (e != default!) {
            err = fmt.Errorf("recovered: %v"u8, e);
        }
    }
});

internal static error /*err*/ errResult(bool fail) {
    heap<error>(out var Ꮡerr);
    func((defer, recover) => {
    ref var err = ref Ꮡerr.ValueSlot;

        deferǃ(setErr, Ꮡerr, defer);
        if (fail) {
            err = fmt.Errorf("original"u8); return;
        }
        err = default!;
    });
    return Ꮡerr.ValueSlot;
}

internal static nint /*n*/ intResult() {
    heap<nint>(out var Ꮡn);
    func((defer, recover) => {
    ref var n = ref Ꮡn.Value;

        deferǃ(addOne, Ꮡn, defer);
        n = 5;
    });
    return Ꮡn.Value;
}

internal static error /*err*/ recoverResult() {
    heap<error>(out var Ꮡerr);
    func((defer, recover) => {
    ref var err = ref Ꮡerr.ValueSlot;

        deferǃ(handlePanic, Ꮡerr, defer);
        throw panic("boom");
    });
    return Ꮡerr.ValueSlot;
}

internal static void Main() {
    fmt.Println(errResult(false));
    fmt.Println(errResult(true));
    fmt.Println(intResult());
    fmt.Println(recoverResult());
}

} // end main_package
