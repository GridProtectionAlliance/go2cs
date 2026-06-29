package main

import "fmt"

// Go evaluates narrow-integer arithmetic (uint8/int8/uint16/int16) at the operand's own width
// with overflow WRAPPING. C# promotes sub-int arithmetic to `int`, so a narrow-arithmetic value
// used where the narrow type is required — a narrow-typed parameter, an assignment to a narrow
// var/field/element, or a typed-var initializer — both fails to compile (int→narrow is not
// implicit) and would lose the wrap. The converter casts back to the narrow type, preserving Go's
// wrapping. Covers the argument, assignment (var/field/element), and declaration contexts.

func takeU8(x uint8) uint8    { return x }
func takeI8(x int8) int8      { return x }
func takeU16(x uint16) uint16 { return x }

type box struct {
	b uint8
}

func main() {
	var a, b uint8 = 200, 100

	// Argument context.
	fmt.Println(takeU8(a + b)) // 300 wraps to 44
	fmt.Println(takeU8(^a))    // ^200 = 55

	var c, d int8 = 100, 100
	fmt.Println(takeI8(c + d)) // 200 wraps to -56

	var e, f uint16 = 60000, 10000
	fmt.Println(takeU16(e + f)) // 70000 wraps to 4464

	fmt.Println(takeU8(a*2 + b)) // 500 -> 244

	// Assignment context: var, reassignment, indexed element, struct field.
	y := a + b // short-var declaration; y is uint8, wraps to 44
	fmt.Println(y)

	y = y + 1 // reassignment, 45
	fmt.Println(y)

	var arr [1]uint8
	arr[0] = a + b // indexed-element assignment, wraps to 44
	fmt.Println(arr[0])

	var bx box
	bx.b = a + b // struct-field assignment, wraps to 44
	fmt.Println(bx.b)

	// Declaration context: typed-var initializer.
	var z uint8 = a + b // wraps to 44
	fmt.Println(z)
}
