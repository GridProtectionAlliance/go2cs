// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package core -- go2cs converted at 2022 March 06 23:31:36 UTC
// import "golang.org/x/tools/internal/event/core" ==> using core = go.golang.org.x.tools.@internal.@event.core_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\core\fast.go
using context = go.context_package;

using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using System;


namespace go.golang.org.x.tools.@internal.@event;

public static partial class core_package {

    // Log1 takes a message and one label delivers a log event to the exporter.
    // It is a customized version of Print that is faster and does no allocation.
public static void Log1(context.Context ctx, @string message, label.Label t1) {
    Export(ctx, MakeEvent(new array<label.Label>(new label.Label[] { keys.Msg.Of(message), t1 }), null));
}

// Log2 takes a message and two labels and delivers a log event to the exporter.
// It is a customized version of Print that is faster and does no allocation.
public static void Log2(context.Context ctx, @string message, label.Label t1, label.Label t2) {
    Export(ctx, MakeEvent(new array<label.Label>(new label.Label[] { keys.Msg.Of(message), t1, t2 }), null));
}

// Metric1 sends a label event to the exporter with the supplied labels.
public static context.Context Metric1(context.Context ctx, label.Label t1) {
    return Export(ctx, MakeEvent(new array<label.Label>(new label.Label[] { keys.Metric.New(), t1 }), null));
}

// Metric2 sends a label event to the exporter with the supplied labels.
public static context.Context Metric2(context.Context ctx, label.Label t1, label.Label t2) {
    return Export(ctx, MakeEvent(new array<label.Label>(new label.Label[] { keys.Metric.New(), t1, t2 }), null));
}

// Start1 sends a span start event with the supplied label list to the exporter.
// It also returns a function that will end the span, which should normally be
// deferred.
public static (context.Context, Action) Start1(context.Context ctx, @string name, label.Label t1) {
    context.Context _p0 = default;
    Action _p0 = default;

    return ExportPair(ctx, MakeEvent(new array<label.Label>(new label.Label[] { keys.Start.Of(name), t1 }), null), MakeEvent(new array<label.Label>(new label.Label[] { keys.End.New() }), null));
}

// Start2 sends a span start event with the supplied label list to the exporter.
// It also returns a function that will end the span, which should normally be
// deferred.
public static (context.Context, Action) Start2(context.Context ctx, @string name, label.Label t1, label.Label t2) {
    context.Context _p0 = default;
    Action _p0 = default;

    return ExportPair(ctx, MakeEvent(new array<label.Label>(new label.Label[] { keys.Start.Of(name), t1, t2 }), null), MakeEvent(new array<label.Label>(new label.Label[] { keys.End.New() }), null));
}

} // end core_package
