// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\SliceLenCap.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            s:=[]int{2,3,5,7,11,13}printSlice(s)s=s[:0]printSlice(s)s=s[:4]printSlice(s)s=s[2:]printSlice(s)
        }

        private static void printSlice(Slice<long> s)
        {
            fmt.Printf("len=%d cap=%d %v\n",len(s),cap(s),s)
        }
    }
}
