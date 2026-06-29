package main

import "fmt"

type cycle struct{ n int }

type rec struct {
	future [3]cycle
}

type holder struct{ r *rec }

func bump(c *cycle) { c.n++ }

// Taking the address of an element of an ARRAY FIELD reached through a POINTER — `&p.future[i]`
// where `p` is a `*rec`. The array field's address must go through the box-field accessor
// (`Ꮡp.of(rec.Ꮡfuture)` for a pointer param, `p.of(rec.Ꮡfuture)` for a pointer local), then be
// indexed with `.at<cycle>(i)`. The defect prefixed `Ꮡ` onto `(~p).future` → `Ꮡ(~p).future.at(...)`,
// which binds `.future` on the box `Ꮡ(~p)` (a ж<rec>, no `future` member) → CS1061. Mirrors
// runtime's `mpc := &mp.future[index]` in mprof.

func viaParam(p *rec, i int) {
	c := &p.future[i] // pointer param: Ꮡp.of(rec.Ꮡfuture).at<cycle>(i)
	bump(c)
}

func viaLocal(h *holder, i int) {
	p := h.r          // pointer local
	c := &p.future[i] // pointer local: p.of(rec.Ꮡfuture).at<cycle>(i)
	bump(c)
}

func main() {
	// Construct with an explicit array literal so the fixed-size array field `future` is actually
	// allocated — neither a zero-value `var r rec` nor an empty `rec{}` allocates a nested
	// fixed-size array field (a known runtime limitation: the generated zero-value/nil ctor leaves
	// it unallocated → NRE on element access), so the array is spelled out.
	r := rec{future: [3]cycle{{0}, {0}, {0}}}
	viaParam(&r, 0)
	viaParam(&r, 0)
	fmt.Println(r.future[0].n) // 2

	h := &holder{r: &r}
	viaLocal(h, 1)
	fmt.Println(r.future[1].n) // 1
}
