namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static void Main() {
    Actionꓸꓸꓸ<@string, any> fn = (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.slice();
        fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
    };
    any logf = fn;
    {
        var (fnΔ1, ok) = logf._<Actionꓸꓸꓸ<@string, any>>(ᐧ); if (ok){
            fnΔ1("value=%v flag=%v"u8, 42, true);
        } else {
            fmt.Println("no match");
        }
    }
    any notFn = "plain";
    {
        var (_, ok) = notFn._<Actionꓸꓸꓸ<@string, any>>(ᐧ); if (ok){
            fmt.Println("unexpected match");
        } else {
            fmt.Println("no match for string");
        }
    }
    any plain = (@string s) => s + "!"u8;
    {
        var (fnΔ2, ok) = plain._<Func<@string, @string>>(ᐧ); if (ok) {
            fmt.Println(fnΔ2("ok"u8));
        }
    }
}

} // end main_package
