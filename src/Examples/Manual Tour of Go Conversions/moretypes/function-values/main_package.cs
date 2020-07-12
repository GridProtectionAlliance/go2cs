/*
package main

import (
	"fmt"
	"math"
)

func compute(fn func(float64, float64) float64) float64 {
	return fn(3, 4)
}

func main() {
	hypot := func(x, y float64) float64 {
		return math.Sqrt(x*x + y*y)
	}
	fmt.Println(hypot(5, 12))

	fmt.Println(compute(hypot))
	fmt.Println(compute(math.Pow))
}
*/
#region source
using System;
using fmt = go.fmt_package;
using math = System.Math;
using float64 = System.Double;

static class main_package
{
	static float64 compute(Func<float64, float64, float64> fn) {
		return fn(3, 4);
	}

    static void Main() {
		float64 hypot(float64 x, float64 y) {
			return math.Sqrt(x*x + y*y);
		}
        fmt.Println(hypot(5, 12));

		fmt.Println(compute(hypot));
		fmt.Println(compute(math.Pow));
    }
}
#endregion