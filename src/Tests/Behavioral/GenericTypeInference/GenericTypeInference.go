package main

import (
	"fmt"
)

type Signed interface {
	~int | ~int8 | ~int16 | ~int32 | ~int64
}

type Unsigned interface {
	~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
}

type Integer interface {
	Signed | Unsigned
}

type Point []int32

func (p Point) String() string {
	return fmt.Sprintf("%d", p)
}

// Scale returns a copy of s with each element multiplied by c.
func Scale[S ~[]E, E Integer](s S, c E) S {
	r := make(S, len(s))
	for i, v := range s {
		r[i] = v * c
	}
	return r
}

// ScaleAndPrint doubles a Point and prints it.
func ScaleAndPrint(p Point) {
	r := Scale(p, 2)
	fmt.Println(r.String())
}

// Twice runs the constrained S back through ANOTHER generic function: Go infers the inner
// Scale's [S, E] through core types, but C# cannot re-infer a type parameter that appears only
// in constraints (CS0411 — the slices.Sort -> pdqsort chain), so the converter renders the
// resolved instantiation explicitly: `Scale<S, E>(s, 2)`.
func Twice[S ~[]E, E Integer](s S, c E) S {
	return Scale(s, c) // constrained S/E flow through: C# needs Scale<S, E>(s, c)
}

func main() {
	var p Point
	p = []int32{1, 2, 3}
	ScaleAndPrint(p)
	fmt.Println(Twice(Point{3, 4}, 2).String())
}