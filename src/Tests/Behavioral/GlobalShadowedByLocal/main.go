package main

import "fmt"

type snapshot struct{ addr int }

// `trace` is BOTH a package-level GLOBAL var and a method name → collision-renamed `Δtrace` (the exact
// runtime traceallocfree shape: global `var trace struct{…}` vs the traceLocker method set).
var trace = snapshot{addr: 42}

type acquirer struct{ n int }

func (acquirer) trace() int { return 1 } // method named `trace` (collides with the global var `trace`)

// collisionGlobalShadow reads the GLOBAL `trace` before declaring a same-named LOCAL — runtime
// traceallocfree's `traceSnapshotMemory` (`trace.minPageHeapAddr` read, then `trace := traceAcquire()`).
//
//go:noinline
func collisionGlobalShadow() int {
	a := trace.addr // reads GLOBAL trace.addr (42) BEFORE the local trace decl
	trace := 7      // local `trace` shadows the global (both C# `Δtrace`)
	return a + trace // 42 + 7 = 49
}

var plainCounter = 100 // a plain global (no collision rename)

// plainGlobalShadow is the same shape without a collision rename — the global-vs-local shadow alone
// still produces CS0841 in C# (function-scoped locals).
//
//go:noinline
func plainGlobalShadow() int {
	x := plainCounter * 2 // reads GLOBAL plainCounter (200) before the local decl
	plainCounter := 5     // local shadows the global
	return x + plainCounter // 200 + 5 = 205
}

func main() {
	fmt.Println(collisionGlobalShadow()) // 49
	fmt.Println(plainGlobalShadow())     // 205
	fmt.Println(trace.addr, plainCounter) // 42 100 (globals unchanged)
}
