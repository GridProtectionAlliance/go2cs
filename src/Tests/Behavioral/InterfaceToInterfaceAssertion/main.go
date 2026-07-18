// Guards the interface-to-interface assertion fix: asserting a value of one NAMED
// interface to ANOTHER named interface (`s.(Marshaler)` where s is a Stringish whose
// dynamic type *widget also implements Marshaler). A named-interface assertion resolves
// at run time only through a compile-time GoImplement adapter — the converter records
// one only when the concrete dynamic type is discovered to implement BOTH the source and
// target interfaces. Before the fix, `convTypeAssertExpr` recorded implementations only
// for an EMPTY-interface source, so this assertion emitted no adapter and panicked at run
// time ("interface conversion: ... not main.Marshaler"). It COMPILES either way; only
// running reveals it. This mirrors hash/adler32's `h.(encoding.BinaryMarshaler)`.
package main

import "fmt"

type Stringish interface{ Str() string }

type Marshaler interface{ Marshal() string }

type widget struct{ n int }

func (w *widget) Str() string     { return fmt.Sprintf("widget(%d)", w.n) }
func (w *widget) Marshal() string { return fmt.Sprintf("<%d>", w.n) }

// other implements Stringish but NOT Marshaler — the negative assertion must miss.
type other struct{}

func (o *other) Str() string { return "other" }

// newStringish returns an interface value; the concrete dynamic type is hidden behind it,
// exactly like hash.New() returning a hash.Hash32 backed by *digest.
func newStringish(n int) Stringish { return &widget{n} }

func main() {
	s := newStringish(7)
	fmt.Println(s.Str())

	// Interface-to-interface assertion to a NAMED interface the dynamic type implements.
	m := s.(Marshaler)
	fmt.Println(m.Marshal())

	// Comma-ok form on a matching dynamic type.
	if m2, ok := s.(Marshaler); ok {
		fmt.Println("ok", m2.Marshal())
	}

	// Comma-ok form on a NON-matching dynamic type must report false, not panic.
	var s2 Stringish = &other{}
	if _, ok := s2.(Marshaler); ok {
		fmt.Println("unexpected: other is Marshaler")
	} else {
		fmt.Println("other is not Marshaler")
	}
}
