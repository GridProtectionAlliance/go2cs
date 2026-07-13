package main

import (
	"fmt"
	"unsafe"
)

// Locks in the go2cs conversion of `(*T)(unsafe.Pointer(p))` where p is a DEREF-ALIASED Go pointer of
// the SAME pointer type `*T` — a pointer PARAMETER (`func f(p *uintptr)`) or a pointer RECEIVER
// (`func (r *utp) m()`). Converting a `*T` to unsafe.Pointer and back to `*T` is a no-op IDENTITY, so
// the converter emits the source pointer's BOX directly (`q := Ꮡp`) rather than round-tripping through
// uintptr. That round-trip resolved through golib's `(ж<T>)(uintptr) => new ж<T>(*(T*)value)`, which
// DEREFERENCES-and-COPIES: the result was a fresh box over a COPY of the pointed-at value, losing
// pointer identity and the shared storage (the bug that false-panicked strings.Builder.copyCheck's
// `b.addr != b` self-check). Emitting the box directly preserves identity AND shared storage, so a
// WRITE through the reinterpreted pointer now flows back (the round-trip's copy caveat is gone here).
//
// This is the same `*T`-to-unsafe.Pointer-to-`*T` shape the runtime hits in select.go
// (`unsafe.Pointer(pc0)`) and heapdump.go (`unsafe.Pointer(pstk)`). The BARE `unsafe.Pointer(p)` pin
// (the deref-aliased-param → `@unsafe.Pointer.FromRef(ref p)` helper, without an identity `(*T)(…)`
// wrapper to collapse) and the GENUINE-reinterpret round-trip (`(*uintptr)(pick(…))` below, whose
// source is an unsafe.Pointer VALUE, not a `*T`) both remain — exercised across the stdlib and by the
// pick/advance cases here. Values are verified against Go.

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
// (a local `*uintptr`), so the identity collapse emits THAT box (`rΔ1`), not the receiver's. The
// identity source is resolved by object identity, so a shadow-renamed `rΔ1` is emitted correctly and
// never mistaken for the receiver (which would read a garbage address — silent wrong value).
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

	// a struct-field address as the identity source: the collapse emits the field box directly
	// (`Ꮡh.of(holder.Ꮡv)`), reading through the real field storage
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
