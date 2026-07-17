package main

import "fmt"

// encComplex mirrors encoding/gob's function of the same shape: a PARAMETER named `i` (Go's
// single most common identifier) shadows the golib `i()` imaginary-literal helper. The
// converter must emit a shadow-immune form for the `0i` literal — a bare `i(0F)` binds the
// parameter (a non-callable int) and fails with CS0149 "Method name expected". The emitted
// form is the POSTFIX extension call `0D.i()` (member access cannot bind a local); the prior
// solution was the class-qualified `builtin.i(0D)`.
func encComplex(i int, c complex128) string {
	_ = i // i is in scope, shadowing the imaginary-literal helper
	if c != 0+0i {
		return "nonzero"
	}
	return "zero"
}

func main() {
	fmt.Println(encComplex(1, complex(0, 0)))  // zero
	fmt.Println(encComplex(2, complex(3, -4))) // nonzero
	fmt.Println(encComplex(3, 5i))             // nonzero (5i imaginary literal)
}
