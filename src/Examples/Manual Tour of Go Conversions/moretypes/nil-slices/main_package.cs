/*
package main

import "fmt"

func main() {
	var s []int
	fmt.Println(s, len(s), cap(s))
	if s == nil {
		fmt.Println("nil!")
	}
}
*/
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
		slice<int> s = default;

		// This works too:
		//var s = new slice<int>();

        fmt.Println(s, len(s), cap(s));
        if (s == nil) {
            println("nil!");
		}
    }
}
