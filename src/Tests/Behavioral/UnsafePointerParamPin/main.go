package main

import (
	"fmt"
	"unsafe"
)

// Locks in the go2cs conversion of `unsafe.Pointer(p)` where p is a DEREF-ALIASED Go pointer — a
// pointer PARAMETER (`func f(p *uintptr)`) or a pointer RECEIVER (`func (r *utp) m()`). Such a
// pointer renders in the body as the pointed-to VALUE alias (`ref var p = ref Ꮡp.Value`), not a box,
// so the pin-helper emission `@unsafe.Pointer.FromRef(ref (p).Value)` was `.Value` on a plain `nuint`
// → CS1061 ("nuint does not contain a definition for val"). The alias is itself a ref-local into
// the boxed storage, so the converter now takes its ref directly: `FromRef(ref p)`. A genuine box
// (a local, a struct field's address, a call result) keeps the `(box).Value` form. Runtime hits the
// param shape in select.go `unsafe.Pointer(pc0)` and heapdump.go `unsafe.Pointer(pstk)` (both
// *uintptr parameters).
//
// The reads below go through a transient FromRef pin within a single expression (address taken,
// value read immediately), which is stable; values are verified against Go. Only READS are
// exercised: the `(ж<T>)(uintptr)` round-trip boxes a COPY of the pointed-at value (golib's
// `ж<T>(uintptr)` operator), so a WRITE through the reinterpreted pointer would hit the copy —
// the same copy-semantics caveat as every raw-address reinterpret. The runtime sites (select.go,
// heapdump.go) only read through these pins.

type utp uintptr

// the select.go/heapdump.go shape: a *uintptr PARAMETER
//
//go:noinline
func readViaParam(p *uintptr) uintptr {
	q := (*uintptr)(unsafe.Pointer(p))
	return *q
}

// the receiver variant: a deref-aliased pointer RECEIVER
func (r *utp) read() uintptr {
	q := (*uintptr)(unsafe.Pointer(r))
	return *q
}

// receiver SHADOWED by an inner pointer local of the same name: the inner `r` is a genuine box
// (a local `*uintptr`), so it must KEEP the `(box).Value` form. The receiver gate matches by name;
// without the rendered-name check a shadow-renamed `rΔ1` would take the receiver's bare-ref form
// and pin the box reference slot — compiling but reading a garbage address (silent wrong value).
func (r *utp) tricky() uintptr {
	var y uintptr = 111
	{
		r := &y // shadows the receiver
		q := (*uintptr)(unsafe.Pointer(r))
		return *q
	}
}

type holder struct{ v uintptr }

// pick returns an unsafe.Pointer PARAMETER whole (runtime map.go mapaccess1_fat's `return zero`).
// An unsafe.Pointer param renders as a plain VALUE param with NO box, so the return-path's
// pointer-param boxing (`return p` → `Ꮡp`, correct for a genuine *T) must not fire here — it
// referenced a nonexistent box (CS0103 Ꮡzero/Ꮡv/Ꮡfd).
//
//go:noinline
func pick(cond bool, a, zero unsafe.Pointer) unsafe.Pointer {
	if cond {
		return a
	}
	return zero
}

// advance returns the unsafe.Pointer param in a TUPLE (panic.go readvarintUnsafe's shape).
//
//go:noinline
func advance(fd unsafe.Pointer, n uint32) (uint32, unsafe.Pointer) {
	return n + 1, fd
}

// same returns a GENUINE *T param whole — the box form must stay (control).
//
//go:noinline
func same(p *uintptr) *uintptr { return p }

func main() {
	var x uintptr = 42
	fmt.Println(readViaParam(&x))

	var t utp = 77
	fmt.Println(t.read())
	fmt.Println(t.tricky()) // 111 — via the INNER shadowing r, not the receiver

	// genuine-box control: the address of a struct field must KEEP the (box).Value form
	h := holder{v: 9}
	q := (*uintptr)(unsafe.Pointer(&h.v))
	fmt.Println(*q)

	// return-an-unsafe.Pointer-param shapes (whole + in a tuple) + a genuine *T control
	var a, z uintptr = 11, 22
	pa, pz := unsafe.Pointer(&a), unsafe.Pointer(&z)
	fmt.Println(*(*uintptr)(pick(true, pa, pz)), *(*uintptr)(pick(false, pa, pz))) // 11 22
	n2, fd2 := advance(pa, 5)
	fmt.Println(n2, *(*uintptr)(fd2)) // 6 11
	w := same(&a)
	*w = 99
	fmt.Println(a) // 99 — the *T control writes through its box
}
