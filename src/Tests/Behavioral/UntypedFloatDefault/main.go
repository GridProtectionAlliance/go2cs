// Regression test: a Go untyped float constant defaults to float64. The converter chose the C#
// suffix by whether the value fit in float32 (`1.0` -> `1.0F`), so `z := 1.0` became a float32;
// subsequent float64 arithmetic on it then failed (CS0266). The suffix is now type-based: float32
// only when go/types resolves the literal as float32, else `D` (double). This is the math/gamma
// `z := 1.0; ...; z = z * x` (x float64) pattern.
package main

import "fmt"

func main() {
	z := 1.0 // untyped float -> float64, NOT float32
	var x float64 = 3
	for x >= 1 {
		z = z * x // float64 arithmetic (would be CS0266 if z were float32)
		x = x - 1
	}
	fmt.Println(z) // 6

	var f float32 = 2.5 // float32 context: the literal stays float32
	f = f * 2
	fmt.Println(f) // 5
}
