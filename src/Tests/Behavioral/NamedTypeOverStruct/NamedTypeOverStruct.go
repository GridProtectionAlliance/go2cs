package main

import "fmt"

type inner struct {
	a int64
	b int64
}

// wrapper is a DEFINED type whose underlying is the inner struct. In Go the underlying struct's
// fields are accessible on the named type (w.a, w.b); the C# wrapper forwards them as get/set
// properties over a mutable value so a write through a *wrapper persists.
type wrapper inner

type box struct {
	w wrapper
}

// fill writes the forwarded fields through a *wrapper obtained from a struct field — mirrors
// runtime's winlibcall `c.Value.fn = fn` (c is a *winlibcall field accessor).
func fill(b *box) {
	c := &b.w
	c.a = 10
	c.b = 20
}

// readBack reads the forwarded fields back through a *wrapper.
func readBack(b *box) int64 {
	c := &b.w
	return c.a + c.b
}

// bump writes through a pointer TO A FIELD of the wrapper — the pinner.go `&p.x` shape. The
// address routes through the wrapper's forwarded field-box accessor (`c.of(wrapper.Ꮡa)`), which
// must exist on the WRAPPER (CS0117 otherwise) and must be a true ref through `m_value` into the
// underlying struct's field — a copy would silently drop the write.
//
//go:noinline
func bump(p *int64) { *p = *p + 7 }

// base is a struct whose NAME is a C# keyword. Embedding it composes generated identifiers
// from the '@'-escaped member name ('@base'); the promoted-struct box field ('Ꮡʗ' + name)
// must strip the escape — 'Ꮡʗ@base' is invalid C# ('@' is only valid leading an identifier).
type base struct {
	id int64
}

func (b base) twice() int64 {
	return b.id * 2
}

// derived embeds the keyword-named struct: the TypeGenerator emits the ж<base> box field,
// the promoted '@base' accessor, promoted field/method forwarding, and both constructors.
type derived struct {
	base
	tag int64
}

func main() {
	var b box
	fill(&b)
	fmt.Println(b.w.a, b.w.b) // 10 20 — write-through the forwarded fields persisted
	fmt.Println(readBack(&b)) // 30 — read back through the pointer

	c := &b.w
	bump(&c.a) // address of the wrapper's forwarded FIELD
	fmt.Println(b.w.a, readBack(&b)) // 17 37 — the write through &c.a persisted in the original

	// An EMBEDDED struct whose field name collides with the outer struct's OWN field
	// (reflect makeFuncImpl embeds makeFuncCtxt, both have fn): Go shadows the embedded
	// member - the generated promotion must skip it (CS0102/CS0111 on fn and its ref).
	s := shadowed{}
	s.fn = 5
	s.ctxt.fn = 7
	s.tag = 9
	fmt.Println(s.fn, s.ctxt.fn, s.tag) // 5 7 9

	d := derived{}
	d.id = 5 // promoted field write through the keyword-named embed
	d.tag = 1
	fmt.Println(d.id, d.tag, d.twice()) // 5 1 10 — promoted method forwarded via '@base'

	e := derived{base: base{id: 21}, tag: 2} // composite literal keyed by the keyword name
	p := &e
	bump(&p.id)                         // address of a field promoted through the keyword embed
	fmt.Println(e.id, e.tag, e.twice()) // 28 2 56 — the write through &p.id persisted
}

type ctxt struct{ fn, tag int }

// shadowed embeds ctxt and declares its OWN fn - the own field shadows ctxt.fn;
// tag stays promoted.
type shadowed struct {
	ctxt
	fn int
}
