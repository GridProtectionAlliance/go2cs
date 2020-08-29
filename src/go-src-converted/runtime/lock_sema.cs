// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin nacl netbsd openbsd plan9 solaris windows

// package runtime -- go2cs converted at 2020 August 29 08:17:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\lock_sema.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // This implementation depends on OS-specific implementations of
        //
        //    func semacreate(mp *m)
        //        Create a semaphore for mp, if it does not already have one.
        //
        //    func semasleep(ns int64) int32
        //        If ns < 0, acquire m's semaphore and return 0.
        //        If ns >= 0, try to acquire m's semaphore for at most ns nanoseconds.
        //        Return 0 if the semaphore was acquired, -1 if interrupted or timed out.
        //
        //    func semawakeup(mp *m)
        //        Wake up mp, which is or will soon be sleeping on its semaphore.
        //
        private static readonly System.UIntPtr locked = 1L;

        private static readonly long active_spin = 4L;
        private static readonly long active_spin_cnt = 30L;
        private static readonly long passive_spin = 1L;

        private static void @lock(ref mutex l)
        {
            var gp = getg();
            if (gp.m.locks < 0L)
            {
                throw("runtime·lock: lock count");
            }
            gp.m.locks++; 

            // Speculative grab for lock.
            if (atomic.Casuintptr(ref l.key, 0L, locked))
            {
                return;
            }
            semacreate(gp.m); 

            // On uniprocessor's, no point spinning.
            // On multiprocessors, spin for ACTIVE_SPIN attempts.
            long spin = 0L;
            if (ncpu > 1L)
            {
                spin = active_spin;
            }
Loop:
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                var v = atomic.Loaduintptr(ref l.key);
                if (v & locked == 0L)
                { 
                    // Unlocked. Try to lock.
                    if (atomic.Casuintptr(ref l.key, v, v | locked))
                    {
                        return;
                    }
                    i = 0L;
                }
                if (i < spin)
                {
                    procyield(active_spin_cnt);
                }
                else if (i < spin + passive_spin)
                {
                    osyield();
                }
                else
                { 
                    // Someone else has it.
                    // l->waitm points to a linked list of M's waiting
                    // for this lock, chained through m->nextwaitm.
                    // Queue this M.
                    while (true)
                    {
                        gp.m.nextwaitm = muintptr(v & ~locked);
                        if (atomic.Casuintptr(ref l.key, v, uintptr(@unsafe.Pointer(gp.m)) | locked))
                        {
                            break;
                        }
                        v = atomic.Loaduintptr(ref l.key);
                        if (v & locked == 0L)
                        {
                            _continueLoop = true;
                            break;
                        }
                    }

                    if (v & locked != 0L)
                    { 
                        // Queued. Wait.
                        semasleep(-1L);
                        i = 0L;
                    }
                }
            }
        }

        //go:nowritebarrier
        // We might not be holding a p in this code.
        private static void unlock(ref mutex l)
        {
            var gp = getg();
            ref m mp = default;
            while (true)
            {
                var v = atomic.Loaduintptr(ref l.key);
                if (v == locked)
                {
                    if (atomic.Casuintptr(ref l.key, locked, 0L))
                    {
                        break;
                    }
                }
                else
                { 
                    // Other M's are waiting for the lock.
                    // Dequeue an M.
                    mp = muintptr(v & ~locked).ptr();
                    if (atomic.Casuintptr(ref l.key, v, uintptr(mp.nextwaitm)))
                    { 
                        // Dequeued an M.  Wake it.
                        semawakeup(mp);
                        break;
                    }
                }
            }

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
            System.UIntPtr v = default;
            while (true)
            {
                v = atomic.Loaduintptr(ref n.key);
                if (atomic.Casuintptr(ref n.key, v, locked))
                {
                    break;
                }
            } 

            // Successfully set waitm to locked.
            // What was it before?
 

            // Successfully set waitm to locked.
            // What was it before?

            if (v == 0L)             else if (v == locked) 
                // Two notewakeups! Not allowed.
                throw("notewakeup - double wakeup");
            else 
                // Must be the waiting m. Wake it up.
                semawakeup((m.Value)(@unsafe.Pointer(v)));
                    }

        private static void notesleep(ref note n)
        {
            var gp = getg();
            if (gp != gp.m.g0)
            {
                throw("notesleep not on g0");
            }
            semacreate(gp.m);
            if (!atomic.Casuintptr(ref n.key, 0L, uintptr(@unsafe.Pointer(gp.m))))
            { 
                // Must be locked (got wakeup).
                if (n.key != locked)
                {
                    throw("notesleep - waitm out of sync");
                }
                return;
            } 
            // Queued. Sleep.
            gp.m.blocked = true;
            if (cgo_yield == null.Value)
            {
                semasleep(-1L);
            }
            else
            { 
                // Sleep for an arbitrary-but-moderate interval to poll libc interceptors.
                const float ns = 10e6F;

                while (atomic.Loaduintptr(ref n.key) == 0L)
                {
                    semasleep(ns);
                    asmcgocall(cgo_yield.Value, null);
                }

            }
            gp.m.blocked = false;
        }

        //go:nosplit
        private static bool notetsleep_internal(ref note n, long ns, ref g gp, long deadline)
        { 
            // gp and deadline are logically local variables, but they are written
            // as parameters so that the stack space they require is charged
            // to the caller.
            // This reduces the nosplit footprint of notetsleep_internal.
            gp = getg(); 

            // Register for wakeup on n->waitm.
            if (!atomic.Casuintptr(ref n.key, 0L, uintptr(@unsafe.Pointer(gp.m))))
            { 
                // Must be locked (got wakeup).
                if (n.key != locked)
                {
                    throw("notetsleep - waitm out of sync");
                }
                return true;
            }
            if (ns < 0L)
            { 
                // Queued. Sleep.
                gp.m.blocked = true;
                if (cgo_yield == null.Value)
                {
                    semasleep(-1L);
                }
                else
                { 
                    // Sleep in arbitrary-but-moderate intervals to poll libc interceptors.
                    const float ns = 10e6F;

                    while (semasleep(ns) < 0L)
                    {
                        asmcgocall(cgo_yield.Value, null);
                    }

                }
                gp.m.blocked = false;
                return true;
            }
            deadline = nanotime() + ns;
            while (true)
            { 
                // Registered. Sleep.
                gp.m.blocked = true;
                if (cgo_yield != null && ns > 10e6F.Value)
                {
                    ns = 10e6F;
                }
                if (semasleep(ns) >= 0L)
                {
                    gp.m.blocked = false; 
                    // Acquired semaphore, semawakeup unregistered us.
                    // Done.
                    return true;
                }
                if (cgo_yield != null.Value)
                {
                    asmcgocall(cgo_yield.Value, null);
                }
                gp.m.blocked = false; 
                // Interrupted or timed out. Still registered. Semaphore not acquired.
                ns = deadline - nanotime();
                if (ns <= 0L)
                {
                    break;
                } 
                // Deadline hasn't arrived. Keep sleeping.
            } 

            // Deadline arrived. Still registered. Semaphore not acquired.
            // Want to give up and return, but have to unregister first,
            // so that any notewakeup racing with the return does not
            // try to grant us the semaphore when we don't expect it.
 

            // Deadline arrived. Still registered. Semaphore not acquired.
            // Want to give up and return, but have to unregister first,
            // so that any notewakeup racing with the return does not
            // try to grant us the semaphore when we don't expect it.
            while (true)
            {
                var v = atomic.Loaduintptr(ref n.key);

                if (v == uintptr(@unsafe.Pointer(gp.m))) 
                    // No wakeup yet; unregister if possible.
                    if (atomic.Casuintptr(ref n.key, v, 0L))
                    {
                        return false;
                    }
                else if (v == locked) 
                    // Wakeup happened so semaphore is available.
                    // Grab it to avoid getting out of sync.
                    gp.m.blocked = true;
                    if (semasleep(-1L) < 0L)
                    {
                        throw("runtime: unable to acquire - semaphore out of sync");
                    }
                    gp.m.blocked = false;
                    return true;
                else 
                    throw("runtime: unexpected waitm - semaphore out of sync");
                            }

        }

        private static bool notetsleep(ref note n, long ns)
        {
            var gp = getg();
            if (gp != gp.m.g0 && gp.m.preemptoff != "")
            {
                throw("notetsleep not on g0");
            }
            semacreate(gp.m);
            return notetsleep_internal(n, ns, null, 0L);
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
            semacreate(gp.m);
            entersyscallblock(0L);
            var ok = notetsleep_internal(n, ns, null, 0L);
            exitsyscall(0L);
            return ok;
        }
    }
}
