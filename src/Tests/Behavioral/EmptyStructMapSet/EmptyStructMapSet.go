// Regression test for an empty-struct (`struct{}{}`) composite literal assigned to a map
// element, and for an anonymous `map[K]struct{}` ("set") parameter.
//
// The converter lifted EVERY `struct{}` composite to a dynamic named type via convStructType.
// For `seen[k] = struct{}{}` the enclosing assignment passes the LHS ident (`seen`) as context,
// so the lift was both named after the variable (`<func>_seen`) AND registered the variable's
// OWN type — the *map* `map[K]struct{}` — in the lifted-type map. That poisoned every later
// reference to that map type: the function parameter's signature became the phantom struct
// (instead of `map<K, EmptyStruct>`), its comma-ok deconstruction and two-arg indexer vanished
// (CS8130/CS0021), and call sites passing a real map mismatched (CS1503). An empty struct must
// never be lifted; it maps to golib's shared `EmptyStruct`.
package main

import "fmt"

// add takes an anonymous map[int]struct{} ("set") parameter and inserts via an empty-struct
// composite literal assigned to a map element.
func add(seen map[int]struct{}, k int) {
	seen[k] = struct{}{}
}

// contains uses the comma-ok form over the same anonymous-map parameter type.
func contains(seen map[int]struct{}, k int) bool {
	_, ok := seen[k]
	return ok
}

// entry/abbrevTable: a NAMED map type (dwarf's `type abbrevTable map[uint32]abbrev`)
// indexed with the comma-ok form. The two-value indexer detection asserted the concrete
// *types.Map, so the NAMED type emitted a single-value index under a (v, ok)
// deconstruction (CS8129 Deconstruct on the struct element / CS8130).
type entry struct {
	tag  string
	size int
}

type registry map[uint32]entry

func lookup(reg registry, id uint32) (string, bool) {
	e, ok := reg[id]
	if !ok {
		return "missing", false
	}
	return e.tag, true
}

func main() {
	seen := make(map[int]struct{})

	add(seen, 3)
	add(seen, 7)
	add(seen, 3) // duplicate; set semantics keep len at 2

	fmt.Println("len:", len(seen)) // len: 2

	// Membership in a fixed order (map iteration order is random in Go).
	for _, k := range []int{1, 3, 5, 7} {
		fmt.Printf("contains(%d) = %t\n", k, contains(seen, k))
	}

	// A locally constructed set literal with empty-struct values.
	lit := map[string]struct{}{
		"a": {},
		"b": {},
	}
	lit["c"] = struct{}{}
	fmt.Println("lit len:", len(lit)) // lit len: 3

	for _, s := range []string{"a", "b", "c", "d"} {
		_, ok := lit[s]
		fmt.Printf("lit[%s] = %t\n", s, ok)
	}

	reg := registry{2: {tag: "leaf", size: 8}}
	t1, ok1 := lookup(reg, 2)
	t2, ok2 := lookup(reg, 9)
	fmt.Println(t1, ok1, t2, ok2) // leaf true missing false

	// A TYPE ASSERT to `chan struct{}` (context's cancelCtx done channel): the assert's
	// type render must not re-sanitize the machinery's own EmptyStruct into the
	// reserved-Δ form (`channel<ΔEmptyStruct>`, CS0246 ×4).
	done := make(chan struct{}, 1)
	var anyDone any = done
	ch, chOK := anyDone.(chan struct{})
	ch <- struct{}{}
	<-done
	fmt.Println("chan assert:", chOK, len(done)) // chan assert: true 0
}
