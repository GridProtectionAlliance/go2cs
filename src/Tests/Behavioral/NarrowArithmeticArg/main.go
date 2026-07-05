package main

import "fmt"

// Go evaluates narrow-integer arithmetic (uint8/int8/uint16/int16) at the operand's own width
// with overflow WRAPPING. C# promotes sub-int arithmetic to `int`, so a narrow-arithmetic value
// used where the narrow type is required — a narrow-typed parameter, an assignment to a narrow
// var/field/element, or a typed-var initializer — both fails to compile (int→narrow is not
// implicit) and would lose the wrap. The converter casts back to the narrow type, preserving Go's
// wrapping. Covers the argument, assignment (var/field/element), declaration, and struct-composite-
// literal (positional + keyed) contexts — the last is image/png's `color.Gray{(b >> 7) * 0xff}`.

func takeU8(x uint8) uint8    { return x }
func takeI8(x int8) int8      { return x }
func takeU16(x uint16) uint16 { return x }

type box struct {
	b uint8
}

// pix mirrors image/color's small-integer structs (Gray.Y, NRGBA fields all uint8).
type pix struct {
	Y uint8
	A uint8
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

	// Struct-composite-literal context. `(a >> 1) * 3` has an untyped-constant operand, so it
	// promotes to int in C# with no cast of its own — the composite element must supply the
	// cast to the uint8 field (the png color.Gray shape). Both positional and keyed forms.
	p1 := pix{(a >> 1) * 3, a + b} // (200>>1)*3 = 300 -> 44 ; 300 -> 44
	fmt.Println(p1.Y, p1.A)        // 44 44

	p2 := pix{Y: (a >> 1) * 3, A: ^a} // 44 ; ^200 = 55
	fmt.Println(p2.Y, p2.A)           // 44 55
}
