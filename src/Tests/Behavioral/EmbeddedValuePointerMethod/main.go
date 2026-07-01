// EmbeddedValuePointerMethod guards a POINTER-receiver method PROMOTED through a VALUE (struct)
// embedded field — the runtime `timeTimer` shape (timeTimer embeds `timer` BY VALUE; newTimer /
// stopTimer / resetTimer call the promoted `(*timer).modify/stop/reset` on a `*timeTimer`). Because
// the promoted method has a POINTER receiver, Go auto-takes `&outer.inner`, so the mutation MUST
// write through to the REAL embedded field, not a copy. The converter routes the receiver through the
// embedded field's box — `o.of(outer.Ꮡinner).method(...)` — reached via the &-machinery. This test
// FAILS (prints 0s) if that boxed a copy (lost writes) — the pallocBits/S5 trap. It exercises both
// receiver forms: a pointer LOCAL (`o := &outer{}`, the newTimer shape) and a deref'd pointer PARAM
// (`viaParam(o *outer)`, the stopTimer shape). A pointer embed is a separate case
// (PointerEmbeddingPromotion) that the converter leaves to the generated forwarder.
package main

import "fmt"

type inner struct {
	n int
}

func (i *inner) bump(d int) { i.n += d } // pointer-receiver: MUTATES the embedded value
func (i *inner) reset0()    { i.n = 0 }
func (i *inner) total() int { return i.n }

type outer struct {
	tag int
	inner
}

//go:noinline
func viaParam(o *outer) { o.bump(100) } // deref'd-*outer param (stopTimer/resetTimer shape)

func main() {
	o := &outer{tag: 7} // pointer local (newTimer `t := new(timeTimer)` shape)
	o.bump(5)           // promoted: (&o.inner).bump(5)
	o.bump(3)
	viaParam(o) // promoted via deref'd param

	fmt.Println(o.total()) // 108 — promoted read reflects the writes
	fmt.Println(o.n)       // 108 — promoted field access agrees
	fmt.Println(o.inner.n) // 108 — explicit embedded access agrees (write-through confirmed)
	fmt.Println(o.tag)     // 7 — sibling field untouched
	o.reset0()             // promoted mutate to 0
	fmt.Println(o.n)       // 0
}
