// Guards a struct field whose name equals its type WHEN that type is ALSO Δ-renamed for a
// type-vs-method collision — internal/trace's `type Label struct{ Label string }` alongside
// `func (e Event) Label() Label`. The method collision renames the TYPE to ΔLabel; a single-Δ
// field rename would then be ΔLabel too (CS0542 again), so the field must double the marker
// (ΔΔLabel), matching the keyword-family case. The field decl, composite-literal key, and every
// access site must all agree on the doubled name.
package main

import "fmt"

// event carries a Kind and, for a Label event, a Label value — mirroring internal/trace.Event.
type event struct {
	kind  int
	label string
}

// Label (the METHOD on event) collides with the Label TYPE below → the type is Δ-renamed.
func (e event) Label() Label {
	return Label{Label: e.label, Resource: e.kind}
}

// Label is a struct whose FIELD is also named Label (field-name == type-name), and the type
// collides with event.Label() above.
type Label struct {
	Label    string // field name equals the enclosing type name AND the colliding method name
	Resource int
}

// describe reads the doubled-marker field through both a value and the method result.
func (l Label) describe() string {
	return fmt.Sprintf("%s@%d", l.Label, l.Resource)
}

func main() {
	e := event{kind: 7, label: "cpu"}
	l := e.Label() // method call returning the Label struct
	fmt.Println(l.Label, l.Resource)
	fmt.Println(l.describe())

	// direct composite literal of the colliding field, then read + write it.
	m := Label{Label: "mem", Resource: 3}
	m.Label = m.Label + "!"
	fmt.Println(m.Label, m.Resource)
}
