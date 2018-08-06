// package main -- go2cs converted at 2018 August 06 15:38:06 UTC
// Original source: D:\Projects\go2cs\src\Examples\IfWithShortStatement.go
using fmt = go.fmt_package;
using math = go.math_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static float64 pow(float64 x, float64 n, float64 lim)
        {
            {
                var v = math.Pow(x, n);

                if (v < lim)
                {
                    return v;
                }            }
            return lim;
        }

        private static void Main()
        {
            fmt.Println(pow(3, 2, 10), pow(3, 3, 20));
        }
    }
}
