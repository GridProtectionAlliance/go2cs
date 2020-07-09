/*
package main

import "fmt"

func main() {
	sum := 1
	for ; sum < 1000; {
		sum += sum
	}
	fmt.Println(sum)
}
*/
using fmt = go.fmt_package;

static class main_package
{
    static void Main() {
		var sum = 1;
        while (sum < 1000) {
            sum += sum;
        }
        fmt.Println(sum);
    }
}
