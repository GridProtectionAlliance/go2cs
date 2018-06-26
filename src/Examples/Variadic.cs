// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\Variadic.go

using fmt = go.fmt_package;// Here's a function that will take an arbitrary number
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        // of `int`s as arguments.
        private static void sum(long nums)
        {
            fmt.Print(nums," ")total:=0for_,num:=rangenums{total+=num}fmt.Println(total)
        }

        private static void Main()
        {
            sum(1,2)sum(1,2,3)nums:=[]int{1,2,3,4}sum(nums...)
        }
    }
}
