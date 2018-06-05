// package main -- go2cs converted at 2018 May 30 19:31:16 UTC
// Original source: D:\Projects\go2cs\src\Examples\Interfaces.go

using fmt = go.fmt_package;
using math = go.math_package;

using static go.BuiltInFunctions;
using System;
using System.ComponentModel;

namespace go
{
    public static unsafe partial class main_package
    {
        public partial interface Abser
        {
            public double Abs();
        }

        private static double Main() => func((defer, panic, recover) =>
        {
            varaAbserf:=MyFloat(-math.Sqrt2)v:=Vertex{3,4}a=fa=&vfmt.Println(a.Abs())
        });

        public partial struct MyFloat
        {
            // Redeclares Go float64 type - see "Interfaces_MyFloatStructOf(float64).cs"
        }
        iff<0{returnfloat64(-f)}returnfloat64(f)

        public partial struct Vertex
        {
            [Description("\"X\" Description")]
            public double X; /* X Comment */

            [Description("`Y` Description")]
            public double Y; // Y Comment
        }
        returnmath.Sqrt(v.X*v.X+v.Y*v.Y)
    }
}
