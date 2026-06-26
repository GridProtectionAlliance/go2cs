// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class trace_package {

// Frame is a frame in stack traces.
[GoType] partial struct Frame {
    public uint64 PC;
    public @string Fn;
    public @string File;
    public nint Line;
}

public static readonly UntypedInt FakeP = /* 1000000 + iota */ 1000000;
public static readonly UntypedInt TimerP = 1000001; // depicts timer unblocks
public static readonly UntypedInt NetpollP = 1000002; // depicts network unblocks
public static readonly UntypedInt SyscallP = 1000003; // depicts returns from syscalls
public static readonly UntypedInt GCP = 1000004; // depicts GC state
public static readonly UntypedInt ProfileP = 1000005; // depicts recording of CPU profile samples

// Event types in the trace.
// Verbatim copy from src/runtime/trace.go with the "trace" prefix removed.
public static readonly UntypedInt EvNone = 0; // unused

public static readonly UntypedInt EvBatch = 1; // start of per-P batch of events [pid, timestamp]

public static readonly UntypedInt EvFrequency = 2; // contains tracer timer frequency [frequency (ticks per second)]

public static readonly UntypedInt EvStack = 3; // stack [stack id, number of PCs, array of {PC, func string ID, file string ID, line}]

public static readonly UntypedInt EvGomaxprocs = 4; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack id]

public static readonly UntypedInt EvProcStart = 5; // start of P [timestamp, thread id]

public static readonly UntypedInt EvProcStop = 6; // stop of P [timestamp]

public static readonly UntypedInt EvGCStart = 7; // GC start [timestamp, seq, stack id]

public static readonly UntypedInt EvGCDone = 8; // GC done [timestamp]

public static readonly UntypedInt EvSTWStart = 9; // GC mark termination start [timestamp, kind]

public static readonly UntypedInt EvSTWDone = 10; // GC mark termination done [timestamp]

public static readonly UntypedInt EvGCSweepStart = 11; // GC sweep start [timestamp, stack id]

public static readonly UntypedInt EvGCSweepDone = 12; // GC sweep done [timestamp, swept, reclaimed]

public static readonly UntypedInt EvGoCreate = 13; // goroutine creation [timestamp, new goroutine id, new stack id, stack id]

public static readonly UntypedInt EvGoStart = 14; // goroutine starts running [timestamp, goroutine id, seq]

public static readonly UntypedInt EvGoEnd = 15; // goroutine ends [timestamp]

public static readonly UntypedInt EvGoStop = 16; // goroutine stops (like in select{}) [timestamp, stack]

public static readonly UntypedInt EvGoSched = 17; // goroutine calls Gosched [timestamp, stack]

public static readonly UntypedInt EvGoPreempt = 18; // goroutine is preempted [timestamp, stack]

public static readonly UntypedInt EvGoSleep = 19; // goroutine calls Sleep [timestamp, stack]

public static readonly UntypedInt EvGoBlock = 20; // goroutine blocks [timestamp, stack]

public static readonly UntypedInt EvGoUnblock = 21; // goroutine is unblocked [timestamp, goroutine id, seq, stack]

public static readonly UntypedInt EvGoBlockSend = 22; // goroutine blocks on chan send [timestamp, stack]

public static readonly UntypedInt EvGoBlockRecv = 23; // goroutine blocks on chan recv [timestamp, stack]

public static readonly UntypedInt EvGoBlockSelect = 24; // goroutine blocks on select [timestamp, stack]

public static readonly UntypedInt EvGoBlockSync = 25; // goroutine blocks on Mutex/RWMutex [timestamp, stack]

public static readonly UntypedInt EvGoBlockCond = 26; // goroutine blocks on Cond [timestamp, stack]

public static readonly UntypedInt EvGoBlockNet = 27; // goroutine blocks on network [timestamp, stack]

public static readonly UntypedInt EvGoSysCall = 28; // syscall enter [timestamp, stack]

public static readonly UntypedInt EvGoSysExit = 29; // syscall exit [timestamp, goroutine id, seq, real timestamp]

public static readonly UntypedInt EvGoSysBlock = 30; // syscall blocks [timestamp]

public static readonly UntypedInt EvGoWaiting = 31; // denotes that goroutine is blocked when tracing starts [timestamp, goroutine id]

public static readonly UntypedInt EvGoInSyscall = 32; // denotes that goroutine is in syscall when tracing starts [timestamp, goroutine id]

public static readonly UntypedInt EvHeapAlloc = 33; // gcController.heapLive change [timestamp, heap live bytes]

public static readonly UntypedInt EvHeapGoal = 34; // gcController.heapGoal change [timestamp, heap goal bytes]

public static readonly UntypedInt EvTimerGoroutine = 35; // denotes timer goroutine [timer goroutine id]

public static readonly UntypedInt EvFutileWakeup = 36; // denotes that the previous wakeup of this goroutine was futile [timestamp]

public static readonly UntypedInt EvString = 37; // string dictionary entry [ID, length, string]

public static readonly UntypedInt EvGoStartLocal = 38; // goroutine starts running on the same P as the last event [timestamp, goroutine id]

public static readonly UntypedInt EvGoUnblockLocal = 39; // goroutine is unblocked on the same P as the last event [timestamp, goroutine id, stack]

public static readonly UntypedInt EvGoSysExitLocal = 40; // syscall exit on the same P as the last event [timestamp, goroutine id, real timestamp]

public static readonly UntypedInt EvGoStartLabel = 41; // goroutine starts running with label [timestamp, goroutine id, seq, label string id]

public static readonly UntypedInt EvGoBlockGC = 42; // goroutine blocks on GC assist [timestamp, stack]

public static readonly UntypedInt EvGCMarkAssistStart = 43; // GC mark assist start [timestamp, stack]

public static readonly UntypedInt EvGCMarkAssistDone = 44; // GC mark assist done [timestamp]

public static readonly UntypedInt EvUserTaskCreate = 45; // trace.NewTask [timestamp, internal task id, internal parent id, name string, stack]

public static readonly UntypedInt EvUserTaskEnd = 46; // end of task [timestamp, internal task id, stack]

public static readonly UntypedInt EvUserRegion = 47; // trace.WithRegion [timestamp, internal task id, mode(0:start, 1:end), name string, stack]

public static readonly UntypedInt EvUserLog = 48; // trace.Log [timestamp, internal id, key string id, stack, value string]

public static readonly UntypedInt EvCPUSample = 49; // CPU profiling sample [timestamp, real timestamp, real P id (-1 when absent), goroutine id, stack]

public static readonly UntypedInt EvCount = 50;

} // end trace_package
