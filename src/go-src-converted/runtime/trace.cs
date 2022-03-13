// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Go execution tracer.
// The tracer captures a wide range of execution events like goroutine
// creation/blocking/unblocking, syscall enter/exit/block, GC-related events,
// changes of heap size, processor start/stop, etc and writes them to a buffer
// in a compact form. A precise nanosecond-precision timestamp and a stack
// trace is captured for most events.
// See https://golang.org/s/go15trace for more info.

// package runtime -- go2cs converted at 2022 March 13 05:27:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\trace.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;


// Event types in the trace, args are given in square brackets.

using System;
public static partial class runtime_package {

private static readonly nint traceEvNone = 0; // unused
private static readonly nint traceEvBatch = 1; // start of per-P batch of events [pid, timestamp]
private static readonly nint traceEvFrequency = 2; // contains tracer timer frequency [frequency (ticks per second)]
private static readonly nint traceEvStack = 3; // stack [stack id, number of PCs, array of {PC, func string ID, file string ID, line}]
private static readonly nint traceEvGomaxprocs = 4; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack id]
private static readonly nint traceEvProcStart = 5; // start of P [timestamp, thread id]
private static readonly nint traceEvProcStop = 6; // stop of P [timestamp]
private static readonly nint traceEvGCStart = 7; // GC start [timestamp, seq, stack id]
private static readonly nint traceEvGCDone = 8; // GC done [timestamp]
private static readonly nint traceEvGCSTWStart = 9; // GC STW start [timestamp, kind]
private static readonly nint traceEvGCSTWDone = 10; // GC STW done [timestamp]
private static readonly nint traceEvGCSweepStart = 11; // GC sweep start [timestamp, stack id]
private static readonly nint traceEvGCSweepDone = 12; // GC sweep done [timestamp, swept, reclaimed]
private static readonly nint traceEvGoCreate = 13; // goroutine creation [timestamp, new goroutine id, new stack id, stack id]
private static readonly nint traceEvGoStart = 14; // goroutine starts running [timestamp, goroutine id, seq]
private static readonly nint traceEvGoEnd = 15; // goroutine ends [timestamp]
private static readonly nint traceEvGoStop = 16; // goroutine stops (like in select{}) [timestamp, stack]
private static readonly nint traceEvGoSched = 17; // goroutine calls Gosched [timestamp, stack]
private static readonly nint traceEvGoPreempt = 18; // goroutine is preempted [timestamp, stack]
private static readonly nint traceEvGoSleep = 19; // goroutine calls Sleep [timestamp, stack]
private static readonly nint traceEvGoBlock = 20; // goroutine blocks [timestamp, stack]
private static readonly nint traceEvGoUnblock = 21; // goroutine is unblocked [timestamp, goroutine id, seq, stack]
private static readonly nint traceEvGoBlockSend = 22; // goroutine blocks on chan send [timestamp, stack]
private static readonly nint traceEvGoBlockRecv = 23; // goroutine blocks on chan recv [timestamp, stack]
private static readonly nint traceEvGoBlockSelect = 24; // goroutine blocks on select [timestamp, stack]
private static readonly nint traceEvGoBlockSync = 25; // goroutine blocks on Mutex/RWMutex [timestamp, stack]
private static readonly nint traceEvGoBlockCond = 26; // goroutine blocks on Cond [timestamp, stack]
private static readonly nint traceEvGoBlockNet = 27; // goroutine blocks on network [timestamp, stack]
private static readonly nint traceEvGoSysCall = 28; // syscall enter [timestamp, stack]
private static readonly nint traceEvGoSysExit = 29; // syscall exit [timestamp, goroutine id, seq, real timestamp]
private static readonly nint traceEvGoSysBlock = 30; // syscall blocks [timestamp]
private static readonly nint traceEvGoWaiting = 31; // denotes that goroutine is blocked when tracing starts [timestamp, goroutine id]
private static readonly nint traceEvGoInSyscall = 32; // denotes that goroutine is in syscall when tracing starts [timestamp, goroutine id]
private static readonly nint traceEvHeapAlloc = 33; // gcController.heapLive change [timestamp, heap_alloc]
private static readonly nint traceEvHeapGoal = 34; // gcController.heapGoal (formerly next_gc) change [timestamp, heap goal in bytes]
private static readonly nint traceEvTimerGoroutine = 35; // not currently used; previously denoted timer goroutine [timer goroutine id]
private static readonly nint traceEvFutileWakeup = 36; // denotes that the previous wakeup of this goroutine was futile [timestamp]
private static readonly nint traceEvString = 37; // string dictionary entry [ID, length, string]
private static readonly nint traceEvGoStartLocal = 38; // goroutine starts running on the same P as the last event [timestamp, goroutine id]
private static readonly nint traceEvGoUnblockLocal = 39; // goroutine is unblocked on the same P as the last event [timestamp, goroutine id, stack]
private static readonly nint traceEvGoSysExitLocal = 40; // syscall exit on the same P as the last event [timestamp, goroutine id, real timestamp]
private static readonly nint traceEvGoStartLabel = 41; // goroutine starts running with label [timestamp, goroutine id, seq, label string id]
private static readonly nint traceEvGoBlockGC = 42; // goroutine blocks on GC assist [timestamp, stack]
private static readonly nint traceEvGCMarkAssistStart = 43; // GC mark assist start [timestamp, stack]
private static readonly nint traceEvGCMarkAssistDone = 44; // GC mark assist done [timestamp]
private static readonly nint traceEvUserTaskCreate = 45; // trace.NewContext [timestamp, internal task id, internal parent task id, stack, name string]
private static readonly nint traceEvUserTaskEnd = 46; // end of a task [timestamp, internal task id, stack]
private static readonly nint traceEvUserRegion = 47; // trace.WithRegion [timestamp, internal task id, mode(0:start, 1:end), stack, name string]
private static readonly nint traceEvUserLog = 48; // trace.Log [timestamp, internal task id, key string id, stack, value string]
private static readonly nint traceEvCount = 49; 
// Byte is used but only 6 bits are available for event type.
// The remaining 2 bits are used to specify the number of arguments.
// That means, the max event type value is 63.

 
// Timestamps in trace are cputicks/traceTickDiv.
// This makes absolute values of timestamp diffs smaller,
// and so they are encoded in less number of bytes.
// 64 on x86 is somewhat arbitrary (one tick is ~20ns on a 3GHz machine).
// The suggested increment frequency for PowerPC's time base register is
// 512 MHz according to Power ISA v2.07 section 6.2, so we use 16 on ppc64
// and ppc64le.
// Tracing won't work reliably for architectures where cputicks is emulated
// by nanotime, so the value doesn't matter for those architectures.
private static readonly nint traceTickDiv = 16 + 48 * (sys.Goarch386 | sys.GoarchAmd64); 
// Maximum number of PCs in a single stack trace.
// Since events contain only stack id rather than whole stack trace,
// we can allow quite large values here.
private static readonly nint traceStackSize = 128; 
// Identifier of a fake P that is used when we trace without a real P.
private static readonly nint traceGlobProc = -1; 
// Maximum number of bytes to encode uint64 in base-128.
private static readonly nint traceBytesPerNumber = 10; 
// Shift of the number of arguments in the first event byte.
private static readonly nint traceArgCountShift = 6; 
// Flag passed to traceGoPark to denote that the previous wakeup of this
// goroutine was futile. For example, a goroutine was unblocked on a mutex,
// but another goroutine got ahead and acquired the mutex before the first
// goroutine is scheduled, so the first goroutine has to block again.
// Such wakeups happen on buffered channels and sync.Mutex,
// but are generally not interesting for end user.
private static readonly byte traceFutileWakeup = 128;

// trace is global tracing context.
private static var trace = default;

// traceBufHeader is per-P tracing buffer.
private partial struct traceBufHeader {
    public traceBufPtr link; // in trace.empty/full
    public ulong lastTicks; // when we wrote the last event
    public nint pos; // next write offset in arr
    public array<System.UIntPtr> stk; // scratch buffer for traceback
}

// traceBuf is per-P tracing buffer.
//
//go:notinheap
private partial struct traceBuf {
    public ref traceBufHeader traceBufHeader => ref traceBufHeader_val;
    public array<byte> arr; // underlying buffer for traceBufHeader.buf
}

// traceBufPtr is a *traceBuf that is not traced by the garbage
// collector and doesn't have write barriers. traceBufs are not
// allocated from the GC'd heap, so this is safe, and are often
// manipulated in contexts where write barriers are not allowed, so
// this is necessary.
//
// TODO: Since traceBuf is now go:notinheap, this isn't necessary.
private partial struct traceBufPtr { // : System.UIntPtr
}

private static ptr<traceBuf> ptr(this traceBufPtr tp) {
    return _addr_(traceBuf.val)(@unsafe.Pointer(tp))!;
}
private static void set(this ptr<traceBufPtr> _addr_tp, ptr<traceBuf> _addr_b) {
    ref traceBufPtr tp = ref _addr_tp.val;
    ref traceBuf b = ref _addr_b.val;

    tp.val = traceBufPtr(@unsafe.Pointer(b));
}
private static traceBufPtr traceBufPtrOf(ptr<traceBuf> _addr_b) {
    ref traceBuf b = ref _addr_b.val;

    return traceBufPtr(@unsafe.Pointer(b));
}

// StartTrace enables tracing for the current process.
// While tracing, the data will be buffered and available via ReadTrace.
// StartTrace returns an error if tracing is already enabled.
// Most clients should use the runtime/trace package or the testing package's
// -test.trace flag instead of calling StartTrace directly.
public static error StartTrace() { 
    // Stop the world so that we can take a consistent snapshot
    // of all goroutines at the beginning of the trace.
    // Do not stop the world during GC so we ensure we always see
    // a consistent view of GC-related events (e.g. a start is always
    // paired with an end).
    stopTheWorldGC("start tracing"); 

    // Prevent sysmon from running any code that could generate events.
    lock(_addr_sched.sysmonlock); 

    // We are in stop-the-world, but syscalls can finish and write to trace concurrently.
    // Exitsyscall could check trace.enabled long before and then suddenly wake up
    // and decide to write to trace at a random point in time.
    // However, such syscall will use the global trace.buf buffer, because we've
    // acquired all p's by doing stop-the-world. So this protects us from such races.
    lock(_addr_trace.bufLock);

    if (trace.enabled || trace.shutdown) {
        unlock(_addr_trace.bufLock);
        unlock(_addr_sched.sysmonlock);
        startTheWorldGC();
        return error.As(errorString("tracing is already enabled"))!;
    }
    var _g_ = getg();
    _g_.m.startingtrace = true; 

    // Obtain current stack ID to use in all traceEvGoCreate events below.
    var mp = acquirem();
    var stkBuf = make_slice<System.UIntPtr>(traceStackSize);
    var stackID = traceStackID(_addr_mp, stkBuf, 2);
    releasem(mp); 

    // World is stopped, no need to lock.
    forEachGRace(gp => {
        var status = readgstatus(gp);
        if (status != _Gdead) {
            gp.traceseq = 0;
            gp.tracelastp = getg().m.p; 
            // +PCQuantum because traceFrameForPC expects return PCs and subtracts PCQuantum.
            var id = trace.stackTab.put(new slice<System.UIntPtr>(new System.UIntPtr[] { gp.startpc+sys.PCQuantum }));
            traceEvent(traceEvGoCreate, -1, uint64(gp.goid), uint64(id), stackID);
        }
        if (status == _Gwaiting) { 
            // traceEvGoWaiting is implied to have seq=1.
            gp.traceseq++;
            traceEvent(traceEvGoWaiting, -1, uint64(gp.goid));
        }
        if (status == _Gsyscall) {
            gp.traceseq++;
            traceEvent(traceEvGoInSyscall, -1, uint64(gp.goid));
        }
        else
 {
            gp.sysblocktraced = false;
        }
    });
    traceProcStart();
    traceGoStart(); 
    // Note: ticksStart needs to be set after we emit traceEvGoInSyscall events.
    // If we do it the other way around, it is possible that exitsyscall will
    // query sysexitticks after ticksStart but before traceEvGoInSyscall timestamp.
    // It will lead to a false conclusion that cputicks is broken.
    trace.ticksStart = cputicks();
    trace.timeStart = nanotime();
    trace.headerWritten = false;
    trace.footerWritten = false; 

    // string to id mapping
    //  0 : reserved for an empty string
    //  remaining: other strings registered by traceString
    trace.stringSeq = 0;
    trace.strings = make_map<@string, ulong>();

    trace.seqGC = 0;
    _g_.m.startingtrace = false;
    trace.enabled = true; 

    // Register runtime goroutine labels.
    var (_, pid, bufp) = traceAcquireBuffer();
    foreach (var (i, label) in gcMarkWorkerModeStrings[..]) {
        trace.markWorkerLabels[i], bufp = traceString(_addr_bufp, pid, label);
    }    traceReleaseBuffer(pid);

    unlock(_addr_trace.bufLock);

    unlock(_addr_sched.sysmonlock);

    startTheWorldGC();
    return error.As(null!)!;
}

// StopTrace stops tracing, if it was previously enabled.
// StopTrace only returns after all the reads for the trace have completed.
public static void StopTrace() { 
    // Stop the world so that we can collect the trace buffers from all p's below,
    // and also to avoid races with traceEvent.
    stopTheWorldGC("stop tracing"); 

    // See the comment in StartTrace.
    lock(_addr_sched.sysmonlock); 

    // See the comment in StartTrace.
    lock(_addr_trace.bufLock);

    if (!trace.enabled) {
        unlock(_addr_trace.bufLock);
        unlock(_addr_sched.sysmonlock);
        startTheWorldGC();
        return ;
    }
    traceGoSched(); 

    // Loop over all allocated Ps because dead Ps may still have
    // trace buffers.
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp[..(int)cap(allp)]) {
            p = __p;
            var buf = p.tracebuf;
            if (buf != 0) {
                traceFullQueue(buf);
                p.tracebuf = 0;
            }
        }
        p = p__prev1;
    }

    if (trace.buf != 0) {
        buf = trace.buf;
        trace.buf = 0;
        if (buf.ptr().pos != 0) {
            traceFullQueue(buf);
        }
    }
    while (true) {
        trace.ticksEnd = cputicks();
        trace.timeEnd = nanotime(); 
        // Windows time can tick only every 15ms, wait for at least one tick.
        if (trace.timeEnd != trace.timeStart) {
            break;
        }
        osyield();
    }

    trace.enabled = false;
    trace.shutdown = true;
    unlock(_addr_trace.bufLock);

    unlock(_addr_sched.sysmonlock);

    startTheWorldGC(); 

    // The world is started but we've set trace.shutdown, so new tracing can't start.
    // Wait for the trace reader to flush pending buffers and stop.
    semacquire(_addr_trace.shutdownSema);
    if (raceenabled) {
        raceacquire(@unsafe.Pointer(_addr_trace.shutdownSema));
    }
    lock(_addr_trace.@lock);
    {
        var p__prev1 = p;

        foreach (var (_, __p) in allp[..(int)cap(allp)]) {
            p = __p;
            if (p.tracebuf != 0) {
                throw("trace: non-empty trace buffer in proc");
            }
        }
        p = p__prev1;
    }

    if (trace.buf != 0) {
        throw("trace: non-empty global trace buffer");
    }
    if (trace.fullHead != 0 || trace.fullTail != 0) {
        throw("trace: non-empty full trace buffer");
    }
    if (trace.reading != 0 || trace.reader != 0) {
        throw("trace: reading after shutdown");
    }
    while (trace.empty != 0) {
        buf = trace.empty;
        trace.empty = buf.ptr().link;
        sysFree(@unsafe.Pointer(buf), @unsafe.Sizeof(buf.ptr().val), _addr_memstats.other_sys);
    }
    trace.strings = null;
    trace.shutdown = false;
    unlock(_addr_trace.@lock);
}

// ReadTrace returns the next chunk of binary tracing data, blocking until data
// is available. If tracing is turned off and all the data accumulated while it
// was on has been returned, ReadTrace returns nil. The caller must copy the
// returned data before calling ReadTrace again.
// ReadTrace must be called from one goroutine at a time.
public static slice<byte> ReadTrace() { 
    // This function may need to lock trace.lock recursively
    // (goparkunlock -> traceGoPark -> traceEvent -> traceFlush).
    // To allow this we use trace.lockOwner.
    // Also this function must not allocate while holding trace.lock:
    // allocation can call heap allocate, which will try to emit a trace
    // event while holding heap lock.
    lock(_addr_trace.@lock);
    trace.lockOwner = getg();

    if (trace.reader != 0) { 
        // More than one goroutine reads trace. This is bad.
        // But we rather do not crash the program because of tracing,
        // because tracing can be enabled at runtime on prod servers.
        trace.lockOwner = null;
        unlock(_addr_trace.@lock);
        println("runtime: ReadTrace called from multiple goroutines simultaneously");
        return null;
    }
    {
        var buf__prev1 = buf;

        var buf = trace.reading;

        if (buf != 0) {
            buf.ptr().link = trace.empty;
            trace.empty = buf;
            trace.reading = 0;
        }
        buf = buf__prev1;

    } 
    // Write trace header.
    if (!trace.headerWritten) {
        trace.headerWritten = true;
        trace.lockOwner = null;
        unlock(_addr_trace.@lock);
        return (slice<byte>)"go 1.11 trace\x00\x00\x00";
    }
    if (trace.fullHead == 0 && !trace.shutdown) {
        trace.reader.set(getg());
        goparkunlock(_addr_trace.@lock, waitReasonTraceReaderBlocked, traceEvGoBlock, 2);
        lock(_addr_trace.@lock);
    }
    if (trace.fullHead != 0) {
        buf = traceFullDequeue();
        trace.reading = buf;
        trace.lockOwner = null;
        unlock(_addr_trace.@lock);
        return buf.ptr().arr[..(int)buf.ptr().pos];
    }
    if (!trace.footerWritten) {
        trace.footerWritten = true; 
        // Use float64 because (trace.ticksEnd - trace.ticksStart) * 1e9 can overflow int64.
        var freq = float64(trace.ticksEnd - trace.ticksStart) * 1e9F / float64(trace.timeEnd - trace.timeStart) / traceTickDiv;
        trace.lockOwner = null;
        unlock(_addr_trace.@lock);
        slice<byte> data = default;
        data = append(data, traceEvFrequency | 0 << (int)(traceArgCountShift));
        data = traceAppend(data, uint64(freq)); 
        // This will emit a bunch of full buffers, we will pick them up
        // on the next iteration.
        trace.stackTab.dump();
        return data;
    }
    if (trace.shutdown) {
        trace.lockOwner = null;
        unlock(_addr_trace.@lock);
        if (raceenabled) { 
            // Model synchronization on trace.shutdownSema, which race
            // detector does not see. This is required to avoid false
            // race reports on writer passed to trace.Start.
            racerelease(@unsafe.Pointer(_addr_trace.shutdownSema));
        }
        semrelease(_addr_trace.shutdownSema);
        return null;
    }
    trace.lockOwner = null;
    unlock(_addr_trace.@lock);
    println("runtime: spurious wakeup of trace reader");
    return null;
}

// traceReader returns the trace reader that should be woken up, if any.
private static ptr<g> traceReader() {
    if (trace.reader == 0 || (trace.fullHead == 0 && !trace.shutdown)) {
        return _addr_null!;
    }
    lock(_addr_trace.@lock);
    if (trace.reader == 0 || (trace.fullHead == 0 && !trace.shutdown)) {
        unlock(_addr_trace.@lock);
        return _addr_null!;
    }
    var gp = trace.reader.ptr();
    trace.reader.set(null);
    unlock(_addr_trace.@lock);
    return _addr_gp!;
}

// traceProcFree frees trace buffer associated with pp.
private static void traceProcFree(ptr<p> _addr_pp) {
    ref p pp = ref _addr_pp.val;

    var buf = pp.tracebuf;
    pp.tracebuf = 0;
    if (buf == 0) {
        return ;
    }
    lock(_addr_trace.@lock);
    traceFullQueue(buf);
    unlock(_addr_trace.@lock);
}

// traceFullQueue queues buf into queue of full buffers.
private static void traceFullQueue(traceBufPtr buf) {
    buf.ptr().link = 0;
    if (trace.fullHead == 0) {
        trace.fullHead = buf;
    }
    else
 {
        trace.fullTail.ptr().link = buf;
    }
    trace.fullTail = buf;
}

// traceFullDequeue dequeues from queue of full buffers.
private static traceBufPtr traceFullDequeue() {
    var buf = trace.fullHead;
    if (buf == 0) {
        return 0;
    }
    trace.fullHead = buf.ptr().link;
    if (trace.fullHead == 0) {
        trace.fullTail = 0;
    }
    buf.ptr().link = 0;
    return buf;
}

// traceEvent writes a single event to trace buffer, flushing the buffer if necessary.
// ev is event type.
// If skip > 0, write current stack id as the last argument (skipping skip top frames).
// If skip = 0, this event type should contain a stack, but we don't want
// to collect and remember it for this particular call.
private static void traceEvent(byte ev, nint skip, params ulong[] args) {
    args = args.Clone();

    var (mp, pid, bufp) = traceAcquireBuffer(); 
    // Double-check trace.enabled now that we've done m.locks++ and acquired bufLock.
    // This protects from races between traceEvent and StartTrace/StopTrace.

    // The caller checked that trace.enabled == true, but trace.enabled might have been
    // turned off between the check and now. Check again. traceLockBuffer did mp.locks++,
    // StopTrace does stopTheWorld, and stopTheWorld waits for mp.locks to go back to zero,
    // so if we see trace.enabled == true now, we know it's true for the rest of the function.
    // Exitsyscall can run even during stopTheWorld. The race with StartTrace/StopTrace
    // during tracing in exitsyscall is resolved by locking trace.bufLock in traceLockBuffer.
    //
    // Note trace_userTaskCreate runs the same check.
    if (!trace.enabled && !mp.startingtrace) {
        traceReleaseBuffer(pid);
        return ;
    }
    if (skip > 0) {
        if (getg() == mp.curg) {
            skip++; // +1 because stack is captured in traceEventLocked.
        }
    }
    traceEventLocked(0, _addr_mp, pid, _addr_bufp, ev, skip, args);
    traceReleaseBuffer(pid);
}

private static void traceEventLocked(nint extraBytes, ptr<m> _addr_mp, int pid, ptr<traceBufPtr> _addr_bufp, byte ev, nint skip, params ulong[] args) {
    args = args.Clone();
    ref m mp = ref _addr_mp.val;
    ref traceBufPtr bufp = ref _addr_bufp.val;

    var buf = bufp.ptr(); 
    // TODO: test on non-zero extraBytes param.
    nint maxSize = 2 + 5 * traceBytesPerNumber + extraBytes; // event type, length, sequence, timestamp, stack id and two add params
    if (buf == null || len(buf.arr) - buf.pos < maxSize) {
        buf = traceFlush(traceBufPtrOf(_addr_buf), pid).ptr();
        bufp.set(buf);
    }
    var ticks = uint64(cputicks()) / traceTickDiv;
    var tickDiff = ticks - buf.lastTicks;
    buf.lastTicks = ticks;
    var narg = byte(len(args));
    if (skip >= 0) {
        narg++;
    }
    if (narg > 3) {
        narg = 3;
    }
    var startPos = buf.pos;
    buf.@byte(ev | narg << (int)(traceArgCountShift));
    ptr<byte> lenp;
    if (narg == 3) { 
        // Reserve the byte for length assuming that length < 128.
        buf.varint(0);
        lenp = _addr_buf.arr[buf.pos - 1];
    }
    buf.varint(tickDiff);
    foreach (var (_, a) in args) {
        buf.varint(a);
    }    if (skip == 0) {
        buf.varint(0);
    }
    else if (skip > 0) {
        buf.varint(traceStackID(_addr_mp, buf.stk[..], skip));
    }
    var evSize = buf.pos - startPos;
    if (evSize > maxSize) {
        throw("invalid length of trace event");
    }
    if (lenp != null) { 
        // Fill in actual length.
        lenp.val = byte(evSize - 2);
    }
}

private static ulong traceStackID(ptr<m> _addr_mp, slice<System.UIntPtr> buf, nint skip) {
    ref m mp = ref _addr_mp.val;

    var _g_ = getg();
    var gp = mp.curg;
    nint nstk = default;
    if (gp == _g_) {
        nstk = callers(skip + 1, buf);
    }
    else if (gp != null) {
        gp = mp.curg;
        nstk = gcallers(gp, skip, buf);
    }
    if (nstk > 0) {
        nstk--; // skip runtime.goexit
    }
    if (nstk > 0 && gp.goid == 1) {
        nstk--; // skip runtime.main
    }
    var id = trace.stackTab.put(buf[..(int)nstk]);
    return uint64(id);
}

// traceAcquireBuffer returns trace buffer to use and, if necessary, locks it.
private static (ptr<m>, int, ptr<traceBufPtr>) traceAcquireBuffer() {
    ptr<m> mp = default!;
    int pid = default;
    ptr<traceBufPtr> bufp = default!;

    mp = acquirem();
    {
        var p = mp.p.ptr();

        if (p != null) {
            return (_addr_mp!, p.id, _addr__addr_p.tracebuf!);
        }
    }
    lock(_addr_trace.bufLock);
    return (_addr_mp!, traceGlobProc, _addr__addr_trace.buf!);
}

// traceReleaseBuffer releases a buffer previously acquired with traceAcquireBuffer.
private static void traceReleaseBuffer(int pid) {
    if (pid == traceGlobProc) {
        unlock(_addr_trace.bufLock);
    }
    releasem(getg().m);
}

// traceFlush puts buf onto stack of full buffers and returns an empty buffer.
private static traceBufPtr traceFlush(traceBufPtr buf, int pid) {
    var owner = trace.lockOwner;
    var dolock = owner == null || owner != getg().m.curg;
    if (dolock) {
        lock(_addr_trace.@lock);
    }
    if (buf != 0) {
        traceFullQueue(buf);
    }
    if (trace.empty != 0) {
        buf = trace.empty;
        trace.empty = buf.ptr().link;
    }
    else
 {
        buf = traceBufPtr(sysAlloc(@unsafe.Sizeof(new traceBuf()), _addr_memstats.other_sys));
        if (buf == 0) {
            throw("trace: out of memory");
        }
    }
    var bufp = buf.ptr();
    bufp.link.set(null);
    bufp.pos = 0; 

    // initialize the buffer for a new batch
    var ticks = uint64(cputicks()) / traceTickDiv;
    bufp.lastTicks = ticks;
    bufp.@byte(traceEvBatch | 1 << (int)(traceArgCountShift));
    bufp.varint(uint64(pid));
    bufp.varint(ticks);

    if (dolock) {
        unlock(_addr_trace.@lock);
    }
    return buf;
}

// traceString adds a string to the trace.strings and returns the id.
private static (ulong, ptr<traceBufPtr>) traceString(ptr<traceBufPtr> _addr_bufp, int pid, @string s) {
    ulong _p0 = default;
    ptr<traceBufPtr> _p0 = default!;
    ref traceBufPtr bufp = ref _addr_bufp.val;

    if (s == "") {
        return (0, _addr_bufp!);
    }
    lock(_addr_trace.stringsLock);
    if (raceenabled) { 
        // raceacquire is necessary because the map access
        // below is race annotated.
        raceacquire(@unsafe.Pointer(_addr_trace.stringsLock));
    }
    {
        var id__prev1 = id;

        var (id, ok) = trace.strings[s];

        if (ok) {
            if (raceenabled) {
                racerelease(@unsafe.Pointer(_addr_trace.stringsLock));
            }
            unlock(_addr_trace.stringsLock);

            return (id, _addr_bufp!);
        }
        id = id__prev1;

    }

    trace.stringSeq++;
    var id = trace.stringSeq;
    trace.strings[s] = id;

    if (raceenabled) {
        racerelease(@unsafe.Pointer(_addr_trace.stringsLock));
    }
    unlock(_addr_trace.stringsLock); 

    // memory allocation in above may trigger tracing and
    // cause *bufp changes. Following code now works with *bufp,
    // so there must be no memory allocation or any activities
    // that causes tracing after this point.

    var buf = bufp.ptr();
    nint size = 1 + 2 * traceBytesPerNumber + len(s);
    if (buf == null || len(buf.arr) - buf.pos < size) {
        buf = traceFlush(traceBufPtrOf(_addr_buf), pid).ptr();
        bufp.set(buf);
    }
    buf.@byte(traceEvString);
    buf.varint(id); 

    // double-check the string and the length can fit.
    // Otherwise, truncate the string.
    var slen = len(s);
    {
        var room = len(buf.arr) - buf.pos;

        if (room < slen + traceBytesPerNumber) {
            slen = room;
        }
    }

    buf.varint(uint64(slen));
    buf.pos += copy(buf.arr[(int)buf.pos..], s[..(int)slen]);

    bufp.set(buf);
    return (id, _addr_bufp!);
}

// traceAppend appends v to buf in little-endian-base-128 encoding.
private static slice<byte> traceAppend(slice<byte> buf, ulong v) {
    while (v >= 0x80) {
        buf = append(buf, 0x80 | byte(v));
        v>>=7;
    }
    buf = append(buf, byte(v));
    return buf;
}

// varint appends v to buf in little-endian-base-128 encoding.
private static void varint(this ptr<traceBuf> _addr_buf, ulong v) {
    ref traceBuf buf = ref _addr_buf.val;

    var pos = buf.pos;
    while (v >= 0x80) {
        buf.arr[pos] = 0x80 | byte(v);
        pos++;
        v>>=7;
    }
    buf.arr[pos] = byte(v);
    pos++;
    buf.pos = pos;
}

// byte appends v to buf.
private static void @byte(this ptr<traceBuf> _addr_buf, byte v) {
    ref traceBuf buf = ref _addr_buf.val;

    buf.arr[buf.pos] = v;
    buf.pos++;
}

// traceStackTable maps stack traces (arrays of PC's) to unique uint32 ids.
// It is lock-free for reading.
private partial struct traceStackTable {
    public mutex @lock;
    public uint seq;
    public traceAlloc mem;
    public array<traceStackPtr> tab;
}

// traceStack is a single stack in traceStackTable.
private partial struct traceStack {
    public traceStackPtr link;
    public System.UIntPtr hash;
    public uint id;
    public nint n;
    public array<System.UIntPtr> stk; // real type [n]uintptr
}

private partial struct traceStackPtr { // : System.UIntPtr
}

private static ptr<traceStack> ptr(this traceStackPtr tp) {
    return _addr_(traceStack.val)(@unsafe.Pointer(tp))!;
}

// stack returns slice of PCs.
private static slice<System.UIntPtr> stack(this ptr<traceStack> _addr_ts) {
    ref traceStack ts = ref _addr_ts.val;

    return new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(_addr_ts.stk))[..(int)ts.n];
}

// put returns a unique id for the stack trace pcs and caches it in the table,
// if it sees the trace for the first time.
private static uint put(this ptr<traceStackTable> _addr_tab, slice<System.UIntPtr> pcs) {
    ref traceStackTable tab = ref _addr_tab.val;

    if (len(pcs) == 0) {
        return 0;
    }
    var hash = memhash(@unsafe.Pointer(_addr_pcs[0]), 0, uintptr(len(pcs)) * @unsafe.Sizeof(pcs[0])); 
    // First, search the hashtable w/o the mutex.
    {
        var id__prev1 = id;

        var id = tab.find(pcs, hash);

        if (id != 0) {
            return id;
        }
        id = id__prev1;

    } 
    // Now, double check under the mutex.
    lock(_addr_tab.@lock);
    {
        var id__prev1 = id;

        id = tab.find(pcs, hash);

        if (id != 0) {
            unlock(_addr_tab.@lock);
            return id;
        }
        id = id__prev1;

    } 
    // Create new record.
    tab.seq++;
    var stk = tab.newStack(len(pcs));
    stk.hash = hash;
    stk.id = tab.seq;
    stk.n = len(pcs);
    var stkpc = stk.stack();
    foreach (var (i, pc) in pcs) {
        stkpc[i] = pc;
    }    var part = int(hash % uintptr(len(tab.tab)));
    stk.link = tab.tab[part];
    atomicstorep(@unsafe.Pointer(_addr_tab.tab[part]), @unsafe.Pointer(stk));
    unlock(_addr_tab.@lock);
    return stk.id;
}

// find checks if the stack trace pcs is already present in the table.
private static uint find(this ptr<traceStackTable> _addr_tab, slice<System.UIntPtr> pcs, System.UIntPtr hash) {
    ref traceStackTable tab = ref _addr_tab.val;

    var part = int(hash % uintptr(len(tab.tab)));
Search:
    {
        var stk = tab.tab[part].ptr();

        while (stk != null) {
            if (stk.hash == hash && stk.n == len(pcs)) {
                foreach (var (i, stkpc) in stk.stack()) {
                    if (stkpc != pcs[i]) {
                        _continueSearch = true;
                        break;
                    }
            stk = stk.link.ptr();
                }
                return stk.id;
            }
        }
    }
    return 0;
}

// newStack allocates a new stack of size n.
private static ptr<traceStack> newStack(this ptr<traceStackTable> _addr_tab, nint n) {
    ref traceStackTable tab = ref _addr_tab.val;

    return _addr_(traceStack.val)(tab.mem.alloc(@unsafe.Sizeof(new traceStack()) + uintptr(n) * sys.PtrSize))!;
}

// allFrames returns all of the Frames corresponding to pcs.
private static slice<Frame> allFrames(slice<System.UIntPtr> pcs) {
    var frames = make_slice<Frame>(0, len(pcs));
    var ci = CallersFrames(pcs);
    while (true) {
        var (f, more) = ci.Next();
        frames = append(frames, f);
        if (!more) {
            return frames;
        }
    }
}

// dump writes all previously cached stacks to trace buffers,
// releases all memory and resets state.
private static void dump(this ptr<traceStackTable> _addr_tab) {
    ref traceStackTable tab = ref _addr_tab.val;

    array<byte> tmp = new array<byte>((2 + 4 * traceStackSize) * traceBytesPerNumber);
    var bufp = traceFlush(0, 0);
    {
        var stk__prev1 = stk;

        foreach (var (_, __stk) in tab.tab) {
            stk = __stk;
            var stk = stk.ptr();
            while (stk != null) {
                var tmpbuf = tmp[..(int)0];
                tmpbuf = traceAppend(tmpbuf, uint64(stk.id));
                var frames = allFrames(stk.stack());
                tmpbuf = traceAppend(tmpbuf, uint64(len(frames)));
                foreach (var (_, f) in frames) {
                    traceFrame frame = default;
                    frame, bufp = traceFrameForPC(bufp, 0, f);
                    tmpbuf = traceAppend(tmpbuf, uint64(f.PC));
                    tmpbuf = traceAppend(tmpbuf, uint64(frame.funcID));
                    tmpbuf = traceAppend(tmpbuf, uint64(frame.fileID));
                    tmpbuf = traceAppend(tmpbuf, uint64(frame.line));
                } 
                // Now copy to the buffer.
                nint size = 1 + traceBytesPerNumber + len(tmpbuf);
                {
                    var buf__prev1 = buf;

                    var buf = bufp.ptr();

                    if (len(buf.arr) - buf.pos < size) {
                        bufp = traceFlush(bufp, 0);
                stk = stk.link.ptr();
                    }

                    buf = buf__prev1;

                }
                buf = bufp.ptr();
                buf.@byte(traceEvStack | 3 << (int)(traceArgCountShift));
                buf.varint(uint64(len(tmpbuf)));
                buf.pos += copy(buf.arr[(int)buf.pos..], tmpbuf);
            }
        }
        stk = stk__prev1;
    }

    lock(_addr_trace.@lock);
    traceFullQueue(bufp);
    unlock(_addr_trace.@lock);

    tab.mem.drop();
    tab.val = new traceStackTable();
    lockInit(_addr_((tab.val).@lock), lockRankTraceStackTab);
}

private partial struct traceFrame {
    public ulong funcID;
    public ulong fileID;
    public ulong line;
}

// traceFrameForPC records the frame information.
// It may allocate memory.
private static (traceFrame, traceBufPtr) traceFrameForPC(traceBufPtr buf, int pid, Frame f) {
    traceFrame _p0 = default;
    traceBufPtr _p0 = default;

    var bufp = _addr_buf;
    traceFrame frame = default;

    var fn = f.Function;
    const nint maxLen = 1 << 10;

    if (len(fn) > maxLen) {
        fn = fn[(int)len(fn) - maxLen..];
    }
    frame.funcID, bufp = traceString(_addr_bufp, pid, fn);
    frame.line = uint64(f.Line);
    var file = f.File;
    if (len(file) > maxLen) {
        file = file[(int)len(file) - maxLen..];
    }
    frame.fileID, bufp = traceString(_addr_bufp, pid, file);
    return (frame, (bufp.val));
}

// traceAlloc is a non-thread-safe region allocator.
// It holds a linked list of traceAllocBlock.
private partial struct traceAlloc {
    public traceAllocBlockPtr head;
    public System.UIntPtr off;
}

// traceAllocBlock is a block in traceAlloc.
//
// traceAllocBlock is allocated from non-GC'd memory, so it must not
// contain heap pointers. Writes to pointers to traceAllocBlocks do
// not need write barriers.
//
//go:notinheap
private partial struct traceAllocBlock {
    public traceAllocBlockPtr next;
    public array<byte> data;
}

// TODO: Since traceAllocBlock is now go:notinheap, this isn't necessary.
private partial struct traceAllocBlockPtr { // : System.UIntPtr
}

private static ptr<traceAllocBlock> ptr(this traceAllocBlockPtr p) {
    return _addr_(traceAllocBlock.val)(@unsafe.Pointer(p))!;
}
private static void set(this ptr<traceAllocBlockPtr> _addr_p, ptr<traceAllocBlock> _addr_x) {
    ref traceAllocBlockPtr p = ref _addr_p.val;
    ref traceAllocBlock x = ref _addr_x.val;

    p.val = traceAllocBlockPtr(@unsafe.Pointer(x));
}

// alloc allocates n-byte block.
private static unsafe.Pointer alloc(this ptr<traceAlloc> _addr_a, System.UIntPtr n) {
    ref traceAlloc a = ref _addr_a.val;

    n = alignUp(n, sys.PtrSize);
    if (a.head == 0 || a.off + n > uintptr(len(a.head.ptr().data))) {
        if (n > uintptr(len(a.head.ptr().data))) {
            throw("trace: alloc too large");
        }
        var block = (traceAllocBlock.val)(sysAlloc(@unsafe.Sizeof(new traceAllocBlock()), _addr_memstats.other_sys));
        if (block == null) {
            throw("trace: out of memory");
        }
        block.next.set(a.head.ptr());
        a.head.set(block);
        a.off = 0;
    }
    var p = _addr_a.head.ptr().data[a.off];
    a.off += n;
    return @unsafe.Pointer(p);
}

// drop frees all previously allocated memory and resets the allocator.
private static void drop(this ptr<traceAlloc> _addr_a) {
    ref traceAlloc a = ref _addr_a.val;

    while (a.head != 0) {
        var block = a.head.ptr();
        a.head.set(block.next.ptr());
        sysFree(@unsafe.Pointer(block), @unsafe.Sizeof(new traceAllocBlock()), _addr_memstats.other_sys);
    }
}

// The following functions write specific events to trace.

private static void traceGomaxprocs(int procs) {
    traceEvent(traceEvGomaxprocs, 1, uint64(procs));
}

private static void traceProcStart() {
    traceEvent(traceEvProcStart, -1, uint64(getg().m.id));
}

private static void traceProcStop(ptr<p> _addr_pp) {
    ref p pp = ref _addr_pp.val;
 
    // Sysmon and stopTheWorld can stop Ps blocked in syscalls,
    // to handle this we temporary employ the P.
    var mp = acquirem();
    var oldp = mp.p;
    mp.p.set(pp);
    traceEvent(traceEvProcStop, -1);
    mp.p = oldp;
    releasem(mp);
}

private static void traceGCStart() {
    traceEvent(traceEvGCStart, 3, trace.seqGC);
    trace.seqGC++;
}

private static void traceGCDone() {
    traceEvent(traceEvGCDone, -1);
}

private static void traceGCSTWStart(nint kind) {
    traceEvent(traceEvGCSTWStart, -1, uint64(kind));
}

private static void traceGCSTWDone() {
    traceEvent(traceEvGCSTWDone, -1);
}

// traceGCSweepStart prepares to trace a sweep loop. This does not
// emit any events until traceGCSweepSpan is called.
//
// traceGCSweepStart must be paired with traceGCSweepDone and there
// must be no preemption points between these two calls.
private static void traceGCSweepStart() { 
    // Delay the actual GCSweepStart event until the first span
    // sweep. If we don't sweep anything, don't emit any events.
    var _p_ = getg().m.p.ptr();
    if (_p_.traceSweep) {
        throw("double traceGCSweepStart");
    }
    (_p_.traceSweep, _p_.traceSwept, _p_.traceReclaimed) = (true, 0, 0);
}

// traceGCSweepSpan traces the sweep of a single page.
//
// This may be called outside a traceGCSweepStart/traceGCSweepDone
// pair; however, it will not emit any trace events in this case.
private static void traceGCSweepSpan(System.UIntPtr bytesSwept) {
    var _p_ = getg().m.p.ptr();
    if (_p_.traceSweep) {
        if (_p_.traceSwept == 0) {
            traceEvent(traceEvGCSweepStart, 1);
        }
        _p_.traceSwept += bytesSwept;
    }
}

private static void traceGCSweepDone() {
    var _p_ = getg().m.p.ptr();
    if (!_p_.traceSweep) {
        throw("missing traceGCSweepStart");
    }
    if (_p_.traceSwept != 0) {
        traceEvent(traceEvGCSweepDone, -1, uint64(_p_.traceSwept), uint64(_p_.traceReclaimed));
    }
    _p_.traceSweep = false;
}

private static void traceGCMarkAssistStart() {
    traceEvent(traceEvGCMarkAssistStart, 1);
}

private static void traceGCMarkAssistDone() {
    traceEvent(traceEvGCMarkAssistDone, -1);
}

private static void traceGoCreate(ptr<g> _addr_newg, System.UIntPtr pc) {
    ref g newg = ref _addr_newg.val;

    newg.traceseq = 0;
    newg.tracelastp = getg().m.p; 
    // +PCQuantum because traceFrameForPC expects return PCs and subtracts PCQuantum.
    var id = trace.stackTab.put(new slice<System.UIntPtr>(new System.UIntPtr[] { pc+sys.PCQuantum }));
    traceEvent(traceEvGoCreate, 2, uint64(newg.goid), uint64(id));
}

private static void traceGoStart() {
    var _g_ = getg().m.curg;
    var _p_ = _g_.m.p;
    _g_.traceseq++;
    if (_p_.ptr().gcMarkWorkerMode != gcMarkWorkerNotWorker) {
        traceEvent(traceEvGoStartLabel, -1, uint64(_g_.goid), _g_.traceseq, trace.markWorkerLabels[_p_.ptr().gcMarkWorkerMode]);
    }
    else if (_g_.tracelastp == _p_) {
        traceEvent(traceEvGoStartLocal, -1, uint64(_g_.goid));
    }
    else
 {
        _g_.tracelastp = _p_;
        traceEvent(traceEvGoStart, -1, uint64(_g_.goid), _g_.traceseq);
    }
}

private static void traceGoEnd() {
    traceEvent(traceEvGoEnd, -1);
}

private static void traceGoSched() {
    var _g_ = getg();
    _g_.tracelastp = _g_.m.p;
    traceEvent(traceEvGoSched, 1);
}

private static void traceGoPreempt() {
    var _g_ = getg();
    _g_.tracelastp = _g_.m.p;
    traceEvent(traceEvGoPreempt, 1);
}

private static void traceGoPark(byte traceEv, nint skip) {
    if (traceEv & traceFutileWakeup != 0) {
        traceEvent(traceEvFutileWakeup, -1);
    }
    traceEvent(traceEv & ~traceFutileWakeup, skip);
}

private static void traceGoUnpark(ptr<g> _addr_gp, nint skip) {
    ref g gp = ref _addr_gp.val;

    var _p_ = getg().m.p;
    gp.traceseq++;
    if (gp.tracelastp == _p_) {
        traceEvent(traceEvGoUnblockLocal, skip, uint64(gp.goid));
    }
    else
 {
        gp.tracelastp = _p_;
        traceEvent(traceEvGoUnblock, skip, uint64(gp.goid), gp.traceseq);
    }
}

private static void traceGoSysCall() {
    traceEvent(traceEvGoSysCall, 1);
}

private static void traceGoSysExit(long ts) {
    if (ts != 0 && ts < trace.ticksStart) { 
        // There is a race between the code that initializes sysexitticks
        // (in exitsyscall, which runs without a P, and therefore is not
        // stopped with the rest of the world) and the code that initializes
        // a new trace. The recorded sysexitticks must therefore be treated
        // as "best effort". If they are valid for this trace, then great,
        // use them for greater accuracy. But if they're not valid for this
        // trace, assume that the trace was started after the actual syscall
        // exit (but before we actually managed to start the goroutine,
        // aka right now), and assign a fresh time stamp to keep the log consistent.
        ts = 0;
    }
    var _g_ = getg().m.curg;
    _g_.traceseq++;
    _g_.tracelastp = _g_.m.p;
    traceEvent(traceEvGoSysExit, -1, uint64(_g_.goid), _g_.traceseq, uint64(ts) / traceTickDiv);
}

private static void traceGoSysBlock(ptr<p> _addr_pp) {
    ref p pp = ref _addr_pp.val;
 
    // Sysmon and stopTheWorld can declare syscalls running on remote Ps as blocked,
    // to handle this we temporary employ the P.
    var mp = acquirem();
    var oldp = mp.p;
    mp.p.set(pp);
    traceEvent(traceEvGoSysBlock, -1);
    mp.p = oldp;
    releasem(mp);
}

private static void traceHeapAlloc() {
    traceEvent(traceEvHeapAlloc, -1, gcController.heapLive);
}

private static void traceHeapGoal() {
    {
        var heapGoal = atomic.Load64(_addr_gcController.heapGoal);

        if (heapGoal == ~uint64(0)) { 
            // Heap-based triggering is disabled.
            traceEvent(traceEvHeapGoal, -1, 0);
        }
        else
 {
            traceEvent(traceEvHeapGoal, -1, heapGoal);
        }
    }
}

// To access runtime functions from runtime/trace.
// See runtime/trace/annotation.go

//go:linkname trace_userTaskCreate runtime/trace.userTaskCreate
private static void trace_userTaskCreate(ulong id, ulong parentID, @string taskType) {
    if (!trace.enabled) {
        return ;
    }
    var (mp, pid, bufp) = traceAcquireBuffer();
    if (!trace.enabled && !mp.startingtrace) {
        traceReleaseBuffer(pid);
        return ;
    }
    var (typeStringID, bufp) = traceString(_addr_bufp, pid, taskType);
    traceEventLocked(0, _addr_mp, pid, _addr_bufp, traceEvUserTaskCreate, 3, id, parentID, typeStringID);
    traceReleaseBuffer(pid);
}

//go:linkname trace_userTaskEnd runtime/trace.userTaskEnd
private static void trace_userTaskEnd(ulong id) {
    traceEvent(traceEvUserTaskEnd, 2, id);
}

//go:linkname trace_userRegion runtime/trace.userRegion
private static void trace_userRegion(ulong id, ulong mode, @string name) {
    if (!trace.enabled) {
        return ;
    }
    var (mp, pid, bufp) = traceAcquireBuffer();
    if (!trace.enabled && !mp.startingtrace) {
        traceReleaseBuffer(pid);
        return ;
    }
    var (nameStringID, bufp) = traceString(_addr_bufp, pid, name);
    traceEventLocked(0, _addr_mp, pid, _addr_bufp, traceEvUserRegion, 3, id, mode, nameStringID);
    traceReleaseBuffer(pid);
}

//go:linkname trace_userLog runtime/trace.userLog
private static void trace_userLog(ulong id, @string category, @string message) {
    if (!trace.enabled) {
        return ;
    }
    var (mp, pid, bufp) = traceAcquireBuffer();
    if (!trace.enabled && !mp.startingtrace) {
        traceReleaseBuffer(pid);
        return ;
    }
    var (categoryID, bufp) = traceString(_addr_bufp, pid, category);

    var extraSpace = traceBytesPerNumber + len(message); // extraSpace for the value string
    traceEventLocked(extraSpace, _addr_mp, pid, _addr_bufp, traceEvUserLog, 3, id, categoryID); 
    // traceEventLocked reserved extra space for val and len(val)
    // in buf, so buf now has room for the following.
    var buf = bufp.ptr(); 

    // double-check the message and its length can fit.
    // Otherwise, truncate the message.
    var slen = len(message);
    {
        var room = len(buf.arr) - buf.pos;

        if (room < slen + traceBytesPerNumber) {
            slen = room;
        }
    }
    buf.varint(uint64(slen));
    buf.pos += copy(buf.arr[(int)buf.pos..], message[..(int)slen]);

    traceReleaseBuffer(pid);
}

} // end runtime_package
