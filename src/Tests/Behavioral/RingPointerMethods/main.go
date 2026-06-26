// Regression test for the self-referential-pointer-method patterns from container/ring and
// container/list. Together these exercise every ж<T> sub-problem the converter must handle when
// a pointer receiver is used as a first-class pointer:
//
//   A         - receiver assigned to a pointer field        (r.next = r)
//   A-variant - receiver compared as a pointer              (p != r)
//   B         - pointer copy of a parameter / receiver      (p := r)
//   C         - calling a direct-ж method on the receiver   (return r.init())  [transitive direct-ж]
//   D         - reassigning the receiver pointer            (r = r.next)
//   E         - chained selector through a pointer field    (p.next.prev = e)
package main

import "fmt"

type Ring struct {
	next, prev *Ring
	Value      int
}

// A + return-receiver: self-link the element and hand back the receiver pointer.
func (r *Ring) init() *Ring {
	r.next = r
	r.prev = r
	return r
}

// C: calls the direct-ж init on the (value) receiver.
func (r *Ring) Next() *Ring {
	if r.next == nil {
		return r.init()
	}
	return r.next
}

func (r *Ring) Prev() *Ring {
	if r.next == nil {
		return r.init()
	}
	return r.prev
}

// D: walks the ring by reassigning the receiver pointer.
func (r *Ring) Move(n int) *Ring {
	if r.next == nil {
		return r.init()
	}
	for ; n < 0; n++ {
		r = r.prev
	}
	for ; n > 0; n-- {
		r = r.next
	}
	return r
}

// A-variant: compares the walked pointer p against the receiver r.
func (r *Ring) Len() int {
	n := 0
	if r != nil {
		n = 1
		for p := r.Next(); p != r; p = p.next {
			n++
		}
	}
	return n
}

// makeRing builds a ring of `count` elements with Values 0..count-1, exercising E
// (`p.next.prev` — a chained selector through the `next` pointer field).
func makeRing(count int) *Ring {
	if count <= 0 {
		return nil
	}
	r := &Ring{Value: 0}
	r.init()
	p := r
	for i := 1; i < count; i++ {
		e := &Ring{Value: i}
		e.prev = p
		e.next = p.next
		p.next.prev = e // E: p.next is *Ring, .prev is its field
		p.next = e
		p = e
	}
	return r
}

func main() {
	// single self-linked element
	a := &Ring{Value: 42}
	a.init()
	fmt.Println(a.Next().Value) // 42 (points to self)
	fmt.Println(a.Len())        // 1
	fmt.Println(a.Move(5).Value) // 42 (any move on a 1-ring lands on self)

	// 4-element ring laid out forward as 0 -> 1 -> 2 -> 3 -> 0
	r := makeRing(4)
	fmt.Println(r.Len())          // 4
	fmt.Println(r.Next().Value)   // 1
	fmt.Println(r.Move(2).Value)  // 2
	fmt.Println(r.Prev().Value)   // 3
	fmt.Println(r.Move(-1).Value) // 3 (one step backward from r)
}
