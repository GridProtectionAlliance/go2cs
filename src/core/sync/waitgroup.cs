// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using race = @internal.race_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using sync;

partial class sync_package {

// A WaitGroup waits for a collection of goroutines to finish.
// The main goroutine calls [WaitGroup.Add] to set the number of
// goroutines to wait for. Then each of the goroutines
// runs and calls [WaitGroup.Done] when finished. At the same time,
// [WaitGroup.Wait] can be used to block until all goroutines have finished.
//
// A WaitGroup must not be copied after first use.
//
// In the terminology of [the Go memory model], a call to [WaitGroup.Done]
// “synchronizes before” the return of any Wait call that it unblocks.
//
// [the Go memory model]: https://go.dev/ref/mem
[GoType] partial struct WaitGroup {
    internal noCopy noCopy;
    internal sync.atomic_package.Uint64 state; // high 32 bits are counter, low 32 bits are waiter count.
    internal uint32 sema;
}

// Add adds delta, which may be negative, to the [WaitGroup] counter.
// If the counter becomes zero, all goroutines blocked on [WaitGroup.Wait] are released.
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
[GoRecv] public static void Add(this ref WaitGroup wg, nint delta) => func((defer, _) => {
    if (race.Enabled) {
        if (delta < 0) {
            // Synchronize decrements with Wait.
            race.ReleaseMerge((uintptr)@unsafe.Pointer.FromRef(ref wg));
        }
        race.Disable();
        defer(race.Enable);
    }
    var state = wg.state.Add(((uint64)delta) << (int)(32));
    var v = ((int32)(state >> (int)(32)));
    var w = ((uint32)state);
    if (race.Enabled && delta > 0 && v == ((int32)delta)) {
        // The first increment must be synchronized with Wait.
        // Need to model this as a read, because there can be
        // several concurrent wg.counter transitions from 0.
        race.Read(new @unsafe.Pointer(Ꮡ(wg.sema)));
    }
    if (v < 0) {
        throw panic("sync: negative WaitGroup counter");
    }
    if (w != 0 && delta > 0 && v == ((int32)delta)) {
        throw panic("sync: WaitGroup misuse: Add called concurrently with Wait");
    }
    if (v > 0 || w == 0) {
        return;
    }
    // This goroutine has set counter to 0 when waiters > 0.
    // Now there can't be concurrent mutations of state:
    // - Adds must not happen concurrently with Wait,
    // - Wait does not increment waiters if it sees counter == 0.
    // Still do a cheap sanity check to detect WaitGroup misuse.
    if (wg.state.Load() != state) {
        throw panic("sync: WaitGroup misuse: Add called concurrently with Wait");
    }
    // Reset waiters count to 0.
    wg.state.Store(0);
    for (; w != 0; w--) {
        runtime_Semrelease(Ꮡ(wg.sema), false, 0);
    }
});

// Done decrements the [WaitGroup] counter by one.
[GoRecv] public static void Done(this ref WaitGroup wg) {
    wg.Add(-1);
}

// Wait blocks until the [WaitGroup] counter is zero.
[GoRecv] public static void Wait(this ref WaitGroup wg) {
    if (race.Enabled) {
        race.Disable();
    }
    while (ᐧ) {
        var state = wg.state.Load();
        var v = ((int32)(state >> (int)(32)));
        var w = ((uint32)state);
        if (v == 0) {
            // Counter is 0, no need to wait.
            if (race.Enabled) {
                race.Enable();
                race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref wg));
            }
            return;
        }
        // Increment waiters count.
        if (wg.state.CompareAndSwap(state, state + 1)) {
            if (race.Enabled && w == 0) {
                // Wait must be synchronized with the first Add.
                // Need to model this is as a write to race with the read in Add.
                // As a consequence, can do the write only for the first waiter,
                // otherwise concurrent Waits will race with each other.
                race.Write(new @unsafe.Pointer(Ꮡ(wg.sema)));
            }
            runtime_Semacquire(Ꮡ(wg.sema));
            if (wg.state.Load() != 0) {
                throw panic("sync: WaitGroup is reused before previous Wait has returned");
            }
            if (race.Enabled) {
                race.Enable();
                race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref wg));
            }
            return;
        }
    }
}

} // end sync_package
