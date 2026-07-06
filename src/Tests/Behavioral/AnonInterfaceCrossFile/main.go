// File B: implements the two interfaces on a pointer receiver and calls `describe` (declared
// in file A) passing the concrete *box. The *box → anon-interface cast must reference the
// SAME lifted name file A registered — the raw Go literal `interface{ Sizer; Namer }` would
// break the emitted GoImplement attribute and the adapter class name (a stray `}`), the
// internal/trace CS1730 cascade. Because file visit order is not guaranteed, this exercises
// the deferred cross-file resolution (registry lookup / post-barrier marker).
package main

import "fmt"

type box struct {
	label string
	n     int
}

func (b *box) Size() int {
	return b.n
}

func (b *box) Name() string {
	return b.label
}

func main() {
	b := &box{label: "widget", n: 42}
	fmt.Println(describe(b)) // widget(42)

	c := &box{label: "gadget", n: 7}
	fmt.Println(describe(c)) // gadget(7)
}
