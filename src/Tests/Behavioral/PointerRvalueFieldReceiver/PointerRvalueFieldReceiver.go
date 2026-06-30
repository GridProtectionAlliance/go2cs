package main

import "fmt"

// slot is a value type with a POINTER-receiver method `set` — exactly like runtime's guintptr.set
// (`func (gp *guintptr) set(...)`). A [GoRecv] ref method needs an ADDRESSABLE receiver.
type slot struct{ v int }

func (s *slot) set(x int) { s.v = x }

// node has slot as a VALUE field `s`. Reaching `s` through a pointer-returning RVALUE — a CALL or a
// pointer-ELEMENT index — makes `(~rvalue).s` an rvalue, so `.set(...)` cannot bind unless the
// receiver is materialized through the field's real box (`rvalue.of(node.Ꮡs)`). This mirrors
// runtime's `(~getg()).schedlink.set(…)` / `(~batch[i]).schedlink.set(…)` / `(~q.tail.ptr()).schedlink.set(…)`.
type node struct {
	s slot
}

var theNode = &node{}

// getNode returns a pointer — the CALL-root case: getNode().s.set(x).
func getNode() *node { return theNode }

// queue.tail returns a pointer via a method call — the METHOD-CALL-root case: q.tail().s.set(x).
type queue struct{ last *node }

func (q *queue) tail() *node { return q.last }

func main() {
	// 1. CALL root: getNode().s is a value field of a pointer-returning call.
	getNode().s.set(42)
	fmt.Println(theNode.s.v) // 42 — the write must persist through the materialized box

	// 2. INDEX root (pointer element): nodes[i].s is a value field of a pointer element.
	a := &node{}
	b := &node{}
	nodes := []*node{a, b}
	nodes[0].s.set(7)
	nodes[1].s.set(9)
	fmt.Println(a.s.v, b.s.v) // 7 9

	// 3. METHOD-CALL root: q.tail().s reaches the value field through a returned pointer (same node a).
	q := &queue{last: a}
	q.tail().s.set(100)
	fmt.Println(a.s.v) // 100 — overwritten via the queue path
}
