package main

import (
	"fmt"

	"golang.org/x/exp/constraints"
)

type Point []int32

func (p Point) String() string {
	return fmt.Sprintf("%d", p)
}

// Scale returns a copy of s with each element multiplied by c.
func Scale[S ~[]E, E constraints.Integer](s S, c E) S {
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

func main() {
	var p Point
	p = []int32{1, 2, 3}
	ScaleAndPrint(p)
}