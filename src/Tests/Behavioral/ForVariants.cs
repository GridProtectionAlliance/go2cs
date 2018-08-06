// package main -- go2cs converted at 2018 August 06 19:54:35 UTC
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
                f(i);
            }

            i = 0;

            while(i < 10)
            {
                f(i);
            }

            {
                var i = 0;

                while(i < 10)
                {
                    f(i);
                }
            }
        }

        private static void f(@int y)
        {
            fmt.Println(y);
        }
    }
}
