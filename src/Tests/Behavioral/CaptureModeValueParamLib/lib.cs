namespace go;

using fmt = fmt_package;

partial class CaptureModeValueParamLib_package {

[GoType] partial struct Config {
    public nint Indent;
    internal @string trace;
}

internal static (@string @out, error err) fprint(this ж<Config> Ꮡcfg, @string label) {
    @string @out = default!;
    error err = default!;
    func((defer, recover) => {
    ref var cfg = ref Ꮡcfg.Value;

        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    err = fmt.Errorf("panic: %v"u8, e);
                }
            }
        });
        cfg.trace = fmt.Sprintf("%s|%s"u8, cfg.trace, label);
        (@out, err) = (fmt.Sprintf("%s@%d"u8, label, cfg.Indent), default!);
    });
    return (@out, err);
}

public static (@string, error) Fprint(this ж<Config> Ꮡcfg, @string label) {
    return Ꮡcfg.fprint(label);
}

[GoRecv] public static @string Trace(this ref Config cfg) {
    return cfg.trace;
}

} // end CaptureModeValueParamLib_package
