// UntypedNestedSliceComposite guards an UNTYPED (type-inferred) composite literal whose
// inferred type is a slice or array — the inner `{…}` of a `[][]rank{ key: {…} }`. Such a
// literal has no explicit type node, so the converter emitted a target-typed `new(…)` ctor
// (correct for a struct element, but `new slice<rank>(a, b)` is CS1729 — a slice has no
// element-list constructor). It must instead use the element-array projection,
// `new rank[]{…}.slice()`. Mirrors runtime/lockrank.go's `lockPartialOrder`.
package main

import "fmt"

type rank int

const (
	rA rank = iota
	rB
	rC
)

// keyed slice-of-slices; each value is an untyped []rank composite
var order = [][]rank{
	rA: {},
	rB: {rA},
	rC: {rA, rB},
}

// untyped array composite nested in a typed slice
var grid = [][2]int{
	{1, 2},
	{3, 4},
}

func main() {
	fmt.Println(len(order[rA]), len(order[rB]), len(order[rC])) // 0 1 2
	fmt.Println(order[rC][0], order[rC][1])                     // 0 1
	fmt.Println(grid[0][1], grid[1][0])                         // 2 3
}
