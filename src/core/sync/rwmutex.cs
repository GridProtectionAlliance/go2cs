// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using race = @internal.race_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using sync;

partial class sync_package {

// There is a modified copy of this file in runtime/rwmutex.go.
// If you make any changes here, see if you should make them there.

// A RWMutex is a reader/writer mutual exclusion lock.
// The lock can be held by an arbitrary number of readers or a single writer.
// The zero value for a RWMutex is an unlocked mutex.
//
// A RWMutex must not be copied after first use.
//
// If any goroutine calls [RWMutex.Lock] while the lock is already held by
// one or more readers, concurrent calls to [RWMutex.RLock] will block until
// the writer has acquired (and released) the lock, to ensure that
// the lock eventually becomes available to the writer.
// Note that this prohibits recursive read-locking.
//
// In the terminology of [the Go memory model],
// the n'th call to [RWMutex.Unlock] “synchronizes before” the m'th call to Lock
// for any n < m, just as for [Mutex].
// For any call to RLock, there exists an n such that
// the n'th call to Unlock “synchronizes before” that call to RLock,
// and the corresponding call to [RWMutex.RUnlock] “synchronizes before”
// the n+1'th call to Lock.
//
// [the Go memory model]: https://go.dev/ref/mem
[GoType] partial struct RWMutex {
    internal Mutex w;        // held if there are pending writers
    internal uint32 writerSem;       // semaphore for writers to wait for completing readers
    internal uint32 readerSem;       // semaphore for readers to wait for completing writers
    internal sync.atomic_package.Int32 readerCount; // number of pending readers
    internal sync.atomic_package.Int32 readerWait; // number of departing readers
}

internal static readonly UntypedInt rwmutexMaxReaders = /* 1 << 30 */ 1073741824;

// Happens-before relationships are indicated to the race detector via:
// - Unlock  -> Lock:  readerSem
// - Unlock  -> RLock: readerSem
// - RUnlock -> Lock:  writerSem
//
// The methods below temporarily disable handling of race synchronization
// events in order to provide the more precise model above to the race
// detector.
//
// For example, atomic.AddInt32 in RLock should not appear to provide
// acquire-release semantics, which would incorrectly synchronize racing
// readers, thus potentially missing races.

// RLock locks rw for reading.
//
// It should not be used for recursive read locking; a blocked Lock
// call excludes new readers from acquiring the lock. See the
// documentation on the [RWMutex] type.
[GoRecv] public static void RLock(this ref RWMutex rw) {
    if (race.Enabled) {
        _ = rw.w.state;
        race.Disable();
    }
    if (rw.readerCount.Add(1) < 0) {
        // A writer is pending, wait for it.
        runtime_SemacquireRWMutexR(Ꮡ(rw.readerSem), false, 0);
    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(new @unsafe.Pointer(Ꮡ(rw.readerSem)));
    }
}

// TryRLock tries to lock rw for reading and reports whether it succeeded.
//
// Note that while correct uses of TryRLock do exist, they are rare,
// and use of TryRLock is often a sign of a deeper problem
// in a particular use of mutexes.
[GoRecv] public static bool TryRLock(this ref RWMutex rw) {
    if (race.Enabled) {
        _ = rw.w.state;
        race.Disable();
    }
    while (ᐧ) {
        var c = rw.readerCount.Load();
        if (c < 0) {
            if (race.Enabled) {
                race.Enable();
            }
            return false;
        }
        if (rw.readerCount.CompareAndSwap(c, c + 1)) {
            if (race.Enabled) {
                race.Enable();
                race.Acquire(new @unsafe.Pointer(Ꮡ(rw.readerSem)));
            }
            return true;
        }
    }
}

// RUnlock undoes a single [RWMutex.RLock] call;
// it does not affect other simultaneous readers.
// It is a run-time error if rw is not locked for reading
// on entry to RUnlock.
[GoRecv] public static void RUnlock(this ref RWMutex rw) {
    if (race.Enabled) {
        _ = rw.w.state;
        race.ReleaseMerge(new @unsafe.Pointer(Ꮡ(rw.writerSem)));
        race.Disable();
    }
    {
        var r = rw.readerCount.Add(-1); if (r < 0) {
            // Outlined slow-path to allow the fast-path to be inlined
            rw.rUnlockSlow(r);
        }
    }
    if (race.Enabled) {
        race.Enable();
    }
}

[GoRecv] internal static void rUnlockSlow(this ref RWMutex rw, int32 r) {
    if (r + 1 == 0 || r + 1 == -rwmutexMaxReaders) {
        race.Enable();
        fatal("sync: RUnlock of unlocked RWMutex"u8);
    }
    // A writer is pending.
    if (rw.readerWait.Add(-1) == 0) {
        // The last reader unblocks the writer.
        runtime_Semrelease(Ꮡ(rw.writerSem), false, 1);
    }
}

// Lock locks rw for writing.
// If the lock is already locked for reading or writing,
// Lock blocks until the lock is available.
[GoRecv] public static void Lock(this ref RWMutex rw) {
    if (race.Enabled) {
        _ = rw.w.state;
        race.Disable();
    }
    // First, resolve competition with other writers.
    rw.w.Lock();
    // Announce to readers there is a pending writer.
    var r = rw.readerCount.Add(-rwmutexMaxReaders) + rwmutexMaxReaders;
    // Wait for active readers.
    if (r != 0 && rw.readerWait.Add(r) != 0) {
        runtime_SemacquireRWMutex(Ꮡ(rw.writerSem), false, 0);
    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(new @unsafe.Pointer(Ꮡ(rw.readerSem)));
        race.Acquire(new @unsafe.Pointer(Ꮡ(rw.writerSem)));
    }
}

// TryLock tries to lock rw for writing and reports whether it succeeded.
//
// Note that while correct uses of TryLock do exist, they are rare,
// and use of TryLock is often a sign of a deeper problem
// in a particular use of mutexes.
[GoRecv] public static bool TryLock(this ref RWMutex rw) {
    if (race.Enabled) {
        _ = rw.w.state;
        race.Disable();
    }
    if (!rw.w.TryLock()) {
        if (race.Enabled) {
            race.Enable();
        }
        return false;
    }
    if (!rw.readerCount.CompareAndSwap(0, -rwmutexMaxReaders)) {
        rw.w.Unlock();
        if (race.Enabled) {
            race.Enable();
        }
        return false;
    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(new @unsafe.Pointer(Ꮡ(rw.readerSem)));
        race.Acquire(new @unsafe.Pointer(Ꮡ(rw.writerSem)));
    }
    return true;
}

// Unlock unlocks rw for writing. It is a run-time error if rw is
// not locked for writing on entry to Unlock.
//
// As with Mutexes, a locked [RWMutex] is not associated with a particular
// goroutine. One goroutine may [RWMutex.RLock] ([RWMutex.Lock]) a RWMutex and then
// arrange for another goroutine to [RWMutex.RUnlock] ([RWMutex.Unlock]) it.
[GoRecv] public static void Unlock(this ref RWMutex rw) {
    if (race.Enabled) {
        _ = rw.w.state;
        race.Release(new @unsafe.Pointer(Ꮡ(rw.readerSem)));
        race.Disable();
    }
    // Announce to readers there is no active writer.
    var r = rw.readerCount.Add(rwmutexMaxReaders);
    if (r >= rwmutexMaxReaders) {
        race.Enable();
        fatal("sync: Unlock of unlocked RWMutex"u8);
    }
    // Unblock blocked readers, if any.
    for (nint i = 0; i < ((nint)r); i++) {
        runtime_Semrelease(Ꮡ(rw.readerSem), false, 0);
    }
    // Allow other writers to proceed.
    rw.w.Unlock();
    if (race.Enabled) {
        race.Enable();
    }
}

// syscall_hasWaitingReaders reports whether any goroutine is waiting
// to acquire a read lock on rw. This exists because syscall.ForkLock
// is an RWMutex, and we can't change that without breaking compatibility.
// We don't need or want RWMutex semantics for ForkLock, and we use
// this private API to avoid having to change the type of ForkLock.
// For more details see the syscall package.
//
//go:linkname syscall_hasWaitingReaders syscall.hasWaitingReaders
internal static bool syscall_hasWaitingReaders(ж<RWMutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.val;

    var r = rw.readerCount.Load();
    return r < 0 && r + rwmutexMaxReaders > 0;
}

// RLocker returns a [Locker] interface that implements
// the [Locker.Lock] and [Locker.Unlock] methods by calling rw.RLock and rw.RUnlock.
[GoRecv] public static Locker RLocker(this ref RWMutex rw) {
    return ~((ж<rlocker>)(rw?.val ?? default!));
}

[GoType("struct{w sync.Mutex; writerSem uint32; readerSem uint32; readerCount sync.atomic.Int32; readerWait sync.atomic.Int32}")] partial struct rlocker;

[GoRecv] internal static void Lock(this ref rlocker r) {
    (((ж<RWMutex>)(r?.val ?? default!))).val.RLock();
}

[GoRecv] internal static void Unlock(this ref rlocker r) {
    (((ж<RWMutex>)(r?.val ?? default!))).val.RUnlock();
}

} // end sync_package
