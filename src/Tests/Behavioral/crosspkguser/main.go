// Cross-package import guard: importing another package (a separate C# assembly) and using its
// exported surface — Phase 1 (plain type + function + method) and Phase 2 (an exported type ALIAS,
// exercising the GoTypeAlias / <ImportedTypeAliases> round-trip).
package main

import (
	"fmt"

	"crosspkglib"
)

func main() {
	b := crosspkglib.Boiling()
	r := b.Add(10)
	fmt.Println(float64(b))
	fmt.Println(float64(r))

	// Phase 2: name the imported exported type ALIAS `crosspkglib.Temperature`.
	var t crosspkglib.Temperature = crosspkglib.Freezing()
	t = t.Add(32)
	fmt.Println(float64(t))

	// Phase 3: exported struct — field access + method call across the assembly boundary.
	s := crosspkglib.Sensor{Name: "kitchen", Temp: crosspkglib.Boiling()}
	fmt.Println(s.Name, float64(s.Temp), s.Hot())

	// Phase 3: cross-assembly interface satisfaction — a Sensor (lib assembly) is a Labeled.
	var l crosspkglib.Labeled = s
	fmt.Println(l.Label())
	fmt.Println(crosspkglib.Describe(s))
}
