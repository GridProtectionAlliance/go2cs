/*
package main

import "fmt"

func main() {
	q := []int{2, 3, 5, 7, 11, 13}
	fmt.Println(q)

	r := []bool{true, false, true, true, false, true}
	fmt.Println(r)

	s := []struct {
		i int
		b bool
	}{
		{2, true},
		{3, false},
		{5, true},
		{7, true},
		{11, false},
		{13, true},
	}
	fmt.Println(s)
}
*/
using go;
using fmt = go.fmt_package;
using static go.builtin;

static partial class main_package
{
    static void Main() {
        var q = slice(new[]{2, 3, 5, 7, 11, 13});
        fmt.Println(q);

        var r = slice(new @bool[]{true, false, true, true, false, true});
        fmt.Println(r);

        var s = slice(@struct(
            /*
                i int
                b bool
            */
            new[] {
                (2, true),
                (3, false),
                (5, true),
                (7, true),
                (11, false),
                (13, true),
        }));
        fmt.Println(s);
    }
}
