// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @internal.runtime;

partial class runtime_package {

// This is a copy of sync/rwmutex.go rewritten to work in the runtime.

// A rwmutex is a reader/writer mutual exclusion lock.
// The lock can be held by an arbitrary number of readers or a single writer.
// This is a variant of sync.RWMutex, for the runtime package.
// Like mutex, rwmutex blocks the calling M.
// It does not interact with the goroutine scheduler.
[GoType] partial struct rwmutex {
    internal mutex rLock;    // protects readers, readerPass, writer
    internal muintptr readers; // list of pending readers
    internal uint32 readerPass;   // number of pending readers to skip readers list
    internal mutex wLock;    // serializes writers
    internal muintptr writer; // pending writer waiting for completing readers
    internal @internal.runtime.atomic_package.Int32 readerCount; // number of pending readers
    internal @internal.runtime.atomic_package.Int32 readerWait; // number of departing readers
    internal lockRank readRank; // semantic lock rank for read locking
}

// Lock ranking an rwmutex has two aspects:
//
// Semantic ranking: this rwmutex represents some higher level lock that
// protects some resource (e.g., allocmLock protects creation of new Ms). The
// read and write locks of that resource need to be represented in the lock
// rank.
//
// Internal ranking: as an implementation detail, rwmutex uses two mutexes:
// rLock and wLock. These have lock order requirements: wLock must be locked
// before rLock. This also needs to be represented in the lock rank.
//
// Semantic ranking is represented by acquiring readRank during read lock and
// writeRank during write lock.
//
// wLock is held for the duration of a write lock, so it uses writeRank
// directly, both for semantic and internal ranking. rLock is only held
// temporarily inside the rlock/lock methods, so it uses readRankInternal to
// represent internal ranking. Semantic ranking is represented by a separate
// acquire of readRank for the duration of a read lock.
//
// The lock ranking must document this ordering:
//   - readRankInternal is a leaf lock.
//   - readRank is taken before readRankInternal.
//   - writeRank is taken before readRankInternal.
//   - readRank is placed in the lock order wherever a read lock of this rwmutex
//     belongs.
//   - writeRank is placed in the lock order wherever a write lock of this
//     rwmutex belongs.
[GoRecv] internal static void init(this ref rwmutex rw, lockRank readRank, lockRank readRankInternal, lockRank writeRank) {
    rw.readRank = readRank;
    lockInit(Ꮡ(rw.rLock), readRankInternal);
    lockInit(Ꮡ(rw.wLock), writeRank);
}

internal static readonly UntypedInt rwmutexMaxReaders = /* 1 << 30 */ 1073741824;

// rlock locks rw for reading.
[GoRecv] internal static void rlock(this ref rwmutex rw) {
    // The reader must not be allowed to lose its P or else other
    // things blocking on the lock may consume all of the Ps and
    // deadlock (issue #20903). Alternatively, we could drop the P
    // while sleeping.
    acquireLockRankAndM(rw.readRank);
    lockWithRankMayAcquire(Ꮡ(rw.rLock), getLockRank(Ꮡ(rw.rLock)));
    if (rw.readerCount.Add(1) < 0) {
        // A writer is pending. Park on the reader queue.
        systemstack(() => {
            @lock(Ꮡ(rw.rLock));
            if (rw.readerPass > 0){
                rw.readerPass -= 1;
                unlock(Ꮡ(rw.rLock));
            } else {
                var m = getg().val.m;
                m.val.schedlink = rw.readers;
                rw.readers.set(m);
                unlock(Ꮡ(rw.rLock));
                notesleep(Ꮡ((~m).park));
                noteclear(Ꮡ((~m).park));
            }
        });
    }
}

// runlock undoes a single rlock call on rw.
[GoRecv] internal static void runlock(this ref rwmutex rw) {
    {
        var r = rw.readerCount.Add(-1); if (r < 0) {
            if (r + 1 == 0 || r + 1 == -rwmutexMaxReaders) {
                @throw("runlock of unlocked rwmutex"u8);
            }
            // A writer is pending.
            if (rw.readerWait.Add(-1) == 0) {
                // The last reader unblocks the writer.
                @lock(Ꮡ(rw.rLock));
                var w = rw.writer.ptr();
                if (w != nil) {
                    notewakeup(Ꮡ((~w).park));
                }
                unlock(Ꮡ(rw.rLock));
            }
        }
    }
    releaseLockRankAndM(rw.readRank);
}

// lock locks rw for writing.
[GoRecv] internal static void @lock(this ref rwmutex rw) {
    // Resolve competition with other writers and stick to our P.
    @lock(Ꮡ(rw.wLock));
    var m = getg().val.m;
    // Announce that there is a pending writer.
    var r = rw.readerCount.Add(-rwmutexMaxReaders) + rwmutexMaxReaders;
    // Wait for any active readers to complete.
    @lock(Ꮡ(rw.rLock));
    if (r != 0 && rw.readerWait.Add(r) != 0){
        // Wait for reader to wake us up.
        systemstack(
        var mʗ2 = m;
        () => {
            rw.writer.set(mʗ2);
            unlock(Ꮡ(rw.rLock));
            notesleep(Ꮡ((~mʗ2).park));
            noteclear(Ꮡ((~mʗ2).park));
        });
    } else {
        unlock(Ꮡ(rw.rLock));
    }
}

// unlock unlocks rw for writing.
[GoRecv] internal static void unlock(this ref rwmutex rw) {
    // Announce to readers that there is no active writer.
    var r = rw.readerCount.Add(rwmutexMaxReaders);
    if (r >= rwmutexMaxReaders) {
        @throw("unlock of unlocked rwmutex"u8);
    }
    // Unblock blocked readers.
    @lock(Ꮡ(rw.rLock));
    while (rw.readers.ptr() != nil) {
        var reader = rw.readers.ptr();
        rw.readers = reader.val.schedlink;
        (~reader).schedlink.set(nil);
        notewakeup(Ꮡ((~reader).park));
        r -= 1;
    }
    // If r > 0, there are pending readers that aren't on the
    // queue. Tell them to skip waiting.
    rw.readerPass += ((uint32)r);
    unlock(Ꮡ(rw.rLock));
    // Allow other writers to proceed.
    unlock(Ꮡ(rw.wLock));
}

} // end runtime_package
