package main

import (
	"fmt"
	"unsafe"
)

// Locks in the pointer-display/identity behaviors that killed the strings test host (TestClone):
// unsafe.StringData of an EMPTY string is nil — and therefore identical across distinct empty
// strings (Go's zero string has a nil data pointer) — and printing a data pointer yields an
// address-like token. golib previously pinned a fresh empty buffer per StringData("") call, so
// empty-string data pointers never compared equal, and PRINTING one dereferenced the zero index
// of the empty buffer and crashed the host. Addresses differ run to run, so the non-nil check
// verifies the printed SHAPE (0x-prefixed token) rather than the value. The out-of-range
// element-reference print shape itself has no Go-parity spelling here (unsafe.Add loses the
// element box through the unsafe.Pointer seam) — that property is guarded at the golib level by
// GolibTests.PointerPrintTests instead.

func main() {
	e1 := unsafe.StringData("")
	var empty string
	e2 := unsafe.StringData(empty)

	fmt.Println(e1 == nil, e1 == e2)
	fmt.Println(e1)

	q := unsafe.StringData("abc")
	t := fmt.Sprintf("%v", q)
	fmt.Println(q != nil, len(t) > 2, t[0] == '0', t[1] == 'x')
}
