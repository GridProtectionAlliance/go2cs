package main

import "fmt"

// A named integer element type (mirrors ML-KEM's fieldElement).
type fieldElement uint16

// Two distinct named array-wrapper types sharing the same array core [4]fieldElement
// (mirrors ML-KEM's ringElement and nttElement). A generic function constrained on the
// array core `~[4]fieldElement` must accept BOTH as its type argument.
type ringElement [4]fieldElement
type nttElement [4]fieldElement

// addPoly is constrained on the array core `~[4]fieldElement`. The body ranges the
// type-parameter value with an index deconstruction, indexes it, and returns a fresh
// zero value `s` of T — exactly the array surface the converter must map to
// `where T : IArray<fieldElement>`. Before the fix this lifted the spurious
// IEqualityOperators<T,T,bool> (which the array-wrapper struct cannot satisfy and which
// exposes no indexer/enumerator), so both the body and the instantiations failed.
func addPoly[T ~[4]fieldElement](a, b T) (s T) {
	for i := range s {
		s[i] = a[i] + b[i]
	}
	return s
}

// scalePoly constructs `var f T` and writes through the indexer.
func scalePoly[T ~[4]fieldElement](a T, k fieldElement) T {
	var f T
	for i := range a {
		f[i] = a[i] * k
	}
	return f
}

// sum ranges the type-parameter value for VALUE (discarding the index), exercising the
// discard/foreach-over-T path.
func sum[T ~[4]fieldElement](a T) fieldElement {
	var total fieldElement
	for _, x := range a {
		total += x
	}
	return total
}

func main() {
	r1 := ringElement{1, 2, 3, 4}
	r2 := ringElement{10, 20, 30, 40}
	rs := addPoly(r1, r2)
	fmt.Println(rs[0], rs[1], rs[2], rs[3], sum(rs))

	n1 := nttElement{5, 6, 7, 8}
	n2 := nttElement{50, 60, 70, 80}
	ns := addPoly(n1, n2)
	fmt.Println(ns[0], ns[1], ns[2], ns[3], sum(ns))

	scaled := scalePoly(r1, 3)
	fmt.Println(scaled[0], scaled[1], scaled[2], scaled[3])
}
