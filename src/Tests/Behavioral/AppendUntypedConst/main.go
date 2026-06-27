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
}
