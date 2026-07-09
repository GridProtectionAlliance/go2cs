namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static @string gather(@string prefix, params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.slice();

    nint total = 0;
    foreach (var (_, v) in vals) {
        total += v;
    }
    return fmt.Sprintf("%s:%d(%d)"u8, prefix, total, len(vals));
}

internal static void apply(Funcꓸꓸꓸ<@string, nint, @string> f) {
    fmt.Println(f("loose"u8, 1, 2, 3));
    fmt.Println(f("empty"u8));
    var nums = new nint[]{4, 5}.slice();
    fmt.Println(f("spread"u8, nums.ꓸꓸꓸ));
}

internal static void report(Actionꓸꓸꓸ<@string, any> emit) {
    emit("%s=%d"u8, "x", 7);
    emit("bare"u8);
}

internal static void Main() {
    apply(gather);
    apply((@string prefix, params ꓸꓸꓸnint valsʗp) => {
        var vals = valsʗp.slice();
        return fmt.Sprintf("%s|%d"u8, prefix, len(vals));
    });
    Funcꓸꓸꓸ<@string, nint, @string> f = default!;
    if (f == default!) {
        fmt.Println("nil func value");
    }
    f = gather;
    fmt.Println(f("var"u8, 10));
    report((@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.slice();
        fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
    });
}

} // end main_package
