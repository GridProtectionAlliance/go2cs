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

func main() {
	c := &mc{n: 9}
	c.reset()
	fmt.Println(c.n) // 0
}
