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
}
