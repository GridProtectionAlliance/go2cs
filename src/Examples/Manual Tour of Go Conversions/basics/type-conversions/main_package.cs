/*
package main

import (
	"fmt"
	"math"
)

func main() {
	var x, y int = 3, 4
	var f float64 = math.Sqrt(float64(x*x + y*y))
	var z uint = uint(f)
	fmt.Println(x, y, z)
}
*/
#region source
using fmt = go.fmt_package;
using math = System.Math;
using float64 = System.Double;

static class main_package
{
    static void Main() {
		int x = 3, y = 4;
		float64 f = math.Sqrt((float64)(x * x + y * y));
		uint z = (uint)f;
        fmt.Println(x, y, z);
    }
}
#endregion