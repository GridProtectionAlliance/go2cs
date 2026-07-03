package main

import "fmt"

type Inner struct {
	V int
	W int
}

// A type ALIAS to a struct (Go 1.9+; runtime/type.go does `type name = abi.Name`). A composite
// literal of the alias must use the C# constructor form `new Inner(V: …)`, not the Go-style
// `new Inner{V: …}` — an alias is a *types.Alias (Go 1.22+), neither *types.Named nor
// *types.Struct, so the struct-composite detection must test the underlying type.
type alias = Inner

func keyed() alias    { return alias{V: 7, W: 3} }
func empty() alias    { return alias{} }
func positional() alias { return alias{1, 2} }

// A type ALIAS to an array of a csproj-aliased numeric type (fiat's
// `type p224UntypedFieldElement = [4]uint64`). The `global using` RHS cannot see the
// golib csproj aliases (C# using directives are invisible to one another), so the
// element must render as a C# keyword: `global using words = go.array<ulong>;`.
type words = [4]uint64

func sum(w words) uint64 {
	var s uint64
	for i := 0; i < len(w); i++ {
		s += w[i]
	}
	return s
}

func main() {
	a := keyed()
	b := empty()
	c := positional()
	fmt.Println(a.V, a.W, b.V, c.V, c.W)
	w := [4]uint64{10, 20, 30, 40}
	fmt.Println(sum(w), len(w))
}
