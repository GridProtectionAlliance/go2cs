// package main -- go2cs converted at 2018 May 23 21:06:31 UTC
// Original source: D:\Projects\go2cs\src\Examples\Packages.go

using fmt = go.fmt_package;
using math/rand = go.math.rand_package;

using static goutil.BuiltInFunctions;
using goutil;
using System;

namespace go
{
    private static partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            fmt.Println("My favorite number is",rand.Intn(10))fmt.Println("My second favorite number is",rand.Intn(10))
        });
    }
}
