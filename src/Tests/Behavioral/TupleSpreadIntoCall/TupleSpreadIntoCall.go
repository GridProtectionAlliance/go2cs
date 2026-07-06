// Guards a `:=` assignment whose RHS is a call WRAPPING a multi-value call — the inner call's
// results must spread into the outer call's parameters. C# has no splat, so the inner call is
// hoisted into temps (`var (ᴛ1, ᴛ2) = parts(); var r = makeNode(ᴛ1, ᴛ2);`); passing the whole
// tuple as one argument is CS7036. Covers BOTH assignment lowering paths: the single-declare
// block (a value result) AND the mixed/escaping block (a pointer result whose local is heap-boxed
// and so is not counted in declaredCount). Same shape as text/template/parse's rangeControl
// `r := t.newRange(t.parseControl("range"))`, where parseControl returns five values.
package main

import "fmt"

type Node struct {
	a, b int
}

func parts() (int, int) {
	return 3, 4
}

func makeNode(a, b int) *Node {
	return &Node{a: a, b: b}
}

func combine(a, b int) int {
	return a*10 + b
}

// r escapes (returned pointer) → the mixed/escaping assignment branch.
func build() *Node {
	r := makeNode(parts())
	return r
}

func main() {
	n := build()
	// s is a plain value local → the single-declare assignment branch.
	s := combine(parts())
	fmt.Println(n.a, n.b, s) // 3 4 34
}
