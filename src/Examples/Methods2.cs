// package main -- go2cs converted at 2018 August 06 15:38:07 UTC
// Original source: D:\Projects\go2cs\src\Examples\Methods2.go
using fmt = go.fmt_package;
using math = go.math_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct MyFloat // : float64
        {
        }

        public static float64 Abs(this MyFloat f)
        {
            if (f < 0)
            {
                return float64(-f);
            }
            return float64(f);
        }

        private static void Main()
        {
            var f = MyFloat(-math.Sqrt2);
            fmt.Println(f.Abs());
        }
    }
}
