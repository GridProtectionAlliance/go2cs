// Regression test: a struct field whose name equals its enclosing struct's type name.
//
// Go allows `type Node struct { Node *Node }`, but C# forbids a member sharing its enclosing
// type's name (CS0542). The converter renames such a field with the disambiguation marker
// (`Node` field → `ΔNode`) at both the declaration and every access, while leaving same-named
// type references (the field type, constructors, slices) untouched. The rename applies only to
// package-level named types — a function-local/lifted type is emitted under a qualified name and
// does not collide, so its same-named field is NOT renamed (covered by the StdLibInternalAbi
// test, whose abi source has a function-local `type u struct { u … }`).
package main

import "fmt"

// Node has a field named exactly "Node" (the colliding one) plus a non-colliding "Nodes".
type Node struct {
	Nodes []Node
	Node  *Node
	value int
}

// A function-local type is lifted to a qualified C# name (localCollision_u). Its same-named
// field is NOT renamed (no C# collision), and a composite literal of it must use the lifted
// name, not the bare Go name `u`.
func localCollision() int {
	type u struct {
		u int
	}
	a := u{u: 9}
	b := u{4}
	return a.u + b.u
}

func main() {
	root := Node{value: 1}
	a := Node{value: 2}
	b := Node{value: 3}

	root.Node = &a            // write the colliding field
	root.Nodes = []Node{a, b} // non-colliding field
	a.Node = &b               // chain through the colliding field

	fmt.Println(root.value)               // 1
	fmt.Println(root.Node.value)          // 2   (read through the colliding field)
	fmt.Println(root.Node.Node.value)     // 3   (chained read)
	fmt.Println(len(root.Nodes))     // 2
	fmt.Println(root.Nodes[1].value) // 3
	fmt.Println(localCollision())    // 13 (9 + 4)
}
