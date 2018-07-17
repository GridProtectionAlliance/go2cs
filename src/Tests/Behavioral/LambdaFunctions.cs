// package main -- go2cs converted at 2018 July 17 05:02:48 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\LambdaFunctions.go

using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public delegate @string Stringy();

        private static @string foo()
        {
            return "Stringy function";
        }

        private static void takesAFunction(Stringy foo)
        {
            fmt.Printf("takesAFunction I: %v\n", foo());
        }

        private static Stringy returnsAFunction()
        {
            return () =>
            {
                fmt.Printf("Inner stringy function\n");
                return "bar"; // have to return a string to be stringy
            };
        }

        private static void Main()
        {
            takesAFunction(foo);
            Stringy f = returnsAFunction();
            f();
            Stringy baz = () => "anonymous stringy\n";
            fmt.Printf(baz());
        }
    }
}
