// package main -- go2cs converted at 2018 June 16 17:46:01 UTC
// Original source: C:\Projects\go2cs\src\Examples\Append.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            vars[]intprintSlice(s)s=append(s,0)printSlice(s)s=append(s,1)printSlice(s)s=append(s,2,3,4)printSlice(s)
        });

        private static void printSlice(Slice<long> s) => func((defer, panic, recover) =>
        {
            fmt.Printf("len=%d cap=%d %v\n",len(s),cap(s),s)
        });
    }
}
