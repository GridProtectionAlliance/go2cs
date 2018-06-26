// package main -- go2cs converted at 2018 June 26 19:23:08 UTC
// Original source: D:\Projects\go2cs\src\Examples\Append.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            vars[]intprintSlice(s)s=append(s,0)printSlice(s)s=append(s,1)printSlice(s)s=append(s,2,3,4)printSlice(s)
        }

        private static void printSlice(Slice<long> s)
        {
            fmt.Printf("len=%d cap=%d %v\n",len(s),cap(s),s)
        }
    }
}
