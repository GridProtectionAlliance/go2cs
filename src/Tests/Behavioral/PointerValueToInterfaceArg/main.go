// Regression test: a deref-aliased pointer (a *T PARAMETER or a pointer METHOD RECEIVER)
// passed WHOLE to an EMPTY-interface (`any`) parameter must keep its pointer identity — the
// converter must emit the box `Ꮡp`, not the deref'd value alias `p`. Losing the box stores the
// pointed-to VALUE, so a later `.(*T)` type assertion sees a value and panics.
//
// This is the fmt `func (p *pp) free() { … ppFree.Put(p) }` shape: p (a *pp) is Put into a
// sync.Pool (Put(x any)); the next newPrinter() does Get().(*pp). With the box dropped the pool
// held a pp VALUE and the 2nd round-trip panicked ("interface conversion: … is pp, not *pp").
// A minimal free list stands in for sync.Pool (the baseline core has no sync.Pool).
package main

import "fmt"

type pp struct {
	id  int
	buf []byte
}

var freeList []any

func poolPut(x any) { freeList = append(freeList, x) } // x is the EMPTY interface

func poolGet() any {
	if n := len(freeList); n > 0 {
		x := freeList[n-1]
		freeList = freeList[:n-1]
		return x
	}
	return new(pp)
}

func newPrinter() *pp {
	p := poolGet().(*pp) // panics on the 2nd call if free() stored a VALUE, not the box
	p.id++
	return p
}

// pointer RECEIVER passed WHOLE to an `any` parameter.
func (p *pp) free() {
	p.buf = p.buf[:0]
	poolPut(p)
}

// pointer PARAMETER passed WHOLE to an `any` parameter (a second position the box can be lost).
func keep(q *pp) { poolPut(q) }

func main() {
	a := newPrinter() // 1st Get: new(pp); id -> 1
	a.buf = append(a.buf, 'a')
	fmt.Println(string(a.buf), a.id) // a 1
	a.free()                         // poolPut(a): stores the *pp box

	b := newPrinter() // 2nd Get: returns what free() stored — must still be *pp
	b.buf = append(b.buf, 'b')
	fmt.Println(string(b.buf), b.id) // b 2  (a reused: buf reset, id 1 -> 2)
	b.free()

	// pointer PARAMETER round-trip: identity must survive the `any` hop.
	c := &pp{id: 100}
	keep(c)                // poolPut(c): stores the *pp box
	got := poolGet().(*pp) // must be c (same pointer), not a copy
	got.id = 200
	fmt.Println(c.id) // 200 — proves got IS c
}
