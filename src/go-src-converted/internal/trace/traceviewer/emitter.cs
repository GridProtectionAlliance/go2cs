// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using json = encoding.json_package;
using fmt = fmt_package;
using trace = @internal.trace_package;
using format = @internal.trace.traceviewer.format_package;
using io = io_package;
using strconv = strconv_package;
using time = time_package;
using @internal;
using @internal.trace.traceviewer;
using encoding;

partial class traceviewer_package {

[GoType] partial struct TraceConsumer {
    public Action<@string> ConsumeTimeUnit;
    public format.Event, required bool) ConsumeViewerEvent;
    public format.Frame) ConsumeViewerFrame;
    public Action Flush;
}

// ViewerDataTraceConsumer returns a TraceConsumer that writes to w. The
// startIdx and endIdx are used for splitting large traces. They refer to
// indexes in the traceEvents output array, not the events in the trace input.
public static TraceConsumer ViewerDataTraceConsumer(io.Writer w, int64 startIdx, int64 endIdx) {
    var allFrames = new format.Frame();
    var requiredFrames = new format.Frame();
    var enc = json.NewEncoder(w);
    nint written = 0;
    var index = ((int64)(-1));
    io.WriteString(w, "{"u8);
    return new TraceConsumer(
        ConsumeTimeUnit: 
        var encʗ1 = enc;
        (@string unit) => {
            io.WriteString(w, @"""displayTimeUnit"":"u8);
            encʗ1.Encode(unit);
            io.WriteString(w, ","u8);
        },
        ConsumeViewerEvent: 
        var allFramesʗ1 = allFrames;
        var encʗ2 = enc;
        var requiredFramesʗ1 = requiredFrames;
        (ж<format.Event> v, bool required) => {
            index++;
            if (!required && (index < startIdx || index > endIdx)) {
                // not in the range. Skip!
                return;
            }
            WalkStackFrames(allFramesʗ1, (~v).Stack, 
            var allFramesʗ2 = allFrames;
            var requiredFramesʗ2 = requiredFrames;
            (nint id) => {
                @string s = strconv.Itoa(id);
                requiredFramesʗ2[s] = allFramesʗ2[s];
            });
            WalkStackFrames(allFrames, (~v).EndStack, 
            var allFramesʗ4 = allFrames;
            var requiredFramesʗ4 = requiredFrames;
            (nint id) => {
                @string s = strconv.Itoa(id);
                requiredFramesʗ4[s] = allFramesʗ4[s];
            });
            if (written == 0) {
                io.WriteString(w, @"""traceEvents"": ["u8);
            }
            if (written > 0) {
                io.WriteString(w, ","u8);
            }
            enc.Encode(v);
            // TODO(mknyszek): get rid of the extra \n inserted by enc.Encode.
            // Same should be applied to splittingTraceConsumer.
            written++;
        },
        ConsumeViewerFrame: 
        var allFramesʗ6 = allFrames;
        (@string k, format.Frame v) => {
            allFramesʗ6[k] = v;
        },
        Flush: 
        var encʗ3 = enc;
        var requiredFramesʗ6 = requiredFrames;
        () => {
            io.WriteString(w, @"], ""stackFrames"":"u8);
            encʗ3.Encode(requiredFramesʗ6);
            io.WriteString(w, @"}"u8);
        }
    );
}

[GoType("dyn")] partial struct SplittingTraceConsumer_eventSz {
    public float64 Time;
    public nint Sz;
    public slice<nint> Frames;
}

public static (ж<splitter>, TraceConsumer) SplittingTraceConsumer(nint max) {
    ref var data = ref heap(new @internal.trace.traceviewer.format_package.Data(), out var Ꮡdata);

    data = new format.Data(Frames: new format.Frame());
    format.Frame allFrames = new format.Frame();
    slice<eventSz> sizes = default!;
    ref var cw = ref heap(new countingWriter(), out var Ꮡcw);
    var s = @new<splitter>();
    return (s, new TraceConsumer(
        ConsumeTimeUnit: 
        var dataʗ1 = data;
        (@string unit) => {
            dataʗ1.TimeUnit = unit;
        },
        ConsumeViewerEvent: 
        var allFramesʗ1 = allFrames;
        var cwʗ1 = cw;
        var dataʗ2 = data;
        var sizesʗ1 = sizes;
        (ж<format.Event> v, bool required) => {
            if (required) {
                // Store required events inside data so flush
                // can include them in the required part of the
                // trace.
                dataʗ2.Events = append(dataʗ2.Events, v);
                WalkStackFrames(allFramesʗ1, (~v).Stack, 
                var allFramesʗ2 = allFrames;
                var dataʗ3 = data;
                (nint id) => {
                    @string sΔ1 = strconv.Itoa(id);
                    dataʗ3.Frames[s] = allFramesʗ2[sΔ1];
                });
                WalkStackFrames(allFrames, (~v).EndStack, 
                var allFramesʗ4 = allFrames;
                var dataʗ5 = data;
                (nint id) => {
                    @string sΔ2 = strconv.Itoa(id);
                    dataʗ5.Frames[s] = allFramesʗ4[sΔ2];
                });
                return;
            }
            var enc = json.NewEncoder(~Ꮡcw);
            enc.Encode(v);
            ref var size = ref heap<SplittingTraceConsumer_eventSz>(out var Ꮡsize);
            size = new eventSz(Time: (~v).Time, Sz: cw.size + 1);
            // +1 for ",".
            // Add referenced stack frames. Their size is computed
            // in flush, where we can dedup across events.
            WalkStackFrames(allFrames, (~v).Stack, 
            var sizeʗ1 = size;
            (nint id) => {
                sizeʗ1.Frames = append(sizeʗ1.Frames, id);
            });
            WalkStackFrames(allFrames, (~v).EndStack, 
            var sizeʗ3 = size;
            (nint id) => {
                sizeʗ3.Frames = append(sizeʗ3.Frames, id);
            });
            // This may add duplicates. We'll dedup later.
            sizes = append(sizes, size);
            cw.size = 0;
        },
        ConsumeViewerFrame: 
        var allFramesʗ6 = allFrames;
        (@string k, format.Frame v) => {
            allFramesʗ6[k] = v;
        },
        Flush: 
        var allFramesʗ7 = allFrames;
        var cwʗ2 = cw;
        var dataʗ7 = data;
        var sʗ1 = s;
        var sizesʗ2 = sizes;
        () => {
            // Calculate size of the mandatory part of the trace.
            // This includes thread names and stack frames for
            // required events.
            cwʗ2.size = 0;
            var enc = json.NewEncoder(~Ꮡcwʗ2);
            enc.Encode(dataʗ7);
            nint requiredSize = cwʗ2.size;
            // Then calculate size of each individual event and
            // their stack frames, grouping them into ranges. We
            // only include stack frames relevant to the events in
            // the range to reduce overhead.
            nint start = 0;
            nint eventsSize = 0;
            format.Frame frames = new format.Frame();
            nint framesSize = 0;
            ref var ev = ref heap(new SplittingTraceConsumer_eventSz(), out var Ꮡev);

            foreach (var (i, ev) in sizesʗ2) {
                eventsSize += ev.Sz;
                // Add required stack frames. Note that they
                // may already be in the map.
                foreach (var (_, id) in ev.Frames) {
                    @string sʗ1 = strconv.Itoa(id);
                    var _ = frames[sʗ1];
                    var ok = frames[sʗ1];
                    if (ok) {
                        continue;
                    }
                    ref var f = ref heap<@internal.trace.traceviewer.format_package.Frame>(out var Ꮡf);
                    f = allFramesʗ7[sʗ1];
                    frames[sʗ1] = f;
                    framesSize += stackFrameEncodedSize(((nuint)id), f);
                }
                nint total = requiredSize + framesSize + eventsSize;
                if (total < max) {
                    continue;
                }
                // Reached max size, commit this range and
                // start a new range.
                var startTime = ((time.Duration)(sizesʗ2[start].Time * 1000));
                var endTime = ((time.Duration)(ev.Time * 1000));
                sʗ1.val.Ranges = append((~sʗ1).Ranges, new Range(
                    Name: fmt.Sprintf("%v-%v"u8, startTime, endTime),
                    Start: start,
                    End: i + 1,
                    StartTime: ((int64)startTime),
                    EndTime: ((int64)endTime)
                ));
                start = i + 1;
                frames = new format.Frame();
                framesSize = 0;
                eventsSize = 0;
            }
            if (len((~sʗ1).Ranges) <= 1) {
                sʗ1.val.Ranges = default!;
                return;
            }
            {
                nint end = len(sizesʗ2) - 1; if (start < end) {
                    sʗ1.val.Ranges = append((~sʗ1).Ranges, new Range(
                        Name: fmt.Sprintf("%v-%v"u8, ((time.Duration)(sizesʗ2[start].Time * 1000)), ((time.Duration)(sizesʗ2[end].Time * 1000))),
                        Start: start,
                        End: end,
                        StartTime: ((int64)(sizesʗ2[start].Time * 1000)),
                        EndTime: ((int64)(sizesʗ2[end].Time * 1000))
                    ));
                }
            }
        }
    ));
}

[GoType] partial struct splitter {
    public slice<Range> Ranges;
}

[GoType] partial struct countingWriter {
    internal nint size;
}

[GoRecv] internal static (nint, error) Write(this ref countingWriter cw, slice<byte> data) {
    cw.size += len(data);
    return (len(data), default!);
}

internal static nint stackFrameEncodedSize(nuint id, format.Frame f) {
    // We want to know the marginal size of traceviewer.Data.Frames for
    // each event. Running full JSON encoding of the map for each event is
    // far too slow.
    //
    // Since the format is fixed, we can easily compute the size without
    // encoding.
    //
    // A single entry looks like one of the following:
    //
    //   "1":{"name":"main.main:30"},
    //   "10":{"name":"pkg.NewSession:173","parent":9},
    //
    // The parent is omitted if 0. The trailing comma is omitted from the
    // last entry, but we don't need that much precision.
    const nint baseSize = /* len(`"`) + len(`":{"name":"`) + len(`"},`) */ 15;
    
    const nint parentBaseSize = /* len(`,"parent":`) */ 10;
    nint size = baseSize;
    size += len(f.Name);
    // Bytes for id (always positive).
    while (id > 0) {
        size += 1;
        id /= 10;
    }
    if (f.Parent > 0) {
        size += parentBaseSize;
        // Bytes for parent (always positive).
        while (f.Parent > 0) {
            size += 1;
            f.Parent /= 10;
        }
    }
    return size;
}

// WalkStackFrames calls fn for id and all of its parent frames from allFrames.
public static void WalkStackFrames(format.Frame allFrames, nint id, Action<nint> fn) {
    while (id != 0) {
        var (f, ok) = allFrames[strconv.Itoa(id)];
        if (!ok) {
            break;
        }
        fn(id);
        id = f.Parent;
    }
}

[GoType("num:nint")] partial struct Mode;

public static readonly Mode ModeGoroutineOriented = /* 1 << iota */ 1;
public static readonly Mode ModeTaskOriented = 2;
public static readonly Mode ModeThreadOriented = 4; // Mutually exclusive with ModeGoroutineOriented.

// NewEmitter returns a new Emitter that writes to c. The rangeStart and
// rangeEnd args are used for splitting large traces.
public static ж<Emitter> NewEmitter(TraceConsumer c, time.Duration rangeStart, time.Duration rangeEnd) {
    c.ConsumeTimeUnit("ns");
    return Ꮡ(new Emitter(
        c: c,
        rangeStart: rangeStart,
        rangeEnd: rangeEnd,
        frameTree: new frameNode(children: new map<uint64, frameNode>()),
        resources: new map<uint64, @string>(),
        tasks: new map<uint64, task>()
    ));
}

[GoType] partial struct Emitter {
    internal TraceConsumer c;
    internal time_package.Duration rangeStart;
    internal time_package.Duration rangeEnd;
    internal heapStats heapStats;
    internal heapStats prevHeapStats;
    internal array<int64> gstates = new(gStateCount);
    internal array<int64> prevGstates = new(gStateCount);
    internal array<int64> threadStats = new(threadStateCount);
    internal array<int64> prevThreadStats = new(threadStateCount);
    internal uint64 gomaxprocs;
    internal frameNode frameTree;
    internal nint frameSeq;
    internal uint64 arrowSeq;
    internal Func<uint64, bool> filter;
    internal @string resourceType;
    internal map<uint64, @string> resources;
    internal uint64 focusResource;
    internal map<uint64, task> tasks;
    internal uint64 asyncSliceSeq;
}

[GoType] partial struct task {
    internal @string name;
    internal nint sortIndex;
}

[GoRecv] public static void Gomaxprocs(this ref Emitter e, uint64 v) {
    if (v > e.gomaxprocs) {
        e.gomaxprocs = v;
    }
}

[GoRecv] public static void Resource(this ref Emitter e, uint64 id, @string name) {
    if (e.filter != default! && !e.filter(id)) {
        return;
    }
    e.resources[id] = name;
}

[GoRecv] public static void SetResourceType(this ref Emitter e, @string name) {
    e.resourceType = name;
}

[GoRecv] public static void SetResourceFilter(this ref Emitter e, Func<uint64, bool> filter) {
    e.filter = filter;
}

[GoRecv] public static void Task(this ref Emitter e, uint64 id, @string name, nint sortIndex) {
    e.tasks[id] = new task(name, sortIndex);
}

[GoRecv] public static void Slice(this ref Emitter e, SliceEvent s) {
    if (e.filter != default! && !e.filter(s.Resource)) {
        return;
    }
    e.Δslice(s, format.ProcsSection, ""u8);
}

[GoRecv] public static void TaskSlice(this ref Emitter e, SliceEvent s) {
    e.Δslice(s, format.TasksSection, pickTaskColor(s.Resource));
}

[GoRecv] internal static void Δslice(this ref Emitter e, SliceEvent s, uint64 sectionID, @string cname) {
    if (!e.tsWithinRange(s.Ts) && !e.tsWithinRange(s.Ts + s.Dur)) {
        return;
    }
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: s.Name,
        Phase: "X"u8,
        Time: viewerTime(s.Ts),
        Dur: viewerTime(s.Dur),
        PID: sectionID,
        TID: s.Resource,
        Stack: s.Stack,
        EndStack: s.EndStack,
        Arg: s.Arg,
        Cname: cname
    )));
}

[GoType] partial struct SliceEvent {
    public @string Name;
    public time_package.Duration Ts;
    public time_package.Duration Dur;
    public uint64 Resource;
    public nint Stack;
    public nint EndStack;
    public any Arg;
}

[GoRecv] public static void AsyncSlice(this ref Emitter e, AsyncSliceEvent s) {
    if (!e.tsWithinRange(s.Ts) && !e.tsWithinRange(s.Ts + s.Dur)) {
        return;
    }
    if (e.filter != default! && !e.filter(s.Resource)) {
        return;
    }
    @string cname = ""u8;
    if (s.TaskColorIndex != 0) {
        cname = pickTaskColor(s.TaskColorIndex);
    }
    e.asyncSliceSeq++;
    e.OptionalEvent(Ꮡ(new format.Event(
        Category: s.Category,
        Name: s.Name,
        Phase: "b"u8,
        Time: viewerTime(s.Ts),
        TID: s.Resource,
        ID: e.asyncSliceSeq,
        Scope: s.Scope,
        Stack: s.Stack,
        Cname: cname
    )));
    e.OptionalEvent(Ꮡ(new format.Event(
        Category: s.Category,
        Name: s.Name,
        Phase: "e"u8,
        Time: viewerTime(s.Ts + s.Dur),
        TID: s.Resource,
        ID: e.asyncSliceSeq,
        Scope: s.Scope,
        Stack: s.EndStack,
        Arg: s.Arg,
        Cname: cname
    )));
}

[GoType] partial struct AsyncSliceEvent {
    public partial ref SliceEvent SliceEvent { get; }
    public @string Category;
    public @string Scope;
    public uint64 TaskColorIndex; // Take on the same color as the task with this ID.
}

[GoRecv] public static void Instant(this ref Emitter e, InstantEvent i) {
    if (!e.tsWithinRange(i.Ts)) {
        return;
    }
    if (e.filter != default! && !e.filter(i.Resource)) {
        return;
    }
    @string cname = ""u8;
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: i.Name,
        Category: i.Category,
        Phase: "I"u8,
        Scope: "t"u8,
        Time: viewerTime(i.Ts),
        PID: format.ProcsSection,
        TID: i.Resource,
        Stack: i.Stack,
        Cname: cname,
        Arg: i.Arg
    )));
}

[GoType] partial struct InstantEvent {
    public time_package.Duration Ts;
    public @string Name;
    public @string Category;
    public uint64 Resource;
    public nint Stack;
    public any Arg;
}

[GoRecv] public static void Arrow(this ref Emitter e, ArrowEvent a) {
    if (e.filter != default! && (!e.filter(a.FromResource) || !e.filter(a.ToResource))) {
        return;
    }
    e.arrow(a, format.ProcsSection);
}

[GoRecv] public static void TaskArrow(this ref Emitter e, ArrowEvent a) {
    e.arrow(a, format.TasksSection);
}

[GoRecv] internal static void arrow(this ref Emitter e, ArrowEvent a, uint64 sectionID) {
    if (!e.tsWithinRange(a.Start) || !e.tsWithinRange(a.End)) {
        return;
    }
    e.arrowSeq++;
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: a.Name,
        Phase: "s"u8,
        TID: a.FromResource,
        PID: sectionID,
        ID: e.arrowSeq,
        Time: viewerTime(a.Start),
        Stack: a.FromStack
    )));
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: a.Name,
        Phase: "t"u8,
        TID: a.ToResource,
        PID: sectionID,
        ID: e.arrowSeq,
        Time: viewerTime(a.End)
    )));
}

[GoType] partial struct ArrowEvent {
    public @string Name;
    public time_package.Duration Start;
    public time_package.Duration End;
    public uint64 FromResource;
    public nint FromStack;
    public uint64 ToResource;
}

[GoRecv] public static void Event(this ref Emitter e, ж<format.Event> Ꮡev) {
    ref var ev = ref Ꮡev.val;

    e.c.ConsumeViewerEvent(ev, true);
}

[GoRecv] public static void HeapAlloc(this ref Emitter e, time.Duration ts, uint64 v) {
    e.heapStats.heapAlloc = v;
    e.emitHeapCounters(ts);
}

[GoRecv] public static void Focus(this ref Emitter e, uint64 id) {
    e.focusResource = id;
}

[GoRecv] public static void GoroutineTransition(this ref Emitter e, time.Duration ts, GState from, GState to) {
    e.gstates[from]--;
    e.gstates[to]++;
    if (e.prevGstates == e.gstates) {
        return;
    }
    if (e.tsWithinRange(ts)) {
        e.OptionalEvent(Ꮡ(new format.Event(
            Name: "Goroutines"u8,
            Phase: "C"u8,
            Time: viewerTime(ts),
            PID: 1,
            Arg: Ꮡ(new format.GoroutineCountersArg(
                Running: ((uint64)e.gstates[GRunning]),
                Runnable: ((uint64)e.gstates[GRunnable]),
                GCWaiting: ((uint64)e.gstates[GWaitingGC])
            ))
        )));
    }
    e.prevGstates = e.gstates;
}

[GoRecv] public static void IncThreadStateCount(this ref Emitter e, time.Duration ts, ThreadState state, int64 delta) {
    e.threadStats[state] += delta;
    if (e.prevThreadStats == e.threadStats) {
        return;
    }
    if (e.tsWithinRange(ts)) {
        e.OptionalEvent(Ꮡ(new format.Event(
            Name: "Threads"u8,
            Phase: "C"u8,
            Time: viewerTime(ts),
            PID: 1,
            Arg: Ꮡ(new format.ThreadCountersArg(
                Running: ((int64)e.threadStats[ThreadStateRunning]),
                InSyscall: ((int64)e.threadStats[ThreadStateInSyscall])
            ))
        )));
    }
    // TODO(mknyszek): Why is InSyscallRuntime not included here?
    e.prevThreadStats = e.threadStats;
}

[GoRecv] public static void HeapGoal(this ref Emitter e, time.Duration ts, uint64 v) {
    // This cutoff at 1 PiB is a Workaround for https://github.com/golang/go/issues/63864.
    //
    // TODO(mknyszek): Remove this once the problem has been fixed.
    static readonly UntypedInt PB = /* 1 << 50 */ 1125899906842624;
    if (v > PB) {
        v = 0;
    }
    e.heapStats.nextGC = v;
    e.emitHeapCounters(ts);
}

[GoRecv] internal static void emitHeapCounters(this ref Emitter e, time.Duration ts) {
    if (e.prevHeapStats == e.heapStats) {
        return;
    }
    ref var diff = ref heap<uint64>(out var Ꮡdiff);
    diff = ((uint64)0);
    if (e.heapStats.nextGC > e.heapStats.heapAlloc) {
        diff = e.heapStats.nextGC - e.heapStats.heapAlloc;
    }
    if (e.tsWithinRange(ts)) {
        e.OptionalEvent(Ꮡ(new format.Event(
            Name: "Heap"u8,
            Phase: "C"u8,
            Time: viewerTime(ts),
            PID: 1,
            Arg: Ꮡ(new format.HeapCountersArg(Allocated: e.heapStats.heapAlloc, NextGC: diff))
        )));
    }
    e.prevHeapStats = e.heapStats;
}

// Err returns an error if the emitter is in an invalid state.
[GoRecv] public static error Err(this ref Emitter e) {
    if (e.gstates[GRunnable] < 0 || e.gstates[GRunning] < 0 || e.threadStats[ThreadStateInSyscall] < 0 || e.threadStats[ThreadStateInSyscallRuntime] < 0) {
        return fmt.Errorf(
            "runnable=%d running=%d insyscall=%d insyscallRuntime=%d"u8,
            e.gstates[GRunnable],
            e.gstates[GRunning],
            e.threadStats[ThreadStateInSyscall],
            e.threadStats[ThreadStateInSyscallRuntime]);
    }
    return default!;
}

[GoRecv] internal static bool tsWithinRange(this ref Emitter e, time.Duration ts) {
    return e.rangeStart <= ts && ts <= e.rangeEnd;
}

// OptionalEvent emits ev if it's within the time range of of the consumer, i.e.
// the selected trace split range.
[GoRecv] public static void OptionalEvent(this ref Emitter e, ж<format.Event> Ꮡev) {
    ref var ev = ref Ꮡev.val;

    e.c.ConsumeViewerEvent(ev, false);
}

[GoRecv] public static void Flush(this ref Emitter e) {
    e.processMeta(format.StatsSection, "STATS"u8, 0);
    if (len(e.tasks) != 0) {
        e.processMeta(format.TasksSection, "TASKS"u8, 1);
    }
    foreach (var (id, task) in e.tasks) {
        e.threadMeta(format.TasksSection, id, task.name, task.sortIndex);
    }
    e.processMeta(format.ProcsSection, e.resourceType, 2);
    e.threadMeta(format.ProcsSection, trace.GCP, "GC"u8, -6);
    e.threadMeta(format.ProcsSection, trace.NetpollP, "Network"u8, -5);
    e.threadMeta(format.ProcsSection, trace.TimerP, "Timers"u8, -4);
    e.threadMeta(format.ProcsSection, trace.SyscallP, "Syscalls"u8, -3);
    foreach (var (id, name) in e.resources) {
        nint priority = ((nint)id);
        if (e.focusResource != 0 && id == e.focusResource) {
            // Put the focus goroutine on top.
            priority = -2;
        }
        e.threadMeta(format.ProcsSection, id, name, priority);
    }
    e.c.Flush();
}

[GoRecv] internal static void threadMeta(this ref Emitter e, uint64 sectionID, uint64 tid, @string name, nint priority) {
    e.Event(Ꮡ(new format.Event(
        Name: "thread_name"u8,
        Phase: "M"u8,
        PID: sectionID,
        TID: tid,
        Arg: Ꮡ(new format.NameArg(Name: name))
    )));
    e.Event(Ꮡ(new format.Event(
        Name: "thread_sort_index"u8,
        Phase: "M"u8,
        PID: sectionID,
        TID: tid,
        Arg: Ꮡ(new format.SortIndexArg(Index: priority))
    )));
}

[GoRecv] internal static void processMeta(this ref Emitter e, uint64 sectionID, @string name, nint priority) {
    e.Event(Ꮡ(new format.Event(
        Name: "process_name"u8,
        Phase: "M"u8,
        PID: sectionID,
        Arg: Ꮡ(new format.NameArg(Name: name))
    )));
    e.Event(Ꮡ(new format.Event(
        Name: "process_sort_index"u8,
        Phase: "M"u8,
        PID: sectionID,
        Arg: Ꮡ(new format.SortIndexArg(Index: priority))
    )));
}

// Stack emits the given frames and returns a unique id for the stack. No
// pointers to the given data are being retained beyond the call to Stack.
[GoRecv] public static nint Stack(this ref Emitter e, slice<trace.Frame> stk) {
    return e.buildBranch(e.frameTree, stk);
}

// buildBranch builds one branch in the prefix tree rooted at ctx.frameTree.
[GoRecv] internal static nint buildBranch(this ref Emitter e, frameNode parent, slice<trace.Frame> stk) {
    if (len(stk) == 0) {
        return parent.id;
    }
    nint last = len(stk) - 1;
    var frame = stk[last];
    stk = stk[..(int)(last)];
    var (node, ok) = parent.children[(~frame).PC];
    if (!ok) {
        e.frameSeq++;
        node.id = e.frameSeq;
        node.children = new map<uint64, frameNode>();
        parent.children[(~frame).PC] = node;
        e.c.ConsumeViewerFrame(strconv.Itoa(node.id), new format.Frame(Name: fmt.Sprintf("%v:%v"u8, (~frame).Fn, (~frame).Line), Parent: parent.id));
    }
    return e.buildBranch(node, stk);
}

[GoType] partial struct heapStats {
    internal uint64 heapAlloc;
    internal uint64 nextGC;
}

internal static float64 viewerTime(time.Duration t) {
    return ((float64)t) / ((float64)time.Microsecond);
}

[GoType("num:nint")] partial struct GState;

public static readonly GState GDead = /* iota */ 0;
public static readonly GState GRunnable = 1;
public static readonly GState GRunning = 2;
public static readonly GState GWaiting = 3;
public static readonly GState GWaitingGC = 4;
internal static readonly GState gStateCount = 5;

[GoType("num:nint")] partial struct ThreadState;

public static readonly ThreadState ThreadStateInSyscall = /* iota */ 0;
public static readonly ThreadState ThreadStateInSyscallRuntime = 1;
public static readonly ThreadState ThreadStateRunning = 2;
internal static readonly ThreadState threadStateCount = 3;

[GoType] partial struct frameNode {
    internal nint id;
    internal map<uint64, frameNode> children;
}

// Mapping from more reasonable color names to the reserved color names in
// https://github.com/catapult-project/catapult/blob/master/tracing/tracing/base/color_scheme.html#L50
// The chrome trace viewer allows only those as cname values.
internal static readonly @string colorLightMauve = "thread_state_uninterruptible"u8; // 182, 125, 143

internal static readonly @string colorOrange = "thread_state_iowait"u8; // 255, 140, 0

internal static readonly @string colorSeafoamGreen = "thread_state_running"u8; // 126, 200, 148

internal static readonly @string colorVistaBlue = "thread_state_runnable"u8; // 133, 160, 210

internal static readonly @string colorTan = "thread_state_unknown"u8; // 199, 155, 125

internal static readonly @string colorIrisBlue = "background_memory_dump"u8; // 0, 180, 180

internal static readonly @string colorMidnightBlue = "light_memory_dump"u8; // 0, 0, 180

internal static readonly @string colorDeepMagenta = "detailed_memory_dump"u8; // 180, 0, 180

internal static readonly @string colorBlue = "vsync_highlight_color"u8; // 0, 0, 255

internal static readonly @string colorGrey = "generic_work"u8;     // 125, 125, 125

internal static readonly @string colorGreen = "good"u8;             // 0, 125, 0

internal static readonly @string colorDarkGoldenrod = "bad"u8;              // 180, 125, 0

internal static readonly @string colorPeach = "terrible"u8;         // 180, 0, 0

internal static readonly @string colorBlack = "black"u8;            // 0, 0, 0

internal static readonly @string colorLightGrey = "grey"u8;             // 221, 221, 221

internal static readonly @string colorWhite = "white"u8;            // 255, 255, 255

internal static readonly @string colorYellow = "yellow"u8;           // 255, 255, 0

internal static readonly @string colorOlive = "olive"u8;            // 100, 100, 0

internal static readonly @string colorCornflowerBlue = "rail_response"u8;    // 67, 135, 253

internal static readonly @string colorSunsetOrange = "rail_animation"u8;   // 244, 74, 63

internal static readonly @string colorTangerine = "rail_idle"u8;        // 238, 142, 0

internal static readonly @string colorShamrockGreen = "rail_load"u8;        // 13, 168, 97

internal static readonly @string colorGreenishYellow = "startup"u8;          // 230, 230, 0

internal static readonly @string colorDarkGrey = "heap_dump_stack_frame"u8; // 128, 128, 128

internal static readonly @string colorTawny = "heap_dump_child_node_arrow"u8; // 204, 102, 0

internal static readonly @string colorLemon = "cq_build_running"u8; // 255, 255, 119

internal static readonly @string colorLime = "cq_build_passed"u8;  // 153, 238, 102

internal static readonly @string colorPink = "cq_build_failed"u8;  // 238, 136, 136

internal static readonly @string colorSilver = "cq_build_abandoned"u8; // 187, 187, 187

internal static readonly @string colorManzGreen = "cq_build_attempt_runnig"u8; // 222, 222, 75

internal static readonly @string colorKellyGreen = "cq_build_attempt_passed"u8; // 108, 218, 35

internal static readonly @string colorAnotherGrey = "cq_build_attempt_failed"u8; // 187, 187, 187

internal static slice<@string> colorForTask = new @string[]{
    colorLightMauve,
    colorOrange,
    colorSeafoamGreen,
    colorVistaBlue,
    colorTan,
    colorMidnightBlue,
    colorIrisBlue,
    colorDeepMagenta,
    colorGreen,
    colorDarkGoldenrod,
    colorPeach,
    colorOlive,
    colorCornflowerBlue,
    colorSunsetOrange,
    colorTangerine,
    colorShamrockGreen,
    colorTawny,
    colorLemon,
    colorLime,
    colorPink,
    colorSilver,
    colorManzGreen,
    colorKellyGreen
}.slice();

internal static @string pickTaskColor(uint64 id) {
    var idx = id % ((uint64)len(colorForTask));
    return colorForTask[idx];
}

} // end traceviewer_package
