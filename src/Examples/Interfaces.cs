// package main -- go2cs converted at 2018 August 10 20:17:57 UTC
// Original source: D:\Projects\go2cs\src\Examples\Interfaces.go
using fmt = go.fmt_package;
using math = go.math_package;
using static go.builtin;
using System.ComponentModel;

namespace go
{
    public static partial class main_package
    {
        public partial interface Abser
        {
            float64 Abs();
        }

        private static void Main()
        {
            Abser a;
            var f = MyFloat(-math.Sqrt2);
            var v = Vertex{3,4};

            a = f; // a MyFloat implements Abser
            a = ref v; // a *Vertex implements Abser

            fmt.Println(a.Abs());
        }

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

        public partial struct Vertex
        {
            [Description("\"X\" Description")]
            public float64 X; /* X Comment */
            [Description("`Y` Description")]
            public float64 Y; // Y Comment
        }

        private static float64 Abs(this ref Vertex v)
        {
            return math.Sqrt(v.X * v.X + v.Y * v.Y);
        }
    }
}
