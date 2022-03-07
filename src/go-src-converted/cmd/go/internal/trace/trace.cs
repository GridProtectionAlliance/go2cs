// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package trace -- go2cs converted at 2022 March 06 23:16:22 UTC
// import "cmd/go/internal/trace" ==> using trace = go.cmd.go.@internal.trace_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\trace\trace.go
using traceviewer = go.cmd.@internal.traceviewer_package;
using context = go.context_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using os = go.os_package;
using strings = go.strings_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using System;


namespace go.cmd.go.@internal;

public static partial class trace_package {

    // Constants used in event fields.
    // See https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU
    // for more details.
private static readonly @string phaseDurationBegin = "B";
private static readonly @string phaseDurationEnd = "E";
private static readonly @string phaseFlowStart = "s";
private static readonly @string phaseFlowEnd = "f";

private static readonly @string bindEnclosingSlice = "e";


private static int traceStarted = default;

private static (traceContext, bool) getTraceContext(context.Context ctx) {
    traceContext _p0 = default;
    bool _p0 = default;

    if (atomic.LoadInt32(_addr_traceStarted) == 0) {
        return (new traceContext(), false);
    }
    var v = ctx.Value(new traceKey());
    if (v == null) {
        return (new traceContext(), false);
    }
    return (v._<traceContext>(), true);

}

// StartSpan starts a trace event with the given name. The Span ends when its Done method is called.
public static (context.Context, ptr<Span>) StartSpan(context.Context ctx, @string name) {
    context.Context _p0 = default;
    ptr<Span> _p0 = default!;

    var (tc, ok) = getTraceContext(ctx);
    if (!ok) {
        return (ctx, _addr_null!);
    }
    ptr<Span> childSpan = addr(new Span(t:tc.t,name:name,tid:tc.tid,start:time.Now()));
    tc.t.writeEvent(addr(new traceviewer.Event(Name:childSpan.name,Time:float64(childSpan.start.UnixNano())/float64(time.Microsecond),TID:childSpan.tid,Phase:phaseDurationBegin,)));
    ctx = context.WithValue(ctx, new traceKey(), new traceContext(tc.t,tc.tid));
    return (ctx, _addr_childSpan!);

}

// StartGoroutine associates the context with a new Thread ID. The Chrome trace viewer associates each
// trace event with a thread, and doesn't expect events with the same thread id to happen at the
// same time.
public static context.Context StartGoroutine(context.Context ctx) {
    var (tc, ok) = getTraceContext(ctx);
    if (!ok) {
        return ctx;
    }
    return context.WithValue(ctx, new traceKey(), new traceContext(tc.t,tc.t.getNextTID()));

}

// Flow marks a flow indicating that the 'to' span depends on the 'from' span.
// Flow should be called while the 'to' span is in progress.
public static void Flow(context.Context ctx, ptr<Span> _addr_from, ptr<Span> _addr_to) {
    ref Span from = ref _addr_from.val;
    ref Span to = ref _addr_to.val;

    var (tc, ok) = getTraceContext(ctx);
    if (!ok || from == null || to == null) {
        return ;
    }
    var id = tc.t.getNextFlowID();
    tc.t.writeEvent(addr(new traceviewer.Event(Name:from.name+" -> "+to.name,Category:"flow",ID:id,Time:float64(from.end.UnixNano())/float64(time.Microsecond),Phase:phaseFlowStart,TID:from.tid,)));
    tc.t.writeEvent(addr(new traceviewer.Event(Name:from.name+" -> "+to.name,Category:"flow",ID:id,Time:float64(to.start.UnixNano())/float64(time.Microsecond),Phase:phaseFlowEnd,TID:to.tid,BindPoint:bindEnclosingSlice,)));

}

public partial struct Span {
    public ptr<tracer> t;
    public @string name;
    public ulong tid;
    public time.Time start;
    public time.Time end;
}

private static void Done(this ptr<Span> _addr_s) {
    ref Span s = ref _addr_s.val;

    if (s == null) {
        return ;
    }
    s.end = time.Now();
    s.t.writeEvent(addr(new traceviewer.Event(Name:s.name,Time:float64(s.end.UnixNano())/float64(time.Microsecond),TID:s.tid,Phase:phaseDurationEnd,)));

}

private partial struct tracer {
    public channel<traceFile> file; // 1-buffered

    public ulong nextTID;
    public ulong nextFlowID;
}

private static error writeEvent(this ptr<tracer> _addr_t, ptr<traceviewer.Event> _addr_ev) => func((defer, _, _) => {
    ref tracer t = ref _addr_t.val;
    ref traceviewer.Event ev = ref _addr_ev.val;

    var f = t.file.Receive();
    defer(() => {
        t.file.Send(f);
    }());
    error err = default!;
    if (f.entries == 0) {
        _, err = f.sb.WriteString("[\n");
    }
    else
 {
        _, err = f.sb.WriteString(",");
    }
    f.entries++;
    if (err != null) {
        return error.As(null!)!;
    }
    {
        error err__prev1 = err;

        err = f.enc.Encode(ev);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Write event string to output file.
    _, err = f.f.WriteString(f.sb.String());
    f.sb.Reset();
    return error.As(err)!;

});

private static error Close(this ptr<tracer> _addr_t) => func((defer, _, _) => {
    ref tracer t = ref _addr_t.val;

    var f = t.file.Receive();
    defer(() => {
        t.file.Send(f);
    }());

    var (_, firstErr) = f.f.WriteString("]");
    {
        var err = f.f.Close();

        if (firstErr == null) {
            firstErr = err;
        }
    }

    return error.As(firstErr)!;

});

private static ulong getNextTID(this ptr<tracer> _addr_t) {
    ref tracer t = ref _addr_t.val;

    return atomic.AddUint64(_addr_t.nextTID, 1);
}

private static ulong getNextFlowID(this ptr<tracer> _addr_t) {
    ref tracer t = ref _addr_t.val;

    return atomic.AddUint64(_addr_t.nextFlowID, 1);
}

// traceKey is the context key for tracing information. It is unexported to prevent collisions with context keys defined in
// other packages.
private partial struct traceKey {
}

private partial struct traceContext {
    public ptr<tracer> t;
    public ulong tid;
}

// Start starts a trace which writes to the given file.
public static (context.Context, Func<error>, error) Start(context.Context ctx, @string file) {
    context.Context _p0 = default;
    Func<error> _p0 = default;
    error _p0 = default!;

    atomic.StoreInt32(_addr_traceStarted, 1);
    if (file == "") {
        return (null, null, error.As(errors.New("no trace file supplied"))!);
    }
    var (f, err) = os.Create(file);
    if (err != null) {
        return (null, null, error.As(err)!);
    }
    ptr<tracer> t = addr(new tracer(file:make(chantraceFile,1)));
    ptr<object> sb = @new<strings.Builder>();
    t.file.Send(new traceFile(f:f,sb:sb,enc:json.NewEncoder(sb),));
    ctx = context.WithValue(ctx, new traceKey(), new traceContext(t:t));
    return (ctx, t.Close, error.As(null!)!);

}

private partial struct traceFile {
    public ptr<os.File> f;
    public ptr<strings.Builder> sb;
    public ptr<json.Encoder> enc;
    public long entries;
}

} // end trace_package
