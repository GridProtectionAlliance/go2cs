// package main -- go2cs converted at 2018 June 16 17:46:03 UTC
// Original source: C:\Projects\go2cs\src\Examples\Interfaces.go

using fmt = go.fmt_package;
using math = go.math_package;

using static go.BuiltInFunctions;
using System.ComponentModel;

namespace go
{
    public static partial class main_package
    {
        public partial interface Abser
        {
            public double Abs();
        }

        private static double Main() => func((defer, panic, recover) =>
        {
            varaAbserf:=MyFloat(-math.Sqrt2)v:=Vertex{3,4}a=fa=&vfmt.Println(a.Abs())
        });

        public partial struct MyFloat // double
        {
        }

        public static double Abs(this MyFloat f) => func((defer, panic, recover) =>
        {
            iff<0{returnfloat64(-f)}returnfloat64(f)
        });

        public partial struct Vertex
        {
            [Description("\"X\" Description")]
            public double X; /* X Comment */

            [Description("`Y` Description")]
            public double Y; // Y Comment

        }

        public static double Abs(this ref Vertex _v) => func(ref _v, (ref Vertex v, Defer defer, Panic panic, Recover recover) =>
        {
            returnmath.Sqrt(v.X*v.X+v.Y*v.Y)
        });
    }
}
