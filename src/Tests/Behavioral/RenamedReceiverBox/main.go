package main

import "fmt"

// A method receiver / pointer parameter named after a colliding package identifier — runtime's
// `func (p *cpuProfile) add()` where `p` collides with the type `p` — is shadow-renamed (`Δp`), but
// its box keeps the raw Go name (`ref var Δp = ref Ꮡp.Value`). A pointer-receiver (capture-mode)
// method call on that receiver, a closure that *reads* through it, and a closure that takes the
// *address* of one of its fields must all route through the box `Ꮡp`, not `Ꮡ`+the renamed value
// alias (`ᏑΔp`, which is not in scope → CS0103). Guards the raw-box-name routing in
// convSelectorExpr (method call), convIdent (closure read), and convUnaryExpr (closure address-of).

// Force `p` into the type-vs-method collision set so any receiver/parameter named `p` is renamed `Δp`.
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

// A pointer parameter `p` (renamed `Δp`, box `Ꮡp`) captured by a closure that both reads through the
// box (`p.n`) and takes the address of its field (`&p.n`).
func addInClosure(p *counter, d int) int {
	apply := func() {
		p.n += d        // closure read: p.n -> Ꮡp.Value.n, NOT ᏑΔp.Value.n
		addInt(&p.n, d) // closure address-of: &p.n routed through box Ꮡp, NOT ᏑΔp
	}
	apply()
	return p.n
}

func main() {
	c := &counter{n: 0}
	c.bumpTwice()
	fmt.Println(c.n) // 2

	fmt.Println(addInClosure(c, 5)) // 2 + 5 + 5 = 12
	fmt.Println(c.n)                // 12

	// Keep the colliding type/method referenced.
	var pv p
	pv.id = 7
	var t tagger
	t.p()
	fmt.Println(pv.id) // 7
}
