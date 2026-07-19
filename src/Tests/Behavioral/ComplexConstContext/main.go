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

	// (3) INTEGER division stays integer even in a float/complex context: Go evaluates untyped int
	// operands with integer division, so `7 / 2` is 3 (then 3.0), NOT 3.5. The int-literal render
	// must not push a float context onto a `/` operand. `e`'s 0D sibling still pins complex128, so
	// its real part is the integer quotient 3.0.
	var q float64 = 7 / 2
	e := complex(7/2, 0)
	fmt.Println(q, real(e), imag(e))

	// A FLOAT-kind operand keeps float division (gHalfPi is UntypedFloat): gHalfPi / 2 = 0.785...
	fmt.Println(gHalfPi / 2)
}
