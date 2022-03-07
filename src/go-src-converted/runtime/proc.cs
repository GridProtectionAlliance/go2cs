// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:11 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\proc.go
using abi = go.@internal.abi_package;
using cpu = go.@internal.cpu_package;
using goexperiment = go.@internal.goexperiment_package;
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class runtime_package {

    // set using cmd/go/internal/modload.ModInfoProg
private static @string modinfo = default;

// Goroutine scheduler
// The scheduler's job is to distribute ready-to-run goroutines over worker threads.
//
// The main concepts are:
// G - goroutine.
// M - worker thread, or machine.
// P - processor, a resource that is required to execute Go code.
//     M must have an associated P to execute Go code, however it can be
//     blocked or in a syscall w/o an associated P.
//
// Design doc at https://golang.org/s/go11sched.

// Worker thread parking/unparking.
// We need to balance between keeping enough running worker threads to utilize
// available hardware parallelism and parking excessive running worker threads
// to conserve CPU resources and power. This is not simple for two reasons:
// (1) scheduler state is intentionally distributed (in particular, per-P work
// queues), so it is not possible to compute global predicates on fast paths;
// (2) for optimal thread management we would need to know the future (don't park
// a worker thread when a new goroutine will be readied in near future).
//
// Three rejected approaches that would work badly:
// 1. Centralize all scheduler state (would inhibit scalability).
// 2. Direct goroutine handoff. That is, when we ready a new goroutine and there
//    is a spare P, unpark a thread and handoff it the thread and the goroutine.
//    This would lead to thread state thrashing, as the thread that readied the
//    goroutine can be out of work the very next moment, we will need to park it.
//    Also, it would destroy locality of computation as we want to preserve
//    dependent goroutines on the same thread; and introduce additional latency.
// 3. Unpark an additional thread whenever we ready a goroutine and there is an
//    idle P, but don't do handoff. This would lead to excessive thread parking/
//    unparking as the additional threads will instantly park without discovering
//    any work to do.
//
// The current approach:
//
// This approach applies to three primary sources of potential work: readying a
// goroutine, new/modified-earlier timers, and idle-priority GC. See below for
// additional details.
//
// We unpark an additional thread when we submit work if (this is wakep()):
// 1. There is an idle P, and
// 2. There are no "spinning" worker threads.
//
// A worker thread is considered spinning if it is out of local work and did
// not find work in the global run queue or netpoller; the spinning state is
// denoted in m.spinning and in sched.nmspinning. Threads unparked this way are
// also considered spinning; we don't do goroutine handoff so such threads are
// out of work initially. Spinning threads spin on looking for work in per-P
// run queues and timer heaps or from the GC before parking. If a spinning
// thread finds work it takes itself out of the spinning state and proceeds to
// execution. If it does not find work it takes itself out of the spinning
// state and then parks.
//
// If there is at least one spinning thread (sched.nmspinning>1), we don't
// unpark new threads when submitting work. To compensate for that, if the last
// spinning thread finds work and stops spinning, it must unpark a new spinning
// thread.  This approach smooths out unjustified spikes of thread unparking,
// but at the same time guarantees eventual maximal CPU parallelism
// utilization.
//
// The main implementation complication is that we need to be very careful
// during spinning->non-spinning thread transition. This transition can race
// with submission of new work, and either one part or another needs to unpark
// another worker thread. If they both fail to do that, we can end up with
// semi-persistent CPU underutilization.
//
// The general pattern for submission is:
// 1. Submit work to the local run queue, timer heap, or GC state.
// 2. #StoreLoad-style memory barrier.
// 3. Check sched.nmspinning.
//
// The general pattern for spinning->non-spinning transition is:
// 1. Decrement nmspinning.
// 2. #StoreLoad-style memory barrier.
// 3. Check all per-P work queues and GC for new work.
//
// Note that all this complexity does not apply to global run queue as we are
// not sloppy about thread unparking when submitting to global queue. Also see
// comments for nmspinning manipulation.
//
// How these different sources of work behave varies, though it doesn't affect
// the synchronization approach:
// * Ready goroutine: this is an obvious source of work; the goroutine is
//   immediately ready and must run on some thread eventually.
// * New/modified-earlier timer: The current timer implementation (see time.go)
//   uses netpoll in a thread with no work available to wait for the soonest
//   timer. If there is no thread waiting, we want a new spinning thread to go
//   wait.
// * Idle-priority GC: The GC wakes a stopped idle thread to contribute to
//   background GC work (note: currently disabled per golang.org/issue/19112).
//   Also see golang.org/issue/44313, as this should be extended to all GC
//   workers.

private static m m0 = default;private static g g0 = default;private static ptr<mcache> mcache0;private static System.UIntPtr raceprocctx0 = default;

//go:linkname runtime_inittask runtime..inittask
private static initTask runtime_inittask = default;

//go:linkname main_inittask main..inittask
private static initTask main_inittask = default;

// main_init_done is a signal used by cgocallbackg that initialization
// has been completed. It is made before _cgo_notify_runtime_init_done,
// so all cgo calls can rely on it existing. When main_init is complete,
// it is closed, meaning cgocallbackg can reliably receive from it.
private static channel<bool> main_init_done = default;

//go:linkname main_main main.main
private static void main_main();

// mainStarted indicates that the main M has started.
private static bool mainStarted = default;

// runtimeInitTime is the nanotime() at which the runtime started.
private static long runtimeInitTime = default;

// Value to use for signal mask for newly created M's.
private static sigset initSigmask = default;

// The main goroutine.
private static void Main() => func((defer, _, _) => {
    var g = getg(); 

    // Racectx of m0->g0 is used only as the parent of the main goroutine.
    // It must not be used for anything else.
    g.m.g0.racectx = 0; 

    // Max stack size is 1 GB on 64-bit, 250 MB on 32-bit.
    // Using decimal instead of binary GB and MB because
    // they look nicer in the stack overflow failure message.
    if (sys.PtrSize == 8) {>>MARKER:FUNCTION_main_main_BLOCK_PREFIX<<
        maxstacksize = 1000000000;
    }
    else
 {
        maxstacksize = 250000000;
    }
    maxstackceiling = 2 * maxstacksize; 

    // Allow newproc to start new Ms.
    mainStarted = true;

    if (GOARCH != "wasm") { // no threads on wasm yet, so no sysmon
        // For runtime_syscall_doAllThreadsSyscall, we
        // register sysmon is not ready for the world to be
        // stopped.
        atomic.Store(_addr_sched.sysmonStarting, 1);
        systemstack(() => {
            newm(sysmon, _addr_null, -1);
        });

    }
    lockOSThread();

    if (g.m != _addr_m0) {
        throw("runtime.main not on m0");
    }
    m0.doesPark = true; 

    // Record when the world started.
    // Must be before doInit for tracing init.
    runtimeInitTime = nanotime();
    if (runtimeInitTime == 0) {
        throw("nanotime returning zero");
    }
    if (debug.inittrace != 0) {
        inittrace.id = getg().goid;
        inittrace.active = true;
    }
    doInit(_addr_runtime_inittask); // Must be before defer.

    // Defer unlock so that runtime.Goexit during init does the unlock too.
    var needUnlock = true;
    defer(() => {
        if (needUnlock) {
            unlockOSThread();
        }
    }());

    gcenable();

    main_init_done = make_channel<bool>();
    if (iscgo) {
        if (_cgo_thread_start == null) {
            throw("_cgo_thread_start missing");
        }
        if (GOOS != "windows") {
            if (_cgo_setenv == null) {
                throw("_cgo_setenv missing");
            }
            if (_cgo_unsetenv == null) {
                throw("_cgo_unsetenv missing");
            }
        }
        if (_cgo_notify_runtime_init_done == null) {
            throw("_cgo_notify_runtime_init_done missing");
        }
        startTemplateThread();
        cgocall(_cgo_notify_runtime_init_done, null);

    }
    doInit(_addr_main_inittask); 

    // Disable init tracing after main init done to avoid overhead
    // of collecting statistics in malloc and newproc
    inittrace.active = false;

    close(main_init_done);

    needUnlock = false;
    unlockOSThread();

    if (isarchive || islibrary) { 
        // A program compiled with -buildmode=c-archive or c-shared
        // has a main, but it is not executed.
        return ;

    }
    var fn = main_main; // make an indirect call, as the linker doesn't know the address of the main package when laying down the runtime
    fn();
    if (raceenabled) {
        racefini();
    }
    if (atomic.Load(_addr_runningPanicDefers) != 0) { 
        // Running deferred functions should not take long.
        for (nint c = 0; c < 1000; c++) {
            if (atomic.Load(_addr_runningPanicDefers) == 0) {
                break;
            }
            Gosched();
        }

    }
    if (atomic.Load(_addr_panicking) != 0) {
        gopark(null, null, waitReasonPanicWait, traceEvGoStop, 1);
    }
    exit(0);
    while (true) {
        ptr<int> x;
        x.val = 0;
    }

});

// os_beforeExit is called from os.Exit(0).
//go:linkname os_beforeExit os.runtime_beforeExit
private static void os_beforeExit() {
    if (raceenabled) {
        racefini();
    }
}

// start forcegc helper goroutine
private static void init() {
    go_(() => forcegchelper());
}

private static void forcegchelper() {
    forcegc.g = getg();
    lockInit(_addr_forcegc.@lock, lockRankForcegc);
    while (true) {
        lock(_addr_forcegc.@lock);
        if (forcegc.idle != 0) {
            throw("forcegc: phase error");
        }
        atomic.Store(_addr_forcegc.idle, 1);
        goparkunlock(_addr_forcegc.@lock, waitReasonForceGCIdle, traceEvGoBlock, 1); 
        // this goroutine is explicitly resumed by sysmon
        if (debug.gctrace > 0) {
            println("GC forced");
        }
        gcStart(new gcTrigger(kind:gcTriggerTime,now:nanotime()));

    }

}

//go:nosplit

// Gosched yields the processor, allowing other goroutines to run. It does not
// suspend the current goroutine, so execution resumes automatically.
public static void Gosched() {
    checkTimeouts();
    mcall(gosched_m);
}

// goschedguarded yields the processor like gosched, but also checks
// for forbidden states and opts out of the yield in those cases.
//go:nosplit
private static void goschedguarded() {
    mcall(goschedguarded_m);
}

// Puts the current goroutine into a waiting state and calls unlockf on the
// system stack.
//
// If unlockf returns false, the goroutine is resumed.
//
// unlockf must not access this G's stack, as it may be moved between
// the call to gopark and the call to unlockf.
//
// Note that because unlockf is called after putting the G into a waiting
// state, the G may have already been readied by the time unlockf is called
// unless there is external synchronization preventing the G from being
// readied. If unlockf returns false, it must guarantee that the G cannot be
// externally readied.
//
// Reason explains why the goroutine has been parked. It is displayed in stack
// traces and heap dumps. Reasons should be unique and descriptive. Do not
// re-use reasons, add new ones.
private static bool gopark(Func<ptr<g>, unsafe.Pointer, bool> unlockf, unsafe.Pointer @lock, waitReason reason, byte traceEv, nint traceskip) {
    if (reason != waitReasonSleep) {
        checkTimeouts(); // timeouts may expire while two goroutines keep the scheduler busy
    }
    var mp = acquirem();
    var gp = mp.curg;
    var status = readgstatus(_addr_gp);
    if (status != _Grunning && status != _Gscanrunning) {
        throw("gopark: bad g status");
    }
    mp.waitlock = lock;
    mp.waitunlockf = unlockf;
    gp.waitreason = reason;
    mp.waittraceev = traceEv;
    mp.waittraceskip = traceskip;
    releasem(mp); 
    // can't do anything that might move the G between Ms here.
    mcall(park_m);

}

// Puts the current goroutine into a waiting state and unlocks the lock.
// The goroutine can be made runnable again by calling goready(gp).
private static void goparkunlock(ptr<mutex> _addr_@lock, waitReason reason, byte traceEv, nint traceskip) {
    ref mutex @lock = ref _addr_@lock.val;

    gopark(parkunlock_c, @unsafe.Pointer(lock), reason, traceEv, traceskip);
}

private static void goready(ptr<g> _addr_gp, nint traceskip) {
    ref g gp = ref _addr_gp.val;

    systemstack(() => {
        ready(_addr_gp, traceskip, true);
    });
}

//go:nosplit
private static ptr<sudog> acquireSudog() { 
    // Delicate dance: the semaphore implementation calls
    // acquireSudog, acquireSudog calls new(sudog),
    // new calls malloc, malloc can call the garbage collector,
    // and the garbage collector calls the semaphore implementation
    // in stopTheWorld.
    // Break the cycle by doing acquirem/releasem around new(sudog).
    // The acquirem/releasem increments m.locks during new(sudog),
    // which keeps the garbage collector from being invoked.
    var mp = acquirem();
    var pp = mp.p.ptr();
    if (len(pp.sudogcache) == 0) {
        lock(_addr_sched.sudoglock); 
        // First, try to grab a batch from central cache.
        while (len(pp.sudogcache) < cap(pp.sudogcache) / 2 && sched.sudogcache != null) {
            var s = sched.sudogcache;
            sched.sudogcache = s.next;
            s.next = null;
            pp.sudogcache = append(pp.sudogcache, s);
        }
        unlock(_addr_sched.sudoglock); 
        // If the central cache is empty, allocate a new one.
        if (len(pp.sudogcache) == 0) {
            pp.sudogcache = append(pp.sudogcache, @new<sudog>());
        }
    }
    var n = len(pp.sudogcache);
    s = pp.sudogcache[n - 1];
    pp.sudogcache[n - 1] = null;
    pp.sudogcache = pp.sudogcache[..(int)n - 1];
    if (s.elem != null) {
        throw("acquireSudog: found s.elem != nil in cache");
    }
    releasem(mp);
    return _addr_s!;

}

//go:nosplit
private static void releaseSudog(ptr<sudog> _addr_s) {
    ref sudog s = ref _addr_s.val;

    if (s.elem != null) {
        throw("runtime: sudog with non-nil elem");
    }
    if (s.isSelect) {
        throw("runtime: sudog with non-false isSelect");
    }
    if (s.next != null) {
        throw("runtime: sudog with non-nil next");
    }
    if (s.prev != null) {
        throw("runtime: sudog with non-nil prev");
    }
    if (s.waitlink != null) {
        throw("runtime: sudog with non-nil waitlink");
    }
    if (s.c != null) {
        throw("runtime: sudog with non-nil c");
    }
    var gp = getg();
    if (gp.param != null) {
        throw("runtime: releaseSudog with non-nil gp.param");
    }
    var mp = acquirem(); // avoid rescheduling to another P
    var pp = mp.p.ptr();
    if (len(pp.sudogcache) == cap(pp.sudogcache)) { 
        // Transfer half of local cache to the central cache.
        ptr<sudog> first;        ptr<sudog> last;

        while (len(pp.sudogcache) > cap(pp.sudogcache) / 2) {
            var n = len(pp.sudogcache);
            var p = pp.sudogcache[n - 1];
            pp.sudogcache[n - 1] = null;
            pp.sudogcache = pp.sudogcache[..(int)n - 1];
            if (first == null) {
                first = p;
            }
            else
 {
                last.next = p;
            }

            last = p;

        }
        lock(_addr_sched.sudoglock);
        last.next = sched.sudogcache;
        sched.sudogcache = first;
        unlock(_addr_sched.sudoglock);

    }
    pp.sudogcache = append(pp.sudogcache, s);
    releasem(mp);

}

// funcPC returns the entry PC of the function f.
// It assumes that f is a func value. Otherwise the behavior is undefined.
// CAREFUL: In programs with plugins, funcPC can return different values
// for the same function (because there are actually multiple copies of
// the same function in the address space). To be safe, don't use the
// results of this function in any == expression. It is only safe to
// use the result as an address at which to start executing code.
//go:nosplit
private static System.UIntPtr funcPC(object f) {
    return new ptr<ptr<ptr<System.UIntPtr>>>(efaceOf(_addr_f).data);
}

// called from assembly
private static void badmcall(Action<ptr<g>> fn) {
    throw("runtime: mcall called on m->g0 stack");
}

private static void badmcall2(Action<ptr<g>> fn) {
    throw("runtime: mcall function returned");
}

private static void badreflectcall() => func((_, panic, _) => {
    panic(plainError("arg size to reflect.call more than 1GB"));
});

private static @string badmorestackg0Msg = "fatal: morestack on g0\n";

//go:nosplit
//go:nowritebarrierrec
private static void badmorestackg0() {
    var sp = stringStructOf(_addr_badmorestackg0Msg);
    write(2, sp.str, int32(sp.len));
}

private static @string badmorestackgsignalMsg = "fatal: morestack on gsignal\n";

//go:nosplit
//go:nowritebarrierrec
private static void badmorestackgsignal() {
    var sp = stringStructOf(_addr_badmorestackgsignalMsg);
    write(2, sp.str, int32(sp.len));
}

//go:nosplit
private static void badctxt() {
    throw("ctxt != 0");
}

private static bool lockedOSThread() {
    var gp = getg();
    return gp.lockedm != 0 && gp.m.lockedg != 0;
}

 
// allgs contains all Gs ever created (including dead Gs), and thus
// never shrinks.
//
// Access via the slice is protected by allglock or stop-the-world.
// Readers that cannot take the lock may (carefully!) use the atomic
// variables below.
private static mutex allglock = default;private static slice<ptr<g>> allgs = default;private static System.UIntPtr allglen = default;private static ptr<ptr<g>> allgptr;

private static void allgadd(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (readgstatus(_addr_gp) == _Gidle) {
        throw("allgadd: bad status Gidle");
    }
    lock(_addr_allglock);
    allgs = append(allgs, gp);
    if (_addr_allgs[0] != allgptr) {
        atomicstorep(@unsafe.Pointer(_addr_allgptr), @unsafe.Pointer(_addr_allgs[0]));
    }
    atomic.Storeuintptr(_addr_allglen, uintptr(len(allgs)));
    unlock(_addr_allglock);

}

// atomicAllG returns &allgs[0] and len(allgs) for use with atomicAllGIndex.
private static (ptr<ptr<g>>, System.UIntPtr) atomicAllG() {
    ptr<ptr<g>> _p0 = default!;
    System.UIntPtr _p0 = default;

    var length = atomic.Loaduintptr(_addr_allglen);
    var ptr = (g.val)(atomic.Loadp(@unsafe.Pointer(_addr_allgptr)));
    return (_addr_ptr!, length);
}

// atomicAllGIndex returns ptr[i] with the allgptr returned from atomicAllG.
private static ptr<g> atomicAllGIndex(ptr<ptr<g>> _addr_ptr, System.UIntPtr i) {
    ref ptr<g> ptr = ref _addr_ptr.val;

    return new ptr<ptr<ptr<ptr<g>>>>(add(@unsafe.Pointer(ptr), i * sys.PtrSize));
}

// forEachG calls fn on every G from allgs.
//
// forEachG takes a lock to exclude concurrent addition of new Gs.
private static void forEachG(Action<ptr<g>> fn) {
    lock(_addr_allglock);
    foreach (var (_, gp) in allgs) {
        fn(gp);
    }    unlock(_addr_allglock);
}

// forEachGRace calls fn on every G from allgs.
//
// forEachGRace avoids locking, but does not exclude addition of new Gs during
// execution, which may be missed.
private static void forEachGRace(Action<ptr<g>> fn) {
    var (ptr, length) = atomicAllG();
    for (var i = uintptr(0); i < length; i++) {
        var gp = atomicAllGIndex(_addr_ptr, i);
        fn(gp);
    }
    return ;
}

 
// Number of goroutine ids to grab from sched.goidgen to local per-P cache at once.
// 16 seems to provide enough amortization, but other than that it's mostly arbitrary number.
private static readonly nint _GoidCacheBatch = 16;


// cpuinit extracts the environment variable GODEBUG from the environment on
// Unix-like operating systems and calls internal/cpu.Initialize.
private static void cpuinit() {
    const @string prefix = "GODEBUG=";

    @string env = default;

    switch (GOOS) {
        case "aix": 

        case "darwin": 

        case "ios": 

        case "dragonfly": 

        case "freebsd": 

        case "netbsd": 

        case "openbsd": 

        case "illumos": 

        case "solaris": 

        case "linux": 
            cpu.DebugOptions = true; 

            // Similar to goenv_unix but extracts the environment value for
            // GODEBUG directly.
            // TODO(moehrmann): remove when general goenvs() can be called before cpuinit()
            var n = int32(0);
            while (argv_index(argv, argc + 1 + n) != null) {
                n++;
            }

            for (var i = int32(0); i < n; i++) {
                var p = argv_index(argv, argc + 1 + i);
                ptr<ptr<@string>> s = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(addr(new stringStruct(unsafe.Pointer(p),findnull(p)))));

                if (hasPrefix(s, prefix)) {
                    env = gostring(p)[(int)len(prefix)..];
                    break;
                }
            }

            break;
    }

    cpu.Initialize(env); 

    // Support cpu feature variables are used in code generated by the compiler
    // to guard execution of instructions that can not be assumed to be always supported.
    x86HasPOPCNT = cpu.X86.HasPOPCNT;
    x86HasSSE41 = cpu.X86.HasSSE41;
    x86HasFMA = cpu.X86.HasFMA;

    armHasVFPv4 = cpu.ARM.HasVFPv4;

    arm64HasATOMICS = cpu.ARM64.HasATOMICS;

}

// The bootstrap sequence is:
//
//    call osinit
//    call schedinit
//    make & queue new G
//    call runtime·mstart
//
// The new G calls runtime·main.
private static void schedinit() {
    lockInit(_addr_sched.@lock, lockRankSched);
    lockInit(_addr_sched.sysmonlock, lockRankSysmon);
    lockInit(_addr_sched.deferlock, lockRankDefer);
    lockInit(_addr_sched.sudoglock, lockRankSudog);
    lockInit(_addr_deadlock, lockRankDeadlock);
    lockInit(_addr_paniclk, lockRankPanic);
    lockInit(_addr_allglock, lockRankAllg);
    lockInit(_addr_allpLock, lockRankAllp);
    lockInit(_addr_reflectOffs.@lock, lockRankReflectOffs);
    lockInit(_addr_finlock, lockRankFin);
    lockInit(_addr_trace.bufLock, lockRankTraceBuf);
    lockInit(_addr_trace.stringsLock, lockRankTraceStrings);
    lockInit(_addr_trace.@lock, lockRankTrace);
    lockInit(_addr_cpuprof.@lock, lockRankCpuprof);
    lockInit(_addr_trace.stackTab.@lock, lockRankTraceStackTab); 
    // Enforce that this lock is always a leaf lock.
    // All of this lock's critical sections should be
    // extremely short.
    lockInit(_addr_memstats.heapStats.noPLock, lockRankLeafRank); 

    // raceinit must be the first call to race detector.
    // In particular, it must be done before mallocinit below calls racemapshadow.
    var _g_ = getg();
    if (raceenabled) {
        _g_.racectx, raceprocctx0 = raceinit();
    }
    sched.maxmcount = 10000; 

    // The world starts stopped.
    worldStopped();

    moduledataverify();
    stackinit();
    mallocinit();
    fastrandinit(); // must run before mcommoninit
    mcommoninit(_addr__g_.m, -1);
    cpuinit(); // must run before alginit
    alginit(); // maps must not be used before this call
    modulesinit(); // provides activeModules
    typelinksinit(); // uses maps, activeModules
    itabsinit(); // uses activeModules

    sigsave(_addr__g_.m.sigmask);
    initSigmask = _g_.m.sigmask;

    {
        var offset = @unsafe.Offsetof(sched.timeToRun);

        if (offset % 8 != 0) {
            println(offset);
            throw("sched.timeToRun not aligned to 8 bytes");
        }
    }


    goargs();
    goenvs();
    parsedebugvars();
    gcinit();

    lock(_addr_sched.@lock);
    sched.lastpoll = uint64(nanotime());
    var procs = ncpu;
    {
        var (n, ok) = atoi32(gogetenv("GOMAXPROCS"));

        if (ok && n > 0) {
            procs = n;
        }
    }

    if (procresize(procs) != null) {
        throw("unknown runnable goroutine during bootstrap");
    }
    unlock(_addr_sched.@lock); 

    // World is effectively started now, as P's can run.
    worldStarted(); 

    // For cgocheck > 1, we turn on the write barrier at all times
    // and check all pointer writes. We can't do this until after
    // procresize because the write barrier needs a P.
    if (debug.cgocheck > 1) {
        writeBarrier.cgo = true;
        writeBarrier.enabled = true;
        foreach (var (_, p) in allp) {
            p.wbBuf.reset();
        }
    }
    if (buildVersion == "") { 
        // Condition should never trigger. This code just serves
        // to ensure runtime·buildVersion is kept in the resulting binary.
        buildVersion = "unknown";

    }
    if (len(modinfo) == 1) { 
        // Condition should never trigger. This code just serves
        // to ensure runtime·modinfo is kept in the resulting binary.
        modinfo = "";

    }
}

private static void dumpgstatus(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    var _g_ = getg();
    print("runtime: gp: gp=", gp, ", goid=", gp.goid, ", gp->atomicstatus=", readgstatus(_addr_gp), "\n");
    print("runtime:  g:  g=", _g_, ", goid=", _g_.goid, ",  g->atomicstatus=", readgstatus(_addr__g_), "\n");
}

// sched.lock must be held.
private static void checkmcount() {
    assertLockHeld(_addr_sched.@lock);

    if (mcount() > sched.maxmcount) {
        print("runtime: program exceeds ", sched.maxmcount, "-thread limit\n");
        throw("thread exhaustion");
    }
}

// mReserveID returns the next ID to use for a new m. This new m is immediately
// considered 'running' by checkdead.
//
// sched.lock must be held.
private static long mReserveID() {
    assertLockHeld(_addr_sched.@lock);

    if (sched.mnext + 1 < sched.mnext) {
        throw("runtime: thread ID overflow");
    }
    var id = sched.mnext;
    sched.mnext++;
    checkmcount();
    return id;

}

// Pre-allocated ID may be passed as 'id', or omitted by passing -1.
private static void mcommoninit(ptr<m> _addr_mp, long id) {
    ref m mp = ref _addr_mp.val;

    var _g_ = getg(); 

    // g0 stack won't make sense for user (and is not necessary unwindable).
    if (_g_ != _g_.m.g0) {
        callers(1, mp.createstack[..]);
    }
    lock(_addr_sched.@lock);

    if (id >= 0) {
        mp.id = id;
    }
    else
 {
        mp.id = mReserveID();
    }
    mp.fastrand[0] = uint32(int64Hash(uint64(mp.id), fastrandseed));
    mp.fastrand[1] = uint32(int64Hash(uint64(cputicks()), ~fastrandseed));
    if (mp.fastrand[0] | mp.fastrand[1] == 0) {
        mp.fastrand[1] = 1;
    }
    mpreinit(mp);
    if (mp.gsignal != null) {
        mp.gsignal.stackguard1 = mp.gsignal.stack.lo + _StackGuard;
    }
    mp.alllink = allm; 

    // NumCgoCall() iterates over allm w/o schedlock,
    // so we need to publish it safely.
    atomicstorep(@unsafe.Pointer(_addr_allm), @unsafe.Pointer(mp));
    unlock(_addr_sched.@lock); 

    // Allocate memory to hold a cgo traceback if the cgo call crashes.
    if (iscgo || GOOS == "solaris" || GOOS == "illumos" || GOOS == "windows") {
        mp.cgoCallers = @new<cgoCallers>();
    }
}

private static System.UIntPtr fastrandseed = default;

private static void fastrandinit() {
    ptr<array<byte>> s = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_fastrandseed))[..];
    getRandomData(s);
}

// Mark gp ready to run.
private static void ready(ptr<g> _addr_gp, nint traceskip, bool next) {
    ref g gp = ref _addr_gp.val;

    if (trace.enabled) {
        traceGoUnpark(gp, traceskip);
    }
    var status = readgstatus(_addr_gp); 

    // Mark runnable.
    var _g_ = getg();
    var mp = acquirem(); // disable preemption because it can be holding p in a local var
    if (status & ~_Gscan != _Gwaiting) {
        dumpgstatus(_addr_gp);
        throw("bad g->status in ready");
    }
    casgstatus(_addr_gp, _Gwaiting, _Grunnable);
    runqput(_addr__g_.m.p.ptr(), _addr_gp, next);
    wakep();
    releasem(mp);

}

// freezeStopWait is a large value that freezetheworld sets
// sched.stopwait to in order to request that all Gs permanently stop.
private static readonly nuint freezeStopWait = 0x7fffffff;

// freezing is set to non-zero if the runtime is trying to freeze the
// world.


// freezing is set to non-zero if the runtime is trying to freeze the
// world.
private static uint freezing = default;

// Similar to stopTheWorld but best-effort and can be called several times.
// There is no reverse operation, used during crashing.
// This function must not lock any mutexes.
private static void freezetheworld() {
    atomic.Store(_addr_freezing, 1); 
    // stopwait and preemption requests can be lost
    // due to races with concurrently executing threads,
    // so try several times
    for (nint i = 0; i < 5; i++) { 
        // this should tell the scheduler to not start any new goroutines
        sched.stopwait = freezeStopWait;
        atomic.Store(_addr_sched.gcwaiting, 1); 
        // this should stop running goroutines
        if (!preemptall()) {
            break; // no running goroutines
        }
        usleep(1000);

    } 
    // to be sure
    usleep(1000);
    preemptall();
    usleep(1000);

}

// All reads and writes of g's status go through readgstatus, casgstatus
// castogscanstatus, casfrom_Gscanstatus.
//go:nosplit
private static uint readgstatus(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    return atomic.Load(_addr_gp.atomicstatus);
}

// The Gscanstatuses are acting like locks and this releases them.
// If it proves to be a performance hit we should be able to make these
// simple atomic stores but for now we are going to throw if
// we see an inconsistent state.
private static void casfrom_Gscanstatus(ptr<g> _addr_gp, uint oldval, uint newval) {
    ref g gp = ref _addr_gp.val;

    var success = false; 

    // Check that transition is valid.

    if (oldval == _Gscanrunnable || oldval == _Gscanwaiting || oldval == _Gscanrunning || oldval == _Gscansyscall || oldval == _Gscanpreempted) 
        if (newval == oldval & ~_Gscan) {
            success = atomic.Cas(_addr_gp.atomicstatus, oldval, newval);
        }
    else 
        print("runtime: casfrom_Gscanstatus bad oldval gp=", gp, ", oldval=", hex(oldval), ", newval=", hex(newval), "\n");
        dumpgstatus(_addr_gp);
        throw("casfrom_Gscanstatus:top gp->status is not in scan state");
        if (!success) {
        print("runtime: casfrom_Gscanstatus failed gp=", gp, ", oldval=", hex(oldval), ", newval=", hex(newval), "\n");
        dumpgstatus(_addr_gp);
        throw("casfrom_Gscanstatus: gp->status is not in scan state");
    }
    releaseLockRank(lockRankGscan);

}

// This will return false if the gp is not in the expected status and the cas fails.
// This acts like a lock acquire while the casfromgstatus acts like a lock release.
private static bool castogscanstatus(ptr<g> _addr_gp, uint oldval, uint newval) => func((_, panic, _) => {
    ref g gp = ref _addr_gp.val;


    if (oldval == _Grunnable || oldval == _Grunning || oldval == _Gwaiting || oldval == _Gsyscall) 
        if (newval == oldval | _Gscan) {
            var r = atomic.Cas(_addr_gp.atomicstatus, oldval, newval);
            if (r) {
                acquireLockRank(lockRankGscan);
            }
            return r;
        }
        print("runtime: castogscanstatus oldval=", hex(oldval), " newval=", hex(newval), "\n");
    throw("castogscanstatus");
    panic("not reached");

});

// If asked to move to or from a Gscanstatus this will throw. Use the castogscanstatus
// and casfrom_Gscanstatus instead.
// casgstatus will loop if the g->atomicstatus is in a Gscan status until the routine that
// put it in the Gscan state is finished.
//go:nosplit
private static void casgstatus(ptr<g> _addr_gp, uint oldval, uint newval) {
    ref g gp = ref _addr_gp.val;

    if ((oldval & _Gscan != 0) || (newval & _Gscan != 0) || oldval == newval) {
        systemstack(() => {
            print("runtime: casgstatus: oldval=", hex(oldval), " newval=", hex(newval), "\n");
            throw("casgstatus: bad incoming values");
        });
    }
    acquireLockRank(lockRankGscan);
    releaseLockRank(lockRankGscan); 

    // See https://golang.org/cl/21503 for justification of the yield delay.
    const nint yieldDelay = 5 * 1000;

    long nextYield = default; 

    // loop if gp->atomicstatus is in a scan state giving
    // GC time to finish and change the state to oldval.
    for (nint i = 0; !atomic.Cas(_addr_gp.atomicstatus, oldval, newval); i++) {
        if (oldval == _Gwaiting && gp.atomicstatus == _Grunnable) {
            throw("casgstatus: waiting for Gwaiting but is Grunnable");
        }
        if (i == 0) {
            nextYield = nanotime() + yieldDelay;
        }
        if (nanotime() < nextYield) {
            for (nint x = 0; x < 10 && gp.atomicstatus != oldval; x++) {
                procyield(1);
            }
        else


        } {
            osyield();
            nextYield = nanotime() + yieldDelay / 2;
        }
    } 

    // Handle tracking for scheduling latencies.
    if (oldval == _Grunning) { 
        // Track every 8th time a goroutine transitions out of running.
        if (gp.trackingSeq % gTrackingPeriod == 0) {
            gp.tracking = true;
        }
        gp.trackingSeq++;

    }
    if (gp.tracking) {
        var now = nanotime();
        if (oldval == _Grunnable) { 
            // We transitioned out of runnable, so measure how much
            // time we spent in this state and add it to
            // runnableTime.
            gp.runnableTime += now - gp.runnableStamp;
            gp.runnableStamp = 0;

        }
        if (newval == _Grunnable) { 
            // We just transitioned into runnable, so record what
            // time that happened.
            gp.runnableStamp = now;

        }
        else if (newval == _Grunning) { 
            // We're transitioning into running, so turn off
            // tracking and record how much time we spent in
            // runnable.
            gp.tracking = false;
            sched.timeToRun.record(gp.runnableTime);
            gp.runnableTime = 0;

        }
    }
}

// casgstatus(gp, oldstatus, Gcopystack), assuming oldstatus is Gwaiting or Grunnable.
// Returns old status. Cannot call casgstatus directly, because we are racing with an
// async wakeup that might come in from netpoll. If we see Gwaiting from the readgstatus,
// it might have become Grunnable by the time we get to the cas. If we called casgstatus,
// it would loop waiting for the status to go back to Gwaiting, which it never will.
//go:nosplit
private static uint casgcopystack(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    while (true) {
        var oldstatus = readgstatus(_addr_gp) & ~_Gscan;
        if (oldstatus != _Gwaiting && oldstatus != _Grunnable) {
            throw("copystack: bad status, not Gwaiting or Grunnable");
        }
        if (atomic.Cas(_addr_gp.atomicstatus, oldstatus, _Gcopystack)) {
            return oldstatus;
        }
    }

}

// casGToPreemptScan transitions gp from _Grunning to _Gscan|_Gpreempted.
//
// TODO(austin): This is the only status operation that both changes
// the status and locks the _Gscan bit. Rethink this.
private static void casGToPreemptScan(ptr<g> _addr_gp, uint old, uint @new) {
    ref g gp = ref _addr_gp.val;

    if (old != _Grunning || new != _Gscan | _Gpreempted) {
        throw("bad g transition");
    }
    acquireLockRank(lockRankGscan);
    while (!atomic.Cas(_addr_gp.atomicstatus, _Grunning, _Gscan | _Gpreempted))     }

}

// casGFromPreempted attempts to transition gp from _Gpreempted to
// _Gwaiting. If successful, the caller is responsible for
// re-scheduling gp.
private static bool casGFromPreempted(ptr<g> _addr_gp, uint old, uint @new) {
    ref g gp = ref _addr_gp.val;

    if (old != _Gpreempted || new != _Gwaiting) {
        throw("bad g transition");
    }
    return atomic.Cas(_addr_gp.atomicstatus, _Gpreempted, _Gwaiting);

}

// stopTheWorld stops all P's from executing goroutines, interrupting
// all goroutines at GC safe points and records reason as the reason
// for the stop. On return, only the current goroutine's P is running.
// stopTheWorld must not be called from a system stack and the caller
// must not hold worldsema. The caller must call startTheWorld when
// other P's should resume execution.
//
// stopTheWorld is safe for multiple goroutines to call at the
// same time. Each will execute its own stop, and the stops will
// be serialized.
//
// This is also used by routines that do stack dumps. If the system is
// in panic or being exited, this may not reliably stop all
// goroutines.
private static void stopTheWorld(@string reason) {
    semacquire(_addr_worldsema);
    var gp = getg();
    gp.m.preemptoff = reason;
    systemstack(() => { 
        // Mark the goroutine which called stopTheWorld preemptible so its
        // stack may be scanned.
        // This lets a mark worker scan us while we try to stop the world
        // since otherwise we could get in a mutual preemption deadlock.
        // We must not modify anything on the G stack because a stack shrink
        // may occur. A stack shrink is otherwise OK though because in order
        // to return from this function (and to leave the system stack) we
        // must have preempted all goroutines, including any attempting
        // to scan our stack, in which case, any stack shrinking will
        // have already completed by the time we exit.
        casgstatus(_addr_gp, _Grunning, _Gwaiting);
        stopTheWorldWithSema();
        casgstatus(_addr_gp, _Gwaiting, _Grunning);

    });

}

// startTheWorld undoes the effects of stopTheWorld.
private static void startTheWorld() {
    systemstack(() => {
        startTheWorldWithSema(false);
    }); 

    // worldsema must be held over startTheWorldWithSema to ensure
    // gomaxprocs cannot change while worldsema is held.
    //
    // Release worldsema with direct handoff to the next waiter, but
    // acquirem so that semrelease1 doesn't try to yield our time.
    //
    // Otherwise if e.g. ReadMemStats is being called in a loop,
    // it might stomp on other attempts to stop the world, such as
    // for starting or ending GC. The operation this blocks is
    // so heavy-weight that we should just try to be as fair as
    // possible here.
    //
    // We don't want to just allow us to get preempted between now
    // and releasing the semaphore because then we keep everyone
    // (including, for example, GCs) waiting longer.
    var mp = acquirem();
    mp.preemptoff = "";
    semrelease1(_addr_worldsema, true, 0);
    releasem(mp);

}

// stopTheWorldGC has the same effect as stopTheWorld, but blocks
// until the GC is not running. It also blocks a GC from starting
// until startTheWorldGC is called.
private static void stopTheWorldGC(@string reason) {
    semacquire(_addr_gcsema);
    stopTheWorld(reason);
}

// startTheWorldGC undoes the effects of stopTheWorldGC.
private static void startTheWorldGC() {
    startTheWorld();
    semrelease(_addr_gcsema);
}

// Holding worldsema grants an M the right to try to stop the world.
private static uint worldsema = 1;

// Holding gcsema grants the M the right to block a GC, and blocks
// until the current GC is done. In particular, it prevents gomaxprocs
// from changing concurrently.
//
// TODO(mknyszek): Once gomaxprocs and the execution tracer can handle
// being changed/enabled during a GC, remove this.
private static uint gcsema = 1;

// stopTheWorldWithSema is the core implementation of stopTheWorld.
// The caller is responsible for acquiring worldsema and disabling
// preemption first and then should stopTheWorldWithSema on the system
// stack:
//
//    semacquire(&worldsema, 0)
//    m.preemptoff = "reason"
//    systemstack(stopTheWorldWithSema)
//
// When finished, the caller must either call startTheWorld or undo
// these three operations separately:
//
//    m.preemptoff = ""
//    systemstack(startTheWorldWithSema)
//    semrelease(&worldsema)
//
// It is allowed to acquire worldsema once and then execute multiple
// startTheWorldWithSema/stopTheWorldWithSema pairs.
// Other P's are able to execute between successive calls to
// startTheWorldWithSema and stopTheWorldWithSema.
// Holding worldsema causes any other goroutines invoking
// stopTheWorld to block.
private static void stopTheWorldWithSema() {
    var _g_ = getg(); 

    // If we hold a lock, then we won't be able to stop another M
    // that is blocked trying to acquire the lock.
    if (_g_.m.locks > 0) {
        throw("stopTheWorld: holding locks");
    }
    lock(_addr_sched.@lock);
    sched.stopwait = gomaxprocs;
    atomic.Store(_addr_sched.gcwaiting, 1);
    preemptall(); 
    // stop current P
    _g_.m.p.ptr().status = _Pgcstop; // Pgcstop is only diagnostic.
    sched.stopwait--; 
    // try to retake all P's in Psyscall status
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp) {
            p = __p;
            var s = p.status;
            if (s == _Psyscall && atomic.Cas(_addr_p.status, s, _Pgcstop)) {
                if (trace.enabled) {
                    traceGoSysBlock(p);
                    traceProcStop(p);
                }
                p.syscalltick++;
                sched.stopwait--;
            }
        }
        p = p__prev1;
    }

    while (true) {
        var p = pidleget();
        if (p == null) {
            break;
        }
        p.status = _Pgcstop;
        sched.stopwait--;

    }
    var wait = sched.stopwait > 0;
    unlock(_addr_sched.@lock); 

    // wait for remaining P's to stop voluntarily
    if (wait) {
        while (true) { 
            // wait for 100us, then try to re-preempt in case of any races
            if (notetsleep(_addr_sched.stopnote, 100 * 1000)) {
                noteclear(_addr_sched.stopnote);
                break;
            }

            preemptall();

        }

    }
    @string bad = "";
    if (sched.stopwait != 0) {
        bad = "stopTheWorld: not stopped (stopwait != 0)";
    }
    else
 {
        {
            var p__prev1 = p;

            foreach (var (_, __p) in allp) {
                p = __p;
                if (p.status != _Pgcstop) {
                    bad = "stopTheWorld: not stopped (status != _Pgcstop)";
                }
            }

            p = p__prev1;
        }
    }
    if (atomic.Load(_addr_freezing) != 0) { 
        // Some other thread is panicking. This can cause the
        // sanity checks above to fail if the panic happens in
        // the signal handler on a stopped thread. Either way,
        // we should halt this thread.
        lock(_addr_deadlock);
        lock(_addr_deadlock);

    }
    if (bad != "") {
        throw(bad);
    }
    worldStopped();

}

private static long startTheWorldWithSema(bool emitTraceEvent) {
    assertWorldStopped();

    var mp = acquirem(); // disable preemption because it can be holding p in a local var
    if (netpollinited()) {
        ref var list = ref heap(netpoll(0), out ptr<var> _addr_list); // non-blocking
        injectglist(_addr_list);

    }
    lock(_addr_sched.@lock);

    var procs = gomaxprocs;
    if (newprocs != 0) {
        procs = newprocs;
        newprocs = 0;
    }
    var p1 = procresize(procs);
    sched.gcwaiting = 0;
    if (sched.sysmonwait != 0) {
        sched.sysmonwait = 0;
        notewakeup(_addr_sched.sysmonnote);
    }
    unlock(_addr_sched.@lock);

    worldStarted();

    while (p1 != null) {
        var p = p1;
        p1 = p1.link.ptr();
        if (p.m != 0) {
            mp = p.m.ptr();
            p.m = 0;
            if (mp.nextp != 0) {
                throw("startTheWorld: inconsistent mp->nextp");
            }
            mp.nextp.set(p);
            notewakeup(_addr_mp.park);
        }
        else
 { 
            // Start M to run P.  Do not start another M below.
            newm(null, _addr_p, -1);

        }
    } 

    // Capture start-the-world time before doing clean-up tasks.
    var startTime = nanotime();
    if (emitTraceEvent) {
        traceGCSTWDone();
    }
    wakep();

    releasem(mp);

    return startTime;

}

// usesLibcall indicates whether this runtime performs system calls
// via libcall.
private static bool usesLibcall() {
    switch (GOOS) {
        case "aix": 

        case "darwin": 

        case "illumos": 

        case "ios": 

        case "solaris": 

        case "windows": 
            return true;
            break;
        case "openbsd": 
            return GOARCH == "386" || GOARCH == "amd64" || GOARCH == "arm" || GOARCH == "arm64";
            break;
    }
    return false;

}

// mStackIsSystemAllocated indicates whether this runtime starts on a
// system-allocated stack.
private static bool mStackIsSystemAllocated() {
    switch (GOOS) {
        case "aix": 

        case "darwin": 

        case "plan9": 

        case "illumos": 

        case "ios": 

        case "solaris": 

        case "windows": 
            return true;
            break;
        case "openbsd": 
            switch (GOARCH) {
                case "386": 

                case "amd64": 

                case "arm": 

                case "arm64": 
                    return true;
                    break;
            }

            break;
    }
    return false;

}

// mstart is the entry-point for new Ms.
// It is written in assembly, uses ABI0, is marked TOPFRAME, and calls mstart0.
private static void mstart();

// mstart0 is the Go entry-point for new Ms.
// This must not split the stack because we may not even have stack
// bounds set up yet.
//
// May run during STW (because it doesn't have a P yet), so write
// barriers are not allowed.
//
//go:nosplit
//go:nowritebarrierrec
private static void mstart0() {
    var _g_ = getg();

    var osStack = _g_.stack.lo == 0;
    if (osStack) {>>MARKER:FUNCTION_mstart_BLOCK_PREFIX<< 
        // Initialize stack bounds from system stack.
        // Cgo may have left stack size in stack.hi.
        // minit may update the stack bounds.
        //
        // Note: these bounds may not be very accurate.
        // We set hi to &size, but there are things above
        // it. The 1024 is supposed to compensate this,
        // but is somewhat arbitrary.
        ref var size = ref heap(_g_.stack.hi, out ptr<var> _addr_size);
        if (size == 0) {
            size = 8192 * sys.StackGuardMultiplier;
        }
        _g_.stack.hi = uintptr(noescape(@unsafe.Pointer(_addr_size)));
        _g_.stack.lo = _g_.stack.hi - size + 1024;

    }
    _g_.stackguard0 = _g_.stack.lo + _StackGuard; 
    // This is the g0, so we can also call go:systemstack
    // functions, which check stackguard1.
    _g_.stackguard1 = _g_.stackguard0;
    mstart1(); 

    // Exit this thread.
    if (mStackIsSystemAllocated()) { 
        // Windows, Solaris, illumos, Darwin, AIX and Plan 9 always system-allocate
        // the stack, but put it in _g_.stack before mstart,
        // so the logic above hasn't set osStack yet.
        osStack = true;

    }
    mexit(osStack);

}

// The go:noinline is to guarantee the getcallerpc/getcallersp below are safe,
// so that we can set up g0.sched to return to the call of mstart1 above.
//go:noinline
private static void mstart1() {
    var _g_ = getg();

    if (_g_ != _g_.m.g0) {
        throw("bad runtime·mstart");
    }
    _g_.sched.g = guintptr(@unsafe.Pointer(_g_));
    _g_.sched.pc = getcallerpc();
    _g_.sched.sp = getcallersp();

    asminit();
    minit(); 

    // Install signal handlers; after minit so that minit can
    // prepare the thread to be able to handle the signals.
    if (_g_.m == _addr_m0) {
        mstartm0();
    }
    {
        var fn = _g_.m.mstartfn;

        if (fn != null) {
            fn();
        }
    }


    if (_g_.m != _addr_m0) {
        acquirep(_addr__g_.m.nextp.ptr());
        _g_.m.nextp = 0;
    }
    schedule();

}

// mstartm0 implements part of mstart1 that only runs on the m0.
//
// Write barriers are allowed here because we know the GC can't be
// running yet, so they'll be no-ops.
//
//go:yeswritebarrierrec
private static void mstartm0() { 
    // Create an extra M for callbacks on threads not created by Go.
    // An extra M is also needed on Windows for callbacks created by
    // syscall.NewCallback. See issue #6751 for details.
    if ((iscgo || GOOS == "windows") && !cgoHasExtraM) {
        cgoHasExtraM = true;
        newextram();
    }
    initsig(false);

}

// mPark causes a thread to park itself - temporarily waking for
// fixups but otherwise waiting to be fully woken. This is the
// only way that m's should park themselves.
//go:nosplit
private static void mPark() {
    var g = getg();
    while (true) {
        notesleep(_addr_g.m.park); 
        // Note, because of signal handling by this parked m,
        // a preemptive mDoFixup() may actually occur via
        // mDoFixupAndOSYield(). (See golang.org/issue/44193)
        noteclear(_addr_g.m.park);
        if (!mDoFixup()) {
            return ;
        }
    }

}

// mexit tears down and exits the current thread.
//
// Don't call this directly to exit the thread, since it must run at
// the top of the thread stack. Instead, use gogo(&_g_.m.g0.sched) to
// unwind the stack to the point that exits the thread.
//
// It is entered with m.p != nil, so write barriers are allowed. It
// will release the P before exiting.
//
//go:yeswritebarrierrec
private static void mexit(bool osStack) {
    var g = getg();
    var m = g.m;

    if (m == _addr_m0) { 
        // This is the main thread. Just wedge it.
        //
        // On Linux, exiting the main thread puts the process
        // into a non-waitable zombie state. On Plan 9,
        // exiting the main thread unblocks wait even though
        // other threads are still running. On Solaris we can
        // neither exitThread nor return from mstart. Other
        // bad things probably happen on other platforms.
        //
        // We could try to clean up this M more before wedging
        // it, but that complicates signal handling.
        handoffp(_addr_releasep());
        lock(_addr_sched.@lock);
        sched.nmfreed++;
        checkdead();
        unlock(_addr_sched.@lock);
        mPark();
        throw("locked m0 woke up");

    }
    sigblock(true);
    unminit(); 

    // Free the gsignal stack.
    if (m.gsignal != null) {
        stackfree(m.gsignal.stack); 
        // On some platforms, when calling into VDSO (e.g. nanotime)
        // we store our g on the gsignal stack, if there is one.
        // Now the stack is freed, unlink it from the m, so we
        // won't write to it when calling VDSO code.
        m.gsignal = null;

    }
    lock(_addr_sched.@lock);
    {
        var pprev = _addr_allm;

        while (pprev != null.val) {
            if (pprev == m.val) {
                pprev.val = m.alllink;
                goto found;
            pprev = _addr_(pprev.val).alllink;
            }

        }
    }
    throw("m not found in allm");
found:
    if (!osStack) { 
        // Delay reaping m until it's done with the stack.
        //
        // If this is using an OS stack, the OS will free it
        // so there's no need for reaping.
        atomic.Store(_addr_m.freeWait, 1); 
        // Put m on the free list, though it will not be reaped until
        // freeWait is 0. Note that the free list must not be linked
        // through alllink because some functions walk allm without
        // locking, so may be using alllink.
        m.freelink = sched.freem;
        sched.freem = m;

    }
    unlock(_addr_sched.@lock);

    atomic.Xadd64(_addr_ncgocall, int64(m.ncgocall)); 

    // Release the P.
    handoffp(_addr_releasep()); 
    // After this point we must not have write barriers.

    // Invoke the deadlock detector. This must happen after
    // handoffp because it may have started a new M to take our
    // P's work.
    lock(_addr_sched.@lock);
    sched.nmfreed++;
    checkdead();
    unlock(_addr_sched.@lock);

    if (GOOS == "darwin" || GOOS == "ios") { 
        // Make sure pendingPreemptSignals is correct when an M exits.
        // For #41702.
        if (atomic.Load(_addr_m.signalPending) != 0) {
            atomic.Xadd(_addr_pendingPreemptSignals, -1);
        }
    }
    mdestroy(m);

    if (osStack) { 
        // Return from mstart and let the system thread
        // library free the g0 stack and terminate the thread.
        return ;

    }
    exitThread(_addr_m.freeWait);

}

// forEachP calls fn(p) for every P p when p reaches a GC safe point.
// If a P is currently executing code, this will bring the P to a GC
// safe point and execute fn on that P. If the P is not executing code
// (it is idle or in a syscall), this will call fn(p) directly while
// preventing the P from exiting its state. This does not ensure that
// fn will run on every CPU executing Go code, but it acts as a global
// memory barrier. GC uses this as a "ragged barrier."
//
// The caller must hold worldsema.
//
//go:systemstack
private static void forEachP(Action<ptr<p>> fn) {
    var mp = acquirem();
    var _p_ = getg().m.p.ptr();

    lock(_addr_sched.@lock);
    if (sched.safePointWait != 0) {
        throw("forEachP: sched.safePointWait != 0");
    }
    sched.safePointWait = gomaxprocs - 1;
    sched.safePointFn = fn; 

    // Ask all Ps to run the safe point function.
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp) {
            p = __p;
            if (p != _p_) {
                atomic.Store(_addr_p.runSafePointFn, 1);
            }
        }
        p = p__prev1;
    }

    preemptall(); 

    // Any P entering _Pidle or _Psyscall from now on will observe
    // p.runSafePointFn == 1 and will call runSafePointFn when
    // changing its status to _Pidle/_Psyscall.

    // Run safe point function for all idle Ps. sched.pidle will
    // not change because we hold sched.lock.
    {
        var p__prev1 = p;

        var p = sched.pidle.ptr();

        while (p != null) {
            if (atomic.Cas(_addr_p.runSafePointFn, 1, 0)) {
                fn(p);
                sched.safePointWait--;
            p = p.link.ptr();
            }

        }

        p = p__prev1;
    }

    var wait = sched.safePointWait > 0;
    unlock(_addr_sched.@lock); 

    // Run fn for the current P.
    fn(_p_); 

    // Force Ps currently in _Psyscall into _Pidle and hand them
    // off to induce safe point function execution.
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp) {
            p = __p;
            var s = p.status;
            if (s == _Psyscall && p.runSafePointFn == 1 && atomic.Cas(_addr_p.status, s, _Pidle)) {
                if (trace.enabled) {
                    traceGoSysBlock(p);
                    traceProcStop(p);
                }
                p.syscalltick++;
                handoffp(_addr_p);
            }
        }
        p = p__prev1;
    }

    if (wait) {
        while (true) { 
            // Wait for 100us, then try to re-preempt in
            // case of any races.
            //
            // Requires system stack.
            if (notetsleep(_addr_sched.safePointNote, 100 * 1000)) {
                noteclear(_addr_sched.safePointNote);
                break;
            }

            preemptall();

        }

    }
    if (sched.safePointWait != 0) {
        throw("forEachP: not done");
    }
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp) {
            p = __p;
            if (p.runSafePointFn != 0) {
                throw("forEachP: P did not run fn");
            }
        }
        p = p__prev1;
    }

    lock(_addr_sched.@lock);
    sched.safePointFn = null;
    unlock(_addr_sched.@lock);
    releasem(mp);

}

// syscall_runtime_doAllThreadsSyscall serializes Go execution and
// executes a specified fn() call on all m's.
//
// The boolean argument to fn() indicates whether the function's
// return value will be consulted or not. That is, fn(true) should
// return true if fn() succeeds, and fn(true) should return false if
// it failed. When fn(false) is called, its return status will be
// ignored.
//
// syscall_runtime_doAllThreadsSyscall first invokes fn(true) on a
// single, coordinating, m, and only if it returns true does it go on
// to invoke fn(false) on all of the other m's known to the process.
//
//go:linkname syscall_runtime_doAllThreadsSyscall syscall.runtime_doAllThreadsSyscall
private static bool syscall_runtime_doAllThreadsSyscall(Func<bool, bool> fn) => func((_, panic, _) => {
    if (iscgo) {
        panic("doAllThreadsSyscall not supported with cgo enabled");
    }
    if (fn == null) {
        return ;
    }
    while (atomic.Load(_addr_sched.sysmonStarting) != 0) {
        osyield();
    } 

    // We don't want this thread to handle signals for the
    // duration of this critical section. The underlying issue
    // being that this locked coordinating m is the one monitoring
    // for fn() execution by all the other m's of the runtime,
    // while no regular go code execution is permitted (the world
    // is stopped). If this present m were to get distracted to
    // run signal handling code, and find itself waiting for a
    // second thread to execute go code before being able to
    // return from that signal handling, a deadlock will result.
    // (See golang.org/issue/44193.)
    lockOSThread();
    ref sigset sigmask = ref heap(out ptr<sigset> _addr_sigmask);
    sigsave(_addr_sigmask);
    sigblock(false);

    stopTheWorldGC("doAllThreadsSyscall");
    if (atomic.Load(_addr_newmHandoff.haveTemplateThread) != 0) { 
        // Ensure that there are no in-flight thread
        // creations: don't want to race with allm.
        lock(_addr_newmHandoff.@lock);
        while (!newmHandoff.waiting) {
            unlock(_addr_newmHandoff.@lock);
            osyield();
            lock(_addr_newmHandoff.@lock);
        }
        unlock(_addr_newmHandoff.@lock);

    }
    if (netpollinited()) {
        netpollBreak();
    }
    sigRecvPrepareForFixup();
    var _g_ = getg();
    if (raceenabled) { 
        // For m's running without racectx, we loan out the
        // racectx of this call.
        lock(_addr_mFixupRace.@lock);
        mFixupRace.ctx = _g_.racectx;
        unlock(_addr_mFixupRace.@lock);

    }
    {
        var ok = fn(true);

        if (ok) {
            var tid = _g_.m.procid;
            {
                var mp__prev1 = mp;

                var mp = allm;

                while (mp != null) {
                    if (mp.procid == tid) { 
                        // This m has already completed fn()
                        // call.
                        continue;
                    mp = mp.alllink;
                    } 
                    // Be wary of mp's without procid values if
                    // they are known not to park. If they are
                    // marked as parking with a zero procid, then
                    // they will be racing with this code to be
                    // allocated a procid and we will annotate
                    // them with the need to execute the fn when
                    // they acquire a procid to run it.
                    if (mp.procid == 0 && !mp.doesPark) { 
                        // Reaching here, we are either
                        // running Windows, or cgo linked
                        // code. Neither of which are
                        // currently supported by this API.
                        throw("unsupported runtime environment");

                    } 
                    // stopTheWorldGC() doesn't guarantee stopping
                    // all the threads, so we lock here to avoid
                    // the possibility of racing with mp.
                    lock(_addr_mp.mFixup.@lock);
                    mp.mFixup.fn = fn;
                    atomic.Store(_addr_mp.mFixup.used, 1);
                    if (mp.doesPark) { 
                        // For non-service threads this will
                        // cause the wakeup to be short lived
                        // (once the mutex is unlocked). The
                        // next real wakeup will occur after
                        // startTheWorldGC() is called.
                        notewakeup(_addr_mp.park);

                    }

                    unlock(_addr_mp.mFixup.@lock);

                }


                mp = mp__prev1;
            }
            while (true) {
                var done = true;
                {
                    var mp__prev2 = mp;

                    mp = allm;

                    while (done && mp != null) {
                        if (mp.procid == tid) {
                            continue;
                        mp = mp.alllink;
                        }

                        done = atomic.Load(_addr_mp.mFixup.used) == 0;

                    }


                    mp = mp__prev2;
                }
                if (done) {
                    break;
                } 
                // if needed force sysmon and/or newmHandoff to wakeup.
                lock(_addr_sched.@lock);
                if (atomic.Load(_addr_sched.sysmonwait) != 0) {
                    atomic.Store(_addr_sched.sysmonwait, 0);
                    notewakeup(_addr_sched.sysmonnote);
                }

                unlock(_addr_sched.@lock);
                lock(_addr_newmHandoff.@lock);
                if (newmHandoff.waiting) {
                    newmHandoff.waiting = false;
                    notewakeup(_addr_newmHandoff.wake);
                }

                unlock(_addr_newmHandoff.@lock);
                osyield();

            }


        }
    }

    if (raceenabled) {
        lock(_addr_mFixupRace.@lock);
        mFixupRace.ctx = 0;
        unlock(_addr_mFixupRace.@lock);
    }
    startTheWorldGC();
    msigrestore(sigmask);
    unlockOSThread();

});

// runSafePointFn runs the safe point function, if any, for this P.
// This should be called like
//
//     if getg().m.p.runSafePointFn != 0 {
//         runSafePointFn()
//     }
//
// runSafePointFn must be checked on any transition in to _Pidle or
// _Psyscall to avoid a race where forEachP sees that the P is running
// just before the P goes into _Pidle/_Psyscall and neither forEachP
// nor the P run the safe-point function.
private static void runSafePointFn() {
    var p = getg().m.p.ptr(); 
    // Resolve the race between forEachP running the safe-point
    // function on this P's behalf and this P running the
    // safe-point function directly.
    if (!atomic.Cas(_addr_p.runSafePointFn, 1, 0)) {
        return ;
    }
    sched.safePointFn(p);
    lock(_addr_sched.@lock);
    sched.safePointWait--;
    if (sched.safePointWait == 0) {
        notewakeup(_addr_sched.safePointNote);
    }
    unlock(_addr_sched.@lock);

}

// When running with cgo, we call _cgo_thread_start
// to start threads for us so that we can play nicely with
// foreign code.
private static unsafe.Pointer cgoThreadStart = default;

private partial struct cgothreadstart {
    public guintptr g;
    public ptr<ulong> tls;
    public unsafe.Pointer fn;
}

// Allocate a new m unassociated with any thread.
// Can use p for allocation context if needed.
// fn is recorded as the new m's m.mstartfn.
// id is optional pre-allocated m ID. Omit by passing -1.
//
// This function is allowed to have write barriers even if the caller
// isn't because it borrows _p_.
//
//go:yeswritebarrierrec
private static ptr<m> allocm(ptr<p> _addr__p_, Action fn, long id) {
    ref p _p_ = ref _addr__p_.val;

    var _g_ = getg();
    acquirem(); // disable GC because it can be called from sysmon
    if (_g_.m.p == 0) {
        acquirep(_addr__p_); // temporarily borrow p for mallocs in this function
    }
    if (sched.freem != null) {
        lock(_addr_sched.@lock);
        ptr<m> newList;
        {
            var freem = sched.freem;

            while (freem != null) {
                if (freem.freeWait != 0) {
                    var next = freem.freelink;
                    freem.freelink = newList;
                    newList = freem;
                    freem = next;
                    continue;
                } 
                // stackfree must be on the system stack, but allocm is
                // reachable off the system stack transitively from
                // startm.
                systemstack(() => {
                    stackfree(freem.g0.stack);
                });
                freem = freem.freelink;

            }

        }
        sched.freem = newList;
        unlock(_addr_sched.@lock);

    }
    ptr<m> mp = @new<m>();
    mp.mstartfn = fn;
    mcommoninit(mp, id); 

    // In case of cgo or Solaris or illumos or Darwin, pthread_create will make us a stack.
    // Windows and Plan 9 will layout sched stack on OS stack.
    if (iscgo || mStackIsSystemAllocated()) {
        mp.g0 = malg(-1);
    }
    else
 {
        mp.g0 = malg(8192 * sys.StackGuardMultiplier);
    }
    mp.g0.m = mp;

    if (_p_ == _g_.m.p.ptr()) {
        releasep();
    }
    releasem(_g_.m);

    return _addr_mp!;

}

// needm is called when a cgo callback happens on a
// thread without an m (a thread not created by Go).
// In this case, needm is expected to find an m to use
// and return with m, g initialized correctly.
// Since m and g are not set now (likely nil, but see below)
// needm is limited in what routines it can call. In particular
// it can only call nosplit functions (textflag 7) and cannot
// do any scheduling that requires an m.
//
// In order to avoid needing heavy lifting here, we adopt
// the following strategy: there is a stack of available m's
// that can be stolen. Using compare-and-swap
// to pop from the stack has ABA races, so we simulate
// a lock by doing an exchange (via Casuintptr) to steal the stack
// head and replace the top pointer with MLOCKED (1).
// This serves as a simple spin lock that we can use even
// without an m. The thread that locks the stack in this way
// unlocks the stack by storing a valid stack head pointer.
//
// In order to make sure that there is always an m structure
// available to be stolen, we maintain the invariant that there
// is always one more than needed. At the beginning of the
// program (if cgo is in use) the list is seeded with a single m.
// If needm finds that it has taken the last m off the list, its job
// is - once it has installed its own m so that it can do things like
// allocate memory - to create a spare m and put it on the list.
//
// Each of these extra m's also has a g0 and a curg that are
// pressed into service as the scheduling stack and current
// goroutine for the duration of the cgo callback.
//
// When the callback is done with the m, it calls dropm to
// put the m back on the list.
//go:nosplit
private static void needm() {
    if ((iscgo || GOOS == "windows") && !cgoHasExtraM) { 
        // Can happen if C/C++ code calls Go from a global ctor.
        // Can also happen on Windows if a global ctor uses a
        // callback created by syscall.NewCallback. See issue #6751
        // for details.
        //
        // Can not throw, because scheduler is not initialized yet.
        write(2, @unsafe.Pointer(_addr_earlycgocallback[0]), int32(len(earlycgocallback)));
        exit(1);

    }
    ref sigset sigmask = ref heap(out ptr<sigset> _addr_sigmask);
    sigsave(_addr_sigmask);
    sigblock(false); 

    // Lock extra list, take head, unlock popped list.
    // nilokay=false is safe here because of the invariant above,
    // that the extra list always contains or will soon contain
    // at least one m.
    var mp = lockextra(false); 

    // Set needextram when we've just emptied the list,
    // so that the eventual call into cgocallbackg will
    // allocate a new m for the extra list. We delay the
    // allocation until then so that it can be done
    // after exitsyscall makes sure it is okay to be
    // running at all (that is, there's no garbage collection
    // running right now).
    mp.needextram = mp.schedlink == 0;
    extraMCount--;
    unlockextra(_addr_mp.schedlink.ptr()); 

    // Store the original signal mask for use by minit.
    mp.sigmask = sigmask; 

    // Install TLS on some platforms (previously setg
    // would do this if necessary).
    osSetupTLS(mp); 

    // Install g (= m->g0) and set the stack bounds
    // to match the current stack. We don't actually know
    // how big the stack is, like we don't know how big any
    // scheduling stack is, but we assume there's at least 32 kB,
    // which is more than enough for us.
    setg(mp.g0);
    var _g_ = getg();
    _g_.stack.hi = getcallersp() + 1024;
    _g_.stack.lo = getcallersp() - 32 * 1024;
    _g_.stackguard0 = _g_.stack.lo + _StackGuard; 

    // Initialize this thread to use the m.
    asminit();
    minit(); 

    // mp.curg is now a real goroutine.
    casgstatus(_addr_mp.curg, _Gdead, _Gsyscall);
    atomic.Xadd(_addr_sched.ngsys, -1);

}

private static slice<byte> earlycgocallback = (slice<byte>)"fatal error: cgo callback before cgo call\n";

// newextram allocates m's and puts them on the extra list.
// It is called with a working local m, so that it can do things
// like call schedlock and allocate.
private static void newextram() {
    var c = atomic.Xchg(_addr_extraMWaiters, 0);
    if (c > 0) {
        for (var i = uint32(0); i < c; i++) {
            oneNewExtraM();
        }
    else


    } { 
        // Make sure there is at least one extra M.
        var mp = lockextra(true);
        unlockextra(_addr_mp);
        if (mp == null) {
            oneNewExtraM();
        }
    }
}

// oneNewExtraM allocates an m and puts it on the extra list.
private static void oneNewExtraM() { 
    // Create extra goroutine locked to extra m.
    // The goroutine is the context in which the cgo callback will run.
    // The sched.pc will never be returned to, but setting it to
    // goexit makes clear to the traceback routines where
    // the goroutine stack ends.
    var mp = allocm(_addr_null, null, -1);
    var gp = malg(4096);
    gp.sched.pc = abi.FuncPCABI0(goexit) + sys.PCQuantum;
    gp.sched.sp = gp.stack.hi;
    gp.sched.sp -= 4 * sys.PtrSize; // extra space in case of reads slightly beyond frame
    gp.sched.lr = 0;
    gp.sched.g = guintptr(@unsafe.Pointer(gp));
    gp.syscallpc = gp.sched.pc;
    gp.syscallsp = gp.sched.sp;
    gp.stktopsp = gp.sched.sp; 
    // malg returns status as _Gidle. Change to _Gdead before
    // adding to allg where GC can see it. We use _Gdead to hide
    // this from tracebacks and stack scans since it isn't a
    // "real" goroutine until needm grabs it.
    casgstatus(_addr_gp, _Gidle, _Gdead);
    gp.m = mp;
    mp.curg = gp;
    mp.lockedInt++;
    mp.lockedg.set(gp);
    gp.lockedm.set(mp);
    gp.goid = int64(atomic.Xadd64(_addr_sched.goidgen, 1));
    if (raceenabled) {
        gp.racectx = racegostart(funcPC(newextram) + sys.PCQuantum);
    }
    allgadd(_addr_gp); 

    // gp is now on the allg list, but we don't want it to be
    // counted by gcount. It would be more "proper" to increment
    // sched.ngfree, but that requires locking. Incrementing ngsys
    // has the same effect.
    atomic.Xadd(_addr_sched.ngsys, +1); 

    // Add m to the extra list.
    var mnext = lockextra(true);
    mp.schedlink.set(mnext);
    extraMCount++;
    unlockextra(_addr_mp);

}

// dropm is called when a cgo callback has called needm but is now
// done with the callback and returning back into the non-Go thread.
// It puts the current m back onto the extra list.
//
// The main expense here is the call to signalstack to release the
// m's signal stack, and then the call to needm on the next callback
// from this thread. It is tempting to try to save the m for next time,
// which would eliminate both these costs, but there might not be
// a next time: the current thread (which Go does not control) might exit.
// If we saved the m for that thread, there would be an m leak each time
// such a thread exited. Instead, we acquire and release an m on each
// call. These should typically not be scheduling operations, just a few
// atomics, so the cost should be small.
//
// TODO(rsc): An alternative would be to allocate a dummy pthread per-thread
// variable using pthread_key_create. Unlike the pthread keys we already use
// on OS X, this dummy key would never be read by Go code. It would exist
// only so that we could register at thread-exit-time destructor.
// That destructor would put the m back onto the extra list.
// This is purely a performance optimization. The current version,
// in which dropm happens on each cgo call, is still correct too.
// We may have to keep the current version on systems with cgo
// but without pthreads, like Windows.
private static void dropm() { 
    // Clear m and g, and return m to the extra list.
    // After the call to setg we can only call nosplit functions
    // with no pointer manipulation.
    var mp = getg().m; 

    // Return mp.curg to dead state.
    casgstatus(_addr_mp.curg, _Gsyscall, _Gdead);
    mp.curg.preemptStop = false;
    atomic.Xadd(_addr_sched.ngsys, +1); 

    // Block signals before unminit.
    // Unminit unregisters the signal handling stack (but needs g on some systems).
    // Setg(nil) clears g, which is the signal handler's cue not to run Go handlers.
    // It's important not to try to handle a signal between those two steps.
    var sigmask = mp.sigmask;
    sigblock(false);
    unminit();

    var mnext = lockextra(true);
    extraMCount++;
    mp.schedlink.set(mnext);

    setg(null); 

    // Commit the release of mp.
    unlockextra(_addr_mp);

    msigrestore(sigmask);

}

// A helper function for EnsureDropM.
private static System.UIntPtr getm() {
    return uintptr(@unsafe.Pointer(getg().m));
}

private static System.UIntPtr extram = default;
private static uint extraMCount = default; // Protected by lockextra
private static uint extraMWaiters = default;

// lockextra locks the extra list and returns the list head.
// The caller must unlock the list by storing a new list head
// to extram. If nilokay is true, then lockextra will
// return a nil list head if that's what it finds. If nilokay is false,
// lockextra will keep waiting until the list head is no longer nil.
//go:nosplit
private static ptr<m> lockextra(bool nilokay) {
    const nint locked = 1;



    var incr = false;
    while (true) {
        var old = atomic.Loaduintptr(_addr_extram);
        if (old == locked) {
            osyield_no_g();
            continue;
        }
        if (old == 0 && !nilokay) {
            if (!incr) { 
                // Add 1 to the number of threads
                // waiting for an M.
                // This is cleared by newextram.
                atomic.Xadd(_addr_extraMWaiters, 1);
                incr = true;

            }

            usleep_no_g(1);
            continue;

        }
        if (atomic.Casuintptr(_addr_extram, old, locked)) {
            return _addr_(m.val)(@unsafe.Pointer(old))!;
        }
        osyield_no_g();
        continue;

    }

}

//go:nosplit
private static void unlockextra(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    atomic.Storeuintptr(_addr_extram, uintptr(@unsafe.Pointer(mp)));
}

// execLock serializes exec and clone to avoid bugs or unspecified behaviour
// around exec'ing while creating/destroying threads.  See issue #19546.
private static rwmutex execLock = default;

// newmHandoff contains a list of m structures that need new OS threads.
// This is used by newm in situations where newm itself can't safely
// start an OS thread.
private static var newmHandoff = default;

// Create a new m. It will start off with a call to fn, or else the scheduler.
// fn needs to be static and not a heap allocated closure.
// May run with m.p==nil, so write barriers are not allowed.
//
// id is optional pre-allocated m ID. Omit by passing -1.
//go:nowritebarrierrec
private static void newm(Action fn, ptr<p> _addr__p_, long id) {
    ref p _p_ = ref _addr__p_.val;

    var mp = allocm(_addr__p_, fn, id);
    mp.doesPark = (_p_ != null);
    mp.nextp.set(_p_);
    mp.sigmask = initSigmask;
    {
        var gp = getg();

        if (gp != null && gp.m != null && (gp.m.lockedExt != 0 || gp.m.incgo) && GOOS != "plan9") { 
            // We're on a locked M or a thread that may have been
            // started by C. The kernel state of this thread may
            // be strange (the user may have locked it for that
            // purpose). We don't want to clone that into another
            // thread. Instead, ask a known-good thread to create
            // the thread for us.
            //
            // This is disabled on Plan 9. See golang.org/issue/22227.
            //
            // TODO: This may be unnecessary on Windows, which
            // doesn't model thread creation off fork.
            lock(_addr_newmHandoff.@lock);
            if (newmHandoff.haveTemplateThread == 0) {
                throw("on a locked thread with no template thread");
            }

            mp.schedlink = newmHandoff.newm;
            newmHandoff.newm.set(mp);
            if (newmHandoff.waiting) {
                newmHandoff.waiting = false;
                notewakeup(_addr_newmHandoff.wake);
            }

            unlock(_addr_newmHandoff.@lock);
            return ;

        }
    }

    newm1(_addr_mp);

}

private static void newm1(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (iscgo) {
        ref cgothreadstart ts = ref heap(out ptr<cgothreadstart> _addr_ts);
        if (_cgo_thread_start == null) {
            throw("_cgo_thread_start missing");
        }
        ts.g.set(mp.g0);
        ts.tls = (uint64.val)(@unsafe.Pointer(_addr_mp.tls[0]));
        ts.fn = @unsafe.Pointer(funcPC(mstart));
        if (msanenabled) {
            msanwrite(@unsafe.Pointer(_addr_ts), @unsafe.Sizeof(ts));
        }
        execLock.rlock(); // Prevent process clone.
        asmcgocall(_cgo_thread_start, @unsafe.Pointer(_addr_ts));
        execLock.runlock();
        return ;

    }
    execLock.rlock(); // Prevent process clone.
    newosproc(mp);
    execLock.runlock();

}

// startTemplateThread starts the template thread if it is not already
// running.
//
// The calling thread must itself be in a known-good state.
private static void startTemplateThread() {
    if (GOARCH == "wasm") { // no threads on wasm yet
        return ;

    }
    var mp = acquirem();
    if (!atomic.Cas(_addr_newmHandoff.haveTemplateThread, 0, 1)) {
        releasem(mp);
        return ;
    }
    newm(templateThread, _addr_null, -1);
    releasem(mp);

}

// mFixupRace is used to temporarily borrow the race context from the
// coordinating m during a syscall_runtime_doAllThreadsSyscall and
// loan it out to each of the m's of the runtime so they can execute a
// mFixup.fn in that context.
private static var mFixupRace = default;

// mDoFixup runs any outstanding fixup function for the running m.
// Returns true if a fixup was outstanding and actually executed.
//
// Note: to avoid deadlocks, and the need for the fixup function
// itself to be async safe, signals are blocked for the working m
// while it holds the mFixup lock. (See golang.org/issue/44193)
//
//go:nosplit
private static bool mDoFixup() {
    var _g_ = getg();
    {
        var used = atomic.Load(_addr__g_.m.mFixup.used);

        if (used == 0) {
            return false;
        }
    } 

    // slow path - if fixup fn is used, block signals and lock.
    ref sigset sigmask = ref heap(out ptr<sigset> _addr_sigmask);
    sigsave(_addr_sigmask);
    sigblock(false);
    lock(_addr__g_.m.mFixup.@lock);
    var fn = _g_.m.mFixup.fn;
    if (fn != null) {
        if (gcphase != _GCoff) { 
            // We can't have a write barrier in this
            // context since we may not have a P, but we
            // clear fn to signal that we've executed the
            // fixup. As long as fn is kept alive
            // elsewhere, technically we should have no
            // issues with the GC, but fn is likely
            // generated in a different package altogether
            // that may change independently. Just assert
            // the GC is off so this lack of write barrier
            // is more obviously safe.
            throw("GC must be disabled to protect validity of fn value");

        }
        if (_g_.racectx != 0 || !raceenabled) {
            fn(false);
        }
        else
 { 
            // temporarily acquire the context of the
            // originator of the
            // syscall_runtime_doAllThreadsSyscall and
            // block others from using it for the duration
            // of the fixup call.
            lock(_addr_mFixupRace.@lock);
            _g_.racectx = mFixupRace.ctx;
            fn(false);
            _g_.racectx = 0;
            unlock(_addr_mFixupRace.@lock);

        }
        (uintptr.val)(@unsafe.Pointer(_addr__g_.m.mFixup.fn)).val;

        0;
        atomic.Store(_addr__g_.m.mFixup.used, 0);

    }
    unlock(_addr__g_.m.mFixup.@lock);
    msigrestore(sigmask);
    return fn != null;

}

// mDoFixupAndOSYield is called when an m is unable to send a signal
// because the allThreadsSyscall mechanism is in progress. That is, an
// mPark() has been interrupted with this signal handler so we need to
// ensure the fixup is executed from this context.
//go:nosplit
private static void mDoFixupAndOSYield() {
    mDoFixup();
    osyield();
}

// templateThread is a thread in a known-good state that exists solely
// to start new threads in known-good states when the calling thread
// may not be in a good state.
//
// Many programs never need this, so templateThread is started lazily
// when we first enter a state that might lead to running on a thread
// in an unknown state.
//
// templateThread runs on an M without a P, so it must not have write
// barriers.
//
//go:nowritebarrierrec
private static void templateThread() {
    lock(_addr_sched.@lock);
    sched.nmsys++;
    checkdead();
    unlock(_addr_sched.@lock);

    while (true) {
        lock(_addr_newmHandoff.@lock);
        while (newmHandoff.newm != 0) {
            var newm = newmHandoff.newm.ptr();
            newmHandoff.newm = 0;
            unlock(_addr_newmHandoff.@lock);
            while (newm != null) {
                var next = newm.schedlink.ptr();
                newm.schedlink = 0;
                newm1(_addr_newm);
                newm = next;
            }

            lock(_addr_newmHandoff.@lock);
        }
        newmHandoff.waiting = true;
        noteclear(_addr_newmHandoff.wake);
        unlock(_addr_newmHandoff.@lock);
        notesleep(_addr_newmHandoff.wake);
        mDoFixup();
    }
}

// Stops execution of the current m until new work is available.
// Returns with acquired P.
private static void stopm() {
    var _g_ = getg();

    if (_g_.m.locks != 0) {
        throw("stopm holding locks");
    }
    if (_g_.m.p != 0) {
        throw("stopm holding p");
    }
    if (_g_.m.spinning) {
        throw("stopm spinning");
    }
    lock(_addr_sched.@lock);
    mput(_addr__g_.m);
    unlock(_addr_sched.@lock);
    mPark();
    acquirep(_addr__g_.m.nextp.ptr());
    _g_.m.nextp = 0;

}

private static void mspinning() { 
    // startm's caller incremented nmspinning. Set the new M's spinning.
    getg().m.spinning = true;

}

// Schedules some M to run the p (creates an M if necessary).
// If p==nil, tries to get an idle P, if no idle P's does nothing.
// May run with m.p==nil, so write barriers are not allowed.
// If spinning is set, the caller has incremented nmspinning and startm will
// either decrement nmspinning or set m.spinning in the newly started M.
//
// Callers passing a non-nil P must call from a non-preemptible context. See
// comment on acquirem below.
//
// Must not have write barriers because this may be called without a P.
//go:nowritebarrierrec
private static void startm(ptr<p> _addr__p_, bool spinning) {
    ref p _p_ = ref _addr__p_.val;
 
    // Disable preemption.
    //
    // Every owned P must have an owner that will eventually stop it in the
    // event of a GC stop request. startm takes transient ownership of a P
    // (either from argument or pidleget below) and transfers ownership to
    // a started M, which will be responsible for performing the stop.
    //
    // Preemption must be disabled during this transient ownership,
    // otherwise the P this is running on may enter GC stop while still
    // holding the transient P, leaving that P in limbo and deadlocking the
    // STW.
    //
    // Callers passing a non-nil P must already be in non-preemptible
    // context, otherwise such preemption could occur on function entry to
    // startm. Callers passing a nil P may be preemptible, so we must
    // disable preemption before acquiring a P from pidleget below.
    var mp = acquirem();
    lock(_addr_sched.@lock);
    if (_p_ == null) {
        _p_ = pidleget();
        if (_p_ == null) {
            unlock(_addr_sched.@lock);
            if (spinning) { 
                // The caller incremented nmspinning, but there are no idle Ps,
                // so it's okay to just undo the increment and give up.
                if (int32(atomic.Xadd(_addr_sched.nmspinning, -1)) < 0) {
                    throw("startm: negative nmspinning");
                }

            }

            releasem(mp);
            return ;

        }
    }
    var nmp = mget();
    if (nmp == null) { 
        // No M is available, we must drop sched.lock and call newm.
        // However, we already own a P to assign to the M.
        //
        // Once sched.lock is released, another G (e.g., in a syscall),
        // could find no idle P while checkdead finds a runnable G but
        // no running M's because this new M hasn't started yet, thus
        // throwing in an apparent deadlock.
        //
        // Avoid this situation by pre-allocating the ID for the new M,
        // thus marking it as 'running' before we drop sched.lock. This
        // new M will eventually run the scheduler to execute any
        // queued G's.
        var id = mReserveID();
        unlock(_addr_sched.@lock);

        Action fn = default;
        if (spinning) { 
            // The caller incremented nmspinning, so set m.spinning in the new M.
            fn = mspinning;

        }
        newm(fn, _addr__p_, id); 
        // Ownership transfer of _p_ committed by start in newm.
        // Preemption is now safe.
        releasem(mp);
        return ;

    }
    unlock(_addr_sched.@lock);
    if (nmp.spinning) {
        throw("startm: m is spinning");
    }
    if (nmp.nextp != 0) {
        throw("startm: m has p");
    }
    if (spinning && !runqempty(_addr__p_)) {
        throw("startm: p has runnable gs");
    }
    nmp.spinning = spinning;
    nmp.nextp.set(_p_);
    notewakeup(_addr_nmp.park); 
    // Ownership transfer of _p_ committed by wakeup. Preemption is now
    // safe.
    releasem(mp);

}

// Hands off P from syscall or locked M.
// Always runs without a P, so write barriers are not allowed.
//go:nowritebarrierrec
private static void handoffp(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;
 
    // handoffp must start an M in any situation where
    // findrunnable would return a G to run on _p_.

    // if it has local work, start it straight away
    if (!runqempty(_addr__p_) || sched.runqsize != 0) {
        startm(_addr__p_, false);
        return ;
    }
    if (gcBlackenEnabled != 0 && gcMarkWorkAvailable(_p_)) {
        startm(_addr__p_, false);
        return ;
    }
    if (atomic.Load(_addr_sched.nmspinning) + atomic.Load(_addr_sched.npidle) == 0 && atomic.Cas(_addr_sched.nmspinning, 0, 1)) { // TODO: fast atomic
        startm(_addr__p_, true);
        return ;

    }
    lock(_addr_sched.@lock);
    if (sched.gcwaiting != 0) {
        _p_.status = _Pgcstop;
        sched.stopwait--;
        if (sched.stopwait == 0) {
            notewakeup(_addr_sched.stopnote);
        }
        unlock(_addr_sched.@lock);
        return ;

    }
    if (_p_.runSafePointFn != 0 && atomic.Cas(_addr__p_.runSafePointFn, 1, 0)) {
        sched.safePointFn(_p_);
        sched.safePointWait--;
        if (sched.safePointWait == 0) {
            notewakeup(_addr_sched.safePointNote);
        }
    }
    if (sched.runqsize != 0) {
        unlock(_addr_sched.@lock);
        startm(_addr__p_, false);
        return ;
    }
    if (sched.npidle == uint32(gomaxprocs - 1) && atomic.Load64(_addr_sched.lastpoll) != 0) {
        unlock(_addr_sched.@lock);
        startm(_addr__p_, false);
        return ;
    }
    var when = nobarrierWakeTime(_p_);
    pidleput(_addr__p_);
    unlock(_addr_sched.@lock);

    if (when != 0) {
        wakeNetPoller(when);
    }
}

// Tries to add one more P to execute G's.
// Called when a G is made runnable (newproc, ready).
private static void wakep() {
    if (atomic.Load(_addr_sched.npidle) == 0) {
        return ;
    }
    if (atomic.Load(_addr_sched.nmspinning) != 0 || !atomic.Cas(_addr_sched.nmspinning, 0, 1)) {
        return ;
    }
    startm(_addr_null, true);

}

// Stops execution of the current m that is locked to a g until the g is runnable again.
// Returns with acquired P.
private static void stoplockedm() {
    var _g_ = getg();

    if (_g_.m.lockedg == 0 || _g_.m.lockedg.ptr().lockedm.ptr() != _g_.m) {
        throw("stoplockedm: inconsistent locking");
    }
    if (_g_.m.p != 0) { 
        // Schedule another M to run this p.
        var _p_ = releasep();
        handoffp(_addr__p_);

    }
    incidlelocked(1); 
    // Wait until another thread schedules lockedg again.
    mPark();
    var status = readgstatus(_addr__g_.m.lockedg.ptr());
    if (status & ~_Gscan != _Grunnable) {
        print("runtime:stoplockedm: lockedg (atomicstatus=", status, ") is not Grunnable or Gscanrunnable\n");
        dumpgstatus(_addr__g_.m.lockedg.ptr());
        throw("stoplockedm: not runnable");
    }
    acquirep(_addr__g_.m.nextp.ptr());
    _g_.m.nextp = 0;

}

// Schedules the locked m to run the locked gp.
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static void startlockedm(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    var _g_ = getg();

    var mp = gp.lockedm.ptr();
    if (mp == _g_.m) {
        throw("startlockedm: locked to me");
    }
    if (mp.nextp != 0) {
        throw("startlockedm: m has p");
    }
    incidlelocked(-1);
    var _p_ = releasep();
    mp.nextp.set(_p_);
    notewakeup(_addr_mp.park);
    stopm();

}

// Stops the current m for stopTheWorld.
// Returns when the world is restarted.
private static void gcstopm() {
    var _g_ = getg();

    if (sched.gcwaiting == 0) {
        throw("gcstopm: not waiting for gc");
    }
    if (_g_.m.spinning) {
        _g_.m.spinning = false; 
        // OK to just drop nmspinning here,
        // startTheWorld will unpark threads as necessary.
        if (int32(atomic.Xadd(_addr_sched.nmspinning, -1)) < 0) {
            throw("gcstopm: negative nmspinning");
        }
    }
    var _p_ = releasep();
    lock(_addr_sched.@lock);
    _p_.status = _Pgcstop;
    sched.stopwait--;
    if (sched.stopwait == 0) {
        notewakeup(_addr_sched.stopnote);
    }
    unlock(_addr_sched.@lock);
    stopm();

}

// Schedules gp to run on the current M.
// If inheritTime is true, gp inherits the remaining time in the
// current time slice. Otherwise, it starts a new time slice.
// Never returns.
//
// Write barriers are allowed because this is called immediately after
// acquiring a P in several places.
//
//go:yeswritebarrierrec
private static void execute(ptr<g> _addr_gp, bool inheritTime) {
    ref g gp = ref _addr_gp.val;

    var _g_ = getg(); 

    // Assign gp.m before entering _Grunning so running Gs have an
    // M.
    _g_.m.curg = gp;
    gp.m = _g_.m;
    casgstatus(_addr_gp, _Grunnable, _Grunning);
    gp.waitsince = 0;
    gp.preempt = false;
    gp.stackguard0 = gp.stack.lo + _StackGuard;
    if (!inheritTime) {
        _g_.m.p.ptr().schedtick++;
    }
    var hz = sched.profilehz;
    if (_g_.m.profilehz != hz) {
        setThreadCPUProfiler(hz);
    }
    if (trace.enabled) { 
        // GoSysExit has to happen when we have a P, but before GoStart.
        // So we emit it here.
        if (gp.syscallsp != 0 && gp.sysblocktraced) {
            traceGoSysExit(gp.sysexitticks);
        }
        traceGoStart();

    }
    gogo(_addr_gp.sched);

}

// Finds a runnable goroutine to execute.
// Tries to steal from other P's, get g from local or global queue, poll network.
private static (ptr<g>, bool) findrunnable() {
    ptr<g> gp = default!;
    bool inheritTime = default;

    var _g_ = getg(); 

    // The conditions here and in handoffp must agree: if
    // findrunnable would return a G to run, handoffp must start
    // an M.

top:
    var _p_ = _g_.m.p.ptr();
    if (sched.gcwaiting != 0) {
        gcstopm();
        goto top;
    }
    if (_p_.runSafePointFn != 0) {
        runSafePointFn();
    }
    var (now, pollUntil, _) = checkTimers(_addr__p_, 0);

    if (fingwait && fingwake) {
        {
            var gp__prev2 = gp;

            var gp = wakefing();

            if (gp != null) {
                ready(_addr_gp, 0, true);
            }

            gp = gp__prev2;

        }

    }
    if (cgo_yield != null.val) {
        asmcgocall(cgo_yield.val, null);
    }
    {
        var gp__prev1 = gp;

        var (gp, inheritTime) = runqget(_addr__p_);

        if (gp != null) {
            return (_addr_gp!, inheritTime);
        }
        gp = gp__prev1;

    } 

    // global runq
    if (sched.runqsize != 0) {
        lock(_addr_sched.@lock);
        gp = globrunqget(_addr__p_, 0);
        unlock(_addr_sched.@lock);
        if (gp != null) {
            return (_addr_gp!, false);
        }
    }
    if (netpollinited() && atomic.Load(_addr_netpollWaiters) > 0 && atomic.Load64(_addr_sched.lastpoll) != 0) {
        {
            var list__prev2 = list;

            ref var list = ref heap(netpoll(0), out ptr<var> _addr_list);

            if (!list.empty()) { // non-blocking
                gp = list.pop();
                injectglist(_addr_list);
                casgstatus(_addr_gp, _Gwaiting, _Grunnable);
                if (trace.enabled) {
                    traceGoUnpark(gp, 0);
                }

                return (_addr_gp!, false);

            }

            list = list__prev2;

        }

    }
    var procs = uint32(gomaxprocs);
    if (_g_.m.spinning || 2 * atomic.Load(_addr_sched.nmspinning) < procs - atomic.Load(_addr_sched.npidle)) {
        if (!_g_.m.spinning) {
            _g_.m.spinning = true;
            atomic.Xadd(_addr_sched.nmspinning, 1);
        }
        var (gp, inheritTime, tnow, w, newWork) = stealWork(now);
        now = tnow;
        if (gp != null) { 
            // Successfully stole.
            return (_addr_gp!, inheritTime);

        }
        if (newWork) { 
            // There may be new timer or GC work; restart to
            // discover.
            goto top;

        }
        if (w != 0 && (pollUntil == 0 || w < pollUntil)) { 
            // Earlier timer to wait for.
            pollUntil = w;

        }
    }
    if (gcBlackenEnabled != 0 && gcMarkWorkAvailable(_p_)) {
        var node = (gcBgMarkWorkerNode.val)(gcBgMarkWorkerPool.pop());
        if (node != null) {
            _p_.gcMarkWorkerMode = gcMarkWorkerIdleMode;
            gp = node.gp.ptr();
            casgstatus(_addr_gp, _Gwaiting, _Grunnable);
            if (trace.enabled) {
                traceGoUnpark(gp, 0);
            }
            return (_addr_gp!, false);
        }
    }
    var (gp, otherReady) = beforeIdle(now, pollUntil);
    if (gp != null) {
        casgstatus(_addr_gp, _Gwaiting, _Grunnable);
        if (trace.enabled) {
            traceGoUnpark(gp, 0);
        }
        return (_addr_gp!, false);

    }
    if (otherReady) {
        goto top;
    }
    var allpSnapshot = allp; 
    // Also snapshot masks. Value changes are OK, but we can't allow
    // len to change out from under us.
    var idlepMaskSnapshot = idlepMask;
    var timerpMaskSnapshot = timerpMask; 

    // return P and block
    lock(_addr_sched.@lock);
    if (sched.gcwaiting != 0 || _p_.runSafePointFn != 0) {
        unlock(_addr_sched.@lock);
        goto top;
    }
    if (sched.runqsize != 0) {
        gp = globrunqget(_addr__p_, 0);
        unlock(_addr_sched.@lock);
        return (_addr_gp!, false);
    }
    if (releasep() != _p_) {
        throw("findrunnable: wrong p");
    }
    pidleput(_addr__p_);
    unlock(_addr_sched.@lock); 

    // Delicate dance: thread transitions from spinning to non-spinning
    // state, potentially concurrently with submission of new work. We must
    // drop nmspinning first and then check all sources again (with
    // #StoreLoad memory barrier in between). If we do it the other way
    // around, another thread can submit work after we've checked all
    // sources but before we drop nmspinning; as a result nobody will
    // unpark a thread to run the work.
    //
    // This applies to the following sources of work:
    //
    // * Goroutines added to a per-P run queue.
    // * New/modified-earlier timers on a per-P timer heap.
    // * Idle-priority GC work (barring golang.org/issue/19112).
    //
    // If we discover new work below, we need to restore m.spinning as a signal
    // for resetspinning to unpark a new worker thread (because there can be more
    // than one starving goroutine). However, if after discovering new work
    // we also observe no idle Ps it is OK to skip unparking a new worker
    // thread: the system is fully loaded so no spinning threads are required.
    // Also see "Worker thread parking/unparking" comment at the top of the file.
    var wasSpinning = _g_.m.spinning;
    if (_g_.m.spinning) {
        _g_.m.spinning = false;
        if (int32(atomic.Xadd(_addr_sched.nmspinning, -1)) < 0) {
            throw("findrunnable: negative nmspinning");
        }
        _p_ = checkRunqsNoP(allpSnapshot, idlepMaskSnapshot);
        if (_p_ != null) {
            acquirep(_addr__p_);
            _g_.m.spinning = true;
            atomic.Xadd(_addr_sched.nmspinning, 1);
            goto top;
        }
        _p_, gp = checkIdleGCNoP();
        if (_p_ != null) {
            acquirep(_addr__p_);
            _g_.m.spinning = true;
            atomic.Xadd(_addr_sched.nmspinning, 1); 

            // Run the idle worker.
            _p_.gcMarkWorkerMode = gcMarkWorkerIdleMode;
            casgstatus(_addr_gp, _Gwaiting, _Grunnable);
            if (trace.enabled) {
                traceGoUnpark(gp, 0);
            }

            return (_addr_gp!, false);

        }
        pollUntil = checkTimersNoP(allpSnapshot, timerpMaskSnapshot, pollUntil);

    }
    if (netpollinited() && (atomic.Load(_addr_netpollWaiters) > 0 || pollUntil != 0) && atomic.Xchg64(_addr_sched.lastpoll, 0) != 0) {
        atomic.Store64(_addr_sched.pollUntil, uint64(pollUntil));
        if (_g_.m.p != 0) {
            throw("findrunnable: netpoll with p");
        }
        if (_g_.m.spinning) {
            throw("findrunnable: netpoll with spinning");
        }
        var delay = int64(-1);
        if (pollUntil != 0) {
            if (now == 0) {
                now = nanotime();
            }
            delay = pollUntil - now;
            if (delay < 0) {
                delay = 0;
            }
        }
        if (faketime != 0) { 
            // When using fake time, just poll.
            delay = 0;

        }
        list = netpoll(delay); // block until new work is available
        atomic.Store64(_addr_sched.pollUntil, 0);
        atomic.Store64(_addr_sched.lastpoll, uint64(nanotime()));
        if (faketime != 0 && list.empty()) { 
            // Using fake time and nothing is ready; stop M.
            // When all M's stop, checkdead will call timejump.
            stopm();
            goto top;

        }
        lock(_addr_sched.@lock);
        _p_ = pidleget();
        unlock(_addr_sched.@lock);
        if (_p_ == null) {
            injectglist(_addr_list);
        }
        else
 {
            acquirep(_addr__p_);
            if (!list.empty()) {
                gp = list.pop();
                injectglist(_addr_list);
                casgstatus(_addr_gp, _Gwaiting, _Grunnable);
                if (trace.enabled) {
                    traceGoUnpark(gp, 0);
                }
                return (_addr_gp!, false);
            }
            if (wasSpinning) {
                _g_.m.spinning = true;
                atomic.Xadd(_addr_sched.nmspinning, 1);
            }
            goto top;
        }
    }
    else if (pollUntil != 0 && netpollinited()) {
        var pollerPollUntil = int64(atomic.Load64(_addr_sched.pollUntil));
        if (pollerPollUntil == 0 || pollerPollUntil > pollUntil) {
            netpollBreak();
        }
    }
    stopm();
    goto top;

}

// pollWork reports whether there is non-background work this P could
// be doing. This is a fairly lightweight check to be used for
// background work loops, like idle GC. It checks a subset of the
// conditions checked by the actual scheduler.
private static bool pollWork() {
    if (sched.runqsize != 0) {
        return true;
    }
    var p = getg().m.p.ptr();
    if (!runqempty(_addr_p)) {
        return true;
    }
    if (netpollinited() && atomic.Load(_addr_netpollWaiters) > 0 && sched.lastpoll != 0) {
        {
            ref var list = ref heap(netpoll(0), out ptr<var> _addr_list);

            if (!list.empty()) {
                injectglist(_addr_list);
                return true;
            }

        }

    }
    return false;

}

// stealWork attempts to steal a runnable goroutine or timer from any P.
//
// If newWork is true, new work may have been readied.
//
// If now is not 0 it is the current time. stealWork returns the passed time or
// the current time if now was passed as 0.
private static (ptr<g>, bool, long, long, bool) stealWork(long now) {
    ptr<g> gp = default!;
    bool inheritTime = default;
    long rnow = default;
    long pollUntil = default;
    bool newWork = default;

    var pp = getg().m.p.ptr();

    var ranTimer = false;

    const nint stealTries = 4;

    for (nint i = 0; i < stealTries; i++) {
        var stealTimersOrRunNextG = i == stealTries - 1;

        for (var @enum = stealOrder.start(fastrand()); !@enum.done(); @enum.next()) {
            if (sched.gcwaiting != 0) { 
                // GC work may be available.
                return (_addr_null!, false, now, pollUntil, true);

            }

            var p2 = allp[@enum.position()];
            if (pp == p2) {
                continue;
            } 

            // Steal timers from p2. This call to checkTimers is the only place
            // where we might hold a lock on a different P's timers. We do this
            // once on the last pass before checking runnext because stealing
            // from the other P's runnext should be the last resort, so if there
            // are timers to steal do that first.
            //
            // We only check timers on one of the stealing iterations because
            // the time stored in now doesn't change in this loop and checking
            // the timers for each P more than once with the same value of now
            // is probably a waste of time.
            //
            // timerpMask tells us whether the P may have timers at all. If it
            // can't, no need to check at all.
            if (stealTimersOrRunNextG && timerpMask.read(@enum.position())) {
                var (tnow, w, ran) = checkTimers(_addr_p2, now);
                now = tnow;
                if (w != 0 && (pollUntil == 0 || w < pollUntil)) {
                    pollUntil = w;
                }
                if (ran) { 
                    // Running the timers may have
                    // made an arbitrary number of G's
                    // ready and added them to this P's
                    // local run queue. That invalidates
                    // the assumption of runqsteal
                    // that it always has room to add
                    // stolen G's. So check now if there
                    // is a local G to run.
                    {
                        var gp__prev3 = gp;

                        var (gp, inheritTime) = runqget(_addr_pp);

                        if (gp != null) {
                            return (_addr_gp!, inheritTime, now, pollUntil, ranTimer);
                        }

                        gp = gp__prev3;

                    }

                    ranTimer = true;

                }

            } 

            // Don't bother to attempt to steal if p2 is idle.
            if (!idlepMask.read(@enum.position())) {
                {
                    var gp__prev2 = gp;

                    var gp = runqsteal(_addr_pp, _addr_p2, stealTimersOrRunNextG);

                    if (gp != null) {
                        return (_addr_gp!, false, now, pollUntil, ranTimer);
                    }

                    gp = gp__prev2;

                }

            }

        }

    } 

    // No goroutines found to steal. Regardless, running a timer may have
    // made some goroutine ready that we missed. Indicate the next timer to
    // wait for.
    return (_addr_null!, false, now, pollUntil, ranTimer);

}

// Check all Ps for a runnable G to steal.
//
// On entry we have no P. If a G is available to steal and a P is available,
// the P is returned which the caller should acquire and attempt to steal the
// work to.
private static ptr<p> checkRunqsNoP(slice<ptr<p>> allpSnapshot, pMask idlepMaskSnapshot) {
    foreach (var (id, p2) in allpSnapshot) {
        if (!idlepMaskSnapshot.read(uint32(id)) && !runqempty(_addr_p2)) {
            lock(_addr_sched.@lock);
            var pp = pidleget();
            unlock(_addr_sched.@lock);
            if (pp != null) {
                return _addr_pp!;
            } 

            // Can't get a P, don't bother checking remaining Ps.
            break;

        }
    }    return _addr_null!;

}

// Check all Ps for a timer expiring sooner than pollUntil.
//
// Returns updated pollUntil value.
private static long checkTimersNoP(slice<ptr<p>> allpSnapshot, pMask timerpMaskSnapshot, long pollUntil) {
    foreach (var (id, p2) in allpSnapshot) {
        if (timerpMaskSnapshot.read(uint32(id))) {
            var w = nobarrierWakeTime(p2);
            if (w != 0 && (pollUntil == 0 || w < pollUntil)) {
                pollUntil = w;
            }
        }
    }    return pollUntil;

}

// Check for idle-priority GC, without a P on entry.
//
// If some GC work, a P, and a worker G are all available, the P and G will be
// returned. The returned P has not been wired yet.
private static (ptr<p>, ptr<g>) checkIdleGCNoP() {
    ptr<p> _p0 = default!;
    ptr<g> _p0 = default!;
 
    // N.B. Since we have no P, gcBlackenEnabled may change at any time; we
    // must check again after acquiring a P.
    if (atomic.Load(_addr_gcBlackenEnabled) == 0) {
        return (_addr_null!, _addr_null!);
    }
    if (!gcMarkWorkAvailable(null)) {
        return (_addr_null!, _addr_null!);
    }
    lock(_addr_sched.@lock);
    var pp = pidleget();
    if (pp == null) {
        unlock(_addr_sched.@lock);
        return (_addr_null!, _addr_null!);
    }
    if (gcBlackenEnabled == 0) {
        pidleput(_addr_pp);
        unlock(_addr_sched.@lock);
        return (_addr_null!, _addr_null!);
    }
    var node = (gcBgMarkWorkerNode.val)(gcBgMarkWorkerPool.pop());
    if (node == null) {
        pidleput(_addr_pp);
        unlock(_addr_sched.@lock);
        return (_addr_null!, _addr_null!);
    }
    unlock(_addr_sched.@lock);

    return (_addr_pp!, _addr_node.gp.ptr()!);

}

// wakeNetPoller wakes up the thread sleeping in the network poller if it isn't
// going to wake up before the when argument; or it wakes an idle P to service
// timers and the network poller if there isn't one already.
private static void wakeNetPoller(long when) {
    if (atomic.Load64(_addr_sched.lastpoll) == 0) { 
        // In findrunnable we ensure that when polling the pollUntil
        // field is either zero or the time to which the current
        // poll is expected to run. This can have a spurious wakeup
        // but should never miss a wakeup.
        var pollerPollUntil = int64(atomic.Load64(_addr_sched.pollUntil));
        if (pollerPollUntil == 0 || pollerPollUntil > when) {
            netpollBreak();
        }
    }
    else
 { 
        // There are no threads in the network poller, try to get
        // one there so it can handle new timers.
        if (GOOS != "plan9") { // Temporary workaround - see issue #42303.
            wakep();

        }
    }
}

private static void resetspinning() {
    var _g_ = getg();
    if (!_g_.m.spinning) {
        throw("resetspinning: not a spinning m");
    }
    _g_.m.spinning = false;
    var nmspinning = atomic.Xadd(_addr_sched.nmspinning, -1);
    if (int32(nmspinning) < 0) {
        throw("findrunnable: negative nmspinning");
    }
    wakep();

}

// injectglist adds each runnable G on the list to some run queue,
// and clears glist. If there is no current P, they are added to the
// global queue, and up to npidle M's are started to run them.
// Otherwise, for each idle P, this adds a G to the global queue
// and starts an M. Any remaining G's are added to the current P's
// local run queue.
// This may temporarily acquire sched.lock.
// Can run concurrently with GC.
private static void injectglist(ptr<gList> _addr_glist) {
    ref gList glist = ref _addr_glist.val;

    if (glist.empty()) {
        return ;
    }
    if (trace.enabled) {
        {
            var gp__prev1 = gp;

            var gp = glist.head.ptr();

            while (gp != null) {
                traceGoUnpark(gp, 0);
                gp = gp.schedlink.ptr();
            }


            gp = gp__prev1;
        }

    }
    var head = glist.head.ptr();
    ptr<g> tail;
    nint qsize = 0;
    {
        var gp__prev1 = gp;

        gp = head;

        while (gp != null) {
            tail = gp;
            qsize++;
            casgstatus(_addr_gp, _Gwaiting, _Grunnable);
            gp = gp.schedlink.ptr();
        }

        gp = gp__prev1;
    } 

    // Turn the gList into a gQueue.
    ref gQueue q = ref heap(out ptr<gQueue> _addr_q);
    q.head.set(head);
    q.tail.set(tail);
    glist = new gList();

    Action<nint> startIdle = n => {
        while (n != 0 && sched.npidle != 0) {
            startm(_addr_null, false);
            n--;
        }

    };

    var pp = getg().m.p.ptr();
    if (pp == null) {
        lock(_addr_sched.@lock);
        globrunqputbatch(_addr_q, int32(qsize));
        unlock(_addr_sched.@lock);
        startIdle(qsize);
        return ;
    }
    var npidle = int(atomic.Load(_addr_sched.npidle));
    ref gQueue globq = ref heap(out ptr<gQueue> _addr_globq);
    nint n = default;
    for (n = 0; n < npidle && !q.empty(); n++) {
        var g = q.pop();
        globq.pushBack(g);
    }
    if (n > 0) {
        lock(_addr_sched.@lock);
        globrunqputbatch(_addr_globq, int32(n));
        unlock(_addr_sched.@lock);
        startIdle(n);
        qsize -= n;
    }
    if (!q.empty()) {
        runqputbatch(_addr_pp, _addr_q, qsize);
    }
}

// One round of scheduler: find a runnable goroutine and execute it.
// Never returns.
private static void schedule() {
    var _g_ = getg();

    if (_g_.m.locks != 0) {
        throw("schedule: holding locks");
    }
    if (_g_.m.lockedg != 0) {
        stoplockedm();
        execute(_addr__g_.m.lockedg.ptr(), false); // Never returns.
    }
    if (_g_.m.incgo) {
        throw("schedule: in cgo");
    }
top:
    var pp = _g_.m.p.ptr();
    pp.preempt = false;

    if (sched.gcwaiting != 0) {
        gcstopm();
        goto top;
    }
    if (pp.runSafePointFn != 0) {
        runSafePointFn();
    }
    if (_g_.m.spinning && (pp.runnext != 0 || pp.runqhead != pp.runqtail)) {
        throw("schedule: spinning with local work");
    }
    checkTimers(_addr_pp, 0);

    ptr<g> gp;
    bool inheritTime = default; 

    // Normal goroutines will check for need to wakeP in ready,
    // but GCworkers and tracereaders will not, so the check must
    // be done here instead.
    var tryWakeP = false;
    if (trace.enabled || trace.shutdown) {
        gp = traceReader();
        if (gp != null) {
            casgstatus(gp, _Gwaiting, _Grunnable);
            traceGoUnpark(gp, 0);
            tryWakeP = true;
        }
    }
    if (gp == null && gcBlackenEnabled != 0) {
        gp = gcController.findRunnableGCWorker(_g_.m.p.ptr());
        if (gp != null) {
            tryWakeP = true;
        }
    }
    if (gp == null) { 
        // Check the global runnable queue once in a while to ensure fairness.
        // Otherwise two goroutines can completely occupy the local runqueue
        // by constantly respawning each other.
        if (_g_.m.p.ptr().schedtick % 61 == 0 && sched.runqsize > 0) {
            lock(_addr_sched.@lock);
            gp = globrunqget(_addr__g_.m.p.ptr(), 1);
            unlock(_addr_sched.@lock);
        }
    }
    if (gp == null) {
        gp, inheritTime = runqget(_addr__g_.m.p.ptr()); 
        // We can see gp != nil here even if the M is spinning,
        // if checkTimers added a local goroutine via goready.
    }
    if (gp == null) {
        gp, inheritTime = findrunnable(); // blocks until work is available
    }
    if (_g_.m.spinning) {
        resetspinning();
    }
    if (sched.disable.user && !schedEnabled(gp)) { 
        // Scheduling of this goroutine is disabled. Put it on
        // the list of pending runnable goroutines for when we
        // re-enable user scheduling and look again.
        lock(_addr_sched.@lock);
        if (schedEnabled(gp)) { 
            // Something re-enabled scheduling while we
            // were acquiring the lock.
            unlock(_addr_sched.@lock);

        }
        else
 {
            sched.disable.runnable.pushBack(gp);
            sched.disable.n++;
            unlock(_addr_sched.@lock);
            goto top;
        }
    }
    if (tryWakeP) {
        wakep();
    }
    if (gp.lockedm != 0) { 
        // Hands off own p to the locked m,
        // then blocks waiting for a new p.
        startlockedm(gp);
        goto top;

    }
    execute(gp, inheritTime);

}

// dropg removes the association between m and the current goroutine m->curg (gp for short).
// Typically a caller sets gp's status away from Grunning and then
// immediately calls dropg to finish the job. The caller is also responsible
// for arranging that gp will be restarted using ready at an
// appropriate time. After calling dropg and arranging for gp to be
// readied later, the caller can do other work but eventually should
// call schedule to restart the scheduling of goroutines on this m.
private static void dropg() {
    var _g_ = getg();

    setMNoWB(_addr__g_.m.curg.m, null);
    setGNoWB(_addr__g_.m.curg, null);
}

// checkTimers runs any timers for the P that are ready.
// If now is not 0 it is the current time.
// It returns the passed time or the current time if now was passed as 0.
// and the time when the next timer should run or 0 if there is no next timer,
// and reports whether it ran any timers.
// If the time when the next timer should run is not 0,
// it is always larger than the returned time.
// We pass now in and out to avoid extra calls of nanotime.
//go:yeswritebarrierrec
private static (long, long, bool) checkTimers(ptr<p> _addr_pp, long now) {
    long rnow = default;
    long pollUntil = default;
    bool ran = default;
    ref p pp = ref _addr_pp.val;
 
    // If it's not yet time for the first timer, or the first adjusted
    // timer, then there is nothing to do.
    var next = int64(atomic.Load64(_addr_pp.timer0When));
    var nextAdj = int64(atomic.Load64(_addr_pp.timerModifiedEarliest));
    if (next == 0 || (nextAdj != 0 && nextAdj < next)) {
        next = nextAdj;
    }
    if (next == 0) { 
        // No timers to run or adjust.
        return (now, 0, false);

    }
    if (now == 0) {
        now = nanotime();
    }
    if (now < next) { 
        // Next timer is not ready to run, but keep going
        // if we would clear deleted timers.
        // This corresponds to the condition below where
        // we decide whether to call clearDeletedTimers.
        if (pp != getg().m.p.ptr() || int(atomic.Load(_addr_pp.deletedTimers)) <= int(atomic.Load(_addr_pp.numTimers) / 4)) {
            return (now, next, false);
        }
    }
    lock(_addr_pp.timersLock);

    if (len(pp.timers) > 0) {
        adjusttimers(pp, now);
        while (len(pp.timers) > 0) { 
            // Note that runtimer may temporarily unlock
            // pp.timersLock.
            {
                var tw = runtimer(pp, now);

                if (tw != 0) {
                    if (tw > 0) {
                        pollUntil = tw;
                    }
                    break;
                }

            }

            ran = true;

        }

    }
    if (pp == getg().m.p.ptr() && int(atomic.Load(_addr_pp.deletedTimers)) > len(pp.timers) / 4) {
        clearDeletedTimers(pp);
    }
    unlock(_addr_pp.timersLock);

    return (now, pollUntil, ran);

}

private static bool parkunlock_c(ptr<g> _addr_gp, unsafe.Pointer @lock) {
    ref g gp = ref _addr_gp.val;

    unlock((mutex.val)(lock));
    return true;
}

// park continuation on g0.
private static void park_m(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    var _g_ = getg();

    if (trace.enabled) {
        traceGoPark(_g_.m.waittraceev, _g_.m.waittraceskip);
    }
    casgstatus(_addr_gp, _Grunning, _Gwaiting);
    dropg();

    {
        var fn = _g_.m.waitunlockf;

        if (fn != null) {
            var ok = fn(gp, _g_.m.waitlock);
            _g_.m.waitunlockf = null;
            _g_.m.waitlock = null;
            if (!ok) {
                if (trace.enabled) {
                    traceGoUnpark(gp, 2);
                }
                casgstatus(_addr_gp, _Gwaiting, _Grunnable);
                execute(_addr_gp, true); // Schedule it back, never returns.
            }

        }
    }

    schedule();

}

private static void goschedImpl(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    var status = readgstatus(_addr_gp);
    if (status & ~_Gscan != _Grunning) {
        dumpgstatus(_addr_gp);
        throw("bad g status");
    }
    casgstatus(_addr_gp, _Grunning, _Grunnable);
    dropg();
    lock(_addr_sched.@lock);
    globrunqput(_addr_gp);
    unlock(_addr_sched.@lock);

    schedule();

}

// Gosched continuation on g0.
private static void gosched_m(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (trace.enabled) {
        traceGoSched();
    }
    goschedImpl(_addr_gp);

}

// goschedguarded is a forbidden-states-avoided version of gosched_m
private static void goschedguarded_m(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (!canPreemptM(gp.m)) {
        gogo(_addr_gp.sched); // never return
    }
    if (trace.enabled) {
        traceGoSched();
    }
    goschedImpl(_addr_gp);

}

private static void gopreempt_m(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (trace.enabled) {
        traceGoPreempt();
    }
    goschedImpl(_addr_gp);

}

// preemptPark parks gp and puts it in _Gpreempted.
//
//go:systemstack
private static void preemptPark(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (trace.enabled) {
        traceGoPark(traceEvGoBlock, 0);
    }
    var status = readgstatus(_addr_gp);
    if (status & ~_Gscan != _Grunning) {
        dumpgstatus(_addr_gp);
        throw("bad g status");
    }
    gp.waitreason = waitReasonPreempted;

    if (gp.asyncSafePoint) { 
        // Double-check that async preemption does not
        // happen in SPWRITE assembly functions.
        // isAsyncSafePoint must exclude this case.
        var f = findfunc(gp.sched.pc);
        if (!f.valid()) {
            throw("preempt at unknown pc");
        }
        if (f.flag & funcFlag_SPWRITE != 0) {
            println("runtime: unexpected SPWRITE function", funcname(f), "in async preempt");
            throw("preempt SPWRITE");
        }
    }
    casGToPreemptScan(_addr_gp, _Grunning, _Gscan | _Gpreempted);
    dropg();
    casfrom_Gscanstatus(_addr_gp, _Gscan | _Gpreempted, _Gpreempted);
    schedule();

}

// goyield is like Gosched, but it:
// - emits a GoPreempt trace event instead of a GoSched trace event
// - puts the current G on the runq of the current P instead of the globrunq
private static void goyield() {
    checkTimeouts();
    mcall(goyield_m);
}

private static void goyield_m(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (trace.enabled) {
        traceGoPreempt();
    }
    var pp = gp.m.p.ptr();
    casgstatus(_addr_gp, _Grunning, _Grunnable);
    dropg();
    runqput(_addr_pp, _addr_gp, false);
    schedule();

}

// Finishes execution of the current goroutine.
private static void goexit1() {
    if (raceenabled) {
        racegoend();
    }
    if (trace.enabled) {
        traceGoEnd();
    }
    mcall(goexit0);

}

// goexit continuation on g0.
private static void goexit0(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    var _g_ = getg();

    casgstatus(_addr_gp, _Grunning, _Gdead);
    if (isSystemGoroutine(gp, false)) {
        atomic.Xadd(_addr_sched.ngsys, -1);
    }
    gp.m = null;
    var locked = gp.lockedm != 0;
    gp.lockedm = 0;
    _g_.m.lockedg = 0;
    gp.preemptStop = false;
    gp.paniconfault = false;
    gp._defer = null; // should be true already but just in case.
    gp._panic = null; // non-nil for Goexit during panic. points at stack-allocated data.
    gp.writebuf = null;
    gp.waitreason = 0;
    gp.param = null;
    gp.labels = null;
    gp.timer = null;

    if (gcBlackenEnabled != 0 && gp.gcAssistBytes > 0) { 
        // Flush assist credit to the global pool. This gives
        // better information to pacing if the application is
        // rapidly creating an exiting goroutines.
        var assistWorkPerByte = float64frombits(atomic.Load64(_addr_gcController.assistWorkPerByte));
        var scanCredit = int64(assistWorkPerByte * float64(gp.gcAssistBytes));
        atomic.Xaddint64(_addr_gcController.bgScanCredit, scanCredit);
        gp.gcAssistBytes = 0;

    }
    dropg();

    if (GOARCH == "wasm") { // no threads yet on wasm
        gfput(_addr__g_.m.p.ptr(), _addr_gp);
        schedule(); // never returns
    }
    if (_g_.m.lockedInt != 0) {
        print("invalid m->lockedInt = ", _g_.m.lockedInt, "\n");
        throw("internal lockOSThread error");
    }
    gfput(_addr__g_.m.p.ptr(), _addr_gp);
    if (locked) { 
        // The goroutine may have locked this thread because
        // it put it in an unusual kernel state. Kill it
        // rather than returning it to the thread pool.

        // Return to mstart, which will release the P and exit
        // the thread.
        if (GOOS != "plan9") { // See golang.org/issue/22227.
            gogo(_addr__g_.m.g0.sched);

        }
        else
 { 
            // Clear lockedExt on plan9 since we may end up re-using
            // this thread.
            _g_.m.lockedExt = 0;

        }
    }
    schedule();

}

// save updates getg().sched to refer to pc and sp so that a following
// gogo will restore pc and sp.
//
// save must not have write barriers because invoking a write barrier
// can clobber getg().sched.
//
//go:nosplit
//go:nowritebarrierrec
private static void save(System.UIntPtr pc, System.UIntPtr sp) {
    var _g_ = getg();

    if (_g_ == _g_.m.g0 || _g_ == _g_.m.gsignal) { 
        // m.g0.sched is special and must describe the context
        // for exiting the thread. mstart1 writes to it directly.
        // m.gsignal.sched should not be used at all.
        // This check makes sure save calls do not accidentally
        // run in contexts where they'd write to system g's.
        throw("save on system g not allowed");

    }
    _g_.sched.pc = pc;
    _g_.sched.sp = sp;
    _g_.sched.lr = 0;
    _g_.sched.ret = 0; 
    // We need to ensure ctxt is zero, but can't have a write
    // barrier here. However, it should always already be zero.
    // Assert that.
    if (_g_.sched.ctxt != null) {
        badctxt();
    }
}

// The goroutine g is about to enter a system call.
// Record that it's not using the cpu anymore.
// This is called only from the go syscall library and cgocall,
// not from the low-level system calls used by the runtime.
//
// Entersyscall cannot split the stack: the save must
// make g->sched refer to the caller's stack segment, because
// entersyscall is going to return immediately after.
//
// Nothing entersyscall calls can split the stack either.
// We cannot safely move the stack during an active call to syscall,
// because we do not know which of the uintptr arguments are
// really pointers (back into the stack).
// In practice, this means that we make the fast path run through
// entersyscall doing no-split things, and the slow path has to use systemstack
// to run bigger things on the system stack.
//
// reentersyscall is the entry point used by cgo callbacks, where explicitly
// saved SP and PC are restored. This is needed when exitsyscall will be called
// from a function further up in the call stack than the parent, as g->syscallsp
// must always point to a valid stack frame. entersyscall below is the normal
// entry point for syscalls, which obtains the SP and PC from the caller.
//
// Syscall tracing:
// At the start of a syscall we emit traceGoSysCall to capture the stack trace.
// If the syscall does not block, that is it, we do not emit any other events.
// If the syscall blocks (that is, P is retaken), retaker emits traceGoSysBlock;
// when syscall returns we emit traceGoSysExit and when the goroutine starts running
// (potentially instantly, if exitsyscallfast returns true) we emit traceGoStart.
// To ensure that traceGoSysExit is emitted strictly after traceGoSysBlock,
// we remember current value of syscalltick in m (_g_.m.syscalltick = _g_.m.p.ptr().syscalltick),
// whoever emits traceGoSysBlock increments p.syscalltick afterwards;
// and we wait for the increment before emitting traceGoSysExit.
// Note that the increment is done even if tracing is not enabled,
// because tracing can be enabled in the middle of syscall. We don't want the wait to hang.
//
//go:nosplit
private static void reentersyscall(System.UIntPtr pc, System.UIntPtr sp) {
    var _g_ = getg(); 

    // Disable preemption because during this function g is in Gsyscall status,
    // but can have inconsistent g->sched, do not let GC observe it.
    _g_.m.locks++; 

    // Entersyscall must not call any function that might split/grow the stack.
    // (See details in comment above.)
    // Catch calls that might, by replacing the stack guard with something that
    // will trip any stack check and leaving a flag to tell newstack to die.
    _g_.stackguard0 = stackPreempt;
    _g_.throwsplit = true; 

    // Leave SP around for GC and traceback.
    save(pc, sp);
    _g_.syscallsp = sp;
    _g_.syscallpc = pc;
    casgstatus(_addr__g_, _Grunning, _Gsyscall);
    if (_g_.syscallsp < _g_.stack.lo || _g_.stack.hi < _g_.syscallsp) {
        systemstack(() => {
            print("entersyscall inconsistent ", hex(_g_.syscallsp), " [", hex(_g_.stack.lo), ",", hex(_g_.stack.hi), "]\n");
            throw("entersyscall");
        });
    }
    if (trace.enabled) {
        systemstack(traceGoSysCall); 
        // systemstack itself clobbers g.sched.{pc,sp} and we might
        // need them later when the G is genuinely blocked in a
        // syscall
        save(pc, sp);

    }
    if (atomic.Load(_addr_sched.sysmonwait) != 0) {
        systemstack(entersyscall_sysmon);
        save(pc, sp);
    }
    if (_g_.m.p.ptr().runSafePointFn != 0) { 
        // runSafePointFn may stack split if run on this stack
        systemstack(runSafePointFn);
        save(pc, sp);

    }
    _g_.m.syscalltick = _g_.m.p.ptr().syscalltick;
    _g_.sysblocktraced = true;
    var pp = _g_.m.p.ptr();
    pp.m = 0;
    _g_.m.oldp.set(pp);
    _g_.m.p = 0;
    atomic.Store(_addr_pp.status, _Psyscall);
    if (sched.gcwaiting != 0) {
        systemstack(entersyscall_gcwait);
        save(pc, sp);
    }
    _g_.m.locks--;

}

// Standard syscall entry used by the go syscall library and normal cgo calls.
//
// This is exported via linkname to assembly in the syscall package.
//
//go:nosplit
//go:linkname entersyscall
private static void entersyscall() {
    reentersyscall(getcallerpc(), getcallersp());
}

private static void entersyscall_sysmon() {
    lock(_addr_sched.@lock);
    if (atomic.Load(_addr_sched.sysmonwait) != 0) {
        atomic.Store(_addr_sched.sysmonwait, 0);
        notewakeup(_addr_sched.sysmonnote);
    }
    unlock(_addr_sched.@lock);

}

private static void entersyscall_gcwait() {
    var _g_ = getg();
    var _p_ = _g_.m.oldp.ptr();

    lock(_addr_sched.@lock);
    if (sched.stopwait > 0 && atomic.Cas(_addr__p_.status, _Psyscall, _Pgcstop)) {
        if (trace.enabled) {
            traceGoSysBlock(_p_);
            traceProcStop(_p_);
        }
        _p_.syscalltick++;
        sched.stopwait--;

        if (sched.stopwait == 0) {
            notewakeup(_addr_sched.stopnote);
        }
    }
    unlock(_addr_sched.@lock);

}

// The same as entersyscall(), but with a hint that the syscall is blocking.
//go:nosplit
private static void entersyscallblock() {
    var _g_ = getg();

    _g_.m.locks++; // see comment in entersyscall
    _g_.throwsplit = true;
    _g_.stackguard0 = stackPreempt; // see comment in entersyscall
    _g_.m.syscalltick = _g_.m.p.ptr().syscalltick;
    _g_.sysblocktraced = true;
    _g_.m.p.ptr().syscalltick++; 

    // Leave SP around for GC and traceback.
    var pc = getcallerpc();
    var sp = getcallersp();
    save(pc, sp);
    _g_.syscallsp = _g_.sched.sp;
    _g_.syscallpc = _g_.sched.pc;
    if (_g_.syscallsp < _g_.stack.lo || _g_.stack.hi < _g_.syscallsp) {
        var sp1 = sp;
        var sp2 = _g_.sched.sp;
        var sp3 = _g_.syscallsp;
        systemstack(() => {
            print("entersyscallblock inconsistent ", hex(sp1), " ", hex(sp2), " ", hex(sp3), " [", hex(_g_.stack.lo), ",", hex(_g_.stack.hi), "]\n");
            throw("entersyscallblock");
        });
    }
    casgstatus(_addr__g_, _Grunning, _Gsyscall);
    if (_g_.syscallsp < _g_.stack.lo || _g_.stack.hi < _g_.syscallsp) {
        systemstack(() => {
            print("entersyscallblock inconsistent ", hex(sp), " ", hex(_g_.sched.sp), " ", hex(_g_.syscallsp), " [", hex(_g_.stack.lo), ",", hex(_g_.stack.hi), "]\n");
            throw("entersyscallblock");
        });
    }
    systemstack(entersyscallblock_handoff); 

    // Resave for traceback during blocked call.
    save(getcallerpc(), getcallersp());

    _g_.m.locks--;

}

private static void entersyscallblock_handoff() {
    if (trace.enabled) {
        traceGoSysCall();
        traceGoSysBlock(getg().m.p.ptr());
    }
    handoffp(_addr_releasep());

}

// The goroutine g exited its system call.
// Arrange for it to run on a cpu again.
// This is called only from the go syscall library, not
// from the low-level system calls used by the runtime.
//
// Write barriers are not allowed because our P may have been stolen.
//
// This is exported via linkname to assembly in the syscall package.
//
//go:nosplit
//go:nowritebarrierrec
//go:linkname exitsyscall
private static void exitsyscall() {
    var _g_ = getg();

    _g_.m.locks++; // see comment in entersyscall
    if (getcallersp() > _g_.syscallsp) {
        throw("exitsyscall: syscall frame is no longer valid");
    }
    _g_.waitsince = 0;
    var oldp = _g_.m.oldp.ptr();
    _g_.m.oldp = 0;
    if (exitsyscallfast(_addr_oldp)) {
        if (trace.enabled) {
            if (oldp != _g_.m.p.ptr() || _g_.m.syscalltick != _g_.m.p.ptr().syscalltick) {
                systemstack(traceGoStart);
            }
        }
        _g_.m.p.ptr().syscalltick++; 
        // We need to cas the status and scan before resuming...
        casgstatus(_addr__g_, _Gsyscall, _Grunning); 

        // Garbage collector isn't running (since we are),
        // so okay to clear syscallsp.
        _g_.syscallsp = 0;
        _g_.m.locks--;
        if (_g_.preempt) { 
            // restore the preemption request in case we've cleared it in newstack
            _g_.stackguard0 = stackPreempt;

        }
        else
 { 
            // otherwise restore the real _StackGuard, we've spoiled it in entersyscall/entersyscallblock
            _g_.stackguard0 = _g_.stack.lo + _StackGuard;

        }
        _g_.throwsplit = false;

        if (sched.disable.user && !schedEnabled(_addr__g_)) { 
            // Scheduling of this goroutine is disabled.
            Gosched();

        }
        return ;

    }
    _g_.sysexitticks = 0;
    if (trace.enabled) { 
        // Wait till traceGoSysBlock event is emitted.
        // This ensures consistency of the trace (the goroutine is started after it is blocked).
        while (oldp != null && oldp.syscalltick == _g_.m.syscalltick) {
            osyield();
        } 
        // We can't trace syscall exit right now because we don't have a P.
        // Tracing code can invoke write barriers that cannot run without a P.
        // So instead we remember the syscall exit time and emit the event
        // in execute when we have a P.
        _g_.sysexitticks = cputicks();

    }
    _g_.m.locks--; 

    // Call the scheduler.
    mcall(exitsyscall0); 

    // Scheduler returned, so we're allowed to run now.
    // Delete the syscallsp information that we left for
    // the garbage collector during the system call.
    // Must wait until now because until gosched returns
    // we don't know for sure that the garbage collector
    // is not running.
    _g_.syscallsp = 0;
    _g_.m.p.ptr().syscalltick++;
    _g_.throwsplit = false;

}

//go:nosplit
private static bool exitsyscallfast(ptr<p> _addr_oldp) {
    ref p oldp = ref _addr_oldp.val;

    var _g_ = getg(); 

    // Freezetheworld sets stopwait but does not retake P's.
    if (sched.stopwait == freezeStopWait) {
        return false;
    }
    if (oldp != null && oldp.status == _Psyscall && atomic.Cas(_addr_oldp.status, _Psyscall, _Pidle)) { 
        // There's a cpu for us, so we can run.
        wirep(_addr_oldp);
        exitsyscallfast_reacquired();
        return true;

    }
    if (sched.pidle != 0) {
        bool ok = default;
        systemstack(() => {
            ok = exitsyscallfast_pidle();
            if (ok && trace.enabled) {
                if (oldp != null) { 
                    // Wait till traceGoSysBlock event is emitted.
                    // This ensures consistency of the trace (the goroutine is started after it is blocked).
                    while (oldp.syscalltick == _g_.m.syscalltick) {
                        osyield();
                    }


                }

                traceGoSysExit(0);

            }

        });
        if (ok) {
            return true;
        }
    }
    return false;

}

// exitsyscallfast_reacquired is the exitsyscall path on which this G
// has successfully reacquired the P it was running on before the
// syscall.
//
//go:nosplit
private static void exitsyscallfast_reacquired() {
    var _g_ = getg();
    if (_g_.m.syscalltick != _g_.m.p.ptr().syscalltick) {
        if (trace.enabled) { 
            // The p was retaken and then enter into syscall again (since _g_.m.syscalltick has changed).
            // traceGoSysBlock for this syscall was already emitted,
            // but here we effectively retake the p from the new syscall running on the same p.
            systemstack(() => { 
                // Denote blocking of the new syscall.
                traceGoSysBlock(_g_.m.p.ptr()); 
                // Denote completion of the current syscall.
                traceGoSysExit(0);

            });

        }
        _g_.m.p.ptr().syscalltick++;

    }
}

private static bool exitsyscallfast_pidle() {
    lock(_addr_sched.@lock);
    var _p_ = pidleget();
    if (_p_ != null && atomic.Load(_addr_sched.sysmonwait) != 0) {
        atomic.Store(_addr_sched.sysmonwait, 0);
        notewakeup(_addr_sched.sysmonnote);
    }
    unlock(_addr_sched.@lock);
    if (_p_ != null) {
        acquirep(_addr__p_);
        return true;
    }
    return false;

}

// exitsyscall slow path on g0.
// Failed to acquire P, enqueue gp as runnable.
//
// Called via mcall, so gp is the calling g from this M.
//
//go:nowritebarrierrec
private static void exitsyscall0(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    casgstatus(_addr_gp, _Gsyscall, _Grunnable);
    dropg();
    lock(_addr_sched.@lock);
    ptr<p> _p_;
    if (schedEnabled(_addr_gp)) {
        _p_ = pidleget();
    }
    bool locked = default;
    if (_p_ == null) {
        globrunqput(_addr_gp); 

        // Below, we stoplockedm if gp is locked. globrunqput releases
        // ownership of gp, so we must check if gp is locked prior to
        // committing the release by unlocking sched.lock, otherwise we
        // could race with another M transitioning gp from unlocked to
        // locked.
        locked = gp.lockedm != 0;

    }
    else if (atomic.Load(_addr_sched.sysmonwait) != 0) {
        atomic.Store(_addr_sched.sysmonwait, 0);
        notewakeup(_addr_sched.sysmonnote);
    }
    unlock(_addr_sched.@lock);
    if (_p_ != null) {
        acquirep(_p_);
        execute(_addr_gp, false); // Never returns.
    }
    if (locked) { 
        // Wait until another thread schedules gp and so m again.
        //
        // N.B. lockedm must be this M, as this g was running on this M
        // before entersyscall.
        stoplockedm();
        execute(_addr_gp, false); // Never returns.
    }
    stopm();
    schedule(); // Never returns.
}

private static void beforefork() {
    var gp = getg().m.curg; 

    // Block signals during a fork, so that the child does not run
    // a signal handler before exec if a signal is sent to the process
    // group. See issue #18600.
    gp.m.locks++;
    sigsave(_addr_gp.m.sigmask);
    sigblock(false); 

    // This function is called before fork in syscall package.
    // Code between fork and exec must not allocate memory nor even try to grow stack.
    // Here we spoil g->_StackGuard to reliably detect any attempts to grow stack.
    // runtime_AfterFork will undo this in parent process, but not in child.
    gp.stackguard0 = stackFork;

}

// Called from syscall package before fork.
//go:linkname syscall_runtime_BeforeFork syscall.runtime_BeforeFork
//go:nosplit
private static void syscall_runtime_BeforeFork() {
    systemstack(beforefork);
}

private static void afterfork() {
    var gp = getg().m.curg; 

    // See the comments in beforefork.
    gp.stackguard0 = gp.stack.lo + _StackGuard;

    msigrestore(gp.m.sigmask);

    gp.m.locks--;

}

// Called from syscall package after fork in parent.
//go:linkname syscall_runtime_AfterFork syscall.runtime_AfterFork
//go:nosplit
private static void syscall_runtime_AfterFork() {
    systemstack(afterfork);
}

// inForkedChild is true while manipulating signals in the child process.
// This is used to avoid calling libc functions in case we are using vfork.
private static bool inForkedChild = default;

// Called from syscall package after fork in child.
// It resets non-sigignored signals to the default handler, and
// restores the signal mask in preparation for the exec.
//
// Because this might be called during a vfork, and therefore may be
// temporarily sharing address space with the parent process, this must
// not change any global variables or calling into C code that may do so.
//
//go:linkname syscall_runtime_AfterForkInChild syscall.runtime_AfterForkInChild
//go:nosplit
//go:nowritebarrierrec
private static void syscall_runtime_AfterForkInChild() { 
    // It's OK to change the global variable inForkedChild here
    // because we are going to change it back. There is no race here,
    // because if we are sharing address space with the parent process,
    // then the parent process can not be running concurrently.
    inForkedChild = true;

    clearSignalHandlers(); 

    // When we are the child we are the only thread running,
    // so we know that nothing else has changed gp.m.sigmask.
    msigrestore(getg().m.sigmask);

    inForkedChild = false;

}

// pendingPreemptSignals is the number of preemption signals
// that have been sent but not received. This is only used on Darwin.
// For #41702.
private static uint pendingPreemptSignals = default;

// Called from syscall package before Exec.
//go:linkname syscall_runtime_BeforeExec syscall.runtime_BeforeExec
private static void syscall_runtime_BeforeExec() { 
    // Prevent thread creation during exec.
    execLock.@lock(); 

    // On Darwin, wait for all pending preemption signals to
    // be received. See issue #41702.
    if (GOOS == "darwin" || GOOS == "ios") {
        while (int32(atomic.Load(_addr_pendingPreemptSignals)) > 0) {
            osyield();
        }
    }
}

// Called from syscall package after Exec.
//go:linkname syscall_runtime_AfterExec syscall.runtime_AfterExec
private static void syscall_runtime_AfterExec() {
    execLock.unlock();
}

// Allocate a new g, with a stack big enough for stacksize bytes.
private static ptr<g> malg(int stacksize) {
    ptr<g> newg = @new<g>();
    if (stacksize >= 0) {
        stacksize = round2(_StackSystem + stacksize);
        systemstack(() => {
            newg.stack = stackalloc(uint32(stacksize));
        });
        newg.stackguard0 = newg.stack.lo + _StackGuard;
        newg.stackguard1 = ~uintptr(0) * (uintptr.val)(@unsafe.Pointer(newg.stack.lo));

        0;

    }
    return _addr_newg!;

}

// Create a new g running fn with siz bytes of arguments.
// Put it on the queue of g's waiting to run.
// The compiler turns a go statement into a call to this.
//
// The stack layout of this call is unusual: it assumes that the
// arguments to pass to fn are on the stack sequentially immediately
// after &fn. Hence, they are logically part of newproc's argument
// frame, even though they don't appear in its signature (and can't
// because their types differ between call sites).
//
// This must be nosplit because this stack layout means there are
// untyped arguments in newproc's argument frame. Stack copies won't
// be able to adjust them and stack splits won't be able to copy them.
//
//go:nosplit
private static void newproc(int siz, ptr<funcval> _addr_fn) {
    ref funcval fn = ref _addr_fn.val;

    var argp = add(@unsafe.Pointer(_addr_fn), sys.PtrSize);
    var gp = getg();
    var pc = getcallerpc();
    systemstack(() => {
        var newg = newproc1(_addr_fn, argp, siz, _addr_gp, pc);

        var _p_ = getg().m.p.ptr();
        runqput(_addr__p_, _addr_newg, true);

        if (mainStarted) {
            wakep();
        }
    });

}

// Create a new g in state _Grunnable, starting at fn, with narg bytes
// of arguments starting at argp. callerpc is the address of the go
// statement that created this. The caller is responsible for adding
// the new g to the scheduler.
//
// This must run on the system stack because it's the continuation of
// newproc, which cannot split the stack.
//
//go:systemstack
private static ptr<g> newproc1(ptr<funcval> _addr_fn, unsafe.Pointer argp, int narg, ptr<g> _addr_callergp, System.UIntPtr callerpc) {
    ref funcval fn = ref _addr_fn.val;
    ref g callergp = ref _addr_callergp.val;

    if (goexperiment.RegabiDefer && narg != 0) { 
        // TODO: When we commit to GOEXPERIMENT=regabidefer,
        // rewrite the comments for newproc and newproc1.
        // newproc will no longer have a funny stack layout or
        // need to be nosplit.
        throw("go with non-empty frame");

    }
    var _g_ = getg();

    if (fn == null) {
        _g_.m.throwing = -1; // do not dump full stacks
        throw("go of nil func value");

    }
    acquirem(); // disable preemption because it can be holding p in a local var
    var siz = narg;
    siz = (siz + 7) & ~7; 

    // We could allocate a larger initial stack if necessary.
    // Not worth it: this is almost always an error.
    // 4*PtrSize: extra space added below
    // PtrSize: caller's LR (arm) or return address (x86, in gostartcall).
    if (siz >= _StackMin - 4 * sys.PtrSize - sys.PtrSize) {
        throw("newproc: function arguments too large for new goroutine");
    }
    var _p_ = _g_.m.p.ptr();
    var newg = gfget(_addr__p_);
    if (newg == null) {
        newg = malg(_StackMin);
        casgstatus(_addr_newg, _Gidle, _Gdead);
        allgadd(_addr_newg); // publishes with a g->status of Gdead so GC scanner doesn't look at uninitialized stack.
    }
    if (newg.stack.hi == 0) {
        throw("newproc1: newg missing stack");
    }
    if (readgstatus(_addr_newg) != _Gdead) {
        throw("newproc1: new g is not Gdead");
    }
    nint totalSize = 4 * sys.PtrSize + uintptr(siz) + sys.MinFrameSize; // extra space in case of reads slightly beyond frame
    totalSize += -totalSize & (sys.StackAlign - 1); // align to StackAlign
    var sp = newg.stack.hi - totalSize;
    var spArg = sp;
    if (usesLR) { 
        // caller's LR
        (uintptr.val)(@unsafe.Pointer(sp)).val;

        0;
        prepGoExitFrame(sp);
        spArg += sys.MinFrameSize;

    }
    if (narg > 0) {
        memmove(@unsafe.Pointer(spArg), argp, uintptr(narg)); 
        // This is a stack-to-stack copy. If write barriers
        // are enabled and the source stack is grey (the
        // destination is always black), then perform a
        // barrier copy. We do this *after* the memmove
        // because the destination stack may have garbage on
        // it.
        if (writeBarrier.needed && !_g_.m.curg.gcscandone) {
            var f = findfunc(fn.fn);
            var stkmap = (stackmap.val)(funcdata(f, _FUNCDATA_ArgsPointerMaps));
            if (stkmap.nbit > 0) { 
                // We're in the prologue, so it's always stack map index 0.
                var bv = stackmapdata(stkmap, 0);
                bulkBarrierBitmap(spArg, spArg, uintptr(bv.n) * sys.PtrSize, 0, bv.bytedata);

            }

        }
    }
    memclrNoHeapPointers(@unsafe.Pointer(_addr_newg.sched), @unsafe.Sizeof(newg.sched));
    newg.sched.sp = sp;
    newg.stktopsp = sp;
    newg.sched.pc = abi.FuncPCABI0(goexit) + sys.PCQuantum; // +PCQuantum so that previous instruction is in same function
    newg.sched.g = guintptr(@unsafe.Pointer(newg));
    gostartcallfn(_addr_newg.sched, fn);
    newg.gopc = callerpc;
    newg.ancestors = saveAncestors(_addr_callergp);
    newg.startpc = fn.fn;
    if (_g_.m.curg != null) {
        newg.labels = _g_.m.curg.labels;
    }
    if (isSystemGoroutine(newg, false)) {
        atomic.Xadd(_addr_sched.ngsys, +1);
    }
    newg.trackingSeq = uint8(fastrand());
    if (newg.trackingSeq % gTrackingPeriod == 0) {
        newg.tracking = true;
    }
    casgstatus(_addr_newg, _Gdead, _Grunnable);

    if (_p_.goidcache == _p_.goidcacheend) { 
        // Sched.goidgen is the last allocated id,
        // this batch must be [sched.goidgen+1, sched.goidgen+GoidCacheBatch].
        // At startup sched.goidgen=0, so main goroutine receives goid=1.
        _p_.goidcache = atomic.Xadd64(_addr_sched.goidgen, _GoidCacheBatch);
        _p_.goidcache -= _GoidCacheBatch - 1;
        _p_.goidcacheend = _p_.goidcache + _GoidCacheBatch;

    }
    newg.goid = int64(_p_.goidcache);
    _p_.goidcache++;
    if (raceenabled) {
        newg.racectx = racegostart(callerpc);
    }
    if (trace.enabled) {
        traceGoCreate(newg, newg.startpc);
    }
    releasem(_g_.m);

    return _addr_newg!;

}

// saveAncestors copies previous ancestors of the given caller g and
// includes infor for the current caller into a new set of tracebacks for
// a g being created.
private static ptr<slice<ancestorInfo>> saveAncestors(ptr<g> _addr_callergp) {
    ref g callergp = ref _addr_callergp.val;
 
    // Copy all prior info, except for the root goroutine (goid 0).
    if (debug.tracebackancestors <= 0 || callergp.goid == 0) {
        return _addr_null!;
    }
    slice<ancestorInfo> callerAncestors = default;
    if (callergp.ancestors != null) {
        callerAncestors = callergp.ancestors.val;
    }
    var n = int32(len(callerAncestors)) + 1;
    if (n > debug.tracebackancestors) {
        n = debug.tracebackancestors;
    }
    var ancestors = make_slice<ancestorInfo>(n);
    copy(ancestors[(int)1..], callerAncestors);

    array<System.UIntPtr> pcs = new array<System.UIntPtr>(_TracebackMaxFrames);
    var npcs = gcallers(callergp, 0, pcs[..]);
    var ipcs = make_slice<System.UIntPtr>(npcs);
    copy(ipcs, pcs[..]);
    ancestors[0] = new ancestorInfo(pcs:ipcs,goid:callergp.goid,gopc:callergp.gopc,);

    ptr<var> ancestorsp = @new<ancestorInfo>();
    ancestorsp.val = ancestors;
    return _addr_ancestorsp!;

}

// Put on gfree list.
// If local list is too long, transfer a batch to the global list.
private static void gfput(ptr<p> _addr__p_, ptr<g> _addr_gp) {
    ref p _p_ = ref _addr__p_.val;
    ref g gp = ref _addr_gp.val;

    if (readgstatus(_addr_gp) != _Gdead) {
        throw("gfput: bad status (not Gdead)");
    }
    var stksize = gp.stack.hi - gp.stack.lo;

    if (stksize != _FixedStack) { 
        // non-standard stack size - free it.
        stackfree(gp.stack);
        gp.stack.lo = 0;
        gp.stack.hi = 0;
        gp.stackguard0 = 0;

    }
    _p_.gFree.push(gp);
    _p_.gFree.n++;
    if (_p_.gFree.n >= 64) {
        int inc = default;        gQueue stackQ = default;        gQueue noStackQ = default;
        while (_p_.gFree.n >= 32) {
            gp = _p_.gFree.pop();
            _p_.gFree.n--;
            if (gp.stack.lo == 0) {
                noStackQ.push(gp);
            }
            else
 {
                stackQ.push(gp);
            }

            inc++;

        }
        lock(_addr_sched.gFree.@lock);
        sched.gFree.noStack.pushAll(noStackQ);
        sched.gFree.stack.pushAll(stackQ);
        sched.gFree.n += inc;
        unlock(_addr_sched.gFree.@lock);

    }
}

// Get from gfree list.
// If local list is empty, grab a batch from global list.
private static ptr<g> gfget(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;

retry:
    if (_p_.gFree.empty() && (!sched.gFree.stack.empty() || !sched.gFree.noStack.empty())) {
        lock(_addr_sched.gFree.@lock); 
        // Move a batch of free Gs to the P.
        while (_p_.gFree.n < 32) { 
            // Prefer Gs with stacks.
            var gp = sched.gFree.stack.pop();
            if (gp == null) {
                gp = sched.gFree.noStack.pop();
                if (gp == null) {
                    break;
                }
            }

            sched.gFree.n--;
            _p_.gFree.push(gp);
            _p_.gFree.n++;

        }
        unlock(_addr_sched.gFree.@lock);
        goto retry;

    }
    gp = _p_.gFree.pop();
    if (gp == null) {
        return _addr_null!;
    }
    _p_.gFree.n--;
    if (gp.stack.lo == 0) { 
        // Stack was deallocated in gfput. Allocate a new one.
        systemstack(() => {
            gp.stack = stackalloc(_FixedStack);
        }
    else
);
        gp.stackguard0 = gp.stack.lo + _StackGuard;

    } {
        if (raceenabled) {
            racemalloc(@unsafe.Pointer(gp.stack.lo), gp.stack.hi - gp.stack.lo);
        }
        if (msanenabled) {
            msanmalloc(@unsafe.Pointer(gp.stack.lo), gp.stack.hi - gp.stack.lo);
        }
    }
    return _addr_gp!;

}

// Purge all cached G's from gfree list to the global list.
private static void gfpurge(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;

    int inc = default;    gQueue stackQ = default;    gQueue noStackQ = default;
    while (!_p_.gFree.empty()) {
        var gp = _p_.gFree.pop();
        _p_.gFree.n--;
        if (gp.stack.lo == 0) {
            noStackQ.push(gp);
        }
        else
 {
            stackQ.push(gp);
        }
        inc++;

    }
    lock(_addr_sched.gFree.@lock);
    sched.gFree.noStack.pushAll(noStackQ);
    sched.gFree.stack.pushAll(stackQ);
    sched.gFree.n += inc;
    unlock(_addr_sched.gFree.@lock);

}

// Breakpoint executes a breakpoint trap.
public static void Breakpoint() {
    breakpoint();
}

// dolockOSThread is called by LockOSThread and lockOSThread below
// after they modify m.locked. Do not allow preemption during this call,
// or else the m might be different in this function than in the caller.
//go:nosplit
private static void dolockOSThread() {
    if (GOARCH == "wasm") {
        return ; // no threads on wasm yet
    }
    var _g_ = getg();
    _g_.m.lockedg.set(_g_);
    _g_.lockedm.set(_g_.m);

}

//go:nosplit

// LockOSThread wires the calling goroutine to its current operating system thread.
// The calling goroutine will always execute in that thread,
// and no other goroutine will execute in it,
// until the calling goroutine has made as many calls to
// UnlockOSThread as to LockOSThread.
// If the calling goroutine exits without unlocking the thread,
// the thread will be terminated.
//
// All init functions are run on the startup thread. Calling LockOSThread
// from an init function will cause the main function to be invoked on
// that thread.
//
// A goroutine should call LockOSThread before calling OS services or
// non-Go library functions that depend on per-thread state.
public static void LockOSThread() => func((_, panic, _) => {
    if (atomic.Load(_addr_newmHandoff.haveTemplateThread) == 0 && GOOS != "plan9") { 
        // If we need to start a new thread from the locked
        // thread, we need the template thread. Start it now
        // while we're in a known-good state.
        startTemplateThread();

    }
    var _g_ = getg();
    _g_.m.lockedExt++;
    if (_g_.m.lockedExt == 0) {
        _g_.m.lockedExt--;
        panic("LockOSThread nesting overflow");
    }
    dolockOSThread();

});

//go:nosplit
private static void lockOSThread() {
    getg().m.lockedInt++;
    dolockOSThread();
}

// dounlockOSThread is called by UnlockOSThread and unlockOSThread below
// after they update m->locked. Do not allow preemption during this call,
// or else the m might be in different in this function than in the caller.
//go:nosplit
private static void dounlockOSThread() {
    if (GOARCH == "wasm") {
        return ; // no threads on wasm yet
    }
    var _g_ = getg();
    if (_g_.m.lockedInt != 0 || _g_.m.lockedExt != 0) {
        return ;
    }
    _g_.m.lockedg = 0;
    _g_.lockedm = 0;

}

//go:nosplit

// UnlockOSThread undoes an earlier call to LockOSThread.
// If this drops the number of active LockOSThread calls on the
// calling goroutine to zero, it unwires the calling goroutine from
// its fixed operating system thread.
// If there are no active LockOSThread calls, this is a no-op.
//
// Before calling UnlockOSThread, the caller must ensure that the OS
// thread is suitable for running other goroutines. If the caller made
// any permanent changes to the state of the thread that would affect
// other goroutines, it should not call this function and thus leave
// the goroutine locked to the OS thread until the goroutine (and
// hence the thread) exits.
public static void UnlockOSThread() {
    var _g_ = getg();
    if (_g_.m.lockedExt == 0) {
        return ;
    }
    _g_.m.lockedExt--;
    dounlockOSThread();

}

//go:nosplit
private static void unlockOSThread() {
    var _g_ = getg();
    if (_g_.m.lockedInt == 0) {
        systemstack(badunlockosthread);
    }
    _g_.m.lockedInt--;
    dounlockOSThread();

}

private static void badunlockosthread() {
    throw("runtime: internal error: misuse of lockOSThread/unlockOSThread");
}

private static int gcount() {
    var n = int32(atomic.Loaduintptr(_addr_allglen)) - sched.gFree.n - int32(atomic.Load(_addr_sched.ngsys));
    foreach (var (_, _p_) in allp) {
        n -= _p_.gFree.n;
    }    if (n < 1) {
        n = 1;
    }
    return n;

}

private static int mcount() {
    return int32(sched.mnext - sched.nmfreed);
}

private static var prof = default;

private static void _System() {
    _System();
}
private static void _ExternalCode() {
    _ExternalCode();
}
private static void _LostExternalCode() {
    _LostExternalCode();
}
private static void _GC() {
    _GC();
}
private static void _LostSIGPROFDuringAtomic64() {
    _LostSIGPROFDuringAtomic64();
}
private static void _VDSO() {
    _VDSO();
}

// Called if we receive a SIGPROF signal.
// Called by the signal handler, may run during STW.
//go:nowritebarrierrec
private static void sigprof(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ptr<g> _addr_gp, ptr<m> _addr_mp) {
    ref g gp = ref _addr_gp.val;
    ref m mp = ref _addr_mp.val;

    if (prof.hz == 0) {
        return ;
    }
    if (mp != null && mp.profilehz == 0) {
        return ;
    }
    if (GOARCH == "mips" || GOARCH == "mipsle" || GOARCH == "arm") {
        {
            var f = findfunc(pc);

            if (f.valid()) {
                if (hasPrefix(funcname(f), "runtime/internal/atomic")) {
                    cpuprof.lostAtomic++;
                    return ;
                }
            }

        }

        if (GOARCH == "arm" && goarm < 7 && GOOS == "linux" && pc & 0xffff0000 == 0xffff0000) { 
            // runtime/internal/atomic functions call into kernel
            // helpers on arm < 7. See
            // runtime/internal/atomic/sys_linux_arm.s.
            cpuprof.lostAtomic++;
            return ;

        }
    }
    getg().m.mallocing++;

    array<System.UIntPtr> stk = new array<System.UIntPtr>(maxCPUProfStack);
    nint n = 0;
    if (mp.ncgo > 0 && mp.curg != null && mp.curg.syscallpc != 0 && mp.curg.syscallsp != 0) {
        nint cgoOff = 0; 
        // Check cgoCallersUse to make sure that we are not
        // interrupting other code that is fiddling with
        // cgoCallers.  We are running in a signal handler
        // with all signals blocked, so we don't have to worry
        // about any other code interrupting us.
        if (atomic.Load(_addr_mp.cgoCallersUse) == 0 && mp.cgoCallers != null && mp.cgoCallers[0] != 0) {
            while (cgoOff < len(mp.cgoCallers) && mp.cgoCallers[cgoOff] != 0) {
                cgoOff++;
            }

            copy(stk[..], mp.cgoCallers[..(int)cgoOff]);
            mp.cgoCallers[0] = 0;
        }
    else
        n = gentraceback(mp.curg.syscallpc, mp.curg.syscallsp, 0, mp.curg, 0, _addr_stk[cgoOff], len(stk) - cgoOff, null, null, 0);
        if (n > 0) {
            n += cgoOff;
        }
    } {
        n = gentraceback(pc, sp, lr, gp, 0, _addr_stk[0], len(stk), null, null, _TraceTrap | _TraceJumpStack);
    }
    if (n <= 0) { 
        // Normal traceback is impossible or has failed.
        // See if it falls into several common cases.
        n = 0;
        if (usesLibcall() && mp.libcallg != 0 && mp.libcallpc != 0 && mp.libcallsp != 0) { 
            // Libcall, i.e. runtime syscall on windows.
            // Collect Go stack that leads to the call.
            n = gentraceback(mp.libcallpc, mp.libcallsp, 0, mp.libcallg.ptr(), 0, _addr_stk[0], len(stk), null, null, 0);

        }
        if (n == 0 && mp != null && mp.vdsoSP != 0) {
            n = gentraceback(mp.vdsoPC, mp.vdsoSP, 0, gp, 0, _addr_stk[0], len(stk), null, null, _TraceTrap | _TraceJumpStack);
        }
        if (n == 0) { 
            // If all of the above has failed, account it against abstract "System" or "GC".
            n = 2;
            if (inVDSOPage(pc)) {
                pc = funcPC(_VDSO) + sys.PCQuantum;
            }
            else if (pc > firstmoduledata.etext) { 
                // "ExternalCode" is better than "etext".
                pc = funcPC(_ExternalCode) + sys.PCQuantum;

            }

            stk[0] = pc;
            if (mp.preemptoff != "") {
                stk[1] = funcPC(_GC) + sys.PCQuantum;
            }
            else
 {
                stk[1] = funcPC(_System) + sys.PCQuantum;
            }

        }
    }
    if (prof.hz != 0) {
        cpuprof.add(gp, stk[..(int)n]);
    }
    getg().m.mallocing--;

}

// If the signal handler receives a SIGPROF signal on a non-Go thread,
// it tries to collect a traceback into sigprofCallers.
// sigprofCallersUse is set to non-zero while sigprofCallers holds a traceback.
private static cgoCallers sigprofCallers = default;
private static uint sigprofCallersUse = default;

// sigprofNonGo is called if we receive a SIGPROF signal on a non-Go thread,
// and the signal handler collected a stack trace in sigprofCallers.
// When this is called, sigprofCallersUse will be non-zero.
// g is nil, and what we can do is very limited.
//go:nosplit
//go:nowritebarrierrec
private static void sigprofNonGo() {
    if (prof.hz != 0) {
        nint n = 0;
        while (n < len(sigprofCallers) && sigprofCallers[n] != 0) {
            n++;
        }
        cpuprof.addNonGo(sigprofCallers[..(int)n]);
    }
    atomic.Store(_addr_sigprofCallersUse, 0);

}

// sigprofNonGoPC is called when a profiling signal arrived on a
// non-Go thread and we have a single PC value, not a stack trace.
// g is nil, and what we can do is very limited.
//go:nosplit
//go:nowritebarrierrec
private static void sigprofNonGoPC(System.UIntPtr pc) {
    if (prof.hz != 0) {
        System.UIntPtr stk = new slice<System.UIntPtr>(new System.UIntPtr[] { pc, funcPC(_ExternalCode)+sys.PCQuantum });
        cpuprof.addNonGo(stk);
    }
}

// setcpuprofilerate sets the CPU profiling rate to hz times per second.
// If hz <= 0, setcpuprofilerate turns off CPU profiling.
private static void setcpuprofilerate(int hz) { 
    // Force sane arguments.
    if (hz < 0) {
        hz = 0;
    }
    var _g_ = getg();
    _g_.m.locks++; 

    // Stop profiler on this thread so that it is safe to lock prof.
    // if a profiling signal came in while we had prof locked,
    // it would deadlock.
    setThreadCPUProfiler(0);

    while (!atomic.Cas(_addr_prof.signalLock, 0, 1)) {
        osyield();
    }
    if (prof.hz != hz) {
        setProcessCPUProfiler(hz);
        prof.hz = hz;
    }
    atomic.Store(_addr_prof.signalLock, 0);

    lock(_addr_sched.@lock);
    sched.profilehz = hz;
    unlock(_addr_sched.@lock);

    if (hz != 0) {
        setThreadCPUProfiler(hz);
    }
    _g_.m.locks--;

}

// init initializes pp, which may be a freshly allocated p or a
// previously destroyed p, and transitions it to status _Pgcstop.
private static void init(this ptr<p> _addr_pp, int id) {
    ref p pp = ref _addr_pp.val;

    pp.id = id;
    pp.status = _Pgcstop;
    pp.sudogcache = pp.sudogbuf[..(int)0];
    foreach (var (i) in pp.deferpool) {
        pp.deferpool[i] = pp.deferpoolbuf[i][..(int)0];
    }    pp.wbBuf.reset();
    if (pp.mcache == null) {
        if (id == 0) {
            if (mcache0 == null) {
                throw("missing mcache?");
            } 
            // Use the bootstrap mcache0. Only one P will get
            // mcache0: the one with ID 0.
            pp.mcache = mcache0;

        }
        else
 {
            pp.mcache = allocmcache();
        }
    }
    if (raceenabled && pp.raceprocctx == 0) {
        if (id == 0) {
            pp.raceprocctx = raceprocctx0;
            raceprocctx0 = 0; // bootstrap
        }
        else
 {
            pp.raceprocctx = raceproccreate();
        }
    }
    lockInit(_addr_pp.timersLock, lockRankTimers); 

    // This P may get timers when it starts running. Set the mask here
    // since the P may not go through pidleget (notably P 0 on startup).
    timerpMask.set(id); 
    // Similarly, we may not go through pidleget before this P starts
    // running if it is P 0 on startup.
    idlepMask.clear(id);

}

// destroy releases all of the resources associated with pp and
// transitions it to status _Pdead.
//
// sched.lock must be held and the world must be stopped.
private static void destroy(this ptr<p> _addr_pp) {
    ref p pp = ref _addr_pp.val;

    assertLockHeld(_addr_sched.@lock);
    assertWorldStopped(); 

    // Move all runnable goroutines to the global queue
    while (pp.runqhead != pp.runqtail) { 
        // Pop from tail of local queue
        pp.runqtail--;
        var gp = pp.runq[pp.runqtail % uint32(len(pp.runq))].ptr(); 
        // Push onto head of global queue
        globrunqputhead(_addr_gp);

    }
    if (pp.runnext != 0) {
        globrunqputhead(_addr_pp.runnext.ptr());
        pp.runnext = 0;
    }
    if (len(pp.timers) > 0) {
        var plocal = getg().m.p.ptr(); 
        // The world is stopped, but we acquire timersLock to
        // protect against sysmon calling timeSleepUntil.
        // This is the only case where we hold the timersLock of
        // more than one P, so there are no deadlock concerns.
        lock(_addr_plocal.timersLock);
        lock(_addr_pp.timersLock);
        moveTimers(plocal, pp.timers);
        pp.timers = null;
        pp.numTimers = 0;
        pp.deletedTimers = 0;
        atomic.Store64(_addr_pp.timer0When, 0);
        unlock(_addr_pp.timersLock);
        unlock(_addr_plocal.timersLock);

    }
    if (gcphase != _GCoff) {
        wbBufFlush1(pp);
        pp.gcw.dispose();
    }
    {
        var i__prev1 = i;

        foreach (var (__i) in pp.sudogbuf) {
            i = __i;
            pp.sudogbuf[i] = null;
        }
        i = i__prev1;
    }

    pp.sudogcache = pp.sudogbuf[..(int)0];
    {
        var i__prev1 = i;

        foreach (var (__i) in pp.deferpool) {
            i = __i;
            foreach (var (j) in pp.deferpoolbuf[i]) {
                pp.deferpoolbuf[i][j] = null;
            }
            pp.deferpool[i] = pp.deferpoolbuf[i][..(int)0];
        }
        i = i__prev1;
    }

    systemstack(() => {
        {
            var i__prev1 = i;

            for (nint i = 0; i < pp.mspancache.len; i++) { 
                // Safe to call since the world is stopped.
                mheap_.spanalloc.free(@unsafe.Pointer(pp.mspancache.buf[i]));

            }


            i = i__prev1;
        }
        pp.mspancache.len = 0;
        lock(_addr_mheap_.@lock);
        pp.pcache.flush(_addr_mheap_.pages);
        unlock(_addr_mheap_.@lock);

    });
    freemcache(pp.mcache);
    pp.mcache = null;
    gfpurge(_addr_pp);
    traceProcFree(pp);
    if (raceenabled) {
        if (pp.timerRaceCtx != 0) { 
            // The race detector code uses a callback to fetch
            // the proc context, so arrange for that callback
            // to see the right thing.
            // This hack only works because we are the only
            // thread running.
            var mp = getg().m;
            var phold = mp.p.ptr();
            mp.p.set(pp);

            racectxend(pp.timerRaceCtx);
            pp.timerRaceCtx = 0;

            mp.p.set(phold);

        }
        raceprocdestroy(pp.raceprocctx);
        pp.raceprocctx = 0;

    }
    pp.gcAssistTime = 0;
    pp.status = _Pdead;

}

// Change number of processors.
//
// sched.lock must be held, and the world must be stopped.
//
// gcworkbufs must not be being modified by either the GC or the write barrier
// code, so the GC must not be running if the number of Ps actually changes.
//
// Returns list of Ps with local work, they need to be scheduled by the caller.
private static ptr<p> procresize(int nprocs) {
    assertLockHeld(_addr_sched.@lock);
    assertWorldStopped();

    var old = gomaxprocs;
    if (old < 0 || nprocs <= 0) {
        throw("procresize: invalid arg");
    }
    if (trace.enabled) {
        traceGomaxprocs(nprocs);
    }
    var now = nanotime();
    if (sched.procresizetime != 0) {
        sched.totaltime += int64(old) * (now - sched.procresizetime);
    }
    sched.procresizetime = now;

    var maskWords = (nprocs + 31) / 32; 

    // Grow allp if necessary.
    if (nprocs > int32(len(allp))) { 
        // Synchronize with retake, which could be running
        // concurrently since it doesn't run on a P.
        lock(_addr_allpLock);
        if (nprocs <= int32(cap(allp))) {
            allp = allp[..(int)nprocs];
        }
        else
 {
            var nallp = make_slice<ptr<p>>(nprocs); 
            // Copy everything up to allp's cap so we
            // never lose old allocated Ps.
            copy(nallp, allp[..(int)cap(allp)]);
            allp = nallp;

        }
        if (maskWords <= int32(cap(idlepMask))) {
            idlepMask = idlepMask[..(int)maskWords];
            timerpMask = timerpMask[..(int)maskWords];
        }
        else
 {
            var nidlepMask = make_slice<uint>(maskWords); 
            // No need to copy beyond len, old Ps are irrelevant.
            copy(nidlepMask, idlepMask);
            idlepMask = nidlepMask;

            var ntimerpMask = make_slice<uint>(maskWords);
            copy(ntimerpMask, timerpMask);
            timerpMask = ntimerpMask;

        }
        unlock(_addr_allpLock);

    }
    {
        var i__prev1 = i;

        for (var i = old; i < nprocs; i++) {
            var pp = allp[i];
            if (pp == null) {
                pp = @new<p>();
            }
            pp.init(i);
            atomicstorep(@unsafe.Pointer(_addr_allp[i]), @unsafe.Pointer(pp));
        }

        i = i__prev1;
    }

    var _g_ = getg();
    if (_g_.m.p != 0 && _g_.m.p.ptr().id < nprocs) { 
        // continue to use the current P
        _g_.m.p.ptr().status = _Prunning;
        _g_.m.p.ptr().mcache.prepareForSweep();

    }
    else
 { 
        // release the current P and acquire allp[0].
        //
        // We must do this before destroying our current P
        // because p.destroy itself has write barriers, so we
        // need to do that from a valid P.
        if (_g_.m.p != 0) {
            if (trace.enabled) { 
                // Pretend that we were descheduled
                // and then scheduled again to keep
                // the trace sane.
                traceGoSched();
                traceProcStop(_g_.m.p.ptr());

            }

            _g_.m.p.ptr().m = 0;

        }
        _g_.m.p = 0;
        var p = allp[0];
        p.m = 0;
        p.status = _Pidle;
        acquirep(_addr_p);
        if (trace.enabled) {
            traceGoStart();
        }
    }
    mcache0 = null; 

    // release resources from unused P's
    {
        var i__prev1 = i;

        for (i = nprocs; i < old; i++) {
            p = allp[i];
            p.destroy(); 
            // can't free P itself because it can be referenced by an M in syscall
        }

        i = i__prev1;
    } 

    // Trim allp.
    if (int32(len(allp)) != nprocs) {
        lock(_addr_allpLock);
        allp = allp[..(int)nprocs];
        idlepMask = idlepMask[..(int)maskWords];
        timerpMask = timerpMask[..(int)maskWords];
        unlock(_addr_allpLock);
    }
    ptr<p> runnablePs;
    {
        var i__prev1 = i;

        for (i = nprocs - 1; i >= 0; i--) {
            p = allp[i];
            if (_g_.m.p.ptr() == p) {
                continue;
            }
            p.status = _Pidle;
            if (runqempty(_addr_p)) {
                pidleput(_addr_p);
            }
            else
 {
                p.m.set(mget());
                p.link.set(runnablePs);
                runnablePs = p;
            }

        }

        i = i__prev1;
    }
    stealOrder.reset(uint32(nprocs));
    ptr<int> int32p_addr_gomaxprocs; // make compiler check that gomaxprocs is an int32
    atomic.Store((uint32.val)(@unsafe.Pointer(int32p)), uint32(nprocs));
    return _addr_runnablePs!;

}

// Associate p and the current m.
//
// This function is allowed to have write barriers even if the caller
// isn't because it immediately acquires _p_.
//
//go:yeswritebarrierrec
private static void acquirep(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;
 
    // Do the part that isn't allowed to have write barriers.
    wirep(_addr__p_); 

    // Have p; write barriers now allowed.

    // Perform deferred mcache flush before this P can allocate
    // from a potentially stale mcache.
    _p_.mcache.prepareForSweep();

    if (trace.enabled) {
        traceProcStart();
    }
}

// wirep is the first step of acquirep, which actually associates the
// current M to _p_. This is broken out so we can disallow write
// barriers for this part, since we don't yet have a P.
//
//go:nowritebarrierrec
//go:nosplit
private static void wirep(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;

    var _g_ = getg();

    if (_g_.m.p != 0) {
        throw("wirep: already in go");
    }
    if (_p_.m != 0 || _p_.status != _Pidle) {
        var id = int64(0);
        if (_p_.m != 0) {
            id = _p_.m.ptr().id;
        }
        print("wirep: p->m=", _p_.m, "(", id, ") p->status=", _p_.status, "\n");
        throw("wirep: invalid p state");

    }
    _g_.m.p.set(_p_);
    _p_.m.set(_g_.m);
    _p_.status = _Prunning;

}

// Disassociate p and the current m.
private static ptr<p> releasep() {
    var _g_ = getg();

    if (_g_.m.p == 0) {
        throw("releasep: invalid arg");
    }
    var _p_ = _g_.m.p.ptr();
    if (_p_.m.ptr() != _g_.m || _p_.status != _Prunning) {
        print("releasep: m=", _g_.m, " m->p=", _g_.m.p.ptr(), " p->m=", hex(_p_.m), " p->status=", _p_.status, "\n");
        throw("releasep: invalid p state");
    }
    if (trace.enabled) {
        traceProcStop(_g_.m.p.ptr());
    }
    _g_.m.p = 0;
    _p_.m = 0;
    _p_.status = _Pidle;
    return _addr__p_!;

}

private static void incidlelocked(int v) {
    lock(_addr_sched.@lock);
    sched.nmidlelocked += v;
    if (v > 0) {
        checkdead();
    }
    unlock(_addr_sched.@lock);

}

// Check for deadlock situation.
// The check is based on number of running M's, if 0 -> deadlock.
// sched.lock must be held.
private static void checkdead() {
    assertLockHeld(_addr_sched.@lock); 

    // For -buildmode=c-shared or -buildmode=c-archive it's OK if
    // there are no running goroutines. The calling program is
    // assumed to be running.
    if (islibrary || isarchive) {
        return ;
    }
    if (panicking > 0) {
        return ;
    }
    int run0 = default;
    if (!iscgo && cgoHasExtraM) {
        var mp = lockextra(true);
        var haveExtraM = extraMCount > 0;
        unlockextra(_addr_mp);
        if (haveExtraM) {
            run0 = 1;
        }
    }
    var run = mcount() - sched.nmidle - sched.nmidlelocked - sched.nmsys;
    if (run > run0) {
        return ;
    }
    if (run < 0) {
        print("runtime: checkdead: nmidle=", sched.nmidle, " nmidlelocked=", sched.nmidlelocked, " mcount=", mcount(), " nmsys=", sched.nmsys, "\n");
        throw("checkdead: inconsistent counts");
    }
    nint grunning = 0;
    forEachG(gp => {
        if (isSystemGoroutine(gp, false)) {
            return ;
        }
        var s = readgstatus(_addr_gp);

        if (s & ~_Gscan == _Gwaiting || s & ~_Gscan == _Gpreempted) 
            grunning++;
        else if (s & ~_Gscan == _Grunnable || s & ~_Gscan == _Grunning || s & ~_Gscan == _Gsyscall) 
            print("runtime: checkdead: find g ", gp.goid, " in status ", s, "\n");
            throw("checkdead: runnable g");
        
    });
    if (grunning == 0) { // possible if main goroutine calls runtime·Goexit()
        unlock(_addr_sched.@lock); // unlock so that GODEBUG=scheddetail=1 doesn't hang
        throw("no goroutines (main called runtime.Goexit) - deadlock!");

    }
    if (faketime != 0) {
        var (when, _p_) = timeSleepUntil();
        if (_p_ != null) {
            faketime = when;
            {
                var pp = _addr_sched.pidle;

                while (pp != 0.val) {
                    if ((pp.val).ptr() == _p_) {
                        pp.val = _p_.link;
                        break;
                    pp = _addr_(pp.val).ptr().link;
                    }

                }

            }
            mp = mget();
            if (mp == null) { 
                // There should always be a free M since
                // nothing is running.
                throw("checkdead: no m for timer");

            }

            mp.nextp.set(_p_);
            notewakeup(_addr_mp.park);
            return ;

        }
    }
    foreach (var (_, _p_) in allp) {
        if (len(_p_.timers) > 0) {
            return ;
        }
    }    getg().m.throwing = -1; // do not dump full stacks
    unlock(_addr_sched.@lock); // unlock so that GODEBUG=scheddetail=1 doesn't hang
    throw("all goroutines are asleep - deadlock!");

}

// forcegcperiod is the maximum time in nanoseconds between garbage
// collections. If we go this long without a garbage collection, one
// is forced to run.
//
// This is a variable for testing purposes. It normally doesn't change.
private static long forcegcperiod = 2 * 60 * 1e9F;

// Always runs without a P, so write barriers are not allowed.
//
//go:nowritebarrierrec
private static void sysmon() {
    lock(_addr_sched.@lock);
    sched.nmsys++;
    checkdead();
    unlock(_addr_sched.@lock); 

    // For syscall_runtime_doAllThreadsSyscall, sysmon is
    // sufficiently up to participate in fixups.
    atomic.Store(_addr_sched.sysmonStarting, 0);

    var lasttrace = int64(0);
    nint idle = 0; // how many cycles in succession we had not wokeup somebody
    var delay = uint32(0);

    while (true) {
        if (idle == 0) { // start with 20us sleep...
            delay = 20;

        }
        else if (idle > 50) { // start doubling the sleep after 1ms...
            delay *= 2;

        }
        if (delay > 10 * 1000) { // up to 10ms
            delay = 10 * 1000;

        }
        usleep(delay);
        mDoFixup(); 

        // sysmon should not enter deep sleep if schedtrace is enabled so that
        // it can print that information at the right time.
        //
        // It should also not enter deep sleep if there are any active P's so
        // that it can retake P's from syscalls, preempt long running G's, and
        // poll the network if all P's are busy for long stretches.
        //
        // It should wakeup from deep sleep if any P's become active either due
        // to exiting a syscall or waking up due to a timer expiring so that it
        // can resume performing those duties. If it wakes from a syscall it
        // resets idle and delay as a bet that since it had retaken a P from a
        // syscall before, it may need to do it again shortly after the
        // application starts work again. It does not reset idle when waking
        // from a timer to avoid adding system load to applications that spend
        // most of their time sleeping.
        var now = nanotime();
        if (debug.schedtrace <= 0 && (sched.gcwaiting != 0 || atomic.Load(_addr_sched.npidle) == uint32(gomaxprocs))) {
            lock(_addr_sched.@lock);
            if (atomic.Load(_addr_sched.gcwaiting) != 0 || atomic.Load(_addr_sched.npidle) == uint32(gomaxprocs)) {
                var syscallWake = false;
                var (next, _) = timeSleepUntil();
                if (next > now) {
                    atomic.Store(_addr_sched.sysmonwait, 1);
                    unlock(_addr_sched.@lock); 
                    // Make wake-up period small enough
                    // for the sampling to be correct.
                    var sleep = forcegcperiod / 2;
                    if (next - now < sleep) {
                        sleep = next - now;
                    }

                    var shouldRelax = sleep >= osRelaxMinNS;
                    if (shouldRelax) {
                        osRelax(true);
                    }

                    syscallWake = notetsleep(_addr_sched.sysmonnote, sleep);
                    mDoFixup();
                    if (shouldRelax) {
                        osRelax(false);
                    }

                    lock(_addr_sched.@lock);
                    atomic.Store(_addr_sched.sysmonwait, 0);
                    noteclear(_addr_sched.sysmonnote);

                }

                if (syscallWake) {
                    idle = 0;
                    delay = 20;
                }

            }

            unlock(_addr_sched.@lock);

        }
        lock(_addr_sched.sysmonlock); 
        // Update now in case we blocked on sysmonnote or spent a long time
        // blocked on schedlock or sysmonlock above.
        now = nanotime(); 

        // trigger libc interceptors if needed
        if (cgo_yield != null.val) {
            asmcgocall(cgo_yield.val, null);
        }
        var lastpoll = int64(atomic.Load64(_addr_sched.lastpoll));
        if (netpollinited() && lastpoll != 0 && lastpoll + 10 * 1000 * 1000 < now) {
            atomic.Cas64(_addr_sched.lastpoll, uint64(lastpoll), uint64(now));
            ref var list = ref heap(netpoll(0), out ptr<var> _addr_list); // non-blocking - returns list of goroutines
            if (!list.empty()) { 
                // Need to decrement number of idle locked M's
                // (pretending that one more is running) before injectglist.
                // Otherwise it can lead to the following situation:
                // injectglist grabs all P's but before it starts M's to run the P's,
                // another M returns from syscall, finishes running its G,
                // observes that there is no work to do and no other running M's
                // and reports deadlock.
                incidlelocked(-1);
                injectglist(_addr_list);
                incidlelocked(1);

            }

        }
        mDoFixup();
        if (GOOS == "netbsd") { 
            // netpoll is responsible for waiting for timer
            // expiration, so we typically don't have to worry
            // about starting an M to service timers. (Note that
            // sleep for timeSleepUntil above simply ensures sysmon
            // starts running again when that timer expiration may
            // cause Go code to run again).
            //
            // However, netbsd has a kernel bug that sometimes
            // misses netpollBreak wake-ups, which can lead to
            // unbounded delays servicing timers. If we detect this
            // overrun, then startm to get something to handle the
            // timer.
            //
            // See issue 42515 and
            // https://gnats.netbsd.org/cgi-bin/query-pr-single.pl?number=50094.
            {
                var next__prev2 = next;

                (next, _) = timeSleepUntil();

                if (next < now) {
                    startm(_addr_null, false);
                }

                next = next__prev2;

            }

        }
        if (atomic.Load(_addr_scavenge.sysmonWake) != 0) { 
            // Kick the scavenger awake if someone requested it.
            wakeScavenger();

        }
        if (retake(now) != 0) {
            idle = 0;
        }
        else
 {
            idle++;
        }
        {
            gcTrigger t = (new gcTrigger(kind:gcTriggerTime,now:now));

            if (t.test() && atomic.Load(_addr_forcegc.idle) != 0) {
                lock(_addr_forcegc.@lock);
                forcegc.idle = 0;
                list = default;
                list.push(forcegc.g);
                injectglist(_addr_list);
                unlock(_addr_forcegc.@lock);
            }

        }

        if (debug.schedtrace > 0 && lasttrace + int64(debug.schedtrace) * 1000000 <= now) {
            lasttrace = now;
            schedtrace(debug.scheddetail > 0);
        }
        unlock(_addr_sched.sysmonlock);

    }

}

private partial struct sysmontick {
    public uint schedtick;
    public long schedwhen;
    public uint syscalltick;
    public long syscallwhen;
}

// forcePreemptNS is the time slice given to a G before it is
// preempted.
private static readonly nint forcePreemptNS = 10 * 1000 * 1000; // 10ms

 // 10ms

private static uint retake(long now) {
    nint n = 0; 
    // Prevent allp slice changes. This lock will be completely
    // uncontended unless we're already stopping the world.
    lock(_addr_allpLock); 
    // We can't use a range loop over allp because we may
    // temporarily drop the allpLock. Hence, we need to re-fetch
    // allp each time around the loop.
    for (nint i = 0; i < len(allp); i++) {
        var _p_ = allp[i];
        if (_p_ == null) { 
            // This can happen if procresize has grown
            // allp but not yet created new Ps.
            continue;

        }
        var pd = _addr__p_.sysmontick;
        var s = _p_.status;
        var sysretake = false;
        if (s == _Prunning || s == _Psyscall) { 
            // Preempt G if it's running for too long.
            var t = int64(_p_.schedtick);
            if (int64(pd.schedtick) != t) {
                pd.schedtick = uint32(t);
                pd.schedwhen = now;
            }
            else if (pd.schedwhen + forcePreemptNS <= now) {
                preemptone(_addr__p_); 
                // In case of syscall, preemptone() doesn't
                // work, because there is no M wired to P.
                sysretake = true;

            }

        }
        if (s == _Psyscall) { 
            // Retake P from syscall if it's there for more than 1 sysmon tick (at least 20us).
            t = int64(_p_.syscalltick);
            if (!sysretake && int64(pd.syscalltick) != t) {
                pd.syscalltick = uint32(t);
                pd.syscallwhen = now;
                continue;
            } 
            // On the one hand we don't want to retake Ps if there is no other work to do,
            // but on the other hand we want to retake them eventually
            // because they can prevent the sysmon thread from deep sleep.
            if (runqempty(_addr__p_) && atomic.Load(_addr_sched.nmspinning) + atomic.Load(_addr_sched.npidle) > 0 && pd.syscallwhen + 10 * 1000 * 1000 > now) {
                continue;
            } 
            // Drop allpLock so we can take sched.lock.
            unlock(_addr_allpLock); 
            // Need to decrement number of idle locked M's
            // (pretending that one more is running) before the CAS.
            // Otherwise the M from which we retake can exit the syscall,
            // increment nmidle and report deadlock.
            incidlelocked(-1);
            if (atomic.Cas(_addr__p_.status, s, _Pidle)) {
                if (trace.enabled) {
                    traceGoSysBlock(_p_);
                    traceProcStop(_p_);
                }
                n++;
                _p_.syscalltick++;
                handoffp(_addr__p_);
            }

            incidlelocked(1);
            lock(_addr_allpLock);

        }
    }
    unlock(_addr_allpLock);
    return uint32(n);

}

// Tell all goroutines that they have been preempted and they should stop.
// This function is purely best-effort. It can fail to inform a goroutine if a
// processor just started running it.
// No locks need to be held.
// Returns true if preemption request was issued to at least one goroutine.
private static bool preemptall() {
    var res = false;
    foreach (var (_, _p_) in allp) {
        if (_p_.status != _Prunning) {
            continue;
        }
        if (preemptone(_addr__p_)) {
            res = true;
        }
    }    return res;

}

// Tell the goroutine running on processor P to stop.
// This function is purely best-effort. It can incorrectly fail to inform the
// goroutine. It can inform the wrong goroutine. Even if it informs the
// correct goroutine, that goroutine might ignore the request if it is
// simultaneously executing newstack.
// No lock needs to be held.
// Returns true if preemption request was issued.
// The actual preemption will happen at some point in the future
// and will be indicated by the gp->status no longer being
// Grunning
private static bool preemptone(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;

    var mp = _p_.m.ptr();
    if (mp == null || mp == getg().m) {
        return false;
    }
    var gp = mp.curg;
    if (gp == null || gp == mp.g0) {
        return false;
    }
    gp.preempt = true; 

    // Every call in a goroutine checks for stack overflow by
    // comparing the current stack pointer to gp->stackguard0.
    // Setting gp->stackguard0 to StackPreempt folds
    // preemption into the normal stack overflow check.
    gp.stackguard0 = stackPreempt; 

    // Request an async preemption of this P.
    if (preemptMSupported && debug.asyncpreemptoff == 0) {
        _p_.preempt = true;
        preemptM(mp);
    }
    return true;

}

private static long starttime = default;

private static void schedtrace(bool detailed) {
    var now = nanotime();
    if (starttime == 0) {
        starttime = now;
    }
    lock(_addr_sched.@lock);
    print("SCHED ", (now - starttime) / 1e6F, "ms: gomaxprocs=", gomaxprocs, " idleprocs=", sched.npidle, " threads=", mcount(), " spinningthreads=", sched.nmspinning, " idlethreads=", sched.nmidle, " runqueue=", sched.runqsize);
    if (detailed) {
        print(" gcwaiting=", sched.gcwaiting, " nmidlelocked=", sched.nmidlelocked, " stopwait=", sched.stopwait, " sysmonwait=", sched.sysmonwait, "\n");
    }
    {
        var _p___prev1 = _p_;

        foreach (var (__i, ___p_) in allp) {
            i = __i;
            _p_ = ___p_;
            var mp = _p_.m.ptr();
            var h = atomic.Load(_addr__p_.runqhead);
            var t = atomic.Load(_addr__p_.runqtail);
            if (detailed) {
                var id = int64(-1);
                if (mp != null) {
                    id = mp.id;
                }
                print("  P", i, ": status=", _p_.status, " schedtick=", _p_.schedtick, " syscalltick=", _p_.syscalltick, " m=", id, " runqsize=", t - h, " gfreecnt=", _p_.gFree.n, " timerslen=", len(_p_.timers), "\n");
            }
            else
 { 
                // In non-detailed mode format lengths of per-P run queues as:
                // [len1 len2 len3 len4]
                print(" ");
                if (i == 0) {
                    print("[");
                }

                print(t - h);
                if (i == len(allp) - 1) {
                    print("]\n");
                }

            }

        }
        _p_ = _p___prev1;
    }

    if (!detailed) {
        unlock(_addr_sched.@lock);
        return ;
    }
    {
        var mp__prev1 = mp;

        mp = allm;

        while (mp != null) {
            var _p_ = mp.p.ptr();
            var gp = mp.curg;
            var lockedg = mp.lockedg.ptr();
            var id1 = int32(-1);
            if (_p_ != null) {
                id1 = _p_.id;
            mp = mp.alllink;
            }

            var id2 = int64(-1);
            if (gp != null) {
                id2 = gp.goid;
            }

            var id3 = int64(-1);
            if (lockedg != null) {
                id3 = lockedg.goid;
            }

            print("  M", mp.id, ": p=", id1, " curg=", id2, " mallocing=", mp.mallocing, " throwing=", mp.throwing, " preemptoff=", mp.preemptoff, "" + " locks=", mp.locks, " dying=", mp.dying, " spinning=", mp.spinning, " blocked=", mp.blocked, " lockedg=", id3, "\n");

        }

        mp = mp__prev1;
    }

    forEachG(gp => {
        mp = gp.m;
        var lockedm = gp.lockedm.ptr();
        id1 = int64(-1);
        if (mp != null) {
            id1 = mp.id;
        }
        id2 = int64(-1);
        if (lockedm != null) {
            id2 = lockedm.id;
        }
        print("  G", gp.goid, ": status=", readgstatus(_addr_gp), "(", gp.waitreason.String(), ") m=", id1, " lockedm=", id2, "\n");

    });
    unlock(_addr_sched.@lock);

}

// schedEnableUser enables or disables the scheduling of user
// goroutines.
//
// This does not stop already running user goroutines, so the caller
// should first stop the world when disabling user goroutines.
private static void schedEnableUser(bool enable) {
    lock(_addr_sched.@lock);
    if (sched.disable.user == !enable) {
        unlock(_addr_sched.@lock);
        return ;
    }
    sched.disable.user = !enable;
    if (enable) {
        var n = sched.disable.n;
        sched.disable.n = 0;
        globrunqputbatch(_addr_sched.disable.runnable, n);
        unlock(_addr_sched.@lock);
        while (n != 0 && sched.npidle != 0) {
            startm(_addr_null, false);
            n--;
        }
    else


    } {
        unlock(_addr_sched.@lock);
    }
}

// schedEnabled reports whether gp should be scheduled. It returns
// false is scheduling of gp is disabled.
//
// sched.lock must be held.
private static bool schedEnabled(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    assertLockHeld(_addr_sched.@lock);

    if (sched.disable.user) {
        return isSystemGoroutine(gp, true);
    }
    return true;

}

// Put mp on midle list.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static void mput(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    assertLockHeld(_addr_sched.@lock);

    mp.schedlink = sched.midle;
    sched.midle.set(mp);
    sched.nmidle++;
    checkdead();
}

// Try to get an m from midle list.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static ptr<m> mget() {
    assertLockHeld(_addr_sched.@lock);

    var mp = sched.midle.ptr();
    if (mp != null) {
        sched.midle = mp.schedlink;
        sched.nmidle--;
    }
    return _addr_mp!;

}

// Put gp on the global runnable queue.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static void globrunqput(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    assertLockHeld(_addr_sched.@lock);

    sched.runq.pushBack(gp);
    sched.runqsize++;
}

// Put gp at the head of the global runnable queue.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static void globrunqputhead(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    assertLockHeld(_addr_sched.@lock);

    sched.runq.push(gp);
    sched.runqsize++;
}

// Put a batch of runnable goroutines on the global runnable queue.
// This clears *batch.
// sched.lock must be held.
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static void globrunqputbatch(ptr<gQueue> _addr_batch, int n) {
    ref gQueue batch = ref _addr_batch.val;

    assertLockHeld(_addr_sched.@lock);

    sched.runq.pushBackAll(batch);
    sched.runqsize += n;
    batch = new gQueue();
}

// Try get a batch of G's from the global runnable queue.
// sched.lock must be held.
private static ptr<g> globrunqget(ptr<p> _addr__p_, int max) {
    ref p _p_ = ref _addr__p_.val;

    assertLockHeld(_addr_sched.@lock);

    if (sched.runqsize == 0) {
        return _addr_null!;
    }
    var n = sched.runqsize / gomaxprocs + 1;
    if (n > sched.runqsize) {
        n = sched.runqsize;
    }
    if (max > 0 && n > max) {
        n = max;
    }
    if (n > int32(len(_p_.runq)) / 2) {
        n = int32(len(_p_.runq)) / 2;
    }
    sched.runqsize -= n;

    var gp = sched.runq.pop();
    n--;
    while (n > 0) {
        var gp1 = sched.runq.pop();
        runqput(_addr__p_, _addr_gp1, false);
        n--;
    }
    return _addr_gp!;

}

// pMask is an atomic bitstring with one bit per P.
private partial struct pMask { // : slice<uint>
}

// read returns true if P id's bit is set.
private static bool read(this pMask p, uint id) {
    var word = id / 32;
    var mask = uint32(1) << (int)((id % 32));
    return (atomic.Load(_addr_p[word]) & mask) != 0;
}

// set sets P id's bit.
private static void set(this pMask p, int id) {
    var word = id / 32;
    var mask = uint32(1) << (int)((id % 32));
    atomic.Or(_addr_p[word], mask);
}

// clear clears P id's bit.
private static void clear(this pMask p, int id) {
    var word = id / 32;
    var mask = uint32(1) << (int)((id % 32));
    atomic.And(_addr_p[word], ~mask);
}

// updateTimerPMask clears pp's timer mask if it has no timers on its heap.
//
// Ideally, the timer mask would be kept immediately consistent on any timer
// operations. Unfortunately, updating a shared global data structure in the
// timer hot path adds too much overhead in applications frequently switching
// between no timers and some timers.
//
// As a compromise, the timer mask is updated only on pidleget / pidleput. A
// running P (returned by pidleget) may add a timer at any time, so its mask
// must be set. An idle P (passed to pidleput) cannot add new timers while
// idle, so if it has no timers at that time, its mask may be cleared.
//
// Thus, we get the following effects on timer-stealing in findrunnable:
//
// * Idle Ps with no timers when they go idle are never checked in findrunnable
//   (for work- or timer-stealing; this is the ideal case).
// * Running Ps must always be checked.
// * Idle Ps whose timers are stolen must continue to be checked until they run
//   again, even after timer expiration.
//
// When the P starts running again, the mask should be set, as a timer may be
// added at any time.
//
// TODO(prattmic): Additional targeted updates may improve the above cases.
// e.g., updating the mask when stealing a timer.
private static void updateTimerPMask(ptr<p> _addr_pp) {
    ref p pp = ref _addr_pp.val;

    if (atomic.Load(_addr_pp.numTimers) > 0) {
        return ;
    }
    lock(_addr_pp.timersLock);
    if (atomic.Load(_addr_pp.numTimers) == 0) {
        timerpMask.clear(pp.id);
    }
    unlock(_addr_pp.timersLock);

}

// pidleput puts p to on the _Pidle list.
//
// This releases ownership of p. Once sched.lock is released it is no longer
// safe to use p.
//
// sched.lock must be held.
//
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static void pidleput(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;

    assertLockHeld(_addr_sched.@lock);

    if (!runqempty(_addr__p_)) {
        throw("pidleput: P has non-empty run queue");
    }
    updateTimerPMask(_addr__p_); // clear if there are no timers.
    idlepMask.set(_p_.id);
    _p_.link = sched.pidle;
    sched.pidle.set(_p_);
    atomic.Xadd(_addr_sched.npidle, 1); // TODO: fast atomic
}

// pidleget tries to get a p from the _Pidle list, acquiring ownership.
//
// sched.lock must be held.
//
// May run during STW, so write barriers are not allowed.
//go:nowritebarrierrec
private static ptr<p> pidleget() {
    assertLockHeld(_addr_sched.@lock);

    var _p_ = sched.pidle.ptr();
    if (_p_ != null) { 
        // Timer may get added at any time now.
        timerpMask.set(_p_.id);
        idlepMask.clear(_p_.id);
        sched.pidle = _p_.link;
        atomic.Xadd(_addr_sched.npidle, -1); // TODO: fast atomic
    }
    return _addr__p_!;

}

// runqempty reports whether _p_ has no Gs on its local run queue.
// It never returns true spuriously.
private static bool runqempty(ptr<p> _addr__p_) {
    ref p _p_ = ref _addr__p_.val;
 
    // Defend against a race where 1) _p_ has G1 in runqnext but runqhead == runqtail,
    // 2) runqput on _p_ kicks G1 to the runq, 3) runqget on _p_ empties runqnext.
    // Simply observing that runqhead == runqtail and then observing that runqnext == nil
    // does not mean the queue is empty.
    while (true) {
        var head = atomic.Load(_addr__p_.runqhead);
        var tail = atomic.Load(_addr__p_.runqtail);
        var runnext = atomic.Loaduintptr((uintptr.val)(@unsafe.Pointer(_addr__p_.runnext)));
        if (tail == atomic.Load(_addr__p_.runqtail)) {
            return head == tail && runnext == 0;
        }
    }

}

// To shake out latent assumptions about scheduling order,
// we introduce some randomness into scheduling decisions
// when running with the race detector.
// The need for this was made obvious by changing the
// (deterministic) scheduling order in Go 1.5 and breaking
// many poorly-written tests.
// With the randomness here, as long as the tests pass
// consistently with -race, they shouldn't have latent scheduling
// assumptions.
private static readonly var randomizeScheduler = raceenabled;

// runqput tries to put g on the local runnable queue.
// If next is false, runqput adds g to the tail of the runnable queue.
// If next is true, runqput puts g in the _p_.runnext slot.
// If the run queue is full, runnext puts g on the global queue.
// Executed only by the owner P.


// runqput tries to put g on the local runnable queue.
// If next is false, runqput adds g to the tail of the runnable queue.
// If next is true, runqput puts g in the _p_.runnext slot.
// If the run queue is full, runnext puts g on the global queue.
// Executed only by the owner P.
private static void runqput(ptr<p> _addr__p_, ptr<g> _addr_gp, bool next) {
    ref p _p_ = ref _addr__p_.val;
    ref g gp = ref _addr_gp.val;

    if (randomizeScheduler && next && fastrand() % 2 == 0) {
        next = false;
    }
    if (next) {
retryNext:
        var oldnext = _p_.runnext;
        if (!_p_.runnext.cas(oldnext, guintptr(@unsafe.Pointer(gp)))) {
            goto retryNext;
        }
        if (oldnext == 0) {
            return ;
        }
        gp = oldnext.ptr();

    }
retry: // load-acquire, synchronize with consumers
    var h = atomic.LoadAcq(_addr__p_.runqhead); // load-acquire, synchronize with consumers
    var t = _p_.runqtail;
    if (t - h < uint32(len(_p_.runq))) {
        _p_.runq[t % uint32(len(_p_.runq))].set(gp);
        atomic.StoreRel(_addr__p_.runqtail, t + 1); // store-release, makes the item available for consumption
        return ;

    }
    if (runqputslow(_addr__p_, _addr_gp, h, t)) {
        return ;
    }
    goto retry;

}

// Put g and a batch of work from local runnable queue on global queue.
// Executed only by the owner P.
private static bool runqputslow(ptr<p> _addr__p_, ptr<g> _addr_gp, uint h, uint t) {
    ref p _p_ = ref _addr__p_.val;
    ref g gp = ref _addr_gp.val;

    array<ptr<g>> batch = new array<ptr<g>>(len(_p_.runq) / 2 + 1); 

    // First, grab a batch from local queue.
    var n = t - h;
    n = n / 2;
    if (n != uint32(len(_p_.runq) / 2)) {
        throw("runqputslow: queue is not full");
    }
    {
        var i__prev1 = i;

        for (var i = uint32(0); i < n; i++) {
            batch[i] = _p_.runq[(h + i) % uint32(len(_p_.runq))].ptr();
        }

        i = i__prev1;
    }
    if (!atomic.CasRel(_addr__p_.runqhead, h, h + n)) { // cas-release, commits consume
        return false;

    }
    batch[n] = gp;

    if (randomizeScheduler) {
        {
            var i__prev1 = i;

            for (i = uint32(1); i <= n; i++) {
                var j = fastrandn(i + 1);
                (batch[i], batch[j]) = (batch[j], batch[i]);
            }


            i = i__prev1;
        }

    }
    {
        var i__prev1 = i;

        for (i = uint32(0); i < n; i++) {
            batch[i].schedlink.set(batch[i + 1]);
        }

        i = i__prev1;
    }
    ref gQueue q = ref heap(out ptr<gQueue> _addr_q);
    q.head.set(batch[0]);
    q.tail.set(batch[n]); 

    // Now put the batch on global queue.
    lock(_addr_sched.@lock);
    globrunqputbatch(_addr_q, int32(n + 1));
    unlock(_addr_sched.@lock);
    return true;

}

// runqputbatch tries to put all the G's on q on the local runnable queue.
// If the queue is full, they are put on the global queue; in that case
// this will temporarily acquire the scheduler lock.
// Executed only by the owner P.
private static void runqputbatch(ptr<p> _addr_pp, ptr<gQueue> _addr_q, nint qsize) {
    ref p pp = ref _addr_pp.val;
    ref gQueue q = ref _addr_q.val;

    var h = atomic.LoadAcq(_addr_pp.runqhead);
    var t = pp.runqtail;
    var n = uint32(0);
    while (!q.empty() && t - h < uint32(len(pp.runq))) {
        var gp = q.pop();
        pp.runq[t % uint32(len(pp.runq))].set(gp);
        t++;
        n++;
    }
    qsize -= int(n);

    if (randomizeScheduler) {
        Func<uint, uint> off = o => {
            return (pp.runqtail + o) % uint32(len(pp.runq));
        };
        for (var i = uint32(1); i < n; i++) {
            var j = fastrandn(i + 1);
            (pp.runq[off(i)], pp.runq[off(j)]) = (pp.runq[off(j)], pp.runq[off(i)]);
        }

    }
    atomic.StoreRel(_addr_pp.runqtail, t);
    if (!q.empty()) {
        lock(_addr_sched.@lock);
        globrunqputbatch(_addr_q, int32(qsize));
        unlock(_addr_sched.@lock);
    }
}

// Get g from local runnable queue.
// If inheritTime is true, gp should inherit the remaining time in the
// current time slice. Otherwise, it should start a new time slice.
// Executed only by the owner P.
private static (ptr<g>, bool) runqget(ptr<p> _addr__p_) {
    ptr<g> gp = default!;
    bool inheritTime = default;
    ref p _p_ = ref _addr__p_.val;
 
    // If there's a runnext, it's the next G to run.
    while (true) {
        var next = _p_.runnext;
        if (next == 0) {
            break;
        }
        if (_p_.runnext.cas(next, 0)) {
            return (_addr_next.ptr()!, true);
        }
    }

    while (true) {
        var h = atomic.LoadAcq(_addr__p_.runqhead); // load-acquire, synchronize with other consumers
        var t = _p_.runqtail;
        if (t == h) {
            return (_addr_null!, false);
        }
        var gp = _p_.runq[h % uint32(len(_p_.runq))].ptr();
        if (atomic.CasRel(_addr__p_.runqhead, h, h + 1)) { // cas-release, commits consume
            return (_addr_gp!, false);

        }
    }

}

// runqdrain drains the local runnable queue of _p_ and returns all goroutines in it.
// Executed only by the owner P.
private static (gQueue, uint) runqdrain(ptr<p> _addr__p_) {
    gQueue drainQ = default;
    uint n = default;
    ref p _p_ = ref _addr__p_.val;

    var oldNext = _p_.runnext;
    if (oldNext != 0 && _p_.runnext.cas(oldNext, 0)) {
        drainQ.pushBack(oldNext.ptr());
        n++;
    }
retry: // load-acquire, synchronize with other consumers
    var h = atomic.LoadAcq(_addr__p_.runqhead); // load-acquire, synchronize with other consumers
    var t = _p_.runqtail;
    var qn = t - h;
    if (qn == 0) {
        return ;
    }
    if (qn > uint32(len(_p_.runq))) { // read inconsistent h and t
        goto retry;

    }
    if (!atomic.CasRel(_addr__p_.runqhead, h, h + qn)) { // cas-release, commits consume
        goto retry;

    }
    for (var i = uint32(0); i < qn; i++) {
        var gp = _p_.runq[(h + i) % uint32(len(_p_.runq))].ptr();
        drainQ.pushBack(gp);
        n++;
    }
    return ;

}

// Grabs a batch of goroutines from _p_'s runnable queue into batch.
// Batch is a ring buffer starting at batchHead.
// Returns number of grabbed goroutines.
// Can be executed by any P.
private static uint runqgrab(ptr<p> _addr__p_, ptr<array<guintptr>> _addr_batch, uint batchHead, bool stealRunNextG) {
    ref p _p_ = ref _addr__p_.val;
    ref array<guintptr> batch = ref _addr_batch.val;

    while (true) {
        var h = atomic.LoadAcq(_addr__p_.runqhead); // load-acquire, synchronize with other consumers
        var t = atomic.LoadAcq(_addr__p_.runqtail); // load-acquire, synchronize with the producer
        var n = t - h;
        n = n - n / 2;
        if (n == 0) {
            if (stealRunNextG) { 
                // Try to steal from _p_.runnext.
                {
                    var next = _p_.runnext;

                    if (next != 0) {
                        if (_p_.status == _Prunning) { 
                            // Sleep to ensure that _p_ isn't about to run the g
                            // we are about to steal.
                            // The important use case here is when the g running
                            // on _p_ ready()s another g and then almost
                            // immediately blocks. Instead of stealing runnext
                            // in this window, back off to give _p_ a chance to
                            // schedule runnext. This will avoid thrashing gs
                            // between different Ps.
                            // A sync chan send/recv takes ~50ns as of time of
                            // writing, so 3us gives ~50x overshoot.
                            if (GOOS != "windows") {
                                usleep(3);
                            }
                            else
 { 
                                // On windows system timer granularity is
                                // 1-15ms, which is way too much for this
                                // optimization. So just yield.
                                osyield();

                            }

                        }

                        if (!_p_.runnext.cas(next, 0)) {
                            continue;
                        }

                        batch[batchHead % uint32(len(batch))] = next;
                        return 1;

                    }

                }

            }

            return 0;

        }
        if (n > uint32(len(_p_.runq) / 2)) { // read inconsistent h and t
            continue;

        }
        for (var i = uint32(0); i < n; i++) {
            var g = _p_.runq[(h + i) % uint32(len(_p_.runq))];
            batch[(batchHead + i) % uint32(len(batch))] = g;
        }
        if (atomic.CasRel(_addr__p_.runqhead, h, h + n)) { // cas-release, commits consume
            return n;

        }
    }

}

// Steal half of elements from local runnable queue of p2
// and put onto local runnable queue of p.
// Returns one of the stolen elements (or nil if failed).
private static ptr<g> runqsteal(ptr<p> _addr__p_, ptr<p> _addr_p2, bool stealRunNextG) {
    ref p _p_ = ref _addr__p_.val;
    ref p p2 = ref _addr_p2.val;

    var t = _p_.runqtail;
    var n = runqgrab(_addr_p2, _addr__p_.runq, t, stealRunNextG);
    if (n == 0) {
        return _addr_null!;
    }
    n--;
    var gp = _p_.runq[(t + n) % uint32(len(_p_.runq))].ptr();
    if (n == 0) {
        return _addr_gp!;
    }
    var h = atomic.LoadAcq(_addr__p_.runqhead); // load-acquire, synchronize with consumers
    if (t - h + n >= uint32(len(_p_.runq))) {
        throw("runqsteal: runq overflow");
    }
    atomic.StoreRel(_addr__p_.runqtail, t + n); // store-release, makes the item available for consumption
    return _addr_gp!;

}

// A gQueue is a dequeue of Gs linked through g.schedlink. A G can only
// be on one gQueue or gList at a time.
private partial struct gQueue {
    public guintptr head;
    public guintptr tail;
}

// empty reports whether q is empty.
private static bool empty(this ptr<gQueue> _addr_q) {
    ref gQueue q = ref _addr_q.val;

    return q.head == 0;
}

// push adds gp to the head of q.
private static void push(this ptr<gQueue> _addr_q, ptr<g> _addr_gp) {
    ref gQueue q = ref _addr_q.val;
    ref g gp = ref _addr_gp.val;

    gp.schedlink = q.head;
    q.head.set(gp);
    if (q.tail == 0) {
        q.tail.set(gp);
    }
}

// pushBack adds gp to the tail of q.
private static void pushBack(this ptr<gQueue> _addr_q, ptr<g> _addr_gp) {
    ref gQueue q = ref _addr_q.val;
    ref g gp = ref _addr_gp.val;

    gp.schedlink = 0;
    if (q.tail != 0) {
        q.tail.ptr().schedlink.set(gp);
    }
    else
 {
        q.head.set(gp);
    }
    q.tail.set(gp);

}

// pushBackAll adds all Gs in l2 to the tail of q. After this q2 must
// not be used.
private static void pushBackAll(this ptr<gQueue> _addr_q, gQueue q2) {
    ref gQueue q = ref _addr_q.val;

    if (q2.tail == 0) {
        return ;
    }
    q2.tail.ptr().schedlink = 0;
    if (q.tail != 0) {
        q.tail.ptr().schedlink = q2.head;
    }
    else
 {
        q.head = q2.head;
    }
    q.tail = q2.tail;

}

// pop removes and returns the head of queue q. It returns nil if
// q is empty.
private static ptr<g> pop(this ptr<gQueue> _addr_q) {
    ref gQueue q = ref _addr_q.val;

    var gp = q.head.ptr();
    if (gp != null) {
        q.head = gp.schedlink;
        if (q.head == 0) {
            q.tail = 0;
        }
    }
    return _addr_gp!;

}

// popList takes all Gs in q and returns them as a gList.
private static gList popList(this ptr<gQueue> _addr_q) {
    ref gQueue q = ref _addr_q.val;

    gList stack = new gList(q.head);
    q.val = new gQueue();
    return stack;
}

// A gList is a list of Gs linked through g.schedlink. A G can only be
// on one gQueue or gList at a time.
private partial struct gList {
    public guintptr head;
}

// empty reports whether l is empty.
private static bool empty(this ptr<gList> _addr_l) {
    ref gList l = ref _addr_l.val;

    return l.head == 0;
}

// push adds gp to the head of l.
private static void push(this ptr<gList> _addr_l, ptr<g> _addr_gp) {
    ref gList l = ref _addr_l.val;
    ref g gp = ref _addr_gp.val;

    gp.schedlink = l.head;
    l.head.set(gp);
}

// pushAll prepends all Gs in q to l.
private static void pushAll(this ptr<gList> _addr_l, gQueue q) {
    ref gList l = ref _addr_l.val;

    if (!q.empty()) {
        q.tail.ptr().schedlink = l.head;
        l.head = q.head;
    }
}

// pop removes and returns the head of l. If l is empty, it returns nil.
private static ptr<g> pop(this ptr<gList> _addr_l) {
    ref gList l = ref _addr_l.val;

    var gp = l.head.ptr();
    if (gp != null) {
        l.head = gp.schedlink;
    }
    return _addr_gp!;

}

//go:linkname setMaxThreads runtime/debug.setMaxThreads
private static nint setMaxThreads(nint @in) {
    nint @out = default;

    lock(_addr_sched.@lock);
    out = int(sched.maxmcount);
    if (in > 0x7fffffff) { // MaxInt32
        sched.maxmcount = 0x7fffffff;

    }
    else
 {
        sched.maxmcount = int32(in);
    }
    checkmcount();
    unlock(_addr_sched.@lock);
    return ;

}

//go:nosplit
private static nint procPin() {
    var _g_ = getg();
    var mp = _g_.m;

    mp.locks++;
    return int(mp.p.ptr().id);
}

//go:nosplit
private static void procUnpin() {
    var _g_ = getg();
    _g_.m.locks--;
}

//go:linkname sync_runtime_procPin sync.runtime_procPin
//go:nosplit
private static nint sync_runtime_procPin() {
    return procPin();
}

//go:linkname sync_runtime_procUnpin sync.runtime_procUnpin
//go:nosplit
private static void sync_runtime_procUnpin() {
    procUnpin();
}

//go:linkname sync_atomic_runtime_procPin sync/atomic.runtime_procPin
//go:nosplit
private static nint sync_atomic_runtime_procPin() {
    return procPin();
}

//go:linkname sync_atomic_runtime_procUnpin sync/atomic.runtime_procUnpin
//go:nosplit
private static void sync_atomic_runtime_procUnpin() {
    procUnpin();
}

// Active spinning for sync.Mutex.
//go:linkname sync_runtime_canSpin sync.runtime_canSpin
//go:nosplit
private static bool sync_runtime_canSpin(nint i) { 
    // sync.Mutex is cooperative, so we are conservative with spinning.
    // Spin only few times and only if running on a multicore machine and
    // GOMAXPROCS>1 and there is at least one other running P and local runq is empty.
    // As opposed to runtime mutex we don't do passive spinning here,
    // because there can be work on global runq or on other Ps.
    if (i >= active_spin || ncpu <= 1 || gomaxprocs <= int32(sched.npidle + sched.nmspinning) + 1) {
        return false;
    }
    {
        var p = getg().m.p.ptr();

        if (!runqempty(_addr_p)) {
            return false;
        }
    }

    return true;

}

//go:linkname sync_runtime_doSpin sync.runtime_doSpin
//go:nosplit
private static void sync_runtime_doSpin() {
    procyield(active_spin_cnt);
}

private static randomOrder stealOrder = default;

// randomOrder/randomEnum are helper types for randomized work stealing.
// They allow to enumerate all Ps in different pseudo-random orders without repetitions.
// The algorithm is based on the fact that if we have X such that X and GOMAXPROCS
// are coprime, then a sequences of (i + X) % GOMAXPROCS gives the required enumeration.
private partial struct randomOrder {
    public uint count;
    public slice<uint> coprimes;
}

private partial struct randomEnum {
    public uint i;
    public uint count;
    public uint pos;
    public uint inc;
}

private static void reset(this ptr<randomOrder> _addr_ord, uint count) {
    ref randomOrder ord = ref _addr_ord.val;

    ord.count = count;
    ord.coprimes = ord.coprimes[..(int)0];
    for (var i = uint32(1); i <= count; i++) {
        if (gcd(i, count) == 1) {
            ord.coprimes = append(ord.coprimes, i);
        }
    }

}

private static randomEnum start(this ptr<randomOrder> _addr_ord, uint i) {
    ref randomOrder ord = ref _addr_ord.val;

    return new randomEnum(count:ord.count,pos:i%ord.count,inc:ord.coprimes[i%uint32(len(ord.coprimes))],);
}

private static bool done(this ptr<randomEnum> _addr_@enum) {
    ref randomEnum @enum = ref _addr_@enum.val;

    return @enum.i == @enum.count;
}

private static void next(this ptr<randomEnum> _addr_@enum) {
    ref randomEnum @enum = ref _addr_@enum.val;

    @enum.i++;
    @enum.pos = (@enum.pos + @enum.inc) % @enum.count;
}

private static uint position(this ptr<randomEnum> _addr_@enum) {
    ref randomEnum @enum = ref _addr_@enum.val;

    return @enum.pos;
}

private static uint gcd(uint a, uint b) {
    while (b != 0) {
        (a, b) = (b, a % b);
    }
    return a;

}

// An initTask represents the set of initializations that need to be done for a package.
// Keep in sync with ../../test/initempty.go:initTask
private partial struct initTask {
    public System.UIntPtr state; // 0 = uninitialized, 1 = in progress, 2 = done
    public System.UIntPtr ndeps;
    public System.UIntPtr nfns; // followed by ndeps instances of an *initTask, one per package depended on
// followed by nfns pcs, one per init function to run
}

// inittrace stores statistics for init functions which are
// updated by malloc and newproc when active is true.
private static tracestat inittrace = default;

private partial struct tracestat {
    public bool active; // init tracing activation status
    public long id; // init goroutine id
    public ulong allocs; // heap allocations
    public ulong bytes; // heap allocated bytes
}

private static void doInit(ptr<initTask> _addr_t) {
    ref initTask t = ref _addr_t.val;

    switch (t.state) {
        case 2: // fully initialized
            return ;
            break;
        case 1: // initialization in progress
            throw("recursive call during initialization - linker skew");
            break;
        default: // not initialized yet
            t.state = 1; // initialization in progress

            {
                var i__prev1 = i;

                for (var i = uintptr(0); i < t.ndeps; i++) {
                    ref var p = ref heap(add(@unsafe.Pointer(t), (3 + i) * sys.PtrSize), out ptr<var> _addr_p);
                    ptr<ptr<ptr<initTask>>> t2 = new ptr<ptr<ptr<ptr<initTask>>>>(p);
                    doInit(t2);
                }


                i = i__prev1;
            }

            if (t.nfns == 0) {
                t.state = 2; // initialization done
                return ;

            }
            long start = default;        tracestat before = default;

            if (inittrace.active) {
                start = nanotime(); 
                // Load stats non-atomically since tracinit is updated only by this init goroutine.
                before = inittrace;

            }
            var firstFunc = add(@unsafe.Pointer(t), (3 + t.ndeps) * sys.PtrSize);
            {
                var i__prev1 = i;

                for (i = uintptr(0); i < t.nfns; i++) {
                    p = add(firstFunc, i * sys.PtrSize);
                    object f = Action(@unsafe.Pointer(_addr_p)).val;
                    f();
                }


                i = i__prev1;
            }

            if (inittrace.active) {
                var end = nanotime(); 
                // Load stats non-atomically since tracinit is updated only by this init goroutine.
                var after = inittrace;

                var pkg = funcpkgpath(findfunc(funcPC(firstFunc)));

                array<byte> sbuf = new array<byte>(24);
                print("init ", pkg, " @");
                print(string(fmtNSAsMS(sbuf[..], uint64(start - runtimeInitTime))), " ms, ");
                print(string(fmtNSAsMS(sbuf[..], uint64(end - start))), " ms clock, ");
                print(string(itoa(sbuf[..], after.bytes - before.bytes)), " bytes, ");
                print(string(itoa(sbuf[..], after.allocs - before.allocs)), " allocs");
                print("\n");

            }
            t.state = 2; // initialization done
            break;
    }

}

} // end runtime_package
