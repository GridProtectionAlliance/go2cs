package main

import "fmt"

type mc struct{ n int }

func clearViaPtr(c *mc) { c.n = 0 }

func bump(p *int) { *p++ }

// A direct-ж receiver method (it takes the address of its own field `&c.n`, so it is emitted with
// the box AS its receiver, `this ж<mc> Ꮡc`). When it ALSO passes the receiver WHOLE to a function
// expecting a `*mc`, that argument must be the box `Ꮡc`, not the deref'd value alias `c` (a value
// cannot bind a `ж<mc>` parameter → CS1503). Mirrors runtime's
// `func (c *mcache) prepareForSweep(){ … stackcache_clear(c) }`.
func (c *mc) reset() {
	bump(&c.n)     // &c.n -> direct-ж receiver
	clearViaPtr(c) // pass the receiver whole -> clearViaPtr(Ꮡc)
}

// The receiver placed whole into a COMPOSITE-LITERAL element whose field is a pointer — the
// runtime symtab.go shape: `func (f *_func) funcInfo() funcInfo { …; return funcInfo{f, mod} }`
// (funcInfo's first field is the embedded *_func). The composite field needs the receiver's BOX,
// which only exists when the method is direct-ж — placing the receiver whole into a composite is
// itself a promotion trigger. Both positional and keyed forms are exercised; identity through the
// composite is verified by writing through the wrapped pointer and reading the original.
type wrap struct {
	p   *mc
	tag int
}

func (c *mc) wrapped() wrap {
	return wrap{c, 7} // positional: the receiver into a pointer field
}

func (c *mc) keyed() wrap {
	return wrap{tag: 8, p: c} // keyed form
}

func main() {
	c := &mc{n: 9}
	c.reset()
	fmt.Println(c.n) // 0

	w := c.wrapped()
	w.p.n = 42 // write through the composite's pointer field
	fmt.Println(c.n, w.tag) // 42 7 — same object as the receiver
	k := c.keyed()
	k.p.n++
	fmt.Println(c.n, k.tag) // 43 8
}
