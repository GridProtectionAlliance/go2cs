package main

import "fmt"

// A pointer-receiver method whose receiver is referenced inside a function literal (a closure) must
// be emitted direct-ж — the box passed AS the receiver param (`this ж<counter> Ꮡc`) — because the
// deref'd ref-local alias (`ref var c = ref Ꮡc.Value`) cannot be captured by a C# lambda (CS8175).
// Inside the closure the receiver routes through the box: a value use becomes `Ꮡc.Value.n`, a field
// address `Ꮡc.of(counter.Ꮡn)`. Mirrors runtime's `func (p *_panic) nextFrame() { systemstack(
// func(){ … p.lr … }) }`. Guards the bodyCapturesReceiverInClosure direct-ж trigger.

type counter struct{ n int }

func addInt(x *int, d int) { *x += d }

// Receiver captured by an immediately-invoked closure that reads and writes through it.
func (c *counter) addInClosure(d int) int {
	apply := func() { c.n += d }
	apply()
	return c.n
}

// Receiver captured by a closure that takes the address of its field.
func (c *counter) addViaFieldPtr(d int) int {
	apply := func() { addInt(&c.n, d) }
	apply()
	return c.n
}

// Receiver captured by a closure that is RETURNED (escapes the method) — the box must outlive the
// call, which it does because the box `Ꮡc` is the real heap pointer the caller passed in.
func (c *counter) makeAdder() func(int) {
	return func(d int) { c.n += d }
}

func main() {
	c := &counter{n: 0}
	fmt.Println(c.addInClosure(5))    // 5
	fmt.Println(c.addViaFieldPtr(3))  // 8

	add := c.makeAdder()
	add(10)
	add(2)
	fmt.Println(c.n) // 20
}
