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

// markSeen mirrors encoding/gob's buildEncEngine: a map keyed by a POINTER type
// (`map[*node]bool`) indexed by a deref-aliased pointer parameter. `p` is emitted as
// `ref var p = ref Ꮡp.Value`, so both the read `seen[p]` and the write `seen[p] = true` must
// use the box `Ꮡp` — a bare `p` passes the node VALUE where the ж<node> key is expected
// (CS1503). The field read `p.val` forces the deref-alias.
func markSeen(p *node, seen map[*node]bool) int {
	if seen[p] {
		return p.val
	}
	seen[p] = true
	return -p.val
}

// walkChain mirrors go/ast resolve.go's scope-chain walk: a deref-aliased pointer PARAMETER
// repointed in the for-loop POST (`for ; p != nil; p = p.next`). The repoint expands to a
// box-repoint (`Ꮡp = p.next`) PLUS a value re-alias (`p = ref Ꮡp.DerefOrNil()`); the two
// cannot share the single for-post slot, so the converter keeps the box-repoint in the post
// and injects the re-alias at the TOP of the loop body (otherwise CS1003/CS1525).
func walkChain(p *node) int {
	count := 0
	for ; p != nil; p = p.next {
		count += p.val
	}
	return count
}

// visitLocal mirrors reflect's Type.FieldByNameFunc: a pointer-keyed map (`map[*node]bool`)
// indexed by a LOCAL variable that already holds a pointer (`t := scan; visited[t]`). Unlike a
// deref-aliased pointer PARAMETER (markSeen above, which needs the box `Ꮡp`), a local holding a
// ж<node> IS already the key, so the index must stay a bare `visited[t]` — the box form `ᏑtΔ1`
// has no accessor (CS0103). Guards the gate restricting the pointer-key box rendering to params.
func visitLocal(nodes []*node) int {
	visited := map[*node]bool{}
	sum := 0
	for _, scan := range nodes {
		t := scan // t is a LOCAL *node — already a pointer, so `visited[t]` needs no box
		if visited[t] {
			continue
		}
		visited[t] = true
		sum += t.val
	}
	return sum
}

// collect mirrors go/ast's CommentMap.addComment: a deref-aliased pointer PARAMETER used as a
// COMPOSITE-LITERAL ELEMENT (`[]*node{p}`) must render the box (`Ꮡp`) — not the value alias
// (`p`) — into the `ж<node>[]` array (CS0029); the sibling `append(list, p)` already took the
// box through the call-argument pointer arm.
func collect(list []*node, p *node) []*node {
	if len(list) == 0 {
		list = []*node{p}
	} else {
		list = append(list, p)
	}
	return list
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

	seen := map[*node]bool{}
	fmt.Println(markSeen(a, seen), markSeen(a, seen), markSeen(b, seen)) // -1 1 -2

	// Linear (nil-terminated) chain for the for-post pointer-param walk.
	x := &node{val: 100}
	y := &node{val: 20}
	x.next = y // y.next stays nil, terminating the walk
	fmt.Println("chain:", walkChain(x)) // 120

	// Local-pointer-var indexing a pointer-keyed map (reflect FieldByNameFunc shape).
	dup := &node{val: 5}
	fmt.Println("visitLocal:", visitLocal([]*node{dup, dup, x})) // dup once + x: 5 + 100 = 105

	// Pointer param as a composite-literal element (go/ast addComment shape): first call takes
	// the `[]*node{p}` literal arm, second the append arm; the boxes must alias the originals.
	var list []*node
	list = collect(list, a)
	list = collect(list, b)
	a.val = 11 // prove the collected pointers alias the original nodes
	fmt.Println("collect:", list[0].val, list[1].val) // 11 2
}
