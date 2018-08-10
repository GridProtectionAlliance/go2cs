// package main -- go2cs converted at 2018 August 09 13:23:02 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\ForVariants.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {

            var i = 0;

            while(i < 10)
            {
                // Inner comment
                f(i); // Call function
                // Increment i
                i++; // Post i comment
            } // Post for comment

            fmt.Println();
            fmt.Println("i =", i);
            fmt.Println();

            i = 0;

            while(i < 10)
            {
                f(i);

                {
                    var j = 0;

                    while(j < 3)
                    {
                        f(i + j);
                        j++;
                    }
                }
                fmt.Println();
                i++;
            }

            fmt.Println("i =", i);
            fmt.Println();

            {
                var i = 0;

                while(i < 5)
                {
                    // a
                    f(i); // b
                    i++;
                }
            } //c

            fmt.Println();
            fmt.Println("i =", i);
            fmt.Println();

            while(true)
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
