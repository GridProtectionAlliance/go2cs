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
#region source
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() => func((defer, _, __) => {
        fmt.Println("counting");

        for (int i = 0; i < 10; i++) {
            // 'i' escapes stack in defer below, so we use a pointer
            var i__ptr = ptr(i);
            defer(() => {
                ref var i = ref i__ptr.Value;
                fmt.Println(i);
            });
        }

        fmt.Println("done");
    });
}
#endregion