// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using math = math_package;
using strings = strings_package;
using time = time_package;
using @event = @internal.trace.event_package;
using go122 = @internal.trace.@event.go122_package;
using version = @internal.trace.version_package;
using @internal.trace;
using @internal.trace.@event;

partial class trace_package {

[GoType("num:uint16")] partial struct EventKind;

public static readonly EventKind EventBad = /* iota */ 0;
public static readonly EventKind EventSync = 1;
public static readonly EventKind EventMetric = 2;
public static readonly EventKind EventLabel = 3;
public static readonly EventKind EventStackSample = 4;
public static readonly EventKind EventRangeBegin = 5;
public static readonly EventKind EventRangeActive = 6;
public static readonly EventKind EventRangeEnd = 7;
public static readonly EventKind EventTaskBegin = 8;
public static readonly EventKind EventTaskEnd = 9;
public static readonly EventKind EventRegionBegin = 10;
public static readonly EventKind EventRegionEnd = 11;
public static readonly EventKind EventLog = 12;
public static readonly EventKind EventStateTransition = 13;
public static readonly EventKind EventExperimental = 14;

// String returns a string form of the EventKind.
public static @string String(this EventKind e) {
    if (((nint)e) >= len(eventKindStrings)) {
        return eventKindStrings[0];
    }
    return eventKindStrings[e];
}

internal static array<@string> eventKindStrings = new runtime.SparseArray<@string>{
    [EventBad] = "Bad"u8,
    [EventSync] = "Sync"u8,
    [EventMetric] = "Metric"u8,
    [EventLabel] = "Label"u8,
    [EventStackSample] = "StackSample"u8,
    [EventRangeBegin] = "RangeBegin"u8,
    [EventRangeActive] = "RangeActive"u8,
    [EventRangeEnd] = "RangeEnd"u8,
    [EventTaskBegin] = "TaskBegin"u8,
    [EventTaskEnd] = "TaskEnd"u8,
    [EventRegionBegin] = "RegionBegin"u8,
    [EventRegionEnd] = "RegionEnd"u8,
    [EventLog] = "Log"u8,
    [EventStateTransition] = "StateTransition"u8,
    [EventExperimental] = "Experimental"u8
}.array();

internal static readonly ΔTime maxTime = /* Time(math.MaxInt64) */ 9223372036854775807;

[GoType("num:int64")] partial struct ΔTime;

// Sub subtracts t0 from t, returning the duration in nanoseconds.
public static time.Duration Sub(this ΔTime t, ΔTime t0) {
    return ((time.Duration)(((int64)t) - ((int64)t0)));
}

// Metric provides details about a Metric event.
[GoType] partial struct ΔMetric {
    // Name is the name of the sampled metric.
    //
    // Names follow the same convention as metric names in the
    // runtime/metrics package, meaning they include the unit.
    // Names that match with the runtime/metrics package represent
    // the same quantity. Note that this corresponds to the
    // runtime/metrics package for the Go version this trace was
    // collected for.
    public @string Name;
    // Value is the sampled value of the metric.
    //
    // The Value's Kind is tied to the name of the metric, and so is
    // guaranteed to be the same for metric samples for the same metric.
    public Value Value;
}

// Label provides details about a Label event.
[GoType] partial struct ΔLabel {
    // Label is the label applied to some resource.
    public @string Label;
    // Resource is the resource to which this label should be applied.
    public ResourceID Resource;
}

// Range provides details about a Range event.
[GoType] partial struct ΔRange {
    // Name is a human-readable name for the range.
    //
    // This name can be used to identify the end of the range for the resource
    // its scoped to, because only one of each type of range may be active on
    // a particular resource. The relevant resource should be obtained from the
    // Event that produced these details. The corresponding RangeEnd will have
    // an identical name.
    public @string Name;
    // Scope is the resource that the range is scoped to.
    //
    // For example, a ResourceGoroutine scope means that the same goroutine
    // must have a start and end for the range, and that goroutine can only
    // have one range of a particular name active at any given time. The
    // ID that this range is scoped to may be obtained via Event.Goroutine.
    //
    // The ResourceNone scope means that the range is globally scoped. As a
    // result, any goroutine/proc/thread may start or end the range, and only
    // one such named range may be active globally at any given time.
    //
    // For RangeBegin and RangeEnd events, this will always reference some
    // resource ID in the current execution context. For RangeActive events,
    // this may reference a resource not in the current context. Prefer Scope
    // over the current execution context.
    public ResourceID Scope;
}

// RangeAttributes provides attributes about a completed Range.
[GoType] partial struct RangeAttribute {
    // Name is the human-readable name for the range.
    public @string Name;
    // Value is the value of the attribute.
    public Value Value;
}

[GoType("num:uint64")] partial struct TaskID;

public static readonly GoUntyped NoTask = /* TaskID(^uint64(0)) */
    GoUntyped.Parse("18446744073709551615");
public static readonly TaskID BackgroundTask = /* TaskID(0) */ 0;

// Task provides details about a Task event.
[GoType] partial struct ΔTask {
    // ID is a unique identifier for the task.
    //
    // This can be used to associate the beginning of a task with its end.
    public TaskID ID;
    // ParentID is the ID of the parent task.
    public TaskID Parent;
    // Type is the taskType that was passed to runtime/trace.NewTask.
    //
    // May be "" if a task's TaskBegin event isn't present in the trace.
    public @string Type;
}

// Region provides details about a Region event.
[GoType] partial struct ΔRegion {
    // Task is the ID of the task this region is associated with.
    public TaskID Task;
    // Type is the regionType that was passed to runtime/trace.StartRegion or runtime/trace.WithRegion.
    public @string Type;
}

// Log provides details about a Log event.
[GoType] partial struct ΔLog {
    // Task is the ID of the task this region is associated with.
    public TaskID Task;
    // Category is the category that was passed to runtime/trace.Log or runtime/trace.Logf.
    public @string Category;
    // Message is the message that was passed to runtime/trace.Log or runtime/trace.Logf.
    public @string Message;
}

// Stack represents a stack. It's really a handle to a stack and it's trivially comparable.
//
// If two Stacks are equal then their Frames are guaranteed to be identical. If they are not
// equal, however, their Frames may still be equal.
[GoType] partial struct ΔStack {
    internal ж<evTable> table;
    internal stackID id;
}

// Frames is an iterator over the frames in a Stack.
public static bool Frames(this ΔStack s, Func<StackFrame, bool> yield) {
    if (s.id == 0) {
        return true;
    }
    var stk = s.table.stacks.mustGet(s.id);
    foreach (var (_, pc) in stk.pcs) {
        var f = s.table.pcs[pc];
        var sf = new StackFrame(
            PC: f.pc,
            Func: s.table.strings.mustGet(f.funcID),
            File: s.table.strings.mustGet(f.fileID),
            Line: f.line
        );
        if (!yield(sf)) {
            return false;
        }
    }
    return true;
}

// NoStack is a sentinel value that can be compared against any Stack value, indicating
// a lack of a stack trace.
public static ΔStack NoStack = new ΔStack(nil);

// StackFrame represents a single frame of a stack.
[GoType] partial struct StackFrame {
    // PC is the program counter of the function call if this
    // is not a leaf frame. If it's a leaf frame, it's the point
    // at which the stack trace was taken.
    public uint64 PC;
    // Func is the name of the function this frame maps to.
    public @string Func;
    // File is the file which contains the source code of Func.
    public @string File;
    // Line is the line number within File which maps to PC.
    public uint64 Line;
}

// ExperimentalEvent presents a raw view of an experimental event's arguments and thier names.
[GoType] partial struct ExperimentalEvent {
    // Name is the name of the event.
    public @string Name;
    // ArgNames is the names of the event's arguments in order.
    // This may refer to a globally shared slice. Copy before mutating.
    public slice<@string> ArgNames;
    // Args contains the event's arguments.
    public slice<uint64> Args;
    // Data is additional unparsed data that is associated with the experimental event.
    // Data is likely to be shared across many ExperimentalEvents, so callers that parse
    // Data are encouraged to cache the parse result and look it up by the value of Data.
    public ж<ExperimentalData> Data;
}

// ExperimentalData represents some raw and unparsed sidecar data present in the trace that is
// associated with certain kinds of experimental events. For example, this data may contain
// tables needed to interpret ExperimentalEvent arguments, or the ExperimentEvent could just be
// a placeholder for a differently encoded event that's actually present in the experimental data.
[GoType] partial struct ExperimentalData {
    // Batches contain the actual experimental data, along with metadata about each batch.
    public slice<ExperimentalBatch> Batches;
}

// ExperimentalBatch represents a packet of unparsed data along with metadata about that packet.
[GoType] partial struct ExperimentalBatch {
    // Thread is the ID of the thread that produced a packet of data.
    public ThreadID Thread;
    // Data is a packet of unparsed data all produced by one thread.
    public slice<byte> Data;
}

// Event represents a single event in the trace.
[GoType] partial struct ΔEvent {
    internal ж<evTable> table;
    internal schedCtx ctx;
    internal baseEvent @base;
}

// Kind returns the kind of event that this is.
public static EventKind Kind(this ΔEvent e) {
    return go122Type2Kind[e.@base.typ];
}

// Time returns the timestamp of the event.
public static ΔTime Time(this ΔEvent e) {
    return e.@base.time;
}

// Goroutine returns the ID of the goroutine that was executing when
// this event happened. It describes part of the execution context
// for this event.
//
// Note that for goroutine state transitions this always refers to the
// state before the transition. For example, if a goroutine is just
// starting to run on this thread and/or proc, then this will return
// NoGoroutine. In this case, the goroutine starting to run will be
// can be found at Event.StateTransition().Resource.
public static GoID Goroutine(this ΔEvent e) {
    return e.ctx.G;
}

// Proc returns the ID of the proc this event event pertains to.
//
// Note that for proc state transitions this always refers to the
// state before the transition. For example, if a proc is just
// starting to run on this thread, then this will return NoProc.
public static ProcID Proc(this ΔEvent e) {
    return e.ctx.P;
}

// Thread returns the ID of the thread this event pertains to.
//
// Note that for thread state transitions this always refers to the
// state before the transition. For example, if a thread is just
// starting to run, then this will return NoThread.
//
// Note: tracking thread state is not currently supported, so this
// will always return a valid thread ID. However thread state transitions
// may be tracked in the future, and callers must be robust to this
// possibility.
public static ThreadID Thread(this ΔEvent e) {
    return e.ctx.M;
}

// Stack returns a handle to a stack associated with the event.
//
// This represents a stack trace at the current moment in time for
// the current execution context.
public static ΔStack Stack(this ΔEvent e) {
    if (e.@base.typ == evSync) {
        return NoStack;
    }
    if (e.@base.typ == go122.EvCPUSample) {
        return new ΔStack(table: e.table, id: ((stackID)e.@base.args[0]));
    }
    var spec = go122.Specs()[e.@base.typ];
    if (len(spec.StackIDs) == 0) {
        return NoStack;
    }
    // The stack for the main execution context is always the
    // first stack listed in StackIDs. Subtract one from this
    // because we've peeled away the timestamp argument.
    var id = ((stackID)e.@base.args[spec.StackIDs[0] - 1]);
    if (id == 0) {
        return NoStack;
    }
    return new ΔStack(table: e.table, id: id);
}

// Metric returns details about a Metric event.
//
// Panics if Kind != EventMetric.
public static ΔMetric Metric(this ΔEvent e) {
    if (e.Kind() != EventMetric) {
        throw panic("Metric called on non-Metric event");
    }
    ΔMetric m = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == go122.EvProcsChange) {
        m.Name = "/sched/gomaxprocs:threads"u8;
        m.Value = new Value(kind: ValueUint64, scalar: e.@base.args[0]);
    }
    else if (exprᴛ1 == go122.EvHeapAlloc) {
        m.Name = "/memory/classes/heap/objects:bytes"u8;
        m.Value = new Value(kind: ValueUint64, scalar: e.@base.args[0]);
    }
    else if (exprᴛ1 == go122.EvHeapGoal) {
        m.Name = "/gc/heap/goal:bytes"u8;
        m.Value = new Value(kind: ValueUint64, scalar: e.@base.args[0]);
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected event type for Metric kind: %s"u8, go122.EventString(e.@base.typ)));
    }

    return m;
}

// Label returns details about a Label event.
//
// Panics if Kind != EventLabel.
public static ΔLabel Label(this ΔEvent e) {
    if (e.Kind() != EventLabel) {
        throw panic("Label called on non-Label event");
    }
    if (e.@base.typ != go122.EvGoLabel) {
        throw panic(fmt.Sprintf("internal error: unexpected event type for Label kind: %s"u8, go122.EventString(e.@base.typ)));
    }
    return new ΔLabel(
        ΔLabel: e.table.strings.mustGet(((stringID)e.@base.args[0])),
        Resource: new ResourceID(Kind: ResourceGoroutine, id: ((int64)e.ctx.G))
    );
}

// Range returns details about an EventRangeBegin, EventRangeActive, or EventRangeEnd event.
//
// Panics if Kind != EventRangeBegin, Kind != EventRangeActive, and Kind != EventRangeEnd.
public static ΔRange Range(this ΔEvent e) {
    {
        var kind = e.Kind(); if (kind != EventRangeBegin && kind != EventRangeActive && kind != EventRangeEnd) {
            throw panic("Range called on non-Range event");
        }
    }
    ΔRange r = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == go122.EvSTWBegin || exprᴛ1 == go122.EvSTWEnd) {
        r.Name = "stop-the-world ("u8 + e.table.strings.mustGet(((stringID)e.@base.args[0])) + ")"u8;
        r.Scope = new ResourceID( // N.B. ordering.advance smuggles in the STW reason as e.base.args[0]
 // for go122.EvSTWEnd (it's already there for Begin).
Kind: ResourceGoroutine, id: ((int64)e.Goroutine()));
    }
    else if (exprᴛ1 == go122.EvGCBegin || exprᴛ1 == go122.EvGCActive || exprᴛ1 == go122.EvGCEnd) {
        r.Name = "GC concurrent mark phase"u8;
        r.Scope = new ResourceID(Kind: ResourceNone);
    }
    else if (exprᴛ1 == go122.EvGCSweepBegin || exprᴛ1 == go122.EvGCSweepActive || exprᴛ1 == go122.EvGCSweepEnd) {
        r.Name = "GC incremental sweep"u8;
        r.Scope = new ResourceID(Kind: ResourceProc);
        if (e.@base.typ == go122.EvGCSweepActive){
            r.Scope.id = ((int64)e.@base.args[0]);
        } else {
            r.Scope.id = ((int64)e.Proc());
        }
        r.Scope.id = ((int64)e.Proc());
    }
    else if (exprᴛ1 == go122.EvGCMarkAssistBegin || exprᴛ1 == go122.EvGCMarkAssistActive || exprᴛ1 == go122.EvGCMarkAssistEnd) {
        r.Name = "GC mark assist"u8;
        r.Scope = new ResourceID(Kind: ResourceGoroutine);
        if (e.@base.typ == go122.EvGCMarkAssistActive){
            r.Scope.id = ((int64)e.@base.args[0]);
        } else {
            r.Scope.id = ((int64)e.Goroutine());
        }
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected event type for Range kind: %s"u8, go122.EventString(e.@base.typ)));
    }

    return r;
}

// RangeAttributes returns attributes for a completed range.
//
// Panics if Kind != EventRangeEnd.
public static slice<RangeAttribute> RangeAttributes(this ΔEvent e) {
    if (e.Kind() != EventRangeEnd) {
        throw panic("Range called on non-Range event");
    }
    if (e.@base.typ != go122.EvGCSweepEnd) {
        return default!;
    }
    return new RangeAttribute[]{
        new(
            Name: "bytes swept"u8,
            Value: new Value(kind: ValueUint64, scalar: e.@base.args[0])
        ),
        new(
            Name: "bytes reclaimed"u8,
            Value: new Value(kind: ValueUint64, scalar: e.@base.args[1])
        )
    }.slice();
}

// Task returns details about a TaskBegin or TaskEnd event.
//
// Panics if Kind != EventTaskBegin and Kind != EventTaskEnd.
public static ΔTask Task(this ΔEvent e) {
    {
        var kind = e.Kind(); if (kind != EventTaskBegin && kind != EventTaskEnd) {
            throw panic("Task called on non-Task event");
        }
    }
    var parentID = NoTask;
    @string typ = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == go122.EvUserTaskBegin) {
        parentID = ((TaskID)e.@base.args[1]);
        typ = e.table.strings.mustGet(((stringID)e.@base.args[2]));
    }
    else if (exprᴛ1 == go122.EvUserTaskEnd) {
        parentID = ((TaskID)e.@base.extra(version.Go122)[0]);
        typ = e.table.getExtraString(((extraStringID)e.@base.extra(version.Go122)[1]));
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected event type for Task kind: %s"u8, go122.EventString(e.@base.typ)));
    }

    return new ΔTask(
        ID: ((TaskID)e.@base.args[0]),
        Parent: parentID,
        Type: typ
    );
}

// Region returns details about a RegionBegin or RegionEnd event.
//
// Panics if Kind != EventRegionBegin and Kind != EventRegionEnd.
public static ΔRegion Region(this ΔEvent e) {
    {
        var kind = e.Kind(); if (kind != EventRegionBegin && kind != EventRegionEnd) {
            throw panic("Region called on non-Region event");
        }
    }
    if (e.@base.typ != go122.EvUserRegionBegin && e.@base.typ != go122.EvUserRegionEnd) {
        throw panic(fmt.Sprintf("internal error: unexpected event type for Region kind: %s"u8, go122.EventString(e.@base.typ)));
    }
    return new ΔRegion(
        ΔTask: ((TaskID)e.@base.args[0]),
        Type: e.table.strings.mustGet(((stringID)e.@base.args[1]))
    );
}

// Log returns details about a Log event.
//
// Panics if Kind != EventLog.
public static ΔLog Log(this ΔEvent e) {
    if (e.Kind() != EventLog) {
        throw panic("Log called on non-Log event");
    }
    if (e.@base.typ != go122.EvUserLog) {
        throw panic(fmt.Sprintf("internal error: unexpected event type for Log kind: %s"u8, go122.EventString(e.@base.typ)));
    }
    return new ΔLog(
        ΔTask: ((TaskID)e.@base.args[0]),
        Category: e.table.strings.mustGet(((stringID)e.@base.args[1])),
        Message: e.table.strings.mustGet(((stringID)e.@base.args[2]))
    );
}

// StateTransition returns details about a StateTransition event.
//
// Panics if Kind != EventStateTransition.
public static ΔStateTransition StateTransition(this ΔEvent e) {
    if (e.Kind() != EventStateTransition) {
        throw panic("StateTransition called on non-StateTransition event");
    }
    ΔStateTransition s = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == go122.EvProcStart) {
        s = procStateTransition(((ProcID)e.@base.args[0]), ProcIdle, ProcRunning);
    }
    else if (exprᴛ1 == go122.EvProcStop) {
        s = procStateTransition(e.ctx.P, ProcRunning, ProcIdle);
    }
    else if (exprᴛ1 == go122.EvProcSteal) {
        var beforeState = ProcRunning;
        if (((go122.ProcStatus)e.@base.extra(version.Go122)[0]) == go122.ProcSyscallAbandoned) {
            // N.B. ordering.advance populates e.base.extra.
            // We've lost information because this ProcSteal advanced on a
            // SyscallAbandoned state. Treat the P as idle because ProcStatus
            // treats SyscallAbandoned as Idle. Otherwise we'll have an invalid
            // transition.
            beforeState = ProcIdle;
        }
        s = procStateTransition(((ProcID)e.@base.args[0]), beforeState, ProcIdle);
    }
    else if (exprᴛ1 == go122.EvProcStatus) {
        s = procStateTransition(((ProcID)e.@base.args[0]), // N.B. ordering.advance populates e.base.extra.
 ((ProcState)e.@base.extra(version.Go122)[0]), go122ProcStatus2ProcState[e.@base.args[1]]);
    }
    else if (exprᴛ1 == go122.EvGoCreate || exprᴛ1 == go122.EvGoCreateBlocked) {
        var status = GoRunnable;
        if (e.@base.typ == go122.EvGoCreateBlocked) {
            status = GoWaiting;
        }
        s = goStateTransition(((GoID)e.@base.args[0]), GoNotExist, status);
        s.Stack = new ΔStack(table: e.table, id: ((stackID)e.@base.args[1]));
    }
    else if (exprᴛ1 == go122.EvGoCreateSyscall) {
        s = goStateTransition(((GoID)e.@base.args[0]), GoNotExist, GoSyscall);
    }
    else if (exprᴛ1 == go122.EvGoStart) {
        s = goStateTransition(((GoID)e.@base.args[0]), GoRunnable, GoRunning);
    }
    else if (exprᴛ1 == go122.EvGoDestroy) {
        s = goStateTransition(e.ctx.G, GoRunning, GoNotExist);
        s.Stack = e.Stack();
    }
    else if (exprᴛ1 == go122.EvGoDestroySyscall) {
        s = goStateTransition(e.ctx.G, // This event references the resource the event happened on.
 GoSyscall, GoNotExist);
    }
    else if (exprᴛ1 == go122.EvGoStop) {
        s = goStateTransition(e.ctx.G, GoRunning, GoRunnable);
        s.Reason = e.table.strings.mustGet(((stringID)e.@base.args[0]));
        s.Stack = e.Stack();
    }
    else if (exprᴛ1 == go122.EvGoBlock) {
        s = goStateTransition(e.ctx.G, // This event references the resource the event happened on.
 GoRunning, GoWaiting);
        s.Reason = e.table.strings.mustGet(((stringID)e.@base.args[0]));
        s.Stack = e.Stack();
    }
    else if (exprᴛ1 == go122.EvGoUnblock || exprᴛ1 == go122.EvGoSwitch || exprᴛ1 == go122.EvGoSwitchDestroy) {
        s = goStateTransition(((GoID)e.@base.args[0]), // This event references the resource the event happened on.
 // N.B. GoSwitch and GoSwitchDestroy both emit additional events, but
 // the first thing they both do is unblock the goroutine they name,
 // identically to an unblock event (even their arguments match).
 GoWaiting, GoRunnable);
    }
    else if (exprᴛ1 == go122.EvGoSyscallBegin) {
        s = goStateTransition(e.ctx.G, GoRunning, GoSyscall);
        s.Stack = e.Stack();
    }
    else if (exprᴛ1 == go122.EvGoSyscallEnd) {
        s = goStateTransition(e.ctx.G, // This event references the resource the event happened on.
 GoSyscall, GoRunning);
        s.Stack = e.Stack();
    }
    else if (exprᴛ1 == go122.EvGoSyscallEndBlocked) {
        s = goStateTransition(e.ctx.G, // This event references the resource the event happened on.
 GoSyscall, GoRunnable);
        s.Stack = e.Stack();
    }
    else if (exprᴛ1 == go122.EvGoStatus || exprᴛ1 == go122.EvGoStatusStack) {
        s = goStateTransition(((GoID)e.@base.args[0]), // This event references the resource the event happened on.
 // N.B. ordering.advance populates e.base.extra.
 ((GoState)e.@base.extra(version.Go122)[0]), go122GoStatus2GoState[e.@base.args[2]]);
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected event type for StateTransition kind: %s"u8, go122.EventString(e.@base.typ)));
    }

    return s;
}

// Experimental returns a view of the raw event for an experimental event.
//
// Panics if Kind != EventExperimental.
public static ExperimentalEvent Experimental(this ΔEvent e) {
    if (e.Kind() != EventExperimental) {
        throw panic("Experimental called on non-Experimental event");
    }
    var spec = go122.Specs()[e.@base.typ];
    var argNames = spec.Args[1..];
    // Skip timestamp; already handled.
    return new ExperimentalEvent(
        Name: spec.Name,
        ArgNames: argNames,
        Args: e.@base.args[..(int)(len(argNames))],
        Data: e.table.expData[spec.Experiment]
    );
}

internal static readonly @event.Type evSync = /* ^event.Type(0) */ 255;

internal static array<EventKind> go122Type2Kind = new runtime.SparseArray<EventKind>{
    [go122.EvCPUSample] = EventStackSample,
    [go122.EvProcsChange] = EventMetric,
    [go122.EvProcStart] = EventStateTransition,
    [go122.EvProcStop] = EventStateTransition,
    [go122.EvProcSteal] = EventStateTransition,
    [go122.EvProcStatus] = EventStateTransition,
    [go122.EvGoCreate] = EventStateTransition,
    [go122.EvGoCreateSyscall] = EventStateTransition,
    [go122.EvGoStart] = EventStateTransition,
    [go122.EvGoDestroy] = EventStateTransition,
    [go122.EvGoDestroySyscall] = EventStateTransition,
    [go122.EvGoStop] = EventStateTransition,
    [go122.EvGoBlock] = EventStateTransition,
    [go122.EvGoUnblock] = EventStateTransition,
    [go122.EvGoSyscallBegin] = EventStateTransition,
    [go122.EvGoSyscallEnd] = EventStateTransition,
    [go122.EvGoSyscallEndBlocked] = EventStateTransition,
    [go122.EvGoStatus] = EventStateTransition,
    [go122.EvSTWBegin] = EventRangeBegin,
    [go122.EvSTWEnd] = EventRangeEnd,
    [go122.EvGCActive] = EventRangeActive,
    [go122.EvGCBegin] = EventRangeBegin,
    [go122.EvGCEnd] = EventRangeEnd,
    [go122.EvGCSweepActive] = EventRangeActive,
    [go122.EvGCSweepBegin] = EventRangeBegin,
    [go122.EvGCSweepEnd] = EventRangeEnd,
    [go122.EvGCMarkAssistActive] = EventRangeActive,
    [go122.EvGCMarkAssistBegin] = EventRangeBegin,
    [go122.EvGCMarkAssistEnd] = EventRangeEnd,
    [go122.EvHeapAlloc] = EventMetric,
    [go122.EvHeapGoal] = EventMetric,
    [go122.EvGoLabel] = EventLabel,
    [go122.EvUserTaskBegin] = EventTaskBegin,
    [go122.EvUserTaskEnd] = EventTaskEnd,
    [go122.EvUserRegionBegin] = EventRegionBegin,
    [go122.EvUserRegionEnd] = EventRegionEnd,
    [go122.EvUserLog] = EventLog,
    [go122.EvGoSwitch] = EventStateTransition,
    [go122.EvGoSwitchDestroy] = EventStateTransition,
    [go122.EvGoCreateBlocked] = EventStateTransition,
    [go122.EvGoStatusStack] = EventStateTransition,
    [go122.EvSpan] = EventExperimental,
    [go122.EvSpanAlloc] = EventExperimental,
    [go122.EvSpanFree] = EventExperimental,
    [go122.EvHeapObject] = EventExperimental,
    [go122.EvHeapObjectAlloc] = EventExperimental,
    [go122.EvHeapObjectFree] = EventExperimental,
    [go122.EvGoroutineStack] = EventExperimental,
    [go122.EvGoroutineStackAlloc] = EventExperimental,
    [go122.EvGoroutineStackFree] = EventExperimental,
    [evSync] = EventSync
}.array();

internal static array<GoState> go122GoStatus2GoState = new runtime.SparseArray<GoState>{
    [go122.GoRunnable] = GoRunnable,
    [go122.GoRunning] = GoRunning,
    [go122.GoWaiting] = GoWaiting,
    [go122.GoSyscall] = GoSyscall
}.array();

internal static array<ProcState> go122ProcStatus2ProcState = new runtime.SparseArray<ProcState>{
    [go122.ProcRunning] = ProcRunning,
    [go122.ProcIdle] = ProcIdle,
    [go122.ProcSyscall] = ProcRunning,
    [go122.ProcSyscallAbandoned] = ProcIdle
}.array();

// String returns the event as a human-readable string.
//
// The format of the string is intended for debugging and is subject to change.
public static @string String(this ΔEvent e) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    fmt.Fprintf(~Ꮡsb, "M=%d P=%d G=%d"u8, e.Thread(), e.Proc(), e.Goroutine());
    fmt.Fprintf(~Ꮡsb, " %s Time=%d"u8, e.Kind(), e.Time());
    // Kind-specific fields.
    {
        var kind = e.Kind();
        var exprᴛ1 = kind;
        if (exprᴛ1 == EventMetric) {
            var m = e.Metric();
            fmt.Fprintf(~Ꮡsb, " Name=%q Value=%s"u8, m.Name, valueAsString(m.Value));
        }
        else if (exprᴛ1 == EventLabel) {
            var l = e.Label();
            fmt.Fprintf(~Ꮡsb, " Label=%q Resource=%s"u8, l.Label, l.Resource);
        }
        else if (exprᴛ1 == EventRangeBegin || exprᴛ1 == EventRangeActive || exprᴛ1 == EventRangeEnd) {
            var r = e.Range();
            fmt.Fprintf(~Ꮡsb, " Name=%q Scope=%s"u8, r.Name, r.Scope);
            if (kind == EventRangeEnd) {
                fmt.Fprintf(~Ꮡsb, " Attributes=["u8);
                foreach (var (i, attr) in e.RangeAttributes()) {
                    if (i != 0) {
                        fmt.Fprintf(~Ꮡsb, " "u8);
                    }
                    fmt.Fprintf(~Ꮡsb, "%q=%s"u8, attr.Name, valueAsString(attr.Value));
                }
                fmt.Fprintf(~Ꮡsb, "]"u8);
            }
        }
        else if (exprᴛ1 == EventTaskBegin || exprᴛ1 == EventTaskEnd) {
            var t = e.Task();
            fmt.Fprintf(~Ꮡsb, " ID=%d Parent=%d Type=%q"u8, t.ID, t.Parent, t.Type);
        }
        else if (exprᴛ1 == EventRegionBegin || exprᴛ1 == EventRegionEnd) {
            var r = e.Region();
            fmt.Fprintf(~Ꮡsb, " Task=%d Type=%q"u8, r.Task, r.Type);
        }
        else if (exprᴛ1 == EventLog) {
            var l = e.Log();
            fmt.Fprintf(~Ꮡsb, " Task=%d Category=%q Message=%q"u8, l.Task, l.Category, l.Message);
        }
        else if (exprᴛ1 == EventStateTransition) {
            var s = e.StateTransition();
            fmt.Fprintf(~Ꮡsb, " Resource=%s Reason=%q"u8, s.Resource, s.Reason);
            var exprᴛ2 = s.Resource.Kind;
            if (exprᴛ2 == ResourceGoroutine) {
                var id = s.Resource.Goroutine();
                var (old, @new) = s.Goroutine();
                fmt.Fprintf(~Ꮡsb, " GoID=%d %s->%s"u8, id, old, @new);
            }
            else if (exprᴛ2 == ResourceProc) {
                var id = s.Resource.Proc();
                var (old, @new) = s.Proc();
                fmt.Fprintf(~Ꮡsb, " ProcID=%d %s->%s"u8, id, old, @new);
            }

            if (s.Stack != NoStack) {
                fmt.Fprintln(~Ꮡsb);
                fmt.Fprintln(~Ꮡsb, "TransitionStack=");
                s.Stack.Frames(
                var sbʗ2 = sb;
                (StackFrame f) => {
                    fmt.Fprintf(~Ꮡsbʗ2, "\t%s @ 0x%x\n"u8, f.Func, f.PC);
                    fmt.Fprintf(~Ꮡsbʗ2, "\t\t%s:%d\n"u8, f.File, f.Line);
                    return true;
                });
            }
        }
        if (exprᴛ1 == EventExperimental) {
            var r = e.Experimental();
            fmt.Fprintf(~Ꮡsb, " Name=%s ArgNames=%v Args=%v"u8, r.Name, r.ArgNames, r.Args);
        }
    }

    {
        var stk = e.Stack(); if (stk != NoStack) {
            fmt.Fprintln(~Ꮡsb);
            fmt.Fprintln(~Ꮡsb, "Stack=");
            stk.Frames(
            var sbʗ5 = sb;
            (StackFrame f) => {
                fmt.Fprintf(~Ꮡsbʗ5, "\t%s @ 0x%x\n"u8, f.Func, f.PC);
                fmt.Fprintf(~Ꮡsbʗ5, "\t\t%s:%d\n"u8, f.File, f.Line);
                return true;
            });
        }
    }
    return sb.String();
}

// validateTableIDs checks to make sure lookups in e.table
// will work.
internal static error validateTableIDs(this ΔEvent e) {
    if (e.@base.typ == evSync) {
        return default!;
    }
    var spec = go122.Specs()[e.@base.typ];
    // Check stacks.
    foreach (var (_, i) in spec.StackIDs) {
        var id = ((stackID)e.@base.args[i - 1]);
        var (_, ok) = e.table.stacks.get(id);
        if (!ok) {
            return fmt.Errorf("found invalid stack ID %d for event %s"u8, id, spec.Name);
        }
    }
    // N.B. Strings referenced by stack frames are validated
    // early on, when reading the stacks in to begin with.
    // Check strings.
    foreach (var (_, i) in spec.StringIDs) {
        var id = ((stringID)e.@base.args[i - 1]);
        var (_, ok) = e.table.strings.get(id);
        if (!ok) {
            return fmt.Errorf("found invalid string ID %d for event %s"u8, id, spec.Name);
        }
    }
    return default!;
}

internal static ΔEvent syncEvent(ж<evTable> Ꮡtable, ΔTime ts) {
    ref var table = ref Ꮡtable.val;

    return new ΔEvent(
        table: table,
        ctx: new schedCtx(
            G: NoGoroutine,
            P: NoProc,
            M: NoThread
        ),
        @base: new baseEvent(
            typ: evSync,
            time: ts
        )
    );
}

} // end trace_package
