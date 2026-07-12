package main

import (
	"fmt"
	"unsafe"
)

type T struct{ x int }

func main() {
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
