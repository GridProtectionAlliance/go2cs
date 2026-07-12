// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted pool.go output — the next
// member of the native sync family after Mutex/RWMutex/WaitGroup/runtime hooks). Go's Pool is a
// per-P sharded cache co-designed with the scheduler (procPin pins the goroutine to a P and
// indexes a GC-managed poolLocal array); converted literally it fails on two fronts: the
// poolLocal shards are slice elements whose promoted-embed boxes only constructors create (a
// zero-valued element NREs on first field access — fmt's first Println), and the pin/victim-GC
// choreography presumes Go's runtime. Reimplemented natively: one ConcurrentBag per Pool
// (thread-safe, lock-free work-stealing), lazily created through the Pool's heap box so every
// user of the same boxed Pool shares it. Pool semantics permit dropping cached items at any
// time; this implementation lets the bag live for the Pool's lifetime (no victim-cache GC
// integration — items are reclaimed with the Pool itself).
using System;
using System.Collections.Concurrent;

// Hand-owned native replacement of the converted pool.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

partial class sync_package {

// A Pool is a set of temporary objects that may be individually saved and
// retrieved.
//
// Any item stored in the Pool may be removed automatically at any time without
// notification. If the Pool holds the only reference when this happens, the
// item might be deallocated.
//
// A Pool is safe for use by multiple goroutines simultaneously.
//
// A Pool must not be copied after first use.
[GoType] partial struct Pool {
    // items lazily materializes through the Pool's heap box on first Put/Get, so all
    // access through the same boxed Pool shares one bag (never through a copy - Go
    // forbids copying a Pool after first use).
    internal ConcurrentBag<any>? items;

    // New optionally specifies a function to generate
    // a value when Get would otherwise return nil.
    // It may not be changed concurrently with calls to Get.
    public Func<any> New;
}

// Put adds x to the pool.
[GoRecv] public static void Put(this ref Pool p, any x) {
    if (x is null or NilType) {
        return;
    }

    (p.items ??= new ConcurrentBag<any>()).Add(x);
}

// Get selects an arbitrary item from the [Pool], removes it from the
// Pool, and returns it to the caller.
// Get may choose to ignore the pool and treat it as empty.
// Callers should not assume any relation between values passed to [Pool.Put] and
// the values returned by Get.
//
// If Get would otherwise return nil and p.New is non-nil, Get returns
// the result of calling p.New.
[GoRecv] public static any Get(this ref Pool p) {
    ConcurrentBag<any>? items = p.items;

    if (items is not null && items.TryTake(out any? item)) {
        return item!;
    }

    if (p.New != default!) {
        return p.New();
    }

    return default!;
}

// The runtime-provided hooks below stay declared here (their home file in Go is pool.go);
// sync's native runtime_impl.cs supplies the implementing parts, and other sync files
// (map.cs, once, cond) reference some of them.

internal static partial uint32 runtime_randn(uint32 n);

internal static partial void runtime_registerPoolCleanup(Action cleanup);

internal static partial nint runtime_procPin();

internal static partial void runtime_procUnpin();

// The below are implemented in internal/runtime/atomic and the
// compiler also knows to intrinsify the symbol we linkname into this
// package.

internal static partial uintptr runtime_LoadAcquintptr(ж<uintptr> ptr);

internal static partial uintptr runtime_StoreReluintptr(ж<uintptr> ptr, uintptr val);

} // end sync_package
