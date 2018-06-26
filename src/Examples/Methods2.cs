// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\Methods2.go

using fmt = go.fmt_package;
using math = go.math_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {

        public static double Abs(this MyFloat f)
        {
            iff<0{returnfloat64(-f)}returnfloat64(f)
        }

        private static void Main()
        {
            f:=MyFloat(-math.Sqrt2)fmt.Println(f.Abs())
        }
    }
}
