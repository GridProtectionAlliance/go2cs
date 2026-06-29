package main

import "fmt"

// A function-LOCAL named type (`type entry struct{…}`) is lifted to a package-level struct named
// `<func>_entry` (intra-function type declarations aren't allowed in C#). References to it as a
// bare identifier are renamed, but using it as a slice/array ELEMENT (`[]entry`) was rendered via
// the type system, which kept the short name `slice<entry>` — unresolved at package scope (CS0246).
// The lifted name must be resolved for the element too: `slice<process_entry>`.
func process() int {
	type entry struct {
		id  int
		val int
	}
	var entries []entry
	entries = append(entries, entry{1, 10})
	entries = append(entries, entry{2, 20})
	entries = append(entries, entry{3, 30})

	total := 0
	for _, e := range entries {
		total += e.val * e.id
	}
	return total
}

// arr exercises the array (fixed-size) form of a function-local element type.
func arr() int {
	type pair struct{ a, b int }
	var ps [2]pair
	ps[0] = pair{1, 2}
	ps[1] = pair{3, 4}
	return ps[0].a + ps[0].b + ps[1].a + ps[1].b
}

func main() {
	fmt.Println(process(), arr())
}
