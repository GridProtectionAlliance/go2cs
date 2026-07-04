package main

import (
	"fmt"
	"unsafe"
)

// A (*[N]T)(ptr)[:n] pointer-cast slice is emitted as a golib slice<T> (not a bare Span<T>), so it
// ranges as (index, element) and supports element-address Ꮡ(s, i) — mirrors runtime's printDebugLog
// (range + &state[i]) and os_windows (range over an unsafe []byte). This is a COMPILE + target guard:
// the values flow through the unsafe.Pointer(&arr) round-trip (a transient fixed address → stale, the
// known unsafe.Pointer=nuint limitation), so the runtime values are not output-compared — only that the
// range and element-address forms over a pointer-cast slice compile.
func main() {
	arr := [4]int{10, 20, 30, 40}
	s := (*[4]int)(unsafe.Pointer(&arr))[:4]

	sum := 0
	for i := range s { // index-only range over a pointer-cast slice
		sum += i
	}

	total := 0
	for _, v := range s { // value range
		total += v
	}

	for i := range s { // element-address through &s[i]
		ps := &s[i]
		*ps += 1
	}

	fmt.Println(sum, total, s[0])

	// census F13: a DOUBLE deref of a double-pointer reinterpret - reflect MapOf's
	// `**(**mapType)(unsafe.Pointer(&imap))` - the outer deref must parenthesize the
	// inner `~`-form deref before appending .Value, or the postfix binds into the cast
	// chain and reads the inner unsafe.Pointer slot (CS0029). Compile-shape only.
	ip := &arr
	back := **(**[4]int)(unsafe.Pointer(&ip))
	_ = back

	// census F14: a literal with an unsafe.Pointer result whose arms return different
	// C#-typed expressions defeats lambda return inference (CS8917, reflect deepEqual
	// ptrval); the emitted lambda states its return type explicitly.
	pick := func(u bool) unsafe.Pointer {
		if u {
			return unsafe.Pointer(uintptr(0))
		}
		return unsafe.Pointer(ip)
	}
	_ = pick(true)

	// A NAMED-over-pointer conversion from a raw address (syscall.Pointer shape,
	// internal/poll WSAMsg.Name CS0030 x6): hops uintptr then the named operator -
	// ((opaque)(zh<nint>)(uintptr)(...)). Compile-shape only.
	op := opaque(unsafe.Pointer(ip))
	_ = op
}

// opaque mirrors syscall.Pointer: a named type whose underlying is a pointer.
type opaque *[4]int

