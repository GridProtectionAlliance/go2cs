// Regression test: a named untyped numeric constant (emitted as the golib UntypedInt wrapper)
// used in arithmetic with a concrete wider/unsigned type. The wrapper's bidirectional implicit
// conversions make e.g. `a * two32` (uint64 * UntypedInt) resolve to the wrong type (CS0029).
// The converter casts the named-untyped-const operand to the concrete operand's type. This is
// the math/bits `div64` `q1*two32 + q0` pattern (two32 = 1<<32).
package main

import "fmt"

const two32 = 1 << 32 // untyped const -> UntypedInt; value exceeds int32

func sum(n int64) int64 { return n }

func main() {
	var a uint64 = 100
	var b uint64 = 3
	fmt.Println(a*two32 + b) // 429496729603
	fmt.Println(two32 * a)   // 429496729600 (const on the left)
	fmt.Println(a >= two32)  // false (comparison still works)
	fmt.Println(a - two32%a) // mixed

	// A SIGNED constant operator expression whose value overflows int32 must be emitted as the
	// folded 64-bit literal — C# computes `1<<63` / `12345*1000000000` in int32 and overflows at
	// compile time in checked mode (CS0220). Go evaluates these in the constant's int64 type.
	var c int64 = 1<<40 + 7 // overflows int32; folded to 1099511627783L
	d := int64(1<<63 - 1)   // maxInt64
	fmt.Println(c)          // 1099511627783
	fmt.Println(d)          // 9223372036854775807
	fmt.Println(sum(12345*1000000000 + 54321)) // 12345000054321
}
