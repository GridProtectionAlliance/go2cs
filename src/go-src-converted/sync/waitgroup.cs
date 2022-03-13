// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2022 March 13 05:24:08 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Program Files\Go\src\sync\waitgroup.go
namespace go;

using race = @internal.race_package;
using atomic = sync.atomic_package;
using @unsafe = @unsafe_package;


// A WaitGroup waits for a collection of goroutines to finish.
// The main goroutine calls Add to set the number of
// goroutines to wait for. Then each of the goroutines
// runs and calls Done when finished. At the same time,
// Wait can be used to block until all goroutines have finished.
//
// A WaitGroup must not be copied after first use.

public static partial class sync_package {

public partial struct WaitGroup {
    public noCopy noCopy; // 64-bit value: high 32 bits are counter, low 32 bits are waiter count.
// 64-bit atomic operations require 64-bit alignment, but 32-bit
// compilers do not ensure it. So we allocate 12 bytes and then use
// the aligned 8 bytes in them as state, and the other 4 as storage
// for the sema.
    public array<uint> state1;
}

// state returns pointers to the state and sema fields stored within wg.state1.
private static (ptr<ulong>, ptr<uint>) state(this ptr<WaitGroup> _addr_wg) {
    ptr<ulong> statep = default!;
    ptr<uint> semap = default!;
    ref WaitGroup wg = ref _addr_wg.val;

    if (uintptr(@unsafe.Pointer(_addr_wg.state1)) % 8 == 0) {
        return (_addr_(uint64.val)(@unsafe.Pointer(_addr_wg.state1))!, _addr__addr_wg.state1[2]!);
    }
    else
 {
        return (_addr_(uint64.val)(@unsafe.Pointer(_addr_wg.state1[1]))!, _addr__addr_wg.state1[0]!);
    }
}

// Add adds delta, which may be negative, to the WaitGroup counter.
// If the counter becomes zero, all goroutines blocked on Wait are released.
// If the counter goes negative, Add panics.
//
// Note that calls with a positive delta that occur when the counter is zero
// must happen before a Wait. Calls with a negative delta, or calls with a
// positive delta that start when the counter is greater than zero, may happen
// at any time.
// Typically this means the calls to Add should execute before the statement
// creating the goroutine or other event to be waited for.
// If a WaitGroup is reused to wait for several independent sets of events,
// new Add calls must happen after all previous Wait calls have returned.
// See the WaitGroup example.
private static void Add(this ptr<WaitGroup> _addr_wg, nint delta) => func((defer, panic, _) => {
    ref WaitGroup wg = ref _addr_wg.val;

    var (statep, semap) = wg.state();
    if (race.Enabled) {
        _ = statep.val; // trigger nil deref early
        if (delta < 0) { 
            // Synchronize decrements with Wait.
            race.ReleaseMerge(@unsafe.Pointer(wg));
        }
        race.Disable();
        defer(race.Enable());
    }
    var state = atomic.AddUint64(statep, uint64(delta) << 32);
    var v = int32(state >> 32);
    var w = uint32(state);
    if (race.Enabled && delta > 0 && v == int32(delta)) { 
        // The first increment must be synchronized with Wait.
        // Need to model this as a read, because there can be
        // several concurrent wg.counter transitions from 0.
        race.Read(@unsafe.Pointer(semap));
    }
    if (v < 0) {
        panic("sync: negative WaitGroup counter");
    }
    if (w != 0 && delta > 0 && v == int32(delta)) {
        panic("sync: WaitGroup misuse: Add called concurrently with Wait");
    }
    if (v > 0 || w == 0) {
        return ;
    }
    if (statep != state.val) {
        panic("sync: WaitGroup misuse: Add called concurrently with Wait");
    }
    statep.val = 0;
    while (w != 0) {
        runtime_Semrelease(semap, false, 0);
        w--;
    }
});

// Done decrements the WaitGroup counter by one.
private static void Done(this ptr<WaitGroup> _addr_wg) {
    ref WaitGroup wg = ref _addr_wg.val;

    wg.Add(-1);
}

// Wait blocks until the WaitGroup counter is zero.
private static void Wait(this ptr<WaitGroup> _addr_wg) => func((_, panic, _) => {
    ref WaitGroup wg = ref _addr_wg.val;

    var (statep, semap) = wg.state();
    if (race.Enabled) {
        _ = statep.val; // trigger nil deref early
        race.Disable();
    }
    while (true) {
        var state = atomic.LoadUint64(statep);
        var v = int32(state >> 32);
        var w = uint32(state);
        if (v == 0) { 
            // Counter is 0, no need to wait.
            if (race.Enabled) {
                race.Enable();
                race.Acquire(@unsafe.Pointer(wg));
            }
            return ;
        }
        if (atomic.CompareAndSwapUint64(statep, state, state + 1)) {
            if (race.Enabled && w == 0) { 
                // Wait must be synchronized with the first Add.
                // Need to model this is as a write to race with the read in Add.
                // As a consequence, can do the write only for the first waiter,
                // otherwise concurrent Waits will race with each other.
                race.Write(@unsafe.Pointer(semap));
            }
            runtime_Semacquire(semap);
            if (statep != 0.val) {
                panic("sync: WaitGroup is reused before previous Wait has returned");
            }
            if (race.Enabled) {
                race.Enable();
                race.Acquire(@unsafe.Pointer(wg));
            }
            return ;
        }
    }
});

} // end sync_package
