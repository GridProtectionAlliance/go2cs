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

// A spread variadic aliases the caller's slice backing array. Replacing an element in the callee
// must therefore be visible through the caller's original slice; the sslice prologue preserves
// that Go behavior without the old Span<T>.slice() copy.
func replaceFirst(bs ...*box) {
	bs[0] = &box{v: 40}
}

// cap is a safe, non-escaping read and has a direct sslice overload.
func countCap(bs ...*box) int {
	return cap(bs)
}

// go2cs passes the params Span by ref into the defer/recover execution wrapper. The empty
// deferred Go closure captures nothing; the wrapper can still use an aliasing sslice view.
func deferredLen(bs ...*box) int {
	defer func() {}()
	return len(bs)
}

// Index mutation remains visible through a spread caller even when defer requires an execution
// wrapper: the wrapper receives the original Span rather than capturing a copied heap slice.
func deferredReplaceFirst(bs ...*box) {
	defer func() {}()
	bs[0] = &box{v: 45}
}

// Named results stay outside the execution wrapper while the variadic Span is passed into it.
// The deferred closure captures n, but not bs.
func deferredNamedLen(bs ...*box) (n int) {
	defer func() { n++ }()
	return len(bs)
}

// A nested function literal likewise forces the heap slice fallback.
func capturedLen(bs ...*box) int {
	count := func() int { return len(bs) }
	return count()
}

// append may grow and therefore deliberately keeps the heap slice fallback.
func appendedLen(bs ...*box) int {
	bs = append(bs, &box{v: 50})
	return len(bs)
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
	replaceFirst(boxes...)
	fmt.Println(boxes[0].v) // 40: spread aliases the caller backing array
	fmt.Println(countCap(a, b, c))
	fmt.Println(deferredLen(a, b))
	deferredReplaceFirst(boxes...)
	fmt.Println(boxes[0].v) // 45: aliasing survives the defer/recover wrapper
	fmt.Println(deferredNamedLen(a, b, c))
	fmt.Println(capturedLen(a, b, c))
	fmt.Println(appendedLen(a, b))

	// variadic of a qualified type
	fmt.Println(countPtrs(unsafe.Pointer(a), unsafe.Pointer(b), unsafe.Pointer(c))) // 3
	fmt.Println(countPtrs())                                                        // 0

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
