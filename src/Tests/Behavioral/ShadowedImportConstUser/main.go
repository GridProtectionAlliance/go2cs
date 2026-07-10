// Guards the `_package`-qualifier fallback against a Δ-renamed foreign CONST: this package
// declares a METHOD named exactly like the import, so every use of the package ident must
// qualify through the static package class (`ShadowedImportConstLib_package`, the
// compress/flate `(*byLiteral).sort` vs `import "sort"` CS0119 fallback) — and a const the
// LIB Δ-renamed for its own const-vs-method collision must substitute the renamed member
// through that fallback too (`…_package.ΔPeak`), not bind the `Peak(this Meter)` extension
// method group (crypto/tls `Config.time` × `time.Second`, CS0019 ×2).
package main

import (
	"fmt"

	"ShadowedImportConstLib"
)

type gauge struct{ level int }

// ShadowedImportConstLib (the METHOD) shadows the import alias in this package's member space.
func (g gauge) ShadowedImportConstLib() int { return g.level }

func main() {
	g := gauge{level: 3}

	// The CS0019 shape: a named-numeric value scaled by the Δ-renamed foreign const, reached
	// through the `_package` qualifier fallback.
	span := ShadowedImportConstLib.Span(2) * ShadowedImportConstLib.Peak
	fmt.Println("span:", int64(span))

	// A NON-renamed member through the same fallback keeps its raw name.
	m := ShadowedImportConstLib.Meter{Level: g.ShadowedImportConstLib()}
	fmt.Println("peak method:", m.Peak())
}
