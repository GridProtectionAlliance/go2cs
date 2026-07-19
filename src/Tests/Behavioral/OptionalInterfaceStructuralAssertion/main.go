// OptionalInterfaceStructuralAssertion guards the declaration-site same-package structural
// GoImplement producer (recordLocalConcreteImplementers): a concrete type that implements a
// NARROWER named interface purely STRUCTURALLY -- never via an explicit cast/conversion anywhere
// in the source -- must still be assertable to that narrower interface at run time.
//
// This mirrors runtime.errorString (asserted to runtime.Error by math/bits) whose home package
// never itself casts errorString to runtime.Error: without a producer at the interface's own
// declaration site, no GoImplement adapter is generated and the assertion silently misses.
//
// The decisive assertion below is `it.(Tagger)` where `it` ranges over a []any: the concrete
// dynamic type is not statically resolvable at the assertion site, so the assertion-site recorder
// records nothing -- the declaration-site producer is the sole source of the widget->Tagger
// adapter. widget is also NEVER cast to Tagger and NEVER held as a Tagger-typed value.
package main

import "fmt"

// Describer is the WIDER interface (single method).
type Describer interface {
	Describe() string
}

// Tagger is the NARROWER interface -- a strict method-set superset of Describer. widget satisfies
// it structurally only; nothing here writes Tagger(x), `var _ Tagger = ...`, or asserts a
// Describer-typed value to Tagger.
type Tagger interface {
	Describe() string
	Tag() string
}

type widget struct {
	name string
}

func (w widget) Describe() string { return "describe:" + w.name }
func (w widget) Tag() string      { return "tag:" + w.name }

// plain implements neither interface -- the comma-ok miss control.
type plain struct{}

// newDescriber hands the concrete back as the WIDER interface only.
func newDescriber(name string) Describer {
	return widget{name: name}
}

func main() {
	// Held as the WIDER named interface; call its method (no Tagger involved here).
	d := newDescriber("alpha")
	fmt.Println(d.Describe())

	// Held as `any` through a heterogeneous slice, then asserted to the NARROWER named interface.
	items := []any{widget{name: "beta"}, plain{}}

	for _, it := range items {
		if t, ok := it.(Tagger); ok {
			fmt.Println(t.Describe(), t.Tag())
		} else {
			fmt.Println("not a Tagger")
		}
	}
}
