// package main -- go2cs converted at 2018 July 16 19:42:07 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\TypeSwitch.go

using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial interface I
        {
            GoString m();
        }


        public partial struct T : I
        {
            public GoString name;
            public I I;
        }


        private static GoString m(this T @ref)
        {
            return @ref.name;
        }

        public partial struct S
        {
            public GoString name;
        }


        private static GoString m(this S @ref)
        {
            return "Am I an I?";
        }

        private static void Main()
        {
            EmptyInterface x = 7;          // x has dynamic type int and value 7
            var i = x.TypeAssert<long>();                   // i has type int and value 7
            fmt.Println(i);

            T y;
            y.name = "Me";

            f(y);

            EmptyInterface s = S{"you"};

            Switch(s)
                .Case(typeof(I))(() =>
                {

                    fmt.Println("S is an I!!");
                })
                .Case(typeof(NilType), typeof(long))(() =>
                {

                    fmt.Println("S is nil or an int");
                })
                .Default(() =>
                {

                    fmt.Println("S is not an I");
                })
;        }

        private static void f(I y)
        {

            //s := y.(string)        // illegal: string does not implement I (missing method m)
            //r := y.(io.Reader)     // r has type io.Reader and the dynamic type of y must implement both I and io.Reader
            fmt.Println(y.m());
        }
    }
}
