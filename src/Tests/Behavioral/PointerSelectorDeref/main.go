// Locks in the auto-deref of a field selector whose BASE is a pointer-valued EXPRESSION rather than
// an identifier: (a) a CALL RESULT whose ARGUMENT contains a conversion star —
// `stringStructOf((*string)(unsafe.Pointer(p))).n` (runtime arena.go) — where the old
// whole-subtree deref check mistook the argument's `(*string)` star for a dereferenced base and
// skipped the `.val` (CS1061 on the ж<T> box); and (b) an EXTRA-PAREN pointer conversion —
// `((*view)(unsafe.Pointer(&g))).n` (runtime mheap.go) — which failed to reach the conversion
// branch through the enclosing parens (the same extra-paren blind spot the reinterpret routing
// had). READ paths only: a WRITE through the reinterpret hits the copy box (the documented
// reinterpret-seam limitation shared by the whole (ж<T>)(uintptr) family); the runtime sites are
// reads.
package main

import (
	"fmt"
	"unsafe"
)

type header struct {
	tag uintptr
	n   int
}

type view struct {
	tag uintptr
	n   int
}

//go:noinline
func viewOf(hp *header) *view {
	return (*view)(unsafe.Pointer(hp))
}

//go:noinline
func readN(p *header) int {
	// arena.go shape: field select on a CALL RESULT whose ARGUMENT contains a conversion star
	return viewOf((*header)(unsafe.Pointer(p))).n
}

func main() {
	h := header{tag: 7, n: 5}
	fmt.Println(readN(&h)) // 5

	// mheap.go shape: field select on an EXTRA-PAREN pointer conversion
	g := header{tag: 9, n: 3}
	v := ((*view)(unsafe.Pointer(&g))).n
	fmt.Println(v) // 3

}
