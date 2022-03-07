// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2022 March 06 22:26:24 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Program Files\Go\src\sync\rwmutex.go
using race = go.@internal.race_package;
using atomic = go.sync.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class sync_package {

    // There is a modified copy of this file in runtime/rwmutex.go.
    // If you make any changes here, see if you should make them there.

    // A RWMutex is a reader/writer mutual exclusion lock.
    // The lock can be held by an arbitrary number of readers or a single writer.
    // The zero value for a RWMutex is an unlocked mutex.
    //
    // A RWMutex must not be copied after first use.
    //
    // If a goroutine holds a RWMutex for reading and another goroutine might
    // call Lock, no goroutine should expect to be able to acquire a read lock
    // until the initial read lock is released. In particular, this prohibits
    // recursive read locking. This is to ensure that the lock eventually becomes
    // available; a blocked Lock call excludes new readers from acquiring the
    // lock.
public partial struct RWMutex {
    public Mutex w; // held if there are pending writers
    public uint writerSem; // semaphore for writers to wait for completing readers
    public uint readerSem; // semaphore for readers to wait for completing writers
    public int readerCount; // number of pending readers
    public int readerWait; // number of departing readers
}

private static readonly nint rwmutexMaxReaders = 1 << 30;

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
// documentation on the RWMutex type.


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
// documentation on the RWMutex type.
private static void RLock(this ptr<RWMutex> _addr_rw) {
    ref RWMutex rw = ref _addr_rw.val;

    if (race.Enabled) {
        _ = rw.w.state;
        race.Disable();
    }
    if (atomic.AddInt32(_addr_rw.readerCount, 1) < 0) { 
        // A writer is pending, wait for it.
        runtime_SemacquireMutex(_addr_rw.readerSem, false, 0);

    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(@unsafe.Pointer(_addr_rw.readerSem));
    }
}

// RUnlock undoes a single RLock call;
// it does not affect other simultaneous readers.
// It is a run-time error if rw is not locked for reading
// on entry to RUnlock.
private static void RUnlock(this ptr<RWMutex> _addr_rw) {
    ref RWMutex rw = ref _addr_rw.val;

    if (race.Enabled) {
        _ = rw.w.state;
        race.ReleaseMerge(@unsafe.Pointer(_addr_rw.writerSem));
        race.Disable();
    }
    {
        var r = atomic.AddInt32(_addr_rw.readerCount, -1);

        if (r < 0) { 
            // Outlined slow-path to allow the fast-path to be inlined
            rw.rUnlockSlow(r);

        }
    }

    if (race.Enabled) {
        race.Enable();
    }
}

private static void rUnlockSlow(this ptr<RWMutex> _addr_rw, int r) {
    ref RWMutex rw = ref _addr_rw.val;

    if (r + 1 == 0 || r + 1 == -rwmutexMaxReaders) {
        race.Enable();
        throw("sync: RUnlock of unlocked RWMutex");
    }
    if (atomic.AddInt32(_addr_rw.readerWait, -1) == 0) { 
        // The last reader unblocks the writer.
        runtime_Semrelease(_addr_rw.writerSem, false, 1);

    }
}

// Lock locks rw for writing.
// If the lock is already locked for reading or writing,
// Lock blocks until the lock is available.
private static void Lock(this ptr<RWMutex> _addr_rw) {
    ref RWMutex rw = ref _addr_rw.val;

    if (race.Enabled) {
        _ = rw.w.state;
        race.Disable();
    }
    rw.w.Lock(); 
    // Announce to readers there is a pending writer.
    var r = atomic.AddInt32(_addr_rw.readerCount, -rwmutexMaxReaders) + rwmutexMaxReaders; 
    // Wait for active readers.
    if (r != 0 && atomic.AddInt32(_addr_rw.readerWait, r) != 0) {
        runtime_SemacquireMutex(_addr_rw.writerSem, false, 0);
    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(@unsafe.Pointer(_addr_rw.readerSem));
        race.Acquire(@unsafe.Pointer(_addr_rw.writerSem));
    }
}

// Unlock unlocks rw for writing. It is a run-time error if rw is
// not locked for writing on entry to Unlock.
//
// As with Mutexes, a locked RWMutex is not associated with a particular
// goroutine. One goroutine may RLock (Lock) a RWMutex and then
// arrange for another goroutine to RUnlock (Unlock) it.
private static void Unlock(this ptr<RWMutex> _addr_rw) {
    ref RWMutex rw = ref _addr_rw.val;

    if (race.Enabled) {
        _ = rw.w.state;
        race.Release(@unsafe.Pointer(_addr_rw.readerSem));
        race.Disable();
    }
    var r = atomic.AddInt32(_addr_rw.readerCount, rwmutexMaxReaders);
    if (r >= rwmutexMaxReaders) {
        race.Enable();
        throw("sync: Unlock of unlocked RWMutex");
    }
    for (nint i = 0; i < int(r); i++) {
        runtime_Semrelease(_addr_rw.readerSem, false, 0);
    } 
    // Allow other writers to proceed.
    rw.w.Unlock();
    if (race.Enabled) {
        race.Enable();
    }
}

// RLocker returns a Locker interface that implements
// the Lock and Unlock methods by calling rw.RLock and rw.RUnlock.
private static Locker RLocker(this ptr<RWMutex> _addr_rw) {
    ref RWMutex rw = ref _addr_rw.val;

    return (rlocker.val)(rw);
}

private partial struct rlocker { // : RWMutex
}

private static void Lock(this ptr<rlocker> _addr_r) {
    ref rlocker r = ref _addr_r.val;

    (RWMutex.val)(r).RLock();
}
private static void Unlock(this ptr<rlocker> _addr_r) {
    ref rlocker r = ref _addr_r.val;

    (RWMutex.val)(r).RUnlock();
}

} // end sync_package
