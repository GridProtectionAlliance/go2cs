// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sync provides basic synchronization primitives such as mutual
// exclusion locks. Other than the Once and WaitGroup types, most are intended
// for use by low-level library routines. Higher-level synchronization is
// better done via channels and communication.
//
// Values containing the types defined in this package should not be copied.

// package sync -- go2cs converted at 2022 March 13 05:24:05 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Program Files\Go\src\sync\mutex.go
namespace go;

using race = @internal.race_package;
using atomic = sync.atomic_package;
using @unsafe = @unsafe_package;

public static partial class sync_package {

private static void @throw(@string _p0); // provided by runtime

// A Mutex is a mutual exclusion lock.
// The zero value for a Mutex is an unlocked mutex.
//
// A Mutex must not be copied after first use.
public partial struct Mutex {
    public int state;
    public uint sema;
}

// A Locker represents an object that can be locked and unlocked.
public partial interface Locker {
    void Lock();
    void Unlock();
}

private static readonly nint mutexLocked = 1 << (int)(iota); // mutex is locked
private static readonly var mutexWoken = 0;
private static readonly mutexWaiterShift mutexStarving = iota; 

// Mutex fairness.
//
// Mutex can be in 2 modes of operations: normal and starvation.
// In normal mode waiters are queued in FIFO order, but a woken up waiter
// does not own the mutex and competes with new arriving goroutines over
// the ownership. New arriving goroutines have an advantage -- they are
// already running on CPU and there can be lots of them, so a woken up
// waiter has good chances of losing. In such case it is queued at front
// of the wait queue. If a waiter fails to acquire the mutex for more than 1ms,
// it switches mutex to the starvation mode.
//
// In starvation mode ownership of the mutex is directly handed off from
// the unlocking goroutine to the waiter at the front of the queue.
// New arriving goroutines don't try to acquire the mutex even if it appears
// to be unlocked, and don't try to spin. Instead they queue themselves at
// the tail of the wait queue.
//
// If a waiter receives ownership of the mutex and sees that either
// (1) it is the last waiter in the queue, or (2) it waited for less than 1 ms,
// it switches mutex back to normal operation mode.
//
// Normal mode has considerably better performance as a goroutine can acquire
// a mutex several times in a row even if there are blocked waiters.
// Starvation mode is important to prevent pathological cases of tail latency.
private static readonly float starvationThresholdNs = 1e6F;

// Lock locks m.
// If the lock is already in use, the calling goroutine
// blocks until the mutex is available.
private static void Lock(this ptr<Mutex> _addr_m) {
    ref Mutex m = ref _addr_m.val;
 
    // Fast path: grab unlocked mutex.
    if (atomic.CompareAndSwapInt32(_addr_m.state, 0, mutexLocked)) {>>MARKER:FUNCTION_@throw_BLOCK_PREFIX<<
        if (race.Enabled) {
            race.Acquire(@unsafe.Pointer(m));
        }
        return ;
    }
    m.lockSlow();
}

private static void lockSlow(this ptr<Mutex> _addr_m) {
    ref Mutex m = ref _addr_m.val;

    long waitStartTime = default;
    var starving = false;
    var awoke = false;
    nint iter = 0;
    var old = m.state;
    while (true) { 
        // Don't spin in starvation mode, ownership is handed off to waiters
        // so we won't be able to acquire the mutex anyway.
        if (old & (mutexLocked | mutexStarving) == mutexLocked && runtime_canSpin(iter)) { 
            // Active spinning makes sense.
            // Try to set mutexWoken flag to inform Unlock
            // to not wake other blocked goroutines.
            if (!awoke && old & mutexWoken == 0 && old >> (int)(mutexWaiterShift) != 0 && atomic.CompareAndSwapInt32(_addr_m.state, old, old | mutexWoken)) {
                awoke = true;
            }
            runtime_doSpin();
            iter++;
            old = m.state;
            continue;
        }
        var @new = old; 
        // Don't try to acquire starving mutex, new arriving goroutines must queue.
        if (old & mutexStarving == 0) {
            new |= mutexLocked;
        }
        if (old & (mutexLocked | mutexStarving) != 0) {
            new += 1 << (int)(mutexWaiterShift);
        }
        if (starving && old & mutexLocked != 0) {
            new |= mutexStarving;
        }
        if (awoke) { 
            // The goroutine has been woken from sleep,
            // so we need to reset the flag in either case.
            if (new & mutexWoken == 0) {
                throw("sync: inconsistent mutex state");
            }
            new &= mutexWoken;
        }
        if (atomic.CompareAndSwapInt32(_addr_m.state, old, new)) {
            if (old & (mutexLocked | mutexStarving) == 0) {
                break; // locked the mutex with CAS
            } 
            // If we were already waiting before, queue at the front of the queue.
            var queueLifo = waitStartTime != 0;
            if (waitStartTime == 0) {
                waitStartTime = runtime_nanotime();
            }
            runtime_SemacquireMutex(_addr_m.sema, queueLifo, 1);
            starving = starving || runtime_nanotime() - waitStartTime > starvationThresholdNs;
            old = m.state;
            if (old & mutexStarving != 0) { 
                // If this goroutine was woken and mutex is in starvation mode,
                // ownership was handed off to us but mutex is in somewhat
                // inconsistent state: mutexLocked is not set and we are still
                // accounted as waiter. Fix that.
                if (old & (mutexLocked | mutexWoken) != 0 || old >> (int)(mutexWaiterShift) == 0) {
                    throw("sync: inconsistent mutex state");
                }
                var delta = int32(mutexLocked - 1 << (int)(mutexWaiterShift));
                if (!starving || old >> (int)(mutexWaiterShift) == 1) { 
                    // Exit starvation mode.
                    // Critical to do it here and consider wait time.
                    // Starvation mode is so inefficient, that two goroutines
                    // can go lock-step infinitely once they switch mutex
                    // to starvation mode.
                    delta -= mutexStarving;
                }
                atomic.AddInt32(_addr_m.state, delta);
                break;
            }
            awoke = true;
            iter = 0;
        }
        else
 {
            old = m.state;
        }
    }

    if (race.Enabled) {
        race.Acquire(@unsafe.Pointer(m));
    }
}

// Unlock unlocks m.
// It is a run-time error if m is not locked on entry to Unlock.
//
// A locked Mutex is not associated with a particular goroutine.
// It is allowed for one goroutine to lock a Mutex and then
// arrange for another goroutine to unlock it.
private static void Unlock(this ptr<Mutex> _addr_m) {
    ref Mutex m = ref _addr_m.val;

    if (race.Enabled) {
        _ = m.state;
        race.Release(@unsafe.Pointer(m));
    }
    var @new = atomic.AddInt32(_addr_m.state, -mutexLocked);
    if (new != 0) { 
        // Outlined slow path to allow inlining the fast path.
        // To hide unlockSlow during tracing we skip one extra frame when tracing GoUnblock.
        m.unlockSlow(new);
    }
}

private static void unlockSlow(this ptr<Mutex> _addr_m, int @new) {
    ref Mutex m = ref _addr_m.val;

    if ((new + mutexLocked) & mutexLocked == 0) {
        throw("sync: unlock of unlocked mutex");
    }
    if (new & mutexStarving == 0) {
        var old = new;
        while (true) { 
            // If there are no waiters or a goroutine has already
            // been woken or grabbed the lock, no need to wake anyone.
            // In starvation mode ownership is directly handed off from unlocking
            // goroutine to the next waiter. We are not part of this chain,
            // since we did not observe mutexStarving when we unlocked the mutex above.
            // So get off the way.
            if (old >> (int)(mutexWaiterShift) == 0 || old & (mutexLocked | mutexWoken | mutexStarving) != 0) {
                return ;
            } 
            // Grab the right to wake someone.
            new = (old - 1 << (int)(mutexWaiterShift)) | mutexWoken;
            if (atomic.CompareAndSwapInt32(_addr_m.state, old, new)) {
                runtime_Semrelease(_addr_m.sema, false, 1);
                return ;
            }
            old = m.state;
        }
    else
    } { 
        // Starving mode: handoff mutex ownership to the next waiter, and yield
        // our time slice so that the next waiter can start to run immediately.
        // Note: mutexLocked is not set, the waiter will set it after wakeup.
        // But mutex is still considered locked if mutexStarving is set,
        // so new coming goroutines won't acquire it.
        runtime_Semrelease(_addr_m.sema, true, 1);
    }
}

} // end sync_package
