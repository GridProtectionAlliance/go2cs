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

func main() {
	fmt.Println(process(5))  // [10 20 0 30 40]
	fmt.Println(mapShadow(2)) // m[2]=200, m[3]=3000 -> 200*10 + 2 = 2002
	fmt.Println(parenIndex()) // 9 (write through (*p)[shadow i=1])
}
