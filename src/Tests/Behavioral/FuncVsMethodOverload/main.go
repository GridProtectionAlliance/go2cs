package main

import (
	"fmt"
	"unsafe"
)

// Locks in overload resolution when a FREE function and a pointer-receiver METHOD share a name —
// runtime's `func add(p unsafe.Pointer, x uintptr)` (stubs.go) vs `func (p *notInHeap) add(bytes
// uintptr)` (malloc.go). Both emit as static `add` overloads in the package class: the free one
// takes `@unsafe.Pointer`, the method companion takes the box `ж<nih>`. A free-call site passes
// the pin `(uintptr)@unsafe.Pointer.FromRef(ref v)` — a `uintptr` — and golib's ж<T> had an
// IMPLICIT `uintptr → ж<T>` operator, so the argument converted to BOTH first parameters (CS0121,
// 6 runtime sites in map.go/mprof.go). The operator is now EXPLICIT (a raw-address reinterpret
// boxes a COPY of the pointed-at value and dereferences an arbitrary address — never something to
// happen silently; emitted reinterprets always use explicit cast syntax `(ж<T>)(uintptr)(p)`), so
// the free call binds the `@unsafe.Pointer` overload exactly.
//
// Reads go through transient pins within a single expression (the documented reinterpret-seam
// contract); only READS are exercised, matching the runtime sites.

// the free function (runtime stubs.go shape)
//
//go:noinline
func add(p unsafe.Pointer, x uintptr) unsafe.Pointer {
	return unsafe.Pointer(uintptr(p) + x)
}

type nih struct{ a, b uint32 }

// the same-named pointer-receiver method (runtime notInHeap shape). The `return p` branch makes
// the method direct-ж (box receiver `ж<nih>`), matching notInHeap's emitted form — the exact
// overload pair that was ambiguous.
func (p *nih) add(bytes uintptr) *nih {
	if bytes == 0 {
		return p
	}
	return (*nih)(unsafe.Pointer(uintptr(unsafe.Pointer(p)) + bytes))
}

// the runtime call shape: the free add called with a pin of a deref-aliased pointer PARAMETER
// (map.go's `add(unsafe.Pointer(b), …)` inside a *bmap method).
//
//go:noinline
func step(v *uint32) uint32 {
	q := (*uint32)(add(unsafe.Pointer(v), unsafe.Sizeof(uint32(0))))
	return *q
}

type holder struct{ x, y uint32 }

// THE ambiguous site shape: a plain [GoRecv] ref receiver has NO box, so `unsafe.Pointer(h)` pins
// via `(uintptr)@unsafe.Pointer.FromRef(ref h)` — a `uintptr`-typed argument, which under the old
// implicit `uintptr → ж<T>` operator converted to BOTH `add` overloads (map.go's `b.keys()` /
// `b.overflow()` and mprof.go's stack-record walkers).
func (h *holder) second() uint32 {
	return *(*uint32)(add(unsafe.Pointer(h), unsafe.Sizeof(uint32(0))))
}

func main() {
	// free add: step one uint32 forward within an array
	vals := [4]uint32{10, 20, 30, 40}
	fmt.Println(step(&vals[0])) // 20

	// free add called directly with a pinned local address
	p := unsafe.Pointer(&vals[1])
	fmt.Println(*(*uint32)(add(p, 8))) // 40

	// method add: the return-receiver branch (bytes == 0) round-trips the same pointer
	n := nih{a: 7, b: 9}
	m := n.add(0)
	fmt.Println(m.a, m.b) // 7 9

	// method add: step to field b (offset 4) through the receiver-derived address
	pb := (*uint32)(unsafe.Pointer(n.add(4)))
	fmt.Println(*pb) // 9

	// the boxless-receiver pin — the exact previously-ambiguous argument shape
	hh := holder{x: 3, y: 5}
	fmt.Println(hh.second()) // 5
}
