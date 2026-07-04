namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt stateA = iota;
internal static readonly UntypedInt stateB = 1;
internal static readonly UntypedInt stateC = 2;

internal static @string classify(nint n, bool flag) {
    @string result = "none"u8;
    var exprᴛ1 = n;
    if (exprᴛ1 == stateA) {
        do {
            if (flag) {
                break;
            }
            result = "a"u8;
        } while (false);
    }
    else if (exprᴛ1 == stateB) {
        do {
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

internal static @string pick(nint x, nint y) {
    @string @out = "none"u8;
Big:
    switch (x) {
    case 1: {
        switch (y) {
        case 2: {
            @out = "one-two"u8;
            goto break_Big;
            break;
        }}

        @out = "one-other"u8;
        break;
    }
    case 3: {
        @out = "three"u8;
        break;
    }}

    break_Big:;
    return @out;
}

internal static void Main() {
    fmt.Println(pick(1, 2), pick(1, 5), pick(3, 0));
    fmt.Println(classify(stateA, true));
    fmt.Println(classify(stateA, false));
    fmt.Println(classify(stateB, true));
    fmt.Println(classify(stateB, false));
    fmt.Println(classify(stateC, false));
}

} // end main_package
