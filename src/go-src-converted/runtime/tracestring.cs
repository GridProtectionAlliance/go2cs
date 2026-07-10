// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace string management.
namespace go;

using @unsafe = unsafe_package;

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
internal static uint64 put(this ж<traceStringTable> Ꮡt, uintptr gen, @string s) {
    ref var t = ref Ꮡt.Value;

    // Put the string in the table.
    var ss = stringStructOf(Ꮡ(s));
    var (id, added) = Ꮡt.of(traceStringTable.Ꮡtab).put((~ss).str, (uintptr)(~ss).len);
    if (added) {
        // Write the string to the buffer.
        systemstack(() => {
            Ꮡt.writeString(gen, id, s);
        });
    }
    return id;
}

// emit emits a string and creates an ID for it, but doesn't add it to the table. Returns the ID.
internal static uint64 emit(this ж<traceStringTable> Ꮡt, uintptr gen, @string s) {
    ref var t = ref Ꮡt.Value;

    // Grab an ID and write the string to the buffer.
    var id = Ꮡt.of(traceStringTable.Ꮡtab).stealID();
    systemstack(() => {
        Ꮡt.writeString(gen, id, s);
    });
    return id;
}

// writeString writes the string to t.buf.
//
// Must run on the systemstack because it acquires t.lock.
//
//go:systemstack
internal static void writeString(this ж<traceStringTable> Ꮡt, uintptr gen, uint64 id, @string s) {
    ref var t = ref Ꮡt.Value;

    // Truncate the string if necessary.
    if (len(s) > maxTraceStringLen) {
        s = s[..(int)(maxTraceStringLen)];
    }
    @lock(Ꮡt.of(traceStringTable.Ꮡlock));
    var w = unsafeTraceWriter(gen, t.buf);
    // Ensure we have a place to write to.
    bool flushed = default!;
    (w, flushed) = w.ensure((nint)(2 + 2 * traceBytesPerNumber) + len(s));
    /* traceEvStrings + traceEvString + ID + len + string data */
    if (flushed) {
        // Annotate the batch as containing strings.
        w.@byte((byte)traceEvStrings);
    }
    // Write out the string.
    w.@byte((byte)traceEvString);
    w.varint(id);
    w.varint((uint64)len(s));
    w.stringData(s);
    // Store back buf in case it was updated during ensure.
    t.buf = w.traceBuf;
    unlock(Ꮡt.of(traceStringTable.Ꮡlock));
}

// reset clears the string table and flushes any buffers it has.
//
// Must be called only once the caller is certain nothing else will be
// added to this table.
internal static void reset(this ж<traceStringTable> Ꮡt, uintptr gen) {
    ref var t = ref Ꮡt.Value;

    if (t.buf != nil) {
        systemstack(() => {
            @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            traceBufFlush(Ꮡt.Value.buf, gen);
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        });
        t.buf = default!;
    }
    // Reset the table.
    Ꮡt.of(traceStringTable.Ꮡtab).reset();
}

} // end runtime_package
