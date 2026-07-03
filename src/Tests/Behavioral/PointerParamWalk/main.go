package main

import "fmt"

type node struct {
	val  int
	next *node
}

// sumWalk REASSIGNS its `*node` parameter `p` to step through a (circular) list, reading the
// pointed-to value each step (`total += p.val; p = p.next`). Every named pointer parameter is
// deref-aliased in C# to a value var (`ref var p = ref Ꮡp.Value`), which cannot itself be
// repointed; the reassignment must repoint the box and re-alias the value var
// (`Ꮡp = p.next; p = ref Ꮡp.Value;`). Earlier it emitted `p = p.next` — assigning a `ж<node>`
// into the `node` value alias — which fails to compile (CS0266/CS0029). This is the no-unsafe
// analogue of the runtime's `bits = addb(bits, n)` *byte memory walk (a value-using,
// never-nil pointer-parameter walk).
func sumWalk(p *node, steps int) int {
	total := 0
	for i := 0; i < steps; i++ {
		total += p.val
		p = p.next
	}
	return total
}

// firstAfter advances the pointer parameter `steps` times and returns it (the reassigned
// parameter itself becomes the result pointer).
func firstAfter(p *node, steps int) *node {
	for i := 0; i < steps; i++ {
		p = p.next
	}
	return p
}

func main() {
	a := &node{val: 1}
	b := &node{val: 2}
	c := &node{val: 3}
	a.next = b
	b.next = c
	c.next = a // circular: next is never nil

	fmt.Println(sumWalk(a, 6))      // two laps: 2*(1+2+3) = 12
	fmt.Println(firstAfter(a, 4).val) // a->b->c->a->b => 2
}
