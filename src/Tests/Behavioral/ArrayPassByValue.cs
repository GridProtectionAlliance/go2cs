// package main -- go2cs converted at 2018 July 05 21:01:33 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\ArrayPassByValue.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            test(a);
            fmt.Println(a[0],a[1]);
            fmt.Println();
            test2(&a);
            fmt.Println(a[0],a[1]);
            fmt.Println();
            test3(a[:]);
            fmt.Println(a[0],a[1]);
            fmt.Println();
            fmt.Println(primes);
        }

        // Arrays are passed by value (a full copy)
        private static void test(GoString[2][2] a)
        {
            fmt.Println(a[0],a[1]);
            fmt.Println(a[0],a[1]);
        }

        private static void test2(ref GoString[2][2] a)
        {
            fmt.Println(a[0],a[1]);
            fmt.Println(a[0],a[1]);
        }

        private static void test3(Slice<GoString> a)
        {
            fmt.Println(a[0],a[1]);
            fmt.Println(a[0],a[1]);
        }
    }
}
