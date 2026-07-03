// NestedFieldPointerAssign guards assignment to a NESTED field through a pointer
// LOCAL variable. The base of such an assignment must use the assignable `.Value`
// dereference throughout the chain, not the value-returning `~` operator: a
// `(~o).stack.hi = …` (the inner `o.stack` deref via `~`) is not a variable or
// property, so it cannot be assigned to (CS0131). Mirrors runtime/cgocall.go's
// `g0.stack.hi = sp + 1024` where g0 is a `*g` local.
package main

import "fmt"

type inner struct {
	hi, lo uintptr
}

type outer struct {
	stack inner
	guard uintptr
}

//go:noinline
func get(o *outer) *outer { return o }

func main() {
	var base outer
	o := get(&base) // o is a *outer LOCAL (a heap box, not a ref-aliased parameter)

	o.stack.hi = 100 // nested field assignment through the pointer local
	o.stack.lo = 50
	o.guard = 5 // direct field assignment through the pointer local

	// Read back through the original value to confirm the writes reached real storage.
	fmt.Println(base.stack.hi, base.stack.lo, base.guard) // 100 50 5
}
