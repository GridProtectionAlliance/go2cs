// Regression test: a Go ARRAY value returned through a CAST-DEREF must clone with the deref
// WRAPPED — `(~(ж<T>)(…)).Clone()`, not `~(ж<T>)(…).Clone()`.
//
// `*(*T)(p)` renders with the PREFIX `~` deref operator (convStarExpr's casted-pointer-deref
// path). C# postfix binds tighter than unary, so appending the array-copy `.Clone()` suffix
// naked re-binds it onto the cast's INNER operand — the unsafe.Pointer — instead of the
// dereferenced array. reflect's InterfaceData
//
//	return *(*[2]uintptr)(v.ptr)
//
// emitted `~(ж<array<uintptr>>)(uintptr)(v.ptr).Clone()` and failed to compile with CS1061
// (@unsafe.Pointer has no Clone), which blocked the whole converted stdlib corpus through
// fmt→reflect. The clone must wrap the deref (golib appendArrayValueClone).
//
// The TYPED-pointer half of this file is OUTPUT-COMPARED. `*(*T)(p)` where p is already `*T`
// is a semantic identity, and it used to route through the raw-address uintptr bridge
// (`(ж<T>)(uintptr)(p)`) even though its source is a managed box, not an address — CS0030 on a
// deref-aliased parameter. It now emits the box directly (`~Ꮡp`), so the cast-deref RUNS, and
// the array-copy semantics this guard is named for can finally be proven by VALUE rather than
// by compile shape: mutating the returned array must not disturb the pointed-to original.
//
// The unsafe.Pointer half stays COMPILE-shape only (its results are deliberately discarded):
// reconstructing an array through an unsafe.Pointer round trip reads raw memory, which cannot
// reproduce Go's values under go2cs's managed model — the deliberate "raw-metal on non-native
// types" fork.
package main

import (
	"fmt"
	"unsafe"
)

type Row [3]int

// ---- TYPED-pointer cast-deref: output-compared ----

// The shape that mis-bound, now reachable at run time: a named array read back through a
// cast-deref of a pointer that already has that type.
func typedCastDeref(p *Row) Row {
	return *(*Row)(p)
}

// The same for an UNNAMED array element type, matching reflect's [2]uintptr.
func typedCastDerefDirect(p *[2]uintptr) [2]uintptr {
	return *(*[2]uintptr)(p)
}

// A cast-deref feeding a composite-literal element — a distinct clone-append site.
type holder struct {
	r Row
}

func typedCastDerefIntoStruct(p *Row) holder {
	return holder{r: *(*Row)(p)}
}

// A cast-deref used as an LVALUE must write THROUGH the pointer, not to a copy.
func typedCastDerefAssign(p *Row) {
	*(*Row)(p) = Row{40, 50, 60}
}

// ---- unsafe.Pointer cast-deref: compile-shape only ----

func castDerefReturn(p unsafe.Pointer) Row {
	return *(*Row)(p)
}

func castDerefReturnDirect(p unsafe.Pointer) [2]uintptr {
	return *(*[2]uintptr)(p)
}

func main() {
	// The clone must COPY: mutating the returned array leaves the original alone.
	src := Row{1, 2, 3}
	got := typedCastDeref(&src)
	got[0] = 99
	fmt.Println("copy:", got, "original:", src)

	var pair = [2]uintptr{7, 8}
	gotPair := typedCastDerefDirect(&pair)
	gotPair[1] = 77
	fmt.Println("direct copy:", gotPair, "original:", pair)

	h := typedCastDerefIntoStruct(&src)
	h.r[2] = 33
	fmt.Println("struct field copy:", h.r, "original:", src)

	// The lvalue form writes through the pointer.
	typedCastDerefAssign(&src)
	fmt.Println("assigned through:", src)

	// Compile-shape coverage only — results discarded (see the file comment).
	var r Row
	var u [2]uintptr
	_ = castDerefReturn(unsafe.Pointer(&r))
	_ = castDerefReturnDirect(unsafe.Pointer(&u))
}
