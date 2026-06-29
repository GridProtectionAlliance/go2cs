namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt stateA = iota;
internal static readonly UntypedInt stateB = 1;
internal static readonly UntypedInt stateC = 2;

internal static @string classify(nint n, bool flag) {
    @string result = "none"u8;
    var exprᴛ1 = n;
    if (exprᴛ1 == stateA) {        do {
            if (flag) {
                break;
            }
            result = "a"u8;        
} while (false);
    }
    else if (exprᴛ1 == stateB) {        do {
            result = "b"u8;
            if (flag) {
                break;
            }
            result = "b-noflag"u8;        
} while (false);
    }
    else { /* default: */
        result = "other"u8;
    }

    return result;
}

internal static void Main() {
    fmt.Println(classify(stateA, true));
    fmt.Println(classify(stateA, false));
    fmt.Println(classify(stateB, true));
    fmt.Println(classify(stateB, false));
    fmt.Println(classify(stateC, false));
}

} // end main_package
