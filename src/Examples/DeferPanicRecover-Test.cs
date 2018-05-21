using static goutil.BuiltInFunctions;
using goutil;
using System;
using fmt = fmt_package;
using math = math_package;

public static partial class main_package
{
    private static void main()
    {
        f();
        Console.WriteLine("Returned normally from f.");
    }

    private static void f() => func((defer, panic, recover) =>
    {
        defer(() =>
        {
            {
                var r = recover();

                if (r != nil)
                    Console.WriteLine($"Recovered in f {r}");
            }
        });

        Console.WriteLine("Calling g.");
        g(0);
        Console.WriteLine("Returned normally from g.");
    });

    private static void g(int i) => func((defer, panic, recover) =>
    {
        //int err;

        if (i > 3)
        {
            Console.WriteLine("Panicking!");
            panic($"{i}");
        }

        defer(() => Console.WriteLine($"Defer in g {i}"));
        Console.WriteLine($"Printing in g {i}");
        g(i + 1);
    });
}