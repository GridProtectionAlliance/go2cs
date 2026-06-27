// Regression test: a composite type (e.g. a map) that names two types from the *current*
// package must not self-qualify either of them. getTypeName/getFullTypeName stripped only the
// FIRST occurrence of the current package's path prefix, so `map[OSArch]info` kept the value
// type self-qualified (`pkg.info`); for a slash-bearing package path the leftover prefix then
// tripped the trailing slash-handling, collapsing the whole type to `pkg.info` → CS0246. This
// is the internal/platform `var distInfo = map[OSArch]osArchInfo{...}` pattern.
package main

import "fmt"

type OSArch struct {
	os, arch string
}

type info struct {
	supported bool
}

// package-level inferred var whose type names two current-package types (map key + value)
var table = map[OSArch]info{
	{"linux", "amd64"}:   {supported: true},
	{"windows", "386"}:   {supported: false},
}

func main() {
	fmt.Println(table[OSArch{"linux", "amd64"}].supported)
	fmt.Println(table[OSArch{"windows", "386"}].supported)
	fmt.Println(len(table))
}
