// package main -- go2cs converted at 2018 August 10 20:34:32 UTC
// Original source: D:\Projects\go2cs\src\Examples\Variadic.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Here's a function that will take an arbitrary number
        // of `int`s as arguments.
        private static void sum(@int nums)
        {
            fmt.Print(nums, " ");
            var total = 0;
            {
                {
                    total += num;
                }
            }
            fmt.Println(total);
        }

        private static void Main()
        {
            // Variadic functions can be called in the usual way
            // with individual arguments.
            sum(1, 2);
            sum(1, 2, 3);

            // If you already have multiple args in a slice,
            // apply them to a variadic function using
            // `func(slice...)` like this.
            var nums = []int{1,2,3,4};
            sum(nums);
        }
    }
}
