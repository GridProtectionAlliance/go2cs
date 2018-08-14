// package main -- go2cs converted at 2018 August 14 00:22:19 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\ForVariants.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            var i = 0;

            while (i < 10)
            {
                // Inner comment
                f(i); // Call function
                // Increment i
                i++; // Post i comment
            } // Post for comment

            fmt.Println();
            fmt.Println("i =", i);
            fmt.Println();


            for (i = 0; i < 10; i++)
            {
                f(i);

                for (var j = 0; j < 3; j++)
                {
                    f(i + j);
                }
                fmt.Println();
            }

            fmt.Println("i =", i);
            fmt.Println();

            for (var i = 0; i < 5; i++)
            {
                // a
                f(i); // b
            } //c

            fmt.Println();
            fmt.Println("i =", i);
            fmt.Println();

            while (true)
            {
                i++;
                f(i);

                if (i > 12)
                {
                    break;
                }
            }

            fmt.Println();
            fmt.Println("i =", i);
        }

        private static void f(@int y)
        {
            fmt.Print(y);
        }
    }
}
