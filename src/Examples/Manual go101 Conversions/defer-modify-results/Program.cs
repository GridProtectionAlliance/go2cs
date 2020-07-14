// From https://go101.org/article/details.html#defer-modify-results
/*
package main

import "fmt"

func F() (r int) {
    defer func() {
        r = 789
    }()

    return 123 // <=> r = 123; return
}

func main() {
    fmt.Println(F()) // 789
}
*/
#region source
using static go.builtin;
using fmt = go.fmt_package;

static class main_package
{
    static int F() => func((defer, _, __) => {
        // 'r' escapes stack in defer below, so it needs heap allocation
        ref int r = ref heap(default(int), out var r__ptr).Value;

        defer(() =>{
            ref int r = ref r__ptr.Value;
            r = 789;
        });

        r = 123;
        return r__ptr; // Named result leaves stack in defer, return pointer to named result
    }).Value; // Defererence named result

    static void Main() {
        fmt.Println(F()); // 789
    }
}
#endregion
// Important conversion takeaway:
//   Defers can change named targets on the way out!!