package main

import "fmt"

// A package-level untyped float constant emits as a golib UntypedFloat (implicitly convertible to
// BOTH float32 and float64). That is what exposes the complex() overload-selection bug below; a
// folded float literal would already emit `D` and rule out the float32 overload on its own.
const gHalfPi = 1.5707963267948966

func main() {
	// (1) complex128-context int-literal SHIFTS out of int32 range must fold to the exact value,
	// not a C# int32-masked shift (1<<35 masked to 1<<3 = 8). Guards floatContextConstLiteral's
	// complex128 arm.
	huge := []complex128{1 << 35, 1 << 240, -1 << 120, 1234567891234567 << 40}
	for _, h := range huge {
		fmt.Println(real(h))
	}

	// (2) An int-literal complex() argument in a complex128 context must bind the float64 element
	// overload, not float32 — else gHalfPi recomputes at float32 (1.5707963705062866) instead of
	// float64 (1.5707963267948966). Guards the convBasicLit int-literal float-context render.
	c := complex(0, gHalfPi)
	fmt.Println(real(c), imag(c))
}
