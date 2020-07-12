/*
package main

import "fmt"

func main() {
	var a [2]string
	a[0] = "Hello"
	a[1] = "World"
	fmt.Println(a[0], a[1])
	fmt.Println(a)

	primes := [6]int{2, 3, 5, 7, 11, 13}
	fmt.Println(primes)
}

*/
#region source
using go;
using static go.builtin;
using fmt = go.fmt_package;

public static class main_package
{
    static void Main() {
		var a = array<@string>(2);
		a[0] = "Hello";
		a[1] = "World";
		fmt.Println(a[0], a[1]);
		fmt.Println(a);

		var primes = array(new[]{2, 3, 5, 7, 11, 13});
        fmt.Println(primes);
    }
}
#endregion