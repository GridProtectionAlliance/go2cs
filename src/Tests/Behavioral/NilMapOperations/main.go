// Regression test for nil-map semantics.
//
// In go2cs a Go map is a `readonly struct map<K,V>` whose backing dictionary is null when
// the map is nil (`var m map[K]V`). Several members dereferenced that backing store directly,
// so the Go-valid operations on a nil map threw NullReferenceException in the converted C#
// instead of behaving like Go:
//   - read of an absent key      -> zero value            (was NRE)
//   - comma-ok read              -> (zero, false)         (was NRE)
//   - len(nil)                   -> 0                      (was NRE)
//   - range over nil             -> zero iterations       (was NRE)
//   - delete(nil, k)             -> no-op                  (was NRE)
//   - m == nil                   -> true; empty != nil    (Count==0 wrongly treated empty as nil)
//   - write to nil (m[k] = v)    -> panic ("assignment to entry in nil map")
package main

import "fmt"

// writePanicked records whether the most recent tryWrite call panicked. A package-level
// flag (set inside the deferred recover) keeps the test focused on nil-map semantics rather
// than on the defer/recover return-value or ref-capture interactions.
var writePanicked bool

func tryWrite(m map[string]int) {
	writePanicked = false
	defer func() {
		if r := recover(); r != nil {
			writePanicked = true
		}
	}()
	m["x"] = 1
}

func main() {
	var m map[string]int // nil map

	// read absent key -> zero value
	fmt.Println(m["a"]) // 0

	// comma-ok read -> (zero, false)
	v, ok := m["a"]
	fmt.Println(v, ok) // 0 false

	// len(nil) -> 0
	fmt.Println(len(m)) // 0

	// range over nil -> no iterations
	count := 0
	for range m {
		count++
	}
	fmt.Println(count) // 0

	// delete(nil, k) -> no-op, no panic
	delete(m, "a")
	fmt.Println("delete ok") // delete ok

	// m == nil -> true for a nil map
	fmt.Println(m == nil) // true

	// an empty but non-nil map is NOT nil
	e := map[string]int{}
	fmt.Println(e == nil) // false
	fmt.Println(len(e))   // 0

	// write to nil map -> panics
	tryWrite(m)
	fmt.Println(writePanicked) // true

	// after a successful make, write works (no panic)
	tryWrite(map[string]int{})
	fmt.Println(writePanicked) // false
}
