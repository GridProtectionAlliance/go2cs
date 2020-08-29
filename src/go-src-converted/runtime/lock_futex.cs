// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux

// package runtime -- go2cs converted at 2020 August 29 08:17:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\lock_futex.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // This implementation depends on OS-specific implementations of
        //
        //    futexsleep(addr *uint32, val uint32, ns int64)
        //        Atomically,
        //            if *addr == val { sleep }
        //        Might be woken up spuriously; that's allowed.
        //        Don't sleep longer than ns; ns < 0 means forever.
        //
        //    futexwakeup(addr *uint32, cnt uint32)
        //        If any procs are sleeping on addr, wake up at most cnt.
        private static readonly long mutex_unlocked = 0L;
        private static readonly long mutex_locked = 1L;
        private static readonly long mutex_sleeping = 2L;

        private static readonly long active_spin = 4L;
        private static readonly long active_spin_cnt = 30L;
        private static readonly long passive_spin = 1L;

        // Possible lock states are mutex_unlocked, mutex_locked and mutex_sleeping.
        // mutex_sleeping means that there is presumably at least one sleeping thread.
        // Note that there can be spinning threads during all states - they do not
        // affect mutex's state.

        // We use the uintptr mutex.key and note.key as a uint32.
        //go:nosplit
        private static ref uint key32(ref System.UIntPtr p)
        {
            return (uint32.Value)(@unsafe.Pointer(p));
        }

        private static void @lock(ref mutex l)
        {
            var gp = getg();

            if (gp.m.locks < 0L)
            {
                throw("runtime·lock: lock count");
            }
            gp.m.locks++; 

            // Speculative grab for lock.
            var v = atomic.Xchg(key32(ref l.key), mutex_locked);
            if (v == mutex_unlocked)
            {
                return;
            } 

            // wait is either MUTEX_LOCKED or MUTEX_SLEEPING
            // depending on whether there is a thread sleeping
            // on this mutex. If we ever change l->key from
            // MUTEX_SLEEPING to some other value, we must be
            // careful to change it back to MUTEX_SLEEPING before
            // returning, to ensure that the sleeping thread gets
            // its wakeup call.
            var wait = v; 

            // On uniprocessors, no point spinning.
            // On multiprocessors, spin for ACTIVE_SPIN attempts.
            long spin = 0L;
            if (ncpu > 1L)
            {
                spin = active_spin;
            }
            while (true)
            { 
                // Try for lock, spinning.
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < spin; i++)
                    {
                        while (l.key == mutex_unlocked)
                        {
                            if (atomic.Cas(key32(ref l.key), mutex_unlocked, wait))
                            {
                                return;
                            }
                        }

                        procyield(active_spin_cnt);
                    } 

                    // Try for lock, rescheduling.


                    i = i__prev2;
                } 

                // Try for lock, rescheduling.
                {
                    long i__prev2 = i;

                    for (i = 0L; i < passive_spin; i++)
                    {
                        while (l.key == mutex_unlocked)
                        {
                            if (atomic.Cas(key32(ref l.key), mutex_unlocked, wait))
                            {
                                return;
                            }
                        }

                        osyield();
                    } 

                    // Sleep.


                    i = i__prev2;
                } 

                // Sleep.
                v = atomic.Xchg(key32(ref l.key), mutex_sleeping);
                if (v == mutex_unlocked)
                {
                    return;
                }
                wait = mutex_sleeping;
                futexsleep(key32(ref l.key), mutex_sleeping, -1L);
            }

        }

        private static void unlock(ref mutex l)
        {
            var v = atomic.Xchg(key32(ref l.key), mutex_unlocked);
            if (v == mutex_unlocked)
            {
                throw("unlock of unlocked lock");
            }
            if (v == mutex_sleeping)
            {
                futexwakeup(key32(ref l.key), 1L);
            }
            var gp = getg();
            gp.m.locks--;
            if (gp.m.locks < 0L)
            {
                throw("runtime·unlock: lock count");
            }
            if (gp.m.locks == 0L && gp.preempt)
            { // restore the preemption request in case we've cleared it in newstack
                gp.stackguard0 = stackPreempt;
            }
        }

        // One-time notifications.
        private static void noteclear(ref note n)
        {
            n.key = 0L;
        }

        private static void notewakeup(ref note n)
        {
            var old = atomic.Xchg(key32(ref n.key), 1L);
            if (old != 0L)
            {
                print("notewakeup - double wakeup (", old, ")\n");
                throw("notewakeup - double wakeup");
            }
            futexwakeup(key32(ref n.key), 1L);
        }

        private static void notesleep(ref note n)
        {
            var gp = getg();
            if (gp != gp.m.g0)
            {
                throw("notesleep not on g0");
            }
            var ns = int64(-1L);
            if (cgo_yield != null.Value)
            { 
                // Sleep for an arbitrary-but-moderate interval to poll libc interceptors.
                ns = 10e6F;
            }
            while (atomic.Load(key32(ref n.key)) == 0L)
            {
                gp.m.blocked = true;
                futexsleep(key32(ref n.key), 0L, ns);
                if (cgo_yield != null.Value)
                {
                    asmcgocall(cgo_yield.Value, null);
                }
                gp.m.blocked = false;
            }

        }

        // May run with m.p==nil if called from notetsleep, so write barriers
        // are not allowed.
        //
        //go:nosplit
        //go:nowritebarrier
        private static bool notetsleep_internal(ref note n, long ns)
        {
            var gp = getg();

            if (ns < 0L)
            {
                if (cgo_yield != null.Value)
                { 
                    // Sleep for an arbitrary-but-moderate interval to poll libc interceptors.
                    ns = 10e6F;
                }
                while (atomic.Load(key32(ref n.key)) == 0L)
                {
                    gp.m.blocked = true;
                    futexsleep(key32(ref n.key), 0L, ns);
                    if (cgo_yield != null.Value)
                    {
                        asmcgocall(cgo_yield.Value, null);
                    }
                    gp.m.blocked = false;
                }

                return true;
            }
            if (atomic.Load(key32(ref n.key)) != 0L)
            {
                return true;
            }
            var deadline = nanotime() + ns;
            while (true)
            {
                if (cgo_yield != null && ns > 10e6F.Value)
                {
                    ns = 10e6F;
                }
                gp.m.blocked = true;
                futexsleep(key32(ref n.key), 0L, ns);
                if (cgo_yield != null.Value)
                {
                    asmcgocall(cgo_yield.Value, null);
                }
                gp.m.blocked = false;
                if (atomic.Load(key32(ref n.key)) != 0L)
                {
                    break;
                }
                var now = nanotime();
                if (now >= deadline)
                {
                    break;
                }
                ns = deadline - now;
            }

            return atomic.Load(key32(ref n.key)) != 0L;
        }

        private static bool notetsleep(ref note n, long ns)
        {
            var gp = getg();
            if (gp != gp.m.g0 && gp.m.preemptoff != "")
            {
                throw("notetsleep not on g0");
            }
            return notetsleep_internal(n, ns);
        }

        // same as runtime·notetsleep, but called on user g (not g0)
        // calls only nosplit functions between entersyscallblock/exitsyscall
        private static bool notetsleepg(ref note n, long ns)
        {
            var gp = getg();
            if (gp == gp.m.g0)
            {
                throw("notetsleepg on g0");
            }
            entersyscallblock(0L);
            var ok = notetsleep_internal(n, ns);
            exitsyscall(0L);
            return ok;
        }
    }
}
