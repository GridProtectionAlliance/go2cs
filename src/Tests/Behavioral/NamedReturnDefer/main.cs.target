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
    });
    return (a, b);
}

internal static nint @double(nint n) {
    return n * 2;
}

internal static (nint @out, @string label) compute(nint x) {
    nint @out = default!;
    @string label = default!;
    func((defer, _) => {
        defer(() => {
            @out += 1000;
        });
        if (x < 0) {
            (@out, label) = (-1, "neg");
            return;
        }
        (@out, label) = (@double(x), fmt.Sprintf("v=%d"u8, x));
    });
    return (@out, label);
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
    });
    return (code, msg);
}

internal static void Main() {
    fmt.Println(incr());
    fmt.Println(incrBare());
    var (a, b) = swapAndBump();
    fmt.Println(a, b);
    var (o1, l1) = compute(3);
    fmt.Println(o1, l1);
    var (o2, l2) = compute(-5);
    fmt.Println(o2, l2);
    var (c1, m1) = guarded(false);
    fmt.Println(c1, m1);
    var (c2, m2) = guarded(true);
    fmt.Println(c2, m2);
}

} // end main_package
