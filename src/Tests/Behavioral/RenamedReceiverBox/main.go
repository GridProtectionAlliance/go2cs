package main

import "fmt"

// A method receiver named after a colliding package identifier — runtime's `func (p *cpuProfile)
// add()` where `p` collides with the type `p` — is shadow-renamed (`Δp`), but its box keeps the
// raw name (`ref var Δp = ref Ꮡp.val`). Calling a pointer-receiver (capture-mode) method on that
// receiver must route through the box `Ꮡp`, not `Ꮡ`+the renamed value alias (`ᏑΔp`, which is not
// in scope → CS0103). Guards the raw-box-name routing in convSelectorExpr.

// Force `p` into the type-vs-method collision set so any receiver/var named `p` is renamed `Δp`.
type p struct{ id int }

type tagger struct{}

func (tagger) p() {}

type counter struct{ n int }

// A capture-mode method (takes the address of its receiver's field).
func (c *counter) add(d int) { addInt(&c.n, d) }

func addInt(x *int, d int) { *x += d }

// The receiver here is named `p` (renamed `Δp`, box `Ꮡp`); it calls the capture-mode method `add`.
func (p *counter) bumpTwice() {
	p.add(1) // p.add(1) -> Ꮡp.add(1), NOT ᏑΔp.add(1)
	p.add(1)
}

func main() {
	c := &counter{n: 0}
	c.bumpTwice()
	fmt.Println(c.n) // 2

	// Keep the colliding type/method referenced.
	var pv p
	pv.id = 7
	var t tagger
	t.p()
	fmt.Println(pv.id) // 7
}
