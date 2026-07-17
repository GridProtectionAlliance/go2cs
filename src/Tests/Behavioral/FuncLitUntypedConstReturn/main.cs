namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt maxRune = /* '\U0010FFFF' */ 1114111;

internal static readonly UntypedInt runeSelf = 0x80;

internal static readonly UntypedInt bigConst = /* 1 << 40 */ 1099511627776;

internal static rune apply(Func<rune, rune> f, rune r) {
    return f(r);
}

internal static void Main() {
    var maxFn = rune (rune _) => maxRune;
    fmt.Println(apply(maxFn, (rune)'a'));
    var encode = rune (rune r) => {
        if (r == runeSelf) {
            return maxRune;
        }
        if (r == maxRune) {
            return runeSelf;
        }
        return r;
    };
    fmt.Println(apply(encode, runeSelf));
    fmt.Println(apply(encode, maxRune));
    fmt.Println(apply(encode, (rune)'x'));
    var pick = int64 (bool neg) => {
        if (neg) {
            return -1;
        }
        return bigConst;
    };
    fmt.Println(pick(false), pick(true));
    var shrink = (rune r) => (rune)'a';
    fmt.Println(apply(shrink, maxRune));
    fmt.Println(apply((rune _) => maxRune, (rune)'b'));
}

} // end main_package
