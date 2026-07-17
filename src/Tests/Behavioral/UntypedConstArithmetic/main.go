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
	var c int64 = 1<<40 + 7                    // overflows int32; folded to 1099511627783L
	d := int64(1<<63 - 1)                      // maxInt64
	fmt.Println(c)                             // 1099511627783
	fmt.Println(d)                             // 9223372036854775807
	fmt.Println(sum(12345*1000000000 + 54321)) // 12345000054321

	// An UNSIGNED-context constant expression with a NESTED untyped shift: go/types lands the
	// uint64 conversion on the OUTERMOST constant node, so the inner `1<<63` stays untyped and
	// no width-cast reaches it - C# would compute it in int32 (CS0220). The whole constant
	// subtree folds to 9223372036854775807UL; the standalone typed shift `(1<<63)%uint64(n)`
	// keeps its readable width-cast form. Mirrors math/rand's Int63n.
	n := int64(7)
	max := int64((1 << 63) - 1 - (1<<63)%uint64(n))
	fmt.Println(max) // 9223372036854775806

	// A FLOAT-context constant built from integer literals folds the same way, and here the damage
	// is SILENT rather than a compile error: C# masks a shift count to 5 bits, so `1<<63` would
	// evaluate in int32 as 63&31 = 31 -> int.MinValue and the `1<<60` divisor as 60&31 = 28 -> 2^28
	// (printing 34359738368 instead of 8). Both fold to the Go-evaluated value (9223372036854775808D,
	// 1152921504606846976D) - note 1<<63 exceeds int64, so only a FLOAT literal can carry it.
	// (These print quotients rather than the raw values because golib's fmt does not yet reproduce
	// Go's %g exponent form - an unrelated formatting gap that would mask what is asserted here.)
	var hf float64 = 1 << 63
	fmt.Println(hf / 1e18)      // 9.223372036854776 (int32 eval would give -2.147483648e-09)
	fmt.Println(hf / (1 << 60)) // 8

	// float32 takes the same fold with an `F` suffix (1099511627776F).
	var sf float32 = 1 << 40
	fmt.Println(sf / (1 << 30)) // 1024

	// A shift nested in a float computation is NOT `untyped int` - Go promotes the operands of
	// `1<<40 * 1.5` to a common kind, leaving the shift `untyped float`, so the signed int64 fold
	// never sees it. It folds from its propagated CONTEXT instead (1099511627776D); emitted as a
	// bare int32 shift it would mask to 256 and silently yield 0.375 here.
	var big float64 = 1 << 40
	fmt.Println(1 << 40 * 1.5 / big) // 1.5

	// NEGATIVE guards - these keep their readable operator form, and prove it stays correct:
	// an int32-RANGE constant computes identically in C# int32 (`1 << 10`), and a FLOAT-literal
	// operand already renders as a C# double computation (`1e18D * 10.0D`).
	var small float64 = 1 << 10
	fmt.Println(small / (1 << 3)) // 128
	fmt.Println(1e18 * 10.0 / hf) // 1.0842021724855044
}
