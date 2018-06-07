// package main -- go2cs converted at 2018 June 06 01:27:10 UTC
// Original source: C:\Projects\go2cs\src\Examples\Variadic.go

using fmt = go.fmt_package;// Here's a function that will take an arbitrary number
// of `int`s as arguments.

using static go.BuiltInFunctions;
using System;

namespace go
{
    public static unsafe partial class main_package
    {
        // Here's a function that will take an arbitrary number
        // of `int`s as arguments.
        private static void sum(params long[] nums) => func((defer, panic, recover) =>
        {
            fmt.Print(nums," ")total:=0for_,num:=rangenums{total+=num}fmt.Println(total)
        });

        private static void Main() => func((defer, panic, recover) =>
        {
            sum(1,2)sum(1,2,3)nums:=[]int{1,2,3,4}sum(nums...)
        });
    }
}
