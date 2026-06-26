// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// traceExpWriter is a wrapper around trace writer that produces traceEvExperimentalBatch
// batches. This means that the data written to the writer need not conform to the standard
// trace format.
[GoType] partial struct traceExpWriter {
    internal partial ref traceWriter traceWriter { get; }
    internal traceExperiment exp;
}

// unsafeTraceExpWriter produces a traceExpWriter that doesn't lock the trace.
//
// It should only be used in contexts where either:
// - Another traceLocker is held.
// - trace.gen is prevented from advancing.
//
// buf may be nil.
internal static traceExpWriter unsafeTraceExpWriter(uintptr gen, ж<traceBuf> Ꮡbuf, traceExperiment exp) {
    ref var buf = ref Ꮡbuf.val;

    return new traceExpWriter(new traceWriter(traceLocker: new traceLocker(gen: gen), traceBuf: buf), exp);
}

// ensure makes sure that at least maxSize bytes are available to write.
//
// Returns whether the buffer was flushed.
internal static (traceExpWriter, bool) ensure(this traceExpWriter w, nint maxSize) {
    var refill = w.traceBuf == nil || !w.available(maxSize);
    if (refill) {
        w.traceWriter = w.traceWriter.refill(w.exp);
    }
    return (w, refill);
}

[GoType("num:uint8")] partial struct traceExperiment;

internal static readonly traceExperiment traceNoExperiment = /* iota */ 0;
internal static readonly traceExperiment traceExperimentAllocFree = 1;

// Experimental events.
internal static readonly traceEv _ᴛ2ʗ = /* 127 + iota */ 127;
// Experimental events for ExperimentAllocFree.

internal static readonly traceEv traceEvSpan = 128; // heap span exists [timestamp, id, npages, type/class]

internal static readonly traceEv traceEvSpanAlloc = 129; // heap span alloc [timestamp, id, npages, type/class]

internal static readonly traceEv traceEvSpanFree = 130; // heap span free [timestamp, id]

internal static readonly traceEv traceEvHeapObject = 131; // heap object exists [timestamp, id, type]

internal static readonly traceEv traceEvHeapObjectAlloc = 132; // heap object alloc [timestamp, id, type]

internal static readonly traceEv traceEvHeapObjectFree = 133; // heap object free [timestamp, id]

internal static readonly traceEv traceEvGoroutineStack = 134; // stack exists [timestamp, id, order]

internal static readonly traceEv traceEvGoroutineStackAlloc = 135; // stack alloc [timestamp, id, order]

internal static readonly traceEv traceEvGoroutineStackFree = 136; // stack free [timestamp, id]

} // end runtime_package
