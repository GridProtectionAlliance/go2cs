// Child-package types for the synthesized-delegate qualification guard: this subpackage's
// import path is slash-bearing (SynthesizedDelegateChildPkg/inner — C# namespace
// go.SynthesizedDelegateChildPkg, class inner_package), so a parent-package func type naming
// *Record used to stringify with the full import path and mis-qualify in the delegate render.
package inner

// Record is the child-package element type carried by the parent's synthesized delegates.
type Record struct {
	Name string
	Hits int
}

// Loader mirrors go/internal/gccgoimporter.Importer: a methodless NAMED func type whose
// signature carries a map of child-package pointers, a nested func param returning a tuple,
// and child-package pointer results.
type Loader func(cache map[string]*Record, name string, lookup func(string) (string, error)) (*Record, error)
