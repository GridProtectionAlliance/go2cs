// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace buffer management.
namespace go;

using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using runtime.@internal;

partial class runtime_package {

// Maximum number of bytes required to encode uint64 in base-128.
internal static readonly UntypedInt traceBytesPerNumber = 10;

// traceWriter is the interface for writing all trace data.
//
// This type is passed around as a value, and all of its methods return
// a new traceWriter. This allows for chaining together calls in a fluent-style
// API. This is partly stylistic, and very slightly for performance, since
// the compiler can destructure this value and pass it between calls as
// just regular arguments. However, this style is not load-bearing, and
// we can change it if it's deemed too error-prone.
[GoType] partial struct traceWriter {
    internal partial ref traceLocker traceLocker { get; }
    public partial ref ж<traceBuf> traceBuf { get; }
}

// write returns an a traceWriter that writes into the current M's stream.
internal static traceWriter writer(this traceLocker tl) {
    return new traceWriter(traceLocker: tl, traceBuf: tl.mp.trace.buf[tl.gen % 2]);
}

// unsafeTraceWriter produces a traceWriter that doesn't lock the trace.
//
// It should only be used in contexts where either:
// - Another traceLocker is held.
// - trace.gen is prevented from advancing.
//
// buf may be nil.
internal static traceWriter unsafeTraceWriter(uintptr gen, ж<traceBuf> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.val;

    return new traceWriter(traceLocker: new traceLocker(gen: gen), traceBuf: buf);
}

// end writes the buffer back into the m.
internal static void end(this traceWriter w) {
    if (w.mp == nil) {
        // Tolerate a nil mp. It makes code that creates traceWriters directly
        // less error-prone.
        return;
    }
    w.mp.trace.buf[w.gen % 2] = w.traceBuf;
}

// ensure makes sure that at least maxSize bytes are available to write.
//
// Returns whether the buffer was flushed.
internal static (traceWriter, bool) ensure(this traceWriter w, nint maxSize) {
    var refill = w.traceBuf == nil || !w.available(maxSize);
    if (refill) {
        w = w.refill(traceNoExperiment);
    }
    return (w, refill);
}

// flush puts w.traceBuf on the queue of full buffers.
internal static traceWriter flush(this traceWriter w) {
    systemstack(
    var traceʗ2 = Δtrace;
    var wʗ2 = w;
    () => {
        @lock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
        if (wʗ2.traceBuf != nil) {
            traceBufFlush(wʗ2.traceBuf, wʗ2.gen);
        }
        unlock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
    });
    w.traceBuf = default!;
    return w;
}

// refill puts w.traceBuf on the queue of full buffers and refresh's w's buffer.
//
// exp indicates whether the refilled batch should be EvExperimentalBatch.
internal static traceWriter refill(this traceWriter w, traceExperiment exp) {
    systemstack(
    var memstatsʗ2 = memstats;
    var traceʗ2 = Δtrace;
    var wʗ2 = w;
    () => {
        @lock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
        if (wʗ2.traceBuf != nil) {
            traceBufFlush(wʗ2.traceBuf, wʗ2.gen);
        }
        if (traceʗ2.empty != nil){
            wʗ2.traceBuf = traceʗ2.empty;
            traceʗ2.empty = wʗ2.traceBuf.link;
            unlock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
        } else {
            unlock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
            wʗ2.traceBuf = (ж<traceBuf>)(uintptr)(sysAlloc(@unsafe.Sizeof(new traceBuf(nil)), Ꮡmemstatsʗ2.of(mstats.Ꮡother_sys)));
            if (wʗ2.traceBuf == nil) {
                @throw("trace: out of memory"u8);
            }
        }
    });
    // Initialize the buffer.
    var ts = traceClockNow();
    if (ts <= w.traceBuf.lastTime) {
        ts = w.traceBuf.lastTime + 1;
    }
    w.traceBuf.lastTime = ts;
    w.traceBuf.link = default!;
    w.traceBuf.pos = 0;
    // Tolerate a nil mp.
    var mID = ~((uint64)0);
    if (w.mp != nil) {
        mID = ((uint64)w.mp.procid);
    }
    // Write the buffer's header.
    if (exp == traceNoExperiment){
        w.@byte(((byte)traceEvEventBatch));
    } else {
        w.@byte(((byte)traceEvExperimentalBatch));
        w.@byte(((byte)exp));
    }
    w.varint(((uint64)w.gen));
    w.varint(((uint64)mID));
    w.varint(((uint64)ts));
    w.traceBuf.lenPos = w.varintReserve();
    return w;
}

// traceBufQueue is a FIFO of traceBufs.
[GoType] partial struct traceBufQueue {
    internal ж<traceBuf> head;
    internal ж<traceBuf> tail;
}

// push queues buf into queue of buffers.
[GoRecv] internal static void push(this ref traceBufQueue q, ж<traceBuf> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.val;

    buf.link = default!;
    if (q.head == nil){
        q.head = buf;
    } else {
        q.tail.link = buf;
    }
    q.tail = buf;
}

// pop dequeues from the queue of buffers.
[GoRecv] internal static ж<traceBuf> pop(this ref traceBufQueue q) {
    var buf = q.head;
    if (buf == nil) {
        return default!;
    }
    q.head = buf.link;
    if (q.head == nil) {
        q.tail = default!;
    }
    buf.link = default!;
    return buf;
}

[GoRecv] internal static bool empty(this ref traceBufQueue q) {
    return q.head == nil;
}

// traceBufHeader is per-P tracing buffer.
[GoType] partial struct traceBufHeader {
    internal ж<traceBuf> link; // in trace.empty/full
    internal traceTime lastTime; // when we wrote the last event
    internal nint pos;      // next write offset in arr
    internal nint lenPos;      // position of batch length value
}

// traceBuf is per-M tracing buffer.
//
// TODO(mknyszek): Rename traceBuf to traceBatch, since they map 1:1 with event batches.
[GoType] partial struct traceBuf {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal partial ref traceBufHeader traceBufHeader { get; }
    internal array<byte> arr = new(64 << (int)(10) - @unsafe.Sizeof(new traceBufHeader(nil))); // underlying buffer for traceBufHeader.buf
}

// byte appends v to buf.
[GoRecv] internal static void @byte(this ref traceBuf buf, byte v) {
    buf.arr[buf.pos] = v;
    buf.pos++;
}

// varint appends v to buf in little-endian-base-128 encoding.
[GoRecv] internal static void varint(this ref traceBuf buf, uint64 v) {
    nint pos = buf.pos;
    var arr = buf.arr[(int)(pos)..(int)(pos + traceBytesPerNumber)];
    foreach (var (i, _) in arr) {
        if (v < 128) {
            pos += i + 1;
            arr[i] = ((byte)v);
            break;
        }
        arr[i] = (byte)(128 | ((byte)v));
        v >>= (UntypedInt)(7);
    }
    buf.pos = pos;
}

// varintReserve reserves enough space in buf to hold any varint.
//
// Space reserved this way can be filled in with the varintAt method.
[GoRecv] internal static nint varintReserve(this ref traceBuf buf) {
    nint Δp = buf.pos;
    buf.pos += traceBytesPerNumber;
    return Δp;
}

// stringData appends s's data directly to buf.
[GoRecv] internal static void stringData(this ref traceBuf buf, @string s) {
    buf.pos += copy(buf.arr[(int)(buf.pos)..], s);
}

[GoRecv] internal static bool available(this ref traceBuf buf, nint size) {
    return len(buf.arr) - buf.pos >= size;
}

// varintAt writes varint v at byte position pos in buf. This always
// consumes traceBytesPerNumber bytes. This is intended for when the caller
// needs to reserve space for a varint but can't populate it until later.
// Use varintReserve to reserve this space.
[GoRecv] internal static void varintAt(this ref traceBuf buf, nint pos, uint64 v) {
    for (nint i = 0; i < traceBytesPerNumber; i++) {
        if (i < traceBytesPerNumber - 1){
            buf.arr[pos] = (byte)(128 | ((byte)v));
        } else {
            buf.arr[pos] = ((byte)v);
        }
        v >>= (UntypedInt)(7);
        pos++;
    }
    if (v != 0) {
        @throw("v could not fit in traceBytesPerNumber"u8);
    }
}

// traceBufFlush flushes a trace buffer.
//
// Must run on the system stack because trace.lock must be held.
//
//go:systemstack
internal static void traceBufFlush(ж<traceBuf> Ꮡbuf, uintptr gen) {
    ref var buf = ref Ꮡbuf.val;

    assertLockHeld(ᏑΔtrace.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
    // Write out the non-header length of the batch in the header.
    //
    // Note: the length of the header is not included to make it easier
    // to calculate this value when deserializing and reserializing the
    // trace. Varints can have additional padding of zero bits that is
    // quite difficult to preserve, and if we include the header we
    // force serializers to do more work. Nothing else actually needs
    // padding.
    buf.varintAt(buf.lenPos, ((uint64)(buf.pos - (buf.lenPos + traceBytesPerNumber))));
    Δtrace.full[gen % 2].push(Ꮡbuf);
    // Notify the scheduler that there's work available and that the trace
    // reader should be scheduled.
    if (!Δtrace.workAvailable.Load()) {
        Δtrace.workAvailable.Store(true);
    }
}

} // end runtime_package
