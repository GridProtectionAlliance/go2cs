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

// A pointer-receiver method can ALSO capture its receiver through a VALUE-receiver METHOD VALUE — the
// receiver expression carries no explicit func literal, but the converter synthesizes a wrapping lambda
// (`() => Ꮡc.Value.field.M()`) because a C# extension over a value receiver has no delegate (CS1113).
// That lambda captures the receiver exactly as a func literal does, so the method must likewise be
// promoted direct-ж or the `ref counter c` receiver captured by the lambda is CS1628. This mirrors
// crypto/internal/hpke's `func (kdf *hkdfKDF) LabeledExtract(...) { ... hkdf.Extract(kdf.hash.New, ...) }`,
// where `kdf.hash.New` is a value-receiver method value on the receiver's `crypto.Hash` field. Guards the
// bodyCapturesReceiverInValueMethodValue direct-ж trigger.

type label int

func (l label) render() string { return fmt.Sprintf("L%d", int(l)) }

type widget struct{ id label }

// tag is a VALUE-receiver method on widget itself (the bare-receiver method-value shape).
func (w widget) tag() string { return fmt.Sprintf("W%d", int(w.id)) }

func call(f func() string) string { return f() }

// Receiver captured via a value-receiver method value on a value FIELD-CHAIN (`w.id.render`) — the
// exact hpke shape.
func (w *widget) viaFieldMethodValue() string { return call(w.id.render) }

// Receiver captured via a value-receiver method value on the BARE receiver (`w.tag`).
func (w *widget) viaBareMethodValue() string { return call(w.tag) }

func main() {
	c := &counter{n: 0}
	fmt.Println(c.addInClosure(5))    // 5
	fmt.Println(c.addViaFieldPtr(3))  // 8

	add := c.makeAdder()
	add(10)
	add(2)
	fmt.Println(c.n) // 20

	w := &widget{id: 42}
	fmt.Println(w.viaFieldMethodValue()) // L42
	fmt.Println(w.viaBareMethodValue())  // W42
}
