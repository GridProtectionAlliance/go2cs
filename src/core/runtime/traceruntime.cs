// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Runtime -> tracer API.
namespace go;

using atomic = @internal.runtime.atomic_package;
using _ = unsafe_package; // for go:linkname
using @internal.runtime;

partial class runtime_package {

// gTraceState is per-G state for the tracer.
[GoType] partial struct gTraceState {
    internal partial ref traceSchedResourceState traceSchedResourceState { get; }
}

// reset resets the gTraceState for a new goroutine.
[GoRecv] internal static void reset(this ref gTraceState s) {
    s.seq = new uint64[]{}.array();
}

// N.B. s.statusTraced is managed and cleared separately.

// mTraceState is per-M state for the tracer.
[GoType] partial struct mTraceState {
    internal @internal.runtime.atomic_package.Uintptr seqlock; // seqlock indicating that this M is writing to a trace buffer.
    internal array<ж<traceBuf>> buf = new(2); // Per-M traceBuf for writing. Indexed by trace.gen%2.
    internal ж<m> link;          // Snapshot of alllink or freelink.
}

// pTraceState is per-P state for the tracer.
[GoType] partial struct pTraceState {
    internal partial ref traceSchedResourceState traceSchedResourceState { get; }
    // mSyscallID is the ID of the M this was bound to before entering a syscall.
    internal int64 mSyscallID;
    // maySweep indicates the sweep events should be traced.
    // This is used to defer the sweep start event until a span
    // has actually been swept.
    internal bool maySweep;
    // inSweep indicates that at least one sweep event has been traced.
    internal bool inSweep;
    // swept and reclaimed track the number of bytes swept and reclaimed
    // by sweeping in the current sweep loop (while maySweep was true).
    internal uintptr swept;
    internal uintptr reclaimed;
}

// traceLockInit initializes global trace locks.
internal static void traceLockInit() {
    // Sharing a lock rank here is fine because they should never be accessed
    // together. If they are, we want to find out immediately.
    lockInit(ᏑΔtrace.stringTab[0].of(traceStringTable.Ꮡlock), lockRankTraceStrings);
    lockInit(ᏑΔtrace.stringTab[0].tab.mem.of(traceRegionAlloc.Ꮡlock), lockRankTraceStrings);
    lockInit(ᏑΔtrace.stringTab[1].of(traceStringTable.Ꮡlock), lockRankTraceStrings);
    lockInit(ᏑΔtrace.stringTab[1].tab.mem.of(traceRegionAlloc.Ꮡlock), lockRankTraceStrings);
    lockInit(ᏑΔtrace.stackTab[0].tab.mem.of(traceRegionAlloc.Ꮡlock), lockRankTraceStackTab);
    lockInit(ᏑΔtrace.stackTab[1].tab.mem.of(traceRegionAlloc.Ꮡlock), lockRankTraceStackTab);
    lockInit(ᏑΔtrace.typeTab[0].tab.mem.of(traceRegionAlloc.Ꮡlock), lockRankTraceTypeTab);
    lockInit(ᏑΔtrace.typeTab[1].tab.mem.of(traceRegionAlloc.Ꮡlock), lockRankTraceTypeTab);
    lockInit(ᏑΔtrace.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock), lockRankTrace);
}

// lockRankMayTraceFlush records the lock ranking effects of a
// potential call to traceFlush.
//
// nosplit because traceAcquire is nosplit.
//
//go:nosplit
internal static void lockRankMayTraceFlush() {
    lockWithRankMayAcquire(ᏑΔtrace.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock), getLockRank(ᏑΔtrace.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock)));
}

[GoType("num:uint8")] partial struct traceBlockReason;

internal static readonly traceBlockReason traceBlockGeneric = /* iota */ 0;
internal static readonly traceBlockReason traceBlockForever = 1;
internal static readonly traceBlockReason traceBlockNet = 2;
internal static readonly traceBlockReason traceBlockSelect = 3;
internal static readonly traceBlockReason traceBlockCondWait = 4;
internal static readonly traceBlockReason traceBlockSync = 5;
internal static readonly traceBlockReason traceBlockChanSend = 6;
internal static readonly traceBlockReason traceBlockChanRecv = 7;
internal static readonly traceBlockReason traceBlockGCMarkAssist = 8;
internal static readonly traceBlockReason traceBlockGCSweep = 9;
internal static readonly traceBlockReason traceBlockSystemGoroutine = 10;
internal static readonly traceBlockReason traceBlockPreempted = 11;
internal static readonly traceBlockReason traceBlockDebugCall = 12;
internal static readonly traceBlockReason traceBlockUntilGCEnds = 13;
internal static readonly traceBlockReason traceBlockSleep = 14;

internal static array<@string> traceBlockReasonStrings = new runtime.SparseArray<@string>{
    [traceBlockGeneric] = "unspecified"u8,
    [traceBlockForever] = "forever"u8,
    [traceBlockNet] = "network"u8,
    [traceBlockSelect] = "select"u8,
    [traceBlockCondWait] = "sync.(*Cond).Wait"u8,
    [traceBlockSync] = "sync"u8,
    [traceBlockChanSend] = "chan send"u8,
    [traceBlockChanRecv] = "chan receive"u8,
    [traceBlockGCMarkAssist] = "GC mark assist wait for work"u8,
    [traceBlockGCSweep] = "GC background sweeper wait"u8,
    [traceBlockSystemGoroutine] = "system goroutine wait"u8,
    [traceBlockPreempted] = "preempted"u8,
    [traceBlockDebugCall] = "wait for debug call"u8,
    [traceBlockUntilGCEnds] = "wait until GC ends"u8,
    [traceBlockSleep] = "sleep"u8
}.array();

[GoType("num:uint8")] partial struct traceGoStopReason;

internal static readonly traceGoStopReason traceGoStopGeneric = /* iota */ 0;
internal static readonly traceGoStopReason traceGoStopGoSched = 1;
internal static readonly traceGoStopReason traceGoStopPreempted = 2;

internal static array<@string> traceGoStopReasonStrings = new runtime.SparseArray<@string>{
    [traceGoStopGeneric] = "unspecified"u8,
    [traceGoStopGoSched] = "runtime.Gosched"u8,
    [traceGoStopPreempted] = "preempted"u8
}.array();

// traceEnabled returns true if the trace is currently enabled.
//
//go:nosplit
internal static bool traceEnabled() {
    return Δtrace.enabled;
}

// traceAllocFreeEnabled returns true if the trace is currently enabled
// and alloc/free events are also enabled.
//
//go:nosplit
internal static bool traceAllocFreeEnabled() {
    return Δtrace.enabledWithAllocFree;
}

// traceShuttingDown returns true if the trace is currently shutting down.
internal static bool traceShuttingDown() {
    return Δtrace.shutdown.Load();
}

// traceLocker represents an M writing trace events. While a traceLocker value
// is valid, the tracer observes all operations on the G/M/P or trace events being
// written as happening atomically.
[GoType] partial struct traceLocker {
    internal ж<m> mp;
    internal uintptr gen;
}

// debugTraceReentrancy checks if the trace is reentrant.
//
// This is optional because throwing in a function makes it instantly
// not inlineable, and we want traceAcquire to be inlineable for
// low overhead when the trace is disabled.
internal const bool debugTraceReentrancy = false;

// traceAcquire prepares this M for writing one or more trace events.
//
// nosplit because it's called on the syscall path when stack movement is forbidden.
//
//go:nosplit
internal static traceLocker traceAcquire() {
    if (!traceEnabled()) {
        return new traceLocker(nil);
    }
    return traceAcquireEnabled();
}

// traceTryAcquire is like traceAcquire, but may return an invalid traceLocker even
// if tracing is enabled. For example, it will return !ok if traceAcquire is being
// called with an active traceAcquire on the M (reentrant locking). This exists for
// optimistically emitting events in the few contexts where tracing is now allowed.
//
// nosplit for alignment with traceTryAcquire, so it can be used in the
// same contexts.
//
//go:nosplit
internal static traceLocker traceTryAcquire() {
    if (!traceEnabled()) {
        return new traceLocker(nil);
    }
    return traceTryAcquireEnabled();
}

// traceAcquireEnabled is the traceEnabled path for traceAcquire. It's explicitly
// broken out to make traceAcquire inlineable to keep the overhead of the tracer
// when it's disabled low.
//
// nosplit because it's called by traceAcquire, which is nosplit.
//
//go:nosplit
internal static traceLocker traceAcquireEnabled() {
    // Any time we acquire a traceLocker, we may flush a trace buffer. But
    // buffer flushes are rare. Record the lock edge even if it doesn't happen
    // this time.
    lockRankMayTraceFlush();
    // Prevent preemption.
    var mp = acquirem();
    // Acquire the trace seqlock. This prevents traceAdvance from moving forward
    // until all Ms are observed to be outside of their seqlock critical section.
    //
    // Note: The seqlock is mutated here and also in traceCPUSample. If you update
    // usage of the seqlock here, make sure to also look at what traceCPUSample is
    // doing.
    var seq = (~mp).trace.seqlock.Add(1);
    if (debugTraceReentrancy && seq % 2 != 1) {
        @throw("bad use of trace.seqlock or tracer is reentrant"u8);
    }
    // N.B. This load of gen appears redundant with the one in traceEnabled.
    // However, it's very important that the gen we use for writing to the trace
    // is acquired under a traceLocker so traceAdvance can make sure no stale
    // gen values are being used.
    //
    // Because we're doing this load again, it also means that the trace
    // might end up being disabled when we load it. In that case we need to undo
    // what we did and bail.
    var gen = Δtrace.gen.Load();
    if (gen == 0) {
        (~mp).trace.seqlock.Add(1);
        releasem(mp);
        return new traceLocker(nil);
    }
    return new traceLocker(mp, gen);
}

// traceTryAcquireEnabled is like traceAcquireEnabled but may return an invalid
// traceLocker under some conditions. See traceTryAcquire for more details.
//
// nosplit for alignment with traceAcquireEnabled, so it can be used in the
// same contexts.
//
//go:nosplit
internal static traceLocker traceTryAcquireEnabled() {
    // Any time we acquire a traceLocker, we may flush a trace buffer. But
    // buffer flushes are rare. Record the lock edge even if it doesn't happen
    // this time.
    lockRankMayTraceFlush();
    // Check if we're already locked. If so, return an invalid traceLocker.
    if ((~(~getg()).m).trace.seqlock.Load() % 2 == 1) {
        return new traceLocker(nil);
    }
    return traceAcquireEnabled();
}

// ok returns true if the traceLocker is valid (i.e. tracing is enabled).
//
// nosplit because it's called on the syscall path when stack movement is forbidden.
//
//go:nosplit
internal static bool ok(this traceLocker tl) {
    return tl.gen != 0;
}

// traceRelease indicates that this M is done writing trace events.
//
// nosplit because it's called on the syscall path when stack movement is forbidden.
//
//go:nosplit
internal static void traceRelease(traceLocker tl) {
    var seq = tl.mp.trace.seqlock.Add(1);
    if (debugTraceReentrancy && seq % 2 != 0) {
        print("runtime: seq=", seq, "\n");
        @throw("bad use of trace.seqlock"u8);
    }
    releasem(tl.mp);
}

// traceExitingSyscall marks a goroutine as exiting the syscall slow path.
//
// Must be paired with a traceExitedSyscall call.
internal static void traceExitingSyscall() {
    Δtrace.exitingSyscall.Add(1);
}

// traceExitedSyscall marks a goroutine as having exited the syscall slow path.
internal static void traceExitedSyscall() {
    Δtrace.exitingSyscall.Add(-1);
}

// Gomaxprocs emits a ProcsChange event.
internal static void Gomaxprocs(this traceLocker tl, int32 procs) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvProcsChange, ((traceArg)procs), tl.stack(1));
}

// ProcStart traces a ProcStart event.
//
// Must be called with a valid P.
internal static void ProcStart(this traceLocker tl) {
    var pp = tl.mp.p.ptr();
    // Procs are typically started within the scheduler when there is no user goroutine. If there is a user goroutine,
    // it must be in _Gsyscall because the only time a goroutine is allowed to have its Proc moved around from under it
    // is during a syscall.
    tl.eventWriter(traceGoSyscall, traceProcIdle).commit(traceEvProcStart, ((traceArg)(~pp).id), (~pp).trace.nextSeq(tl.gen));
}

// ProcStop traces a ProcStop event.
internal static void ProcStop(this traceLocker tl, ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.val;

    // The only time a goroutine is allowed to have its Proc moved around
    // from under it is during a syscall.
    tl.eventWriter(traceGoSyscall, traceProcRunning).commit(traceEvProcStop);
}

// GCActive traces a GCActive event.
//
// Must be emitted by an actively running goroutine on an active P. This restriction can be changed
// easily and only depends on where it's currently called.
internal static void GCActive(this traceLocker tl) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGCActive, ((traceArg)Δtrace.seqGC));
    // N.B. Only one GC can be running at a time, so this is naturally
    // serialized by the caller.
    Δtrace.seqGC++;
}

// GCStart traces a GCBegin event.
//
// Must be emitted by an actively running goroutine on an active P. This restriction can be changed
// easily and only depends on where it's currently called.
internal static void GCStart(this traceLocker tl) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGCBegin, ((traceArg)Δtrace.seqGC), tl.stack(3));
    // N.B. Only one GC can be running at a time, so this is naturally
    // serialized by the caller.
    Δtrace.seqGC++;
}

// GCDone traces a GCEnd event.
//
// Must be emitted by an actively running goroutine on an active P. This restriction can be changed
// easily and only depends on where it's currently called.
internal static void GCDone(this traceLocker tl) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGCEnd, ((traceArg)Δtrace.seqGC));
    // N.B. Only one GC can be running at a time, so this is naturally
    // serialized by the caller.
    Δtrace.seqGC++;
}

// STWStart traces a STWBegin event.
internal static void STWStart(this traceLocker tl, stwReason reason) {
    // Although the current P may be in _Pgcstop here, we model the P as running during the STW. This deviates from the
    // runtime's state tracking, but it's more accurate and doesn't result in any loss of information.
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvSTWBegin, tl.@string(reason.String()), tl.stack(2));
}

// STWDone traces a STWEnd event.
internal static void STWDone(this traceLocker tl) {
    // Although the current P may be in _Pgcstop here, we model the P as running during the STW. This deviates from the
    // runtime's state tracking, but it's more accurate and doesn't result in any loss of information.
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvSTWEnd);
}

// GCSweepStart prepares to trace a sweep loop. This does not
// emit any events until traceGCSweepSpan is called.
//
// GCSweepStart must be paired with traceGCSweepDone and there
// must be no preemption points between these two calls.
//
// Must be called with a valid P.
internal static void GCSweepStart(this traceLocker tl) {
    // Delay the actual GCSweepBegin event until the first span
    // sweep. If we don't sweep anything, don't emit any events.
    var pp = tl.mp.p.ptr();
    if ((~pp).trace.maySweep) {
        @throw("double traceGCSweepStart"u8);
    }
    ((~pp).trace.maySweep, (~pp).trace.swept, (~pp).trace.reclaimed) = (true, 0, 0);
}

// GCSweepSpan traces the sweep of a single span. If this is
// the first span swept since traceGCSweepStart was called, this
// will emit a GCSweepBegin event.
//
// This may be called outside a traceGCSweepStart/traceGCSweepDone
// pair; however, it will not emit any trace events in this case.
//
// Must be called with a valid P.
internal static void GCSweepSpan(this traceLocker tl, uintptr bytesSwept) {
    var pp = tl.mp.p.ptr();
    if ((~pp).trace.maySweep) {
        if ((~pp).trace.swept == 0) {
            tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGCSweepBegin, tl.stack(1));
            (~pp).trace.inSweep = true;
        }
        (~pp).trace.swept += bytesSwept;
    }
}

// GCSweepDone finishes tracing a sweep loop. If any memory was
// swept (i.e. traceGCSweepSpan emitted an event) then this will emit
// a GCSweepEnd event.
//
// Must be called with a valid P.
internal static void GCSweepDone(this traceLocker tl) {
    var pp = tl.mp.p.ptr();
    if (!(~pp).trace.maySweep) {
        @throw("missing traceGCSweepStart"u8);
    }
    if ((~pp).trace.inSweep) {
        tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGCSweepEnd, ((traceArg)(~pp).trace.swept), ((traceArg)(~pp).trace.reclaimed));
        (~pp).trace.inSweep = false;
    }
    (~pp).trace.maySweep = false;
}

// GCMarkAssistStart emits a MarkAssistBegin event.
internal static void GCMarkAssistStart(this traceLocker tl) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGCMarkAssistBegin, tl.stack(1));
}

// GCMarkAssistDone emits a MarkAssistEnd event.
internal static void GCMarkAssistDone(this traceLocker tl) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGCMarkAssistEnd);
}

// GoCreate emits a GoCreate event.
internal static void GoCreate(this traceLocker tl, ж<g> Ꮡnewg, uintptr pc, bool blocked) {
    ref var newg = ref Ꮡnewg.val;

    newg.trace.setStatusTraced(tl.gen);
    var ev = traceEvGoCreate;
    if (blocked) {
        ev = traceEvGoCreateBlocked;
    }
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(ev, ((traceArg)newg.goid), tl.startPC(pc), tl.stack(2));
}

// GoStart emits a GoStart event.
//
// Must be called with a valid P.
internal static void GoStart(this traceLocker tl) {
    var gp = (~getg()).m.val.curg;
    var pp = (~gp).m.val.p;
    var w = tl.eventWriter(traceGoRunnable, traceProcRunning);
    w = w.write(traceEvGoStart, ((traceArg)(~gp).goid), (~gp).trace.nextSeq(tl.gen));
    if ((~pp.ptr()).gcMarkWorkerMode != gcMarkWorkerNotWorker) {
        w = w.write(traceEvGoLabel, Δtrace.markWorkerLabels[tl.gen % 2][(~pp.ptr()).gcMarkWorkerMode]);
    }
    w.end();
}

// GoEnd emits a GoDestroy event.
//
// TODO(mknyszek): Rename this to GoDestroy.
internal static void GoEnd(this traceLocker tl) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGoDestroy);
}

// GoSched emits a GoStop event with a GoSched reason.
internal static void GoSched(this traceLocker tl) {
    tl.GoStop(traceGoStopGoSched);
}

// GoPreempt emits a GoStop event with a GoPreempted reason.
internal static void GoPreempt(this traceLocker tl) {
    tl.GoStop(traceGoStopPreempted);
}

// GoStop emits a GoStop event with the provided reason.
internal static void GoStop(this traceLocker tl, traceGoStopReason reason) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGoStop, ((traceArg)Δtrace.goStopReasons[tl.gen % 2][reason]), tl.stack(1));
}

// GoPark emits a GoBlock event with the provided reason.
//
// TODO(mknyszek): Replace traceBlockReason with waitReason. It's silly
// that we have both, and waitReason is way more descriptive.
internal static void GoPark(this traceLocker tl, traceBlockReason reason, nint skip) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGoBlock, ((traceArg)Δtrace.goBlockReasons[tl.gen % 2][reason]), tl.stack(skip));
}

// GoUnpark emits a GoUnblock event.
internal static void GoUnpark(this traceLocker tl, ж<g> Ꮡgp, nint skip) {
    ref var gp = ref Ꮡgp.val;

    // Emit a GoWaiting status if necessary for the unblocked goroutine.
    var w = tl.eventWriter(traceGoRunning, traceProcRunning);
    // Careful: don't use the event writer. We never want status or in-progress events
    // to trigger more in-progress events.
    w.w = emitUnblockStatus(w.w, Ꮡgp, tl.gen);
    w.commit(traceEvGoUnblock, ((traceArg)gp.goid), gp.trace.nextSeq(tl.gen), tl.stack(skip));
}

// GoCoroswitch emits a GoSwitch event. If destroy is true, the calling goroutine
// is simultaneously being destroyed.
internal static void GoSwitch(this traceLocker tl, ж<g> Ꮡnextg, bool destroy) {
    ref var nextg = ref Ꮡnextg.val;

    // Emit a GoWaiting status if necessary for the unblocked goroutine.
    var w = tl.eventWriter(traceGoRunning, traceProcRunning);
    // Careful: don't use the event writer. We never want status or in-progress events
    // to trigger more in-progress events.
    w.w = emitUnblockStatus(w.w, Ꮡnextg, tl.gen);
    var ev = traceEvGoSwitch;
    if (destroy) {
        ev = traceEvGoSwitchDestroy;
    }
    w.commit(ev, ((traceArg)nextg.goid), nextg.trace.nextSeq(tl.gen));
}

// emitUnblockStatus emits a GoStatus GoWaiting event for a goroutine about to be
// unblocked to the trace writer.
internal static traceWriter emitUnblockStatus(traceWriter w, ж<g> Ꮡgp, uintptr gen) {
    ref var gp = ref Ꮡgp.val;

    if (!gp.trace.statusWasTraced(gen) && gp.trace.acquireStatus(gen)) {
        // TODO(go.dev/issue/65634): Although it would be nice to add a stack trace here of gp,
        // we cannot safely do so. gp is in _Gwaiting and so we don't have ownership of its stack.
        // We can fix this by acquiring the goroutine's scan bit.
        w = w.writeGoStatus(gp.goid, -1, traceGoWaiting, gp.inMarkAssist, 0);
    }
    return w;
}

// GoSysCall emits a GoSyscallBegin event.
//
// Must be called with a valid P.
internal static void GoSysCall(this traceLocker tl) {
    // Scribble down the M that the P is currently attached to.
    var pp = tl.mp.p.ptr();
    (~pp).trace.mSyscallID = ((int64)tl.mp.procid);
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGoSyscallBegin, (~pp).trace.nextSeq(tl.gen), tl.stack(1));
}

// GoSysExit emits a GoSyscallEnd event, possibly along with a GoSyscallBlocked event
// if lostP is true.
//
// lostP must be true in all cases that a goroutine loses its P during a syscall.
// This means it's not sufficient to check if it has no P. In particular, it needs to be
// true in the following cases:
// - The goroutine lost its P, it ran some other code, and then got it back. It's now running with that P.
// - The goroutine lost its P and was unable to reacquire it, and is now running without a P.
// - The goroutine lost its P and acquired a different one, and is now running with that P.
internal static void GoSysExit(this traceLocker tl, bool lostP) {
    var ev = traceEvGoSyscallEnd;
    var procStatus = traceProcSyscall;
    // Procs implicitly enter traceProcSyscall on GoSyscallBegin.
    if (lostP){
        ev = traceEvGoSyscallEndBlocked;
        procStatus = traceProcRunning;
    } else {
        // If a G has a P when emitting this event, it reacquired a P and is indeed running.
        (~tl.mp.p.ptr()).trace.mSyscallID = -1;
    }
    tl.eventWriter(traceGoSyscall, procStatus).commit(ev);
}

// ProcSteal indicates that our current M stole a P from another M.
//
// inSyscall indicates that we're stealing the P from a syscall context.
//
// The caller must have ownership of pp.
internal static void ProcSteal(this traceLocker tl, ж<Δp> Ꮡpp, bool inSyscall) {
    ref var pp = ref Ꮡpp.val;

    // Grab the M ID we stole from.
    var mStolenFrom = pp.trace.mSyscallID;
    pp.trace.mSyscallID = -1;
    // The status of the proc and goroutine, if we need to emit one here, is not evident from the
    // context of just emitting this event alone. There are two cases. Either we're trying to steal
    // the P just to get its attention (e.g. STW or sysmon retake) or we're trying to steal a P for
    // ourselves specifically to keep running. The two contexts look different, but can be summarized
    // fairly succinctly. In the former, we're a regular running goroutine and proc, if we have either.
    // In the latter, we're a goroutine in a syscall.
    var goStatus = traceGoRunning;
    var procStatus = traceProcRunning;
    if (inSyscall) {
        goStatus = traceGoSyscall;
        procStatus = traceProcSyscallAbandoned;
    }
    var w = tl.eventWriter(goStatus, procStatus);
    // Emit the status of the P we're stealing. We may have *just* done this when creating the event
    // writer but it's not guaranteed, even if inSyscall is true. Although it might seem like from a
    // syscall context we're always stealing a P for ourselves, we may have not wired it up yet (so
    // it wouldn't be visible to eventWriter) or we may not even intend to wire it up to ourselves
    // at all (e.g. entersyscall_gcwait).
    if (!pp.trace.statusWasTraced(tl.gen) && pp.trace.acquireStatus(tl.gen)) {
        // Careful: don't use the event writer. We never want status or in-progress events
        // to trigger more in-progress events.
        w.w = w.w.writeProcStatus(((uint64)pp.id), traceProcSyscallAbandoned, pp.trace.inSweep);
    }
    w.commit(traceEvProcSteal, ((traceArg)pp.id), pp.trace.nextSeq(tl.gen), ((traceArg)mStolenFrom));
}

// HeapAlloc emits a HeapAlloc event.
internal static void HeapAlloc(this traceLocker tl, uint64 live) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvHeapAlloc, ((traceArg)live));
}

// HeapGoal reads the current heap goal and emits a HeapGoal event.
internal static void HeapGoal(this traceLocker tl) {
    var heapGoal = gcController.heapGoal();
    if (heapGoal == ~((uint64)0)) {
        // Heap-based triggering is disabled.
        heapGoal = 0;
    }
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvHeapGoal, ((traceArg)heapGoal));
}

// GoCreateSyscall indicates that a goroutine has transitioned from dead to GoSyscall.
//
// Unlike GoCreate, the caller must be running on gp.
//
// This occurs when C code calls into Go. On pthread platforms it occurs only when
// a C thread calls into Go code for the first time.
internal static void GoCreateSyscall(this traceLocker tl, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.val;

    // N.B. We should never trace a status for this goroutine (which we're currently running on),
    // since we want this to appear like goroutine creation.
    gp.trace.setStatusTraced(tl.gen);
    tl.eventWriter(traceGoBad, traceProcBad).commit(traceEvGoCreateSyscall, ((traceArg)gp.goid));
}

// GoDestroySyscall indicates that a goroutine has transitioned from GoSyscall to dead.
//
// Must not have a P.
//
// This occurs when Go code returns back to C. On pthread platforms it occurs only when
// the C thread is destroyed.
internal static void GoDestroySyscall(this traceLocker tl) {
    // N.B. If we trace a status here, we must never have a P, and we must be on a goroutine
    // that is in the syscall state.
    tl.eventWriter(traceGoSyscall, traceProcBad).commit(traceEvGoDestroySyscall);
}

// To access runtime functions from runtime/trace.
// See runtime/trace/annotation.go

// trace_userTaskCreate emits a UserTaskCreate event.
//
//go:linkname trace_userTaskCreate runtime/trace.userTaskCreate
internal static void trace_userTaskCreate(uint64 id, uint64 parentID, @string taskType) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvUserTaskBegin, ((traceArg)id), ((traceArg)parentID), tl.@string(taskType), tl.stack(3));
    traceRelease(tl);
}

// trace_userTaskEnd emits a UserTaskEnd event.
//
//go:linkname trace_userTaskEnd runtime/trace.userTaskEnd
internal static void trace_userTaskEnd(uint64 id) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvUserTaskEnd, ((traceArg)id), tl.stack(2));
    traceRelease(tl);
}

// trace_userTaskEnd emits a UserRegionBegin or UserRegionEnd event,
// depending on mode (0 == Begin, 1 == End).
//
// TODO(mknyszek): Just make this two functions.
//
//go:linkname trace_userRegion runtime/trace.userRegion
internal static void trace_userRegion(uint64 id, uint64 mode, @string name) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    traceEv ev = default!;
    switch (mode) {
    case 0: {
        ev = traceEvUserRegionBegin;
        break;
    }
    case 1: {
        ev = traceEvUserRegionEnd;
        break;
    }
    default: {
        return;
    }}

    tl.eventWriter(traceGoRunning, traceProcRunning).commit(ev, ((traceArg)id), tl.@string(name), tl.stack(3));
    traceRelease(tl);
}

// trace_userTaskEnd emits a UserRegionBegin or UserRegionEnd event.
//
//go:linkname trace_userLog runtime/trace.userLog
internal static void trace_userLog(uint64 id, @string category, @string message) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvUserLog, ((traceArg)id), tl.@string(category), tl.uniqueString(message), tl.stack(3));
    traceRelease(tl);
}

// traceThreadDestroy is called when a thread is removed from
// sched.freem.
//
// mp must not be able to emit trace events anymore.
//
// sched.lock must be held to synchronize with traceAdvance.
internal static void traceThreadDestroy(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.val;

    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));
    // Flush all outstanding buffers to maintain the invariant
    // that an M only has active buffers while on sched.freem
    // or allm.
    //
    // Perform a traceAcquire/traceRelease on behalf of mp to
    // synchronize with the tracer trying to flush our buffer
    // as well.
    var seq = mp.trace.seqlock.Add(1);
    if (debugTraceReentrancy && seq % 2 != 1) {
        @throw("bad use of trace.seqlock or tracer is reentrant"u8);
    }
    systemstack(
    var traceʗ2 = Δtrace;
    () => {
        @lock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
        ref var i = ref heap(new nint(), out var Ꮡi);

        foreach (var (i, _) in mp.traceʗ2.buf) {
            if (mp.traceʗ2.buf[i] != nil) {
                traceBufFlush(mp.traceʗ2.buf[i], ((uintptr)i));
                mp.traceʗ2.buf[i] = default!;
            }
        }
        unlock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
    });
    var seq1 = mp.trace.seqlock.Add(1);
    if (seq1 != seq + 1) {
        print("runtime: seq1=", seq1, "\n");
        @throw("bad use of trace.seqlock"u8);
    }
}

} // end runtime_package
