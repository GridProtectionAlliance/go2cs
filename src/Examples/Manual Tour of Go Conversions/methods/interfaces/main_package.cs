/*
package main

import (
	"fmt"
	"math"
)

type Abser interface {
	Abs() float64
}

func main() {
	var a Abser
	f := MyFloat(-math.Sqrt2)
	v := Vertex{3, 4}

	a = f  // a MyFloat implements Abser
	a = &v // a *Vertex implements Abser

	// In the following line, v is a Vertex (not *Vertex)
	// and does NOT implement Abser.
	//a = v

	fmt.Println(a.Abs())
}

type MyFloat float64

func (f MyFloat) Abs() float64 {
	if f < 0 {
		return float64(-f)
	}
	return float64(f)
}

type Vertex struct {
	X, Y float64
}

func (v *Vertex) Abs() float64 {
	return math.Sqrt(v.X*v.X + v.Y*v.Y)
}
*/

using System;
using static go.builtin;
using float64 = System.Double;
using MyFloat = System.Double;

static partial class main_package
{
    partial interface Abser {
		float64 Abs();
    }

	static void Main() {
		Abser a;
		var f = (MyFloat)(-Math.Sqrt(2));
		ref var v = ref heap(new Vertex(3, 4), out var _v_ptr).Value;

		a = Abser.As(f);  // a MyFloat implements Abser
		a = Abser.As(_v_ptr); // a *Vertex implements Abser

		// In the following line, v is a Vertex (not *Vertex)
		// and does NOT implement Abser -- in Go anyway, here in
		// C# this would work fine with current implementation
		//a = v

		println(a.Abs());
    }

	public static float64 Abs(this MyFloat f) {
        if (f < 0) {
			return (float64)(-f);
        }
		return (float64)(f);
    }

    public partial struct Vertex
    {
        public float64 X;
        public float64 Y;
    }

	public static float64 Abs(this ref Vertex v) {
		return Math.Sqrt(v.X * v.X + v.Y * v.Y);
	}
}