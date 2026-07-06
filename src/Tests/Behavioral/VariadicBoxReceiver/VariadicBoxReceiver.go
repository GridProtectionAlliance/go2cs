// Guards the RecvGenerator's ж<T> overload for a VARIADIC pointer-receiver method — cryptobyte's
// `func (b *Builder) add(bytes ...byte)` called on a `ж<Builder>` (a closure parameter). The value
// method emits `add(this ref Builder b, params ꓸꓸꓸbyte bytesʗp)`, but the generated ж overload had
// dropped the `params` modifier (`Span<byte> bytesʗp`), so a call passing individual elements
// (`c.add(0xff)`) could not bind it and fell back to the ref-receiver method — CS1929. The overload
// must keep `params`. Exercised through a pointer receiver reached by BOX (via a closure taking a
// `*sink`), calling the variadic method with zero, one, and several elements plus a spread.
package main

import "fmt"

type sink struct {
	buf []byte
}

// add is a VARIADIC pointer-receiver method — its ж<sink> overload must be `params`.
func (s *sink) add(bytes ...byte) {
	s.buf = append(s.buf, bytes...)
}

// with passes a *sink into a closure (the closure's parameter is a box), mirroring cryptobyte's
// AddASN1 BuilderContinuation — the closure calls the variadic method on the box.
func with(s *sink, f func(c *sink)) {
	f(s)
}

func main() {
	s := &sink{}
	with(s, func(c *sink) {
		c.add(0xff)           // single element — the failing shape
		c.add(1, 2, 3)        // several elements
		c.add()               // zero elements
		more := []byte{9, 8}
		c.add(more...)        // spread
	})
	fmt.Println(s.buf) // [255 1 2 3 9 8]

	// direct box call too (not through a closure).
	t := &sink{}
	t.add(7)
	t.add(byte('A'), byte('B'))
	fmt.Println(t.buf) // [7 65 66]
}
