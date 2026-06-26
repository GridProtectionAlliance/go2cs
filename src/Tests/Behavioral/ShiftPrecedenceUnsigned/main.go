// Regression test for three numeric-lowering defects, all exercised by math/bits:
//
//  1. Unary minus on an unsigned operand. Go's `-x` on an unsigned value is
//     two's-complement negation that wraps mod 2^N (the `x & -x` lowest-set-bit
//     idiom). C# forbids unary minus on unsigned types (CS0023); the converter now
//     emits `(T)0 - x`, which has identical wrap-around semantics.
//
//  2. Shift-operator precedence. Go's `<<`/`>>` bind tighter than `+`/`-`, but
//     C#'s bind looser. So Go's `x>>4 + x` (= `(x>>4)+x`) and `1<<15 - 1` (= 32767)
//     would re-associate in C# as `x>>(4+x)` and `1<<(15-1)`. The converter now
//     parenthesizes shift expressions to preserve Go grouping.
//
//  3. Compound shift-assignment count. The shift count in a C# `<<=`/`>>=` must be
//     `int`; casting it to the RHS's own (unsigned / native-width) type is rejected
//     (CS0019). The converter now casts shift-assignment counts to `int`.
package main

import "fmt"

func lowestSetBit(x uint32) uint32 {
	return x & -x // wrap-around negation; isolates the lowest set bit
}

func main() {
	// (1) unary minus on unsigned.
	fmt.Println(lowestSetBit(12)) // 0b1100 -> 4
	fmt.Println(lowestSetBit(40)) // 0b101000 -> 8
	var z uint64 = 6
	fmt.Println(z & -z) // 2

	// (2) shift precedence: `x>>4 + x` must be `(x>>4)+x`.
	var x uint64 = 0x100
	fmt.Println(x>>4 + x) // (0x100>>4)+0x100 = 16 + 256 = 272

	// shift precedence: `1<<15 - 1` must be `(1<<15)-1` = 32767.
	fmt.Println(1<<15 - 1) // 32767

	// shift precedence with subtraction on the left: `x - 1<<4`.
	fmt.Println(x - 1<<4) // 256 - 16 = 240

	// (3) compound shift-assignment with an unsigned shift count.
	var s uint = 3
	var y uint64 = 1
	y <<= s
	fmt.Println(y) // 8
	y >>= s
	fmt.Println(y) // 1
}
