namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() => func((defer, recover) => {
    func((defer, recover) => {
        fmt.Println("a");
    });
    nint x = func((defer, recover) => 6 * 7);
    fmt.Println(x);
    func((defer, recover) => {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println("recovered:", r);
                }
            }
        });
        throw panic("boom");
    });
    fmt.Println("after recover");
    nint total = 10 + func((defer, recover) => {
        nint sum = 0;
        for (nint i = 1; i <= 4; i++) {
            sum += i;
        }
        return sum;
    });
    fmt.Println(total);
    nint y = func((defer, recover) => func((defer, recover) => 5) * 2);
    fmt.Println(y);
    func((defer, recover) => {
        nint n = 7;
        fmt.Println("n =", n);
    });
    nint tri = func((defer, recover) => {
        nint a = 1;
        nint b = 2;
        nint c = 3;
        return a + b + c;
    });
    fmt.Println(tri);
    for (nint k = 0; k < 3; k++) {
        func((defer, recover) => {
            nint xΔ1 = k;
            fmt.Print(xΔ1, " ");
        });
    }
    fmt.Println();
    func((defer, recover) => {
        @string label = "scope"u8;
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(label, "recovered:", r);
                }
            }
        });
        throw panic("argboom");
    });
    fmt.Println("after arg recover");
    nint calls = 0;
    var bump = () => {
        calls++;
        return 99;
    };
    func((defer, recover) => {
        _ = bump();
        fmt.Println("ignored arg; calls =", calls);
    });
});

} // end main_package
