// Regression test: appending an *untyped constant* element to a typed slice (e.g. []uint16)
// must compile. The untyped const renders as a golib `Untyped*` wrapper, which made C# append
// overload resolution ambiguous (the `append<T>(ISlice, params T[])` overload infers T from the
// element while the `slice<T>` overloads infer T from the slice → CS0121). The converter now
// casts an untyped-const append element to the slice's element type, matching Go's implicit
// conversion. This is the unicode/utf16 `append(a, replacementChar)` pattern.
package main

import "fmt"

// untyped named constant (the utf16 replacementChar pattern)
const marker = 0xFFFD

// named slice type (exercises the ISlice append overload)
type words []uint16

func main() {
	var a []uint16
	a = append(a, marker) // untyped named const -> cast to uint16
	a = append(a, 7, 8)   // untyped literals -> cast to uint16
	var v uint16 = 9
	a = append(a, v) // typed var: NOT cast, must still compile
	fmt.Println(len(a), a[0], a[1], a[2], a[3])

	var w words
	w = append(w, marker, 1) // append untyped consts to a named slice type
	fmt.Println(w[0], w[1])

	// A unary numeric operator over an untyped constant (`-1`, `+2`, `^0`) is itself an untyped
	// numeric constant, but a UnaryExpr not a bare literal — regexp's `append(a, -1)` on []int (a
	// nint slice) left the two append overloads ambiguous until the converter recursed through the
	// unary operator to cast the element (CS0121).
	var ints []int
	ints = append(ints, -1) // unary minus
	ints = append(ints, +2) // unary plus
	ints = append(ints, ^0) // bitwise complement (= -1 for a signed int)
	fmt.Println(ints[0], ints[1], ints[2]) // -1 2 -1

	// Appending a CONCRETE composite (a []byte) to an []any: both append overloads apply — the
	// ISlice overload infers T=[]byte, the slice<T> overload infers T=any — leaving them
	// AMBIGUOUS (CS0121; testing flushToParent's `append(args[:len(args):len(args)], c.output)`
	// with args []any and c.output []byte). The converter casts the differing element to `any`
	// (the empty-interface element type) so both overloads agree.
	var anys []any
	data := []byte{7, 8, 9}
	anys = append(anys[:len(anys):len(anys)], data)        // []byte element -> cast to any
	anys = append(anys, 5)                                 // int element -> cast to any
	fmt.Println(len(anys), len(anys[0].([]byte)), anys[1]) // 2 3 5
}
