// Cross-package import guard: importing another package (a separate C# assembly) and using its
// exported surface — Phase 1 (plain type + function + method), Phase 2 (an exported type ALIAS,
// exercising the GoTypeAlias / <ImportedTypeAliases> round-trip), and Phase 3 (exported struct field
// access + cross-assembly interface satisfaction).
package main

import (
	"fmt"

	"CrossPkgLib"
)

func main() {
	b := CrossPkgLib.Boiling()
	r := b.Add(10)
	fmt.Println(float64(b))
	fmt.Println(float64(r))

	// Phase 2: name the imported exported type ALIAS `CrossPkgLib.Temperature`.
	var t CrossPkgLib.Temperature = CrossPkgLib.Freezing()
	t = t.Add(32)
	fmt.Println(float64(t))

	// Phase 3: exported struct — field access + method call across the assembly boundary.
	s := CrossPkgLib.Sensor{Name: "kitchen", Temp: CrossPkgLib.Boiling()}
	fmt.Println(s.Name, float64(s.Temp), s.Hot())

	// Phase 3: cross-assembly interface satisfaction — a Sensor (lib assembly) is a Labeled.
	var l CrossPkgLib.Labeled = s
	fmt.Println(l.Label())
	fmt.Println(CrossPkgLib.Describe(s))

	// Phase 4: promoted fields through a cross-package POINTER embed — read AND write-through
	// (the accessor is a true ref through the embed: a write via p.Temp mutates the target s2).
	// NOTE: promoted METHOD calls (p.Hot()) are a separate, still-open cross-package gap (the
	// method-promotion path is also syntax-resolved); zero runtime sites need it — call the method
	// through the embed explicitly.
	s2 := CrossPkgLib.Sensor{Name: "attic", Temp: 20}
	p := probe{Sensor: &s2, id: 7}
	fmt.Println(p.Name, float64(p.Temp), p.id) // attic 20 7
	p.Temp = 75
	fmt.Println(float64(s2.Temp), s2.Hot()) // 75 true — write-through observed via the target

	// Phase 4b: a promoted POINTER-RECEIVER METHOD through the cross-package pointer embed —
	// `p.Calibrate(5)` has no generated forwarder (method promotion is syntax-resolved), so the
	// converter emits the explicit hop `p.Sensor.val.Calibrate(…)`; the write reaches the target.
	// (A promoted VALUE-receiver method call, p.Hot(), remains a documented open gap — call
	// through the embed explicitly.)
	p.Calibrate(5)
	fmt.Println(float64(s2.Temp)) // 80 — the promoted pointer-receiver write reached s2

	// Phase 4: promoted fields through a cross-package VALUE embed.
	g := tagged{Sensor: CrossPkgLib.Sensor{Name: "cellar", Temp: 5}, n: 3}
	fmt.Println(g.Name, float64(g.Temp), g.n) // cellar 5 3
	g.Temp = 60
	fmt.Println(float64(g.Temp), g.Sensor.Hot()) // 60 true
}

// Phase 4: field promotion through a CROSS-PACKAGE embed. In a real MSBuild build the library
// arrives as a METADATA reference (never a CompilationReference), so the generator's syntax-based
// member resolution finds nothing and the promoted-field accessors were silently EMPTY — every
// `p.Name`/`p.Temp` on the embedding struct was CS1061 (runtime hits this on
// `type rtype struct { *abi.Type }`, t.TFlag/t.Str/t.Kind_). The generator now falls back to the
// semantic model (public instance fields via the type's metadata symbol), emitting true-ref
// accessors through the embed (`ref Sensor.val.Temp` for a pointer embed), so writes through the
// promoted name reach the embedded target.

// probe embeds *CrossPkgLib.Sensor (POINTER embed — the rtype shape).
type probe struct {
	*CrossPkgLib.Sensor
	id int
}

// tagged embeds CrossPkgLib.Sensor BY VALUE (the same resolution failure hit value embeds).
type tagged struct {
	CrossPkgLib.Sensor
	n int
}
