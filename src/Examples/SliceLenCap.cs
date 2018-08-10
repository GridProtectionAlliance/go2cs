// package main -- go2cs converted at 2018 August 10 20:17:57 UTC
// Original source: D:\Projects\go2cs\src\Examples\SliceLenCap.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            var s = []int{2,3,5,7,11,13};
            printSlice(s);

            // Slice the slice to give it zero length.
            s = s.slice(high:0);
            printSlice(s);

            // Extend its length.
            s = s.slice(high:4);
            printSlice(s);

            // Drop its first two values.
            s = s.slice(2);
            printSlice(s);
        }

        private static void printSlice(slice<@int> s)
        {
            fmt.Printf("len=%d cap=%d %v\n", len(s), cap(s), s);
        }
    }
}
