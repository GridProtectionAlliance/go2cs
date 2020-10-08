// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 08 03:32:13 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_mutex.go
using atomic = go.sync.atomic_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // fdMutex is a specialized synchronization primitive that manages
        // lifetime of an fd and serializes access to Read, Write and Close
        // methods on FD.
        private partial struct fdMutex
        {
            public ulong state;
            public uint rsema;
            public uint wsema;
        }

        // fdMutex.state is organized as follows:
        // 1 bit - whether FD is closed, if set all subsequent lock operations will fail.
        // 1 bit - lock for read operations.
        // 1 bit - lock for write operations.
        // 20 bits - total number of references (read+write+misc).
        // 20 bits - number of outstanding read waiters.
        // 20 bits - number of outstanding write waiters.
        private static readonly long mutexClosed = (long)1L << (int)(0L);
        private static readonly long mutexRLock = (long)1L << (int)(1L);
        private static readonly long mutexWLock = (long)1L << (int)(2L);
        private static readonly long mutexRef = (long)1L << (int)(3L);
        private static readonly long mutexRefMask = (long)(1L << (int)(20L) - 1L) << (int)(3L);
        private static readonly long mutexRWait = (long)1L << (int)(23L);
        private static readonly long mutexRMask = (long)(1L << (int)(20L) - 1L) << (int)(23L);
        private static readonly long mutexWWait = (long)1L << (int)(43L);
        private static readonly long mutexWMask = (long)(1L << (int)(20L) - 1L) << (int)(43L);


        private static readonly @string overflowMsg = (@string)"too many concurrent operations on a single file or socket (max 1048575)";

        // Read operations must do rwlock(true)/rwunlock(true).
        //
        // Write operations must do rwlock(false)/rwunlock(false).
        //
        // Misc operations must do incref/decref.
        // Misc operations include functions like setsockopt and setDeadline.
        // They need to use incref/decref to ensure that they operate on the
        // correct fd in presence of a concurrent close call (otherwise fd can
        // be closed under their feet).
        //
        // Close operations must do increfAndClose/decref.

        // incref adds a reference to mu.
        // It reports whether mu is available for reading or writing.


        // Read operations must do rwlock(true)/rwunlock(true).
        //
        // Write operations must do rwlock(false)/rwunlock(false).
        //
        // Misc operations must do incref/decref.
        // Misc operations include functions like setsockopt and setDeadline.
        // They need to use incref/decref to ensure that they operate on the
        // correct fd in presence of a concurrent close call (otherwise fd can
        // be closed under their feet).
        //
        // Close operations must do increfAndClose/decref.

        // incref adds a reference to mu.
        // It reports whether mu is available for reading or writing.
        private static bool incref(this ptr<fdMutex> _addr_mu) => func((_, panic, __) =>
        {
            ref fdMutex mu = ref _addr_mu.val;

            while (true)
            {
                var old = atomic.LoadUint64(_addr_mu.state);
                if (old & mutexClosed != 0L)
                {
                    return false;
                }

                var @new = old + mutexRef;
                if (new & mutexRefMask == 0L)
                {
                    panic(overflowMsg);
                }

                if (atomic.CompareAndSwapUint64(_addr_mu.state, old, new))
                {
                    return true;
                }

            }


        });

        // increfAndClose sets the state of mu to closed.
        // It returns false if the file was already closed.
        private static bool increfAndClose(this ptr<fdMutex> _addr_mu) => func((_, panic, __) =>
        {
            ref fdMutex mu = ref _addr_mu.val;

            while (true)
            {
                var old = atomic.LoadUint64(_addr_mu.state);
                if (old & mutexClosed != 0L)
                {
                    return false;
                } 
                // Mark as closed and acquire a reference.
                var @new = (old | mutexClosed) + mutexRef;
                if (new & mutexRefMask == 0L)
                {
                    panic(overflowMsg);
                } 
                // Remove all read and write waiters.
                new &= mutexRMask | mutexWMask;
                if (atomic.CompareAndSwapUint64(_addr_mu.state, old, new))
                { 
                    // Wake all read and write waiters,
                    // they will observe closed flag after wakeup.
                    while (old & mutexRMask != 0L)
                    {
                        old -= mutexRWait;
                        runtime_Semrelease(_addr_mu.rsema);
                    }

                    while (old & mutexWMask != 0L)
                    {
                        old -= mutexWWait;
                        runtime_Semrelease(_addr_mu.wsema);
                    }

                    return true;

                }

            }


        });

        // decref removes a reference from mu.
        // It reports whether there is no remaining reference.
        private static bool decref(this ptr<fdMutex> _addr_mu) => func((_, panic, __) =>
        {
            ref fdMutex mu = ref _addr_mu.val;

            while (true)
            {
                var old = atomic.LoadUint64(_addr_mu.state);
                if (old & mutexRefMask == 0L)
                {
                    panic("inconsistent poll.fdMutex");
                }

                var @new = old - mutexRef;
                if (atomic.CompareAndSwapUint64(_addr_mu.state, old, new))
                {
                    return new & (mutexClosed | mutexRefMask) == mutexClosed;
                }

            }


        });

        // lock adds a reference to mu and locks mu.
        // It reports whether mu is available for reading or writing.
        private static bool rwlock(this ptr<fdMutex> _addr_mu, bool read) => func((_, panic, __) =>
        {
            ref fdMutex mu = ref _addr_mu.val;

            ulong mutexBit = default;            ulong mutexWait = default;            ulong mutexMask = default;

            ptr<uint> mutexSema;
            if (read)
            {
                mutexBit = mutexRLock;
                mutexWait = mutexRWait;
                mutexMask = mutexRMask;
                mutexSema = _addr_mu.rsema;
            }
            else
            {
                mutexBit = mutexWLock;
                mutexWait = mutexWWait;
                mutexMask = mutexWMask;
                mutexSema = _addr_mu.wsema;
            }

            while (true)
            {
                var old = atomic.LoadUint64(_addr_mu.state);
                if (old & mutexClosed != 0L)
                {
                    return false;
                }

                ulong @new = default;
                if (old & mutexBit == 0L)
                { 
                    // Lock is free, acquire it.
                    new = (old | mutexBit) + mutexRef;
                    if (new & mutexRefMask == 0L)
                    {
                        panic(overflowMsg);
                    }

                }
                else
                { 
                    // Wait for lock.
                    new = old + mutexWait;
                    if (new & mutexMask == 0L)
                    {
                        panic(overflowMsg);
                    }

                }

                if (atomic.CompareAndSwapUint64(_addr_mu.state, old, new))
                {
                    if (old & mutexBit == 0L)
                    {
                        return true;
                    }

                    runtime_Semacquire(mutexSema); 
                    // The signaller has subtracted mutexWait.
                }

            }


        });

        // unlock removes a reference from mu and unlocks mu.
        // It reports whether there is no remaining reference.
        private static bool rwunlock(this ptr<fdMutex> _addr_mu, bool read) => func((_, panic, __) =>
        {
            ref fdMutex mu = ref _addr_mu.val;

            ulong mutexBit = default;            ulong mutexWait = default;            ulong mutexMask = default;

            ptr<uint> mutexSema;
            if (read)
            {
                mutexBit = mutexRLock;
                mutexWait = mutexRWait;
                mutexMask = mutexRMask;
                mutexSema = _addr_mu.rsema;
            }
            else
            {
                mutexBit = mutexWLock;
                mutexWait = mutexWWait;
                mutexMask = mutexWMask;
                mutexSema = _addr_mu.wsema;
            }

            while (true)
            {
                var old = atomic.LoadUint64(_addr_mu.state);
                if (old & mutexBit == 0L || old & mutexRefMask == 0L)
                {
                    panic("inconsistent poll.fdMutex");
                } 
                // Drop lock, drop reference and wake read waiter if present.
                var @new = (old & ~mutexBit) - mutexRef;
                if (old & mutexMask != 0L)
                {
                    new -= mutexWait;
                }

                if (atomic.CompareAndSwapUint64(_addr_mu.state, old, new))
                {
                    if (old & mutexMask != 0L)
                    {
                        runtime_Semrelease(mutexSema);
                    }

                    return new & (mutexClosed | mutexRefMask) == mutexClosed;

                }

            }


        });

        // Implemented in runtime package.
        private static void runtime_Semacquire(ptr<uint> sema)
;
        private static void runtime_Semrelease(ptr<uint> sema)
;

        // incref adds a reference to fd.
        // It returns an error when fd cannot be used.
        private static error incref(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (!fd.fdmu.incref())
            {>>MARKER:FUNCTION_runtime_Semrelease_BLOCK_PREFIX<<
                return error.As(errClosing(fd.isFile))!;
            }

            return error.As(null!)!;

        }

        // decref removes a reference from fd.
        // It also closes fd when the state of fd is set to closed and there
        // is no remaining reference.
        private static error decref(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (fd.fdmu.decref())
            {>>MARKER:FUNCTION_runtime_Semacquire_BLOCK_PREFIX<<
                return error.As(fd.destroy())!;
            }

            return error.As(null!)!;

        }

        // readLock adds a reference to fd and locks fd for reading.
        // It returns an error when fd cannot be used for reading.
        private static error readLock(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (!fd.fdmu.rwlock(true))
            {
                return error.As(errClosing(fd.isFile))!;
            }

            return error.As(null!)!;

        }

        // readUnlock removes a reference from fd and unlocks fd for reading.
        // It also closes fd when the state of fd is set to closed and there
        // is no remaining reference.
        private static void readUnlock(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (fd.fdmu.rwunlock(true))
            {
                fd.destroy();
            }

        }

        // writeLock adds a reference to fd and locks fd for writing.
        // It returns an error when fd cannot be used for writing.
        private static error writeLock(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (!fd.fdmu.rwlock(false))
            {
                return error.As(errClosing(fd.isFile))!;
            }

            return error.As(null!)!;

        }

        // writeUnlock removes a reference from fd and unlocks fd for writing.
        // It also closes fd when the state of fd is set to closed and there
        // is no remaining reference.
        private static void writeUnlock(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (fd.fdmu.rwunlock(false))
            {
                fd.destroy();
            }

        }
    }
}}
