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

func main() {
	a := keyed()
	b := empty()
	c := positional()
	fmt.Println(a.V, a.W, b.V, c.V, c.W)
}
