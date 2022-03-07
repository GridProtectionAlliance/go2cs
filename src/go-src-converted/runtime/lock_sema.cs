// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || netbsd || openbsd || plan9 || solaris || windows
// +build aix darwin netbsd openbsd plan9 solaris windows

// package runtime -- go2cs converted at 2022 March 06 22:08:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\lock_sema.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

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
private static readonly System.UIntPtr locked = 1;

private static readonly nint active_spin = 4;
private static readonly nint active_spin_cnt = 30;
private static readonly nint passive_spin = 1;


private static void @lock(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    lockWithRank(l, getLockRank(l));
}

private static void lock2(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    var gp = getg();
    if (gp.m.locks < 0) {
        throw("runtime·lock: lock count");
    }
    gp.m.locks++; 

    // Speculative grab for lock.
    if (atomic.Casuintptr(_addr_l.key, 0, locked)) {
        return ;
    }
    semacreate(gp.m); 

    // On uniprocessor's, no point spinning.
    // On multiprocessors, spin for ACTIVE_SPIN attempts.
    nint spin = 0;
    if (ncpu > 1) {
        spin = active_spin;
    }
Loop:
    for (nint i = 0; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        var v = atomic.Loaduintptr(_addr_l.key);
        if (v & locked == 0) { 
            // Unlocked. Try to lock.
            if (atomic.Casuintptr(_addr_l.key, v, v | locked)) {
                return ;
            }

            i = 0;

        }
        if (i < spin) {
            procyield(active_spin_cnt);
        }
        else if (i < spin + passive_spin) {
            osyield();
        }
        else
 { 
            // Someone else has it.
            // l->waitm points to a linked list of M's waiting
            // for this lock, chained through m->nextwaitm.
            // Queue this M.
            while (true) {
                gp.m.nextwaitm = muintptr(v & ~locked);
                if (atomic.Casuintptr(_addr_l.key, v, uintptr(@unsafe.Pointer(gp.m)) | locked)) {
                    break;
                }
                v = atomic.Loaduintptr(_addr_l.key);
                if (v & locked == 0) {
                    _continueLoop = true;
                    break;
                }

            }

            if (v & locked != 0) { 
                // Queued. Wait.
                semasleep(-1);
                i = 0;

            }

        }
    }

}

private static void unlock(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    unlockWithRank(l);
}

//go:nowritebarrier
// We might not be holding a p in this code.
private static void unlock2(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    var gp = getg();
    ptr<m> mp;
    while (true) {
        var v = atomic.Loaduintptr(_addr_l.key);
        if (v == locked) {
            if (atomic.Casuintptr(_addr_l.key, locked, 0)) {
                break;
            }
        }
        else
 { 
            // Other M's are waiting for the lock.
            // Dequeue an M.
            mp = muintptr(v & ~locked).ptr();
            if (atomic.Casuintptr(_addr_l.key, v, uintptr(mp.nextwaitm))) { 
                // Dequeued an M.  Wake it.
                semawakeup(mp);
                break;

            }

        }
    }
    gp.m.locks--;
    if (gp.m.locks < 0) {
        throw("runtime·unlock: lock count");
    }
    if (gp.m.locks == 0 && gp.preempt) { // restore the preemption request in case we've cleared it in newstack
        gp.stackguard0 = stackPreempt;

    }
}

// One-time notifications.
private static void noteclear(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;

    if (GOOS == "aix") { 
        // On AIX, semaphores might not synchronize the memory in some
        // rare cases. See issue #30189.
        atomic.Storeuintptr(_addr_n.key, 0);

    }
    else
 {
        n.key = 0;
    }
}

private static void notewakeup(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;

    System.UIntPtr v = default;
    while (true) {
        v = atomic.Loaduintptr(_addr_n.key);
        if (atomic.Casuintptr(_addr_n.key, v, locked)) {
            break;
        }
    } 

    // Successfully set waitm to locked.
    // What was it before?

    if (v == 0)     else if (v == locked) 
        // Two notewakeups! Not allowed.
        throw("notewakeup - double wakeup");
    else 
        // Must be the waiting m. Wake it up.
        semawakeup((m.val)(@unsafe.Pointer(v)));
    
}

private static void notesleep(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;

    var gp = getg();
    if (gp != gp.m.g0) {
        throw("notesleep not on g0");
    }
    semacreate(gp.m);
    if (!atomic.Casuintptr(_addr_n.key, 0, uintptr(@unsafe.Pointer(gp.m)))) { 
        // Must be locked (got wakeup).
        if (n.key != locked) {
            throw("notesleep - waitm out of sync");
        }
        return ;

    }
    gp.m.blocked = true;
    if (cgo_yield == null.val) {
        semasleep(-1);
    }
    else
 { 
        // Sleep for an arbitrary-but-moderate interval to poll libc interceptors.
        const float ns = 10e6F;

        while (atomic.Loaduintptr(_addr_n.key) == 0) {
            semasleep(ns);
            asmcgocall(cgo_yield.val, null);
        }

    }
    gp.m.blocked = false;

}

//go:nosplit
private static bool notetsleep_internal(ptr<note> _addr_n, long ns, ptr<g> _addr_gp, long deadline) {
    ref note n = ref _addr_n.val;
    ref g gp = ref _addr_gp.val;
 
    // gp and deadline are logically local variables, but they are written
    // as parameters so that the stack space they require is charged
    // to the caller.
    // This reduces the nosplit footprint of notetsleep_internal.
    gp = getg(); 

    // Register for wakeup on n->waitm.
    if (!atomic.Casuintptr(_addr_n.key, 0, uintptr(@unsafe.Pointer(gp.m)))) { 
        // Must be locked (got wakeup).
        if (n.key != locked) {
            throw("notetsleep - waitm out of sync");
        }
        return true;

    }
    if (ns < 0) { 
        // Queued. Sleep.
        gp.m.blocked = true;
        if (cgo_yield == null.val) {
            semasleep(-1);
        }
        else
 { 
            // Sleep in arbitrary-but-moderate intervals to poll libc interceptors.
            const float ns = 10e6F;

            while (semasleep(ns) < 0) {
                asmcgocall(cgo_yield.val, null);
            }


        }
        gp.m.blocked = false;
        return true;

    }
    deadline = nanotime() + ns;
    while (true) { 
        // Registered. Sleep.
        gp.m.blocked = true;
        if (cgo_yield != null && ns > 10e6F.val) {
            ns = 10e6F;
        }
        if (semasleep(ns) >= 0) {
            gp.m.blocked = false; 
            // Acquired semaphore, semawakeup unregistered us.
            // Done.
            return true;

        }
        if (cgo_yield != null.val) {
            asmcgocall(cgo_yield.val, null);
        }
        gp.m.blocked = false; 
        // Interrupted or timed out. Still registered. Semaphore not acquired.
        ns = deadline - nanotime();
        if (ns <= 0) {
            break;
        }
    } 

    // Deadline arrived. Still registered. Semaphore not acquired.
    // Want to give up and return, but have to unregister first,
    // so that any notewakeup racing with the return does not
    // try to grant us the semaphore when we don't expect it.
    while (true) {
        var v = atomic.Loaduintptr(_addr_n.key);

        if (v == uintptr(@unsafe.Pointer(gp.m))) 
            // No wakeup yet; unregister if possible.
            if (atomic.Casuintptr(_addr_n.key, v, 0)) {
                return false;
            }
        else if (v == locked) 
            // Wakeup happened so semaphore is available.
            // Grab it to avoid getting out of sync.
            gp.m.blocked = true;
            if (semasleep(-1) < 0) {
                throw("runtime: unable to acquire - semaphore out of sync");
            }
            gp.m.blocked = false;
            return true;
        else 
            throw("runtime: unexpected waitm - semaphore out of sync");
        
    }

}

private static bool notetsleep(ptr<note> _addr_n, long ns) {
    ref note n = ref _addr_n.val;

    var gp = getg();
    if (gp != gp.m.g0) {
        throw("notetsleep not on g0");
    }
    semacreate(gp.m);
    return notetsleep_internal(_addr_n, ns, _addr_null, 0);

}

// same as runtime·notetsleep, but called on user g (not g0)
// calls only nosplit functions between entersyscallblock/exitsyscall
private static bool notetsleepg(ptr<note> _addr_n, long ns) {
    ref note n = ref _addr_n.val;

    var gp = getg();
    if (gp == gp.m.g0) {
        throw("notetsleepg on g0");
    }
    semacreate(gp.m);
    entersyscallblock();
    var ok = notetsleep_internal(_addr_n, ns, _addr_null, 0);
    exitsyscall();
    return ok;

}

private static (ptr<g>, bool) beforeIdle(long _p0, long _p0) {
    ptr<g> _p0 = default!;
    bool _p0 = default;

    return (_addr_null!, false);
}

private static void checkTimeouts() {
}

} // end runtime_package
