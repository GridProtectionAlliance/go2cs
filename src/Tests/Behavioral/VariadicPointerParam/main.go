// Regression test for a variadic parameter whose element type is a pointer (or any generic
// C# type), e.g. `func In(r rune, ranges ...*RangeTable)`.
//
// The converter emits a variadic parameter as a C# params collection. For a simple element
// type it used a namespace-level using alias (`ꓸꓸꓸT = Span<T>`) so the parameter reads as
// `params ꓸꓸꓸT`, mimicking Go's `...T`. But for a pointer element type the C# type name is
// `ж<RangeTable>` — and `using ꓸꓸꓸж<RangeTable> = Span<ж<RangeTable>>;` is invalid: an alias
// identifier cannot contain '<'/'>'. The fix emits the span type inline for any generic/pointer
// element: `params Span<ж<box>>` (mirroring the existing type-parameter special case).
package main

import (
	"fmt"
	"unsafe"
)

type box struct {
	v int
}

// variadic of a pointer type
func total(bs ...*box) int {
	sum := 0
	for _, b := range bs {
		sum += b.v
	}
	return sum
}

// variadic of a qualified (cross-package) type. Unlike the pointer element above, this one DOES get
// an alias: the identifier joins the qualifier with the `ꓸ` glyph (`params ꓸꓸꓸunsafeꓸPointer`), so it
// still reads like Go's `...unsafe.Pointer`. The referent, however, cannot use the file-local
// `using @unsafe = unsafe_package;` alias — C# resolves a using-alias referent with the compilation
// unit's own usings NOT in effect (CS0246) — so it is rewritten to that alias's own target:
// `using ꓸꓸꓸunsafeꓸPointer = Span<unsafe_package.Pointer>;`.
func countPtrs(ps ...unsafe.Pointer) int {
	return len(ps)
}

func main() {
	a := &box{v: 1}
	b := &box{v: 2}
	c := &box{v: 3}

	fmt.Println(total(a, b, c)) // 6
	fmt.Println(total())        // 0
	fmt.Println(total(a))       // 1

	// call-site spread of a slice into the variadic
	boxes := []*box{a, b, c, &box{v: 4}}
	fmt.Println(total(boxes...)) // 10

	// variadic of a qualified type
	fmt.Println(countPtrs(unsafe.Pointer(a), unsafe.Pointer(b), unsafe.Pointer(c))) // 3
	fmt.Println(countPtrs())                                                         // 0

	fmt.Println(pairTotal(a, b, c)) // 6
}

// deref-aliased pointer PARAMETERS passed as variadic pointer args: every trailing arg must
// render as its box, not the deref'd value alias - edwards25519's checkInitialized(p, q)
// emitted only the first variadic arg as the box (CS1503).
func pairTotal(p, q, r *box) int {
	return total(p, q, r)
}
