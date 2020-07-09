/*
package main

import "fmt"

var i, j int = 1, 2

func main() {
	var c, python, java = true, false, "no!"
	fmt.Println(i, j, c, python, java)
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static int i = 1, j = 2;

    static void Main() {
        var c = true; var python = false; var java = "no!";
        fmt.Println(i, j, c, python, java);
    }
}
#endregion