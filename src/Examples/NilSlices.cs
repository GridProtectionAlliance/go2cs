// package main -- go2cs converted at 2018 August 06 15:38:07 UTC
// Original source: D:\Projects\go2cs\src\Examples\NilSlices.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            slice<@int> s;
            fmt.Println(s, len(s), cap(s));
            if (s == nil)
            {
                fmt.Println("nil!");
            }        }
    }
}
