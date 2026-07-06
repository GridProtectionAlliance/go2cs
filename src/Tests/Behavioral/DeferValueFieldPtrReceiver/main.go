package main

import "fmt"

type counter struct{ n int }

func (c *counter) reset() { c.n = 0 }
func (c *counter) inc()   { c.n++ }

type builder struct{ c counter }

// A pointer-receiver method whose body DEFERS a pointer-receiver method (`reset`) on a VALUE field
// (`c counter`) reached through a nested selector — the runtime/pprof `defer b.deck.reset()` shape.
// Go auto-takes `&b.c`; the deferred call must run at scope exit and, because `b` is a pointer, the
// reset must ALIAS the real `b.c` (observed after the method returns).
func (b *builder) run() {
	defer b.c.reset()
	b.c.inc()
	b.c.inc()
	fmt.Println("inside:", b.c.n) // inside: 2
}

func main() {
	b := &builder{}
	b.run()
	fmt.Println("after:", b.c.n) // after: 0 — reset aliased the real b.c through the box

	// The same shape but the base is a pointer LOCAL (not a receiver), in a deferring closure.
	x := &builder{}
	func() {
		defer x.c.reset()
		x.c.inc()
		fmt.Println("inside2:", x.c.n) // inside2: 1
	}()
	fmt.Println("after2:", x.c.n) // after2: 0
}
