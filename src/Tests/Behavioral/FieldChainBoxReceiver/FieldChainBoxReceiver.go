package main

import "fmt"

// counter's methods take the address of the receiver (ptr returns it), so counter's methods are
// emitted with a box (ж<counter>) receiver — "direct-ж". Calling inc() on a value field-chain
// therefore needs the chain's real storage boxed, not a copy (which would lose the write).
type counter struct{ n int64 }

func (c *counter) ptr() *counter { return c }
func (c *counter) inc()          { c.ptr().n++ }

type holder struct{ c counter }
type box struct{ h holder }

// param root: b.h.c is a value field-chain rooted at the pointer PARAMETER b.
func viaParam(b *box) { b.h.c.inc() }

type wrapper struct{ b box }

// self() returns the receiver address, so wrapper's methods are direct-ж (box receiver `Ꮡw`).
func (w *wrapper) self() *wrapper { return w }

// receiver root: w.b.h.c is a value field-chain rooted at the (direct-ж) pointer RECEIVER w.
func (w *wrapper) bump() {
	_ = w.self()
	w.b.h.c.inc()
}

func main() {
	b := &box{}
	viaParam(b)
	viaParam(b)
	fmt.Println(b.h.c.n) // 2 — write-through the param-root box must persist

	w := &wrapper{}
	w.bump()
	w.bump()
	w.bump()
	fmt.Println(w.b.h.c.n) // 3 — write-through the receiver-root box must persist
}
