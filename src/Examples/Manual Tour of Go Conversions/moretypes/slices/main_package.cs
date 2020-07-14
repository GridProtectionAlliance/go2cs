/*
package main

import "fmt"

func main() {
    primes := [6]int{2, 3, 5, 7, 11, 13}

    var s []int = primes[1:4]
    fmt.Println(s)
}
*/
#region source
using static go.builtin;
using fmt = go.fmt_package;

static class main_package
{
    static void Main() {
        var primes = array(new[] { 2, 3, 5, 7, 11, 13 });

        var s = primes[1..4];
        fmt.Println(s);
    }
}
#endregion