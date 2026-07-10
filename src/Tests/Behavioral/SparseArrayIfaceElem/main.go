// SparseArrayIfaceElem guards the keyed (sparse) array literal whose ELEMENT type is an
// INTERFACE, keyed by UNTYPED named consts — go/internal/gccgoimporter's lookupBuiltinType shape:
//
//	return [...]types.Type{gccgoBuiltinINT8: types.Typ[types.Int8], …}[typ]
//
// The interface element type routes the composite through the interface-cast element loop, which
// rendered each KeyValueExpr as a FLAT `key, value` pair — feeding the golib SparseArray collection
// initializer one item at a time (Add(key) then Add(value): CS1950/CS1503 ×21 pairs) — instead of
// the `[key] = value` indexer form the non-interface path (crypto's digestSizes) emits.
package main

import "fmt"

// shape is the interface element type — the trigger for the interface-cast element loop.
type shape interface {
	name() string
}

type circle struct{}

func (circle) name() string { return "circle" }

type square struct{}

func (square) name() string { return "square" }

// UNTYPED named consts as keys (gccgoBuiltin* analogues): the sparse-key detection must keep the
// indexer form for ident keys with no named type.
const (
	kCircle = iota + 2
	kSquare
	kLast
)

// lookup mirrors lookupBuiltinType: a FUNCTION-BODY sparse literal, immediately indexed.
func lookup(i int) shape {
	return [...]shape{
		kCircle: circle{},
		kSquare: square{},
	}[i]
}

// registry is the PACKAGE-LEVEL form of the same shape; hash.Hash-style tables use it.
var registry = [...]shape{
	kCircle: circle{},
	kSquare: square{},
}

// hashKind exercises the NAMED-integer-type key alongside (the digestSizes shape, which must keep
// its `(int)` index cast) with an interface element.
type hashKind uint

const (
	hCircle hashKind = 5
	hSquare hashKind = 6
)

var byKind = [...]shape{
	hCircle: circle{},
	hSquare: square{},
}

func main() {
	fmt.Println(lookup(kCircle).name())
	fmt.Println(lookup(kSquare).name())
	fmt.Println(lookup(0) == nil)

	fmt.Println("registry:", registry[kCircle].name(), registry[kSquare].name(), len(registry))

	fmt.Println("byKind:", byKind[hCircle].name(), byKind[hSquare].name(), len(byKind))
}
