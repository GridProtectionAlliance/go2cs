// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;
using go122 = @internal.trace.@event.go122_package;
using oldtrace = @internal.trace.@internal.oldtrace_package;
using version = @internal.trace.version_package;
using @internal.trace;
using @internal.trace.@event;
using @internal.trace.@internal;

partial class trace_package {

// Reader reads a byte stream, validates it, and produces trace events.
[GoType] partial struct Reader {
    internal ж<bufio_package.Reader> r;
    internal ΔTime lastTs;
    internal ж<generation> gen;
    internal ж<spilledBatch> spill;
    internal error spillErr; // error from reading spill
    internal slice<ж<batchCursor>> frontier;
    internal slice<cpuSample> cpuSamples;
    internal ordering order;
    internal bool emittedSync;
    internal ж<oldTraceConverter> go121Events;
}

// NewReader creates a new trace reader.
public static (ж<Reader>, error) NewReader(io.Reader r) {
    var br = bufio.NewReader(r);
    var (v, err) = version.ReadHeader(~br);
    if (err != default!) {
        return (default!, err);
    }
    var exprᴛ1 = v;
    if (exprᴛ1 == version.Go111 || exprᴛ1 == version.Go119 || exprᴛ1 == version.Go121) {
        var (tr, errΔ2) = oldtrace.Parse(~br, v);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        return (Ꮡ(new Reader(
            go121Events: convertOldFormat(tr)
        )), default!);
    }
    if (exprᴛ1 == version.Go122 || exprᴛ1 == version.Go123) {
        return (Ꮡ(new Reader(
            r: br,
            order: new ordering(
                mStates: new trace.mState(),
                pStates: new trace.pState(),
                gStates: new trace.gState(),
                activeTasks: new trace.taskState()
            ), // Don't emit a sync event when we first go to emit events.

            emittedSync: true
        )), default!);
    }
    { /* default: */
        return (default!, fmt.Errorf("unknown or unsupported version go 1.%d"u8, v));
    }

}

// ReadEvent reads a single event from the stream.
//
// If the stream has been exhausted, it returns an invalid
// event and io.EOF.
[GoRecv] public static (ΔEvent e, error err) ReadEvent(this ref Reader r) => func((defer, _) => {
    ΔEvent e = default!;
    error errΔ1 = default!;

    if (r.go121Events != nil) {
        var (evΔ1, errΔ2) = r.go121Events.next();
        if (errΔ2 != default!) {
            // XXX do we have to emit an EventSync when the trace is done?
            return (new ΔEvent(nil), errΔ2);
        }
        return (evΔ1, default!);
    }
    // Go 1.22+ trace parsing algorithm.
    //
    // (1) Read in all the batches for the next generation from the stream.
    //   (a) Use the size field in the header to quickly find all batches.
    // (2) Parse out the strings, stacks, CPU samples, and timestamp conversion data.
    // (3) Group each event batch by M, sorted by timestamp. (batchCursor contains the groups.)
    // (4) Organize batchCursors in a min-heap, ordered by the timestamp of the next event for each M.
    // (5) Try to advance the next event for the M at the top of the min-heap.
    //   (a) On success, select that M.
    //   (b) On failure, sort the min-heap and try to advance other Ms. Select the first M that advances.
    //   (c) If there's nothing left to advance, goto (1).
    // (6) Select the latest event for the selected M and get it ready to be returned.
    // (7) Read the next event for the selected M and update the min-heap.
    // (8) Return the selected event, goto (5) on the next call.
    // Set us up to track the last timestamp and fix up
    // the timestamp of any event that comes through.
    var eʗ1 = e;
    defer(() => {
        if (errΔ1 != default!) {
            return (eʗ1, errΔ1);
        }
        {
            errΔ1 = eʗ1.validateTableIDs(); if (errΔ1 != default!) {
                return (eʗ1, errΔ1);
            }
        }
        if (eʗ1.@base.time <= r.lastTs) {
            eʗ1.@base.time = r.lastTs + 1;
        }
        r.lastTs = eʗ1.@base.time;
    });
    // Consume any events in the ordering first.
    {
        var (evΔ2, okΔ1) = r.order.Next(); if (okΔ1) {
            return (evΔ2, default!);
        }
    }
    // Check if we need to refresh the generation.
    if (len(r.frontier) == 0 && len(r.cpuSamples) == 0) {
        if (!r.emittedSync) {
            r.emittedSync = true;
            return (syncEvent(r.gen.evTable, r.lastTs), default!);
        }
        if (r.spillErr != default!) {
            return (new ΔEvent(nil), r.spillErr);
        }
        if (r.gen != nil && r.spill == nil) {
            // If we have a generation from the last read,
            // and there's nothing left in the frontier, and
            // there's no spilled batch, indicating that there's
            // no further generation, it means we're done.
            // Return io.EOF.
            return (new ΔEvent(nil), io.EOF);
        }
        // Read the next generation.
        error err = default!;
        (r.gen, r.spill, errΔ1) = readGeneration(r.r, r.spill);
        if (r.gen == nil) {
            return (new ΔEvent(nil), err);
        }
        r.spillErr = err;
        // Reset CPU samples cursor.
        r.cpuSamples = r.gen.cpuSamples;
        // Reset frontier.
        ref var m = ref heap(new ThreadID(), out var Ꮡm);

        foreach (var (m, batches) in r.gen.batches) {
            var bc = Ꮡ(new batchCursor(m: m));
            var (okΔ2, errΔ3) = bc.nextEvent(batches, r.gen.freq);
            if (errΔ3 != default!) {
                return (new ΔEvent(nil), errΔ3);
            }
            if (!okΔ2) {
                // Turns out there aren't actually any events in these batches.
                continue;
            }
            r.frontier = heapInsert(r.frontier, bc);
        }
        // Reset emittedSync.
        r.emittedSync = false;
    }
    var tryAdvance = 
    (nint i) => {
        var bc = r.frontier[i];
        {
            var (okΔ3, errΔ4) = r.order.Advance(Ꮡ((~bc).ev), r.gen.evTable, (~bc).m, r.gen.gen); if (!okΔ3 || errΔ4 != default!) {
                return (okΔ3, errΔ4);
            }
        }
        // Refresh the cursor's event.
        var (okΔ4, err) = bc.nextEvent(r.gen.batches[(~bc).m], r.gen.freq);
        if (err != default!) {
            return (false, err);
        }
        if (okΔ4){
            // If we successfully refreshed, update the heap.
            heapUpdate(r.frontier, i);
        } else {
            // There's nothing else to read. Delete this cursor from the frontier.
            r.frontier = heapRemove(r.frontier, i);
        }
        return (true, default!);
    };
    // Inject a CPU sample if it comes next.
    if (len(r.cpuSamples) != 0) {
        if (len(r.frontier) == 0 || r.cpuSamples[0].time < r.frontier[0].ev.time) {
            var eΔ1 = r.cpuSamples[0].asEvent(r.gen.evTable);
            r.cpuSamples = r.cpuSamples[1..];
            return (eΔ1, default!);
        }
    }
    // Try to advance the head of the frontier, which should have the minimum timestamp.
    // This should be by far the most common case
    if (len(r.frontier) == 0) {
        return (new ΔEvent(nil), fmt.Errorf("broken trace: frontier is empty:\n[gen=%d]\n\n%s\n%s\n"u8, r.gen.gen, dumpFrontier(r.frontier), dumpOrdering(Ꮡ(r.order))));
    }
    {
        var (okΔ5, errΔ5) = tryAdvance(0); if (errΔ5 != default!){
            return (new ΔEvent(nil), errΔ5);
        } else 
        if (!okΔ5) {
            // Try to advance the rest of the frontier, in timestamp order.
            //
            // To do this, sort the min-heap. A sorted min-heap is still a
            // min-heap, but now we can iterate over the rest and try to
            // advance in order. This path should be rare.
            slices.SortFunc(r.frontier, (ж<batchCursor>).compare);
            var success = false;
            for (nint i = 1; i < len(r.frontier); i++) {
                {
                    (okΔ5, errΔ5) = tryAdvance(i); if (errΔ5 != default!){
                        return (new ΔEvent(nil), errΔ5);
                    } else 
                    if (okΔ5) {
                        success = true;
                        break;
                    }
                }
            }
            if (!success) {
                return (new ΔEvent(nil), fmt.Errorf("broken trace: failed to advance: frontier:\n[gen=%d]\n\n%s\n%s\n"u8, r.gen.gen, dumpFrontier(r.frontier), dumpOrdering(Ꮡ(r.order))));
            }
        }
    }
    // Pick off the next event on the queue. At this point, one must exist.
    var (ev, ok) = r.order.Next();
    if (!ok) {
        throw panic("invariant violation: advance successful, but queue is empty");
    }
    return (ev, default!);
});

internal static @string dumpFrontier(slice<ж<batchCursor>> frontier) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    foreach (var (_, bc) in frontier) {
        var spec = go122.Specs()[(~bc).ev.typ];
        fmt.Fprintf(~Ꮡsb, "M %d [%s time=%d"u8, (~bc).m, spec.Name, (~bc).ev.time);
        foreach (var (i, arg) in spec.Args[1..]) {
            fmt.Fprintf(~Ꮡsb, " %s=%d"u8, arg, (~bc).ev.args[i]);
        }
        fmt.Fprintf(~Ꮡsb, "]\n"u8);
    }
    return sb.String();
}

} // end trace_package
