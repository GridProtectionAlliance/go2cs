// Regression test for variadic parameters whose element type is NOT a plain same-package name,
// e.g. `func In(r rune, ranges ...*RangeTable)`.
//
// The converter emits a variadic parameter as a C# params collection, preferring a namespace-level
// using alias (`ꓸꓸꓸT = Span<T>`) so the parameter reads as `params ꓸꓸꓸT`, mimicking Go's `...T`.
// A using-alias identifier cannot contain '<', '>' or '.', so this file pins all three outcomes:
//
//   - POINTER element → aliased via go2cs's own `ж` notation, `params ꓸꓸꓸжbox` for `...*box`. The
//     referent must qualify the pointee for namespace scope (`Span<ж<main_package.box>>`) — only
//     the inline form sits inside the package class where bare `box` would resolve.
//   - CROSS-PACKAGE element → qualifier joined with the `ꓸ` glyph, `params ꓸꓸꓸunsafeꓸPointer`. Its
//     referent cannot use the file-local `using @unsafe = …` alias (C# resolves a using-alias
//     referent with the compilation unit's own usings NOT in effect, CS0246), so it is rewritten to
//     that alias's target: `Span<unsafe_package.Pointer>`.
//   - SLICE (or any other constructed) element → no identifier-safe transliteration exists, so it
//     keeps the INLINE form, `params Span<slice<byte>>`. (The type-parameter arm of that same
//     fallback is guarded by the GenericVariadicFunc test.)
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

// variadic of a SLICE type — `[]byte` renders as the constructed `slice<byte>`, which (unlike a
// pointer's `ж`) has no identifier-safe transliteration, so this one keeps the inline
// `params Span<slice<byte>>`. Guards that fallback now that the pointer arm above is aliased.
func totalLens(bss ...[]byte) int {
	sum := 0
	for _, bs := range bss {
		sum += len(bs)
	}
	return sum
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

	// variadic of a slice type — stays inline, no alias
	fmt.Println(totalLens([]byte("ab"), []byte("cde"))) // 5
	fmt.Println(totalLens())                            // 0

	fmt.Println(pairTotal(a, b, c)) // 6
}

// deref-aliased pointer PARAMETERS passed as variadic pointer args: every trailing arg must
// render as its box, not the deref'd value alias - edwards25519's checkInitialized(p, q)
// emitted only the first variadic arg as the box (CS1503).
func pairTotal(p, q, r *box) int {
	return total(p, q, r)
}
