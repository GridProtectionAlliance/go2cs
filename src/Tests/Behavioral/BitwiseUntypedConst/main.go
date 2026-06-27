// Regression test: a named untyped constant used as a bitwise operand (incl. the `~` of Go's
// `&^`) must be cast to the bitwise result type. The UntypedInt wrapper otherwise resolves to
// `int` under the operator, breaking a wider context — `Float64bits(f) &^ signBit` (uint64,
// signBit = 1<<63) → `ulong & int` (CS0019). This is the math/copysign pattern.
package main

import "fmt"

func copysign(f, sign uint64) uint64 {
	const signBit = 1 << 63 // 2^63, exceeds int64 -> UntypedInt
	return f&^signBit | sign&signBit
}

func main() {
	fmt.Println(copysign(0xFF, 0x8000000000000000)) // keep 0xFF magnitude, take the sign bit
	fmt.Println(copysign(0x8000000000000042, 0))    // clear the sign bit
}
