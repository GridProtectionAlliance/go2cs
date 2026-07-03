// EscapedLoopVarSiblingIndex guards two entangled shadow-rename completeness fixes the runtime
// `runqputslow` shape needs:
//
//  (1) A function-body `for i := …` loop whose var ESCAPES to the heap is hoisted to a func-scope
//      `ref var i = ref heap(…)` decl, so the SIBLING `for i := …` loops that reuse the name collide
//      with it in C# (CS0136). Fixed by collecting the escaped loop var as function-level so the
//      siblings are shadow-renamed `iΔ1`/`iΔ2`.
//  (2) The renamed var used as an LHS INDEX / MAP KEY (`a[i] = …`, `m[ns] = …`) must be renamed too —
//      the `=` assignment case had only handled the ROOT ident (`a`/`m`), so the inner index kept the
//      raw name, resolving to the WRONG (enclosing) variable: a silent wrong value (map case below), or
//      CS0136/CS0165 once the loop var is renamed (array case). This is a silent-correctness bug on its
//      own (reproduces with a plain var shadow, no loop-escape involved — see mapShadow).
//
// The test FAILS (wrong output or won't compile) without BOTH fixes.
package main

import "fmt"

type node struct{ next *node }

//go:noinline
func (n *node) link(m *node) { n.next = m } // pointer-receiver on an element forces the element (and i) to escape

//go:noinline
func process(n int) [5]int {
	var a [5]int
	for i := 0; i < n; i++ {
		a[i] = i * 10 // loop 1: LHS-index write with the loop var
	}
	if n >= 3 {
		for i := 0; i < 2; i++ {
			a[i], a[i+1] = a[i+1], a[i] // loop 2 (nested): LHS-index swap
		}
	}
	var nodes [5]node
	for i := 0; i < n-1; i++ {
		nodes[i].link(&nodes[i+1]) // loop 3: &nodes[i] via pointer-receiver -> i escapes -> hoisted
	}
	return a
}

//go:noinline
func mapShadow(ns int) int {
	m := map[int]int{}
	m[ns] = ns * 100 // LHS key is the param ns
	{
		ns := 3           // block var shadows the param ns
		m[ns] = ns * 1000 // LHS key is the SHADOW ns (3) -> m[3], NOT m[param]
	}
	return m[ns]*10 + len(m) // m[param ns]*10 + count
}

//go:noinline
func boxedSiblings(kind int) int {
	// GENUINELY escaping sibling loop vars in ONE switch case: each loop takes &i directly, so
	// each hoists a real `ref var i = ref heap(…)` box into the same case block. Without the
	// per-container force-shadow rename the second box duplicates the first (CS0128); the first
	// keeps the name, the second renames. (The pointer is used within its own iteration, so the
	// value is independent of per-iteration box identity.)
	total := 0
	switch kind {
	case 1:
		for i := 0; i < 3; i++ {
			p := &i
			total += *p
		}
		for i := 10; i < 13; i++ {
			p := &i
			total += *p * 2
		}
	}
	return total
}

//go:noinline
func caseSiblings(kind int) int {
	// The runtime `typesEqual` shape: TWO sibling `for i := …` loops in ONE switch case whose
	// `i` is used ONLY as an index — the addressed storage is the ARRAY element (`&xs[i+1]`),
	// never `&i`, so NO box is hoisted for either loop and both legally reuse the plain
	// loop-scoped `i` (the old contains-anywhere escape check heap-boxed both, colliding CS0128).
	total := 0
	switch kind {
	case 1:
		var xs [3]node
		for i := 0; i < 2; i++ {
			xs[i].link(&xs[i+1]) // i escapes (element address via pointer receiver)
		}
		var ys [3]node
		for i := 0; i < 2; i++ {
			ys[i].link(&ys[i+1]) // sibling i escapes too -> second hoisted box, same case block
		}
		for p := &xs[0]; p != nil; p = p.next {
			total++
		}
		for p := &ys[0]; p != nil; p = p.next {
			total += 10
		}
	}
	return total
}

func main() {
	fmt.Println(process(5))  // [10 20 0 30 40]
	fmt.Println(mapShadow(2)) // m[2]=200, m[3]=3000 -> 200*10 + 2 = 2002
	fmt.Println(parenIndex()) // 9 (write through (*p)[shadow i=1])
	fmt.Println(caseSiblings(1)) // 33 (3 linked xs + 3 linked ys * 10)
	fmt.Println(boxedSiblings(1)) // 69 (0+1+2 + 2*(10+11+12))
}
