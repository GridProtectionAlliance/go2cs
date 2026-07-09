// A middle library for the synthesized-delegate cross-package rename guard: it declares a
// METHODLESS named func type whose signature names ANOTHER package's Δ-renamed type
// (CrossPkgLib.Status → ΔStatus) — the go/ast `FieldFilter func(string, reflect.Value) bool`
// shape — plus an exported func matching it (the `ast.NotNilFilter` shape). A consumer that
// imports only THIS package must still render Status through its recorded alias when it
// synthesizes the collapsed delegate type.
package CrossPkgFuncLib

import (
	"CrossPkgLib"
)

// Picker is a methodless named func type over the foreign renamed Status; it collapses to its
// base C# delegate everywhere it is referenced.
type Picker func(CrossPkgLib.Status) bool

// Hot reports whether a status code is above 10 — exported so a consumer can pass it where a
// Picker is expected.
func Hot(st CrossPkgLib.Status) bool {
	return st.Code > 10
}

// Count applies the picker across an internal status set, so a consumer never needs to name
// CrossPkgLib itself.
func Count(pick Picker) int {
	n := 0

	for _, st := range []CrossPkgLib.Status{{Code: 5}, {Code: 15}, {Code: 25}} {
		if pick(st) {
			n++
		}
	}

	return n
}
