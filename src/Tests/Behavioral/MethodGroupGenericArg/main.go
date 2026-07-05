package main

import "fmt"

func addInt(a, b int) int { return a + b }

// foldSlice mirrors slices.SortFunc's shape: the element type E appears only in the `~[]E`
// CONSTRAINT and in the func-typed parameter. C# cannot infer E from the constraint, and a
// METHOD-GROUP argument (`addInt`, a bare function reference) gives it nothing either, so the
// converter must spell out the type arguments — bare `foldSlice(nums, addInt)` is CS0411
// (encoding/asn1's `slices.SortFunc(l, bytes.Compare)`).
func foldSlice[S ~[]E, E any](s S, combine func(E, E) E) E {
	var acc E
	for _, v := range s {
		acc = combine(acc, v)
	}
	return acc
}

func main() {
	nums := []int{1, 2, 3, 4}
	sum := foldSlice(nums, addInt) // method-group comparator; E inferable only via S ~[]E
	fmt.Println(sum)               // 10
}
