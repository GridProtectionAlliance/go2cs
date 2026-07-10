package main

import "fmt"

type thing struct {
	n int
	s string
}

// clone mirrors go/types predicates.go clone: flat copy of *p, address of the copy.
func clone[P *T, T any](p P) P {
	c := *p
	return &c
}

// getThrough reads through the pointer-core parameter.
func getThrough[P *T, T any](p P) T {
	return *p
}

// setThrough writes through the pointer-core parameter.
func setThrough[P *T, T any](p P, v T) {
	*p = v
}

// cloneRev swaps the type-parameter order: the erased parameter follows its element.
func cloneRev[T any, P *T](p P) P {
	c := *p
	return &c
}

// pick returns a pointer-core parameter WHOLE (no deref/address involved).
func pick[P *T, T any](a, b P, useA bool) P {
	if useA {
		return a
	}
	return b
}

// cloneChain passes a pointer-core value onward to another erased callee and recurses.
func cloneChain[P *T, T any](p P, depth int) P {
	if depth > 0 {
		return cloneChain(clone(p), depth-1)
	}
	c := *p
	return &c
}

// aliasWrite copies the pointer-core parameter into a local (a Go pointer copy) and writes
// through the copy.
func aliasWrite[P *T, T any](p P, v T) {
	q := p
	*q = v
}

// orZero nil-compares the pointer-core parameter; nil is a legal argument.
func orZero[P *T, T any](p P) T {
	if p == nil {
		var z T
		return z
	}
	return *p
}

// PtrOf is a named constraint interface whose type set is the single pointer term.
type PtrOf[T any] interface{ *T }

// cloneNamed spells the constraint through the named interface directly.
func cloneNamed[P PtrOf[T], T any](p P) P {
	c := *p
	return &c
}

// cloneEmbedded spells the identical type set with the named interface EMBEDDED.
func cloneEmbedded[P interface{ PtrOf[T] }, T any](p P) P {
	c := *p
	return &c
}

func main() {
	t1 := thing{n: 1, s: "one"}
	p1 := clone(&t1)
	t1.n = 42 // mutate the original AFTER the clone: the copy must be independent
	fmt.Println(p1.n, p1.s, t1.n)

	n := 7
	pn := clone(&n)
	n = 100
	fmt.Println(*pn, n)

	g := getThrough(&t1)
	fmt.Println(g.n, g.s)

	setThrough(&n, 55)
	fmt.Println(n)

	pr := cloneRev(&t1)
	pr.n = 9
	fmt.Println(pr.n, t1.n)

	// return-whole through P (both branches)
	pa := pick(&t1, p1, true)
	fmt.Println(pa == &t1, pick(&t1, p1, false) == p1)

	// pass-onward + recursion through erased callees
	pc := cloneChain(&n, 3)
	n = 1000
	fmt.Println(*pc, n)

	// pointer copy of P into a local, write through the copy
	aliasWrite(&n, 77)
	fmt.Println(n)

	// nil comparison with a nil argument (explicit instantiation: nil cannot infer P/T)
	fmt.Println(orZero[*int, int](nil), orZero(&n))

	// explicit full and partial instantiation lists at call position
	setThrough[*int, int](&n, 8)
	fmt.Println(n)
	pf := clone[*thing](&t1)
	fmt.Println(pf.s)

	// explicit instantiation as a function VALUE
	fv := clone[*thing, thing]
	pv := fv(&t1)
	t1.s = "changed"
	fmt.Println(pv.s, t1.s)

	// named constraint interface: direct and embedded spellings of the same type set
	pn1 := cloneNamed(&t1)
	pn2 := cloneEmbedded(&t1)
	fmt.Println(pn1.s, pn2.s)
}
