package main

import "unsafe"

// This test locks in the go2cs conversion of a pointer-type conversion whose SOURCE is a raw address:
// `(*unsafe.Pointer)(p)` and its deref `*((*unsafe.Pointer)(p))`. Because unsafe.Pointer is the golib
// `Pointer : ж<uintptr>`, a direct `(ж<@unsafe.Pointer>)p` needs the two chained user-defined conversions
// Pointer->uintptr->ж<Pointer> that C# rejects (CS0030). The converter routes through uintptr instead:
// `(ж<@unsafe.Pointer>)(uintptr)(p)`, reading the T at p's address. Two shapes are exercised:
//   - indirectKey: `*((*unsafe.Pointer)(k))` — the extra-paren deref, as in runtime/map.go's indirect key.
//   - writeBarrier: `(*unsafe.Pointer)(ptr)` as a bare call ARGUMENT, as in runtime/atomic_pointer.go's
//     `atomicwb((*unsafe.Pointer)(ptr), new)`.
// This is memory-layout-dependent (raw-address) code — golib's map<K,V> is what actually runs at runtime,
// so the converted runtime/map.go only needs to COMPILE. Reading a managed unsafe.Pointer back out of a
// raw address is not faithfully representable in the managed model, so the runtime VALUES are not the
// contract here: this is a Compile + Target (golden byte-comparison) test, NOT an output-comparison test.

//go:noinline
func indirectKey(k unsafe.Pointer) unsafe.Pointer {
	return *((*unsafe.Pointer)(k))
}

//go:noinline
func store(dst *unsafe.Pointer, val unsafe.Pointer) {
	*dst = val
}

//go:noinline
func writeBarrier(ptr unsafe.Pointer, val unsafe.Pointer) {
	store((*unsafe.Pointer)(ptr), val)
}

// funcAt derefs a reinterpret whose starred inner is a FUNC TYPE, inside a TUPLE return —
// runtime panic.go's `return *(*func())(add(p.slotsPtr, …)), true`. A func-type starred inner
// has no identifier, so this misses the ident-gated cast-deref branch and falls to the default
// deref, which must WRAP the cast before `.Value` (C# postfix beats cast precedence — a naked
// `.Value` re-binds onto the cast's inner operand: CS0029 ж<Action>→Action in the tuple).
//
//go:noinline
func funcAt(p unsafe.Pointer) (func(), bool) {
	return *(*func())(p), true
}

// zeroPair INDEXES a reinterpret-cast result directly — runtime malloc.go's
// `(*[2]uint64)(x)[0] = 0`. The pointer-to-array auto-deref `.Value` (and the index itself) must
// wrap the cast: postfix beats cast precedence, so a naked append read the inner
// @unsafe.Pointer's uintptr and indexed a nuint (CS0021). Copy-box seam: the write hits the
// boxed copy (documented reinterpret contract) — Compile + Target only, like the rest of this
// test.
//
//go:noinline
func zeroPair(x unsafe.Pointer) uint64 {
	(*[2]uint64)(x)[0] = 0
	(*[2]uint64)(x)[1] = 0
	return (*[2]uint64)(x)[0]
}

// linkaddr is a NAMED type over uintptr — runtime's gclinkptr (`type gclinkptr uintptr`, the
// allocator's span-address arithmetic type). Converting one to unsafe.Pointer
// (`unsafe.Pointer(v)`, malloc/mcache/stack) needs the underlying hop
// `((@unsafe.Pointer)(uintptr)v)`: the [GoType] wrapper converts named↔underlying only, and
// golib's Pointer converts from uintptr only — the direct cast is a two-op chain C# won't
// build (CS0030). A named-uintptr value is a pure NUMBER (never a managed reference), so the
// round-trip is value-exact and asserted below.
type linkaddr uintptr

//go:noinline
func throughPointer(v linkaddr) uintptr {
	return uintptr(unsafe.Pointer(v))
}

func main() {
	// Back the pointers with a real uintptr slot (avoids a bare `var _ unsafe.Pointer` zero-value
	// declaration, which is an unrelated converter gap: unsafe.Pointer has no parameterless C#
	// constructor). This test only needs to exercise the (*unsafe.Pointer)(p) emission and COMPILE.
	var backing uintptr
	p := unsafe.Pointer(&backing)
	writeBarrier(p, p)
	_ = indirectKey(p)
	var fslot func()
	_, ok := funcAt(unsafe.Pointer(&fslot))
	_ = ok
	var pair [2]uint64
	_ = zeroPair(unsafe.Pointer(&pair))

	// named-uintptr -> unsafe.Pointer -> uintptr round-trip: pure number plumbing, value-exact.
	// (Explicit-typed decl: `base := linkaddr(0x4000)` elides the conversion — the untyped const
	// adopts the target type in Go — leaving C#'s `var` to infer int; a separate documented shape.)
	var base linkaddr = 0x4000
	next := linkaddr(uintptr(base) + 32)
	println(throughPointer(base) == 0x4000, throughPointer(next)-throughPointer(base))

	println("compiled")
}
