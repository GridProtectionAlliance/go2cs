// Guards the maps.Clone runtime intrinsic: a bodyless function carrying
// `//go:linkname <local> maps.clone` (the exact shape maps.clone pulls
// runtime.mapclone through) must convert to a forwarder that calls golib's
// `mapclone` builtin, NOT a throwing PartialStubGenerator stub. The clone must
// be a shallow copy with an INDEPENDENT backing store: mutating the clone
// (assign, add, delete) must not affect the original.
package main

import (
	"fmt"
	_ "unsafe"
)

//go:linkname clone maps.clone
func clone(m any) any

func main() {
	m := map[string]int{"a": 1, "b": 2, "c": 3}

	mc := clone(m).(map[string]int)

	// Mutate the clone: overwrite, add, delete.
	mc["a"] = 99
	mc["d"] = 4
	delete(mc, "b")

	// The original must be untouched (independent backing store).
	fmt.Println("orig len:", len(m), "clone len:", len(mc))
	fmt.Println("orig a:", m["a"], "clone a:", mc["a"])

	_, origHasD := m["d"]
	_, origHasB := m["b"]
	fmt.Println("orig has d:", origHasD, "orig has b:", origHasB)

	_, cloneHasB := mc["b"]
	fmt.Println("clone c:", mc["c"], "clone d:", mc["d"], "clone has b:", cloneHasB)
}
