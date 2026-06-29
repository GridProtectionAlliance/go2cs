package main

import "fmt"

type node struct {
	next *node
	val  int
}

// advance returns a POINTER FIELD of a pointer parameter. The result type is a pointer, but the
// returned value is the field `n.next`, not the parameter itself — so it must emit `n.next`, NOT
// `Ꮡn.next`. The bug: the return path used getIdentifier(expr), which digs through the selector
// to the root param `n` and wrongly boxed the whole expression (`Ꮡn.next`, where the box `Ꮡn` has
// no `next` member → CS1061). Mirrors the runtime's `reflect_mapiterkey(it *hiter) { return it.key }`
// where `key` is an unsafe.Pointer field.
func advance(n *node) *node {
	return n.next
}

// self returns the bare pointer PARAMETER whole — this DOES need the box (`Ꮡn`), since the value
// alias cannot bind the pointer result. This path must stay unchanged.
func self(n *node) *node {
	return n
}

func main() {
	c := &node{val: 3}
	b := &node{next: c, val: 2}
	a := &node{next: b, val: 1}

	fmt.Println(advance(a).val)          // b.val = 2
	fmt.Println(advance(advance(a)).val) // c.val = 3
	fmt.Println(self(a).val)             // a.val = 1
	fmt.Println(advance(c) == nil)       // c.next is nil -> true
}
