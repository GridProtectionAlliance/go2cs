﻿/*
package main

import (
    "fmt"
)

func fibonacci(n int, c chan int) {
    x, y := 0, 1
    for i := 0; i < n; i++ {
        c <- x
        x, y = y, x+y
    }
    close(c)
}

func main() {
    c := make(chan int, 10)
    go fibonacci(cap(c), c)
    for i := range c {
        fmt.Println(i)
    }
}
*/
#region source
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void fibonacci(long n, channel<int> c) {
        int x = 0, y = 1;
        for (int i = 0; i < n; i++) {
            c.Send(x);
            var _y1 = x + y;
            x = y;
            y = _y1;
        }
        close(c);
    }

    static void Main() {
        // 'c' escapes stack in goroutine below, so we need a pointer
        ref var c = ref heap(make_channel<int>(10), out var c__ptr);
        go_(() =>
        {
            ref var c = ref c__ptr.val;
            fibonacci(cap(c), c);
        });
        foreach (int i in c) {
            fmt.Println(i);
        }
    }
}
#endregion
