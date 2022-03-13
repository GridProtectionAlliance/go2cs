// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector (GC).
//
// The GC runs concurrently with mutator threads, is type accurate (aka precise), allows multiple
// GC thread to run in parallel. It is a concurrent mark and sweep that uses a write barrier. It is
// non-generational and non-compacting. Allocation is done using size segregated per P allocation
// areas to minimize fragmentation while eliminating locks in the common case.
//
// The algorithm decomposes into several steps.
// This is a high level description of the algorithm being used. For an overview of GC a good
// place to start is Richard Jones' gchandbook.org.
//
// The algorithm's intellectual heritage includes Dijkstra's on-the-fly algorithm, see
// Edsger W. Dijkstra, Leslie Lamport, A. J. Martin, C. S. Scholten, and E. F. M. Steffens. 1978.
// On-the-fly garbage collection: an exercise in cooperation. Commun. ACM 21, 11 (November 1978),
// 966-975.
// For journal quality proofs that these steps are complete, correct, and terminate see
// Hudson, R., and Moss, J.E.B. Copying Garbage Collection without stopping the world.
// Concurrency and Computation: Practice and Experience 15(3-5), 2003.
//
// 1. GC performs sweep termination.
//
//    a. Stop the world. This causes all Ps to reach a GC safe-point.
//
//    b. Sweep any unswept spans. There will only be unswept spans if
//    this GC cycle was forced before the expected time.
//
// 2. GC performs the mark phase.
//
//    a. Prepare for the mark phase by setting gcphase to _GCmark
//    (from _GCoff), enabling the write barrier, enabling mutator
//    assists, and enqueueing root mark jobs. No objects may be
//    scanned until all Ps have enabled the write barrier, which is
//    accomplished using STW.
//
//    b. Start the world. From this point, GC work is done by mark
//    workers started by the scheduler and by assists performed as
//    part of allocation. The write barrier shades both the
//    overwritten pointer and the new pointer value for any pointer
//    writes (see mbarrier.go for details). Newly allocated objects
//    are immediately marked black.
//
//    c. GC performs root marking jobs. This includes scanning all
//    stacks, shading all globals, and shading any heap pointers in
//    off-heap runtime data structures. Scanning a stack stops a
//    goroutine, shades any pointers found on its stack, and then
//    resumes the goroutine.
//
//    d. GC drains the work queue of grey objects, scanning each grey
//    object to black and shading all pointers found in the object
//    (which in turn may add those pointers to the work queue).
//
//    e. Because GC work is spread across local caches, GC uses a
//    distributed termination algorithm to detect when there are no
//    more root marking jobs or grey objects (see gcMarkDone). At this
//    point, GC transitions to mark termination.
//
// 3. GC performs mark termination.
//
//    a. Stop the world.
//
//    b. Set gcphase to _GCmarktermination, and disable workers and
//    assists.
//
//    c. Perform housekeeping like flushing mcaches.
//
// 4. GC performs the sweep phase.
//
//    a. Prepare for the sweep phase by setting gcphase to _GCoff,
//    setting up sweep state and disabling the write barrier.
//
//    b. Start the world. From this point on, newly allocated objects
//    are white, and allocating sweeps spans before use if necessary.
//
//    c. GC does concurrent sweeping in the background and in response
//    to allocation. See description below.
//
// 5. When sufficient allocation has taken place, replay the sequence
// starting with 1 above. See discussion of GC rate below.

// Concurrent sweep.
//
// The sweep phase proceeds concurrently with normal program execution.
// The heap is swept span-by-span both lazily (when a goroutine needs another span)
// and concurrently in a background goroutine (this helps programs that are not CPU bound).
// At the end of STW mark termination all spans are marked as "needs sweeping".
//
// The background sweeper goroutine simply sweeps spans one-by-one.
//
// To avoid requesting more OS memory while there are unswept spans, when a
// goroutine needs another span, it first attempts to reclaim that much memory
// by sweeping. When a goroutine needs to allocate a new small-object span, it
// sweeps small-object spans for the same object size until it frees at least
// one object. When a goroutine needs to allocate large-object span from heap,
// it sweeps spans until it frees at least that many pages into heap. There is
// one case where this may not suffice: if a goroutine sweeps and frees two
// nonadjacent one-page spans to the heap, it will allocate a new two-page
// span, but there can still be other one-page unswept spans which could be
// combined into a two-page span.
//
// It's critical to ensure that no operations proceed on unswept spans (that would corrupt
// mark bits in GC bitmap). During GC all mcaches are flushed into the central cache,
// so they are empty. When a goroutine grabs a new span into mcache, it sweeps it.
// When a goroutine explicitly frees an object or sets a finalizer, it ensures that
// the span is swept (either by sweeping it, or by waiting for the concurrent sweep to finish).
// The finalizer goroutine is kicked off only when all spans are swept.
// When the next GC starts, it sweeps all not-yet-swept spans (if any).

// GC rate.
// Next GC is after we've allocated an extra amount of memory proportional to
// the amount already in use. The proportion is controlled by GOGC environment variable
// (100 by default). If GOGC=100 and we're using 4M, we'll GC again when we get to 8M
// (this mark is tracked in gcController.heapGoal variable). This keeps the GC cost in
// linear proportion to the allocation cost. Adjusting GOGC just changes the linear constant
// (and also the amount of extra memory used).

// Oblets
//
// In order to prevent long pauses while scanning large objects and to
// improve parallelism, the garbage collector breaks up scan jobs for
// objects larger than maxObletBytes into "oblets" of at most
// maxObletBytes. When scanning encounters the beginning of a large
// object, it scans only the first oblet and enqueues the remaining
// oblets as new scan jobs.

// package runtime -- go2cs converted at 2022 March 13 05:25:21 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mgc.go
namespace go;

using cpu = @internal.cpu_package;
using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;
using System.Threading;
using System;

public static partial class runtime_package {

private static readonly nint _DebugGC = 0;
private static readonly var _ConcurrentSweep = true;
private static readonly nint _FinBlockSize = 4 * 1024; 

// debugScanConservative enables debug logging for stack
// frames that are scanned conservatively.
private static readonly var debugScanConservative = false; 

// sweepMinHeapDistance is a lower bound on the heap distance
// (in bytes) reserved for concurrent sweeping between GC
// cycles.
private static readonly nint sweepMinHeapDistance = 1024 * 1024;

private static void gcinit() {
    if (@unsafe.Sizeof(new workbuf()) != _WorkbufSize) {
        throw("size of Workbuf is suboptimal");
    }
    mheap_.sweepDrained = 1; 

    // Initialize GC pacer state.
    // Use the environment variable GOGC for the initial gcPercent value.
    gcController.init(readGOGC());

    work.startSema = 1;
    work.markDoneSema = 1;
    lockInit(_addr_work.sweepWaiters.@lock, lockRankSweepWaiters);
    lockInit(_addr_work.assistQueue.@lock, lockRankAssistQueue);
    lockInit(_addr_work.wbufSpans.@lock, lockRankWbufSpans);
}

// Temporary in order to enable register ABI work.
// TODO(register args): convert back to local chan in gcenabled, passed to "go" stmts.
private static channel<nint> gcenable_setup = default;

// gcenable is called after the bulk of the runtime initialization,
// just before we're about to start letting user code run.
// It kicks off the background sweeper goroutine, the background
// scavenger goroutine, and enables GC.
private static void gcenable() { 
    // Kick off sweeping and scavenging.
    gcenable_setup = make_channel<nint>(2);
    go_(() => bgsweep());
    go_(() => bgscavenge());
    gcenable_setup.Receive().Send(gcenable_setup);
    gcenable_setup = null;
    memstats.enablegc = true; // now that runtime is initialized, GC is okay
}

// Garbage collector phase.
// Indicates to write barrier and synchronization task to perform.
private static uint gcphase = default;

// The compiler knows about this variable.
// If you change it, you must change builtin/runtime.go, too.
// If you change the first four bytes, you must also change the write
// barrier insertion code.
private static var writeBarrier = default;

// gcBlackenEnabled is 1 if mutator assists and background mark
// workers are allowed to blacken objects. This must only be set when
// gcphase == _GCmark.
private static uint gcBlackenEnabled = default;

private static readonly var _GCoff = iota; // GC not running; sweeping in background, write barrier disabled
private static readonly var _GCmark = 0; // GC marking roots and workbufs: allocate black, write barrier ENABLED
private static readonly var _GCmarktermination = 1; // GC mark termination: allocate black, P's help GC, write barrier ENABLED

//go:nosplit
private static void setGCPhase(uint x) {
    atomic.Store(_addr_gcphase, x);
    writeBarrier.needed = gcphase == _GCmark || gcphase == _GCmarktermination;
    writeBarrier.enabled = writeBarrier.needed || writeBarrier.cgo;
}

// gcMarkWorkerMode represents the mode that a concurrent mark worker
// should operate in.
//
// Concurrent marking happens through four different mechanisms. One
// is mutator assists, which happen in response to allocations and are
// not scheduled. The other three are variations in the per-P mark
// workers and are distinguished by gcMarkWorkerMode.
private partial struct gcMarkWorkerMode { // : nint
}

 
// gcMarkWorkerNotWorker indicates that the next scheduled G is not
// starting work and the mode should be ignored.
private static readonly gcMarkWorkerMode gcMarkWorkerNotWorker = iota; 

// gcMarkWorkerDedicatedMode indicates that the P of a mark
// worker is dedicated to running that mark worker. The mark
// worker should run without preemption.
private static readonly var gcMarkWorkerDedicatedMode = 0; 

// gcMarkWorkerFractionalMode indicates that a P is currently
// running the "fractional" mark worker. The fractional worker
// is necessary when GOMAXPROCS*gcBackgroundUtilization is not
// an integer and using only dedicated workers would result in
// utilization too far from the target of gcBackgroundUtilization.
// The fractional worker should run until it is preempted and
// will be scheduled to pick up the fractional part of
// GOMAXPROCS*gcBackgroundUtilization.
private static readonly var gcMarkWorkerFractionalMode = 1; 

// gcMarkWorkerIdleMode indicates that a P is running the mark
// worker because it has nothing else to do. The idle worker
// should run until it is preempted and account its time
// against gcController.idleMarkTime.
private static readonly var gcMarkWorkerIdleMode = 2;

// gcMarkWorkerModeStrings are the strings labels of gcMarkWorkerModes
// to use in execution traces.
private static array<@string> gcMarkWorkerModeStrings = new array<@string>(new @string[] { "Not worker", "GC (dedicated)", "GC (fractional)", "GC (idle)" });

// pollFractionalWorkerExit reports whether a fractional mark worker
// should self-preempt. It assumes it is called from the fractional
// worker.
private static bool pollFractionalWorkerExit() { 
    // This should be kept in sync with the fractional worker
    // scheduler logic in findRunnableGCWorker.
    var now = nanotime();
    var delta = now - gcController.markStartTime;
    if (delta <= 0) {
        return true;
    }
    var p = getg().m.p.ptr();
    var selfTime = p.gcFractionalMarkTime + (now - p.gcMarkWorkerStartTime); 
    // Add some slack to the utilization goal so that the
    // fractional worker isn't behind again the instant it exits.
    return float64(selfTime) / float64(delta) > 1.2F * gcController.fractionalUtilizationGoal;
}

private static var work = default;

// GC runs a garbage collection and blocks the caller until the
// garbage collection is complete. It may also block the entire
// program.
public static void GC() { 
    // We consider a cycle to be: sweep termination, mark, mark
    // termination, and sweep. This function shouldn't return
    // until a full cycle has been completed, from beginning to
    // end. Hence, we always want to finish up the current cycle
    // and start a new one. That means:
    //
    // 1. In sweep termination, mark, or mark termination of cycle
    // N, wait until mark termination N completes and transitions
    // to sweep N.
    //
    // 2. In sweep N, help with sweep N.
    //
    // At this point we can begin a full cycle N+1.
    //
    // 3. Trigger cycle N+1 by starting sweep termination N+1.
    //
    // 4. Wait for mark termination N+1 to complete.
    //
    // 5. Help with sweep N+1 until it's done.
    //
    // This all has to be written to deal with the fact that the
    // GC may move ahead on its own. For example, when we block
    // until mark termination N, we may wake up in cycle N+2.

    // Wait until the current sweep termination, mark, and mark
    // termination complete.
    var n = atomic.Load(_addr_work.cycles);
    gcWaitOnMark(n); 

    // We're now in sweep N or later. Trigger GC cycle N+1, which
    // will first finish sweep N if necessary and then enter sweep
    // termination N+1.
    gcStart(new gcTrigger(kind:gcTriggerCycle,n:n+1)); 

    // Wait for mark termination N+1 to complete.
    gcWaitOnMark(n + 1); 

    // Finish sweep N+1 before returning. We do this both to
    // complete the cycle and because runtime.GC() is often used
    // as part of tests and benchmarks to get the system into a
    // relatively stable and isolated state.
    while (atomic.Load(_addr_work.cycles) == n + 1 && sweepone() != ~uintptr(0)) {
        sweep.nbgsweep++;
        Gosched();
    } 

    // Callers may assume that the heap profile reflects the
    // just-completed cycle when this returns (historically this
    // happened because this was a STW GC), but right now the
    // profile still reflects mark termination N, not N+1.
    //
    // As soon as all of the sweep frees from cycle N+1 are done,
    // we can go ahead and publish the heap profile.
    //
    // First, wait for sweeping to finish. (We know there are no
    // more spans on the sweep queue, but we may be concurrently
    // sweeping spans, so we have to wait.)
    while (atomic.Load(_addr_work.cycles) == n + 1 && !isSweepDone()) {
        Gosched();
    } 

    // Now we're really done with sweeping, so we can publish the
    // stable heap profile. Only do this if we haven't already hit
    // another mark termination.
    var mp = acquirem();
    var cycle = atomic.Load(_addr_work.cycles);
    if (cycle == n + 1 || (gcphase == _GCmark && cycle == n + 2)) {
        mProf_PostSweep();
    }
    releasem(mp);
}

// gcWaitOnMark blocks until GC finishes the Nth mark phase. If GC has
// already completed this mark phase, it returns immediately.
private static void gcWaitOnMark(uint n) {
    while (true) { 
        // Disable phase transitions.
        lock(_addr_work.sweepWaiters.@lock);
        var nMarks = atomic.Load(_addr_work.cycles);
        if (gcphase != _GCmark) { 
            // We've already completed this cycle's mark.
            nMarks++;
        }
        if (nMarks > n) { 
            // We're done.
            unlock(_addr_work.sweepWaiters.@lock);
            return ;
        }
        work.sweepWaiters.list.push(getg());
        goparkunlock(_addr_work.sweepWaiters.@lock, waitReasonWaitForGCCycle, traceEvGoBlock, 1);
    }
}

// gcMode indicates how concurrent a GC cycle should be.
private partial struct gcMode { // : nint
}

private static readonly gcMode gcBackgroundMode = iota; // concurrent GC and sweep
private static readonly var gcForceMode = 0; // stop-the-world GC now, concurrent sweep
private static readonly var gcForceBlockMode = 1; // stop-the-world GC now and STW sweep (forced by user)

// A gcTrigger is a predicate for starting a GC cycle. Specifically,
// it is an exit condition for the _GCoff phase.
private partial struct gcTrigger {
    public gcTriggerKind kind;
    public long now; // gcTriggerTime: current time
    public uint n; // gcTriggerCycle: cycle number to start
}

private partial struct gcTriggerKind { // : nint
}

 
// gcTriggerHeap indicates that a cycle should be started when
// the heap size reaches the trigger heap size computed by the
// controller.
private static readonly gcTriggerKind gcTriggerHeap = iota; 

// gcTriggerTime indicates that a cycle should be started when
// it's been more than forcegcperiod nanoseconds since the
// previous GC cycle.
private static readonly var gcTriggerTime = 0; 

// gcTriggerCycle indicates that a cycle should be started if
// we have not yet started cycle number gcTrigger.n (relative
// to work.cycles).
private static readonly var gcTriggerCycle = 1;

// test reports whether the trigger condition is satisfied, meaning
// that the exit condition for the _GCoff phase has been met. The exit
// condition should be tested when allocating.
private static bool test(this gcTrigger t) {
    if (!memstats.enablegc || panicking != 0 || gcphase != _GCoff) {
        return false;
    }

    if (t.kind == gcTriggerHeap) 
        // Non-atomic access to gcController.heapLive for performance. If
        // we are going to trigger on this, this thread just
        // atomically wrote gcController.heapLive anyway and we'll see our
        // own write.
        return gcController.heapLive >= gcController.trigger;
    else if (t.kind == gcTriggerTime) 
        if (gcController.gcPercent < 0) {
            return false;
        }
        var lastgc = int64(atomic.Load64(_addr_memstats.last_gc_nanotime));
        return lastgc != 0 && t.now - lastgc > forcegcperiod;
    else if (t.kind == gcTriggerCycle) 
        // t.n > work.cycles, but accounting for wraparound.
        return int32(t.n - work.cycles) > 0;
        return true;
}

// gcStart starts the GC. It transitions from _GCoff to _GCmark (if
// debug.gcstoptheworld == 0) or performs all of GC (if
// debug.gcstoptheworld != 0).
//
// This may return without performing this transition in some cases,
// such as when called on a system stack or with locks held.
private static void gcStart(gcTrigger trigger) { 
    // Since this is called from malloc and malloc is called in
    // the guts of a number of libraries that might be holding
    // locks, don't attempt to start GC in non-preemptible or
    // potentially unstable situations.
    var mp = acquirem();
    {
        var gp = getg();

        if (gp == mp.g0 || mp.locks > 1 || mp.preemptoff != "") {
            releasem(mp);
            return ;
        }
    }
    releasem(mp);
    mp = null; 

    // Pick up the remaining unswept/not being swept spans concurrently
    //
    // This shouldn't happen if we're being invoked in background
    // mode since proportional sweep should have just finished
    // sweeping everything, but rounding errors, etc, may leave a
    // few spans unswept. In forced mode, this is necessary since
    // GC can be forced at any point in the sweeping cycle.
    //
    // We check the transition condition continuously here in case
    // this G gets delayed in to the next GC cycle.
    while (trigger.test() && sweepone() != ~uintptr(0)) {
        sweep.nbgsweep++;
    } 

    // Perform GC initialization and the sweep termination
    // transition.
    semacquire(_addr_work.startSema); 
    // Re-check transition condition under transition lock.
    if (!trigger.test()) {
        semrelease(_addr_work.startSema);
        return ;
    }
    work.userForced = trigger.kind == gcTriggerCycle; 

    // In gcstoptheworld debug mode, upgrade the mode accordingly.
    // We do this after re-checking the transition condition so
    // that multiple goroutines that detect the heap trigger don't
    // start multiple STW GCs.
    var mode = gcBackgroundMode;
    if (debug.gcstoptheworld == 1) {
        mode = gcForceMode;
    }
    else if (debug.gcstoptheworld == 2) {
        mode = gcForceBlockMode;
    }
    semacquire(_addr_gcsema);
    semacquire(_addr_worldsema);

    if (trace.enabled) {
        traceGCStart();
    }
    foreach (var (_, p) in allp) {
        {
            var fg = atomic.Load(_addr_p.mcache.flushGen);

            if (fg != mheap_.sweepgen) {
                println("runtime: p", p.id, "flushGen", fg, "!= sweepgen", mheap_.sweepgen);
                throw("p mcache not flushed");
            }

        }
    }    gcBgMarkStartWorkers();

    systemstack(gcResetMarkState);

    (work.stwprocs, work.maxprocs) = (gomaxprocs, gomaxprocs);    if (work.stwprocs > ncpu) { 
        // This is used to compute CPU time of the STW phases,
        // so it can't be more than ncpu, even if GOMAXPROCS is.
        work.stwprocs = ncpu;
    }
    work.heap0 = atomic.Load64(_addr_gcController.heapLive);
    work.pauseNS = 0;
    work.mode = mode;

    var now = nanotime();
    work.tSweepTerm = now;
    work.pauseStart = now;
    if (trace.enabled) {
        traceGCSTWStart(1);
    }
    systemstack(stopTheWorldWithSema); 
    // Finish sweep before we start concurrent scan.
    systemstack(() => {
        finishsweep_m();
    }); 

    // clearpools before we start the GC. If we wait they memory will not be
    // reclaimed until the next GC cycle.
    clearpools();

    work.cycles++;

    gcController.startCycle();
    work.heapGoal = gcController.heapGoal; 

    // In STW mode, disable scheduling of user Gs. This may also
    // disable scheduling of this goroutine, so it may block as
    // soon as we start the world again.
    if (mode != gcBackgroundMode) {
        schedEnableUser(false);
    }
    setGCPhase(_GCmark);

    gcBgMarkPrepare(); // Must happen before assist enable.
    gcMarkRootPrepare(); 

    // Mark all active tinyalloc blocks. Since we're
    // allocating from these, they need to be black like
    // other allocations. The alternative is to blacken
    // the tiny block on every allocation from it, which
    // would slow down the tiny allocator.
    gcMarkTinyAllocs(); 

    // At this point all Ps have enabled the write
    // barrier, thus maintaining the no white to
    // black invariant. Enable mutator assists to
    // put back-pressure on fast allocating
    // mutators.
    atomic.Store(_addr_gcBlackenEnabled, 1); 

    // Assists and workers can start the moment we start
    // the world.
    gcController.markStartTime = now; 

    // In STW mode, we could block the instant systemstack
    // returns, so make sure we're not preemptible.
    mp = acquirem(); 

    // Concurrent mark.
    systemstack(() => {
        now = startTheWorldWithSema(trace.enabled);
        work.pauseNS += now - work.pauseStart;
        work.tMark = now;
        memstats.gcPauseDist.record(now - work.pauseStart);
    }); 

    // Release the world sema before Gosched() in STW mode
    // because we will need to reacquire it later but before
    // this goroutine becomes runnable again, and we could
    // self-deadlock otherwise.
    semrelease(_addr_worldsema);
    releasem(mp); 

    // Make sure we block instead of returning to user code
    // in STW mode.
    if (mode != gcBackgroundMode) {
        Gosched();
    }
    semrelease(_addr_work.startSema);
}

// gcMarkDoneFlushed counts the number of P's with flushed work.
//
// Ideally this would be a captured local in gcMarkDone, but forEachP
// escapes its callback closure, so it can't capture anything.
//
// This is protected by markDoneSema.
private static uint gcMarkDoneFlushed = default;

// gcMarkDone transitions the GC from mark to mark termination if all
// reachable objects have been marked (that is, there are no grey
// objects and can be no more in the future). Otherwise, it flushes
// all local work to the global queues where it can be discovered by
// other workers.
//
// This should be called when all local mark work has been drained and
// there are no remaining workers. Specifically, when
//
//   work.nwait == work.nproc && !gcMarkWorkAvailable(p)
//
// The calling context must be preemptible.
//
// Flushing local work is important because idle Ps may have local
// work queued. This is the only way to make that work visible and
// drive GC to completion.
//
// It is explicitly okay to have write barriers in this function. If
// it does transition to mark termination, then all reachable objects
// have been marked, so the write barrier cannot shade any more
// objects.
private static void gcMarkDone() { 
    // Ensure only one thread is running the ragged barrier at a
    // time.
    semacquire(_addr_work.markDoneSema);

top: 

    // forEachP needs worldsema to execute, and we'll need it to
    // stop the world later, so acquire worldsema now.
    if (!(gcphase == _GCmark && work.nwait == work.nproc && !gcMarkWorkAvailable(_addr_null))) {
        semrelease(_addr_work.markDoneSema);
        return ;
    }
    semacquire(_addr_worldsema); 

    // Flush all local buffers and collect flushedWork flags.
    gcMarkDoneFlushed = 0;
    systemstack(() => {
        var gp = getg().m.curg; 
        // Mark the user stack as preemptible so that it may be scanned.
        // Otherwise, our attempt to force all P's to a safepoint could
        // result in a deadlock as we attempt to preempt a worker that's
        // trying to preempt us (e.g. for a stack scan).
        casgstatus(gp, _Grunning, _Gwaiting);
        forEachP(_p_ => { 
            // Flush the write barrier buffer, since this may add
            // work to the gcWork.
            wbBufFlush1(_p_); 

            // Flush the gcWork, since this may create global work
            // and set the flushedWork flag.
            //
            // TODO(austin): Break up these workbufs to
            // better distribute work.
            _p_.gcw.dispose(); 
            // Collect the flushedWork flag.
            if (_p_.gcw.flushedWork) {
                atomic.Xadd(_addr_gcMarkDoneFlushed, 1);
                _p_.gcw.flushedWork = false;
            }
        });
        casgstatus(gp, _Gwaiting, _Grunning);
    });

    if (gcMarkDoneFlushed != 0) { 
        // More grey objects were discovered since the
        // previous termination check, so there may be more
        // work to do. Keep going. It's possible the
        // transition condition became true again during the
        // ragged barrier, so re-check it.
        semrelease(_addr_worldsema);
        goto top;
    }
    var now = nanotime();
    work.tMarkTerm = now;
    work.pauseStart = now;
    getg().m.preemptoff = "gcing";
    if (trace.enabled) {
        traceGCSTWStart(0);
    }
    systemstack(stopTheWorldWithSema); 
    // The gcphase is _GCmark, it will transition to _GCmarktermination
    // below. The important thing is that the wb remains active until
    // all marking is complete. This includes writes made by the GC.

    // There is sometimes work left over when we enter mark termination due
    // to write barriers performed after the completion barrier above.
    // Detect this and resume concurrent mark. This is obviously
    // unfortunate.
    //
    // See issue #27993 for details.
    //
    // Switch to the system stack to call wbBufFlush1, though in this case
    // it doesn't matter because we're non-preemptible anyway.
    var restart = false;
    systemstack(() => {
        foreach (var (_, p) in allp) {
            wbBufFlush1(p);
            if (!p.gcw.empty()) {
                restart = true;
                break;
            }
        }
    });
    if (restart) {
        getg().m.preemptoff = "";
        systemstack(() => {
            now = startTheWorldWithSema(true);
            work.pauseNS += now - work.pauseStart;
            memstats.gcPauseDist.record(now - work.pauseStart);
        });
        semrelease(_addr_worldsema);
        goto top;
    }
    atomic.Store(_addr_gcBlackenEnabled, 0); 

    // Wake all blocked assists. These will run when we
    // start the world again.
    gcWakeAllAssists(); 

    // Likewise, release the transition lock. Blocked
    // workers and assists will run when we start the
    // world again.
    semrelease(_addr_work.markDoneSema); 

    // In STW mode, re-enable user goroutines. These will be
    // queued to run after we start the world.
    schedEnableUser(true); 

    // endCycle depends on all gcWork cache stats being flushed.
    // The termination algorithm above ensured that up to
    // allocations since the ragged barrier.
    var nextTriggerRatio = gcController.endCycle(work.userForced); 

    // Perform mark termination. This will restart the world.
    gcMarkTermination(nextTriggerRatio);
}

// World must be stopped and mark assists and background workers must be
// disabled.
private static void gcMarkTermination(double nextTriggerRatio) { 
    // Start marktermination (write barrier remains enabled for now).
    setGCPhase(_GCmarktermination);

    work.heap1 = gcController.heapLive;
    var startTime = nanotime();

    var mp = acquirem();
    mp.preemptoff = "gcing";
    var _g_ = getg();
    _g_.m.traceback = 2;
    var gp = _g_.m.curg;
    casgstatus(gp, _Grunning, _Gwaiting);
    gp.waitreason = waitReasonGarbageCollection; 

    // Run gc on the g0 stack. We do this so that the g stack
    // we're currently running on will no longer change. Cuts
    // the root set down a bit (g0 stacks are not scanned, and
    // we don't need to scan gc's internal state).  We also
    // need to switch to g0 so we can shrink the stack.
    systemstack(() => {
        gcMark(startTime); 
        // Must return immediately.
        // The outer function's stack may have moved
        // during gcMark (it shrinks stacks, including the
        // outer function's stack), so we must not refer
        // to any of its variables. Return back to the
        // non-system stack to pick up the new addresses
        // before continuing.
    });

    systemstack(() => {
        work.heap2 = work.bytesMarked;
        if (debug.gccheckmark > 0) { 
            // Run a full non-parallel, stop-the-world
            // mark using checkmark bits, to check that we
            // didn't forget to mark anything during the
            // concurrent mark process.
            startCheckmarks();
            gcResetMarkState();
            var gcw = _addr_getg().m.p.ptr().gcw;
            gcDrain(gcw, 0);
            wbBufFlush1(getg().m.p.ptr());
            gcw.dispose();
            endCheckmarks();
        }
        setGCPhase(_GCoff);
        gcSweep(work.mode);
    });

    _g_.m.traceback = 0;
    casgstatus(gp, _Gwaiting, _Grunning);

    if (trace.enabled) {
        traceGCDone();
    }
    mp.preemptoff = "";

    if (gcphase != _GCoff) {
        throw("gc done but gcphase != _GCoff");
    }
    gcController.lastHeapGoal = gcController.heapGoal;
    memstats.last_heap_inuse = memstats.heap_inuse; 

    // Update GC trigger and pacing for the next cycle.
    gcController.commit(nextTriggerRatio); 

    // Update timing memstats
    var now = nanotime();
    var (sec, nsec, _) = time_now();
    var unixNow = sec * 1e9F + int64(nsec);
    work.pauseNS += now - work.pauseStart;
    work.tEnd = now;
    memstats.gcPauseDist.record(now - work.pauseStart);
    atomic.Store64(_addr_memstats.last_gc_unix, uint64(unixNow)); // must be Unix time to make sense to user
    atomic.Store64(_addr_memstats.last_gc_nanotime, uint64(now)); // monotonic time for us
    memstats.pause_ns[memstats.numgc % uint32(len(memstats.pause_ns))] = uint64(work.pauseNS);
    memstats.pause_end[memstats.numgc % uint32(len(memstats.pause_end))] = uint64(unixNow);
    memstats.pause_total_ns += uint64(work.pauseNS); 

    // Update work.totaltime.
    var sweepTermCpu = int64(work.stwprocs) * (work.tMark - work.tSweepTerm); 
    // We report idle marking time below, but omit it from the
    // overall utilization here since it's "free".
    var markCpu = gcController.assistTime + gcController.dedicatedMarkTime + gcController.fractionalMarkTime;
    var markTermCpu = int64(work.stwprocs) * (work.tEnd - work.tMarkTerm);
    var cycleCpu = sweepTermCpu + markCpu + markTermCpu;
    work.totaltime += cycleCpu; 

    // Compute overall GC CPU utilization.
    var totalCpu = sched.totaltime + (now - sched.procresizetime) * int64(gomaxprocs);
    memstats.gc_cpu_fraction = float64(work.totaltime) / float64(totalCpu); 

    // Reset sweep state.
    sweep.nbgsweep = 0;
    sweep.npausesweep = 0;

    if (work.userForced) {
        memstats.numforcedgc++;
    }
    lock(_addr_work.sweepWaiters.@lock);
    memstats.numgc++;
    injectglist(_addr_work.sweepWaiters.list);
    unlock(_addr_work.sweepWaiters.@lock); 

    // Finish the current heap profiling cycle and start a new
    // heap profiling cycle. We do this before starting the world
    // so events don't leak into the wrong cycle.
    mProf_NextCycle(); 

    // There may be stale spans in mcaches that need to be swept.
    // Those aren't tracked in any sweep lists, so we need to
    // count them against sweep completion until we ensure all
    // those spans have been forced out.
    var sl = newSweepLocker();
    sl.blockCompletion();

    systemstack(() => {
        startTheWorldWithSema(true);
    }); 

    // Flush the heap profile so we can start a new cycle next GC.
    // This is relatively expensive, so we don't do it with the
    // world stopped.
    mProf_Flush(); 

    // Prepare workbufs for freeing by the sweeper. We do this
    // asynchronously because it can take non-trivial time.
    prepareFreeWorkbufs(); 

    // Free stack spans. This must be done between GC cycles.
    systemstack(freeStackSpans); 

    // Ensure all mcaches are flushed. Each P will flush its own
    // mcache before allocating, but idle Ps may not. Since this
    // is necessary to sweep all spans, we need to ensure all
    // mcaches are flushed before we start the next GC cycle.
    systemstack(() => {
        forEachP(_p_ => {
            _p_.mcache.prepareForSweep();
        });
    }); 
    // Now that we've swept stale spans in mcaches, they don't
    // count against unswept spans.
    sl.dispose(); 

    // Print gctrace before dropping worldsema. As soon as we drop
    // worldsema another cycle could start and smash the stats
    // we're trying to print.
    if (debug.gctrace > 0) {
        var util = int(memstats.gc_cpu_fraction * 100);

        array<byte> sbuf = new array<byte>(24);
        printlock();
        print("gc ", memstats.numgc, " @", string(itoaDiv(sbuf[..], uint64(work.tSweepTerm - runtimeInitTime) / 1e6F, 3)), "s ", util, "%: ");
        var prev = work.tSweepTerm;
        {
            long i__prev1 = i;
            long ns__prev1 = ns;

            foreach (var (__i, __ns) in new slice<long>(new long[] { work.tMark, work.tMarkTerm, work.tEnd })) {
                i = __i;
                ns = __ns;
                if (i != 0) {
                    print("+");
                }
                print(string(fmtNSAsMS(sbuf[..], uint64(ns - prev))));
                prev = ns;
            }

            i = i__prev1;
            ns = ns__prev1;
        }

        print(" ms clock, ");
        {
            long i__prev1 = i;
            long ns__prev1 = ns;

            foreach (var (__i, __ns) in new slice<long>(new long[] { sweepTermCpu, gcController.assistTime, gcController.dedicatedMarkTime+gcController.fractionalMarkTime, gcController.idleMarkTime, markTermCpu })) {
                i = __i;
                ns = __ns;
                if (i == 2 || i == 3) { 
                    // Separate mark time components with /.
                    print("/");
                }
                else if (i != 0) {
                    print("+");
                }
                print(string(fmtNSAsMS(sbuf[..], uint64(ns))));
            }

            i = i__prev1;
            ns = ns__prev1;
        }

        print(" ms cpu, ", work.heap0 >> 20, "->", work.heap1 >> 20, "->", work.heap2 >> 20, " MB, ", work.heapGoal >> 20, " MB goal, ", work.maxprocs, " P");
        if (work.userForced) {
            print(" (forced)");
        }
        print("\n");
        printunlock();
    }
    semrelease(_addr_worldsema);
    semrelease(_addr_gcsema); 
    // Careful: another GC cycle may start now.

    releasem(mp);
    mp = null; 

    // now that gc is done, kick off finalizer thread if needed
    if (!concurrentSweep) { 
        // give the queued finalizers, if any, a chance to run
        Gosched();
    }
}

// gcBgMarkStartWorkers prepares background mark worker goroutines. These
// goroutines will not run until the mark phase, but they must be started while
// the work is not stopped and from a regular G stack. The caller must hold
// worldsema.
private static void gcBgMarkStartWorkers() { 
    // Background marking is performed by per-P G's. Ensure that each P has
    // a background GC G.
    //
    // Worker Gs don't exit if gomaxprocs is reduced. If it is raised
    // again, we can reuse the old workers; no need to create new workers.
    while (gcBgMarkWorkerCount < gomaxprocs) {
        go_(() => gcBgMarkWorker());

        notetsleepg(_addr_work.bgMarkReady, -1);
        noteclear(_addr_work.bgMarkReady); 
        // The worker is now guaranteed to be added to the pool before
        // its P's next findRunnableGCWorker.

        gcBgMarkWorkerCount++;
    }
}

// gcBgMarkPrepare sets up state for background marking.
// Mutator assists must not yet be enabled.
private static void gcBgMarkPrepare() { 
    // Background marking will stop when the work queues are empty
    // and there are no more workers (note that, since this is
    // concurrent, this may be a transient state, but mark
    // termination will clean it up). Between background workers
    // and assists, we don't really know how many workers there
    // will be, so we pretend to have an arbitrarily large number
    // of workers, almost all of which are "waiting". While a
    // worker is working it decrements nwait. If nproc == nwait,
    // there are no workers.
    work.nproc = ~uint32(0);
    work.nwait = ~uint32(0);
}

// gcBgMarkWorker is an entry in the gcBgMarkWorkerPool. It points to a single
// gcBgMarkWorker goroutine.
private partial struct gcBgMarkWorkerNode {
    public lfnode node; // The g of this worker.
    public guintptr gp; // Release this m on park. This is used to communicate with the unlock
// function, which cannot access the G's stack. It is unused outside of
// gcBgMarkWorker().
    public muintptr m;
}

private static void gcBgMarkWorker() {
    var gp = getg(); 

    // We pass node to a gopark unlock function, so it can't be on
    // the stack (see gopark). Prevent deadlock from recursively
    // starting GC by disabling preemption.
    gp.m.preemptoff = "GC worker init";
    ptr<gcBgMarkWorkerNode> node = @new<gcBgMarkWorkerNode>();
    gp.m.preemptoff = "";

    node.gp.set(gp);

    node.m.set(acquirem());
    notewakeup(_addr_work.bgMarkReady); 
    // After this point, the background mark worker is generally scheduled
    // cooperatively by gcController.findRunnableGCWorker. While performing
    // work on the P, preemption is disabled because we are working on
    // P-local work buffers. When the preempt flag is set, this puts itself
    // into _Gwaiting to be woken up by gcController.findRunnableGCWorker
    // at the appropriate time.
    //
    // When preemption is enabled (e.g., while in gcMarkDone), this worker
    // may be preempted and schedule as a _Grunnable G from a runq. That is
    // fine; it will eventually gopark again for further scheduling via
    // findRunnableGCWorker.
    //
    // Since we disable preemption before notifying bgMarkReady, we
    // guarantee that this G will be in the worker pool for the next
    // findRunnableGCWorker. This isn't strictly necessary, but it reduces
    // latency between _GCmark starting and the workers starting.

    while (true) { 
        // Go to sleep until woken by
        // gcController.findRunnableGCWorker.
        gopark((g, nodep) => {
            node = (gcBgMarkWorkerNode.val)(nodep);

            {
                var mp = node.m.ptr();

                if (mp != null) { 
                    // The worker G is no longer running; release
                    // the M.
                    //
                    // N.B. it is _safe_ to release the M as soon
                    // as we are no longer performing P-local mark
                    // work.
                    //
                    // However, since we cooperatively stop work
                    // when gp.preempt is set, if we releasem in
                    // the loop then the following call to gopark
                    // would immediately preempt the G. This is
                    // also safe, but inefficient: the G must
                    // schedule again only to enter gopark and park
                    // again. Thus, we defer the release until
                    // after parking the G.
                    releasem(mp);
                } 

                // Release this G to the pool.

            } 

            // Release this G to the pool.
            gcBgMarkWorkerPool.push(_addr_node.node); 
            // Note that at this point, the G may immediately be
            // rescheduled and may be running.
            return true;
        }, @unsafe.Pointer(node), waitReasonGCWorkerIdle, traceEvGoBlock, 0); 

        // Preemption must not occur here, or another G might see
        // p.gcMarkWorkerMode.

        // Disable preemption so we can use the gcw. If the
        // scheduler wants to preempt us, we'll stop draining,
        // dispose the gcw, and then preempt.
        node.m.set(acquirem());
        var pp = gp.m.p.ptr(); // P can't change with preemption disabled.

        if (gcBlackenEnabled == 0) {
            println("worker mode", pp.gcMarkWorkerMode);
            throw("gcBgMarkWorker: blackening not enabled");
        }
        if (pp.gcMarkWorkerMode == gcMarkWorkerNotWorker) {
            throw("gcBgMarkWorker: mode not set");
        }
        var startTime = nanotime();
        pp.gcMarkWorkerStartTime = startTime;

        var decnwait = atomic.Xadd(_addr_work.nwait, -1);
        if (decnwait == work.nproc) {
            println("runtime: work.nwait=", decnwait, "work.nproc=", work.nproc);
            throw("work.nwait was > work.nproc");
        }
        systemstack(() => { 
            // Mark our goroutine preemptible so its stack
            // can be scanned. This lets two mark workers
            // scan each other (otherwise, they would
            // deadlock). We must not modify anything on
            // the G stack. However, stack shrinking is
            // disabled for mark workers, so it is safe to
            // read from the G stack.
            casgstatus(gp, _Grunning, _Gwaiting);

            if (pp.gcMarkWorkerMode == gcMarkWorkerDedicatedMode) 
                gcDrain(_addr_pp.gcw, gcDrainUntilPreempt | gcDrainFlushBgCredit);
                if (gp.preempt) { 
                    // We were preempted. This is
                    // a useful signal to kick
                    // everything out of the run
                    // queue so it can run
                    // somewhere else.
                    {
                        var (drainQ, n) = runqdrain(pp);

                        if (n > 0) {
                            lock(_addr_sched.@lock);
                            globrunqputbatch(_addr_drainQ, int32(n));
                            unlock(_addr_sched.@lock);
                        }

                    }
                } 
                // Go back to draining, this time
                // without preemption.
                gcDrain(_addr_pp.gcw, gcDrainFlushBgCredit);
            else if (pp.gcMarkWorkerMode == gcMarkWorkerFractionalMode) 
                gcDrain(_addr_pp.gcw, gcDrainFractional | gcDrainUntilPreempt | gcDrainFlushBgCredit);
            else if (pp.gcMarkWorkerMode == gcMarkWorkerIdleMode) 
                gcDrain(_addr_pp.gcw, gcDrainIdle | gcDrainUntilPreempt | gcDrainFlushBgCredit);
            else 
                throw("gcBgMarkWorker: unexpected gcMarkWorkerMode");
                        casgstatus(gp, _Gwaiting, _Grunning);
        }); 

        // Account for time.
        var duration = nanotime() - startTime;

        if (pp.gcMarkWorkerMode == gcMarkWorkerDedicatedMode) 
            atomic.Xaddint64(_addr_gcController.dedicatedMarkTime, duration);
            atomic.Xaddint64(_addr_gcController.dedicatedMarkWorkersNeeded, 1);
        else if (pp.gcMarkWorkerMode == gcMarkWorkerFractionalMode) 
            atomic.Xaddint64(_addr_gcController.fractionalMarkTime, duration);
            atomic.Xaddint64(_addr_pp.gcFractionalMarkTime, duration);
        else if (pp.gcMarkWorkerMode == gcMarkWorkerIdleMode) 
            atomic.Xaddint64(_addr_gcController.idleMarkTime, duration);
        // Was this the last worker and did we run out
        // of work?
        var incnwait = atomic.Xadd(_addr_work.nwait, +1);
        if (incnwait > work.nproc) {
            println("runtime: p.gcMarkWorkerMode=", pp.gcMarkWorkerMode, "work.nwait=", incnwait, "work.nproc=", work.nproc);
            throw("work.nwait > work.nproc");
        }
        pp.gcMarkWorkerMode = gcMarkWorkerNotWorker; 

        // If this worker reached a background mark completion
        // point, signal the main GC goroutine.
        if (incnwait == work.nproc && !gcMarkWorkAvailable(_addr_null)) { 
            // We don't need the P-local buffers here, allow
            // preemption becuse we may schedule like a regular
            // goroutine in gcMarkDone (block on locks, etc).
            releasem(node.m.ptr());
            node.m.set(null);

            gcMarkDone();
        }
    }
}

// gcMarkWorkAvailable reports whether executing a mark worker
// on p is potentially useful. p may be nil, in which case it only
// checks the global sources of work.
private static bool gcMarkWorkAvailable(ptr<p> _addr_p) {
    ref p p = ref _addr_p.val;

    if (p != null && !p.gcw.empty()) {
        return true;
    }
    if (!work.full.empty()) {
        return true; // global work available
    }
    if (work.markrootNext < work.markrootJobs) {
        return true; // root scan work available
    }
    return false;
}

// gcMark runs the mark (or, for concurrent GC, mark termination)
// All gcWork caches must be empty.
// STW is in effect at this point.
private static void gcMark(long startTime) => func((_, panic, _) => {
    if (debug.allocfreetrace > 0) {
        tracegc();
    }
    if (gcphase != _GCmarktermination) {
        throw("in gcMark expecting to see gcphase as _GCmarktermination");
    }
    work.tstart = startTime; 

    // Check that there's no marking work remaining.
    if (work.full != 0 || work.markrootNext < work.markrootJobs) {
        print("runtime: full=", hex(work.full), " next=", work.markrootNext, " jobs=", work.markrootJobs, " nDataRoots=", work.nDataRoots, " nBSSRoots=", work.nBSSRoots, " nSpanRoots=", work.nSpanRoots, " nStackRoots=", work.nStackRoots, "\n");
        panic("non-empty mark queue after concurrent mark");
    }
    if (debug.gccheckmark > 0) { 
        // This is expensive when there's a large number of
        // Gs, so only do it if checkmark is also enabled.
        gcMarkRootCheck();
    }
    if (work.full != 0) {
        throw("work.full != 0");
    }
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp) {
            p = __p; 
            // The write barrier may have buffered pointers since
            // the gcMarkDone barrier. However, since the barrier
            // ensured all reachable objects were marked, all of
            // these must be pointers to black objects. Hence we
            // can just discard the write barrier buffer.
            if (debug.gccheckmark > 0) { 
                // For debugging, flush the buffer and make
                // sure it really was all marked.
                wbBufFlush1(p);
            }
            else
 {
                p.wbBuf.reset();
            }
            var gcw = _addr_p.gcw;
            if (!gcw.empty()) {
                printlock();
                print("runtime: P ", p.id, " flushedWork ", gcw.flushedWork);
                if (gcw.wbuf1 == null) {
                    print(" wbuf1=<nil>");
                }
                else
 {
                    print(" wbuf1.n=", gcw.wbuf1.nobj);
                }
                if (gcw.wbuf2 == null) {
                    print(" wbuf2=<nil>");
                }
                else
 {
                    print(" wbuf2.n=", gcw.wbuf2.nobj);
                }
                print("\n");
                throw("P has cached GC work at end of mark termination");
            } 
            // There may still be cached empty buffers, which we
            // need to flush since we're going to free them. Also,
            // there may be non-zero stats because we allocated
            // black after the gcMarkDone barrier.
            gcw.dispose();
        }
        p = p__prev1;
    }

    gcController.heapMarked = work.bytesMarked; 

    // Flush scanAlloc from each mcache since we're about to modify
    // heapScan directly. If we were to flush this later, then scanAlloc
    // might have incorrect information.
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp) {
            p = __p;
            var c = p.mcache;
            if (c == null) {
                continue;
            }
            gcController.heapScan += uint64(c.scanAlloc);
            c.scanAlloc = 0;
        }
        p = p__prev1;
    }

    gcController.heapLive = work.bytesMarked;
    gcController.heapScan = uint64(gcController.scanWork);

    if (trace.enabled) {
        traceHeapAlloc();
    }
});

// gcSweep must be called on the system stack because it acquires the heap
// lock. See mheap for details.
//
// The world must be stopped.
//
//go:systemstack
private static void gcSweep(gcMode mode) {
    assertWorldStopped();

    if (gcphase != _GCoff) {
        throw("gcSweep being done but phase is not GCoff");
    }
    lock(_addr_mheap_.@lock);
    mheap_.sweepgen += 2;
    mheap_.sweepDrained = 0;
    mheap_.pagesSwept = 0;
    mheap_.sweepArenas = mheap_.allArenas;
    mheap_.reclaimIndex = 0;
    mheap_.reclaimCredit = 0;
    unlock(_addr_mheap_.@lock);

    sweep.centralIndex.clear();

    if (!_ConcurrentSweep || mode == gcForceBlockMode) { 
        // Special case synchronous sweep.
        // Record that no proportional sweeping has to happen.
        lock(_addr_mheap_.@lock);
        mheap_.sweepPagesPerByte = 0;
        unlock(_addr_mheap_.@lock); 
        // Sweep all spans eagerly.
        while (sweepone() != ~uintptr(0)) {
            sweep.npausesweep++;
        } 
        // Free workbufs eagerly.
        prepareFreeWorkbufs();
        while (freeSomeWbufs(false)) {
        } 
        // All "free" events for this mark/sweep cycle have
        // now happened, so we can make this profile cycle
        // available immediately.
        mProf_NextCycle();
        mProf_Flush();
        return ;
    }
    lock(_addr_sweep.@lock);
    if (sweep.parked) {
        sweep.parked = false;
        ready(sweep.g, 0, true);
    }
    unlock(_addr_sweep.@lock);
}

// gcResetMarkState resets global state prior to marking (concurrent
// or STW) and resets the stack scan state of all Gs.
//
// This is safe to do without the world stopped because any Gs created
// during or after this will start out in the reset state.
//
// gcResetMarkState must be called on the system stack because it acquires
// the heap lock. See mheap for details.
//
//go:systemstack
private static void gcResetMarkState() { 
    // This may be called during a concurrent phase, so lock to make sure
    // allgs doesn't change.
    forEachG(gp => {
        gp.gcscandone = false; // set to true in gcphasework
        gp.gcAssistBytes = 0;
    }); 

    // Clear page marks. This is just 1MB per 64GB of heap, so the
    // time here is pretty trivial.
    lock(_addr_mheap_.@lock);
    var arenas = mheap_.allArenas;
    unlock(_addr_mheap_.@lock);
    foreach (var (_, ai) in arenas) {
        var ha = mheap_.arenas[ai.l1()][ai.l2()];
        foreach (var (i) in ha.pageMarks) {
            ha.pageMarks[i] = 0;
        }
    }    work.bytesMarked = 0;
    work.initialHeapLive = atomic.Load64(_addr_gcController.heapLive);
}

// Hooks for other packages

private static Action poolcleanup = default;

//go:linkname sync_runtime_registerPoolCleanup sync.runtime_registerPoolCleanup
private static void sync_runtime_registerPoolCleanup(Action f) {
    poolcleanup = f;
}

private static void clearpools() { 
    // clear sync.Pools
    if (poolcleanup != null) {
        poolcleanup();
    }
    lock(_addr_sched.sudoglock);
    ptr<sudog> sg;    ptr<sudog> sgnext;

    sg = sched.sudogcache;

    while (sg != null) {
        sgnext = sg.next;
        sg.next = null;
        sg = addr(sgnext);
    }
    sched.sudogcache = null;
    unlock(_addr_sched.sudoglock); 

    // Clear central defer pools.
    // Leave per-P pools alone, they have strictly bounded size.
    lock(_addr_sched.deferlock);
    foreach (var (i) in sched.deferpool) { 
        // disconnect cached list before dropping it on the floor,
        // so that a dangling ref to one entry does not pin all of them.
        ptr<_defer> d;        ptr<_defer> dlink;

        d = sched.deferpool[i];

        while (d != null) {
            dlink = d.link;
            d.link = null;
            d = addr(dlink);
        }
        sched.deferpool[i] = null;
    }    unlock(_addr_sched.deferlock);
}

// Timing

// itoaDiv formats val/(10**dec) into buf.
private static slice<byte> itoaDiv(slice<byte> buf, ulong val, nint dec) {
    var i = len(buf) - 1;
    var idec = i - dec;
    while (val >= 10 || i >= idec) {
        buf[i] = byte(val % 10 + '0');
        i--;
        if (i == idec) {
            buf[i] = '.';
            i--;
        }
        val /= 10;
    }
    buf[i] = byte(val + '0');
    return buf[(int)i..];
}

// fmtNSAsMS nicely formats ns nanoseconds as milliseconds.
private static slice<byte> fmtNSAsMS(slice<byte> buf, ulong ns) {
    if (ns >= 10e6F) { 
        // Format as whole milliseconds.
        return itoaDiv(buf, ns / 1e6F, 0);
    }
    var x = ns / 1e3F;
    if (x == 0) {
        buf[0] = '0';
        return buf[..(int)1];
    }
    nint dec = 3;
    while (x >= 100) {
        x /= 10;
        dec--;
    }
    return itoaDiv(buf, x, dec);
}

// Helpers for testing GC.

// gcTestMoveStackOnNextCall causes the stack to be moved on a call
// immediately following the call to this. It may not work correctly
// if any other work appears after this call (such as returning).
// Typically the following call should be marked go:noinline so it
// performs a stack check.
//
// In rare cases this may not cause the stack to move, specifically if
// there's a preemption between this call and the next.
private static void gcTestMoveStackOnNextCall() {
    var gp = getg();
    gp.stackguard0 = stackForceMove;
}

// gcTestIsReachable performs a GC and returns a bit set where bit i
// is set if ptrs[i] is reachable.
private static ulong gcTestIsReachable(params unsafe.Pointer[] ptrs) => func((_, panic, _) => {
    ulong mask = default;
    ptrs = ptrs.Clone();
 
    // This takes the pointers as unsafe.Pointers in order to keep
    // them live long enough for us to attach specials. After
    // that, we drop our references to them.

    if (len(ptrs) > 64) {
        panic("too many pointers for uint64 mask");
    }
    semacquire(_addr_gcsema); 

    // Create reachability specials for ptrs.
    var specials = make_slice<ptr<specialReachable>>(len(ptrs));
    {
        var i__prev1 = i;

        foreach (var (__i, __p) in ptrs) {
            i = __i;
            p = __p;
            lock(_addr_mheap_.speciallock);
            var s = (specialReachable.val)(mheap_.specialReachableAlloc.alloc());
            unlock(_addr_mheap_.speciallock);
            s.special.kind = _KindSpecialReachable;
            if (!addspecial(p, _addr_s.special)) {
                throw("already have a reachable special (duplicate pointer?)");
            }
            specials[i] = s; 
            // Make sure we don't retain ptrs.
            ptrs[i] = null;
        }
        i = i__prev1;
    }

    semrelease(_addr_gcsema); 

    // Force a full GC and sweep.
    GC(); 

    // Process specials.
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in specials) {
            i = __i;
            s = __s;
            if (!s.done) {
                printlock();
                println("runtime: object", i, "was not swept");
                throw("IsReachable failed");
            }
            if (s.reachable) {
                mask |= 1 << (int)(i);
            }
            lock(_addr_mheap_.speciallock);
            mheap_.specialReachableAlloc.free(@unsafe.Pointer(s));
            unlock(_addr_mheap_.speciallock);
        }
        i = i__prev1;
        s = s__prev1;
    }

    return mask;
});

// gcTestPointerClass returns the category of what p points to, one of:
// "heap", "stack", "data", "bss", "other". This is useful for checking
// that a test is doing what it's intended to do.
//
// This is nosplit simply to avoid extra pointer shuffling that may
// complicate a test.
//
//go:nosplit
private static @string gcTestPointerClass(unsafe.Pointer p) {
    var p2 = uintptr(noescape(p));
    var gp = getg();
    if (gp.stack.lo <= p2 && p2 < gp.stack.hi) {
        return "stack";
    }
    {
        var (base, _, _) = findObject(p2, 0, 0);

        if (base != 0) {
            return "heap";
        }
    }
    foreach (var (_, datap) in activeModules()) {
        if (datap.data <= p2 && p2 < datap.edata || datap.noptrdata <= p2 && p2 < datap.enoptrdata) {
            return "data";
        }
        if (datap.bss <= p2 && p2 < datap.ebss || datap.noptrbss <= p2 && p2 <= datap.enoptrbss) {
            return "bss";
        }
    }    KeepAlive(p);
    return "other";
}

} // end runtime_package
