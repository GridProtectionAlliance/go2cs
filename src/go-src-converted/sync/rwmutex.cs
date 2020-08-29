// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2020 August 29 08:36:44 UTC
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

        private static readonly long rwmutexMaxReaders = 1L << (int)(30L);

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
        private static void RLock(this ref RWMutex rw)
        {
            if (race.Enabled)
            {
                _ = rw.w.state;
                race.Disable();
            }
            if (atomic.AddInt32(ref rw.readerCount, 1L) < 0L)
            { 
                // A writer is pending, wait for it.
                runtime_Semacquire(ref rw.readerSem);
            }
            if (race.Enabled)
            {
                race.Enable();
                race.Acquire(@unsafe.Pointer(ref rw.readerSem));
            }
        }

        // RUnlock undoes a single RLock call;
        // it does not affect other simultaneous readers.
        // It is a run-time error if rw is not locked for reading
        // on entry to RUnlock.
        private static void RUnlock(this ref RWMutex rw)
        {
            if (race.Enabled)
            {
                _ = rw.w.state;
                race.ReleaseMerge(@unsafe.Pointer(ref rw.writerSem));
                race.Disable();
            }
            {
                var r = atomic.AddInt32(ref rw.readerCount, -1L);

                if (r < 0L)
                {
                    if (r + 1L == 0L || r + 1L == -rwmutexMaxReaders)
                    {
                        race.Enable();
                        throw("sync: RUnlock of unlocked RWMutex");
                    } 
                    // A writer is pending.
                    if (atomic.AddInt32(ref rw.readerWait, -1L) == 0L)
                    { 
                        // The last reader unblocks the writer.
                        runtime_Semrelease(ref rw.writerSem, false);
                    }
                }

            }
            if (race.Enabled)
            {
                race.Enable();
            }
        }

        // Lock locks rw for writing.
        // If the lock is already locked for reading or writing,
        // Lock blocks until the lock is available.
        private static void Lock(this ref RWMutex rw)
        {
            if (race.Enabled)
            {
                _ = rw.w.state;
                race.Disable();
            } 
            // First, resolve competition with other writers.
            rw.w.Lock(); 
            // Announce to readers there is a pending writer.
            var r = atomic.AddInt32(ref rw.readerCount, -rwmutexMaxReaders) + rwmutexMaxReaders; 
            // Wait for active readers.
            if (r != 0L && atomic.AddInt32(ref rw.readerWait, r) != 0L)
            {
                runtime_Semacquire(ref rw.writerSem);
            }
            if (race.Enabled)
            {
                race.Enable();
                race.Acquire(@unsafe.Pointer(ref rw.readerSem));
                race.Acquire(@unsafe.Pointer(ref rw.writerSem));
            }
        }

        // Unlock unlocks rw for writing. It is a run-time error if rw is
        // not locked for writing on entry to Unlock.
        //
        // As with Mutexes, a locked RWMutex is not associated with a particular
        // goroutine. One goroutine may RLock (Lock) a RWMutex and then
        // arrange for another goroutine to RUnlock (Unlock) it.
        private static void Unlock(this ref RWMutex rw)
        {
            if (race.Enabled)
            {
                _ = rw.w.state;
                race.Release(@unsafe.Pointer(ref rw.readerSem));
                race.Release(@unsafe.Pointer(ref rw.writerSem));
                race.Disable();
            } 

            // Announce to readers there is no active writer.
            var r = atomic.AddInt32(ref rw.readerCount, rwmutexMaxReaders);
            if (r >= rwmutexMaxReaders)
            {
                race.Enable();
                throw("sync: Unlock of unlocked RWMutex");
            } 
            // Unblock blocked readers, if any.
            for (long i = 0L; i < int(r); i++)
            {
                runtime_Semrelease(ref rw.readerSem, false);
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
        private static Locker RLocker(this ref RWMutex rw)
        {
            return (rlocker.Value)(rw);
        }

        private partial struct rlocker // : RWMutex
        {
        }

        private static void Lock(this ref rlocker r)
        {
            (RWMutex.Value)(r).RLock();

        }
        private static void Unlock(this ref rlocker r)
        {
            (RWMutex.Value)(r).RUnlock();

        }
    }
}
