// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sync provides basic synchronization primitives such as mutual
// exclusion locks. Other than the [Once] and [WaitGroup] types, most are intended
// for use by low-level library routines. Higher-level synchronization is
// better done via channels and communication.
//
// Values containing the types defined in this package should not be copied.
namespace go;

using race = @internal.race_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using sync;

partial class sync_package {

// Provided by runtime via linkname.
internal static partial void @throw(@string _);

internal static partial void fatal(@string _);

// A Mutex is a mutual exclusion lock.
// The zero value for a Mutex is an unlocked mutex.
//
// A Mutex must not be copied after first use.
//
// In the terminology of [the Go memory model],
// the n'th call to [Mutex.Unlock] “synchronizes before” the m'th call to [Mutex.Lock]
// for any n < m.
// A successful call to [Mutex.TryLock] is equivalent to a call to Lock.
// A failed call to TryLock does not establish any “synchronizes before”
// relation at all.
//
// [the Go memory model]: https://go.dev/ref/mem
[GoType] partial struct Mutex {
    internal int32 state;
    internal uint32 sema;
}

// A Locker represents an object that can be locked and unlocked.
[GoType] partial interface Locker {
    void Lock();
    void Unlock();
}

internal static readonly UntypedInt mutexLocked = /* 1 << iota */ 1; // mutex is locked
internal static readonly UntypedInt mutexWoken = 2;
internal static readonly UntypedInt mutexStarving = 4;
internal static readonly UntypedInt mutexWaiterShift = iota;
internal static readonly UntypedFloat starvationThresholdNs = 1e+06;

// Lock locks m.
// If the lock is already in use, the calling goroutine
// blocks until the mutex is available.
[GoRecv] public static void Lock(this ref Mutex m) {
    // Fast path: grab unlocked mutex.
    if (atomic.CompareAndSwapInt32(Ꮡ(m.state), 0, mutexLocked)) {
        if (race.Enabled) {
            race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref m));
        }
        return;
    }
    // Slow path (outlined so that the fast path can be inlined)
    m.lockSlow();
}

// TryLock tries to lock m and reports whether it succeeded.
//
// Note that while correct uses of TryLock do exist, they are rare,
// and use of TryLock is often a sign of a deeper problem
// in a particular use of mutexes.
[GoRecv] public static bool TryLock(this ref Mutex m) {
    var old = m.state;
    if ((int32)(old & ((int32)(mutexLocked | mutexStarving))) != 0) {
        return false;
    }
    // There may be a goroutine waiting for the mutex, but we are
    // running now and can try to grab the mutex before that
    // goroutine wakes up.
    if (!atomic.CompareAndSwapInt32(Ꮡ(m.state), old, (int32)(old | mutexLocked))) {
        return false;
    }
    if (race.Enabled) {
        race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref m));
    }
    return true;
}

[GoRecv] internal static void lockSlow(this ref Mutex m) {
    int64 waitStartTime = default!;
    var starving = false;
    var awoke = false;
    nint iter = 0;
    var old = m.state;
    while (ᐧ) {
        // Don't spin in starvation mode, ownership is handed off to waiters
        // so we won't be able to acquire the mutex anyway.
        if ((int32)(old & ((int32)(mutexLocked | mutexStarving))) == mutexLocked && runtime_canSpin(iter)) {
            // Active spinning makes sense.
            // Try to set mutexWoken flag to inform Unlock
            // to not wake other blocked goroutines.
            if (!awoke && (int32)(old & mutexWoken) == 0 && old >> (int)(mutexWaiterShift) != 0 && atomic.CompareAndSwapInt32(Ꮡ(m.state), old, (int32)(old | mutexWoken))) {
                awoke = true;
            }
            runtime_doSpin();
            iter++;
            old = m.state;
            continue;
        }
        var @new = old;
        // Don't try to acquire starving mutex, new arriving goroutines must queue.
        if ((int32)(old & mutexStarving) == 0) {
            @new |= (int32)(mutexLocked);
        }
        if ((int32)(old & ((int32)(mutexLocked | mutexStarving))) != 0) {
            @new += 1 << (int)(mutexWaiterShift);
        }
        // The current goroutine switches mutex to starvation mode.
        // But if the mutex is currently unlocked, don't do the switch.
        // Unlock expects that starving mutex has waiters, which will not
        // be true in this case.
        if (starving && (int32)(old & mutexLocked) != 0) {
            @new |= (int32)(mutexStarving);
        }
        if (awoke) {
            // The goroutine has been woken from sleep,
            // so we need to reset the flag in either case.
            if ((int32)(@new & mutexWoken) == 0) {
                @throw("sync: inconsistent mutex state"u8);
            }
            @new &= ~(int32)(mutexWoken);
        }
        if (atomic.CompareAndSwapInt32(Ꮡ(m.state), old, @new)){
            if ((int32)(old & ((int32)(mutexLocked | mutexStarving))) == 0) {
                break;
            }
            // locked the mutex with CAS
            // If we were already waiting before, queue at the front of the queue.
            var queueLifo = waitStartTime != 0;
            if (waitStartTime == 0) {
                waitStartTime = runtime_nanotime();
            }
            runtime_SemacquireMutex(Ꮡ(m.sema), queueLifo, 1);
            starving = starving || runtime_nanotime() - waitStartTime > starvationThresholdNs;
            old = m.state;
            if ((int32)(old & mutexStarving) != 0) {
                // If this goroutine was woken and mutex is in starvation mode,
                // ownership was handed off to us but mutex is in somewhat
                // inconsistent state: mutexLocked is not set and we are still
                // accounted as waiter. Fix that.
                if ((int32)(old & ((int32)(mutexLocked | mutexWoken))) != 0 || old >> (int)(mutexWaiterShift) == 0) {
                    @throw("sync: inconsistent mutex state"u8);
                }
                var delta = ((int32)(mutexLocked - 1 << (int)(mutexWaiterShift)));
                if (!starving || old >> (int)(mutexWaiterShift) == 1) {
                    // Exit starvation mode.
                    // Critical to do it here and consider wait time.
                    // Starvation mode is so inefficient, that two goroutines
                    // can go lock-step infinitely once they switch mutex
                    // to starvation mode.
                    delta -= mutexStarving;
                }
                atomic.AddInt32(Ꮡ(m.state), delta);
                break;
            }
            awoke = true;
            iter = 0;
        } else {
            old = m.state;
        }
    }
    if (race.Enabled) {
        race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref m));
    }
}

// Unlock unlocks m.
// It is a run-time error if m is not locked on entry to Unlock.
//
// A locked [Mutex] is not associated with a particular goroutine.
// It is allowed for one goroutine to lock a Mutex and then
// arrange for another goroutine to unlock it.
[GoRecv] public static void Unlock(this ref Mutex m) {
    if (race.Enabled) {
        _ = m.state;
        race.Release((uintptr)@unsafe.Pointer.FromRef(ref m));
    }
    // Fast path: drop lock bit.
    var @new = atomic.AddInt32(Ꮡ(m.state), -mutexLocked);
    if (@new != 0) {
        // Outlined slow path to allow inlining the fast path.
        // To hide unlockSlow during tracing we skip one extra frame when tracing GoUnblock.
        m.unlockSlow(@new);
    }
}

[GoRecv] internal static void unlockSlow(this ref Mutex m, int32 @new) {
    if ((int32)((@new + mutexLocked) & mutexLocked) == 0) {
        fatal("sync: unlock of unlocked mutex"u8);
    }
    if ((int32)(@new & mutexStarving) == 0){
        var old = @new;
        while (ᐧ) {
            // If there are no waiters or a goroutine has already
            // been woken or grabbed the lock, no need to wake anyone.
            // In starvation mode ownership is directly handed off from unlocking
            // goroutine to the next waiter. We are not part of this chain,
            // since we did not observe mutexStarving when we unlocked the mutex above.
            // So get off the way.
            if (old >> (int)(mutexWaiterShift) == 0 || (int32)(old & ((int32)((UntypedInt)(mutexLocked | mutexWoken) | mutexStarving))) != 0) {
                return;
            }
            // Grab the right to wake someone.
            @new = (int32)((old - 1 << (int)(mutexWaiterShift)) | mutexWoken);
            if (atomic.CompareAndSwapInt32(Ꮡ(m.state), old, @new)) {
                runtime_Semrelease(Ꮡ(m.sema), false, 1);
                return;
            }
            old = m.state;
        }
    } else {
        // Starving mode: handoff mutex ownership to the next waiter, and yield
        // our time slice so that the next waiter can start to run immediately.
        // Note: mutexLocked is not set, the waiter will set it after wakeup.
        // But mutex is still considered locked if mutexStarving is set,
        // so new coming goroutines won't acquire it.
        runtime_Semrelease(Ꮡ(m.sema), true, 1);
    }
}

} // end sync_package
