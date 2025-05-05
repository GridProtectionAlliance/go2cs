// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Runtime -> tracer API for memory events.
namespace go;

using abi = @internal.abi_package;
using sys = runtime.@internal.sys_package;
using @internal;
using runtime.@internal;

partial class runtime_package {

// Batch type values for the alloc/free experiment.
internal static readonly UntypedInt traceAllocFreeTypesBatch = iota; // Contains types. [{id, address, size, ptrspan, name length, name string} ...]

internal static readonly UntypedInt traceAllocFreeInfoBatch = 1; // Contains info for interpreting events. [min heap addr, page size, min heap align, min stack align]

// traceSnapshotMemory takes a snapshot of all runtime memory that there are events for
// (heap spans, heap objects, goroutine stacks, etc.) and writes out events for them.
//
// The world must be stopped and tracing must be enabled when this function is called.
internal static void traceSnapshotMemory(uintptr gen) {
    assertWorldStopped();
    // Write a batch containing information that'll be necessary to
    // interpret the events.
    bool flushed = default!;
    var w = unsafeTraceExpWriter(gen, nil, traceExperimentAllocFree);
    (w, flushed) = w.ensure(1 + 4 * traceBytesPerNumber);
    if (flushed) {
        // Annotate the batch as containing additional info.
        w.@byte(((byte)traceAllocFreeInfoBatch));
    }
    // Emit info.
    w.varint(((uint64)Δtrace.minPageHeapAddr));
    w.varint(((uint64)pageSize));
    w.varint(((uint64)minHeapAlign));
    w.varint(((uint64)fixedStack));
    // Finish writing the batch.
    w.flush().end();
    // Start tracing.
    ref var trace = ref heap<traceLocker>(out var Ꮡtrace);
    Δtrace = traceAcquire();
    if (!Δtrace.ok()) {
        @throw("traceSnapshotMemory: tracing is not enabled"u8);
    }
    // Write out all the heap spans and heap objects.
    foreach (var (_, s) in mheap_.allspans) {
        if ((~s).state.get() == mSpanDead) {
            continue;
        }
        // It's some kind of span, so trace that it exists.
        Δtrace.SpanExists(s);
        // Write out allocated objects if it's a heap span.
        if ((~s).state.get() != mSpanInUse) {
            continue;
        }
        // Find all allocated objects.
        var abits = s.allocBitsForIndex(0);
        for (var i = ((uintptr)0); i < ((uintptr)(~s).nelems); i++) {
            if (abits.index < ((uintptr)(~s).freeindex) || abits.isMarked()) {
                ref var x = ref heap<uintptr>(out var Ꮡx);
                x = s.@base() + i * (~s).elemsize;
                Δtrace.HeapObjectExists(x, s.typePointersOfUnchecked(x).typ);
            }
            abits.advance();
        }
    }
    // Write out all the goroutine stacks.
    forEachGRace(
    var traceʗ2 = Δtrace;
    (ж<g> gp) => {
        traceʗ2.GoroutineStackExists((~gp).stack.lo, (~gp).stack.hi - (~gp).stack.lo);
    });
    traceRelease(Δtrace);
}

internal static traceArg traceSpanTypeAndClass(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    if (s.state.get() == mSpanInUse) {
        return ((traceArg)s.spanclass) << (int)(1);
    }
    return ((traceArg)1);
}

// SpanExists records an event indicating that the span exists.
internal static void SpanExists(this traceLocker tl, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvSpan, traceSpanID(Ꮡs), ((traceArg)s.npages), traceSpanTypeAndClass(Ꮡs));
}

// SpanAlloc records an event indicating that the span has just been allocated.
internal static void SpanAlloc(this traceLocker tl, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvSpanAlloc, traceSpanID(Ꮡs), ((traceArg)s.npages), traceSpanTypeAndClass(Ꮡs));
}

// SpanFree records an event indicating that the span is about to be freed.
internal static void SpanFree(this traceLocker tl, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvSpanFree, traceSpanID(Ꮡs));
}

// traceSpanID creates a trace ID for the span s for the trace.
internal static traceArg traceSpanID(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.val;

    return ((traceArg)(((uint64)s.@base()) - Δtrace.minPageHeapAddr)) / pageSize;
}

// HeapObjectExists records that an object already exists at addr with the provided type.
// The type is optional, and the size of the slot occupied the object is inferred from the
// span containing it.
internal static void HeapObjectExists(this traceLocker tl, uintptr addr, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvHeapObject, traceHeapObjectID(addr), tl.rtype(Ꮡtyp));
}

// HeapObjectAlloc records that an object was newly allocated at addr with the provided type.
// The type is optional, and the size of the slot occupied the object is inferred from the
// span containing it.
internal static void HeapObjectAlloc(this traceLocker tl, uintptr addr, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvHeapObjectAlloc, traceHeapObjectID(addr), tl.rtype(Ꮡtyp));
}

// HeapObjectFree records that an object at addr is about to be freed.
internal static void HeapObjectFree(this traceLocker tl, uintptr addr) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvHeapObjectFree, traceHeapObjectID(addr));
}

// traceHeapObjectID creates a trace ID for a heap object at address addr.
internal static traceArg traceHeapObjectID(uintptr addr) {
    return ((traceArg)(((uint64)addr) - Δtrace.minPageHeapAddr)) / minHeapAlign;
}

// GoroutineStackExists records that a goroutine stack already exists at address base with the provided size.
internal static void GoroutineStackExists(this traceLocker tl, uintptr @base, uintptr size) {
    var order = traceCompressStackSize(size);
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGoroutineStack, traceGoroutineStackID(@base), order);
}

// GoroutineStackAlloc records that a goroutine stack was newly allocated at address base with the provided size..
internal static void GoroutineStackAlloc(this traceLocker tl, uintptr @base, uintptr size) {
    var order = traceCompressStackSize(size);
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGoroutineStackAlloc, traceGoroutineStackID(@base), order);
}

// GoroutineStackFree records that a goroutine stack at address base is about to be freed.
internal static void GoroutineStackFree(this traceLocker tl, uintptr @base) {
    tl.eventWriter(traceGoRunning, traceProcRunning).commit(traceEvGoroutineStackFree, traceGoroutineStackID(@base));
}

// traceGoroutineStackID creates a trace ID for the goroutine stack from its base address.
internal static traceArg traceGoroutineStackID(uintptr @base) {
    return ((traceArg)(((uint64)@base) - Δtrace.minPageHeapAddr)) / fixedStack;
}

// traceCompressStackSize assumes size is a power of 2 and returns log2(size).
internal static traceArg traceCompressStackSize(uintptr size) {
    if ((uintptr)(size & (size - 1)) != 0) {
        @throw("goroutine stack size is not a power of 2"u8);
    }
    return ((traceArg)sys.Len64(((uint64)size)));
}

} // end runtime_package
