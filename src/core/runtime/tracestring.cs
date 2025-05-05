// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace string management.
namespace go;

partial class runtime_package {

// Trace strings.
internal static readonly UntypedInt maxTraceStringLen = 1024;

// traceStringTable is map of string -> unique ID that also manages
// writing strings out into the trace.
[GoType] partial struct traceStringTable {
    // lock protects buf.
    internal mutex @lock;
    internal ж<traceBuf> buf; // string batches to write out to the trace.
    // tab is a mapping of string -> unique ID.
    internal traceMap tab;
}

// put adds a string to the table, emits it, and returns a unique ID for it.
[GoRecv] internal static uint64 put(this ref traceStringTable t, uintptr gen, @string s) {
    // Put the string in the table.
    var ss = stringStructOf(Ꮡ(s));
    var (id, added) = t.tab.put((~ss).str, ((uintptr)(~ss).len));
    if (added) {
        // Write the string to the buffer.
        systemstack(() => {
            t.writeString(gen, id, s);
        });
    }
    return id;
}

// emit emits a string and creates an ID for it, but doesn't add it to the table. Returns the ID.
[GoRecv] internal static uint64 emit(this ref traceStringTable t, uintptr gen, @string s) {
    // Grab an ID and write the string to the buffer.
    var id = t.tab.stealID();
    systemstack(() => {
        t.writeString(gen, id, s);
    });
    return id;
}

// writeString writes the string to t.buf.
//
// Must run on the systemstack because it acquires t.lock.
//
//go:systemstack
[GoRecv] internal static void writeString(this ref traceStringTable t, uintptr gen, uint64 id, @string s) {
    // Truncate the string if necessary.
    if (len(s) > maxTraceStringLen) {
        s = s[..(int)(maxTraceStringLen)];
    }
    @lock(Ꮡ(t.@lock));
    var w = unsafeTraceWriter(gen, t.buf);
    // Ensure we have a place to write to.
    bool flushed = default!;
    (w, flushed) = w.ensure(2 + 2 * traceBytesPerNumber + len(s));
    /* traceEvStrings + traceEvString + ID + len + string data */
    if (flushed) {
        // Annotate the batch as containing strings.
        w.@byte(((byte)traceEvStrings));
    }
    // Write out the string.
    w.@byte(((byte)traceEvString));
    w.varint(id);
    w.varint(((uint64)len(s)));
    w.stringData(s);
    // Store back buf in case it was updated during ensure.
    t.buf = w.traceBuf;
    unlock(Ꮡ(t.@lock));
}

// reset clears the string table and flushes any buffers it has.
//
// Must be called only once the caller is certain nothing else will be
// added to this table.
[GoRecv] internal static void reset(this ref traceStringTable t, uintptr gen) {
    if (t.buf != nil) {
        systemstack(
        var traceʗ2 = Δtrace;
        () => {
            @lock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
            traceBufFlush(t.buf, gen);
            unlock(Ꮡtraceʗ2.of(atomic.Int32; seqGC uint64; minPageHeapAddr uint64; debugMalloc bool}.Ꮡlock));
        });
        t.buf = default!;
    }
    // Reset the table.
    t.tab.reset();
}

} // end runtime_package
