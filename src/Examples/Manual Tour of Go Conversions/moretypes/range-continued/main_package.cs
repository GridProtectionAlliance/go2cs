/*
package main

import "fmt"

func main() {
	pow := make([]int, 10)
	for i := range pow {
		pow[i] = 1 << uint(i) // == 2**i
	}
	for _, value := range pow {
		fmt.Printf("%d\n", value)
	}
}*/
#region source
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
		var pow = make_slice<int>(10);
        foreach (long i in range(pow)) {
			pow[i] = 1 << (int)i; // == 2**i
		}
        foreach ((_, int value) in range(pow, WithVal)) {
            fmt.Printf("{0}\n", value);
        }
    }
}
#endregion