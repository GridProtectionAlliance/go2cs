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
//
// Also: a pointer parameter reassigned from a TUPLE-returning call — `(p, skipped) = advance(p)`
// (runtime proc.go's `pp, _ = pidleget(0)` and mgcstack.go's `(left, x, idx) = binarySearchTree(…)`
// shapes). The single-assign box-reassignment triggers gated element-wise on the RHS, so a tuple
// deconstruction assigned the ж<T> component into the deref'd VALUE alias (CS0029). The LHS now
// emits the box with the same nil-safe re-alias the single-assign path uses:
// `(Ꮡp, skipped) = advance(Ꮡp); p = ref Ꮡp.DerefOrNil();`.
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

// advance returns the next node and the value stepped over — a tuple with a pointer component.
//
//go:noinline
func advance(p *node) (*node, int) {
	return p.next, p.val
}

// sumEveryOther walks the parameter by reassigning it FROM A TUPLE (the proc.go/mgcstack.go
// shape), nil-compared so the re-alias must stay nil-safe on the final step; the skipped value
// from the tuple is consumed too.
func sumEveryOther(p *node) int {
	total := 0
	skipped := 0
	for p != nil {
		total += p.val
		p, skipped = advance(p)
		_ = skipped
		if p != nil {
			p, skipped = advance(p) // step again: sum positions 0, 2, ...
			total += skipped * 0    // keep skipped genuinely used
		}
	}
	return total
}

// bumpFirstViaTuple reassigns the parameter from a tuple, then MUTATES through it — proving the
// re-aliased value var aliases the real repointed box, not a copy.
func bumpFirstViaTuple(p *node) {
	var skipped int
	p, skipped = advance(p)
	_ = skipped
	if p != nil {
		p.val += 100
	}
}

func main() {
	list := build(1, 2, 3, 4)
	fmt.Println(sumList(list)) // 10
	doubleList(list)           // mutate through the *node parameter
	fmt.Println(sumList(list)) // 20 (mutations visible through the original boxes)
	fmt.Println(sumList(nil))  // 0  (empty list: entry deref of a nil argument is nil-safe)

	// tuple-reassign walk: sums positions 0 and 2 of [2 4 6 8] = 8; nil-safe on the final step
	fmt.Println(sumEveryOther(list)) // 8
	fmt.Println(sumEveryOther(nil))  // 0

	// tuple-reassign then mutate-through: node 1 (value 4) gets +100, visible via the original
	bumpFirstViaTuple(list)
	fmt.Println(sumList(list)) // 120
}
