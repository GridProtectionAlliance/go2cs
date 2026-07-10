// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log.slog.@internal;

// Handlers for benchmarking.
using context = context_package;
using fmt = fmt_package;
using io = io_package;
using slog = go.log.slog_package;
using buffer = go.log.slog.@internal.buffer_package;
using strconv = strconv_package;
using time = time_package;
using go.log;
using go.log.slog.@internal;

partial class benchmarks_package {

// A fastTextHandler writes a Record to an io.Writer in a format similar to
// slog.TextHandler, but without quoting or locking. It has a few other
// performance-motivated shortcuts, like writing times as seconds since the
// epoch instead of strings.
//
// It is intended to represent a high-performance Handler that synchronously
// writes text (as opposed to binary).
[GoType] partial struct fastTextHandler {
    internal io.Writer w;
}

internal static slogꓸHandler newFastTextHandler(io.Writer w) {
    return new fastTextHandlerжΔHandler(Ꮡ(new fastTextHandler(w: w)));
}

[GoRecv] internal static bool Enabled(this ref fastTextHandler h, context.Context _Δp1, slogꓸLevel _Δp2) {
    return true;
}

internal static error Handle(this ж<fastTextHandler> Ꮡh, context.Context _Δp1, slog.Record r) => func((defer, recover) => {
    ref var h = ref Ꮡh.Value;

    var buf = buffer.New();
    var bufʗ1 = buf;
    defer(bufʗ1.Free);
    if (!r.Time.IsZero()) {
        buf.WriteString("time="u8);
        h.appendTime(buf, r.Time);
        buf.WriteByte((rune)' ');
    }
    buf.WriteString("level="u8);
    buf.ValueSlot = strconv.AppendInt(buf.ValueSlot, (int64)(nint)r.Level, 10);
    buf.WriteByte((rune)' ');
    buf.WriteString("msg="u8);
    buf.WriteString(r.Message);
    var bufʗ2 = buf;
    r.Attrs((slog.Attr a) => {
        bufʗ2.WriteByte((rune)' ');
        bufʗ2.WriteString(a.Key);
        bufʗ2.WriteByte((rune)'=');
        Ꮡh.Value.appendValue(bufʗ2, a.Value);
        return true;
    });
    buf.WriteByte((rune)'\n');
    var (_, err) = h.w.Write(buf.ValueSlot);
    return err;
});

[GoRecv] internal static void appendValue(this ref fastTextHandler h, ж<buffer.Buffer> Ꮡbuf, slog.Value v) {
    ref var buf = ref Ꮡbuf.Value;

    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == slog.KindString) {
        buf.WriteString(v.String());
    }
    else if (exprᴛ1 == slog.KindInt64) {
        buf = strconv.AppendInt(buf, v.Int64(), 10);
    }
    else if (exprᴛ1 == slog.KindUint64) {
        buf = strconv.AppendUint(buf, v.Uint64(), 10);
    }
    else if (exprᴛ1 == slog.KindFloat64) {
        buf = strconv.AppendFloat(buf, v.Float64(), (rune)'g', -1, 64);
    }
    else if (exprᴛ1 == slog.KindBool) {
        buf = strconv.AppendBool(buf, v.Bool());
    }
    else if (exprᴛ1 == slog.KindDuration) {
        buf = strconv.AppendInt(buf, v.Duration().Nanoseconds(), 10);
    }
    else if (exprᴛ1 == slog.KindTime) {
        h.appendTime(Ꮡbuf, v.Time());
    }
    else if (exprᴛ1 == slog.KindAny) {
        var a = v.Any();
        switch (a.type()) {
        case {} ΔaΔ1 when ΔaΔ1._<error>(out var aΔ1): {
            buf.WriteString(aΔ1.Error());
            break;
        }
        default: {
            var aΔ1 = a;
            fmt.Fprint(new buffer_BufferжWriter(Ꮡbuf), aΔ1);
            break;
        }}
    }
    else { /* default: */
        throw panic(fmt.Sprintf("bad kind: %s"u8, v.Kind()));
    }

}

[GoRecv] internal static void appendTime(this ref fastTextHandler h, ж<buffer.Buffer> Ꮡbuf, time.Time t) {
    ref var buf = ref Ꮡbuf.Value;

    buf = strconv.AppendInt(buf, t.Unix(), 10);
}

[GoRecv] internal static slogꓸHandler WithAttrs(this ref fastTextHandler h, slice<slog.Attr> _) {
    throw panic("fastTextHandler: With unimplemented");
}

[GoRecv] internal static slogꓸHandler WithGroup(this ref fastTextHandler _Δp0, @string _Δp1) {
    throw panic("fastTextHandler: WithGroup unimplemented");
}

// An asyncHandler simulates a Handler that passes Records to a
// background goroutine for processing.
// Because sending to a channel can be expensive due to locking,
// we simulate a lock-free queue by adding the Record to a ring buffer.
// Omitting the locking makes this little more than a copy of the Record,
// but that is a worthwhile thing to measure because Records are on the large
// side. Since nothing actually reads from the ring buffer, it can handle an
// arbitrary number of Records without either blocking or allocation.
[GoType] partial struct asyncHandler {
    internal array<slog.Record> ringBuffer = new(100);
    internal nint next;
}

internal static ж<asyncHandler> newAsyncHandler() {
    return Ꮡ(new asyncHandler(nil));
}

[GoRecv] internal static bool Enabled(this ref asyncHandler _Δp0, context.Context _Δp1, slogꓸLevel _Δp2) {
    return true;
}

[GoRecv] internal static error Handle(this ref asyncHandler h, context.Context _, slog.Record r) {
    h.ringBuffer[h.next] = r.Clone();
    h.next = (h.next + 1) % len(h.ringBuffer);
    return default!;
}

[GoRecv] internal static slogꓸHandler WithAttrs(this ref asyncHandler _Δp0, slice<slog.Attr> _Δp1) {
    throw panic("asyncHandler: With unimplemented");
}

[GoRecv] internal static slogꓸHandler WithGroup(this ref asyncHandler _Δp0, @string _Δp1) {
    throw panic("asyncHandler: WithGroup unimplemented");
}

// A disabledHandler's Enabled method always returns false.
[GoType] partial struct disabledHandler {
}

internal static bool Enabled(this disabledHandler _Δp0, context.Context _Δp1, slogꓸLevel _Δp2) {
    return false;
}

internal static error Handle(this disabledHandler _Δp0, context.Context _Δp1, slog.Record _Δp2) {
    throw panic("should not be called");
}

internal static slogꓸHandler WithAttrs(this disabledHandler _Δp0, slice<slog.Attr> _Δp1) {
    throw panic("disabledHandler: With unimplemented");
}

internal static slogꓸHandler WithGroup(this disabledHandler _Δp0, @string _Δp1) {
    throw panic("disabledHandler: WithGroup unimplemented");
}

} // end benchmarks_package
