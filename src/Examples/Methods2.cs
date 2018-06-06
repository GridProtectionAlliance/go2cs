// package main -- go2cs converted at 2018 June 05 23:51:43 UTC
// Original source: C:\Projects\go2cs\src\Examples\Methods2.go

using fmt = go.fmt_package;
using math = go.math_package;

using static go.BuiltInFunctions;
using System;

namespace go
{
    public static unsafe partial class main_package
    {
        public partial struct MyFloat
        {
            // Redeclares Go float64 type - see "Methods2_MyFloatStructOf(float64).cs"
        }
        iff<0{returnfloat64(-f)}returnfloat64(f)

        private static double Main() => func((defer, panic, recover) =>
        {
            f:=MyFloat(-math.Sqrt2)fmt.Println(f.Abs())
        });
    }
}
