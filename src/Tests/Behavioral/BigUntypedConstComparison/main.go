// Regression test: a named untyped constant whose value exceeds int64/uint64 is emitted as
// GoUntyped (= System.Numerics.BigInteger), which has no implicit operator with the built-in
// numeric types. Comparing it with a concrete type — `x > Two129` (double > BigInteger) — failed
// with CS0019. The converter now casts a BigInteger-backed untyped const to the concrete operand's
// type in comparisons (it already did so for arithmetic). This is the math/j0 `x > Two129` pattern.
package main

import "fmt"

const Two129 = 1 << 129 // 2**129 ~= 6.8e38, exceeds uint64 -> GoUntyped (BigInteger)

func main() {
	var x float64 = 1e40
	fmt.Println(x > Two129) // true
	fmt.Println(x < Two129) // false
	var y float64 = 1e30
	fmt.Println(y >= Two129) // false
}
