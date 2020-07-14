/*
package main

import (
    "fmt"
    "math"
)

type MyFloat float64

func (f MyFloat) Abs() float64 {
    if f < 0 {
        return float64(-f)
    }
    return float64(f)
}

func main() {
    f := MyFloat(-math.Sqrt2)
    fmt.Println(f.Abs())
}
*/
// The following type based using statements create a true alias,
// the trouble is the "using" directive will now be needed in every
// source code file that references MyFloat - more stuff to track...
#region source
using fmt = go.fmt_package;
using math = System.Math;
using float64 = System.Double;

using MyFloat = System.Double;

static class main_package
{
    static float64 Abs(this MyFloat f) {
        if (f < 0) {
            return (float64)(-f);
        }
        return (float64)(f);
    }

    static void Main() {
        var f = (float64)(-math.Sqrt(2));
        fmt.Println(f.Abs());
    }
}
#endregion