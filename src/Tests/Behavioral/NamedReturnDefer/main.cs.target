namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint /*x*/ incr() {
    nint x = default!;
    func((defer, _) => {
    defer(() => {
        x++;
    });
    x = 5;
    return;
});
    return x;
}

internal static nint /*x*/ incrBare() {
    nint x = default!;
    func((defer, _) => {
    defer(() => {
        x += 10;
    });
    x = 1;
    return;
});
    return x;
}

internal static (nint a, nint b) swapAndBump() {
    nint a = default!;
    nint b = default!;
    func((defer, _) => {
    defer(() => {
        (a, b) = (b, a);
        a += 100;
    });
    a = 1;
    b = 2;
    return;
});
    return (a, b);
}

internal static (nint code, @string msg) guarded(bool boom) {
    nint code = default!;
    @string msg = default!;
    func((defer, recover) => {
    defer(() => {
        {
            var r = recover(); if (r != default!) {
                code = -1;
                msg = "recovered"u8;
            }
        }
    });
    if (boom) {
        throw panic("kaboom");
    }
    code = 0;
    msg = "ok"u8;
    return;
});
    return (code, msg);
}

internal static void Main() {
    fmt.Println(incr());
    fmt.Println(incrBare());
    var (a, b) = swapAndBump();
    fmt.Println(a, b);
    var (c1, m1) = guarded(false);
    fmt.Println(c1, m1);
    var (c2, m2) = guarded(true);
    fmt.Println(c2, m2);
}

} // end main_package
