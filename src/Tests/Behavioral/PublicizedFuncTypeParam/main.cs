namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] public partial struct options {
    internal nint level;
}

public delegate void Option(ж<options> _);

internal static Option withLevel(nint n) {
    return (ж<options> o) => {
        o.Value.level = n;
    };
}

internal static nint apply(params Span<main_package.Option> optsʗp) {
    var opts = optsʗp.slice();

    var o = Ꮡ(new options(nil));
    foreach (var (_, opt) in opts) {
        opt(o);
    }
    return (~o).level;
}

internal static void Main() {
    fmt.Println(apply(withLevel(5), withLevel(9)));
}

} // end main_package
