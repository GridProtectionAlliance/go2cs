// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:51 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\rwmutex.go
using atomic = go.runtime.@internal.atomic_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // This is a copy of sync/rwmutex.go rewritten to work in the runtime.

        // A rwmutex is a reader/writer mutual exclusion lock.
        // The lock can be held by an arbitrary number of readers or a single writer.
        // This is a variant of sync.RWMutex, for the runtime package.
        // Like mutex, rwmutex blocks the calling M.
        // It does not interact with the goroutine scheduler.
        private partial struct rwmutex
        {
            public mutex rLock; // protects readers, readerPass, writer
            public muintptr readers; // list of pending readers
            public uint readerPass; // number of pending readers to skip readers list

            public mutex wLock; // serializes writers
            public muintptr writer; // pending writer waiting for completing readers

            public uint readerCount; // number of pending readers
            public uint readerWait; // number of departing readers
        }

        private static readonly long rwmutexMaxReaders = 1L << (int)(30L);

        // rlock locks rw for reading.


        // rlock locks rw for reading.
        private static void rlock(this ref rwmutex rw)
        { 
            // The reader must not be allowed to lose its P or else other
            // things blocking on the lock may consume all of the Ps and
            // deadlock (issue #20903). Alternatively, we could drop the P
            // while sleeping.
            acquirem();
            if (int32(atomic.Xadd(ref rw.readerCount, 1L)) < 0L)
            { 
                // A writer is pending. Park on the reader queue.
                systemstack(() =>
                {
                    lock(ref rw.rLock);
                    if (rw.readerPass > 0L)
                    { 
                        // Writer finished.
                        rw.readerPass -= 1L;
                        unlock(ref rw.rLock);
                    }
                    else
                    { 
                        // Queue this reader to be woken by
                        // the writer.
                        var m = getg().m;
                        m.schedlink = rw.readers;
                        rw.readers.set(m);
                        unlock(ref rw.rLock);
                        notesleep(ref m.park);
                        noteclear(ref m.park);
                    }
                });
            }
        }

        // runlock undoes a single rlock call on rw.
        private static void runlock(this ref rwmutex rw)
        {
            {
                var r = int32(atomic.Xadd(ref rw.readerCount, -1L));

                if (r < 0L)
                {
                    if (r + 1L == 0L || r + 1L == -rwmutexMaxReaders)
                    {
                        throw("runlock of unlocked rwmutex");
                    } 
                    // A writer is pending.
                    if (atomic.Xadd(ref rw.readerWait, -1L) == 0L)
                    { 
                        // The last reader unblocks the writer.
                        lock(ref rw.rLock);
                        var w = rw.writer.ptr();
                        if (w != null)
                        {
                            notewakeup(ref w.park);
                        }
                        unlock(ref rw.rLock);
                    }
                }

            }
            releasem(getg().m);
        }

        // lock locks rw for writing.
        private static void @lock(this ref rwmutex rw)
        { 
            // Resolve competition with other writers and stick to our P.
            lock(ref rw.wLock);
            var m = getg().m; 
            // Announce that there is a pending writer.
            var r = int32(atomic.Xadd(ref rw.readerCount, -rwmutexMaxReaders)) + rwmutexMaxReaders; 
            // Wait for any active readers to complete.
            lock(ref rw.rLock);
            if (r != 0L && atomic.Xadd(ref rw.readerWait, r) != 0L)
            { 
                // Wait for reader to wake us up.
                systemstack(() =>
                {
                    rw.writer.set(m);
                    unlock(ref rw.rLock);
                    notesleep(ref m.park);
                    noteclear(ref m.park);
                }
            else
);
            }            {
                unlock(ref rw.rLock);
            }
        }

        // unlock unlocks rw for writing.
        private static void unlock(this ref rwmutex rw)
        { 
            // Announce to readers that there is no active writer.
            var r = int32(atomic.Xadd(ref rw.readerCount, rwmutexMaxReaders));
            if (r >= rwmutexMaxReaders)
            {
                throw("unlock of unlocked rwmutex");
            } 
            // Unblock blocked readers.
            lock(ref rw.rLock);
            while (rw.readers.ptr() != null)
            {
                var reader = rw.readers.ptr();
                rw.readers = reader.schedlink;
                reader.schedlink.set(null);
                notewakeup(ref reader.park);
                r -= 1L;
            } 
            // If r > 0, there are pending readers that aren't on the
            // queue. Tell them to skip waiting.
 
            // If r > 0, there are pending readers that aren't on the
            // queue. Tell them to skip waiting.
            rw.readerPass += uint32(r);
            unlock(ref rw.rLock); 
            // Allow other writers to proceed.
            unlock(ref rw.wLock);
        }
    }
}
