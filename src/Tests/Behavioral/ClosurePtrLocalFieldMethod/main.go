package main

import "fmt"

// A pointer LOCAL written inside a closure routes to shared storage (the write-visibility
// capture routing), heap-boxing it one level up: `ref var c = ref heap<ж<T>>(out var Ꮡc)` —
// the box `Ꮡc` is a `ж<ж<T>>`, and the uncapturable ref-local alias `c` is the box's slot.
// A pointer-receiver method called through a FIELD of that local INSIDE the closure must
// field-ref through the HELD pointer (`Ꮡc.ValueSlot.of(T.Ꮡfield)`), not the bare box
// (`Ꮡc.of(…)` fed the `ж<ж<T>>` to `.of` → CS0411; runtime allocmcache's
// `c.flushGen.Store(…)` inside systemstack — the census-gate regression this guards).

type counter struct {
	n int
}

// bump has a POINTER receiver, so a call through a value field takes the field's address
// via the &-machinery (`.of(…)`) — the defective render position.
func (c *counter) bump(delta int) {
	c.n += delta
}

func (c *counter) get() int {
	return c.n
}

type cachelike struct {
	flush counter
	pad   int
}

//go:noinline
func run(f func()) { f() }

//go:noinline
func alloc() *cachelike {
	return &cachelike{}
}

// allocShape mirrors runtime allocmcache: declare the pointer, then inside the closure
// write it and immediately method-call through a value field, then read back after the
// closure — proving both the write-through (the assignment landed in the shared box) and
// the in-closure field-method call (the `.of(…)` bound the real storage, not a copy).
func allocShape() (int, int) {
	var c *cachelike
	run(func() {
		c = alloc()      // write through the box: Ꮡc.ValueSlot = …
		c.flush.bump(41) // field-method through the held pointer: Ꮡc.ValueSlot.of(…).bump(41)
		c.pad = 7        // plain field write through the held pointer
	})
	c.flush.bump(1) // outside the closure the alias is the ж<T> itself: c.of(…)
	return c.flush.get(), c.pad
}

type gauge struct {
	v counter
}

//go:noinline
func newGauge() *gauge {
	return &gauge{}
}

// namedAfterType pins the accessor-type qualification: the pointer local is NAMED after its
// own type (`gauge`, declared `ж<gauge>`), so inside the closure a bare owning-type reference
// `gauge.Ꮡv` would bind the enclosing uncapturable ref-local (no identical-simple-name
// fallback — the declared type differs from the type name) → the box-deref receiver
// qualifies the owning type (`Ꮡgauge.ValueSlot.of(main_package.gauge.Ꮡv)`).
func namedAfterType() int {
	gauge := newGauge()
	run(func() {
		gauge = newGauge() // written in the closure → shared-storage (box-ref) routing
		gauge.v.bump(5)
	})
	gauge.v.bump(2)
	return gauge.v.get()
}

func main() {
	a, b := allocShape()
	fmt.Println(a, b) // 42 7
	fmt.Println(namedAfterType()) // 7
}
