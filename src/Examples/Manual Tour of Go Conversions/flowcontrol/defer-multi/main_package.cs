/*
package main

import "fmt"

func main() {
	fmt.Println("counting")

	for i := 0; i < 10; i++ {
		defer fmt.Println(i)
	}

	fmt.Println("done")
}
*/
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() => func((defer, __, _) =>
    {
        fmt.Println("counting");

        for (int i = 0; i < 10; i++) {
            // i escapes stack in defer below, so we need a pointer
            var i__ptr = ptr(i);
            defer(() => {
                ref var i = ref i__ptr.Value;
                fmt.Println(i);
            });
        }

        fmt.Println("done");
    });
}