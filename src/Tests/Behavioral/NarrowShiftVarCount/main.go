// NarrowShiftVarCount guards the width-wraparound of a NARROW (sub-int-width) left shift by a
// non-constant count: Go computes `byte(200) << 1` at byte width (144), but C# promotes a
// byte/sbyte/short/ushort left operand to `int` in a shift (400 — no wrap). The converter re-types
// the shift result to its resolved Go type — `(byte)(cb << (int)k)` — keyed on the SHIFT
// EXPRESSION's resolved type, so var, typed-const, and untyped-const left operands all wrap alike
// (the untyped-const flavor previously took the cast only via its Untyped* wrapper path; typed
// operands got no cast at all and printed 400). Right shifts of a narrow operand cannot exceed the
// narrow width (byte zero-extends / int8 sign-extends into the int-width shift), so they take no
// cast. Verified vs Go including int8/int16 sign wraps and a count exceeding the operand width.
package main

import "fmt"

const tcb byte = 200     // typed const left operand
const ucb = 200          // untyped const left operand
const tcw uint16 = 40000 // typed const uint16 left operand

type nb byte // named type over a narrow width

func main() {
	var k uint = 1
	var ki int = 9
	var b byte = 1
	var cb byte = 200
	var w uint16 = 40000
	var s8 int8 = -100
	var s16 int16 = -30000
	var n nb = 200

	fmt.Println(b + cb<<k)  // 145: byte(200)<<1 wraps to 144, +1
	fmt.Println(cb << k)    // 144: var left operand
	fmt.Println(tcb << k)   // 144: typed-const left operand
	fmt.Println(b + ucb<<k) // 145: untyped-const left operand (resolves to byte via context)
	fmt.Println(cb << 3)    // 64: constant count, var operand — 1600 wraps at byte width
	fmt.Println(cb << ki)   // 0: count exceeds byte width, all bits shifted out
	fmt.Println(w << k)     // 14464: 80000 wraps at uint16 width
	fmt.Println(tcw << k)   // 14464: typed-const uint16
	fmt.Println(s8 << k)    // 56: -200 wraps at int8 width
	fmt.Println(s16 << k)   // 5536: -60000 wraps at int16 width
	fmt.Println(n << k)     // 144: named type over byte wraps at the underlying width
	fmt.Println(cb >> k)    // 100: right shift needs no wrap
	fmt.Println(s8 >> k)    // -50: arithmetic right shift of int8

	x := cb << k
	fmt.Println(x + b) // 145: shift result stored then reused
}
