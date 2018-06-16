// package main -- go2cs converted at 2018 June 16 17:46:03 UTC
// Original source: C:\Projects\go2cs\src\Examples\Methods2.go

using fmt = go.fmt_package;
using math = go.math_package;

using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct MyFloat // double
        {
        }

        public static double Abs(this MyFloat f) => func((defer, panic, recover) =>
        {
            iff<0{returnfloat64(-f)}returnfloat64(f)
        });

        private static double Main() => func((defer, panic, recover) =>
        {
            f:=MyFloat(-math.Sqrt2)fmt.Println(f.Abs())
        });
    }
}
