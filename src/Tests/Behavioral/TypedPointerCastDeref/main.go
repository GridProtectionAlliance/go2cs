// Regression test: a cast-deref whose source is a TYPED pointer must NOT route through the
// raw-address uintptr bridge.
//
// `*(*T)(p)` with p already `*T` is a semantic IDENTITY — Go's way of re-reading a pointer at a
// fixed type. go2cs renders the conversion via convStarExpr's casted-pointer-deref path, which
// sets isPointerCast, and convCallExpr took that flag alone as licence to emit the raw-address
// reinterpret `(ж<T>)(uintptr)(p)`. That bridge exists only to turn an ADDRESS — an
// unsafe.Pointer or uintptr — into a `ж<T>`; a typed Go pointer is a managed BOX, and a
// deref-aliased pointer parameter renders as that box's VALUE alias, so the `(uintptr)` leg had
// no conversion at all:
//
//	CS0030: Cannot convert type 'Pt' to 'uintptr'
//
// The identity is now recognised up front (pointerReinterpretIdentitySource) and the source box
// is emitted directly — `~Ꮡp` — so the deref reads the real storage in place. That is correct for
// a value read, for an lvalue write, and for pointer identity alike, where the uintptr round trip
// would have DEREFERENCED-AND-COPIED through `new ж<T>(*(T*)value)`.
//
// The recognition must stay pinned to a genuine `(*T)(…)` CONVERSION: matching on the argument
// type alone would collapse any one-argument call that takes and returns the same pointer type
// (`advance(a)` → `a`), silently deleting the call.
package main

import (
	"fmt"
	"unsafe"
)

type Pt struct{ X, Y int }

type Count uint32

// Pointer-to-STRUCT.
func derefStruct(p *Pt) Pt {
	return *(*Pt)(p)
}

// Pointer-to-NAMED-NUMERIC.
func derefNamedNum(p *Count) Count {
	return *(*Count)(p)
}

// The same identity reached through an unsafe.Pointer round trip — the strings.Builder copyCheck
// idiom. Must also collapse to the source box, preserving pointer IDENTITY.
func viaUnsafe(p *Pt) *Pt {
	return (*Pt)(unsafe.Pointer(p))
}

// Non-deref identity conversion.
func convIdentity(p *Pt) *Pt {
	return (*Pt)(p)
}

// The cast-deref as an LVALUE must write THROUGH the pointer, not to a copy.
func assignThrough(p *Pt) {
	*(*Pt)(p) = Pt{7, 8}
}

// A cast-deref of a LOCAL pointer (not a parameter) takes the same path.
func derefLocal() Pt {
	pt := Pt{3, 4}
	q := &pt
	return *(*Pt)(q)
}

// Guard against the over-match: a one-argument function taking and returning *Pt is a CALL, not a
// conversion, and must survive.
func advance(p *Pt) *Pt {
	p.X += 100
	return p
}

func main() {
	pt := Pt{1, 2}
	var c Count = 42

	fmt.Println("struct:", derefStruct(&pt))
	fmt.Println("named num:", derefNamedNum(&c))

	// Identity conversions preserve the pointer, so a write through the result is visible.
	viaUnsafe(&pt).Y = 20
	fmt.Println("via unsafe writes through:", pt)

	convIdentity(&pt).X = 10
	fmt.Println("conv identity writes through:", pt)

	// The deref is a COPY: mutating it leaves the original alone.
	got := derefStruct(&pt)
	got.X = 555
	fmt.Println("copy:", got, "original:", pt)

	assignThrough(&pt)
	fmt.Println("assigned through:", pt)

	fmt.Println("local:", derefLocal())

	// The real call must not be elided.
	fmt.Println("call survives:", *advance(&pt))
}
