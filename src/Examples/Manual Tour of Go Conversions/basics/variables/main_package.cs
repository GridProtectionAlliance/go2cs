/*
package main

import "fmt"

var c, python, java bool

func main() {
	var i int
	fmt.Println(i, c, python, java)
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static bool c, python, java;

    static void Main() {
        int i = default; // Local Go variables do not require initialization
        fmt.Println(i, c, python, java);
    }
}
#endregion