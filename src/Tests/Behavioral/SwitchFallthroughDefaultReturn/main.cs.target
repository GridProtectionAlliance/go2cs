namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string classify(nint n) {
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 0) { matchᴛ1 = true;
        return "zero"u8;
    }
    if (exprᴛ1 is 1) { matchᴛ1 = true;
        return "one"u8;
    }
    if (exprᴛ1 is 2) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return "many"u8;
    }
    return default!;

}

internal static nint nonTerminal(nint n) {
    nint acc = 0;
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 0) { matchᴛ1 = true;
        acc = 10;
    }
    else if (exprᴛ1 is 1) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        acc = 20;
    }

    return acc + n;
}

internal static nint conditionalReturn(nint n) {
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 0) { matchᴛ1 = true;
        if (n < 0) {
            return 111;
        }
    }
    if (exprᴛ1 is 1) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return n * 1000;
    }

    return 777;
}

internal static (nint r, bool ok) namedDefer(nint n) {
    nint r = default!;
    bool ok = default!;
    func((defer, recover) => {
        defer(() => {
            if (recover() != default!) {
                (r, ok) = (-1, false);
            }
        });
        var exprᴛ1 = n;
        var matchᴛ1 = false;
        if (exprᴛ1 is 0) { matchᴛ1 = true;
            (r, ok) = (100, true); return;
        }
        if (exprᴛ1 is 1) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            (r, ok) = (n * 10, true); return;
        }

    });
    return (r, ok);
}

internal static @string keepAlive(nint idle, nint interval) {
    switch (ᐧ) {
    case {} when idle < 0 && interval >= 0: {
        return "interval-only"u8;
    }
    case {} when idle < 0 && interval < 0: {
        return "none"u8;
    }
    case {} when idle >= 0 && interval >= 0: {
        break;
    }}

    return "both-or-idle"u8;
}

internal static void Main() {
    foreach (var (_, n) in new nint[]{0, 1, 2, 3}.slice()) {
        fmt.Println(classify(n));
    }
    fmt.Println(keepAlive(-1, 5), keepAlive(-1, -1), keepAlive(3, 5));
    foreach (var (_, n) in new nint[]{0, 1, 2}.slice()) {
        fmt.Println(nonTerminal(n));
    }
    foreach (var (_, n) in new nint[]{0, 1, 2}.slice()) {
        fmt.Println(conditionalReturn(n));
    }
    foreach (var (_, n) in new nint[]{0, 1, 2}.slice()) {
        var (r, ok) = namedDefer(n);
        fmt.Println(r, ok);
    }
}

} // end main_package
