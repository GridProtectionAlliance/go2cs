// package main -- go2cs converted at 2018 July 06 21:24:28 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\LambdaFunctions.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public delegate GoString Stringy();

        private static GoString foo()
        {
            return ;
        }

        private static void takesAFunction(Stringy foo)
        {
            fmt.Printf("takesAFunction \111: %v\n",foo());
        }

        private static Stringy returnsAFunction()
        {
            return () =>
            {
                fmt.Printf("Inner stringy function\n");
                return ; // have to return a string to be stringy
            };
        }

        private static void Main()
        {
            takesAFunction(foo);
            f();
            () =>
            {
                return ;
            }            fmt.Printf(baz());
        }
    }
}
