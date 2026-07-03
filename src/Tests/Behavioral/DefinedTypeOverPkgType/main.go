package main

import (
	"fmt"
	"sync/atomic"
	"unsafe"
)

// A DEFINED type over a cross-package named type (`type X pkg.Type`). The converter emits an
// inherited [GoType] wrapper of the named type, not a bare orphan selector (CS1585).
//
// Case 1 (runtime/os_windows): no methods, used only as package vars (`type stdFunction unsafe.Pointer`).
type stdFunction unsafe.Pointer

var handler stdFunction
var other stdFunction

// Case 2 (runtime/mprof): a defined type over a cross-package STRUCT with methods that convert back
// via `(*pkg.Type)(p)` (`type goroutineProfileStateHolder atomic.Uint32`). The pointer-reinterpret
// cast must compile (it boxes the value conversion so the pointer-receiver methods resolve).
type counter atomic.Uint32

func (c *counter) Load() uint32        { return (*atomic.Uint32)(c).Load() }
func (c *counter) Store(v uint32)      { (*atomic.Uint32)(c).Store(v) }
func (c *counter) Add(d uint32) uint32 { return (*atomic.Uint32)(c).Add(d) }

func main() {
	handler = other
	fmt.Println(handler == other)

	// Exercise the method case so it compiles and runs without crashing. (The pointer-reinterpret
	// boxes a copy, so the atomic result is not asserted — atomic intrinsics are platform stubs.)
	var c counter
	c.Store(10)
	_ = c.Add(5)
	fmt.Println("defined-type methods compiled")

	var seed uintptr = 42
	h := handleT(seed)
	k := openKey(h)
	fmt.Println(k == keyT(h), uintptr(h))
}

// Case 3 (internal/syscall/windows/registry): a defined type over a NAMED NUMERIC with an
// explicit VALUE conversion (Key(handle) where type Key syscall.Handle). The [GoType]
// wrapper already provides the implicit operators both ways; recording the conversion
// again made ImplicitConvGenerator emit the identical operator (CS0557).
type handleT uintptr
type keyT handleT

func openKey(h handleT) keyT { return keyT(h) }
