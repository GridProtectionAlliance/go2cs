package main

import (
	"fmt"
	"sync/atomic"
)

// Package-level atomic globals: a capture-mode method (Store/CompareAndSwap/etc. take the
// address of a receiver field) called on a value global must route through the global's heap
// box, mirroring internal/runtime/exithook's `var locked atomic.Int32; locked.CompareAndSwap(...)`.
var (
	locked  atomic.Int32
	runGoid atomic.Uint64
)

// lockUnlock exercises both a deferred capture-mode method value on a global (`defer
// locked.Store(0)`) and a typed-argument deferred method value (`defer runGoid.Store(0)`,
// where Store takes uint64 — the deferred arg must be typed to the parameter).
func lockUnlock(id uint64) int32 {
	for !locked.CompareAndSwap(0, 1) {
	}
	defer locked.Store(0)
	runGoid.Store(id)
	defer runGoid.Store(0)
	return locked.Load()
}

// localAtomicDefer exercises the same deferred capture-mode method value on a LOCAL atomic
// (also previously mis-snapshotted into a value copy with no box).
func localAtomicDefer() int32 {
	var n atomic.Int32
	n.Store(41)
	defer n.Store(0)
	return n.Load()
}

func main() {
	fmt.Println("while locked:", lockUnlock(7))
	fmt.Println("after unlock:", locked.Load())
	fmt.Println("runGoid:", runGoid.Load())
	fmt.Println("local during:", localAtomicDefer())
}
