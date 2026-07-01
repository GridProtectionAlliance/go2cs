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

func main() {
	// Back the pointers with a real uintptr slot (avoids a bare `var _ unsafe.Pointer` zero-value
	// declaration, which is an unrelated converter gap: unsafe.Pointer has no parameterless C#
	// constructor). This test only needs to exercise the (*unsafe.Pointer)(p) emission and COMPILE.
	var backing uintptr
	p := unsafe.Pointer(&backing)
	writeBarrier(p, p)
	_ = indirectKey(p)
	println("compiled")
}
