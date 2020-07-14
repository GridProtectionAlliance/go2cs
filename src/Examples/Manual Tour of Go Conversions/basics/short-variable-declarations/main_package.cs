/*
package main

import "fmt"

func main() {
    var i, j int = 1, 2
    k := 3
    c, python, java := true, false, "no!"

    fmt.Println(i, j, k, c, python, java)
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static void Main(string[] args) {
        int i = 1, j = 2;
        var k = 3;
        var c = true; var python = false; var java = "no!";

        fmt.Println(i, j, k, c, python, java);
    }
}
#endregion