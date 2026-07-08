package main

import "fmt"

// A package-level var whose type is a map with an ANONYMOUS STRUCT value type — the exact
// construct crypto/internal/hpke's SupportedKEMs / SupportedAEADs use. Before the converter fix,
// the anonymous value struct was not lifted, so getTypeName emitted its raw Go `struct{…}` syntax
// straight into the C# map signature (`map<uint16, struct{ … }>`), which C# cannot parse — a
// CS1519/CS1003 syntax cascade. The value struct must lift to a named type so the map reads
// `map<uint16, kindsᴛ1>`. A func-typed field (like SupportedAEADs' `aead`) is included so the
// lifted struct's delegate field and its method-group / func-literal values are exercised too.
var kinds = map[uint16]struct {
	name  string
	size  int
	scale func(int) int
}{
	0x0001: {name: "alpha", size: 16, scale: func(n int) int { return n * 2 }},
	0x0002: {name: "beta", size: 32, scale: triple},
}

func triple(n int) int { return n * 3 }

func main() {
	// Read back by key (NOT range — map order is randomized) so the output is deterministic.
	// Each read copies the struct value; field access + the func-typed field call prove the
	// lifted struct's members resolve and the values round-trip.
	a := kinds[0x0001]
	fmt.Println(a.name, a.size, a.scale(a.size))

	b := kinds[0x0002]
	fmt.Println(b.name, b.size, b.scale(b.size))

	fmt.Println(len(kinds))
}
