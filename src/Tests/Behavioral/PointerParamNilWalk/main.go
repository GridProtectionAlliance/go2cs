// Regression test for walking a *node parameter to a NIL terminator.
//
// A named pointer parameter is deref-aliased in C# to a value var (`ref var p = ref Ꮡp.val`)
// over its box `Ꮡp`. Two things must hold together for a nil-terminated walk
// (`for p != nil { …; p = p.next }`):
//   1. the loop guard must compare the BOX `Ꮡp != nil`, not the value alias `p` (a `node`
//      struct value) — emitting the value form compares the wrong thing; and
//   2. the in-loop re-alias after repointing the box (`Ꮡp = p.next; p = ref Ꮡp.…`) must be
//      nil-safe — on the final step `p.next` is nil, and the plain `.val` getter would throw a
//      nil-pointer dereference before the guard is re-checked.
// The converter detects a pointer parameter compared with nil and (a) emits its box in the
// comparison and (b) routes its deref/re-alias through the nil-safe `Ꮡp.DerefOrNil()` accessor,
// so the entry alias also tolerates a nil argument (the empty-list `sumList(nil)` case).
//
// Sibling PointerParamWalk covers the never-nil (CIRCULAR) param walk; this one covers the
// nil-terminated walk plus mutate-through-the-parameter.
package main

import "fmt"

type node struct {
	val  int
	next *node
}

// sumList walks the parameter to its nil terminator, summing values (read-through).
func sumList(p *node) int {
	total := 0
	for p != nil {
		total += p.val
		p = p.next
	}
	return total
}

// doubleList walks the parameter to its nil terminator, mutating each node THROUGH the pointer.
func doubleList(p *node) {
	for p != nil {
		p.val *= 2
		p = p.next
	}
}

// build links vals into a nil-terminated list and returns its head (nil for no values).
func build(vals ...int) *node {
	var head *node
	for i := len(vals) - 1; i >= 0; i-- {
		head = &node{val: vals[i], next: head}
	}
	return head
}

func main() {
	list := build(1, 2, 3, 4)
	fmt.Println(sumList(list)) // 10
	doubleList(list)           // mutate through the *node parameter
	fmt.Println(sumList(list)) // 20 (mutations visible through the original boxes)
	fmt.Println(sumList(nil))  // 0  (empty list: entry deref of a nil argument is nil-safe)
}
