// File A (sorts BEFORE main.go): declares a package-level slice of an ANONYMOUS struct
// type. The converter lifts the element type to a named C# struct (e.g. `sizeTestsᴛ1`)
// and records it in the package-level dynamic-type registry, so main.go's range —
// visited AFTER this file — resolves the lifted name directly from the registry.
package main

var sizeTests = []struct {
	name string
	want int
}{
	{"alpha", 5},
	{"be", 2},
	{"gamma", 5},
}
