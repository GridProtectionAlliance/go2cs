package main

import (
	"fmt"
	"math"
)

// Guards Go-style complex rendering in the golib fmt stub: complex128 (System.Numerics.Complex)
// and complex64 (go.complex64) must print as "(2+3i)" — imaginary part always signed — not the
// .NET ToString() forms "<2; 3>" / "(2, 3)".
func main() {
	var c128 complex128 = 2 + 3i
	var neg complex128 = 2 - 3i
	var frac complex128 = complex(2.5, -1.25)
	var realOnly complex128 = complex(5, 0)
	var imagOnly complex128 = 3i
	// NOTE: the natural forms `complex(2.5, -3.5)` / `2.5 - 3.5i` currently hit a converter
	// constant-typing defect in complex64/float32 contexts (double-typed literals) — negating
	// a float32 variable sidesteps it until that is fixed
	var c64re, c64im float32 = 2.5, 3.5
	c64 := complex(c64re, -c64im)
	var c64pos complex64 = 1 + 2i
	inf := complex(math.Inf(1), math.Inf(-1))
	nan := complex(math.NaN(), math.NaN())

	fmt.Println(c128)
	fmt.Println(neg)
	fmt.Println(frac)
	fmt.Println(realOnly)
	fmt.Println(imagOnly)
	fmt.Println(c64)
	fmt.Println(c64pos)
	fmt.Println(inf)
	fmt.Println(nan)
	fmt.Println(2 + 3i)
	fmt.Printf("%v;%v\n", c128, c64)
	fmt.Println("sprint:", fmt.Sprint(neg))
}
