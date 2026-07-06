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

// receiver stored AS A POINTER ELEMENT of a slice composite literal (`[]*ring{r}`) — the
// internal/trace summary.Descendents() shape. The receiver must render as its box `Ꮡr` so the
// stored element is the real receiver, not a value copy (CS0029 otherwise).
func (r *ring) chain(n int) []*ring {
	nodes := []*ring{r}
	p := r
	for i := 0; i < n; i++ {
		p = p.next
		nodes = append(nodes, p)
	}
	return nodes
}

// receiver stored into an ARRAY-of-pointers composite literal (`[2]*ring{r, other}`).
func (r *ring) pair(other *ring) [2]*ring {
	return [2]*ring{r, other}
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

	// slice-of-pointers composite literal: chain[0] IS b (the stored box), not a copy — mutate
	// through it and read back through b.
	chain := b.chain(2)              // [b, c, b]
	chain[0].data = 7
	fmt.Println(b.data)              // 7
	fmt.Println(chain[1].data)       // 3 (c)

	// array-of-pointers composite literal
	arr := b.pair(c)
	fmt.Println(arr[0].data, arr[1].data) // 7 3
}
