package main

import "fmt"

// Locks in the go2cs conversion of a pointer reinterpret from a pointer-to-named-numeric to a
// pointer-to-its-underlying-basic: `(*uint64)(head)` where head is a *lfstack (type lfstack uint64).
// This is the runtime pattern `atomic.Load64((*uint64)(head))` / `atomic.Cas64((*uint64)(head), …)` on
// the named atomic types lfstack / sweepClass / profAtomic / sysMemStat. C# has no `ж<lfstack> → ж<uint64>`
// conversion (distinct generic instantiations), so the converter boxes a COPY of the [GoType] value
// conversion: `atomic.Load64(Ꮡ((uint64)(head)))`. The READ path is faithful (the copy holds the right
// value — the runtime's Load reads it), and it is exercised here vs Go. WRITE-through a reinterpret uses
// copy semantics (the copy is not the original storage); the runtime's Store/Cas/Xadd on these types are
// asm-stub atomics, so that is not the contract — this test asserts only the read path.

// load64 / load32 stand in for internal/runtime/atomic.Load64 / Load (which read *p).
//
//go:noinline
func load64(p *uint64) uint64 { return *p }

//go:noinline
func load32(p *uint32) uint32 { return *p }

type lfstack uint64   // runtime/lfstack.go
type sweepClass uint32 // runtime/mgcsweep.go

func (head *lfstack) peek() uint64 { return load64((*uint64)(head)) }
func (sc *sweepClass) peek() uint32 { return load32((*uint32)(sc)) }

// A non-receiver pointer variable (not a deref-aliased receiver) also reinterprets — the box-deref arm.
func peekVia(p *lfstack) uint64 { return load64((*uint64)(p)) }

func main() {
	var a lfstack = 0xDEADBEEF01
	var b sweepClass = 1234
	fmt.Println(a.peek(), b.peek()) // 956397711105 1234
	fmt.Println(peekVia(&a))        // 956397711105

	var c lfstack = 42
	fmt.Println(c.peek()) // 42
}
