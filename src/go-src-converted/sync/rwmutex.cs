// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2020 October 08 03:18:57 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Go\src\sync\rwmutex.go
using race = go.@internal.race_package;
using atomic = go.sync.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class sync_package
    {
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
        public partial struct RWMutex
        {
            public Mutex w; // held if there are pending writers
            public uint writerSem; // semaphore for writers to wait for completing readers
            public uint readerSem; // semaphore for readers to wait for completing writers
            public int readerCount; // number of pending readers
            public int readerWait; // number of departing readers
        }

        private static readonly long rwmutexMaxReaders = (long)1L << (int)(30L);

        // RLock locks rw for reading.
        //
        // It should not be used for recursive read locking; a blocked Lock
        // call excludes new readers from acquiring the lock. See the
        // documentation on the RWMutex type.


        // RLock locks rw for reading.
        //
        // It should not be used for recursive read locking; a blocked Lock
        // call excludes new readers from acquiring the lock. See the
        // documentation on the RWMutex type.
        private static void RLock(this ptr<RWMutex> _addr_rw)
        {
            ref RWMutex rw = ref _addr_rw.val;

            if (race.Enabled)
            {
                _ = rw.w.state;
                race.Disable();
            }

            if (atomic.AddInt32(_addr_rw.readerCount, 1L) < 0L)
            { 
                // A writer is pending, wait for it.
                runtime_SemacquireMutex(_addr_rw.readerSem, false, 0L);

            }

            if (race.Enabled)
            {
                race.Enable();
                race.Acquire(@unsafe.Pointer(_addr_rw.readerSem));
            }

        }

        // RUnlock undoes a single RLock call;
        // it does not affect other simultaneous readers.
        // It is a run-time error if rw is not locked for reading
        // on entry to RUnlock.
        private static void RUnlock(this ptr<RWMutex> _addr_rw)
        {
            ref RWMutex rw = ref _addr_rw.val;

            if (race.Enabled)
            {
                _ = rw.w.state;
                race.ReleaseMerge(@unsafe.Pointer(_addr_rw.writerSem));
                race.Disable();
            }

            {
                var r = atomic.AddInt32(_addr_rw.readerCount, -1L);

                if (r < 0L)
                { 
                    // Outlined slow-path to allow the fast-path to be inlined
                    rw.rUnlockSlow(r);

                }

            }

            if (race.Enabled)
            {
                race.Enable();
            }

        }

        private static void rUnlockSlow(this ptr<RWMutex> _addr_rw, int r)
        {
            ref RWMutex rw = ref _addr_rw.val;

            if (r + 1L == 0L || r + 1L == -rwmutexMaxReaders)
            {
                race.Enable();
                throw("sync: RUnlock of unlocked RWMutex");
            } 
            // A writer is pending.
            if (atomic.AddInt32(_addr_rw.readerWait, -1L) == 0L)
            { 
                // The last reader unblocks the writer.
                runtime_Semrelease(_addr_rw.writerSem, false, 1L);

            }

        }

        // Lock locks rw for writing.
        // If the lock is already locked for reading or writing,
        // Lock blocks until the lock is available.
        private static void Lock(this ptr<RWMutex> _addr_rw)
        {
            ref RWMutex rw = ref _addr_rw.val;

            if (race.Enabled)
            {
                _ = rw.w.state;
                race.Disable();
            } 
            // First, resolve competition with other writers.
            rw.w.Lock(); 
            // Announce to readers there is a pending writer.
            var r = atomic.AddInt32(_addr_rw.readerCount, -rwmutexMaxReaders) + rwmutexMaxReaders; 
            // Wait for active readers.
            if (r != 0L && atomic.AddInt32(_addr_rw.readerWait, r) != 0L)
            {
                runtime_SemacquireMutex(_addr_rw.writerSem, false, 0L);
            }

            if (race.Enabled)
            {
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
        private static void Unlock(this ptr<RWMutex> _addr_rw)
        {
            ref RWMutex rw = ref _addr_rw.val;

            if (race.Enabled)
            {
                _ = rw.w.state;
                race.Release(@unsafe.Pointer(_addr_rw.readerSem));
                race.Disable();
            } 

            // Announce to readers there is no active writer.
            var r = atomic.AddInt32(_addr_rw.readerCount, rwmutexMaxReaders);
            if (r >= rwmutexMaxReaders)
            {
                race.Enable();
                throw("sync: Unlock of unlocked RWMutex");
            } 
            // Unblock blocked readers, if any.
            for (long i = 0L; i < int(r); i++)
            {
                runtime_Semrelease(_addr_rw.readerSem, false, 0L);
            } 
            // Allow other writers to proceed.
 
            // Allow other writers to proceed.
            rw.w.Unlock();
            if (race.Enabled)
            {
                race.Enable();
            }

        }

        // RLocker returns a Locker interface that implements
        // the Lock and Unlock methods by calling rw.RLock and rw.RUnlock.
        private static Locker RLocker(this ptr<RWMutex> _addr_rw)
        {
            ref RWMutex rw = ref _addr_rw.val;

            return (rlocker.val)(rw);
        }

        private partial struct rlocker // : RWMutex
        {
        }

        private static void Lock(this ptr<rlocker> _addr_r)
        {
            ref rlocker r = ref _addr_r.val;

            (RWMutex.val)(r).RLock();
        }
        private static void Unlock(this ptr<rlocker> _addr_r)
        {
            ref rlocker r = ref _addr_r.val;

            (RWMutex.val)(r).RUnlock();
        }
    }
}
