/*
package main

import (
    "fmt"
    "math"
)

func pow(x, n, lim float64) float64 {
    if v := math.Pow(x, n); v < lim {
        return v
    } else {
        fmt.Printf("%g >= %g\n", v, lim)
    }
    // can't use v here, though
    return lim
}

func main() {
    fmt.Println(
        pow(3, 2, 10),
        pow(3, 3, 20),
    )
}
*/
#region source
using fmt = go.fmt_package;
using math = System.Math;
using float64 = System.Double;

static class main_package
{
    static float64 pow(float64 x, float64 n, float64 lim) {
        {
            var v = math.Pow(x, n);
            if (v < lim) {
                return v;
            } else {
                fmt.Printf("{0} >= {1}\n", v, lim);
            }
        }
        // can't use v here, though
        return lim;
    }

    static void Main() {
        fmt.Println(
            pow(3, 2, 10),
            pow(3, 3, 20)
        );
    }
}
#endregion