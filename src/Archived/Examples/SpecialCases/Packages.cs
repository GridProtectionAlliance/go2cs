// package main -- go2cs converted at 2018 August 10 20:17:57 UTC
// Original source: D:\Projects\go2cs\src\Examples\Packages.go
using fmt = go.fmt_package;
using rand = go.math.rand_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            fmt.Println("My favorite number is", rand.Intn(10));
            fmt.Println("My second favorite number is", rand.Intn(10));
        }
    }
}
