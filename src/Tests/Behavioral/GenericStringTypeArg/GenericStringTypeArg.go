package main

import "fmt"

type Box[T any] struct {
	v T
}

type Pair[A any, B any] struct {
	a A
	b B
}

// Exercises a Go `string` used as a GENERIC TYPE ARGUMENT. The converter must render
// it as golib `@string`, not C# `string` (System.String): the converter adds a `new()`
// constraint to generic type parameters, which `string` violates (CS0310), and assigning
// a string literal (a ReadOnlySpan<byte> u8 literal) into such a field fails (CS0029).
//
// The defect was specific to the SECOND+ type argument: splitTopLevelTypes keeps the
// ", " separator's leading space, so " string" missed the exact `case "string"` switch
// and fell through to the named-type path. So Pair's second arg is the key case; the
// single-arg Box[string] guards the first-arg path stays correct too.
func main() {
	var p Pair[int, string]
	p.a = 5
	p.b = "hi"
	fmt.Println(p.a, p.b)

	var b Box[string]
	b.v = "boxed"
	fmt.Println(b.v)

	// string as both args, and string as a nested second arg.
	var sp Pair[string, string]
	sp.a = "x"
	sp.b = "y"
	fmt.Println(sp.a, sp.b)

	var nested Pair[int, Box[string]]
	nested.a = 9
	nested.b.v = "deep"
	fmt.Println(nested.a, nested.b.v)
}
