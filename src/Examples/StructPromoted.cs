// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\StructPromoted.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {


        private static void Main()
        {
            v:=Vertex{1,2,DoDad{12,"Hello",[2][2]string{{"one","two"},{"three","four"}}}}v.X=4v.DoDad.O="Bye"v.a[1][0]="another"fmt.Println(v.X,v.O,v.a[1][0])
        }
    }
}
