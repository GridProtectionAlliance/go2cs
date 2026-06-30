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
}
