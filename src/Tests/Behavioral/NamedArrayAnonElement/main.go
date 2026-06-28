package main

import "fmt"

type semaRoot struct{ n int }

// A NAMED array type whose element is an anonymous struct (runtime/sema's `semTable`). The element
// must be lifted to a named type so the `[GoType]` attribute and the `&t[i].field` address inside
// rootFor (which emits `.of(ElemType.ᏑField)`) reference a named type, not a raw `struct{…}`. The
// converter always emits rootFor, so the Compile/Target phases validate the lift compiles. (main
// does not index the array: zero-valuing a named fixed-size array type does not yet allocate its
// backing array — a separate concern.)
type semTable [4]struct {
	root semaRoot
	pad  [40]byte
}

func (t *semTable) rootFor(i int) *semaRoot {
	return &t[i].root
}

func main() {
	fmt.Println("named-array anon element compiles")
}
