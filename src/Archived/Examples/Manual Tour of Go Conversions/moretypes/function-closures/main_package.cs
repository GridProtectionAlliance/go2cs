/*
package main

import "fmt"

func adder() func(int) int {
    sum := 0
    return func(x int) int {
        sum += x
        return sum
    }
}

func main() {
    pos, neg := adder(), adder()
    for i := 0; i < 10; i++ {
        fmt.Println(
            pos(i),
            neg(-2*i),
        )
    }
}
*/
#region source
using System;
using fmt = go.fmt_package;

static class main_package
{
    static Func<int, int> adder() {
        var sum = 0;
        return x => {
            sum += x;
            return sum;
        };
    }

    static void Main() {
        var pos = adder();
        var neg = adder();

        for (int i = 0; i < 10; i++) {
            fmt.Println(
                pos(i),
                neg(-2 * i)
            );
        }
    }
}
#endregion