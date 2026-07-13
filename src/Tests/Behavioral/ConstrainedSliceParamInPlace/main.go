package main

import "fmt"

// reverse reverses a plain []E slice in place — an order-free "[]E-typed sink".
func reverse[E any](s []E) {
	for i, j := 0, len(s)-1; i < j; i, j = i+1, j-1 {
		s[i], s[j] = s[j], s[i]
	}
}

// reverseSeq forwards a ~[]E-constrained parameter where []E is expected — the exact
// construct slices.Sort/SortFunc/SortStableFunc use (each runs pdqsort/stableCmpFunc on
// the []E-converted parameter). The caller MUST observe the mutation: a Go slice-to-slice
// conversion copies the slice header but SHARES the backing array — it must not deep-copy.
func reverseSeq[S ~[]E, E any](x S) {
	reverse(x)
}

// explicitReverseSeq is the EXPLICIT-conversion twin of reverseSeq: it writes the Go
// slice conversion []E(x) out explicitly (rather than relying on assignability). Before
// the converter fix this emitted a broken `slice<E>(x)` that bound golib's array-only
// builtin.slice<T>(T[]) — CS1503, since the source S is an ISlice<E>, not E[]. It now
// emits the sharing `new slice<E>(x)`, so the caller still observes the in-place reversal.
func explicitReverseSeq[S ~[]E, E any](x S) {
	reverse([]E(x))
}

// insertionSort is a real in-place stable sort, mirroring slices' stableCmpFunc[E].
func insertionSort[E any](data []E, less func(a, b E) bool) {
	for i := 1; i < len(data); i++ {
		for j := i; j > 0 && less(data[j], data[j-1]); j-- {
			data[j], data[j-1] = data[j-1], data[j]
		}
	}
}

// sortSeq mirrors slices.SortStableFunc: forward the ~[]E parameter to a []E sorter.
func sortSeq[S ~[]E, E any](x S, less func(a, b E) bool) {
	insertionSort(x, less)
}

// numbers and sortedMap are named slice types — sortedMap mirrors internal/fmtsort's
// `SortedMap []KeyValue`, whose values fmt builds with make+append then sorts in place.
type numbers []int
type sortedMap []int

func main() {
	// Plain []int, reversed in place.
	a := []int{1, 2, 3, 4, 5}
	reverseSeq(a)
	fmt.Println(a) // [5 4 3 2 1]

	// Named slice type (composite literal), reversed in place.
	b := numbers{10, 20, 30}
	reverseSeq(b)
	fmt.Println(b) // [30 20 10]

	// []string element type, reversed in place.
	s := []string{"go", "2", "cs"}
	reverseSeq(s)
	fmt.Println(s) // [cs 2 go]

	// EXPLICIT []E(x) conversion of a ~[]E parameter — plain and named source.
	c := []int{6, 7, 8, 9}
	explicitReverseSeq(c)
	fmt.Println(c) // [9 8 7 6]

	d := numbers{100, 200}
	explicitReverseSeq(d)
	fmt.Println(d) // [200 100]

	// internal/fmtsort shape: a named slice built via make+append, sorted in place
	// through a ~[]E parameter forwarded to a []E-typed sorter. This is what makes
	// fmt.Println(map) print keys in sorted order.
	m := make(sortedMap, 0, 4)
	m = append(m, 3, 1, 4, 1)
	sortSeq(m, func(x, y int) bool { return x < y })
	fmt.Println(m) // [1 1 3 4]
}
