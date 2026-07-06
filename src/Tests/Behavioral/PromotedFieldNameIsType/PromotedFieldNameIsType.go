// Guards the generator's promoted-field-accessor emission when a promoted field's name equals
// the ENCLOSING type name — debug/gosym's `type Func struct{ *Sym }` where `Sym` has a field
// `Func *Func`, so the promoted `Sym.Func` accessor would land as a `Func` member inside `Func`
// (CS0542, "member names cannot be the same as their enclosing type"). The generator now
// Δ-prefixes just that accessor's NAME (the field access stays `Sym.Func`). The promoted field
// is read on the embedded struct directly (`n.sym.Node`), never via the outer value, mirroring
// gosym (which only ever does `sym.Func = fn`), so no reference to the renamed accessor is needed.
package main

import "fmt"

// sym has a field whose name (Node) equals the OUTER type below.
type sym struct {
	Node  *Node // field named 'Node' — same as the enclosing type of the struct that embeds sym
	Value int
	Name  string
}

// Node embeds *sym, so sym.Node promotes to a `Node` accessor inside `Node` — the collision.
type Node struct {
	*sym
	Weight int
}

func main() {
	a := &Node{sym: &sym{Value: 1, Name: "a"}, Weight: 10}
	b := &Node{sym: &sym{Value: 2, Name: "b"}, Weight: 20}
	// Link through the embedded struct's field directly (explicit, not the promoted accessor).
	a.sym.Node = b
	b.sym.Node = a

	// Read promoted NON-colliding fields (Value/Name/Weight) via promotion, and the linked
	// Node via the explicit embedded path.
	fmt.Println(a.Value, a.Name, a.Weight)       // promoted Value/Name + own Weight
	fmt.Println(a.sym.Node.Name, a.sym.Node.Weight) // b via the explicit field
	fmt.Println(b.sym.Node.Name, b.Value)           // a via the explicit field, own promoted Value
}
