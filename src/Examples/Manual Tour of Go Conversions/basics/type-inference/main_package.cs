/*
package main

import "fmt"

func main() {
	v := 42 // change me!
	fmt.Printf("v is of type %T\n", v)
}
*/
#region source
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
        var v = 42; // change me!
        fmt.Printf("v is of type {0}\n", GetGoTypeName(v.GetType()));
    }
}
#endregion