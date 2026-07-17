package main

import "fmt"

// Guards contextual typing of untyped constants in float32/complex64 contexts: go/types
// resolves the context on the OUTERMOST constant expression only, leaving inner operands
// untyped, so the converter must propagate the context down (F vs D literal suffixes and
// the postfix .i()/complex() overload choice) instead of falling back to the untyped
// default float64/complex128.
func main() {
	var c64 complex64 = complex(2.5, -3.5) // builtin args through a unary minus
	var c64b complex64 = 2.5 - 3.5i        // binary operands: real constant + imaginary literal
	var a, b float32 = 2.5, -3.5           // negated float constant in direct assignment
	var nested float32 = -(1.5 + 2.0)      // context through parens and nested binary
	var quo float32 = 7.0 / 2.0            // quotient operands
	var c128 complex128 = 2.5 - 3.5i       // float64 context stays D
	var prec complex128 = 0.1 + 0.1i       // 0.1i must NOT round-trip through complex64
	var minf float32 = min(1.5, -2.5)      // min/max builtin args
	var maxf float32 = max(1.5, -2.5)
	var re float32 = real(2.5 - 3.5i)        // real/imag args are the complex counterpart
	var im float32 = imag(complex(2.5, -3.5))

	type meters float32
	var dist meters = -1.5 // named type context resolves through its underlying float32

	// Complex values print via their real/imag parts so this test guards only the
	// converter's constant typing, not golib fmt's Go-style complex rendering
	fmt.Println(real(c64), imag(c64))
	fmt.Println(real(c64b), imag(c64b))
	fmt.Println(a, b)
	fmt.Println(nested)
	fmt.Println(quo)
	fmt.Println(real(c128), imag(c128))
	fmt.Println(real(prec), imag(prec)) // old converter: imag printed 0.10000000149011612
	fmt.Println(minf, maxf)
	fmt.Println(re, im)
	fmt.Println(dist)
}
