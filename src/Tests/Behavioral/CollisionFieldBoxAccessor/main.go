package main

import "fmt"

// `mark` is BOTH a package-level type and a method name → the converter records a
// type-vs-method name collision and Δ-renames the package-level identifier (`Δmark`).
// A struct FIELD named `mark` is struct-scoped and is NOT renamed (declared `mark`), so
// its box accessor must be `holder.Ꮡmark` — matching the generated static — NOT the
// collision-renamed `holder.ᏑΔmark` (which would be CS0117). Guards structFieldBoxName.

type mark struct{ id int }

type tagger struct{}

func (tagger) mark() {} // method named `mark` (collides with type `mark`)

type holder struct {
	mark  int
	extra int
}

var h holder

func main() {
	// &global.field routes through the box-field accessor: Ꮡh.of(holder.Ꮡmark).
	p := &h.mark
	*p = 42

	q := &h.extra
	*q = 7

	fmt.Println(h.mark, h.extra) // 42 7

	// Keep the colliding type/method referenced so they are emitted.
	var m mark
	m.id = 3
	var t tagger
	t.mark()
	fmt.Println(m.id) // 3
}
