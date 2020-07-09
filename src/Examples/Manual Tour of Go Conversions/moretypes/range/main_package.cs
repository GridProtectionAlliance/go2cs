/*
package main

import "fmt"

var pow = []int{1, 2, 4, 8, 16, 32, 64, 128}

func main() {
	for i, v := range pow {
		fmt.Printf("2**%d = %d\n", i, v)
	}
}
*/
using go;
using static go.builtin;
using fmt = go.fmt_package;

static class main_package
{
    // As of C# 8.0, members cannot be var
    static slice<int> pow = new[]{1, 2, 4, 8, 16, 32, 64, 128};

    static void Main() {
        foreach ((long i, int v) in range(pow, WithVal)) {
            fmt.Printf("2**{0} = {1}\n", i, v);
        }
    }
}