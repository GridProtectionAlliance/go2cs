// EmbeddedValuePointerMethod guards a POINTER-receiver method PROMOTED through a VALUE (struct)
// embedded field — the runtime `timeTimer` shape (timeTimer embeds `timer` BY VALUE; newTimer /
// stopTimer / resetTimer call the promoted `(*timer).modify/stop/reset` on a `*timeTimer`). Because
// the promoted method has a POINTER receiver, Go auto-takes `&outer.inner`, so the mutation MUST
// write through to the REAL embedded field, not a copy. The converter routes the receiver through the
// embedded field's box — `o.of(outer.Ꮡinner).method(...)` — reached via the &-machinery. This test
// FAILS (prints 0s) if that boxed a copy (lost writes) — the pallocBits/S5 trap. It exercises three
// receiver forms: a pointer LOCAL (`o := &outer{}`, the newTimer shape), a deref'd pointer PARAM
// (`viaParam(o *outer)`, the stopTimer shape), and the enclosing method's OWN `[GoRecv] ref`
// receiver (`(c *chunk) alloc` calling the promoted `c.set(...)` — the runtime mgcscavenge
// `(*scavChunkData).alloc/free` → `sc.setEmpty()` shape). The receiver form has NO box (`Ꮡc` does
// not exist — the &-descent emitted CS0103), and needs none: the embedded field of a `ref`
// receiver is addressable, so the promoted `[GoRecv] ref` overload binds on the explicit field
// call `c.flags.set(...)`. An inner pointer local SHADOWING the receiver name must keep the
// box-descent path (`cΔ1.of(chunk.Ꮡflags)`) — the rendered==raw guard. A pointer embed is a
// separate case (PointerEmbeddingPromotion) that the converter leaves to the generated forwarder.
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

type flags uint8

func (f *flags) set(bit flags)   { *f |= bit }
func (f *flags) clear(bit flags) { *f &^= bit }

type chunk struct {
	inUse uint16
	flags // embedded named-numeric VALUE
}

// a plain pointer-receiver method (no direct-ж trigger — its receiver renders `this ref chunk`
// with NO box) calling the EMBEDDING-PROMOTED pointer-receiver method on its own receiver: the
// runtime `(*scavChunkData).alloc` → `sc.setEmpty()` shape. Emits the explicit field call
// `c.flags.set(...)` (addressable through the ref receiver), which must write through.
func (c *chunk) alloc(n uint16) {
	c.inUse += n
	if c.inUse >= 4 {
		c.set(1)
	}
}

func (c *chunk) free(n uint16) {
	c.inUse -= n
	c.clear(1)
}

// shadow control: an inner *chunk pointer LOCAL shadowing the receiver name must NOT take the
// receiver arm — a pointer local is already the box, so the promoted call keeps the descent
// (`cΔ1.of(chunk.Ꮡflags).set(...)`), and the write goes through the pointer to its target.
func (c *chunk) bump(other *chunk) {
	{
		c := other // shadows the receiver
		c.set(2)
	}
}

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

	// the [GoRecv]-ref-receiver form: promoted call inside the outer type's own pointer-receiver
	// method, write-through observed via the original value. (Composite-literal construction —
	// the documented zero-value caveat: a default-constructed embedding struct has no embedded
	// box, so promoted access NREs; see NestedEmbeddingPromotion.)
	c := chunk{}
	c.alloc(5)
	fmt.Println(c.inUse, uint8(c.flags)) // 5 1 — the promoted set() reached the embedded field
	c.free(2)
	fmt.Println(c.inUse, uint8(c.flags)) // 3 0 — the promoted clear() too

	// the shadow control: the inner pointer local writes through to ITS target, not the receiver
	d, e := chunk{}, chunk{}
	d.bump(&e)
	fmt.Println(uint8(d.flags), uint8(e.flags)) // 0 2
}
