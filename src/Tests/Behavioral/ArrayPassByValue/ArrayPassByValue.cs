// package main -- go2cs converted at 2019 November 11 05:54:01 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\ArrayPassByValue.go
//using fmt = go.fmt_package;

using System;
using System.Linq;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            array<@string> a = new array<@string>(2);

            a[0] = "Hello";
            a[1] = "World";

            test(a.Clone());
            Console.WriteLine("{0}, {1}", a[0], a[1]);
            Console.WriteLine();

            a[0] = "Hello";
            test2(ref a);
            Console.WriteLine("{0}, {1}", a[0], a[1]);
            Console.WriteLine();

            a[0] = "Hello";
            test3(a.slice());
            Console.WriteLine("{0}, {1}", a[0], a[1]);
            Console.WriteLine();

            int[] primes = {2,3,5,7,11,13};
            Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", primes.Cast<object>().ToArray());

            Console.ReadLine();
        }

        // Arrays are passed by value (a full copy)
        private static void test(array<@string> a)
        { 
            // Update to array will be local
            Console.WriteLine("{0}, {1}", a[0], a[1]);
            a[0] = "Goodbye";
            Console.WriteLine("{0}, {1}", a[0], a[1]);
        }

        private static void test2(ref array<@string> a)
        {
            Console.WriteLine("{0}, {1}", a[0], a[1]);
            a[0] = "Goodbye";
            Console.WriteLine("{0}, {1}", a[0], a[1]);
        }

        private static void test3(slice<@string> a)
        {
            Console.WriteLine("{0}, {1}", a[0], a[1]);
            a[0] = "Goodbye2";
            Console.WriteLine("{0}, {1}", a[0], a[1]);
        }
    }
}
