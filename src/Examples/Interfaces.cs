// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\Interfaces.go

using fmt = go.fmt_package;
using math = go.math_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial interface Abser
        {
            double Abs();
        }

        private static void Main()
        {
            varaAbserf:=MyFloat(-math.Sqrt2)v:=Vertex{3,4}a=fa=&vfmt.Println(a.Abs())
        }


        public static double Abs(this MyFloat f)
        {
            iff<0{returnfloat64(-f)}returnfloat64(f)
        }


        public static double Abs(this ref Vertex v)
        {
            returnmath.Sqrt(v.X*v.X+v.Y*v.Y)
        }
    }
}
