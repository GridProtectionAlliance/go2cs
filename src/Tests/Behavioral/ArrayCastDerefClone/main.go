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
// This is a COMPILE-shape guard (no [GoTestMatchingConsoleOutput]): the defect is a C#
// compile error, and reconstructing an array through unsafe.Pointer cannot RUN under go2cs's
// managed model — an unsafe.Pointer is a nuint that cannot carry a managed box across the
// round trip (the deliberate "raw-metal on non-native types" fork). The runtime semantics of
// the array copy itself are covered by ArrayValueCopySites' other gap classes.
package main

import "unsafe"

type Row [3]int

// The cast-deref return: the shape that mis-bound.
func castDerefReturn(p unsafe.Pointer) Row {
	return *(*Row)(p)
}

// A direct (unnamed) array element type takes the same path, matching reflect's [2]uintptr.
func castDerefReturnDirect(p unsafe.Pointer) [2]uintptr {
	return *(*[2]uintptr)(p)
}

func main() {
	var r Row
	var u [2]uintptr

	_ = castDerefReturn(unsafe.Pointer(&r))
	_ = castDerefReturnDirect(unsafe.Pointer(&u))
}
