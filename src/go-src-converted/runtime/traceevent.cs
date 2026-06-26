// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace event writing API for trace2runtime.go.
namespace go;

using abi = @internal.abi_package;
using sys = runtime.@internal.sys_package;
using @internal;
using runtime.@internal;
using ꓸꓸꓸtraceArg = Span<traceArg>;

partial class runtime_package {

[GoType("num:uint8")] partial struct traceEv;

internal static readonly traceEv traceEvNone = /* iota */ 0; // unused
internal static readonly traceEv traceEvEventBatch = 1; // start of per-M batch of events [generation, M ID, timestamp, batch length]
internal static readonly traceEv traceEvStacks = 2; // start of a section of the stack table [...traceEvStack]
internal static readonly traceEv traceEvStack = 3; // stack table entry [ID, ...{PC, func string ID, file string ID, line #}]
internal static readonly traceEv traceEvStrings = 4; // start of a section of the string dictionary [...traceEvString]
internal static readonly traceEv traceEvString = 5; // string dictionary entry [ID, length, string]
internal static readonly traceEv traceEvCPUSamples = 6; // start of a section of CPU samples [...traceEvCPUSample]
internal static readonly traceEv traceEvCPUSample = 7; // CPU profiling sample [timestamp, M ID, P ID, goroutine ID, stack ID]
internal static readonly traceEv traceEvFrequency = 8; // timestamp units per sec [freq]
internal static readonly traceEv traceEvProcsChange = 9; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack ID]
internal static readonly traceEv traceEvProcStart = 10; // start of P [timestamp, P ID, P seq]
internal static readonly traceEv traceEvProcStop = 11; // stop of P [timestamp]
internal static readonly traceEv traceEvProcSteal = 12; // P was stolen [timestamp, P ID, P seq, M ID]
internal static readonly traceEv traceEvProcStatus = 13; // P status at the start of a generation [timestamp, P ID, status]
internal static readonly traceEv traceEvGoCreate = 14; // goroutine creation [timestamp, new goroutine ID, new stack ID, stack ID]
internal static readonly traceEv traceEvGoCreateSyscall = 15; // goroutine appears in syscall (cgo callback) [timestamp, new goroutine ID]
internal static readonly traceEv traceEvGoStart = 16; // goroutine starts running [timestamp, goroutine ID, goroutine seq]
internal static readonly traceEv traceEvGoDestroy = 17; // goroutine ends [timestamp]
internal static readonly traceEv traceEvGoDestroySyscall = 18; // goroutine ends in syscall (cgo callback) [timestamp]
internal static readonly traceEv traceEvGoStop = 19; // goroutine yields its time, but is runnable [timestamp, reason, stack ID]
internal static readonly traceEv traceEvGoBlock = 20; // goroutine blocks [timestamp, reason, stack ID]
internal static readonly traceEv traceEvGoUnblock = 21; // goroutine is unblocked [timestamp, goroutine ID, goroutine seq, stack ID]
internal static readonly traceEv traceEvGoSyscallBegin = 22; // syscall enter [timestamp, P seq, stack ID]
internal static readonly traceEv traceEvGoSyscallEnd = 23; // syscall exit [timestamp]
internal static readonly traceEv traceEvGoSyscallEndBlocked = 24; // syscall exit and it blocked at some point [timestamp]
internal static readonly traceEv traceEvGoStatus = 25; // goroutine status at the start of a generation [timestamp, goroutine ID, M ID, status]
internal static readonly traceEv traceEvSTWBegin = 26; // STW start [timestamp, kind]
internal static readonly traceEv traceEvSTWEnd = 27; // STW done [timestamp]
internal static readonly traceEv traceEvGCActive = 28; // GC active [timestamp, seq]
internal static readonly traceEv traceEvGCBegin = 29; // GC start [timestamp, seq, stack ID]
internal static readonly traceEv traceEvGCEnd = 30; // GC done [timestamp, seq]
internal static readonly traceEv traceEvGCSweepActive = 31; // GC sweep active [timestamp, P ID]
internal static readonly traceEv traceEvGCSweepBegin = 32; // GC sweep start [timestamp, stack ID]
internal static readonly traceEv traceEvGCSweepEnd = 33; // GC sweep done [timestamp, swept bytes, reclaimed bytes]
internal static readonly traceEv traceEvGCMarkAssistActive = 34; // GC mark assist active [timestamp, goroutine ID]
internal static readonly traceEv traceEvGCMarkAssistBegin = 35; // GC mark assist start [timestamp, stack ID]
internal static readonly traceEv traceEvGCMarkAssistEnd = 36; // GC mark assist done [timestamp]
internal static readonly traceEv traceEvHeapAlloc = 37; // gcController.heapLive change [timestamp, heap alloc in bytes]
internal static readonly traceEv traceEvHeapGoal = 38; // gcController.heapGoal() change [timestamp, heap goal in bytes]
internal static readonly traceEv traceEvGoLabel = 39; // apply string label to current running goroutine [timestamp, label string ID]
internal static readonly traceEv traceEvUserTaskBegin = 40; // trace.NewTask [timestamp, internal task ID, internal parent task ID, name string ID, stack ID]
internal static readonly traceEv traceEvUserTaskEnd = 41; // end of a task [timestamp, internal task ID, stack ID]
internal static readonly traceEv traceEvUserRegionBegin = 42; // trace.{Start,With}Region [timestamp, internal task ID, name string ID, stack ID]
internal static readonly traceEv traceEvUserRegionEnd = 43; // trace.{End,With}Region [timestamp, internal task ID, name string ID, stack ID]
internal static readonly traceEv traceEvUserLog = 44; // trace.Log [timestamp, internal task ID, key string ID, stack, value string ID]
internal static readonly traceEv traceEvGoSwitch = 45; // goroutine switch (coroswitch) [timestamp, goroutine ID, goroutine seq]
internal static readonly traceEv traceEvGoSwitchDestroy = 46; // goroutine switch and destroy [timestamp, goroutine ID, goroutine seq]
internal static readonly traceEv traceEvGoCreateBlocked = 47; // goroutine creation (starts blocked) [timestamp, new goroutine ID, new stack ID, stack ID]
internal static readonly traceEv traceEvGoStatusStack = 48; // goroutine status at the start of a generation, with a stack [timestamp, goroutine ID, M ID, status, stack ID]
internal static readonly traceEv traceEvExperimentalBatch = 49; // start of extra data [experiment ID, generation, M ID, timestamp, batch length, batch data...]

[GoType("num:uint64")] partial struct traceArg;

// traceEventWriter is the high-level API for writing trace events.
//
// See the comment on traceWriter about style for more details as to why
// this type and its methods are structured the way they are.
[GoType] partial struct traceEventWriter {
    internal traceWriter w;
}

// eventWriter creates a new traceEventWriter. It is the main entrypoint for writing trace events.
//
// Before creating the event writer, this method will emit a status for the current goroutine
// or proc if it exists, and if it hasn't had its status emitted yet. goStatus and procStatus indicate
// what the status of goroutine or P should be immediately *before* the events that are about to
// be written using the eventWriter (if they exist). No status will be written if there's no active
// goroutine or P.
//
// Callers can elect to pass a constant value here if the status is clear (e.g. a goroutine must have
// been Runnable before a GoStart). Otherwise, callers can query the status of either the goroutine
// or P and pass the appropriate status.
//
// In this case, the default status should be traceGoBad or traceProcBad to help identify bugs sooner.
internal static traceEventWriter eventWriter(this traceLocker tl, traceGoStatus goStatus, traceProcStatus procStatus) {
    var w = tl.writer();
    {
        var pp = tl.mp.p.ptr(); if (pp != nil && !(~pp).trace.statusWasTraced(tl.gen) && (~pp).trace.acquireStatus(tl.gen)) {
            w = w.writeProcStatus(((uint64)(~pp).id), procStatus, (~pp).trace.inSweep);
        }
    }
    {
        var gp = tl.mp.curg; if (gp != nil && !(~gp).trace.statusWasTraced(tl.gen) && (~gp).trace.acquireStatus(tl.gen)) {
            w = w.writeGoStatus(((uint64)(~gp).goid), ((int64)tl.mp.procid), goStatus, (~gp).inMarkAssist, 0);
        }
    }
    /* no stack */
    return new traceEventWriter(w);
}

// commit writes out a trace event and calls end. It's a helper to make the
// common case of writing out a single event less error-prone.
internal static void commit(this traceEventWriter e, traceEv ev, params ꓸꓸꓸtraceArg argsʗp) {
    var args = argsʗp.slice();

    e = e.write(ev, args.ꓸꓸꓸ);
    e.end();
}

// write writes an event into the trace.
internal static traceEventWriter write(this traceEventWriter e, traceEv ev, params ꓸꓸꓸtraceArg argsʗp) {
    var args = argsʗp.slice();

    e.w = e.w.@event(ev, args.ꓸꓸꓸ);
    return e;
}

// end finishes writing to the trace. The traceEventWriter must not be used after this call.
internal static void end(this traceEventWriter e) {
    e.w.end();
}

// traceEventWrite is the part of traceEvent that actually writes the event.
internal static traceWriter @event(this traceWriter w, traceEv ev, params ꓸꓸꓸtraceArg argsʗp) {
    var args = argsʗp.slice();

    // Make sure we have room.
    (w, _) = w.ensure(1 + (len(args) + 1) * traceBytesPerNumber);
    // Compute the timestamp diff that we'll put in the trace.
    var ts = traceClockNow();
    if (ts <= w.traceBuf.lastTime) {
        ts = w.traceBuf.lastTime + 1;
    }
    var tsDiff = ((uint64)(ts - w.traceBuf.lastTime));
    w.traceBuf.lastTime = ts;
    // Write out event.
    w.@byte(((byte)ev));
    w.varint(tsDiff);
    foreach (var (_, arg) in args) {
        w.varint(((uint64)arg));
    }
    return w;
}

// stack takes a stack trace skipping the provided number of frames.
// It then returns a traceArg representing that stack which may be
// passed to write.
internal static traceArg stack(this traceLocker tl, nint skip) {
    return ((traceArg)traceStack(skip, nil, tl.gen));
}

// startPC takes a start PC for a goroutine and produces a unique
// stack ID for it.
//
// It then returns a traceArg representing that stack which may be
// passed to write.
internal static traceArg startPC(this traceLocker tl, uintptr pc) {
    // +PCQuantum because makeTraceFrame expects return PCs and subtracts PCQuantum.
    return ((traceArg)Δtrace.stackTab[tl.gen % 2].put(new uintptr[]{
        logicalStackSentinel,
        startPCForTrace(pc) + sys.PCQuantum
    }.slice()));
}

// string returns a traceArg representing s which may be passed to write.
// The string is assumed to be relatively short and popular, so it may be
// stored for a while in the string dictionary.
internal static traceArg @string(this traceLocker tl, @string s) {
    return ((traceArg)Δtrace.stringTab[tl.gen % 2].put(tl.gen, s));
}

// uniqueString returns a traceArg representing s which may be passed to write.
// The string is assumed to be unique or long, so it will be written out to
// the trace eagerly.
internal static traceArg uniqueString(this traceLocker tl, @string s) {
    return ((traceArg)Δtrace.stringTab[tl.gen % 2].emit(tl.gen, s));
}

// rtype returns a traceArg representing typ which may be passed to write.
internal static traceArg rtype(this traceLocker tl, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    return ((traceArg)Δtrace.typeTab[tl.gen % 2].put(Ꮡtyp));
}

} // end runtime_package
