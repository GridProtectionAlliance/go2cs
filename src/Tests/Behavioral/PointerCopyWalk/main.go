// Regression test for copying a pointer parameter into a local and walking a linked structure.
//
// Go `r := start` where start is a *T is a pointer copy: r is a *T. The converter deref's a
// pointer parameter to a value alias (`ref var start = ref Ꮡstart.val`), so `r := start` used
// to emit `var r = start` — copying the pointed-to *value*, making r a T, not a *T. The rest
// of the converter already treats such a walked variable as a pointer (`r.next`, `r = r.next`),
// so the value-copy form failed to compile. The fix emits the pointer (box) form
// `var r = Ꮡstart`, so r is a ж<T>.
package main

import "fmt"

type node struct {
	val  int
	next *node
}

// `r := start` (pointer copy) then walk to the tail.
func last(start *node) *node {
	r := start
	for r.next != nil {
		r = r.next
	}
	return r
}

// pointer copy in a for-init clause, reassigned each iteration.
func length(start *node) int {
	n := 0
	for r := start; r != nil; r = r.next {
		n++
	}
	return n
}

// sum via an explicit pointer local seeded from the parameter.
func sum(start *node) int {
	total := 0
	p := start
	for p != nil {
		total += p.val
		p = p.next
	}
	return total
}

func main() {
	third := &node{val: 3}
	second := &node{val: 2, next: third}
	first := &node{val: 1, next: second}

	fmt.Println(last(first).val) // 3
	fmt.Println(length(first))   // 3
	fmt.Println(sum(first))      // 6
}
