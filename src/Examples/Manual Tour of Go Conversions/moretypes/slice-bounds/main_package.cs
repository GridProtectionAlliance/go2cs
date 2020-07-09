/*
package main

import "fmt"

func main() {
	s := []int{2, 3, 5, 7, 11, 13}

	s = s[1:4]
	fmt.Println(s)

	s = s[:2]
	fmt.Println(s)

	s = s[1:]
	fmt.Println(s)
}
*/
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
        var s = slice(new[]{2, 3, 5, 7, 11, 13});

		s = s[1..4];
        fmt.Println(s);

		s = s[..2];
        fmt.Println(s);
    
        s = s[1..];
        fmt.Println(s);
    }
}