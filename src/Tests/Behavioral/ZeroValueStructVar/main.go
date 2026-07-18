package main

import "fmt"

// Guards the `var z T` zero-value shape (a plain declaration, NO composite literal) for a struct
// whose default(T) is BROKEN. Go zeroes a fixed-size array field to its length N, so ranging /
// indexing / len must work. The converter previously emitted `holder z = default!;`, and default!
// SKIPS the field initializer the converter emits for the array field (`array<nint> tbl = new(8)`),
// leaving a null backing (len 0, or NRE on index). The fix emits the generated parameterless
// constructor (`new()`) instead, which runs the field initializers + AppendZeroValueInitializers —
// mirroring go2cs-gen StructTypeTemplate.NeedsConstruction. A NESTED value-struct field carrying
// its own array must construct too (recursive), while a SCALAR-only struct must NOT construct
// (its zero value stays default!). Sibling of ZeroValueArrayField, which covers the
// composite-literal-omission shape and deliberately excludes this `var z` shape.

type holder struct {
	name string
	tbl  [8]int
	tail []int
}

type wrapper struct {
	id int
	h  holder
}

type point struct {
	x, y int
}

func main() {
	// Core: `var z holder` — the [8]int field must have Go's zero-value length 8, and be
	// rangeable, indexable, and writable.
	var z holder
	fmt.Println(len(z.name), len(z.tbl), len(z.tail)) // 0 8 0

	for i := range z.tbl {
		z.tbl[i] = i * i
	}
	sum := 0
	for _, v := range z.tbl {
		sum += v
	}
	fmt.Println(sum, z.tbl[7]) // 140 49

	// Nested: `var w wrapper` — wrapper's holder field (and its [8]int backing) must construct
	// through the recursive AppendZeroValueInitializers, else w.h.tbl[3] NREs.
	var w wrapper
	w.h.tbl[3] = 42
	fmt.Println(w.id, len(w.h.tbl), w.h.tbl[3]) // 0 8 42

	// Control: a scalar-only struct needs NO construction — its zero value stays default! (the
	// golden proves the predicate does not over-fire and churn every struct var).
	var p point
	p.x = 3
	fmt.Println(p.x + p.y) // 3
}
