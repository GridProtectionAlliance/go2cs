namespace go;

using fmt = fmt_package;

partial class main_package {

internal static array<@string> words = new @string[]{"alpha", "beta", "gamma"}.array();

internal static readonly UntypedInt one = 1;

internal static @string classify(@string s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == words[0] || exprᴛ1 == words[2]) {
        return "ends"u8;
    }
    if (exprᴛ1 == words[one]) {
        return "middle"u8;
    }
    { /* default: */
        return "none"u8;
    }

}

internal static void Main() {
    fmt.Println(classify("alpha"u8));
    fmt.Println(classify("beta"u8));
    fmt.Println(classify("gamma"u8));
    fmt.Println(classify("delta"u8));
}

} // end main_package
