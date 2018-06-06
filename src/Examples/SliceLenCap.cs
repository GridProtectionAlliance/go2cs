// package main -- go2cs converted at 2018 June 05 23:51:43 UTC
// Original source: C:\Projects\go2cs\src\Examples\SliceLenCap.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;
using System;

namespace go
{
    public static unsafe partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            s:=[]int{2,3,5,7,11,13}printSlice(s)s=s[:0]printSlice(s)s=s[:4]printSlice(s)s=s[2:]printSlice(s)
        });

        private static void printSlice(Slice<long> s) => func((defer, panic, recover) =>
        {
            fmt.Printf("len=%d cap=%d %v\n",len(s),cap(s),s)
        });
    }
}
