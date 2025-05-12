// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements conversion from old (Go 1.11–Go 1.21) traces to the Go
// 1.22 format.
//
// Most events have direct equivalents in 1.22, at worst requiring arguments to
// be reordered. Some events, such as GoWaiting need to look ahead for follow-up
// events to determine the correct translation. GoSyscall, which is an
// instantaneous event, gets turned into a 1 ns long pair of
// GoSyscallStart+GoSyscallEnd, unless we observe a GoSysBlock, in which case we
// emit a GoSyscallStart+GoSyscallEndBlocked pair with the correct duration
// (i.e. starting at the original GoSyscall).
//
// The resulting trace treats the old trace as a single, large generation,
// sharing a single evTable for all events.
//
// We use a new (compared to what was used for 'go tool trace' in earlier
// versions of Go) parser for old traces that is optimized for speed, low memory
// usage, and minimal GC pressure. It allocates events in batches so that even
// though we have to load the entire trace into memory, the conversion process
// shouldn't result in a doubling of memory usage, even if all converted events
// are kept alive, as we free batches once we're done with them.
//
// The conversion process is lossless.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using @event = @internal.trace.event_package;
using go122 = @internal.trace.@event.go122_package;
using oldtrace = @internal.trace.@internal.oldtrace_package;
using io = io_package;
using @internal.trace;
using @internal.trace.@event;
using @internal.trace.@internal;

partial class trace_package {

[GoType] partial struct oldTraceConverter {
    internal @internal.trace.@internal.oldtrace_package.Trace trace;
    internal ж<evTable> evt;
    internal bool preInit;
    internal map<GoID, EmptyStruct> createdPreInit;
    internal @internal.trace.@internal.oldtrace_package.Events events;
    internal slice<ΔEvent> extra;
    internal array<ΔEvent> extraArr = new(3);
    internal trace.taskState tasks;
    internal map<ProcID, EmptyStruct> seenProcs;
    internal ΔTime lastTs;
    internal trace.ThreadID procMs;
    internal uint64 lastStwReason;
    internal slice<uint64> inlineToStringID;
    internal slice<uint64> builtinToStringID;
}

internal static readonly UntypedInt sForever = iota;
internal static readonly UntypedInt sPreempted = 1;
internal static readonly UntypedInt sGosched = 2;
internal static readonly UntypedInt sSleep = 3;
internal static readonly UntypedInt sChanSend = 4;
internal static readonly UntypedInt sChanRecv = 5;
internal static readonly UntypedInt sNetwork = 6;
internal static readonly UntypedInt sSync = 7;
internal static readonly UntypedInt sSyncCond = 8;
internal static readonly UntypedInt sSelect = 9;
internal static readonly UntypedInt sEmpty = 10;
internal static readonly UntypedInt sMarkAssistWait = 11;
internal static readonly UntypedInt sSTWUnknown = 12;
internal static readonly UntypedInt sSTWGCMarkTermination = 13;
internal static readonly UntypedInt sSTWGCSweepTermination = 14;
internal static readonly UntypedInt sSTWWriteHeapDump = 15;
internal static readonly UntypedInt sSTWGoroutineProfile = 16;
internal static readonly UntypedInt sSTWGoroutineProfileCleanup = 17;
internal static readonly UntypedInt sSTWAllGoroutinesStackTrace = 18;
internal static readonly UntypedInt sSTWReadMemStats = 19;
internal static readonly UntypedInt sSTWAllThreadsSyscall = 20;
internal static readonly UntypedInt sSTWGOMAXPROCS = 21;
internal static readonly UntypedInt sSTWStartTrace = 22;
internal static readonly UntypedInt sSTWStopTrace = 23;
internal static readonly UntypedInt sSTWCountPagesInUse = 24;
internal static readonly UntypedInt sSTWReadMetricsSlow = 25;
internal static readonly UntypedInt sSTWReadMemStatsSlow = 26;
internal static readonly UntypedInt sSTWPageCachePagesLeaked = 27;
internal static readonly UntypedInt sSTWResetDebugLog = 28;
internal static readonly UntypedInt sLast = 29;

[GoRecv] internal static error init(this ref oldTraceConverter it, oldtrace.Trace pr) {
    it.trace = pr;
    it.preInit = true;
    it.createdPreInit = new map<GoID, EmptyStruct>();
    it.evt = Ꮡ(new evTable(pcs: new map<uint64, frame>()));
    it.events = pr.Events;
    it.extra = it.extraArr[..0];
    it.tasks = new trace.taskState();
    it.seenProcs = new map<ProcID, EmptyStruct>();
    it.procMs = new trace.ThreadID();
    it.lastTs = -1;
    var evt = it.evt;
    // Convert from oldtracer's Strings map to our dataTable.
    uint64 max = default!;
    foreach (var (id, s) in pr.Strings) {
        (~evt).strings.insert(((stringID)id), s);
        if (id > max) {
            max = id;
        }
    }
    pr.Strings = default!;
    // Add all strings used for UserLog. In the old trace format, these were
    // stored inline and didn't have IDs. We generate IDs for them.
    if (max + ((uint64)len(pr.InlineStrings)) < max) {
        return errors.New("trace contains too many strings"u8);
    }
    error addErr = default!;
    var add = 
    var addErrʗ1 = addErr;
    var evtʗ1 = evt;
    (stringID id, @string s) => {
        {
            var err = (~evtʗ1).strings.insert(id, s); if (err != default! && addErrʗ1 == default!) {
                addErrʗ1 = err;
            }
        }
    };
    foreach (var (id, s) in pr.InlineStrings) {
        var nid = max + 1 + ((uint64)id);
        it.inlineToStringID = append(it.inlineToStringID, nid);
        add(((stringID)nid), s);
    }
    max += ((uint64)len(pr.InlineStrings));
    pr.InlineStrings = default!;
    // Add strings that the converter emits explicitly.
    if (max + ((uint64)sLast) < max) {
        return errors.New("trace contains too many strings"u8);
    }
    it.builtinToStringID = new slice<uint64>(sLast);
    var addBuiltin = 
    var addʗ1 = add;
    (nint c, @string s) => {
        var nid = max + 1 + ((uint64)c);
        it.builtinToStringID[c] = nid;
        addʗ1(((stringID)nid), s);
    };
    addBuiltin(sForever, "forever"u8);
    addBuiltin(sPreempted, "preempted"u8);
    addBuiltin(sGosched, "runtime.Gosched"u8);
    addBuiltin(sSleep, "sleep"u8);
    addBuiltin(sChanSend, "chan send"u8);
    addBuiltin(sChanRecv, "chan receive"u8);
    addBuiltin(sNetwork, "network"u8);
    addBuiltin(sSync, "sync"u8);
    addBuiltin(sSyncCond, "sync.(*Cond).Wait"u8);
    addBuiltin(sSelect, "select"u8);
    addBuiltin(sEmpty, ""u8);
    addBuiltin(sMarkAssistWait, "GC mark assist wait for work"u8);
    addBuiltin(sSTWUnknown, ""u8);
    addBuiltin(sSTWGCMarkTermination, "GC mark termination"u8);
    addBuiltin(sSTWGCSweepTermination, "GC sweep termination"u8);
    addBuiltin(sSTWWriteHeapDump, "write heap dump"u8);
    addBuiltin(sSTWGoroutineProfile, "goroutine profile"u8);
    addBuiltin(sSTWGoroutineProfileCleanup, "goroutine profile cleanup"u8);
    addBuiltin(sSTWAllGoroutinesStackTrace, "all goroutine stack trace"u8);
    addBuiltin(sSTWReadMemStats, "read mem stats"u8);
    addBuiltin(sSTWAllThreadsSyscall, "AllThreadsSyscall"u8);
    addBuiltin(sSTWGOMAXPROCS, "GOMAXPROCS"u8);
    addBuiltin(sSTWStartTrace, "start trace"u8);
    addBuiltin(sSTWStopTrace, "stop trace"u8);
    addBuiltin(sSTWCountPagesInUse, "CountPagesInUse (test)"u8);
    addBuiltin(sSTWReadMetricsSlow, "ReadMetricsSlow (test)"u8);
    addBuiltin(sSTWReadMemStatsSlow, "ReadMemStatsSlow (test)"u8);
    addBuiltin(sSTWPageCachePagesLeaked, "PageCachePagesLeaked (test)"u8);
    addBuiltin(sSTWResetDebugLog, "ResetDebugLog (test)"u8);
    if (addErr != default!) {
        // This should be impossible but let's be safe.
        return fmt.Errorf("couldn't add strings: %w"u8, addErr);
    }
    it.evt.strings.compactify();
    // Convert stacks.
    foreach (var (id, stk) in pr.Stacks) {
        (~evt).stacks.insert(((stackID)id), new stack(pcs: stk));
    }
    // OPT(dh): if we could share the frame type between this package and
    // oldtrace we wouldn't have to copy the map.
    foreach (var (pc, f) in pr.PCs) {
        (~evt).pcs[pc] = new frame(
            pc: pc,
            funcID: ((stringID)f.Fn),
            fileID: ((stringID)f.File),
            line: ((uint64)f.Line)
        );
    }
    pr.Stacks = default!;
    pr.PCs = default!;
    (~evt).stacks.compactify();
    return default!;
}

// next returns the next event, io.EOF if there are no more events, or a
// descriptive error for invalid events.
[GoRecv] internal static (ΔEvent, error) next(this ref oldTraceConverter it) {
    if (len(it.extra) > 0) {
        var evΔ1 = it.extra[0];
        it.extra = it.extra[1..];
        if (len(it.extra) == 0) {
            it.extra = it.extraArr[..0];
        }
        // Two events aren't allowed to fall on the same timestamp in the new API,
        // but this may happen when we produce EvGoStatus events
        if (evΔ1.@base.time <= it.lastTs) {
            .@base.time = it.lastTs + 1;
        }
        it.lastTs = evΔ1.@base.time;
        return (evΔ1, default!);
    }
    var (oev, ok) = it.events.Pop();
    if (!ok) {
        return (new ΔEvent(nil), io.EOF);
    }
    var (ev, err) = it.convertEvent(oev);
    if (AreEqual(err, errSkip)){
        return it.next();
    } else 
    if (err != default!) {
        return (new ΔEvent(nil), err);
    }
    // Two events aren't allowed to fall on the same timestamp in the new API,
    // but this may happen when we produce EvGoStatus events
    if (ev.@base.time <= it.lastTs) {
        ev.@base.time = it.lastTs + 1;
    }
    it.lastTs = ev.@base.time;
    return (ev, default!);
}

internal static error errSkip = errors.New("skip event"u8);

[GoType("dyn")] partial struct convertEvent_it {
}

[GoType("dyn")] partial struct convertEvent_itᴛ1 {
}

[GoType("dyn")] partial struct convertEvent_itᴛ2 {
}

// convertEvent converts an event from the old trace format to zero or more
// events in the new format. Most events translate 1 to 1. Some events don't
// result in an event right away, in which case convertEvent returns errSkip.
// Some events result in more than one new event; in this case, convertEvent
// returns the first event and stores additional events in it.extra. When
// encountering events that oldtrace shouldn't be able to emit, ocnvertEvent
// returns a descriptive error.
[GoRecv] internal static (ΔEvent OUT, error ERR) convertEvent(this ref oldTraceConverter it, ж<oldtrace.Event> Ꮡev) {
    ΔEvent OUT = default!;
    error ERR = default!;

    ref var ev = ref Ꮡev.val;
    @event.Type mappedType = default!;
    timedEventArgs mappedArgs = default!;
    copy(mappedArgs[..], ev.Args[..]);
    var exprᴛ1 = ev.Type;
    if (exprᴛ1 == oldtrace.EvGomaxprocs) {
        mappedType = go122.EvProcsChange;
        if (it.preInit) {
            // The first EvGomaxprocs signals the end of trace initialization. At this point we've seen
            // all goroutines that already existed at trace begin.
            it.preInit = false;
            foreach (var (gid, _) in it.createdPreInit) {
                // These are goroutines that already existed when tracing started but for which we
                // received neither GoWaiting, GoInSyscall, or GoStart. These are goroutines that are in
                // the states _Gidle or _Grunnable.
                it.extra = append(it.extra, new ΔEvent(
                    ctx: new schedCtx( // G: GoID(gid),

                        G: NoGoroutine,
                        P: NoProc,
                        M: NoThread
                    ),
                    table: it.evt,
                    @base: new baseEvent(
                        typ: go122.EvGoStatus,
                        time: ((ΔTime)ev.Ts),
                        args: new timedEventArgs{((uint64)gid), ~((uint64)0), ((uint64)go122.GoRunnable)}
                    )
                ));
            }
            it.createdPreInit = default!;
            return (new ΔEvent(nil), errSkip);
        }
    }
    if (exprᴛ1 == oldtrace.EvProcStart) {
        it.procMs[((ProcID)ev.P)] = ((ThreadID)ev.Args[0]);
        {
            var (_, ok) = it.seenProcs[((ProcID)ev.P)]; if (ok){
                mappedType = go122.EvProcStart;
                mappedArgs = new timedEventArgs{((uint64)ev.P)};
            } else {
                it.seenProcs[((ProcID)ev.P)] = new convertEvent_it();
                mappedType = go122.EvProcStatus;
                mappedArgs = new timedEventArgs{((uint64)ev.P), ((uint64)go122.ProcRunning)};
            }
        }
    }
    else if (exprᴛ1 == oldtrace.EvProcStop) {
        {
            var (_, ok) = it.seenProcs[((ProcID)ev.P)]; if (ok){
                mappedType = go122.EvProcStop;
                mappedArgs = new timedEventArgs{((uint64)ev.P)};
            } else {
                it.seenProcs[((ProcID)ev.P)] = new convertEvent_itᴛ1();
                mappedType = go122.EvProcStatus;
                mappedArgs = new timedEventArgs{((uint64)ev.P), ((uint64)go122.ProcIdle)};
            }
        }
    }
    else if (exprᴛ1 == oldtrace.EvGCStart) {
        mappedType = go122.EvGCBegin;
    }
    else if (exprᴛ1 == oldtrace.EvGCDone) {
        mappedType = go122.EvGCEnd;
    }
    else if (exprᴛ1 == oldtrace.EvSTWStart) {
        var sid = it.builtinToStringID[sSTWUnknown + it.trace.STWReason(ev.Args[0])];
        it.lastStwReason = sid;
        mappedType = go122.EvSTWBegin;
        mappedArgs = new timedEventArgs{((uint64)sid)};
    }
    else if (exprᴛ1 == oldtrace.EvSTWDone) {
        mappedType = go122.EvSTWEnd;
        mappedArgs = new timedEventArgs{it.lastStwReason};
    }
    else if (exprᴛ1 == oldtrace.EvGCSweepStart) {
        mappedType = go122.EvGCSweepBegin;
    }
    else if (exprᴛ1 == oldtrace.EvGCSweepDone) {
        mappedType = go122.EvGCSweepEnd;
    }
    else if (exprᴛ1 == oldtrace.EvGoCreate) {
        if (it.preInit) {
            it.createdPreInit[((GoID)ev.Args[0])] = new convertEvent_itᴛ2();
            return (new ΔEvent(nil), errSkip);
        }
        mappedType = go122.EvGoCreate;
    }
    else if (exprᴛ1 == oldtrace.EvGoStart) {
        if (it.preInit){
            mappedType = go122.EvGoStatus;
            mappedArgs = new timedEventArgs{ev.Args[0], ~((uint64)0), ((uint64)go122.GoRunning)};
            delete(it.createdPreInit, ((GoID)ev.Args[0]));
        } else {
            mappedType = go122.EvGoStart;
        }
    }
    else if (exprᴛ1 == oldtrace.EvGoStartLabel) {
        it.extra = new ΔEvent[]{new(
            ctx: new schedCtx(
                G: ((GoID)ev.G),
                P: ((ProcID)ev.P),
                M: it.procMs[((ProcID)ev.P)]
            ),
            table: it.evt,
            @base: new baseEvent(
                typ: go122.EvGoLabel,
                time: ((ΔTime)ev.Ts),
                args: new timedEventArgs{ev.Args[2]}
            )
        )
        }.slice();
        return (new ΔEvent(
            ctx: new schedCtx(
                G: ((GoID)ev.G),
                P: ((ProcID)ev.P),
                M: it.procMs[((ProcID)ev.P)]
            ),
            table: it.evt,
            @base: new baseEvent(
                typ: go122.EvGoStart,
                time: ((ΔTime)ev.Ts),
                args: mappedArgs
            )
        ), default!);
    }
    if (exprᴛ1 == oldtrace.EvGoEnd) {
        mappedType = go122.EvGoDestroy;
    }
    else if (exprᴛ1 == oldtrace.EvGoStop) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sForever]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoSched) {
        mappedType = go122.EvGoStop;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sGosched]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoPreempt) {
        mappedType = go122.EvGoStop;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sPreempted]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoSleep) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sSleep]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoBlock) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sEmpty]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoUnblock) {
        mappedType = go122.EvGoUnblock;
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockSend) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sChanSend]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockRecv) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sChanRecv]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockSelect) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sSelect]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockSync) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sSync]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockCond) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sSyncCond]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockNet) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sNetwork]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockGC) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs{((uint64)it.builtinToStringID[sMarkAssistWait]), ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvGoSysCall) {
        var blocked = false;
        it.events.All()((ж<oldtrace.Event> nev) => {
            if ((~nev).G != ev.G) {
                return true;
            }
            if ((~nev).Type == oldtrace.EvGoSysBlock) {
                blocked = true;
            }
            return false;
        });
        if (blocked){
            mappedType = go122.EvGoSyscallBegin;
            mappedArgs = new timedEventArgs{1: ((uint64)ev.StkID)};
        } else {
            // Convert the old instantaneous syscall event to a pair of syscall
            // begin and syscall end and give it the shortest possible duration,
            // 1ns.
            var out1 = new ΔEvent(
                ctx: new schedCtx(
                    G: ((GoID)ev.G),
                    P: ((ProcID)ev.P),
                    M: it.procMs[((ProcID)ev.P)]
                ),
                table: it.evt,
                @base: new baseEvent(
                    typ: go122.EvGoSyscallBegin,
                    time: ((ΔTime)ev.Ts),
                    args: new timedEventArgs{1: ((uint64)ev.StkID)}
                )
            );
            var out2 = new ΔEvent(
                ctx: out1.ctx,
                table: it.evt,
                @base: new baseEvent(
                    typ: go122.EvGoSyscallEnd,
                    time: ((ΔTime)(ev.Ts + 1)),
                    args: new timedEventArgs{nil}
                )
            );
            it.extra = append(it.extra, out2);
            return (out1, default!);
        }
    }
    if (exprᴛ1 == oldtrace.EvGoSysExit) {
        mappedType = go122.EvGoSyscallEndBlocked;
    }
    else if (exprᴛ1 == oldtrace.EvGoSysBlock) {
        return (new ΔEvent(nil), errSkip);
    }
    if (exprᴛ1 == oldtrace.EvGoWaiting) {
        mappedType = go122.EvGoStatus;
        mappedArgs = new timedEventArgs{ev.Args[0], ~((uint64)0), ((uint64)go122.GoWaiting)};
        delete(it.createdPreInit, ((GoID)ev.Args[0]));
    }
    else if (exprᴛ1 == oldtrace.EvGoInSyscall) {
        mappedType = go122.EvGoStatus;
        mappedArgs = new timedEventArgs{ // In the new tracer, GoStatus with GoSyscall knows what thread the
 // syscall is on. In the old tracer, EvGoInSyscall doesn't contain that
 // information and all we can do here is specify NoThread.
ev.Args[0], ~((uint64)0), ((uint64)go122.GoSyscall)};
        delete(it.createdPreInit, ((GoID)ev.Args[0]));
    }
    else if (exprᴛ1 == oldtrace.EvHeapAlloc) {
        mappedType = go122.EvHeapAlloc;
    }
    else if (exprᴛ1 == oldtrace.EvHeapGoal) {
        mappedType = go122.EvHeapGoal;
    }
    else if (exprᴛ1 == oldtrace.EvGCMarkAssistStart) {
        mappedType = go122.EvGCMarkAssistBegin;
    }
    else if (exprᴛ1 == oldtrace.EvGCMarkAssistDone) {
        mappedType = go122.EvGCMarkAssistEnd;
    }
    else if (exprᴛ1 == oldtrace.EvUserTaskCreate) {
        mappedType = go122.EvUserTaskBegin;
        var parent = ev.Args[1];
        if (parent == 0) {
            parent = ((uint64)NoTask);
        }
        mappedArgs = new timedEventArgs{ev.Args[0], parent, ev.Args[2], ((uint64)ev.StkID)};
        var (name, _) = it.evt.strings.get(((stringID)ev.Args[2]));
        it.tasks[((TaskID)ev.Args[0])] = new taskState(name: name, parentID: ((TaskID)ev.Args[1]));
    }
    else if (exprᴛ1 == oldtrace.EvUserTaskEnd) {
        mappedType = go122.EvUserTaskEnd;
        var (ts, ok) = it.tasks[((TaskID)ev.Args[0])];
        if (ok){
            // Event.Task expects the parent and name to be smuggled in extra args
            // and as extra strings.
            delete(it.tasks, ((TaskID)ev.Args[0]));
            mappedArgs = new timedEventArgs{
                ev.Args[0],
                ev.Args[1],
                ((uint64)ts.parentID),
                ((uint64)it.evt.addExtraString(ts.name))
            };
        } else {
            mappedArgs = new timedEventArgs{ev.Args[0], ev.Args[1], ((uint64)NoTask), ((uint64)it.evt.addExtraString(""u8))};
        }
    }
    else if (exprᴛ1 == oldtrace.EvUserRegion) {
        switch (ev.Args[1]) {
        case 0: {
            mappedType = go122.EvUserRegionBegin;
            break;
        }
        case 1: {
            mappedType = go122.EvUserRegionEnd;
            break;
        }}

        mappedArgs = new timedEventArgs{ // start
 // end
ev.Args[0], ev.Args[2], ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvUserLog) {
        mappedType = go122.EvUserLog;
        mappedArgs = new timedEventArgs{ev.Args[0], ev.Args[1], it.inlineToStringID[ev.Args[3]], ((uint64)ev.StkID)};
    }
    else if (exprᴛ1 == oldtrace.EvCPUSample) {
        mappedType = go122.EvCPUSample;
        mappedArgs = new timedEventArgs{ // When emitted by the Go 1.22 tracer, CPU samples have 5 arguments:
 // timestamp, M, P, G, stack. However, after they get turned into Event,
 // they have the arguments stack, M, P, G.
 //
 // In Go 1.21, CPU samples did not have Ms.
((uint64)ev.StkID), ~((uint64)0), ((uint64)ev.P), ev.G};
    }
    else { /* default: */
        return (new ΔEvent(nil), fmt.Errorf("unexpected event type %v"u8, ev.Type));
    }

    if (oldtrace.EventDescriptions[ev.Type].Stack) {
        {
            var stackIDs = go122.Specs()[mappedType].StackIDs; if (len(stackIDs) > 0) {
                mappedArgs[stackIDs[0] - 1] = ((uint64)ev.StkID);
            }
        }
    }
    var m = NoThread;
    if (ev.P != -1 && ev.Type != oldtrace.EvCPUSample) {
        {
            var (t, ok) = it.procMs[((ProcID)ev.P)]; if (ok) {
                m = ((ThreadID)t);
            }
        }
    }
    if (ev.Type == oldtrace.EvProcStop) {
        delete(it.procMs, ((ProcID)ev.P));
    }
    var g = ((GoID)ev.G);
    if (g == 0) {
        g = NoGoroutine;
    }
    var @out = new ΔEvent(
        ctx: new schedCtx(
            G: ((GoID)g),
            P: ((ProcID)ev.P),
            M: m
        ),
        table: it.evt,
        @base: new baseEvent(
            typ: mappedType,
            time: ((ΔTime)ev.Ts),
            args: mappedArgs
        )
    );
    return (@out, default!);
}

// convertOldFormat takes a fully loaded trace in the old trace format and
// returns an iterator over events in the new format.
internal static ж<oldTraceConverter> convertOldFormat(oldtrace.Trace pr) {
    var it = Ꮡ(new oldTraceConverter(nil));
    it.init(pr);
    return it;
}

} // end trace_package
