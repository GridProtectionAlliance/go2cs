// package main -- go2cs converted at 2018 July 17 05:02:47 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\ArrayPassByValue.go

using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            @string[] a;

            a[0] = "Hello";
            a[1] = "World";

            test(a);
            fmt.Println(a[0], a[1]);
            fmt.Println();

            a[0] = "Hello";
            test2(ref a);
            fmt.Println(a[0], a[1]);
            fmt.Println();

            a[0] = "Hello";
            test3(a.slice());
            fmt.Println(a[0], a[1]);
            fmt.Println();

            var primes = [6]int{2,3,5,7,11,13};
            fmt.Println(primes);
        }

        // Arrays are passed by value (a full copy)
        private static void test(@string[] a)
        {
            fmt.Println(a[0], a[1]);
            a[0] = "Goodbye";
            fmt.Println(a[0], a[1]);
        }

        private static void test2(ref @string[] a)
        {
            fmt.Println(a[0], a[1]);
            a[0] = "Goodbye";
            fmt.Println(a[0], a[1]);
        }

        private static void test3(slice<@string> a)
        {
            fmt.Println(a[0], a[1]);
            a[0] = "Goodbye";
            fmt.Println(a[0], a[1]);
        }
    }
}
