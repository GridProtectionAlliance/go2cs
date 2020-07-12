/*
package main

import "fmt"

func main() {
	sum := 0
	for i := 0; i < 10; i++ {
		sum += i
	}
	fmt.Println(sum)
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static void Main() {
		var sum = 0;
        for (int i = 0; i < 10; i++) {
            sum += i;
        }
        fmt.Println(sum);
    }
}
#endregion