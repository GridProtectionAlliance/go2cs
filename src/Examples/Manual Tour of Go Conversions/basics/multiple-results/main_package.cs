/*
package main

import "fmt"

func swap(x, y string) (string, string) {
    return y, x
}

func main() {
    a, b := swap("hello", "world")
    fmt.Println(a, b)
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static (string, string) swap(string x, string y) {
        return (y, x);
    }

    static void Main() {
        var (a, b) = swap("hello", "world");
        fmt.Println(a, b);
    }
}
#endregion