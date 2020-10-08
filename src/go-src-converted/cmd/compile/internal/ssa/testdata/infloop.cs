// package main -- go2cs converted at 2020 October 08 04:27:09 UTC
// Original source: C:\Go\src\cmd\compile\internal\ssa\testdata\infloop.go

using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static long sink = default;

        //go:noinline
        private static void test()
        { 
            // This is for #30167, incorrect line numbers in an infinite loop
            go_(() => () =>
            {
            }());

            while (true)
            {
            }


        }

        private static void Main()
        {
            test();
        }
    }
}
