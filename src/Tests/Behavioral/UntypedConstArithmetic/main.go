// Regression test: a named untyped numeric constant (emitted as the golib UntypedInt wrapper)
// used in arithmetic with a concrete wider/unsigned type. The wrapper's bidirectional implicit
// conversions make e.g. `a * two32` (uint64 * UntypedInt) resolve to the wrong type (CS0029).
// The converter casts the named-untyped-const operand to the concrete operand's type. This is
// the math/bits `div64` `q1*two32 + q0` pattern (two32 = 1<<32).
package main

import "fmt"

const two32 = 1 << 32 // untyped const -> UntypedInt; value exceeds int32

func main() {
	var a uint64 = 100
	var b uint64 = 3
	fmt.Println(a*two32 + b) // 429496729603
	fmt.Println(two32 * a)   // 429496729600 (const on the left)
	fmt.Println(a >= two32)  // false (comparison still works)
	fmt.Println(a - two32%a) // mixed
}
