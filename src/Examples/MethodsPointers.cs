// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\MethodsPointers.go

using fmt = go.fmt_package;
using math = go.math_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {

        public static double Abs(this Vertex v)
        {
            returnmath.Sqrt(v.X*v.X+v.Y*v.Y)
        }

        public static void Scale(this ref Vertex v, double f)
        {
            v.X=v.X*fv.Y=v.Y*f
        }

        private static void Main()
        {
            v:=Vertex{3,4}v.Scale(10)fmt.Println(v.Abs())
        }
    }
}
