// package main -- go2cs converted at 2018 August 06 15:38:06 UTC
// Original source: D:\Projects\go2cs\src\Examples\Append.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            slice<@int> s;
            printSlice(s);

            // append works on nil slices.
            s = append(s, 0);
            printSlice(s);

            // The slice grows as needed.
            s = append(s, 1);
            printSlice(s);

            // We can add more than one element at a time.
            s = append(s, 2, 3, 4);
            printSlice(s);
        }

        private static void printSlice(slice<@int> s)
        {
            fmt.Printf("len=%d cap=%d %v\n", len(s), cap(s), s);
        }
    }
}
