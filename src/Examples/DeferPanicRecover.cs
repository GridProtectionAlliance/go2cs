// package main -- go2cs converted at 2018 June 06 01:27:08 UTC
// Original source: C:\Projects\go2cs\src\Examples\DeferPanicRecover.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;
using System;

namespace go
{
    public static unsafe partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            f()fmt.Println("Returned normally from f.")
        });

        private static void f() => func((defer, panic, recover) =>
        {
            ifr:=recover();r!=nil{fmt.Println("Recovered in f",r)}
            deferfunc(){ifr:=recover();r!=nil{fmt.Println("Recovered in f",r)}}()fmt.Println("Calling g.")g(0)fmt.Println("Returned normally from g.")
        });

        private static void g(long i) => func((defer, panic, recover) =>
        {
            ifi>3{fmt.Println("Panicking!")panic(fmt.Sprintf("%v",i))}deferfmt.Println("Defer in g",i)fmt.Println("Printing in g",i)g(i+1)
        });
    }
}
