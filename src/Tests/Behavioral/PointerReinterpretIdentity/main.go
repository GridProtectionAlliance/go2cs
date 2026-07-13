package main

// Regression guard for the strings.Builder copyCheck false-panic. Go's copy-by-value guard writes
// `b.addr = (*builder)(noescape(unsafe.Pointer(b)))`, which the language spec makes exactly
// `b.addr = b` (an escape-analysis hack; the type's own TODO says to revert it once escape analysis
// improves). Converting a `*builder` to unsafe.Pointer and back to `*builder` is a no-op IDENTITY, so
// the converter must emit the receiver's BOX directly (`b.addr = Ꮡb`). The old rendering round-tripped
// through uintptr, which golib's `(ж<T>)(uintptr) => new ж<T>(*(T*)value)` resolved by DEREFERENCING-
// and-COPYING: `b.addr` became a fresh box over a COPY of the receiver, never reference-equal to the
// receiver, so the self-check `b.addr != b` FALSE-PANICKED on the second method call. This program
// exercises both directions of the guard — a normal builder used across several calls must NOT panic,
// and a genuine copy-by-value MUST still be caught — so a regression re-appears as an output mismatch.

import (
	"fmt"
	"unsafe"
)

// noescape hides a pointer from escape analysis, returning it unchanged (identity). Mirrors
// internal/abi.NoEscape and strings.Builder's historical package-local helper.
//
//go:nosplit
func noescape(p unsafe.Pointer) unsafe.Pointer {
	x := uintptr(p)
	//lint:ignore SA4016 identity by construction — the point is to defeat escape analysis
	return unsafe.Pointer(x ^ 0)
}

// builder mirrors strings.Builder's copy-by-value guard.
type builder struct {
	addr *builder
	buf  []byte
}

func (b *builder) copyCheck() {
	if b.addr == nil {
		// (*builder)(noescape(unsafe.Pointer(b))) is Go's escape-hiding idiom for `b.addr = b`.
		b.addr = (*builder)(noescape(unsafe.Pointer(b)))
	} else if b.addr != b {
		panic("builder: illegal use of non-zero builder copied by value")
	}
}

func (b *builder) write(s string) {
	b.copyCheck()
	b.buf = append(b.buf, s...)
}

func (b *builder) String() string { return string(b.buf) }

func main() {
	// Several method calls on ONE builder: copyCheck runs each time; every call after the first hits
	// the `b.addr != b` branch, which must stay false (the bug made it true → a spurious panic).
	var b builder
	b.write("hello")
	b.write(", ")
	b.write("world")
	fmt.Println(b.String())

	// A GENUINE copy-by-value must still be detected: cp.addr keeps pointing at &src, not &cp.
	func() {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println("caught:", r)
			}
		}()
		var src builder
		src.write("seed") // src.addr now self-points
		cp := src         // copy by value — cp is a distinct object, cp.addr still points at src
		cp.write("more")  // copyCheck: cp.addr (&src) != &cp → panic
		fmt.Println("unreachable")
	}()
}
