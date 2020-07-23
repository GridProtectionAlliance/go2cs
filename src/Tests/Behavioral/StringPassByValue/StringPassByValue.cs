// package main -- go2cs converted at 2018 August 14 00:22:20 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StringPassByValue.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            @string a;

            a = "Hello World";

            test(a);
            fmt.Println(a);
            fmt.Println();

            a = "Hello World";
            test2(ref a);
            fmt.Println(a);
        }

        private static void test(@string a)
        {
            fmt.Println(a);
            a = "Goodbye World";
            fmt.Println(a);
        }

        private static void test2(ref @string a)
        {
            fmt.Println(a.Deref);
            a.Deref = "Goodbye World";
            fmt.Println(a.Deref);
        }
    }
}
