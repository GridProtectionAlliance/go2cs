package main

import "fmt"

type Box[T any] struct {
	v T
}

type Pair[A any, B any] struct {
	a A
	b B
}

// Exercises nested generic type arguments in the converter's type-name parsing
// (convertToCSFullTypeName): a naive "first '>'" / "split on every ','" approach
// mis-parses Box[Box[int]] and Pair[Box[int], Box[int]], slicing out of range.
func main() {
	var nested Box[Box[int]]
	nested.v.v = 42
	fmt.Println(nested.v.v)

	// Top-level comma between two nested generics exercises the depth-aware split.
	var p Pair[Box[int], Box[int]]
	p.a.v = 7
	p.b.v = 8
	fmt.Println(p.a.v, p.b.v)

	var deep Box[Box[Box[int]]]
	deep.v.v.v = 99
	fmt.Println(deep.v.v.v)
}
