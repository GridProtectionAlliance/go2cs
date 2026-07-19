// Guards the conversion of `unsafe.Pointer(p)` where p is a pointer PARAMETER that may legitimately
// be nil.
//
// A pointer parameter is emitted as the box `ж<T> Ꮡp` plus a deref'd VALUE alias
// (`ref var p = ref Ꮡp.Value`). Taking the pointer's address through that alias
// (`@unsafe.Pointer.FromRef(ref p)`) forces the alias to be materialized, so the entry-time deref
// throws a nil-pointer panic for a nil argument — even though Go never touches the pointee and
// `uintptr(unsafe.Pointer(nil))` is simply 0. Rendering the BOX instead
// (`new @unsafe.Pointer(Ꮡp)`) is exact, nil-safe, and additionally leaves the value alias unused so
// it is dropped as dead.
//
// This is the syscall wrapper shape: DuplicateHandle's `lpTargetHandle *Handle` is passed nil (with
// DUPLICATE_CLOSE_SOURCE) to close a handle without receiving a duplicate, and
// syscall.StartProcess does exactly that in a deferred call. Before the fix, spawning any child
// process panicked there. Pointer parameters whose pointee is a basic or struct type already took
// the box form; a NAMED-numeric pointee (`*Handle`) and a pointer-to-pointer (`**uint16`) did not.
package main

import (
	"fmt"
	"unsafe"
)

// Handle is a named type over uintptr, matching syscall.Handle — the pointee shape that used to
// take the deref-alias route.
type Handle uintptr

// addrOfNamed reports the address of a *Handle without ever dereferencing it. A nil argument must
// yield 0, not panic.
//
//go:noinline
func addrOfNamed(h *Handle) uintptr {
	return uintptr(unsafe.Pointer(h))
}

// addrOfPtrPtr is the pointer-to-pointer sibling (syscall's `stringSid **uint16`), which took the
// same deref-alias route.
//
//go:noinline
func addrOfPtrPtr(pp **uint16) uintptr {
	return uintptr(unsafe.Pointer(pp))
}

// addrOfBasic is the control: a basic pointee already used the box form, so its behavior must be
// unchanged.
//
//go:noinline
func addrOfBasic(b *uint16) uintptr {
	return uintptr(unsafe.Pointer(b))
}

// liveAlias keeps the deref'd VALUE genuinely live alongside the unsafe.Pointer use, so the value
// alias must still be emitted and must still observe writes through the pointer. Guards against the
// box rendering wrongly dropping an alias that is actually read.
//
//go:noinline
func liveAlias(h *Handle) (uintptr, Handle) {
	*h = *h + 7
	return uintptr(unsafe.Pointer(h)), *h
}

func main() {
	// A nil pointer parameter converts to the 0 address rather than panicking.
	fmt.Println("named nil  :", addrOfNamed(nil))
	fmt.Println("ptrptr nil :", addrOfPtrPtr(nil))
	fmt.Println("basic nil  :", addrOfBasic(nil))

	// A non-nil pointer yields a non-zero address. The address VALUE is not stable across
	// runtimes, so only its non-nilness is compared.
	var h Handle = 3
	fmt.Println("named nonnil nonzero :", addrOfNamed(&h) != 0)

	var u uint16 = 9
	pu := &u
	fmt.Println("ptrptr nonnil nonzero:", addrOfPtrPtr(&pu) != 0)
	fmt.Println("basic nonnil nonzero :", addrOfBasic(&u) != 0)

	// The value alias stays live and the write through the pointer is observed.
	addr, val := liveAlias(&h)
	fmt.Println("live alias nonzero:", addr != 0, "value:", val, "readback:", h)
}
