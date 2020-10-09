﻿/*
package main

import "fmt"

func fibonacci(c, quit chan int) {
    x, y := 0, 1
    for {
        select {
        case c <- x:
            x, y = y, x+y
        case <-quit:
            fmt.Println("quit")
            return
        }
    }
}

func main() {
    c := make(chan int)
    quit := make(chan int)
    go func() {
        for i := 0; i < 10; i++ {
            fmt.Println(<-c)
        }
        quit <- 0
    }()
    fibonacci(c, quit)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void fibonacci(channel<int> c, channel<int> quit) {
        int x = 0, y = 1;

        while (true) {
            if (c.Sent(x)) {
                var _y1 = x + y;
                x = y;
                y = _y1;
            }
            else if (quit.Received(out _)) {
                fmt.Println("quit");
                return;
            }
        }
    }

    static void Main() {
        // 'c' and 'quit' escape stack in goroutine below, so we use pointers
        ref var c = ref heap(make_channel<int>(), out var c__ptr);
        ref var quit = ref heap(make_channel<int>(), out var quit__ptr);
        go_(() => {
            ref var c = ref c__ptr.val;
            ref var quit = ref quit__ptr.val;

            for (int i = 0; i < 10; i++) {
                fmt.Println(c.Receive());
            }

            quit.Send(0);
        });

        fibonacci(c, quit);
    }
}
#endregion
// Technically, because of channel's current design, it doesn't actually require
// heap allocation before leaving the stack - but to be consistent, the procedure
// is followed.

// Go detects "deadlock" behavior when nothing is ready, currently this
// code will spin. May need to implement a similar behavior.

// This also works:
//switch (true) {
//    case true when c.SendIsReady:
//        c.Send(x);
//        var _y1 = x + y;
//        x = y;
//        y = _y1;
//        break;
//    case true when quit.ReceiveIsReady:
//        quit.Receive();
//        fmt.Println("quit");
//        return;

// With extra wait handles per channel, something like
// this would work too, but it is more overhead:

//switch (new[] { c.Sending, quit.Receiving }.Select())
//{
//    case 0:
//        c.Send(x);
//        var _y1 = x + y;
//        x = y;
//        y = _y1;
//        break;
//    case 1:
//        quit.Receive();
//        fmt.Println("quit");
//        return;
//}
