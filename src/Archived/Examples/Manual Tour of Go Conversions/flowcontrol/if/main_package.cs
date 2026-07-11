/*
package main

import (
    "fmt"
    "math"
)

func sqrt(x float64) string {
    if x < 0 {
        return sqrt(-x) + "i"
    }
    return fmt.Sprint(math.Sqrt(x))
}

func main() {
    fmt.Println(sqrt(2), sqrt(-4))
}
*/
#region source
using fmt = go.fmt_package;
using math = System.Math;
using float64 = System.Double;

static class main_package
{
    static string sqrt(float64 x)  {
        if (x < 0) {
            return sqrt(-x) + "i";
        }
        return fmt.Sprintf("{0}", math.Sqrt(x));
    }

    static void Main() {
        fmt.Println(sqrt(2), sqrt(-4));
    }
}
#endregion