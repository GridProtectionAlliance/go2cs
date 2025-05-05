// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace.@event;

using fmt = fmt_package;
using @event = @internal.trace.event_package;
using @internal.trace;

partial class go122_package {

public static readonly @event.Type EvNone = /* iota */ 0; // unused
public static readonly @event.Type EvEventBatch = 1; // start of per-M batch of events [generation, M ID, timestamp, batch length]
public static readonly @event.Type EvStacks = 2; // start of a section of the stack table [...EvStack]
public static readonly @event.Type EvStack = 3; // stack table entry [ID, ...{PC, func string ID, file string ID, line #}]
public static readonly @event.Type EvStrings = 4; // start of a section of the string dictionary [...EvString]
public static readonly @event.Type EvString = 5; // string dictionary entry [ID, length, string]
public static readonly @event.Type EvCPUSamples = 6; // start of a section of CPU samples [...EvCPUSample]
public static readonly @event.Type EvCPUSample = 7; // CPU profiling sample [timestamp, M ID, P ID, goroutine ID, stack ID]
public static readonly @event.Type EvFrequency = 8; // timestamp units per sec [freq]
public static readonly @event.Type EvProcsChange = 9; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack ID]
public static readonly @event.Type EvProcStart = 10; // start of P [timestamp, P ID, P seq]
public static readonly @event.Type EvProcStop = 11; // stop of P [timestamp]
public static readonly @event.Type EvProcSteal = 12; // P was stolen [timestamp, P ID, P seq, M ID]
public static readonly @event.Type EvProcStatus = 13; // P status at the start of a generation [timestamp, P ID, status]
public static readonly @event.Type EvGoCreate = 14; // goroutine creation [timestamp, new goroutine ID, new stack ID, stack ID]
public static readonly @event.Type EvGoCreateSyscall = 15; // goroutine appears in syscall (cgo callback) [timestamp, new goroutine ID]
public static readonly @event.Type EvGoStart = 16; // goroutine starts running [timestamp, goroutine ID, goroutine seq]
public static readonly @event.Type EvGoDestroy = 17; // goroutine ends [timestamp]
public static readonly @event.Type EvGoDestroySyscall = 18; // goroutine ends in syscall (cgo callback) [timestamp]
public static readonly @event.Type EvGoStop = 19; // goroutine yields its time, but is runnable [timestamp, reason, stack ID]
public static readonly @event.Type EvGoBlock = 20; // goroutine blocks [timestamp, reason, stack ID]
public static readonly @event.Type EvGoUnblock = 21; // goroutine is unblocked [timestamp, goroutine ID, goroutine seq, stack ID]
public static readonly @event.Type EvGoSyscallBegin = 22; // syscall enter [timestamp, P seq, stack ID]
public static readonly @event.Type EvGoSyscallEnd = 23; // syscall exit [timestamp]
public static readonly @event.Type EvGoSyscallEndBlocked = 24; // syscall exit and it blocked at some point [timestamp]
public static readonly @event.Type EvGoStatus = 25; // goroutine status at the start of a generation [timestamp, goroutine ID, thread ID, status]
public static readonly @event.Type EvSTWBegin = 26; // STW start [timestamp, kind]
public static readonly @event.Type EvSTWEnd = 27; // STW done [timestamp]
public static readonly @event.Type EvGCActive = 28; // GC active [timestamp, seq]
public static readonly @event.Type EvGCBegin = 29; // GC start [timestamp, seq, stack ID]
public static readonly @event.Type EvGCEnd = 30; // GC done [timestamp, seq]
public static readonly @event.Type EvGCSweepActive = 31; // GC sweep active [timestamp, P ID]
public static readonly @event.Type EvGCSweepBegin = 32; // GC sweep start [timestamp, stack ID]
public static readonly @event.Type EvGCSweepEnd = 33; // GC sweep done [timestamp, swept bytes, reclaimed bytes]
public static readonly @event.Type EvGCMarkAssistActive = 34; // GC mark assist active [timestamp, goroutine ID]
public static readonly @event.Type EvGCMarkAssistBegin = 35; // GC mark assist start [timestamp, stack ID]
public static readonly @event.Type EvGCMarkAssistEnd = 36; // GC mark assist done [timestamp]
public static readonly @event.Type EvHeapAlloc = 37; // gcController.heapLive change [timestamp, heap alloc in bytes]
public static readonly @event.Type EvHeapGoal = 38; // gcController.heapGoal() change [timestamp, heap goal in bytes]
public static readonly @event.Type EvGoLabel = 39; // apply string label to current running goroutine [timestamp, label string ID]
public static readonly @event.Type EvUserTaskBegin = 40; // trace.NewTask [timestamp, internal task ID, internal parent task ID, name string ID, stack ID]
public static readonly @event.Type EvUserTaskEnd = 41; // end of a task [timestamp, internal task ID, stack ID]
public static readonly @event.Type EvUserRegionBegin = 42; // trace.{Start,With}Region [timestamp, internal task ID, name string ID, stack ID]
public static readonly @event.Type EvUserRegionEnd = 43; // trace.{End,With}Region [timestamp, internal task ID, name string ID, stack ID]
public static readonly @event.Type EvUserLog = 44; // trace.Log [timestamp, internal task ID, key string ID, value string ID, stack]
public static readonly @event.Type EvGoSwitch = 45; // goroutine switch (coroswitch) [timestamp, goroutine ID, goroutine seq]
public static readonly @event.Type EvGoSwitchDestroy = 46; // goroutine switch and destroy [timestamp, goroutine ID, goroutine seq]
public static readonly @event.Type EvGoCreateBlocked = 47; // goroutine creation (starts blocked) [timestamp, new goroutine ID, new stack ID, stack ID]
public static readonly @event.Type EvGoStatusStack = 48; // goroutine status at the start of a generation, with a stack [timestamp, goroutine ID, M ID, status, stack ID]
public static readonly @event.Type EvExperimentalBatch = 49; // start of extra data [experiment ID, generation, M ID, timestamp, batch length, batch data...]

// Experiments.
public static readonly @event.Experiment AllocFree = /* 1 + iota */ 1;

// Experimental events.
internal static readonly @event.Type _ = /* 127 + iota */ 127;
// Experimental events for AllocFree.

public static readonly @event.Type EvSpan = 128; // heap span exists [timestamp, id, npages, type/class]

public static readonly @event.Type EvSpanAlloc = 129; // heap span alloc [timestamp, id, npages, type/class]

public static readonly @event.Type EvSpanFree = 130; // heap span free [timestamp, id]

public static readonly @event.Type EvHeapObject = 131; // heap object exists [timestamp, id, type]

public static readonly @event.Type EvHeapObjectAlloc = 132; // heap object alloc [timestamp, id, type]

public static readonly @event.Type EvHeapObjectFree = 133; // heap object free [timestamp, id]

public static readonly @event.Type EvGoroutineStack = 134; // stack exists [timestamp, id, order]

public static readonly @event.Type EvGoroutineStackAlloc = 135; // stack alloc [timestamp, id, order]

public static readonly @event.Type EvGoroutineStackFree = 136; // stack free [timestamp, id]

// EventString returns the name of a Go 1.22 event.
public static @string EventString(@event.Type typ) {
    if (((nint)typ) < len(specs)) {
        return specs[typ].Name;
    }
    return fmt.Sprintf("Invalid(%d)"u8, typ);
}

public static slice<@event.Spec> Specs() {
    return specs[..];
}

// "Structural" Events.
// N.B. There's clearly a timestamp here, but these Events
// are special in that they don't appear in the regular
// M streams.
// Easier to represent for raw readers.
// "Timed" Events.
// Experimental events.
internal static array<@event.Spec> specs = new runtime.SparseArray<@event.Spec>{
    [EvEventBatch] = new @event.Spec(
        Name: "EventBatch"u8,
        Args: new @string[]{"gen", "m", "time", "size"}.slice()
    ),
    [EvStacks] = new @event.Spec(
        Name: "Stacks"u8
    ),
    [EvStack] = new @event.Spec(
        Name: "Stack"u8,
        Args: new @string[]{"id", "nframes"}.slice(),
        IsStack: true
    ),
    [EvStrings] = new @event.Spec(
        Name: "Strings"u8
    ),
    [EvString] = new @event.Spec(
        Name: "String"u8,
        Args: new @string[]{"id"}.slice(),
        HasData: true
    ),
    [EvCPUSamples] = new @event.Spec(
        Name: "CPUSamples"u8
    ),
    [EvCPUSample] = new @event.Spec(
        Name: "CPUSample"u8,
        Args: new @string[]{"time", "m", "p", "g", "stack"}.slice()
    ),
    [EvFrequency] = new @event.Spec(
        Name: "Frequency"u8,
        Args: new @string[]{"freq"}.slice()
    ),
    [EvExperimentalBatch] = new @event.Spec(
        Name: "ExperimentalBatch"u8,
        Args: new @string[]{"exp", "gen", "m", "time"}.slice(),
        HasData: true
    ),
    [EvProcsChange] = new @event.Spec(
        Name: "ProcsChange"u8,
        Args: new @string[]{"dt", "procs_value", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{2}.slice()
    ),
    [EvProcStart] = new @event.Spec(
        Name: "ProcStart"u8,
        Args: new @string[]{"dt", "p", "p_seq"}.slice(),
        IsTimedEvent: true
    ),
    [EvProcStop] = new @event.Spec(
        Name: "ProcStop"u8,
        Args: new @string[]{"dt"}.slice(),
        IsTimedEvent: true
    ),
    [EvProcSteal] = new @event.Spec(
        Name: "ProcSteal"u8,
        Args: new @string[]{"dt", "p", "p_seq", "m"}.slice(),
        IsTimedEvent: true
    ),
    [EvProcStatus] = new @event.Spec(
        Name: "ProcStatus"u8,
        Args: new @string[]{"dt", "p", "pstatus"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoCreate] = new @event.Spec(
        Name: "GoCreate"u8,
        Args: new @string[]{"dt", "new_g", "new_stack", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{3, 2}.slice()
    ),
    [EvGoCreateSyscall] = new @event.Spec(
        Name: "GoCreateSyscall"u8,
        Args: new @string[]{"dt", "new_g"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoStart] = new @event.Spec(
        Name: "GoStart"u8,
        Args: new @string[]{"dt", "g", "g_seq"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoDestroy] = new @event.Spec(
        Name: "GoDestroy"u8,
        Args: new @string[]{"dt"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoDestroySyscall] = new @event.Spec(
        Name: "GoDestroySyscall"u8,
        Args: new @string[]{"dt"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoStop] = new @event.Spec(
        Name: "GoStop"u8,
        Args: new @string[]{"dt", "reason_string", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{2}.slice(),
        StringIDs: new nint[]{1}.slice()
    ),
    [EvGoBlock] = new @event.Spec(
        Name: "GoBlock"u8,
        Args: new @string[]{"dt", "reason_string", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{2}.slice(),
        StringIDs: new nint[]{1}.slice()
    ),
    [EvGoUnblock] = new @event.Spec(
        Name: "GoUnblock"u8,
        Args: new @string[]{"dt", "g", "g_seq", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{3}.slice()
    ),
    [EvGoSyscallBegin] = new @event.Spec(
        Name: "GoSyscallBegin"u8,
        Args: new @string[]{"dt", "p_seq", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{2}.slice()
    ),
    [EvGoSyscallEnd] = new @event.Spec(
        Name: "GoSyscallEnd"u8,
        Args: new @string[]{"dt"}.slice(),
        StartEv: EvGoSyscallBegin,
        IsTimedEvent: true
    ),
    [EvGoSyscallEndBlocked] = new @event.Spec(
        Name: "GoSyscallEndBlocked"u8,
        Args: new @string[]{"dt"}.slice(),
        StartEv: EvGoSyscallBegin,
        IsTimedEvent: true
    ),
    [EvGoStatus] = new @event.Spec(
        Name: "GoStatus"u8,
        Args: new @string[]{"dt", "g", "m", "gstatus"}.slice(),
        IsTimedEvent: true
    ),
    [EvSTWBegin] = new @event.Spec(
        Name: "STWBegin"u8,
        Args: new @string[]{"dt", "kind_string", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{2}.slice(),
        StringIDs: new nint[]{1}.slice()
    ),
    [EvSTWEnd] = new @event.Spec(
        Name: "STWEnd"u8,
        Args: new @string[]{"dt"}.slice(),
        StartEv: EvSTWBegin,
        IsTimedEvent: true
    ),
    [EvGCActive] = new @event.Spec(
        Name: "GCActive"u8,
        Args: new @string[]{"dt", "gc_seq"}.slice(),
        IsTimedEvent: true,
        StartEv: EvGCBegin
    ),
    [EvGCBegin] = new @event.Spec(
        Name: "GCBegin"u8,
        Args: new @string[]{"dt", "gc_seq", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{2}.slice()
    ),
    [EvGCEnd] = new @event.Spec(
        Name: "GCEnd"u8,
        Args: new @string[]{"dt", "gc_seq"}.slice(),
        StartEv: EvGCBegin,
        IsTimedEvent: true
    ),
    [EvGCSweepActive] = new @event.Spec(
        Name: "GCSweepActive"u8,
        Args: new @string[]{"dt", "p"}.slice(),
        StartEv: EvGCSweepBegin,
        IsTimedEvent: true
    ),
    [EvGCSweepBegin] = new @event.Spec(
        Name: "GCSweepBegin"u8,
        Args: new @string[]{"dt", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{1}.slice()
    ),
    [EvGCSweepEnd] = new @event.Spec(
        Name: "GCSweepEnd"u8,
        Args: new @string[]{"dt", "swept_value", "reclaimed_value"}.slice(),
        StartEv: EvGCSweepBegin,
        IsTimedEvent: true
    ),
    [EvGCMarkAssistActive] = new @event.Spec(
        Name: "GCMarkAssistActive"u8,
        Args: new @string[]{"dt", "g"}.slice(),
        StartEv: EvGCMarkAssistBegin,
        IsTimedEvent: true
    ),
    [EvGCMarkAssistBegin] = new @event.Spec(
        Name: "GCMarkAssistBegin"u8,
        Args: new @string[]{"dt", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{1}.slice()
    ),
    [EvGCMarkAssistEnd] = new @event.Spec(
        Name: "GCMarkAssistEnd"u8,
        Args: new @string[]{"dt"}.slice(),
        StartEv: EvGCMarkAssistBegin,
        IsTimedEvent: true
    ),
    [EvHeapAlloc] = new @event.Spec(
        Name: "HeapAlloc"u8,
        Args: new @string[]{"dt", "heapalloc_value"}.slice(),
        IsTimedEvent: true
    ),
    [EvHeapGoal] = new @event.Spec(
        Name: "HeapGoal"u8,
        Args: new @string[]{"dt", "heapgoal_value"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoLabel] = new @event.Spec(
        Name: "GoLabel"u8,
        Args: new @string[]{"dt", "label_string"}.slice(),
        IsTimedEvent: true,
        StringIDs: new nint[]{1}.slice()
    ),
    [EvUserTaskBegin] = new @event.Spec(
        Name: "UserTaskBegin"u8,
        Args: new @string[]{"dt", "task", "parent_task", "name_string", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{4}.slice(),
        StringIDs: new nint[]{3}.slice()
    ),
    [EvUserTaskEnd] = new @event.Spec(
        Name: "UserTaskEnd"u8,
        Args: new @string[]{"dt", "task", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{2}.slice()
    ),
    [EvUserRegionBegin] = new @event.Spec(
        Name: "UserRegionBegin"u8,
        Args: new @string[]{"dt", "task", "name_string", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{3}.slice(),
        StringIDs: new nint[]{2}.slice()
    ),
    [EvUserRegionEnd] = new @event.Spec(
        Name: "UserRegionEnd"u8,
        Args: new @string[]{"dt", "task", "name_string", "stack"}.slice(),
        StartEv: EvUserRegionBegin,
        IsTimedEvent: true,
        StackIDs: new nint[]{3}.slice(),
        StringIDs: new nint[]{2}.slice()
    ),
    [EvUserLog] = new @event.Spec(
        Name: "UserLog"u8,
        Args: new @string[]{"dt", "task", "key_string", "value_string", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{4}.slice(),
        StringIDs: new nint[]{2, 3}.slice()
    ),
    [EvGoSwitch] = new @event.Spec(
        Name: "GoSwitch"u8,
        Args: new @string[]{"dt", "g", "g_seq"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoSwitchDestroy] = new @event.Spec(
        Name: "GoSwitchDestroy"u8,
        Args: new @string[]{"dt", "g", "g_seq"}.slice(),
        IsTimedEvent: true
    ),
    [EvGoCreateBlocked] = new @event.Spec(
        Name: "GoCreateBlocked"u8,
        Args: new @string[]{"dt", "new_g", "new_stack", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{3, 2}.slice()
    ),
    [EvGoStatusStack] = new @event.Spec(
        Name: "GoStatusStack"u8,
        Args: new @string[]{"dt", "g", "m", "gstatus", "stack"}.slice(),
        IsTimedEvent: true,
        StackIDs: new nint[]{4}.slice()
    ),
    [EvSpan] = new @event.Spec(
        Name: "Span"u8,
        Args: new @string[]{"dt", "id", "npages_value", "kindclass"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvSpanAlloc] = new @event.Spec(
        Name: "SpanAlloc"u8,
        Args: new @string[]{"dt", "id", "npages_value", "kindclass"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvSpanFree] = new @event.Spec(
        Name: "SpanFree"u8,
        Args: new @string[]{"dt", "id"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvHeapObject] = new @event.Spec(
        Name: "HeapObject"u8,
        Args: new @string[]{"dt", "id", "type"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvHeapObjectAlloc] = new @event.Spec(
        Name: "HeapObjectAlloc"u8,
        Args: new @string[]{"dt", "id", "type"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvHeapObjectFree] = new @event.Spec(
        Name: "HeapObjectFree"u8,
        Args: new @string[]{"dt", "id"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvGoroutineStack] = new @event.Spec(
        Name: "GoroutineStack"u8,
        Args: new @string[]{"dt", "id", "order"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvGoroutineStackAlloc] = new @event.Spec(
        Name: "GoroutineStackAlloc"u8,
        Args: new @string[]{"dt", "id", "order"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvGoroutineStackFree] = new @event.Spec(
        Name: "GoroutineStackFree"u8,
        Args: new @string[]{"dt", "id"}.slice(),
        IsTimedEvent: true,
        Experiment: AllocFree
    )
}.array();

[GoType("num:uint8")] partial struct GoStatus;

public static readonly GoStatus GoBad = /* iota */ 0;
public static readonly GoStatus GoRunnable = 1;
public static readonly GoStatus GoRunning = 2;
public static readonly GoStatus GoSyscall = 3;
public static readonly GoStatus GoWaiting = 4;

public static @string String(this GoStatus s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == GoRunnable) {
        return "Runnable"u8;
    }
    if (exprᴛ1 == GoRunning) {
        return "Running"u8;
    }
    if (exprᴛ1 == GoSyscall) {
        return "Syscall"u8;
    }
    if (exprᴛ1 == GoWaiting) {
        return "Waiting"u8;
    }

    return "Bad"u8;
}

[GoType("num:uint8")] partial struct ProcStatus;

public static readonly ProcStatus ProcBad = /* iota */ 0;
public static readonly ProcStatus ProcRunning = 1;
public static readonly ProcStatus ProcIdle = 2;
public static readonly ProcStatus ProcSyscall = 3;
public static readonly ProcStatus ProcSyscallAbandoned = 4;

public static @string String(this ProcStatus s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == ProcRunning) {
        return "Running"u8;
    }
    if (exprᴛ1 == ProcIdle) {
        return "Idle"u8;
    }
    if (exprᴛ1 == ProcSyscall) {
        return "Syscall"u8;
    }

    return "Bad"u8;
}

public static readonly UntypedInt MaxBatchSize = /* 64 << 10 */ 65536;
public static readonly UntypedInt MaxFramesPerStack = 128;
public static readonly UntypedInt MaxStringSize = /* 1 << 10 */ 1024;

} // end go122_package
