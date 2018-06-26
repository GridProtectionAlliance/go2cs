// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\Packages.go

using fmt = go.fmt_package;
using rand = go.math.rand_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            fmt.Println("My favorite number is",rand.Intn(10))fmt.Println("My second favorite number is",rand.Intn(10))
        }
    }
}
