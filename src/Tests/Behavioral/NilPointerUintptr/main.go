package main

import (
	"fmt"
	"unsafe"
)

type T struct{ x int }

// Package-scope initializers reach the converter with NO enclosing function, so
// v.currentFuncSignature is nil. convCallExpr's unsafe.Pointer special case (which rewrites the
// arg into a ref-based extension call when the current function is a pointer-receiver method)
// dereferenced that signature unconditionally, panicking with a nil-pointer dereference during
// CONVERSION of any package-level `uintptr(unsafe.Pointer(...))`. cmp_test.go declares exactly
// these two package vars, which is what blocked cmp's Phase-4 validation.
var global = T{x: 42}
var gPtr uintptr = uintptr(unsafe.Pointer(&global)) // real (non-zero) address
var gNil uintptr = uintptr(unsafe.Pointer(nil))     // 0

func main() {
	// Package-scope forms (the construct that crashed the converter). gNil additionally exercises
	// golib's Pointer->uintptr conversion on the null Pointer reference that `unsafe.Pointer(nil)`
	// produces: a nil unsafe.Pointer has uintptr value 0, so it must not dereference the null.
	fmt.Println(gPtr != 0) // true
	fmt.Println(gNil == 0) // true

	// Go: uintptr(unsafe.Pointer(nil)) == 0. golib's ж<T>→uintptr must return 0 for a nil box,
	// not dereference it (which threw) — the syscall wrappers pass nil pointers this way
	// (syscall.Write hands writeFile a nil *Overlapped, then uintptr(unsafe.Pointer(overlapped))).
	var p *T = nil
	fmt.Println(uintptr(unsafe.Pointer(p)) == 0) // true

	// A non-nil pointer has a real (non-zero) address.
	q := &T{x: 5}
	fmt.Println(uintptr(unsafe.Pointer(q)) != 0) // true
	fmt.Println(q.x)                             // 5 (pointer still usable)
}
