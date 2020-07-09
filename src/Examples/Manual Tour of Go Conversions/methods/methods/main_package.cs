/*
package main

import (
	"fmt"
	"math"
)

type Vertex struct {
	X, Y float64
}

func (v Vertex) Abs() float64 {
	return math.Sqrt(v.X*v.X + v.Y*v.Y)
}

func main() {
	v := Vertex{3, 4}
	fmt.Println(v.Abs())
}
*/

using fmt = go.fmt_package;
using float64 = System.Double;
using math = System.Math;

static partial class main_package
{
    public partial struct Vertex {
        public float64 X;
        public float64 Y;
    }

    public static float64 Abs(this ref Vertex v) {
	    return math.Sqrt(v.X*v.X + v.Y*v.Y);
    }

    static void Main() {
        var v = new Vertex(3, 4);
        fmt.Println(v.Abs());
    }
}