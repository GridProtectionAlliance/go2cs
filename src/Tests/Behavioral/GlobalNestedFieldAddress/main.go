// GlobalNestedFieldAddress guards taking the address of a NESTED field of an
// address-taken package global — `&g.mid.a` where `g` is heap-boxed (its address is
// taken elsewhere). The address must chain box-field accessors,
// `Ꮡg.of(outer.Ꮡmid).of(inner.Ꮡa)`; the converter must recurse on the base selector
// rather than prefixing `Ꮡ` onto `g.mid` (which would bind to the box var `Ꮡg`,
// whose value has no `mid` member → CS1061). Mirrors runtime's
// `&work.sweepWaiters.lock`.
package main

import "fmt"

type inner struct {
	a, b int
}

type outer struct {
	mid inner
	tag int
}

var g outer // package global

//go:noinline
func keep(p *outer) { _ = p } // takes &g so g is address-taken (heap-boxed)

func mutate() {
	p := &g.mid.a // &boxedGlobal.field.subfield -> *int
	*p = 42
	q := &g.mid // &boxedGlobal.field -> *inner
	q.b = 7
}

func main() {
	keep(&g)
	mutate()
	fmt.Println(g.mid.a, g.mid.b, g.tag) // 42 7 0
}
