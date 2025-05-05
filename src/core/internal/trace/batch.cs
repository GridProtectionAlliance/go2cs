// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using @event = @internal.trace.event_package;
using go122 = @internal.trace.@event.go122_package;
using @internal.trace;
using @internal.trace.@event;
using encoding;

partial class trace_package {

[GoType("num:uint64")] partial struct timestamp;

// batch represents a batch of trace events.
// It is unparsed except for its header.
[GoType] partial struct batch {
    internal ThreadID m;
    internal timestamp time;
    internal slice<byte> data;
    internal @internal.trace.event_package.Experiment exp;
}

[GoRecv] internal static bool isStringsBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvStrings;
}

[GoRecv] internal static bool isStacksBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvStacks;
}

[GoRecv] internal static bool isCPUSamplesBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvCPUSamples;
}

[GoRecv] internal static bool isFreqBatch(this ref batch b) {
    return b.exp == @event.NoExperiment && len(b.data) > 0 && ((@event.Type)b.data[0]) == go122.EvFrequency;
}

[GoType("dyn")] partial interface readBatch_r :
    io.Reader,
    io.ByteReader
{
}

// readBatch reads the next full batch from r.
internal static (batch, uint64, error) readBatch(readBatch_r r) {
    // Read batch header byte.
    var (b, err) = r.ReadByte();
    if (err != default!) {
        return (new batch(nil), 0, err);
    }
    {
        var typ = ((@event.Type)b); if (typ != go122.EvEventBatch && typ != go122.EvExperimentalBatch) {
            return (new batch(nil), 0, fmt.Errorf("expected batch event, got %s"u8, go122.EventString(typ)));
        }
    }
    // Read the experiment of we have one.
    @event.Experiment exp = @event.NoExperiment;
    if (((@event.Type)b) == go122.EvExperimentalBatch) {
        var (e, errΔ1) = r.ReadByte();
        if (errΔ1 != default!) {
            return (new batch(nil), 0, errΔ1);
        }
        exp = ((@event.Experiment)e);
    }
    // Read the batch header: gen (generation), thread (M) ID, base timestamp
    // for the batch.
    var (gen, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch gen: %w"u8, err));
    }
    var (m, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch M ID: %w"u8, err));
    }
    var (ts, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch timestamp: %w"u8, err));
    }
    // Read in the size of the batch to follow.
    var (size, err) = binary.ReadUvarint(r);
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("error reading batch size: %w"u8, err));
    }
    if (size > go122.MaxBatchSize) {
        return (new batch(nil), gen, fmt.Errorf("invalid batch size %d, maximum is %d"u8, size, go122.MaxBatchSize));
    }
    // Copy out the batch for later processing.
    ref var data = ref heap(new bytes_package.Buffer(), out var Ꮡdata);
    data.Grow(((nint)size));
    var (n, err) = io.CopyN(~Ꮡdata, r, ((int64)size));
    if (n != ((int64)size)) {
        return (new batch(nil), gen, fmt.Errorf("failed to read full batch: read %d but wanted %d"u8, n, size));
    }
    if (err != default!) {
        return (new batch(nil), gen, fmt.Errorf("copying batch data: %w"u8, err));
    }
    // Return the batch.
    return (new batch(
        m: ((ThreadID)m),
        time: ((timestamp)ts),
        data: data.Bytes(),
        exp: exp
    ), gen, default!);
}

} // end trace_package
