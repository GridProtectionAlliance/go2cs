// Regression test for using a pointer receiver as a bare pointer value.
//
// A method like `func (r *ring) initSelf() { r.next = r }` assigns the receiver itself to a
// pointer field — i.e. it uses the receiver as a *T value, not just for field access. The
// converter deref's a value-ref receiver (`this ref ring r`) to a value, which has no pointer
// to hand out, so `r.next = r` assigned a ring value to a ж<ring> field (did not compile, and
// would point into a copy). The fix marks such methods direct-ж (`this ж<ring> Ꮡr`) and emits
// the receiver box `Ꮡr` when the receiver is used as a pointer value, so the field points at
// the real receiver. This is the self-referential-pointer-struct pattern from container/ring
// and container/list.
package main

import "fmt"

type ring struct {
	data int
	next *ring
}

// receiver assigned to its own pointer field (self-link)
func (r *ring) initSelf() {
	r.next = r
}

// receiver linked to another node
func (r *ring) linkTo(other *ring) {
	r.next = other
}

// walk n hops and return the node reached (exercises pointer-copy + receiver pointer use)
func (r *ring) advance(n int) *ring {
	p := r
	for i := 0; i < n; i++ {
		p = p.next
	}
	return p
}

func main() {
	a := &ring{data: 1}
	a.initSelf() // a.next = a

	// mutate through a; reading back through the self-link must see the change, proving
	// a.next points at the real a, not a copy.
	a.data = 42
	fmt.Println(a.next.data)         // 42
	fmt.Println(a.advance(5).data)   // 42 (self-loop, any hop count lands on a)

	b := &ring{data: 2}
	c := &ring{data: 3}
	b.linkTo(c)
	fmt.Println(b.next.data)         // 3
	c.linkTo(b)
	fmt.Println(b.advance(2).data)   // 2 (b -> c -> b)
}
