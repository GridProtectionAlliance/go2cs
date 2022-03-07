// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package export -- go2cs converted at 2022 March 06 23:31:38 UTC
// import "golang.org/x/tools/internal/event/export" ==> using export = go.golang.org.x.tools.@internal.@event.export_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\trace.go
using context = go.context_package;
using fmt = go.fmt_package;
using sync = go.sync_package;

using @event = go.golang.org.x.tools.@internal.@event_package;
using core = go.golang.org.x.tools.@internal.@event.core_package;
using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using System;


namespace go.golang.org.x.tools.@internal.@event;

public static partial class export_package {

public partial struct SpanContext {
    public TraceID TraceID;
    public SpanID SpanID;
}

public partial struct Span {
    public @string Name;
    public SpanContext ID;
    public SpanID ParentID;
    public sync.Mutex mu;
    public core.Event start;
    public core.Event finish;
    public slice<core.Event> events;
}

private partial struct contextKeyType { // : nint
}

private static readonly var spanContextKey = contextKeyType(iota);
private static readonly var labelContextKey = 0;


public static ptr<Span> GetSpan(context.Context ctx) {
    var v = ctx.Value(spanContextKey);
    if (v == null) {
        return _addr_null!;
    }
    return v._<ptr<Span>>();

}

// Spans creates an exporter that maintains hierarchical span structure in the
// context.
// It creates new spans on start events, adds events to the current span on
// log or label, and closes the span on end events.
// The span structure can then be used by other exporters.
public static event.Exporter Spans(event.Exporter output) {
    return (ctx, ev, lm) => {

        if (@event.IsLog(ev) || @event.IsLabel(ev)) 
            {
                var span__prev1 = span;

                var span = GetSpan(ctx);

                if (span != null) {
                    span.mu.Lock();
                    span.events = append(span.events, ev);
                    span.mu.Unlock();
                }

                span = span__prev1;

            }

        else if (@event.IsStart(ev)) 
            span = addr(new Span(Name:keys.Start.Get(lm),start:ev,));
            {
                var parent = GetSpan(ctx);

                if (parent != null) {
                    span.ID.TraceID = parent.ID.TraceID;
                    span.ParentID = parent.ID.SpanID;
                }
                else
 {
                    span.ID.TraceID = newTraceID();
                }

            }

            span.ID.SpanID = newSpanID();
            ctx = context.WithValue(ctx, spanContextKey, span);
        else if (@event.IsEnd(ev)) 
            {
                var span__prev1 = span;

                span = GetSpan(ctx);

                if (span != null) {
                    span.mu.Lock();
                    span.finish = ev;
                    span.mu.Unlock();
                }

                span = span__prev1;

            }

        else if (@event.IsDetach(ev)) 
            ctx = context.WithValue(ctx, spanContextKey, null);
                return output(ctx, ev, lm);

    };

}

private static void Format(this ptr<SpanContext> _addr_s, fmt.State f, int r) {
    ref SpanContext s = ref _addr_s.val;

    fmt.Fprintf(f, "%v:%v", s.TraceID, s.SpanID);
}

private static core.Event Start(this ptr<Span> _addr_s) {
    ref Span s = ref _addr_s.val;
 
    // start never changes after construction, so we dont need to hold the mutex
    return s.start;

}

private static core.Event Finish(this ptr<Span> _addr_s) => func((defer, _, _) => {
    ref Span s = ref _addr_s.val;

    s.mu.Lock();
    defer(s.mu.Unlock());
    return s.finish;
});

private static slice<core.Event> Events(this ptr<Span> _addr_s) => func((defer, _, _) => {
    ref Span s = ref _addr_s.val;

    s.mu.Lock();
    defer(s.mu.Unlock());
    return s.events;
});

private static void Format(this ptr<Span> _addr_s, fmt.State f, int r) => func((defer, _, _) => {
    ref Span s = ref _addr_s.val;

    s.mu.Lock();
    defer(s.mu.Unlock());
    fmt.Fprintf(f, "%v %v", s.Name, s.ID);
    if (s.ParentID.IsValid()) {
        fmt.Fprintf(f, "[%v]", s.ParentID);
    }
    fmt.Fprintf(f, " %v->%v", s.start, s.finish);

});

} // end export_package
