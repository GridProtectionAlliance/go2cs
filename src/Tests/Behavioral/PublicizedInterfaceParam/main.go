package main

import "fmt"

// An UNEXPORTED interface used as the parameter of an EXPORTED function. The
// converter must publicize the interface (emit `public`) or it defaults to
// C# `internal` and becomes less accessible than the exported member that
// references it (CS0051 — the shape of testing.MainStart(deps testDeps, ...)).
// Its method returns a built-in type, so publicizing it needs no further
// (transitive) publicization.
type describer interface {
	Describe() string
}

// An EXPORTED impl type — keeps the focus on the interface-parameter publicize
// (an unexported impl with an exported method would trip a separate receiver
// accessibility case).
type Label struct {
	text string
}

func (l Label) Describe() string {
	return "label:" + l.text
}

// Exported — forces `describer` into the publicized set.
func Show(d describer) string {
	return d.Describe()
}

func main() {
	fmt.Println(Show(Label{text: "A"}))
	fmt.Println(Show(Label{text: "B"}))
}
