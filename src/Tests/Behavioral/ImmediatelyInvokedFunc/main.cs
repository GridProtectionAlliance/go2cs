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
});

} // end main_package
