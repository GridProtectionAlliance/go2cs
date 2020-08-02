using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        public partial interface I
        {
            @string m();
        }

        public partial struct T : I
        {
            public @string name;
            public I I;
        }

        public static @string m(this T @ref)
        {
            return ref.name;
        };

        public partial struct S
        {
            public @string name;
        }

        public static @string m(this S @ref)
        {
            return "Am I an I?";
        };

        private static void Main()
        { 
            // A type `switch` compares types instead of values.  You
            // can use this to discover the type of an interface
            // value.  In this example, the variable `t` will have the
            // type corresponding to its clause.
            Action<object> whatAmI = i =>
            {
                var t = i;

                Switch(t)
                .Case(typeof(bool))(() =>
                {
                    fmt.Println("I'm a bool");
                })
                .Case(typeof(long))(() =>
                {
                    fmt.Println("I'm an int");
                })
                .Default(() =>
                {
                    fmt.Printf("Don't know type %T\n", t);
                });
            }
;
            whatAmI(true);
            whatAmI(1L);
            whatAmI("hey");

            object x = 7L; // x has dynamic type int and value 7
            long i = x._<long>(); // i has type int and value 7
            fmt.Println(i);

            T y;

            y.name = "Me";

            f(y);

            object s = S{"you"};

            Switch(s)
            .Case(typeof(I))(() =>
            {
                fmt.Println("S is an I!!");
            })
            .Case(typeof(long))(() =>
            {
                fmt.Println("S is nil or an int");
            })
            .Default(() =>
            {
                fmt.Println("S is not an I");
            });
        }

        private static void f(I y)
        { 
            //s := y.(string)        // illegal: string does not implement I (missing method m)
            //r := y.(io.Reader)     // r has type io.Reader and the dynamic type of y must implement both I and io.Reader

            fmt.Println(y.m());
        }
    }
}
