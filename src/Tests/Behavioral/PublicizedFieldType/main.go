// Regression test: an unexported type used as the type of an exported struct field must be
// emitted as `public`.
//
// C# requires a field's type to be at least as accessible as the field. Go's unicode package has
// `type d [MaxCase]rune` (unexported) used as the exported field `CaseRange.Delta`, which failed
// with CS0052 (and its generated constructor with CS0051). A package pre-pass now collects every
// unexported named type used as the type of an exported field (an array, struct, or basic type
// directly, or through a pointer/slice/array/map/channel) and emits it `public`; the generator honors the
// converter's explicit access modifier over its default name-based scope.
package main

import "fmt"

// unexported array type used as an exported field (the unicode `d` pattern).
type d [3]rune

// unexported struct type used as an exported field.
type inner struct {
	v int
}

// unexported named basic type used as an exported field.
type level int

// CaseRange is exported and has exported fields of the unexported types above.
type CaseRange struct {
	Lo    uint32
	Delta d
	Item  inner
	Lvl   level
}

// coder is an UNEXPORTED struct type. EncBuffer is an EXPORTED defined type OVER it
// (`type EncBuffer coder`) — image/png's `type EncoderBuffer encoder` shape. The generated
// [GoType("coder")] wrapper for EncBuffer is public (EncBuffer is exported), so its Value property,
// constructor, and implicit operators all expose `coder`; the wrapped type must therefore be
// public too — a public member exposing an internal type is CS0051/CS0053/CS0056/CS0057, and
// C# conversion operators are required to be public (CS0558), so an internal wrapper is not an
// option. The converter now publicizes the written RHS of an exported wrapper.
type coder struct {
	tag string
	seq int
}

type EncBuffer coder

// tally and weight are UNEXPORTED named types used in the signatures of EXPORTED methods on
// the exported CaseRange — `func (CaseRange) Tally() tally` and `func (CaseRange) Weigh(weight)`.
// An exported method is public in C#; a return type less accessible than the method is CS0050
// and a parameter type is CS0051 (crypto/internal/bigmod's `func (x *Nat) Equal(y *Nat) choice`,
// choice unexported). The converter publicizes an exported type's exported-method signature types.
type tally int
type weight int

func (cr CaseRange) Tally() tally        { return tally(cr.Lo) }
func (cr CaseRange) Weigh(w weight) int  { return int(w) * 2 }

func main() {
	var cr CaseRange
	cr.Lo = 65
	cr.Item = inner{v: 9}
	cr.Lvl = level(3)

	fmt.Println(cr.Lo)       // 65
	fmt.Println(cr.Delta[0]) // 0 (zero value of the array element)
	fmt.Println(cr.Item.v)   // 9
	fmt.Println(cr.Lvl)      // 3

	// Exercise the exported-wrapper-over-unexported surface: merely DECLARING the exported
	// wrapper over the unexported struct forces the generated wrapper's public Value/ctor/
	// operators to reference `coder`, reproducing the accessibility failure; the composite
	// literal and field reads confirm the wrapper still works end-to-end.
	b := EncBuffer{tag: "png", seq: 7}
	fmt.Println(b.tag, b.seq) // png 7

	// Exercise the exported-method-signature publicization: Tally returns the unexported
	// `tally`, Weigh takes the unexported `weight`.
	fmt.Println(cr.Tally(), cr.Weigh(3)) // 65 6
}
