// UntypedNestedSliceComposite guards an UNTYPED (type-inferred) composite literal whose
// inferred type is a slice, array, or pointer-to-struct — a `{…}` with no explicit type
// node. The converter emitted a target-typed `new(…)` ctor (correct for a struct element),
// but that is wrong for: a slice/array element (`new slice<rank>(a, b)` is CS1729 — no
// element-list ctor → use `new rank[]{…}.slice()`), and a `*Struct` element (the `[]*T{ {…} }`
// shorthand for `&T{…}` — `new(…)` targets the box `ж<T>`, whose ctor lacks the fields, CS1739
// → use `Ꮡ(new T(field: val, …))`). Mirrors runtime/lockrank.go's `lockPartialOrder` and
// runtime1.go's `dbgvars`.
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

type dbgVar struct {
	name  string
	value *int32
}

var x int32 = 7

// untyped *Struct composites (the []*T{ {…} } shorthand for &T{…})
var dbgvars = []*dbgVar{
	{name: "a", value: &x},
	{name: "b", value: &x},
}

func main() {
	fmt.Println(len(order[rA]), len(order[rB]), len(order[rC])) // 0 1 2
	fmt.Println(order[rC][0], order[rC][1])                     // 0 1
	fmt.Println(grid[0][1], grid[1][0])                         // 2 3
	fmt.Println(dbgvars[0].name, *dbgvars[1].value, len(dbgvars)) // a 7 2
}
