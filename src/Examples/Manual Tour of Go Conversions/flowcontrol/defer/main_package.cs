/*
package main

import "fmt"

func main() {
	defer fmt.Println("world")

	fmt.Println("hello")
}
*/
#region source
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() => func((defer, _, __) => {
        defer(() => { fmt.Println("world"); });

        fmt.Println("hello");
    });
}
#endregion