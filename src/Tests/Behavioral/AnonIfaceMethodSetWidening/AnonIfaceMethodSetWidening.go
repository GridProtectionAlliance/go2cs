package main

import "fmt"

// Guards Go's value-vs-pointer METHOD-SET rule through the duck-typing machinery
// (reflection-bridge Phase-3, X1×X3): a VALUE-sourced dynamic-interface value must
// not re-assert to a wider anonymous interface carrying a pointer-receiver-only
// method, while a POINTER-sourced one must.

type gadget struct{ n int }

func (g gadget) Foo() string  { return fmt.Sprintf("foo %d", g.n) } // value receiver
func (g *gadget) Bar() string { return "bar" }                      // pointer receiver ONLY

type fooer interface{ Foo() string }

func main() {
	// Value-sourced interface value: only the value method set.
	var v fooer = gadget{1}
	if _, ok := v.(interface {
		Foo() string
		Bar() string
	}); ok {
		fmt.Println("value-widened-wrong")
	} else {
		fmt.Println("value-not-widened-ok")
	}

	// Pointer-sourced: the full *T method set widens.
	var p fooer = &gadget{2}
	if b, ok := p.(interface {
		Foo() string
		Bar() string
	}); ok {
		fmt.Println("pointer-widened-ok", b.Foo(), b.Bar())
	} else {
		fmt.Println("pointer-not-widened-wrong")
	}
}
