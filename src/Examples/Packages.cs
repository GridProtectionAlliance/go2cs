// package main -- go2cs converted at 2018 June 05 23:51:43 UTC
// Original source: C:\Projects\go2cs\src\Examples\Packages.go

using fmt = go.fmt_package;
using rand = go.math.rand_package;

using static go.BuiltInFunctions;
using System;

namespace go
{
    public static unsafe partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            fmt.Println("My favorite number is",rand.Intn(10))fmt.Println("My second favorite number is",rand.Intn(10))
        });
    }
}
