// PointerReceiverPointerLocalField guards a pointer-receiver method called on a FIELD
// of a pointer LOCAL — `h.s.inc()` where `h` is a `*holder` local and `inc` has a
// pointer receiver. The method is emitted as a `[GoRecv]` extension needing an
// addressable receiver, but the value-returning `~` dereference of the field is an
// rvalue, so `(~h).s.inc()` fails (CS1510). The converter takes the field's box
// address — `h.of(holder.Ꮡs).inc()` — which binds the `ж` overload. Mirrors runtime's
// `(~c).gp.set(gp)` / `(~c).gp.cas(...)` in coro.
package main

import "fmt"

type slot struct{ n int }

func (s *slot) inc()     { s.n++ }
func (s *slot) add(d int) { s.n += d }
func (s *slot) get() int  { return s.n }

type holder struct {
	s slot
}

//go:noinline
func get(h *holder) *holder { return h }

func main() {
	base := &holder{}
	h := get(base) // *holder LOCAL (heap box, not a ref-aliased parameter)

	h.s.inc()    // pointer-receiver method (writes) on a field of the pointer local
	h.s.inc()
	h.s.add(10)  // 12

	// Read back through the original pointer to confirm the writes reached real storage.
	fmt.Println(base.s.get()) // 12
}
