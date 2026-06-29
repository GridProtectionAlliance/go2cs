package main

import "fmt"

// Go evaluates narrow-integer arithmetic (uint8/int8/uint16/int16) at the operand's own width
// with overflow WRAPPING. C# promotes sub-int arithmetic to `int`, so passing `a+b`/`^a` to a
// narrow-typed parameter both fails to compile (int→uint8 is not implicit) and would lose the
// wrap. The converter casts the argument back to the parameter type — `take((uint8)(a + b))` —
// which both compiles and preserves Go's wrapping. Guards isNarrowIntegerKind / castArgToType.

func takeU8(x uint8) uint8    { return x }
func takeI8(x int8) int8      { return x }
func takeU16(x uint16) uint16 { return x }

func main() {
	var a, b uint8 = 200, 100
	fmt.Println(takeU8(a + b)) // 300 wraps to 44
	fmt.Println(takeU8(^a))    // ^200 = 55

	var c, d int8 = 100, 100
	fmt.Println(takeI8(c + d)) // 200 wraps to -56

	var e, f uint16 = 60000, 10000
	fmt.Println(takeU16(e + f)) // 70000 wraps to 4464

	// Nested arithmetic argument.
	fmt.Println(takeU8(a*2 + b)) // (400+100)=500 -> 244
}
