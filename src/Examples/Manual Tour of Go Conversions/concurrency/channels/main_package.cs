/*
package main

import "fmt"

func sum(s []int, c chan int) {
    sum := 0
    for _, v := range s {
        sum += v
    }
    c <- sum // send sum to c
}

func main() {
    s := []int{7, 2, 8, -9, 4, 0}

    c := make(chan int)
    go sum(s[:len(s)/2], c)
    go sum(s[len(s)/2:], c)
    x, y := <-c, <-c // receive from c

    fmt.Println(x, y, x+y)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void sum(slice<int> s, ref channel<int> c) {
        var sum = 0;
        foreach ((long, int) _tuple in s) {
            var (_, v) = _tuple;
            sum += v;
        }
        c.Send(sum); // send sum to c
    }

    static void Main() {
        var s = slice(new[] { 7, 2, 8, -9, 4, 0 });

        var c = make_channel<int>();
        go_(() => sum(s[..(len(s)/2)], ref c));
        go_(() => sum(s[(len(s)/2)..], ref c));

        var x = c.Receive(); // receive from c
        var y = c.Receive();

        fmt.Println(x, y, x+y);
    }
}
#endregion