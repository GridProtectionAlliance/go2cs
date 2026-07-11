/*
package main

import "fmt"

func add(x, y int) int {
    return x + y
}

func main() {
    fmt.Println(add(42, 13))
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static int add(int x, int y) {
        return x + y;
    }

    static void Main() {
        fmt.Println(add(42, 13));
    }
}
#endregion