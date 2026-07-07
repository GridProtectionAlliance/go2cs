// UntypedNestedSliceComposite guards an UNTYPED (type-inferred) composite literal whose
// inferred type is a slice, array, or pointer-to-struct — a `{…}` with no explicit type
// node. The converter emitted a target-typed `new(…)` ctor (correct for a struct element),
// but that is wrong for: a slice/array element (`new slice<rank>(a, b)` is CS1729 — no
// element-list ctor → use `new rank[]{…}.slice()`), and a `*Struct` element (the `[]*T{ {…} }`
// shorthand for `&T{…}` — `new(…)` targets the box `ж<T>`, whose ctor lacks the fields, CS1739
// → use `Ꮡ(new T(field: val, …))`). Mirrors runtime/lockrank.go's `lockPartialOrder` and
// runtime1.go's `dbgvars`.
//
// It ALSO guards the KEYED variant of the untyped nested composite: an inner `{key: value}`
// whose inferred type is a slice or array must emit a golib SparseArray (`{[k] = v}.slice()` /
// `.array()`), NOT the plain `new T[]{ key: value }` array-initializer, which cannot take Go's
// `key: value` keyed syntax (CS1003 ×62 in x/net/idna's `joinStates`, the motivating case).
package main

import "fmt"

type rank int

const (
	rA rank = iota
	rB
	rC
)

// keyed slice-of-slices; each value is an untyped []rank composite (POSITIONAL inner)
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

type js int // a "join state" analog; named-over-int, so keys take the (int) cast

const (
	j0 js = iota
	j1
	j2
	j3
	numJS
)

// jsTable mirrors x/net/idna's joinStates: a keyed slice of KEYED arrays. The inner
// `{j1: …, j3: …}` is an untyped KEYED ARRAY literal — the fix emits a golib SparseArray
// (`{[j1] = …}.array()`), not `new js[]{ j1: … }` (the CS1003 cascade). Each inner sets its
// top index (j3) so the SparseArray-materialized dense length equals [numJS]js.
var jsTable = [][numJS]js{
	j0: {j1: j2, j3: j1},
	j2: {j0: j3, j3: j2},
}

// sparseRows: a POSITIONAL slice of KEYED (sparse) SLICE literals. A sparse []T is length
// max-key+1, which SparseArray.slice() materializes exactly (unlike a fixed array).
var sparseRows = [][]string{
	{2: "two", 0: "zero"},
	{1: "one"},
}

func main() {
	fmt.Println(len(order[rA]), len(order[rB]), len(order[rC]))   // 0 1 2
	fmt.Println(order[rC][0], order[rC][1])                       // 0 1
	fmt.Println(grid[0][1], grid[1][0])                           // 2 3
	fmt.Println(dbgvars[0].name, *dbgvars[1].value, len(dbgvars)) // a 7 2
	// inner KEYED array (idna joinStates shape): set + in-range unset (zero) indices
	fmt.Println(jsTable[j0][j1], jsTable[j0][j2], jsTable[j0][j3]) // 2 0 1
	fmt.Println(jsTable[j2][j0], jsTable[j2][j3])                  // 3 2
	// inner KEYED slice (sparse, exact length)
	fmt.Println(sparseRows[0][0], sparseRows[0][2], len(sparseRows[0])) // zero two 3
	fmt.Println(sparseRows[1][1], len(sparseRows[1]))                   // one 2
}
