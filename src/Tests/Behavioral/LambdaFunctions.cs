// package main -- go2cs converted at 2018 July 12 03:35:10 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\LambdaFunctions.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public delegate GoString Stringy();

        private static GoString foo()
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
            f();
            () =>
            {
                return "anonymous stringy\n";
            }            fmt.Printf(baz());
        }
    }
}
