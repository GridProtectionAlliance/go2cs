using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            ref array<@string> a = ref heap(new array<@string>(2L), out ptr<array<@string>> _addr_a);

            a[0L] = "Hello";
            a[1L] = "World";

            test(a);
            fmt.Println(a[0L], a[1L]);
            fmt.Println();

            a[0L] = "Hello";
            test2(_addr_a);
            fmt.Println(a[0L], a[1L]);
            fmt.Println();

            a[0L] = "Hello";
            test3(a[..]);
            fmt.Println(a[0L], a[1L]);
            fmt.Println();

            array<long> primes = new array<long>(new long[] { 2, 3, 5, 7, 11, 13 });
            fmt.Println(primes);
        }

        // Arrays are passed by value (a full copy)
        private static void test(array<@string> a)
        {
            a = a.Clone();
 
            // Update to array will be local
            fmt.Println(a[0L], a[1L]);
            a[0L] = "Goodbye";
            fmt.Println(a[0L], a[1L]);
        }

        private static void test2(ptr<array<@string>> _addr_a)
        {
            ref array<@string> a = ref _addr_a.val;

            fmt.Println(a[0L], a[1L]);
            a[0L] = "Goodbye";
            fmt.Println(a[0L], a[1L]);
        }

        private static void test3(slice<@string> a)
        {
            fmt.Println(a[0L], a[1L]);
            a[0L] = "Goodbye";
            fmt.Println(a[0L], a[1L]);
        }
    }
}
