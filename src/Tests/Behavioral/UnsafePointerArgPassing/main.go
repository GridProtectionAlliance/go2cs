// UnsafePointerArgPassing guards passing an unsafe.Pointer argument to an
// unsafe.Pointer parameter. The argument must be passed as the `@unsafe.Pointer`
// struct directly, not reduced to its inner `uintptr` via `.val`: a `uintptr`
// converts implicitly to BOTH the `@unsafe.Pointer` parameter AND any same-named
// method's `ж<T>` overload, so the call goes ambiguous (CS0121). Here the free
// `add(unsafe.Pointer, uintptr)` collides with the `(*counter).add` method exactly
// as runtime's free `add` collides with `(*notInHeap).add`. (The result is compared
// numerically — the unsafe.Pointer→managed round-trip is a separate runtime limitation,
// so the test exercises the call resolution, not a write-through-and-read-back.)
package main

import (
	"fmt"
	"unsafe"
)

type counter struct{ n int }

func (c *counter) add(d uintptr) { c.n += int(d) } // method add -> generates a ж<counter> overload

//go:noinline
func add(p unsafe.Pointer, x uintptr) unsafe.Pointer { // free add(unsafe.Pointer, uintptr)
	return unsafe.Pointer(uintptr(p) + x)
}

func main() {
	var base int64 = 100
	p := unsafe.Pointer(&base)

	q := add(p, 16) // free add must resolve unambiguously (pass p, not p.val)
	r := add(p, 0)

	// Pure numeric checks — no managed round-trip through the unsafe pointer.
	fmt.Println(uintptr(q)-uintptr(p) == 16) // true
	fmt.Println(uintptr(r) == uintptr(p))    // true

	c := &counter{}
	c.add(3)
	c.add(4)
	fmt.Println(c.n) // 7
}
