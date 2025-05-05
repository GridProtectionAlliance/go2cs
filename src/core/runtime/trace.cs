// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Go execution tracer.
// The tracer captures a wide range of execution events like goroutine
// creation/blocking/unblocking, syscall enter/exit/block, GC-related events,
// changes of heap size, processor start/stop, etc and writes them to a buffer
// in a compact form. A precise nanosecond-precision timestamp and a stack
// trace is captured for most events.
//
// Tracer invariants (to keep the synchronization making sense):
// - An m that has a trace buffer must be on either the allm or sched.freem lists.
// - Any trace buffer mutation must either be happening in traceAdvance or between
//   a traceAcquire and a subsequent traceRelease.
// - traceAdvance cannot return until the previous generation's buffers are all flushed.
//
// See https://go.dev/issue/60773 for a link to the full design.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// Trace state.

// trace is global tracing context.

[GoType("dyn")] partial struct Δtrace {
    // trace.lock must only be acquired on the system stack where
    // stack splits cannot happen while it is held.
    internal mutex @lock;
    // Trace buffer management.
    //
    // First we check the empty list for any free buffers. If not, buffers
    // are allocated directly from the OS. Once they're filled up and/or
    // flushed, they end up on the full queue for trace.gen%2.
    //
    // The trace reader takes buffers off the full list one-by-one and
    // places them into reading until they're finished being read from.
    // Then they're placed onto the empty list.
    //
    // Protected by trace.lock.
    internal ж<traceBuf> reading; // buffer currently handed off to user
    internal ж<traceBuf> empty; // stack of empty buffers
    internal array<traceBufQueue> full = new(2);
    internal @internal.runtime.atomic_package.Bool workAvailable;
    // State for the trace reader goroutine.
    //
    // Protected by trace.lock.
    internal @internal.runtime.atomic_package.Uintptr readerGen; // the generation the reader is currently reading for
    internal @internal.runtime.atomic_package.Uintptr flushedGen; // the last completed generation
    internal bool headerWritten;           // whether ReadTrace has emitted trace header
    // doneSema is used to synchronize the reader and traceAdvance. Specifically,
    // it notifies traceAdvance that the reader is done with a generation.
    // Both semaphores are 0 by default (so, acquires block). traceAdvance
    // attempts to acquire for gen%2 after flushing the last buffers for gen.
    // Meanwhile the reader releases the sema for gen%2 when it has finished
    // processing gen.
    internal array<uint32> doneSema = new(2);
    // Trace data tables for deduplicating data going into the trace.
    // There are 2 of each: one for gen%2, one for 1-gen%2.
    internal array<traceStackTable> stackTab = new(2); // maps stack traces to unique ids
    internal array<traceStringTable> stringTab = new(2); // maps strings to unique ids
    internal array<traceTypeTable> typeTab = new(2); // maps type pointers to unique ids
    // cpuLogRead accepts CPU profile samples from the signal handler where
    // they're generated. There are two profBufs here: one for gen%2, one for
    // 1-gen%2. These profBufs use a three-word header to hold the IDs of the P, G,
    // and M (respectively) that were active at the time of the sample. Because
    // profBuf uses a record with all zeros in its header to indicate overflow,
    // we make sure to make the P field always non-zero: The ID of a real P will
    // start at bit 1, and bit 0 will be set. Samples that arrive while no P is
    // running (such as near syscalls) will set the first header field to 0b10.
    // This careful handling of the first header field allows us to store ID of
    // the active G directly in the second field, even though that will be 0
    // when sampling g0.
    //
    // Initialization and teardown of these fields is protected by traceAdvanceSema.
    internal array<ж<profBuf>> cpuLogRead = new(2);
    internal @internal.runtime.atomic_package.Uint32 signalLock;             // protects use of the following member, only usable in signal handlers
    internal atomic.Pointer<profBuf> cpuLogWrite = new(2); // copy of cpuLogRead for use in signal handlers, set without signalLock
    internal ж<wakeableSleep> cpuSleep;
    internal /*<-*/channel<struct{}> cpuLogDone;
    internal array<ж<traceBuf>> cpuBuf = new(2);
    internal @internal.runtime.atomic_package.Pointer reader; // goroutine that called ReadTrace, or nil
    // Fast mappings from enumerations to string IDs that are prepopulated
    // in the trace.
    internal array<array<traceArg>> markWorkerLabels = new(2);
    internal array<array<traceArg>> goStopReasons = new(2);
    internal array<array<traceArg>> goBlockReasons = new(2);
    // enabled indicates whether tracing is enabled, but it is only an optimization,
    // NOT the source of truth on whether tracing is enabled. Tracing is only truly
    // enabled if gen != 0. This is used as an optimistic fast path check.
    //
    // Transitioning this value from true -> false is easy (once gen is 0)
    // because it's OK for enabled to have a stale "true" value. traceAcquire will
    // always double-check gen.
    //
    // Transitioning this value from false -> true is harder. We need to make sure
    // this is observable as true strictly before gen != 0. To maintain this invariant
    // we only make this transition with the world stopped and use the store to gen
    // as a publication barrier.
    internal bool enabled;
    // enabledWithAllocFree is set if debug.traceallocfree is != 0 when tracing begins.
    // It follows the same synchronization protocol as enabled.
    internal bool enabledWithAllocFree;
    // Trace generation counter.
    internal @internal.runtime.atomic_package.Uintptr gen;
    internal uintptr lastNonZeroGen; // last non-zero value of gen
    // shutdown is set when we are waiting for trace reader to finish after setting gen to 0
    //
    // Writes protected by trace.lock.
    internal @internal.runtime.atomic_package.Bool shutdown;
    // Number of goroutines in syscall exiting slow path.
    internal @internal.runtime.atomic_package.Int32 exitingSyscall;
    // seqGC is the sequence counter for GC begin/end.
    //
    // Mutated only during stop-the-world.
    internal uint64 seqGC;
    // minPageHeapAddr is the minimum address of the page heap when tracing started.
    internal uint64 minPageHeapAddr;
    // debugMalloc is the value of debug.malloc before tracing began.
    internal bool debugMalloc;
}
public static Δtrace Δtrace;

// Trace public API.
internal static uint32 traceAdvanceSema = 1;
internal static uint32 traceShutdownSema = 1;

// StartTrace enables tracing for the current process.
// While tracing, the data will be buffered and available via [ReadTrace].
// StartTrace returns an error if tracing is already enabled.
// Most clients should use the [runtime/trace] package or the [testing] package's
// -test.trace flag instead of calling StartTrace directly.
public static error StartTrace() {
    if (traceEnabled() || traceShuttingDown()) {
        return ((errorString)"tracing is already enabled"u8);
    }
    // Block until cleanup of the last trace is done.
    semacquire(Ꮡ(traceShutdownSema));
    semrelease(Ꮡ(traceShutdownSema));
    // Hold traceAdvanceSema across trace start, since we'll want it on
    // the other side of tracing being enabled globally.
    semacquire(Ꮡ(traceAdvanceSema));
    // Initialize CPU profile -> trace ingestion.
    traceInitReadCPU();
    // Compute the first generation for this StartTrace.
    //
    // Note: we start from the last non-zero generation rather than 1 so we
    // can avoid resetting all the arrays indexed by gen%2 or gen%3. There's
    // more than one of each per m, p, and goroutine.
    var firstGen = traceNextGen(Δtrace.lastNonZeroGen);
    // Reset GC sequencer.
    Δtrace.seqGC = 1;
    // Reset trace reader state.
    Δtrace.headerWritten = false;
    Δtrace.readerGen.Store(firstGen);
    Δtrace.flushedGen.Store(0);
    // Register some basic strings in the string tables.
    traceRegisterLabelsAndReasons(firstGen);
    // Stop the world.
    //
    // The purpose of stopping the world is to make sure that no goroutine is in a
    // context where it could emit an event by bringing all goroutines to a safe point
    // with no opportunity to transition.
    //
    // The exception to this rule are goroutines that are concurrently exiting a syscall.
    // Those will all be forced into the syscalling slow path, and we'll just make sure
    // that we don't observe any goroutines in that critical section before starting
    // the world again.
    //
    // A good follow-up question to this is why stopping the world is necessary at all
    // given that we have traceAcquire and traceRelease. Unfortunately, those only help
    // us when tracing is already active (for performance, so when tracing is off the
    // tracing seqlock is left untouched). The main issue here is subtle: we're going to
    // want to obtain a correct starting status for each goroutine, but there are windows
    // of time in which we could read and emit an incorrect status. Specifically:
    //
    //	trace := traceAcquire()
    //  // <----> problem window
    //	casgstatus(gp, _Gwaiting, _Grunnable)
    //	if trace.ok() {
    //		trace.GoUnpark(gp, 2)
    //		traceRelease(trace)
    //	}
    //
    // More precisely, if we readgstatus for a gp while another goroutine is in the problem
    // window and that goroutine didn't observe that tracing had begun, then we might write
    // a GoStatus(GoWaiting) event for that goroutine, but it won't trace an event marking
    // the transition from GoWaiting to GoRunnable. The trace will then be broken, because
    // future events will be emitted assuming the tracer sees GoRunnable.
    //
    // In short, what we really need here is to make sure that the next time *any goroutine*
    // hits a traceAcquire, it sees that the trace is enabled.
    //
    // Note also that stopping the world is necessary to make sure sweep-related events are
    // coherent. Since the world is stopped and sweeps are non-preemptible, we can never start
    // the world and see an unpaired sweep 'end' event. Other parts of the tracer rely on this.
    var stw = stopTheWorld(stwStartTrace);
    // Prevent sysmon from running any code that could generate events.
    @lock(Ꮡsched.of(schedt.Ꮡsysmonlock));
    // Grab the minimum page heap address. All Ps are stopped, so it's safe to read this since
    // nothing can allocate heap memory.
    Δtrace.minPageHeapAddr = ((uint64)mheap_.pages.inUse.ranges[0].@base.addr());
    // Reset mSyscallID on all Ps while we have them stationary and the trace is disabled.
    foreach (var (_, pp) in allp) {
        (~pp).trace.mSyscallID = -1;
    }
    // Start tracing.
    //
    // Set trace.enabled. This is *very* subtle. We need to maintain the invariant that if
    // trace.gen != 0, then trace.enabled is always observed as true. Simultaneously, for
    // performance, we need trace.enabled to be read without any synchronization.
    //
    // We ensure this is safe by stopping the world, which acts a global barrier on almost
    // every M, and explicitly synchronize with any other Ms that could be running concurrently
    // with us. Today, there are only two such cases:
    // - sysmon, which we synchronized with by acquiring sysmonlock.
    // - goroutines exiting syscalls, which we synchronize with via trace.exitingSyscall.
    //
    // After trace.gen is updated, other Ms may start creating trace buffers and emitting
    // data into them.
    Δtrace.enabled = true;
    if (debug.traceallocfree.Load() != 0) {
        // Enable memory events since the GODEBUG is set.
        Δtrace.debugMalloc = debug.malloc;
        Δtrace.enabledWithAllocFree = true;
        debug.malloc = true;
    }
    Δtrace.gen.Store(firstGen);
    // Wait for exitingSyscall to drain.
    //
    // It may not monotonically decrease to zero, but in the limit it will always become
    // zero because the world is stopped and there are no available Ps for syscall-exited
    // goroutines to run on.
    //
    // Because we set gen before checking this, and because exitingSyscall is always incremented
    // *before* traceAcquire (which checks gen), we can be certain that when exitingSyscall is zero
    // that any goroutine that goes to exit a syscall from then on *must* observe the new gen as
    // well as trace.enabled being set to true.
    //
    // The critical section on each goroutine here is going to be quite short, so the likelihood
    // that we observe a zero value is high.
    while (Δtrace.exitingSyscall.Load() != 0) {
        osyield();
    }
    // Record some initial pieces of information.
    //
    // N.B. This will also emit a status event for this goroutine.
    var tl = traceAcquire();
    tl.Gomaxprocs(gomaxprocs);
    // Get this as early in the trace as possible. See comment in traceAdvance.
    tl.STWStart(stwStartTrace);
    // We didn't trace this above, so trace it now.
    // Record the fact that a GC is active, if applicable.
    if (gcphase == _GCmark || gcphase == _GCmarktermination) {
        tl.GCActive();
    }
    // Dump a snapshot of memory, if enabled.
    if (Δtrace.enabledWithAllocFree) {
        traceSnapshotMemory(firstGen);
    }
    // Record the heap goal so we have it at the very beginning of the trace.
    tl.HeapGoal();
    // Make sure a ProcStatus is emitted for every P, while we're here.
    foreach (var (_, pp) in allp) {
        tl.writer().writeProcStatusForP(pp, pp == (~tl.mp).p.ptr()).end();
    }
    traceRelease(tl);
    unlock(Ꮡsched.of(schedt.Ꮡsysmonlock));
    startTheWorld(stw);
    traceStartReadCPU();
    traceAdvancer.start();
    semrelease(Ꮡ(traceAdvanceSema));
    return default!;
}

// StopTrace stops tracing, if it was previously enabled.
// StopTrace only returns after all the reads for the trace have completed.
public static void StopTrace() {
    traceAdvance(true);
}

// Collect all the untraced Gs.
[GoType("dyn")] partial struct traceAdvance_untracedG {
    internal ж<g> gp;
    internal uint64 goid;
    internal int64 mid;
    internal uint64 stackID;
    internal uint32 status;
    internal waitReason waitreason;
    internal bool inMarkAssist;
}

// traceAdvance moves tracing to the next generation, and cleans up the current generation,
// ensuring that it's flushed out before returning. If stopTrace is true, it disables tracing
// altogether instead of advancing to the next generation.
//
// traceAdvanceSema must not be held.
//
// traceAdvance is called by golang.org/x/exp/trace using linkname.
//
//go:linkname traceAdvance
internal static void traceAdvance(bool stopTrace) {
    semacquire(Ꮡ(traceAdvanceSema));
    // Get the gen that we're advancing from. In this function we don't really care much
    // about the generation we're advancing _into_ since we'll do all the cleanup in this
    // generation for the next advancement.
    ref var gen = ref heap<uintptr>(out var Ꮡgen);
    gen = Δtrace.gen.Load();
    if (gen == 0) {
        // We may end up here traceAdvance is called concurrently with StopTrace.
        semrelease(Ꮡ(traceAdvanceSema));
        return;
    }
    // Write an EvFrequency event for this generation.
    //
    // N.B. This may block for quite a while to get a good frequency estimate, so make sure we do
    // this here and not e.g. on the trace reader.
    traceFrequency(gen);
    slice<untracedG> untracedGs = default!;
    forEachGRace(
    var genʗ5 = gen;
    var untracedGsʗ2 = untracedGs;
    (ж<g> gp) => {
        (~gp).trace.readyNextGen(genʗ5);
        if ((~gp).trace.statusWasTraced(genʗ5)) {
            return;
        }
        ref var ug = ref heap<traceAdvance_untracedG>(out var Ꮡug);
        ug = new untracedG(gp: gp, mid: -1);
        systemstack(
        var genʗ7 = gen;
        var ugʗ5 = ug;
        () => {
            var me = (~getg()).m.val.curg;
            casGToWaitingForGC(me, _Grunning, waitReasonTraceGoroutineStatus);
            ref var s = ref heap<suspendGState>(out var Ꮡs);
            s = suspendG(gp);
            if (!s.dead) {
                ugʗ5.goid = s.g.val.goid;
                if ((~s.g).m != nil) {
                    ugʗ5.mid = ((int64)(~(~s.g).m).procid);
                }
                ugʗ5.status = (uint32)(readgstatus(s.g) & ~_Gscan);
                ugʗ5.waitreason = s.g.val.waitreason;
                ugʗ5.inMarkAssist = s.g.val.inMarkAssist;
                ugʗ5.stackID = traceStack(0, gp, genʗ7);
            }
            resumeG(s);
            casgstatus(me, _Gwaiting, _Grunning);
        });
        if (ug.goid != 0) {
            untracedGs = append(untracedGs, ug);
        }
    });
    if (!stopTrace) {
        // Re-register runtime goroutine labels and stop/block reasons.
        traceRegisterLabelsAndReasons(traceNextGen(gen));
    }
    // Now that we've done some of the heavy stuff, prevent the world from stopping.
    // This is necessary to ensure the consistency of the STW events. If we're feeling
    // adventurous we could lift this restriction and add a STWActive event, but the
    // cost of maintaining this consistency is low. We're not going to hold this semaphore
    // for very long and most STW periods are very short.
    // Once we hold worldsema, prevent preemption as well so we're not interrupted partway
    // through this. We want to get this done as soon as possible.
    semacquire(Ꮡ(worldsema));
    var mp = acquirem();
    // Advance the generation or stop the trace.
    Δtrace.lastNonZeroGen = gen;
    if (stopTrace){
        systemstack(
        var traceʗ2 = Δtrace;
        () => {
            @lock(Ꮡtraceʗ2.of(Δtrace.Ꮡlock));
            traceʗ2.shutdown.Store(true);
            traceʗ2.gen.Store(0);
            unlock(Ꮡtraceʗ2.of(Δtrace.Ꮡlock));
            traceʗ2.enabled = false;
        });
    } else {
        Δtrace.gen.Store(traceNextGen(gen));
    }
    // Emit a ProcsChange event so we have one on record for each generation.
    // Let's emit it as soon as possible so that downstream tools can rely on the value
    // being there fairly soon in a generation.
    //
    // It's important that we do this before allowing stop-the-worlds again,
    // because the procs count could change.
    if (!stopTrace) {
        var tl = traceAcquire();
        tl.Gomaxprocs(gomaxprocs);
        traceRelease(tl);
    }
    // Emit a GCActive event in the new generation if necessary.
    //
    // It's important that we do this before allowing stop-the-worlds again,
    // because that could emit global GC-related events.
    if (!stopTrace && (gcphase == _GCmark || gcphase == _GCmarktermination)) {
        var tl = traceAcquire();
        tl.GCActive();
        traceRelease(tl);
    }
    // Preemption is OK again after this. If the world stops or whatever it's fine.
    // We're just cleaning up the last generation after this point.
    //
    // We also don't care if the GC starts again after this for the same reasons.
    releasem(mp);
    semrelease(Ꮡ(worldsema));
    // Snapshot allm and freem.
    //
    // Snapshotting after the generation counter update is sufficient.
    // Because an m must be on either allm or sched.freem if it has an active trace
    // buffer, new threads added to allm after this point must necessarily observe
    // the new generation number (sched.lock acts as a barrier).
    //
    // Threads that exit before this point and are on neither list explicitly
    // flush their own buffers in traceThreadDestroy.
    //
    // Snapshotting freem is necessary because Ms can continue to emit events
    // while they're still on that list. Removal from sched.freem is serialized with
    // this snapshot, so either we'll capture an m on sched.freem and race with
    // the removal to flush its buffers (resolved by traceThreadDestroy acquiring
    // the thread's seqlock, which one of us must win, so at least its old gen buffer
    // will be flushed in time for the new generation) or it will have flushed its
    // buffers before we snapshotted it to begin with.
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    var mToFlush = allm;
    for (var mpΔ1 = mToFlush; mpΔ1 != nil; mp = mpΔ1.val.alllink) {
        (~mp).trace.link = mpΔ1.val.alllink;
    }
    for (var mpΔ2 = sched.freem; mpΔ2 != nil; mp = mpΔ2.val.freelink) {
        (~mp).trace.link = mToFlush;
        mToFlush = mpΔ2;
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    // Iterate over our snapshot, flushing every buffer until we're done.
    //
    // Because trace writers read the generation while the seqlock is
    // held, we can be certain that when there are no writers there are
    // also no stale generation values left. Therefore, it's safe to flush
    // any buffers that remain in that generation's slot.
    const bool debugDeadlock = false;
    systemstack(
    var genʗ14 = gen;
    var mToFlushʗ2 = mToFlush;
    var traceʗ5 = Δtrace;
    () => {
        nint i = 0;
        var detectedDeadlock = false;
        while (mToFlushʗ2 != nil) {
            var prev = Ꮡ(mToFlushʗ2);
            for (var mpΔ3 = prev.val; mpΔ3 != nil; ) {
                if ((~mpΔ3).traceʗ5.seqlock.Load() % 2 != 0) {
                    prev = Ꮡ(~mpΔ3).traceʗ5.of(mTraceState.Ꮡlink);
                    mp = (~mpΔ3).traceʗ5.link;
                    continue;
                }
                @lock(Ꮡtraceʗ5.of(Δtrace.Ꮡlock));
                var bufp = Ꮡ(~mpΔ3).traceʗ5.buf.at<ж<traceBuf>>(genʗ14 % 2);
                if (bufp.val != nil) {
                    traceBufFlush(bufp.val, genʗ14);
                    bufp.val = default!;
                }
                unlock(Ꮡtraceʗ5.of(Δtrace.Ꮡlock));
                prev.val = (~mpΔ3).traceʗ5.link;
                (~mp).traceʗ5.link = default!;
                mp = prev.val;
            }
            if (mToFlushʗ2 != nil) {
                osyield();
            }
            if (debugDeadlock) {
                if (i > 100000 && !detectedDeadlock) {
                    detectedDeadlock = true;
                    println("runtime: failing to flush");
                    for (var mpΔ4 = mToFlushʗ2; mpΔ4 != nil; mp = (~mpΔ4).traceʗ5.link) {
                        print("runtime: m=", (~mpΔ4).id, "\n");
                    }
                }
                i++;
            }
        }
    });
    // At this point, the old generation is fully flushed minus stack and string
    // tables, CPU samples, and goroutines that haven't run at all during the last
    // generation.
    // Check to see if any Gs still haven't had events written out for them.
    var statusWriter = unsafeTraceWriter(gen, nil);
    foreach (var (_, ug) in untracedGs) {
        if ((~ug.gp).trace.statusWasTraced(gen)) {
            // It was traced, we don't need to do anything.
            continue;
        }
        // It still wasn't traced. Because we ensured all Ms stopped writing trace
        // events to the last generation, that must mean the G never had its status
        // traced in gen between when we recorded it and now. If that's true, the goid
        // and status we recorded then is exactly what we want right now.
        var status = goStatusToTraceGoStatus(ug.status, ug.waitreason);
        statusWriter = statusWriter.writeGoStatus(ug.goid, ug.mid, status, ug.inMarkAssist, ug.stackID);
    }
    statusWriter.flush().end();
    // Read everything out of the last gen's CPU profile buffer.
    traceReadCPU(gen);
    // Flush CPU samples, stacks, and strings for the last generation. This is safe,
    // because we're now certain no M is writing to the last generation.
    //
    // Ordering is important here. traceCPUFlush may generate new stacks and dumping
    // stacks may generate new strings.
    traceCPUFlush(gen);
    Δtrace.stackTab[gen % 2].dump(gen);
    Δtrace.typeTab[gen % 2].dump(gen);
    Δtrace.stringTab[gen % 2].reset(gen);
    // That's it. This generation is done producing buffers.
    systemstack(
    var genʗ17 = gen;
    var traceʗ8 = Δtrace;
    () => {
        @lock(Ꮡtraceʗ8.of(Δtrace.Ꮡlock));
        traceʗ8.flushedGen.Store(genʗ17);
        unlock(Ꮡtraceʗ8.of(Δtrace.Ꮡlock));
    });
    // Perform status reset on dead Ps because they just appear as idle.
    //
    // Preventing preemption is sufficient to access allp safely. allp is only
    // mutated by GOMAXPROCS calls, which require a STW.
    //
    // TODO(mknyszek): Consider explicitly emitting ProcCreate and ProcDestroy
    // events to indicate whether a P exists, rather than just making its
    // existence implicit.
    mp = acquirem();
    foreach (var (_, pp) in allp[(int)(len(allp))..(int)(cap(allp))]) {
        (~pp).trace.readyNextGen(traceNextGen(gen));
    }
    releasem(mp);
    if (stopTrace){
        // Acquire the shutdown sema to begin the shutdown process.
        semacquire(Ꮡ(traceShutdownSema));
        // Finish off CPU profile reading.
        traceStopReadCPU();
        // Reset debug.malloc if necessary. Note that this is set in a racy
        // way; that's OK. Some mallocs may still enter into the debug.malloc
        // block, but they won't generate events because tracing is disabled.
        // That is, it's OK if mallocs read a stale debug.malloc or
        // trace.enabledWithAllocFree value.
        if (Δtrace.enabledWithAllocFree) {
            Δtrace.enabledWithAllocFree = false;
            debug.malloc = Δtrace.debugMalloc;
        }
    } else {
        // Go over each P and emit a status event for it if necessary.
        //
        // We do this at the beginning of the new generation instead of the
        // end like we do for goroutines because forEachP doesn't give us a
        // hook to skip Ps that have already been traced. Since we have to
        // preempt all Ps anyway, might as well stay consistent with StartTrace
        // which does this during the STW.
        semacquire(Ꮡ(worldsema));
        forEachP(waitReasonTraceProcStatus, 
        (ж<Δp> pp) => {
            ref var tl = ref heap<traceLocker>(out var Ꮡtl);
            tl = traceAcquire();
            if (!(~pp).trace.statusWasTraced(tl.gen)) {
                tl.writer().writeProcStatusForP(pp, false).end();
            }
            traceRelease(tl);
        });
        semrelease(Ꮡ(worldsema));
    }
    // Block until the trace reader has finished processing the last generation.
    semacquire(ᏑΔtrace.doneSema.at<uint32>(gen % 2));
    if (raceenabled) {
        raceacquire(new @unsafe.Pointer(ᏑΔtrace.doneSema.at<uint32>(gen % 2)));
    }
    // Double-check that things look as we expect after advancing and perform some
    // final cleanup if the trace has fully stopped.
    systemstack(
    var genʗ20 = gen;
    var memstatsʗ2 = memstats;
    var traceʗ11 = Δtrace;
    () => {
        @lock(Ꮡtraceʗ11.of(Δtrace.Ꮡlock));
        if (!traceʗ11.full[genʗ20 % 2].empty()) {
            @throw("trace: non-empty full trace buffer for done generation"u8);
        }
        if (stopTrace) {
            if (!traceʗ11.full[1 - (genʗ20 % 2)].empty()) {
                @throw("trace: non-empty full trace buffer for next generation"u8);
            }
            if (traceʗ11.reading != nil || traceʗ11.reader.Load() != nil) {
                @throw("trace: reading after shutdown"u8);
            }
            while (traceʗ11.empty != nil) {
                var buf = traceʗ11.empty;
                traceʗ11.empty = buf.link;
                sysFree(new @unsafe.Pointer(buf), @unsafe.Sizeof(buf.val), Ꮡmemstatsʗ2.of(mstats.Ꮡother_sys));
            }
            traceʗ11.headerWritten = false;
            traceʗ11.shutdown.Store(false);
        }
        unlock(Ꮡtraceʗ11.of(Δtrace.Ꮡlock));
    });
    if (stopTrace) {
        // Clear the sweep state on every P for the next time tracing is enabled.
        //
        // It may be stale in the next trace because we may have ended tracing in
        // the middle of a sweep on a P.
        //
        // It's fine not to call forEachP here because tracing is disabled and we
        // know at this point that nothing is calling into the tracer, but we do
        // need to look at dead Ps too just because GOMAXPROCS could have been called
        // at any point since we stopped tracing, and we have to ensure there's no
        // bad state on dead Ps too. Prevent a STW and a concurrent GOMAXPROCS that
        // might mutate allp by making ourselves briefly non-preemptible.
        var mpΔ5 = acquirem();
        foreach (var (_, pp) in allp[..(int)(cap(allp))]) {
            (~pp).trace.inSweep = false;
            (~pp).trace.maySweep = false;
            (~pp).trace.swept = 0;
            (~pp).trace.reclaimed = 0;
        }
        releasem(mpΔ5);
    }
    // Release the advance semaphore. If stopTrace is true we're still holding onto
    // traceShutdownSema.
    //
    // Do a direct handoff. Don't let one caller of traceAdvance starve
    // other calls to traceAdvance.
    semrelease1(Ꮡ(traceAdvanceSema), true, 0);
    if (stopTrace) {
        // Stop the traceAdvancer. We can't be holding traceAdvanceSema here because
        // we'll deadlock (we're blocked on the advancer goroutine exiting, but it
        // may be currently trying to acquire traceAdvanceSema).
        traceAdvancer.stop();
        semrelease(Ꮡ(traceShutdownSema));
    }
}

internal static uintptr traceNextGen(uintptr gen) {
    if (gen == ^((uintptr)0)) {
        // gen is used both %2 and %3 and we want both patterns to continue when we loop around.
        // ^uint32(0) and ^uint64(0) are both odd and multiples of 3. Therefore the next generation
        // we want is even and one more than a multiple of 3. The smallest such number is 4.
        return 4;
    }
    return gen + 1;
}

// traceRegisterLabelsAndReasons re-registers mark worker labels and
// goroutine stop/block reasons in the string table for the provided
// generation. Note: the provided generation must not have started yet.
internal static void traceRegisterLabelsAndReasons(uintptr gen) {
    foreach (var (i, label) in gcMarkWorkerModeStrings[..]) {
        Δtrace.markWorkerLabels[gen % 2][i] = ((traceArg)Δtrace.stringTab[gen % 2].put(gen, label));
    }
    foreach (var (i, str) in traceBlockReasonStrings[..]) {
        Δtrace.goBlockReasons[gen % 2][i] = ((traceArg)Δtrace.stringTab[gen % 2].put(gen, str));
    }
    foreach (var (i, str) in traceGoStopReasonStrings[..]) {
        Δtrace.goStopReasons[gen % 2][i] = ((traceArg)Δtrace.stringTab[gen % 2].put(gen, str));
    }
}

// ReadTrace returns the next chunk of binary tracing data, blocking until data
// is available. If tracing is turned off and all the data accumulated while it
// was on has been returned, ReadTrace returns nil. The caller must copy the
// returned data before calling ReadTrace again.
// ReadTrace must be called from one goroutine at a time.
public static slice<byte> ReadTrace() {
top:
    slice<byte> buf = default!;
    bool park = default!;
    systemstack(
    var bufʗ2 = buf;
    () => {
        (bufʗ2, park) = readTrace0();
    });
    if (park) {
        gopark(
        var traceʗ1 = Δtrace;
        (ж<g> gp, @unsafe.Pointer _) => {
            if (!traceʗ1.reader.CompareAndSwapNoWB(nil, gp)) {
                // We're racing with another reader.
                // Wake up and handle this case.
                return false;
            }
            {
                var g2 = traceReader(); if (gp == g2){
                    // New data arrived between unlocking
                    // and the CAS and we won the wake-up
                    // race, so wake up directly.
                    return false;
                } else 
                if (g2 != nil) {
                    printlock();
                    println("runtime: got trace reader", g2, (~g2).goid);
                    @throw("unexpected trace reader"u8);
                }
            }
            return true;
        }, nil, waitReasonTraceReaderBlocked, traceBlockSystemGoroutine, 2);
        goto top;
    }
    return buf;
}

// readTrace0 is ReadTrace's continuation on g0. This must run on the
// system stack because it acquires trace.lock.
//
//go:systemstack
internal static (slice<byte> buf, bool park) readTrace0() => func((defer, _) => {
    slice<byte> buf = default!;
    bool park = default!;

    if (raceenabled) {
        // g0 doesn't have a race context. Borrow the user G's.
        if ((~getg()).racectx != 0) {
            @throw("expected racectx == 0"u8);
        }
        getg().val.racectx = (~(~getg()).m).curg.val.racectx;
        // (This defer should get open-coded, which is safe on
        // the system stack.)
        defer(() => {
            getg().val.racectx = 0;
        });
    }
    // This function must not allocate while holding trace.lock:
    // allocation can call heap allocate, which will try to emit a trace
    // event while holding heap lock.
    @lock(ᏑΔtrace.of(Δtrace.Ꮡlock));
    if (Δtrace.reader.Load() != nil) {
        // More than one goroutine reads trace. This is bad.
        // But we rather do not crash the program because of tracing,
        // because tracing can be enabled at runtime on prod servers.
        unlock(ᏑΔtrace.of(Δtrace.Ꮡlock));
        println("runtime: ReadTrace called from multiple goroutines simultaneously");
        return (default!, false);
    }
    // Recycle the old buffer.
    {
        var bufΔ1 = Δtrace.reading; if (bufΔ1 != nil) {
            bufΔ1.link = Δtrace.empty;
            Δtrace.empty = bufΔ1;
            Δtrace.reading = default!;
        }
    }
    // Write trace header.
    if (!Δtrace.headerWritten) {
        Δtrace.headerWritten = true;
        unlock(ᏑΔtrace.of(Δtrace.Ꮡlock));
        return (slice<byte>("go 1.23 trace\x00\x00\x00"), false);
    }
    // Read the next buffer.
    if (Δtrace.readerGen.Load() == 0) {
        Δtrace.readerGen.Store(1);
    }
    ref var gen = ref heap(new uintptr(), out var Ꮡgen);
    while (ᐧ) {
        assertLockHeld(ᏑΔtrace.of(Δtrace.Ꮡlock));
        gen = Δtrace.readerGen.Load();
        // Check to see if we need to block for more data in this generation
        // or if we need to move our generation forward.
        if (!Δtrace.full[gen % 2].empty()) {
            break;
        }
        // Most of the time readerGen is one generation ahead of flushedGen, as the
        // current generation is being read from. Then, once the last buffer is flushed
        // into readerGen, flushedGen will rise to meet it. At this point, the tracer
        // is waiting on the reader to finish flushing the last generation so that it
        // can continue to advance.
        if (Δtrace.flushedGen.Load() == gen) {
            if (Δtrace.shutdown.Load()) {
                unlock(ᏑΔtrace.of(Δtrace.Ꮡlock));
                // Wake up anyone waiting for us to be done with this generation.
                //
                // Do this after reading trace.shutdown, because the thread we're
                // waking up is going to clear trace.shutdown.
                if (raceenabled) {
                    // Model synchronization on trace.doneSema, which te race
                    // detector does not see. This is required to avoid false
                    // race reports on writer passed to trace.Start.
                    racerelease(new @unsafe.Pointer(ᏑΔtrace.doneSema.at<uint32>(gen % 2)));
                }
                semrelease(ᏑΔtrace.doneSema.at<uint32>(gen % 2));
                // We're shutting down, and the last generation is fully
                // read. We're done.
                return (default!, false);
            }
            // The previous gen has had all of its buffers flushed, and
            // there's nothing else for us to read. Advance the generation
            // we're reading from and try again.
            Δtrace.readerGen.Store(Δtrace.gen.Load());
            unlock(ᏑΔtrace.of(Δtrace.Ꮡlock));
            // Wake up anyone waiting for us to be done with this generation.
            //
            // Do this after reading gen to make sure we can't have the trace
            // advance until we've read it.
            if (raceenabled) {
                // See comment above in the shutdown case.
                racerelease(new @unsafe.Pointer(ᏑΔtrace.doneSema.at<uint32>(gen % 2)));
            }
            semrelease(ᏑΔtrace.doneSema.at<uint32>(gen % 2));
            // Reacquire the lock and go back to the top of the loop.
            @lock(ᏑΔtrace.of(Δtrace.Ꮡlock));
            continue;
        }
        // Wait for new data.
        //
        // We don't simply use a note because the scheduler
        // executes this goroutine directly when it wakes up
        // (also a note would consume an M).
        //
        // Before we drop the lock, clear the workAvailable flag. Work can
        // only be queued with trace.lock held, so this is at least true until
        // we drop the lock.
        Δtrace.workAvailable.Store(false);
        unlock(ᏑΔtrace.of(Δtrace.Ꮡlock));
        return (default!, true);
    }
    // Pull a buffer.
    var tbuf = Δtrace.full[gen % 2].pop();
    Δtrace.reading = tbuf;
    unlock(ᏑΔtrace.of(Δtrace.Ꮡlock));
    return ((~tbuf).arr[..(int)(tbuf.pos)], false);
});

// traceReader returns the trace reader that should be woken up, if any.
// Callers should first check (traceEnabled() || traceShuttingDown()).
//
// This must run on the system stack because it acquires trace.lock.
//
//go:systemstack
internal static ж<g> traceReader() {
    var gp = traceReaderAvailable();
    if (gp == nil || !Δtrace.reader.CompareAndSwapNoWB(gp, nil)) {
        return default!;
    }
    return gp;
}

// traceReaderAvailable returns the trace reader if it is not currently
// scheduled and should be. Callers should first check that
// (traceEnabled() || traceShuttingDown()) is true.
internal static ж<g> traceReaderAvailable() {
    // There are three conditions under which we definitely want to schedule
    // the reader:
    // - The reader is lagging behind in finishing off the last generation.
    //   In this case, trace buffers could even be empty, but the trace
    //   advancer will be waiting on the reader, so we have to make sure
    //   to schedule the reader ASAP.
    // - The reader has pending work to process for it's reader generation
    //   (assuming readerGen is not lagging behind). Note that we also want
    //   to be careful *not* to schedule the reader if there's no work to do.
    // - The trace is shutting down. The trace stopper blocks on the reader
    //   to finish, much like trace advancement.
    //
    // We also want to be careful not to schedule the reader if there's no
    // reason to.
    if (Δtrace.flushedGen.Load() == Δtrace.readerGen.Load() || Δtrace.workAvailable.Load() || Δtrace.shutdown.Load()) {
        return Δtrace.reader.Load();
    }
    return default!;
}

// Trace advancer goroutine.
internal static traceAdvancerState traceAdvancer;

[GoType] partial struct traceAdvancerState {
    internal ж<wakeableSleep> timer;
    internal channel<struct{}> done;
}

[GoType("dyn")] partial struct start_type {
}

// start starts a new traceAdvancer.
[GoRecv] internal static void start(this ref traceAdvancerState s) {
    // Start a goroutine to periodically advance the trace generation.
    s.done = new channel<struct{}>(1);
    s.timer = newWakeableSleep();
    var debugʗ1 = debug;
    goǃ(() => {
        while (traceEnabled()) {
            // Set a timer to wake us up
            s.timer.sleep(((int64)debugʗ1.traceadvanceperiod));
            // Try to advance the trace.
            traceAdvance(false);
        }
        s.done.ᐸꟷ(new start_type());
    });
}

// stop stops a traceAdvancer and blocks until it exits.
[GoRecv] internal static void stop(this ref traceAdvancerState s) {
    s.timer.wake();
    ᐸꟷ(s.done);
    close(s.done);
    s.timer.close();
}

// traceAdvancePeriod is the approximate period between
// new generations.
internal static readonly UntypedFloat defaultTraceAdvancePeriod = 1e+09; // 1 second.

// wakeableSleep manages a wakeable goroutine sleep.
//
// Users of this type must call init before first use and
// close to free up resources. Once close is called, init
// must be called before another use.
[GoType] partial struct wakeableSleep {
    internal ж<timer> timer;
    // lock protects access to wakeup, but not send/recv on it.
    internal mutex @lock;
    internal channel<struct{}> wakeup;
}

// newWakeableSleep initializes a new wakeableSleep and returns it.
internal static ж<wakeableSleep> newWakeableSleep() {
    var s = @new<wakeableSleep>();
    lockInit(Ꮡ((~s).@lock), lockRankWakeableSleep);
    s.val.wakeup = new channel<struct{}>(1);
    s.val.timer = @new<timer>();
    var f = (any s, uintptr _, int64 _) => {
        sΔ1._<wakeableSleep.val>().wake();
    };
    (~s).timer.init(f, s);
    return s;
}

// sleep sleeps for the provided duration in nanoseconds or until
// another goroutine calls wake.
//
// Must not be called by more than one goroutine at a time and
// must not be called concurrently with close.
[GoRecv] internal static void sleep(this ref wakeableSleep s, int64 ns) {
    s.timer.reset(nanotime() + ns, 0);
    @lock(Ꮡ(s.@lock));
    if (raceenabled) {
        raceacquire(new @unsafe.Pointer(Ꮡ(s.@lock)));
    }
    var wakeup = s.wakeup;
    if (raceenabled) {
        racerelease(new @unsafe.Pointer(Ꮡ(s.@lock)));
    }
    unlock(Ꮡ(s.@lock));
    ᐸꟷ(wakeup);
    s.timer.stop();
}

// wake awakens any goroutine sleeping on the timer.
//
// Safe for concurrent use with all other methods.
[GoRecv] internal static void wake(this ref wakeableSleep s) {
    // Grab the wakeup channel, which may be nil if we're
    // racing with close.
    @lock(Ꮡ(s.@lock));
    if (raceenabled) {
        raceacquire(new @unsafe.Pointer(Ꮡ(s.@lock)));
    }
    if (s.wakeup != default!) {
        // Non-blocking send.
        //
        // Others may also write to this channel and we don't
        // want to block on the receiver waking up. This also
        // effectively batches together wakeup notifications.
        switch (ᐧ) {
        case ᐧ: {
            break;
        }
        default: {
            break;
        }}
    }
    if (raceenabled) {
        racerelease(new @unsafe.Pointer(Ꮡ(s.@lock)));
    }
    unlock(Ꮡ(s.@lock));
}

// close wakes any goroutine sleeping on the timer and prevents
// further sleeping on it.
//
// Once close is called, the wakeableSleep must no longer be used.
//
// It must only be called once no goroutine is sleeping on the
// timer *and* nothing else will call wake concurrently.
[GoRecv] internal static void close(this ref wakeableSleep s) {
    // Set wakeup to nil so that a late timer ends up being a no-op.
    @lock(Ꮡ(s.@lock));
    if (raceenabled) {
        raceacquire(new @unsafe.Pointer(Ꮡ(s.@lock)));
    }
    var wakeup = s.wakeup;
    s.wakeup = default!;
    // Close the channel.
    close(wakeup);
    if (raceenabled) {
        racerelease(new @unsafe.Pointer(Ꮡ(s.@lock)));
    }
    unlock(Ꮡ(s.@lock));
    return;
}

} // end runtime_package
