/*
package main

import "fmt"

func main() {
    ch := make(chan int, 2)
    ch <- 1
    ch <- 2
    fmt.Println(<-ch)
    fmt.Println(<-ch)
}
*/
#region source
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
        var ch = make_channel<int>(2);
        ch.Send(1);
        ch.Send(2);
        fmt.Println(ch.Receive());
        fmt.Println(ch.Receive());
    }
}
#endregion
// Stylistically, with some operator overload magic, this would work:
//  const System.Action __ = default;
//  _= 1 >- ch;
//  _= 2 >- ch;
//  fmt.Println(__ <- ch);
//  fmt.Println(__ <- ch);
