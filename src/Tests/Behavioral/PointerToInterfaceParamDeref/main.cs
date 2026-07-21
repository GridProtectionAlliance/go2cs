namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void handle(@string label, ж<error> Ꮡerr) => func((defer, recover) => {
    ref var err = ref Ꮡerr.ValueSlot;

    {
        var e = recover(); if (e != default!) {
            var prev = err;
            throw panic(fmt.Sprintf("wrapped %s: %v (prev=%v)"u8, label, e, prev));
        }
    }
});

internal static error /*err*/ doit() {
    error err = default!;
    func((defer, recover) => {
        deferǃ(handle, (@string)"doit", Ꮡ(err), defer);
        throw panic("boom");
    });
    return err;
}

internal static void Main() => func((defer, recover) => {
    defer(() => {
        {
            var e = recover(); if (e != default!) {
                fmt.Println(e);
            }
        }
    });
    doit();
});

} // end main_package
