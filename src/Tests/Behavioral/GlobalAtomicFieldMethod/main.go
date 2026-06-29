// GlobalAtomicFieldMethod guards a capture-mode (pointer-receiver) method called on
// a FIELD of an address-taken package global — e.g. `gcController.totalFree.Add(…)`
// in the Go runtime. Because the global has its address taken it is emitted as a heap
// box (`ж<T> ᏑG`), and the method needs the address of the field: `&G.field`. That
// must route through the box-field accessor `ᏑG.of(T.Ꮡfield)`; naively prefixing `Ꮡ`
// to `G.field` would instead bind to the box variable `ᏑG` (whose value has no such
// field) → CS1061.
package main

import (
	"fmt"
	"sync/atomic"
)

type controller struct {
	total atomic.Int64
}

var ctrl controller // package global

//go:noinline
func keep(c *controller) { _ = c } // takes &ctrl so ctrl is address-taken (heap-boxed)

func bump() {
	ctrl.total.Add(5) // global.atomicField.Method(): &ctrl.total boxed for the ptr receiver
}

func main() {
	keep(&ctrl)
	bump()
	bump()
	fmt.Println(ctrl.total.Load()) // 10
}
