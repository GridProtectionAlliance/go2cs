// Regression test: an integer-valued float-literal constant used in an integer context (Go allows
// `1.0` where an int is expected, since 1.0 is representable as int) must be emitted as an integer,
// not a `double`. The converter emitted `1.0D` (and `1.0F` before the float-default fix), which a
// C# int parameter rejects (CS1503). This is the math/cmplx `math.Inf(1.0)` pattern (Inf takes an
// int). The negated form (`-1.0`) is the convUnaryExpr companion case.
package main

import "fmt"

func takesInt(n int) int {
	return n * 10
}

func main() {
	fmt.Println(takesInt(1.0))  // integer-valued float const -> int 1
	fmt.Println(takesInt(-2.0)) // negated -> int -2
	x := 1.5                    // a real float -> float64, stays double
	fmt.Println(x)
}
