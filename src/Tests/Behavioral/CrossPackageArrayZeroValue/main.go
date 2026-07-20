// Cross-package zero-value construction guard (math/rand/v2's ChaCha8 shape).
//
// Holder plays ChaCha8: a struct whose field type is declared in ANOTHER package and carries a
// fixed array. Nothing here initializes that field, so the generator's zero-value constructors are
// what must construct it — and the generator only knew how to resolve a field's type by SYNTAX,
// which a cross-package type has none of in a real MSBuild build (a <ProjectReference> arrives as
// compiled METADATA). Left `default`, the nested array's backing is null and the first index or
// pin throws instead of yielding Go's zeroed words.
//
// The SUBPACKAGE is what makes this a guard: a same-project field type resolves by syntax and
// would pass even against the unfixed generator.
package main

import (
	"fmt"

	"CrossPackageArrayZeroValue/bufpkg"
)

// Holder plays math/rand/v2's ChaCha8 — a cross-package struct field with no initializer, beside
// a same-package fixed array (which self-initializes and was never affected).
type Holder struct {
	state   bufpkg.State
	readBuf [8]byte
	tag     string
}

// Deep reaches the fixed array only through TWO struct hops, exercising the recursive walk.
type Deep struct {
	nested bufpkg.Nested
}

func main() {
	h := new(Holder)

	// Direct index into the cross-package fixed array. A null backing THROWS here, rather than
	// silently measuring zero the way len/range do — so the guard fails loudly, not quietly.
	h.state.Buf[2] = 42
	fmt.Println(h.state.Buf[2])

	// Write through the pointer receiver, then read back the array field.
	h.state.Fill()
	fmt.Println(bufpkg.Sum(&h.state))
	fmt.Println(len(h.state.Buf), len(h.state.Seed), h.state.N)

	// Address-of a fixed array field — the pinning shape.
	bufpkg.FillArray(&h.state.Buf)
	fmt.Println(bufpkg.Sum(&h.state))

	// The same-package fixed array and the plain scalar/string zero values stay correct.
	fmt.Println(len(h.readBuf), h.readBuf[7], h.tag == "")

	// NOTE: a composite literal that OMITS the needy field — `&Holder{tag: "lit"}` — is NOT
	// exercised here. It goes through the generator's PARAMETERIZED constructor, which assigns
	// `this.state = state` unconditionally and never consults StructTypeNeedsConstruction, so it
	// overwrites the field with the omitted argument's `default`. That is a SEPARATE, still-open
	// defect: it breaks identically for a SAME-package field type, so it is not the cross-package
	// resolution bug this project guards, and fixing it would widen the blast radius to every
	// parameterized constructor. (The fixed-array member beside it already has the analogous
	// `if (readBuf.Source is not null)` guard — the needy-struct member has no equivalent.)

	// Two struct hops to the array.
	d := new(Deep)
	d.nested.Inner.Fill()
	fmt.Println(bufpkg.Sum(&d.nested.Inner), d.nested.Label == "")
}
