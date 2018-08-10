// package main -- go2cs converted at 2018 August 10 20:17:57 UTC
// Original source: D:\Projects\go2cs\src\Examples\StructPromoted.go
using fmt = go.fmt_package;
using static go.builtin;
using System.ComponentModel;

namespace go
{
    public static partial class main_package
    {
        public partial struct DoDad
        {
            public @int I;
            public @string O;
            public @string[][] a;
        }

        public partial struct Vertex
        {
            [Description("Hi")]
            public @int X;
            [Description("Hi")]
            public @int Y;
            public ref DoDad DoDad => ref DoDad_val;
        }

        private static void Main()
        {
            var v = Vertex{1,2,DoDad{12,"Hello",[2][2]string{{"one","two"},{"three","four"}}}};
            v.X = 4;
            v.DoDad.O = "Bye";
            v.a[1][0] = "another";
            fmt.Println(v.X, v.O, v.a[1][0]);
        }
    }
}
