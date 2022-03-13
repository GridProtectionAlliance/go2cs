// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || linux
// +build dragonfly freebsd linux

// package runtime -- go2cs converted at 2022 March 13 05:24:38 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\lock_futex.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;


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

public static partial class runtime_package {

private static readonly nint mutex_unlocked = 0;
private static readonly nint mutex_locked = 1;
private static readonly nint mutex_sleeping = 2;

private static readonly nint active_spin = 4;
private static readonly nint active_spin_cnt = 30;
private static readonly nint passive_spin = 1;

// Possible lock states are mutex_unlocked, mutex_locked and mutex_sleeping.
// mutex_sleeping means that there is presumably at least one sleeping thread.
// Note that there can be spinning threads during all states - they do not
// affect mutex's state.

// We use the uintptr mutex.key and note.key as a uint32.
//go:nosplit
private static ptr<uint> key32(ptr<System.UIntPtr> _addr_p) {
    ref System.UIntPtr p = ref _addr_p.val;

    return _addr_(uint32.val)(@unsafe.Pointer(p))!;
}

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
    var v = atomic.Xchg(key32(_addr_l.key), mutex_locked);
    if (v == mutex_unlocked) {
        return ;
    }
    var wait = v; 

    // On uniprocessors, no point spinning.
    // On multiprocessors, spin for ACTIVE_SPIN attempts.
    nint spin = 0;
    if (ncpu > 1) {
        spin = active_spin;
    }
    while (true) { 
        // Try for lock, spinning.
        {
            nint i__prev2 = i;

            for (nint i = 0; i < spin; i++) {
                while (l.key == mutex_unlocked) {
                    if (atomic.Cas(key32(_addr_l.key), mutex_unlocked, wait)) {
                        return ;
                    }
                }

                procyield(active_spin_cnt);
            } 

            // Try for lock, rescheduling.


            i = i__prev2;
        } 

        // Try for lock, rescheduling.
        {
            nint i__prev2 = i;

            for (i = 0; i < passive_spin; i++) {
                while (l.key == mutex_unlocked) {
                    if (atomic.Cas(key32(_addr_l.key), mutex_unlocked, wait)) {
                        return ;
                    }
                }

                osyield();
            } 

            // Sleep.


            i = i__prev2;
        } 

        // Sleep.
        v = atomic.Xchg(key32(_addr_l.key), mutex_sleeping);
        if (v == mutex_unlocked) {
            return ;
        }
        wait = mutex_sleeping;
        futexsleep(key32(_addr_l.key), mutex_sleeping, -1);
    }
}

private static void unlock(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    unlockWithRank(l);
}

private static void unlock2(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    var v = atomic.Xchg(key32(_addr_l.key), mutex_unlocked);
    if (v == mutex_unlocked) {
        throw("unlock of unlocked lock");
    }
    if (v == mutex_sleeping) {
        futexwakeup(key32(_addr_l.key), 1);
    }
    var gp = getg();
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

    n.key = 0;
}

private static void notewakeup(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;

    var old = atomic.Xchg(key32(_addr_n.key), 1);
    if (old != 0) {
        print("notewakeup - double wakeup (", old, ")\n");
        throw("notewakeup - double wakeup");
    }
    futexwakeup(key32(_addr_n.key), 1);
}

private static void notesleep(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;

    var gp = getg();
    if (gp != gp.m.g0) {
        throw("notesleep not on g0");
    }
    var ns = int64(-1);
    if (cgo_yield != null.val) { 
        // Sleep for an arbitrary-but-moderate interval to poll libc interceptors.
        ns = 10e6F;
    }
    while (atomic.Load(key32(_addr_n.key)) == 0) {
        gp.m.blocked = true;
        futexsleep(key32(_addr_n.key), 0, ns);
        if (cgo_yield != null.val) {
            asmcgocall(cgo_yield.val, null);
        }
        gp.m.blocked = false;
    }
}

// May run with m.p==nil if called from notetsleep, so write barriers
// are not allowed.
//
//go:nosplit
//go:nowritebarrier
private static bool notetsleep_internal(ptr<note> _addr_n, long ns) {
    ref note n = ref _addr_n.val;

    var gp = getg();

    if (ns < 0) {
        if (cgo_yield != null.val) { 
            // Sleep for an arbitrary-but-moderate interval to poll libc interceptors.
            ns = 10e6F;
        }
        while (atomic.Load(key32(_addr_n.key)) == 0) {
            gp.m.blocked = true;
            futexsleep(key32(_addr_n.key), 0, ns);
            if (cgo_yield != null.val) {
                asmcgocall(cgo_yield.val, null);
            }
            gp.m.blocked = false;
        }
        return true;
    }
    if (atomic.Load(key32(_addr_n.key)) != 0) {
        return true;
    }
    var deadline = nanotime() + ns;
    while (true) {
        if (cgo_yield != null && ns > 10e6F.val) {
            ns = 10e6F;
        }
        gp.m.blocked = true;
        futexsleep(key32(_addr_n.key), 0, ns);
        if (cgo_yield != null.val) {
            asmcgocall(cgo_yield.val, null);
        }
        gp.m.blocked = false;
        if (atomic.Load(key32(_addr_n.key)) != 0) {
            break;
        }
        var now = nanotime();
        if (now >= deadline) {
            break;
        }
        ns = deadline - now;
    }
    return atomic.Load(key32(_addr_n.key)) != 0;
}

private static bool notetsleep(ptr<note> _addr_n, long ns) {
    ref note n = ref _addr_n.val;

    var gp = getg();
    if (gp != gp.m.g0 && gp.m.preemptoff != "") {
        throw("notetsleep not on g0");
    }
    return notetsleep_internal(_addr_n, ns);
}

// same as runtime·notetsleep, but called on user g (not g0)
// calls only nosplit functions between entersyscallblock/exitsyscall
private static bool notetsleepg(ptr<note> _addr_n, long ns) {
    ref note n = ref _addr_n.val;

    var gp = getg();
    if (gp == gp.m.g0) {
        throw("notetsleepg on g0");
    }
    entersyscallblock();
    var ok = notetsleep_internal(_addr_n, ns);
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
