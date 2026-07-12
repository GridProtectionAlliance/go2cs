package main

import (
	"fmt"
	"sync/atomic"
)

// Exercises sync/atomic.Value (Load/Store/Swap/CompareAndSwap). The zero Value loads nil; Store then
// Load round-trips; Swap returns the prior value; CompareAndSwap succeeds only when the current value
// equals old (by value). Guards the native atomic.Value reimplementation — the converted Go form
// manipulated an interface's (type,data) words via unsafe.Pointer, which NREs in the managed world.
// Uses typed string variables and equality checks (not literals/assertions) so the guard stays on
// atomic.Value and off the untyped-constant boxing path.
func main() {
	var v atomic.Value
	fmt.Println(v.Load() == nil)

	a := "alpha"
	b := "beta"
	c := "gamma"

	v.Store(a)
	fmt.Println(v.Load() == a)

	old := v.Swap(b)
	fmt.Println(old == a, v.Load() == b)

	fmt.Println(v.CompareAndSwap(b, c), v.Load() == c)
	fmt.Println(v.CompareAndSwap(b, a), v.Load() == c)
}
