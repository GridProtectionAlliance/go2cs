// package main -- go2cs converted at 2018 July 02 21:21:29 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\LambdaFunctions.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;
using System;

namespace go
{
    public static partial class main_package
    {
        public delegate GoString Stringy();

        private static GoString foo()
        {
            return"Stringy function"
        }

        private static void takesAFunction(Stringy foo)
        {
            fmt.Printf("takesAFunction: %v\n",foo())
        }

        private static Stringy returnsAFunction()
        {
            fmt.Printf("Inner stringy function\n");return"bar"
            returnfunc()string{fmt.Printf("Inner stringy function\n");return"bar"}
        }

        private static void Main()
        {
            return"anonymous stringy\n"
            takesAFunction(foo);varfStringy=returnsAFunction();f();varbazStringy=func()string{return"anonymous stringy\n"};fmt.Printf(baz());
        }
    }
}
