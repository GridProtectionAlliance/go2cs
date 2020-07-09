/*
package main

import "fmt"

func split(sum int) (x, y int) {
	x = sum * 4 / 9
	y = sum - x
	return
}

func main() {
	fmt.Println(split(17))
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static (int x, int y) split(int sum) {
        int x; int y; // Named results are variables in Go function
        x = sum * 4 / 9;
        y = sum - x;
        return (x, y); // This was a naked return in Go
    }

    static void Main() {
        fmt.Println(split(17));
    }
}
#endregion