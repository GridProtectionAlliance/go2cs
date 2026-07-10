namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nint")] partial struct reply;

internal static readonly reply statusOK = 0x00;

internal static @string describe(reply code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == statusOK) {
        return "ok"u8;
    }
    if (exprᴛ1 == (reply)(0x01)) {
        return "general failure"u8;
    }
    if (exprᴛ1 == (reply)(0x02) || exprᴛ1 == (reply)(0x03)) {
        return "blocked"u8;
    }
    { /* default: */
        return "unknown"u8;
    }

}

internal static @string kind(reply code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == (reply)(0x00)) {
        return "zero"u8;
    }
    if (exprᴛ1 == (reply)(0x04)) {
        return "four"u8;
    }

    return "many"u8;
}

internal static void Main() {
    for (nint i = 0; i <= 4; i++) {
        fmt.Println(describe(((reply)i)), kind(((reply)i)));
    }
}

} // end main_package
