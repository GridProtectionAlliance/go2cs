// package main -- go2cs converted at 2018 August 06 15:38:07 UTC
// Original source: D:\Projects\go2cs\src\Examples\MethodsPointers.go
using fmt = go.fmt_package;
using math = go.math_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct Vertex
        {
            public float64 X;
            public float64 Y;
        }

        public static float64 Abs(this Vertex v)
        {
            return math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        private static void Scale(this ref Vertex v, float64 f)
        {
            v.X = v.X * f;
            v.Y = v.Y * f;
        }

        private static void Main()
        {
            var v = Vertex{3,4};
            v.Scale(10);
            fmt.Println(v.Abs());
        }
    }
}
