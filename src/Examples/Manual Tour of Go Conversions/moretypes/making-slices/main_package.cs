/*
package main

import "fmt"

func main() {
	a := make([]int, 5)
	printSlice("a", a)

	b := make([]int, 0, 5)
	printSlice("b", b)

	c := b[:2]
	printSlice("c", c)

	d := c[2:5]
	printSlice("d", d)
}

func printSlice(s string, x []int) {
	fmt.Printf("%s len=%d cap=%d %v\n",
		s, len(x), cap(x), x)
}
*/
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
		var a = make_slice<int>(5);
		printSlice("a", a);

		var b = make_slice<int>(0, 5);
        printSlice("b", b);

		var c = b[..2];
        printSlice("c", c);

		var d = c[2..5];
        printSlice("d", d);
    }

    static void printSlice(@string s, in slice<int> x) {
		fmt.Printf("{0} len={1} cap={2} {3}\n",
            s, len(x), cap(x), x);
    }
}
