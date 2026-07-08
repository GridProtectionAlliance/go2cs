package main

import "fmt"

// Node is a value type with a pointer-receiver method, so a `*Node` receiver is deref-aliased by the
// converter (`ref var n = ref Ꮡn.Value`): the value alias `n` is a `Node`, its box `Ꮡn` the `*Node`.
type Node struct{ val int }

func (n *Node) Value() int { return n.val }

// group reproduces crypto/x509 Verify's `[][]*Certificate{{c}}` shape: an ELIDED nested composite
// literal whose inner `{n}` is a type-inferred `[]*Node`, with the pointer receiver `n` as its sole
// element. The elided (untyped) slice path must render a bare pointer-typed ident element as its box
// (`Ꮡn`, the pointer value), not the deref-aliased value alias — a `Node` value in a `ж<Node>[]`
// array is CS0029. Guards the elided-pointer-element boxing fix in convCompositeLit.
func (n *Node) group() [][]*Node {
	return [][]*Node{{n}}
}

func main() {
	n := &Node{val: 42}
	groups := n.group()
	fmt.Println(len(groups), len(groups[0]), groups[0][0].Value())
}
