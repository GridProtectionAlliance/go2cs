// GlobalCapturedInClosure guards references to a package GLOBAL from inside a closure.
// A global is a C# static, accessed LIVE — it must never be snapshot-captured: a value
// snapshot (`var gʗ1 = g`) copies the struct (so `&gʗ1` has no box → CS0103, and writes
// through the global are lost) and is semantically wrong (Go reads/writes the live
// global). For an address-taken (heap-boxed) global, the closure references the static
// box `Ꮡmheap` directly. Mirrors runtime's `systemstack(func(){ span = mheap_.alloc() })`.
package main

import "fmt"

type heap struct {
	count int
}

func (h *heap) alloc() int { h.count++; return h.count }

var mheap heap

//go:noinline
func keep(h *heap) { _ = h } // takes &mheap so it is heap-boxed

//go:noinline
func run(f func()) { f() }

func main() {
	keep(&mheap)
	var got int
	run(func() {
		got = mheap.alloc() // pointer-receiver method on the boxed global, inside the closure
		p := &mheap.count   // address of a field of the boxed global, inside the closure
		*p += 10
	})
	fmt.Println(got, mheap.count) // 1 11
}
