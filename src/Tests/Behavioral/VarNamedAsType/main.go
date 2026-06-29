package main

import "fmt"

// A local pointer variable named after its own type — `m := getg().m` in runtime — shadows the
// type, so taking the address of one of its fields (`&m.park`) emits a box accessor
// `m.of(node.Ꮡx)` whose TYPE reference `node` (here the type) would bind to the variable `m`
// instead, leaving it without the `Ꮡx` static (CS1061). The converter qualifies the type with the
// root namespace ONLY on this collision — `m.of(go.node.Ꮡx)` — so it resolves to the type.
// Guards boxAccessorType.

type node struct {
	x int
	y int
}

func main() {
	v := node{x: 5, y: 6}
	node := &v // local named `node`, of type *node — shadows the type `node`

	px := &node.x // &node.x -> node.of(go.node.Ꮡx)
	*px = 99

	py := &node.y
	*py = 88

	fmt.Println(v.x, v.y)       // 99 88 — writes reach the original
	fmt.Println(node.x, node.y) // 99 88
}
