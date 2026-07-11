/*
package main

import "fmt"

func main() {
    var i interface{}
    describe(i)

    i = 42
    describe(i)

    i = "hello"
    describe(i)
}

func describe(i interface{}) {
    fmt.Printf("(%v, %T)\n", i, i)
}
*/
#region source
using fmt = go.fmt_package;

static partial class main_package
{
    static void Main() {
        object i = default!;
        describe(i);

        i = 42;
        describe(i);

        i = "hello";
        describe(i);
    }

    static void describe(object i) {
        fmt.Println($"({i ?? "<nil>"}, {TypeName(i)})");
    }
}
#endregion