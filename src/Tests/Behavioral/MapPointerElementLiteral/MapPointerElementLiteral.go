// Guards a composite MAP literal whose VALUE (or KEY) type is a pointer, with the element supplied by
// a deref'd pointer parameter — the map case dropped from ReceiverPointerValue in an earlier session.
// `map[K]*T{k: c}` / `map[*T]V{c: 1}` where `c` is a `*node` param renders the VALUE alias `c` into a
// `ж<node>` map slot (CS0029); the fix boxes it as `Ꮡc` (convKeyValueExpr, MapSource pointer element).
package main

import "fmt"

type node struct{ id int }

// pointer-VALUE map literal.
func buildValueMap(a, b *node) map[string]*node {
	return map[string]*node{"a": a, "b": b}
}

// pointer-KEY map literal (lookup is by pointer identity).
func buildKeyMap(a, b *node) map[*node]int {
	return map[*node]int{a: 10, b: 20}
}

func main() {
	a := &node{id: 1}
	b := &node{id: 2}

	vm := buildValueMap(a, b)
	fmt.Println(vm["a"].id, vm["b"].id) // 1 2

	// aliasing: vm["a"] IS the stored box a — mutate through it, read back through a.
	vm["a"].id = 99
	fmt.Println(a.id) // 99

	km := buildKeyMap(a, b)
	fmt.Println(km[a], km[b]) // 10 20
}
