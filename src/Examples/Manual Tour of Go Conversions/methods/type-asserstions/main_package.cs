/*
package main

import "fmt"

func main() {
    var i interface{} = "hello"

    s := i.(string)
    fmt.Println(s)

    s, ok := i.(string)
    fmt.Println(s, ok)

    f, ok := i.(float64)
    fmt.Println(f, ok)

    f = i.(float64) // panic
    fmt.Println(f)
}
*/
#region source
using fmt = go.fmt_package;
using float64 = System.Double;
using static go.builtin;

static class main_package
{
    static void Main() {
        object i = "hello";

        string s = i._<string>();
        fmt.Println(s);

        bool ok;
        (s, ok) = i._<string>(WithOK);
        fmt.Println(s, ok);

        float64 f;
        (f, ok) = i._<float64>(WithOK);
        fmt.Println(f, ok);

        f = i._<float64>(); // panic
        fmt.Println(f, ok);
    }
}
#endregion