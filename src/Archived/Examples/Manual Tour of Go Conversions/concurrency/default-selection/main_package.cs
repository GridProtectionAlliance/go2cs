/*
package main

import (
    "fmt"
    "time"
)

func main() {
    tick := time.Tick(100 * time.Millisecond)
    boom := time.After(500 * time.Millisecond)
    for {
        select {
        case <-tick:
            fmt.Println("tick.")
        case <-boom:
            fmt.Println("BOOM!")
            return
        default:
            fmt.Println("    .")
            time.Sleep(50 * time.Millisecond)
        }
    }
}
*/
#region source
using fmt = go.fmt_package;
using time = go.time_package;

static class main_package
{
    static void Main() {
        var tick = time.Tick(100 * time.Millisecond);
        var boom = time.After(500 * time.Millisecond);
        while (true) {
            if (tick.Received(out _)) {
                fmt.Println("tick.");
            }
            else if (boom.Received(out _)) {
                fmt.Println("BOOM!");
                return;
            }
            else {
                fmt.Println("    .");
                time.Sleep(50 * time.Millisecond);
            }
        }
    }
}
#endregion