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

func (v *Vertex) Scale(f float64) {
	v.X = v.X * f
	v.Y = v.Y * f
}

func main() {
	v := Vertex{3, 4}
	v.Scale(10)
	fmt.Println(v.Abs())
}
*/

using fmt = go.fmt_package;
using math = System.Math;

using float64 = System.Double;

static partial class main_package
{
    public partial struct Vertex {
        public float64 X;
        public float64 Y;
    }

    public static float64 Abs(this Vertex v) {
	    return math.Sqrt(v.X*v.X + v.Y*v.Y);
    }

    public static void Scale(this ref Vertex v, float64 f) {
        v.X = v.X * f;
        v.Y = v.Y * f;
    }

    static void Main() {
        var v = new Vertex(3, 4);
        v.Scale(10);
        fmt.Println(v.Abs());
    }
}