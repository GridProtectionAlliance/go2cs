// package main -- go2cs converted at 2018 June 16 17:46:03 UTC
// Original source: C:\Projects\go2cs\src\Examples\MethodsPointers.go

using fmt = go.fmt_package;
using math = go.math_package;

using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct Vertex
        {
            public double X;
            public double Y;
        }

        public static double Abs(this Vertex v) => func((defer, panic, recover) =>
        {
            returnmath.Sqrt(v.X*v.X+v.Y*v.Y)
        });

        public static double Scale(this ref Vertex _v, double f) => func(ref _v, (ref Vertex v, Defer defer, Panic panic, Recover recover) =>
        {
            v.X=v.X*fv.Y=v.Y*f
        });

        private static double Main() => func((defer, panic, recover) =>
        {
            v:=Vertex{3,4}v.Scale(10)fmt.Println(v.Abs())
        });
    }
}
