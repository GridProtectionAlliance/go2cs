// _Switch statements_ express conditionals across many
// branches.

// package main -- go2cs converted at 2018 August 06 03:29:32 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\ExprSwitch.go
using fmt = go.fmt_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {

            // Here's a basic `switch`.
            var i = 2;
            fmt.Print("Write ", i, " as ");
            Switch(i)
            .Case(1)(() =>
            {
                fmt.Println("one");
            })
            .Case(2)(() =>
            {
                fmt.Println("two");
            })
            .Case(3)(() =>
            {
                fmt.Println("three");
            });

            // You can use commas to separate multiple expressions
            // in the same `case` statement. We use the optional
            // `default` case in this example as well.
            Switch(time.Now().Weekday())
            .Case(time.Saturday, time.Sunday)(() =>
            {
                fmt.Println("It's the weekend");
            })
            .Default(() =>
            {
                fmt.Println("It's a weekday");
            });

            // `switch` without an expression is an alternate way
            // to express if/else logic. Here we also show how the
            // `case` expressions can be non-constants.
            var t = time.Now();
            Switch()
            .Case(t.Hour() < 12)(() =>
            {
                fmt.Println("It's before noon");
            })
            .Default(() =>
            {
                fmt.Println("It's after noon");
            });

            // A type `switch` compares types instead of values.  You
            // can use this to discover the type of an interface
            // value.  In this example, the variable `t` will have the
            // type corresponding to its clause.
            var whatAmI = i =>
            {
                var t = i;

                Switch(t)
                .Case(typeof(@bool))(() =>
                {
                    fmt.Println("I'm a bool");
                })
                .Case(typeof(@int))(() =>
                {
                    fmt.Println("I'm an int");
                })
                .Default(() =>
                {
                    fmt.Printf("Don't know type %T\n", t);
                });
            };
            whatAmI(true);
            whatAmI(1);
            whatAmI("hey");
        }
    }
}
