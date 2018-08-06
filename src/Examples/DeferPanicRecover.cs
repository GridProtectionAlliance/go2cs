// package main -- go2cs converted at 2018 August 06 15:38:06 UTC
// Original source: D:\Projects\go2cs\src\Examples\DeferPanicRecover.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            f();
            fmt.Println("Returned normally from f.");
        }

        private static void f() => func((defer, _, _) =>
        {
            defer(() =>
            {
                {
                    var r = recover();

                    if (r != nil)
                    {
                        fmt.Println("Recovered in f", r);
                    }
                }
            }());
            fmt.Println("Calling g.");
            g(0);
            fmt.Println("Returned normally from g.");
        });

        private static void g(@int i) => func((defer, panic, _) =>
        {
            if (i > 3)
            {
                fmt.Println("Panicking!");
                panic(fmt.Sprintf("%v", i));
            }
            defer(fmt.Println("Defer in g", i));
            fmt.Println("Printing in g", i);
            g(i + 1);
        });
    }
}
