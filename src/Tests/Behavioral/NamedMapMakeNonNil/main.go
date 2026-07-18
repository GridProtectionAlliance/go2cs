// Regression guard: `make` of a DEFINED map type with NO size argument must yield a
// NON-nil empty map, exactly like Go.
//
// In go2cs a defined map type (`type StrIntMap map[string]int`) becomes a generated
// wrapper struct with an allocating `(nint size)` constructor but NO parameterless one.
// The converter used to emit `make(StrIntMap)` (no size) as `new StrIntMap()`, which is
// default(StrIntMap) — a NIL map (its backing store is null), so `m == nil` was wrongly
// true. Go's `make` returns a non-nil empty map, so `m == nil` must be false. The fix
// defaults the size to 0 (`new StrIntMap(0)`), running the allocating constructor.
package main

import "fmt"

type StrIntMap map[string]int

func main() {
	// make with NO size -> non-nil empty map
	m := make(StrIntMap)
	fmt.Println(m == nil) // false
	fmt.Println(len(m))   // 0
	m["a"] = 1            // writable (a nil map would panic here)
	fmt.Println(m["a"])   // 1

	// make WITH an explicit zero size -> also non-nil
	m2 := make(StrIntMap, 0)
	fmt.Println(m2 == nil) // false

	// a plain var (no make) IS the nil map
	var z StrIntMap
	fmt.Println(z == nil) // true
	fmt.Println(len(z))   // 0

	// a named-map composite literal is non-nil too
	e := StrIntMap{}
	fmt.Println(e == nil) // false
}
