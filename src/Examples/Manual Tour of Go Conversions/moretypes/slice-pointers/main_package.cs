/*
package main

import "fmt"

func main() {
    names := [4]string{
        "John",
        "Paul",
        "George",
        "Ringo",
    }
    fmt.Println(names)

    a := names[0:2]
    b := names[1:3]
    fmt.Println(a, b)

    b[0] = "XXX"
    fmt.Println(a, b)
    fmt.Println(names)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
        var names = array(new @string[]{
            "John",
            "Paul",
            "George",
            "Ringo"
        });
        fmt.Println(names);

        var a = names[0..2];
        var b = names[1..3];
        fmt.Println(a, b);

        b[0] = "XXX";
        fmt.Println(a, b);
        fmt.Println(names);
    }
}
#endregion
// Note that native comparable C# code fails to operate in same way as Go
//var names = new[]{
//    "John",
//    "Paul",
//    "George",
//    "Ringo"
//};
//fmt.Println($"[{string.Join(" ", names)}]");

//var a = names[0..2];
//var b = names[1..3];
//fmt.Println(a, b);

//b[0] = "XXX";
//fmt.Println(a, b);
//fmt.Println(names);