// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package runtime -- go2cs converted at 2022 March 06 22:10:14 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\netpoll.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // Integrated network poller (platform-independent part).
    // A particular implementation (epoll/kqueue/port/AIX/Windows)
    // must define the following functions:
    //
    // func netpollinit()
    //     Initialize the poller. Only called once.
    //
    // func netpollopen(fd uintptr, pd *pollDesc) int32
    //     Arm edge-triggered notifications for fd. The pd argument is to pass
    //     back to netpollready when fd is ready. Return an errno value.
    //
    // func netpollclose(fd uintptr) int32
    //     Disable notifications for fd. Return an errno value.
    //
    // func netpoll(delta int64) gList
    //     Poll the network. If delta < 0, block indefinitely. If delta == 0,
    //     poll without blocking. If delta > 0, block for up to delta nanoseconds.
    //     Return a list of goroutines built by calling netpollready.
    //
    // func netpollBreak()
    //     Wake up the network poller, assumed to be blocked in netpoll.
    //
    // func netpollIsPollDescriptor(fd uintptr) bool
    //     Reports whether fd is a file descriptor used by the poller.

    // Error codes returned by runtime_pollReset and runtime_pollWait.
    // These must match the values in internal/poll/fd_poll_runtime.go.
private static readonly nint pollNoError = 0; // no error
private static readonly nint pollErrClosing = 1; // descriptor is closed
private static readonly nint pollErrTimeout = 2; // I/O timeout
private static readonly nint pollErrNotPollable = 3; // general error polling descriptor

// pollDesc contains 2 binary semaphores, rg and wg, to park reader and writer
// goroutines respectively. The semaphore can be in the following states:
// pdReady - io readiness notification is pending;
//           a goroutine consumes the notification by changing the state to nil.
// pdWait - a goroutine prepares to park on the semaphore, but not yet parked;
//          the goroutine commits to park by changing the state to G pointer,
//          or, alternatively, concurrent io notification changes the state to pdReady,
//          or, alternatively, concurrent timeout/close changes the state to nil.
// G pointer - the goroutine is blocked on the semaphore;
//             io notification or timeout/close changes the state to pdReady or nil respectively
//             and unparks the goroutine.
// nil - none of the above.
private static readonly System.UIntPtr pdReady = 1;
private static readonly System.UIntPtr pdWait = 2;


private static readonly nint pollBlockSize = 4 * 1024;

// Network poller descriptor.
//
// No heap pointers.
//
//go:notinheap


// Network poller descriptor.
//
// No heap pointers.
//
//go:notinheap
private partial struct pollDesc {
    public ptr<pollDesc> link; // in pollcache, protected by pollcache.lock

// The lock protects pollOpen, pollSetDeadline, pollUnblock and deadlineimpl operations.
// This fully covers seq, rt and wt variables. fd is constant throughout the PollDesc lifetime.
// pollReset, pollWait, pollWaitCanceled and runtime·netpollready (IO readiness notification)
// proceed w/o taking the lock. So closing, everr, rg, rd, wg and wd are manipulated
// in a lock-free way by all operations.
// TODO(golang.org/issue/49008): audit these lock-free fields for continued correctness.
// NOTE(dvyukov): the following code uses uintptr to store *g (rg/wg),
// that will blow up when GC starts moving objects.
    public mutex @lock; // protects the following fields
    public System.UIntPtr fd;
    public bool closing;
    public bool everr; // marks event scanning error happened
    public uint user; // user settable cookie
    public System.UIntPtr rseq; // protects from stale read timers
    public System.UIntPtr rg; // pdReady, pdWait, G waiting for read or nil. Accessed atomically.
    public timer rt; // read deadline timer (set if rt.f != nil)
    public long rd; // read deadline
    public System.UIntPtr wseq; // protects from stale write timers
    public System.UIntPtr wg; // pdReady, pdWait, G waiting for write or nil. Accessed atomically.
    public timer wt; // write deadline timer
    public long wd; // write deadline
    public ptr<pollDesc> self; // storage for indirect interface. See (*pollDesc).makeArg.
}

private partial struct pollCache {
    public mutex @lock;
    public ptr<pollDesc> first; // PollDesc objects must be type-stable,
// because we can get ready notification from epoll/kqueue
// after the descriptor is closed/reused.
// Stale notifications are detected using seq variable,
// seq is incremented when deadlines are changed or descriptor is reused.
}

private static mutex netpollInitLock = default;private static uint netpollInited = default;private static pollCache pollcache = default;private static uint netpollWaiters = default;

//go:linkname poll_runtime_pollServerInit internal/poll.runtime_pollServerInit
private static void poll_runtime_pollServerInit() {
    netpollGenericInit();
}

private static void netpollGenericInit() {
    if (atomic.Load(_addr_netpollInited) == 0) {
        lockInit(_addr_netpollInitLock, lockRankNetpollInit);
        lock(_addr_netpollInitLock);
        if (netpollInited == 0) {
            netpollinit();
            atomic.Store(_addr_netpollInited, 1);
        }
        unlock(_addr_netpollInitLock);

    }
}

private static bool netpollinited() {
    return atomic.Load(_addr_netpollInited) != 0;
}

//go:linkname poll_runtime_isPollServerDescriptor internal/poll.runtime_isPollServerDescriptor

// poll_runtime_isPollServerDescriptor reports whether fd is a
// descriptor being used by netpoll.
private static bool poll_runtime_isPollServerDescriptor(System.UIntPtr fd) {
    return netpollIsPollDescriptor(fd);
}

//go:linkname poll_runtime_pollOpen internal/poll.runtime_pollOpen
private static (ptr<pollDesc>, nint) poll_runtime_pollOpen(System.UIntPtr fd) {
    ptr<pollDesc> _p0 = default!;
    nint _p0 = default;

    var pd = pollcache.alloc();
    lock(_addr_pd.@lock);
    var wg = atomic.Loaduintptr(_addr_pd.wg);
    if (wg != 0 && wg != pdReady) {
        throw("runtime: blocked write on free polldesc");
    }
    var rg = atomic.Loaduintptr(_addr_pd.rg);
    if (rg != 0 && rg != pdReady) {
        throw("runtime: blocked read on free polldesc");
    }
    pd.fd = fd;
    pd.closing = false;
    pd.everr = false;
    pd.rseq++;
    atomic.Storeuintptr(_addr_pd.rg, 0);
    pd.rd = 0;
    pd.wseq++;
    atomic.Storeuintptr(_addr_pd.wg, 0);
    pd.wd = 0;
    pd.self = pd;
    unlock(_addr_pd.@lock);

    var errno = netpollopen(fd, pd);
    if (errno != 0) {
        pollcache.free(pd);
        return (_addr_null!, int(errno));
    }
    return (_addr_pd!, 0);

}

//go:linkname poll_runtime_pollClose internal/poll.runtime_pollClose
private static void poll_runtime_pollClose(ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    if (!pd.closing) {
        throw("runtime: close polldesc w/o unblock");
    }
    var wg = atomic.Loaduintptr(_addr_pd.wg);
    if (wg != 0 && wg != pdReady) {
        throw("runtime: blocked write on closing polldesc");
    }
    var rg = atomic.Loaduintptr(_addr_pd.rg);
    if (rg != 0 && rg != pdReady) {
        throw("runtime: blocked read on closing polldesc");
    }
    netpollclose(pd.fd);
    pollcache.free(pd);

}

private static void free(this ptr<pollCache> _addr_c, ptr<pollDesc> _addr_pd) {
    ref pollCache c = ref _addr_c.val;
    ref pollDesc pd = ref _addr_pd.val;

    lock(_addr_c.@lock);
    pd.link = c.first;
    c.first = pd;
    unlock(_addr_c.@lock);
}

// poll_runtime_pollReset, which is internal/poll.runtime_pollReset,
// prepares a descriptor for polling in mode, which is 'r' or 'w'.
// This returns an error code; the codes are defined above.
//go:linkname poll_runtime_pollReset internal/poll.runtime_pollReset
private static nint poll_runtime_pollReset(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    var errcode = netpollcheckerr(_addr_pd, int32(mode));
    if (errcode != pollNoError) {
        return errcode;
    }
    if (mode == 'r') {
        atomic.Storeuintptr(_addr_pd.rg, 0);
    }
    else if (mode == 'w') {
        atomic.Storeuintptr(_addr_pd.wg, 0);
    }
    return pollNoError;

}

// poll_runtime_pollWait, which is internal/poll.runtime_pollWait,
// waits for a descriptor to be ready for reading or writing,
// according to mode, which is 'r' or 'w'.
// This returns an error code; the codes are defined above.
//go:linkname poll_runtime_pollWait internal/poll.runtime_pollWait
private static nint poll_runtime_pollWait(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    var errcode = netpollcheckerr(_addr_pd, int32(mode));
    if (errcode != pollNoError) {
        return errcode;
    }
    if (GOOS == "solaris" || GOOS == "illumos" || GOOS == "aix") {
        netpollarm(pd, mode);
    }
    while (!netpollblock(_addr_pd, int32(mode), false)) {
        errcode = netpollcheckerr(_addr_pd, int32(mode));
        if (errcode != pollNoError) {
            return errcode;
        }
    }
    return pollNoError;

}

//go:linkname poll_runtime_pollWaitCanceled internal/poll.runtime_pollWaitCanceled
private static void poll_runtime_pollWaitCanceled(ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;
 
    // This function is used only on windows after a failed attempt to cancel
    // a pending async IO operation. Wait for ioready, ignore closing or timeouts.
    while (!netpollblock(_addr_pd, int32(mode), true))     }

}

//go:linkname poll_runtime_pollSetDeadline internal/poll.runtime_pollSetDeadline
private static void poll_runtime_pollSetDeadline(ptr<pollDesc> _addr_pd, long d, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    lock(_addr_pd.@lock);
    if (pd.closing) {
        unlock(_addr_pd.@lock);
        return ;
    }
    var rd0 = pd.rd;
    var wd0 = pd.wd;
    var combo0 = rd0 > 0 && rd0 == wd0;
    if (d > 0) {
        d += nanotime();
        if (d <= 0) { 
            // If the user has a deadline in the future, but the delay calculation
            // overflows, then set the deadline to the maximum possible value.
            d = 1 << 63 - 1;

        }
    }
    if (mode == 'r' || mode == 'r' + 'w') {
        pd.rd = d;
    }
    if (mode == 'w' || mode == 'r' + 'w') {
        pd.wd = d;
    }
    var combo = pd.rd > 0 && pd.rd == pd.wd;
    var rtf = netpollReadDeadline;
    if (combo) {
        rtf = netpollDeadline;
    }
    if (pd.rt.f == null) {
        if (pd.rd > 0) {
            pd.rt.f = rtf; 
            // Copy current seq into the timer arg.
            // Timer func will check the seq against current descriptor seq,
            // if they differ the descriptor was reused or timers were reset.
            pd.rt.arg = pd.makeArg();
            pd.rt.seq = pd.rseq;
            resettimer(_addr_pd.rt, pd.rd);

        }
    }
    else if (pd.rd != rd0 || combo != combo0) {
        pd.rseq++; // invalidate current timers
        if (pd.rd > 0) {
            modtimer(_addr_pd.rt, pd.rd, 0, rtf, pd.makeArg(), pd.rseq);
        }
        else
 {
            deltimer(_addr_pd.rt);
            pd.rt.f = null;
        }
    }
    if (pd.wt.f == null) {
        if (pd.wd > 0 && !combo) {
            pd.wt.f = netpollWriteDeadline;
            pd.wt.arg = pd.makeArg();
            pd.wt.seq = pd.wseq;
            resettimer(_addr_pd.wt, pd.wd);
        }
    }
    else if (pd.wd != wd0 || combo != combo0) {
        pd.wseq++; // invalidate current timers
        if (pd.wd > 0 && !combo) {
            modtimer(_addr_pd.wt, pd.wd, 0, netpollWriteDeadline, pd.makeArg(), pd.wseq);
        }
        else
 {
            deltimer(_addr_pd.wt);
            pd.wt.f = null;
        }
    }
    ptr<g> rg;    ptr<g> wg;

    if (pd.rd < 0 || pd.wd < 0) {
        atomic.StorepNoWB(noescape(@unsafe.Pointer(_addr_wg)), null); // full memory barrier between stores to rd/wd and load of rg/wg in netpollunblock
        if (pd.rd < 0) {
            rg = netpollunblock(_addr_pd, 'r', false);
        }
        if (pd.wd < 0) {
            wg = netpollunblock(_addr_pd, 'w', false);
        }
    }
    unlock(_addr_pd.@lock);
    if (rg != null) {
        netpollgoready(rg, 3);
    }
    if (wg != null) {
        netpollgoready(wg, 3);
    }
}

//go:linkname poll_runtime_pollUnblock internal/poll.runtime_pollUnblock
private static void poll_runtime_pollUnblock(ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    lock(_addr_pd.@lock);
    if (pd.closing) {
        throw("runtime: unblock on closing polldesc");
    }
    pd.closing = true;
    pd.rseq++;
    pd.wseq++;
    ptr<g> rg;    ptr<g> wg;

    atomic.StorepNoWB(noescape(@unsafe.Pointer(_addr_rg)), null); // full memory barrier between store to closing and read of rg/wg in netpollunblock
    rg = netpollunblock(_addr_pd, 'r', false);
    wg = netpollunblock(_addr_pd, 'w', false);
    if (pd.rt.f != null) {
        deltimer(_addr_pd.rt);
        pd.rt.f = null;
    }
    if (pd.wt.f != null) {
        deltimer(_addr_pd.wt);
        pd.wt.f = null;
    }
    unlock(_addr_pd.@lock);
    if (rg != null) {
        netpollgoready(rg, 3);
    }
    if (wg != null) {
        netpollgoready(wg, 3);
    }
}

// netpollready is called by the platform-specific netpoll function.
// It declares that the fd associated with pd is ready for I/O.
// The toRun argument is used to build a list of goroutines to return
// from netpoll. The mode argument is 'r', 'w', or 'r'+'w' to indicate
// whether the fd is ready for reading or writing or both.
//
// This may run while the world is stopped, so write barriers are not allowed.
//go:nowritebarrier
private static void netpollready(ptr<gList> _addr_toRun, ptr<pollDesc> _addr_pd, int mode) {
    ref gList toRun = ref _addr_toRun.val;
    ref pollDesc pd = ref _addr_pd.val;

    ptr<g> rg;    ptr<g> wg;

    if (mode == 'r' || mode == 'r' + 'w') {
        rg = netpollunblock(_addr_pd, 'r', true);
    }
    if (mode == 'w' || mode == 'r' + 'w') {
        wg = netpollunblock(_addr_pd, 'w', true);
    }
    if (rg != null) {
        toRun.push(rg);
    }
    if (wg != null) {
        toRun.push(wg);
    }
}

private static nint netpollcheckerr(ptr<pollDesc> _addr_pd, int mode) {
    ref pollDesc pd = ref _addr_pd.val;

    if (pd.closing) {
        return pollErrClosing;
    }
    if ((mode == 'r' && pd.rd < 0) || (mode == 'w' && pd.wd < 0)) {
        return pollErrTimeout;
    }
    if (mode == 'r' && pd.everr) {
        return pollErrNotPollable;
    }
    return pollNoError;

}

private static bool netpollblockcommit(ptr<g> _addr_gp, unsafe.Pointer gpp) {
    ref g gp = ref _addr_gp.val;

    var r = atomic.Casuintptr((uintptr.val)(gpp), pdWait, uintptr(@unsafe.Pointer(gp)));
    if (r) { 
        // Bump the count of goroutines waiting for the poller.
        // The scheduler uses this to decide whether to block
        // waiting for the poller if there is nothing else to do.
        atomic.Xadd(_addr_netpollWaiters, 1);

    }
    return r;

}

private static void netpollgoready(ptr<g> _addr_gp, nint traceskip) {
    ref g gp = ref _addr_gp.val;

    atomic.Xadd(_addr_netpollWaiters, -1);
    goready(gp, traceskip + 1);
}

// returns true if IO is ready, or false if timedout or closed
// waitio - wait only for completed IO, ignore errors
// Concurrent calls to netpollblock in the same mode are forbidden, as pollDesc
// can hold only a single waiting goroutine for each mode.
private static bool netpollblock(ptr<pollDesc> _addr_pd, int mode, bool waitio) {
    ref pollDesc pd = ref _addr_pd.val;

    var gpp = _addr_pd.rg;
    if (mode == 'w') {
        gpp = _addr_pd.wg;
    }
    while (true) { 
        // Consume notification if already ready.
        if (atomic.Casuintptr(gpp, pdReady, 0)) {
            return true;
        }
        if (atomic.Casuintptr(gpp, 0, pdWait)) {
            break;
        }
        {
            var v = atomic.Loaduintptr(gpp);

            if (v != pdReady && v != 0) {
                throw("runtime: double wait");
            }

        }

    } 

    // need to recheck error states after setting gpp to pdWait
    // this is necessary because runtime_pollUnblock/runtime_pollSetDeadline/deadlineimpl
    // do the opposite: store to closing/rd/wd, membarrier, load of rg/wg
    if (waitio || netpollcheckerr(_addr_pd, mode) == 0) {
        gopark(netpollblockcommit, @unsafe.Pointer(gpp), waitReasonIOWait, traceEvGoBlockNet, 5);
    }
    var old = atomic.Xchguintptr(gpp, 0);
    if (old > pdWait) {
        throw("runtime: corrupted polldesc");
    }
    return old == pdReady;

}

private static ptr<g> netpollunblock(ptr<pollDesc> _addr_pd, int mode, bool ioready) {
    ref pollDesc pd = ref _addr_pd.val;

    var gpp = _addr_pd.rg;
    if (mode == 'w') {
        gpp = _addr_pd.wg;
    }
    while (true) {
        var old = atomic.Loaduintptr(gpp);
        if (old == pdReady) {
            return _addr_null!;
        }
        if (old == 0 && !ioready) { 
            // Only set pdReady for ioready. runtime_pollWait
            // will check for timeout/cancel before waiting.
            return _addr_null!;

        }
        System.UIntPtr @new = default;
        if (ioready) {
            new = pdReady;
        }
        if (atomic.Casuintptr(gpp, old, new)) {
            if (old == pdWait) {
                old = 0;
            }
            return _addr_(g.val)(@unsafe.Pointer(old))!;
        }
    }

}

private static void netpolldeadlineimpl(ptr<pollDesc> _addr_pd, System.UIntPtr seq, bool read, bool write) {
    ref pollDesc pd = ref _addr_pd.val;

    lock(_addr_pd.@lock); 
    // Seq arg is seq when the timer was set.
    // If it's stale, ignore the timer event.
    var currentSeq = pd.rseq;
    if (!read) {
        currentSeq = pd.wseq;
    }
    if (seq != currentSeq) { 
        // The descriptor was reused or timers were reset.
        unlock(_addr_pd.@lock);
        return ;

    }
    ptr<g> rg;
    if (read) {
        if (pd.rd <= 0 || pd.rt.f == null) {
            throw("runtime: inconsistent read deadline");
        }
        pd.rd = -1;
        atomic.StorepNoWB(@unsafe.Pointer(_addr_pd.rt.f), null); // full memory barrier between store to rd and load of rg in netpollunblock
        rg = netpollunblock(_addr_pd, 'r', false);

    }
    ptr<g> wg;
    if (write) {
        if (pd.wd <= 0 || pd.wt.f == null && !read) {
            throw("runtime: inconsistent write deadline");
        }
        pd.wd = -1;
        atomic.StorepNoWB(@unsafe.Pointer(_addr_pd.wt.f), null); // full memory barrier between store to wd and load of wg in netpollunblock
        wg = netpollunblock(_addr_pd, 'w', false);

    }
    unlock(_addr_pd.@lock);
    if (rg != null) {
        netpollgoready(rg, 0);
    }
    if (wg != null) {
        netpollgoready(wg, 0);
    }
}

private static void netpollDeadline(object arg, System.UIntPtr seq) {
    netpolldeadlineimpl(arg._<ptr<pollDesc>>(), seq, true, true);
}

private static void netpollReadDeadline(object arg, System.UIntPtr seq) {
    netpolldeadlineimpl(arg._<ptr<pollDesc>>(), seq, true, false);
}

private static void netpollWriteDeadline(object arg, System.UIntPtr seq) {
    netpolldeadlineimpl(arg._<ptr<pollDesc>>(), seq, false, true);
}

private static ptr<pollDesc> alloc(this ptr<pollCache> _addr_c) {
    ref pollCache c = ref _addr_c.val;

    lock(_addr_c.@lock);
    if (c.first == null) {
        const var pdSize = @unsafe.Sizeof(new pollDesc());

        var n = pollBlockSize / pdSize;
        if (n == 0) {
            n = 1;
        }
        var mem = persistentalloc(n * pdSize, 0, _addr_memstats.other_sys);
        for (var i = uintptr(0); i < n; i++) {
            var pd = (pollDesc.val)(add(mem, i * pdSize));
            pd.link = c.first;
            c.first = pd;
        }

    }
    pd = c.first;
    c.first = pd.link;
    lockInit(_addr_pd.@lock, lockRankPollDesc);
    unlock(_addr_c.@lock);
    return _addr_pd!;

}

// makeArg converts pd to an interface{}.
// makeArg does not do any allocation. Normally, such
// a conversion requires an allocation because pointers to
// go:notinheap types (which pollDesc is) must be stored
// in interfaces indirectly. See issue 42076.
private static object makeArg(this ptr<pollDesc> _addr_pd) {
    object i = default;
    ref pollDesc pd = ref _addr_pd.val;

    var x = (eface.val)(@unsafe.Pointer(_addr_i));
    x._type = pdType;
    x.data = @unsafe.Pointer(_addr_pd.self);
    return ;
}

private static var pdEface = (pollDesc.val)(null);private static ptr<_type> pdTypeefaceOf(_addr_pdEface)._type;

} // end runtime_package
