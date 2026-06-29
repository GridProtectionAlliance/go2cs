package main

import "fmt"

// A local variable named after a colliding package identifier (a type `p` plus a method `p` on
// another type forces `p` into the type-vs-method collision set) is renamed `Δp`, but when its
// address is taken it is heap-boxed and the box keeps the RAW Go name: `ref var Δp = ref
// heap<box>(out var Ꮡp)`. Taking its address `&p` — e.g. passing it to a function — must reference
// the box by the raw name `Ꮡp`, not `Ꮡ`+the renamed alias (`ᏑΔp`, which is not in scope → CS0103).
// This is the escaping-local analogue of the renamed-receiver/parameter box (RenamedReceiverBox);
// runtime hit it in `gopanic` (`var p _panic; … preprintpanics(&p)` with `p` colliding the type `p`).
// Contrast: a nested-scope SHADOW rename (`i` → `iΔ1`) keeps the SHADOW box name (`ᏑiΔ1`), so only
// the collision form is rewritten to the raw name.

type p struct{ id int }

type tagger struct{}

func (tagger) p() {}

type box struct{ n int }

func setN(b *box, v int) { b.n = v }

// References the colliding type `p` and method `p` in a scope with no local `p` shadow.
func usesTypeP() int {
	var pv p
	pv.id = 1
	var t tagger
	t.p()
	return pv.id
}

func main() {
	p := box{n: 0} // local `p` collides with type `p` -> alias Δp, heap box Ꮡp
	setN(&p, 7)    // &p -> Ꮡp (raw box), NOT ᏑΔp
	setN(&p, p.n+3)
	fmt.Println(p.n, usesTypeP()) // 10 1
}
