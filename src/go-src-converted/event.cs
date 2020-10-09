// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @event -- go2cs converted at 2020 October 09 06:01:41 UTC
// import "golang.org/x/tools/internal/event" ==> using @event = go.golang.org.x.tools.@internal.@event_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\event.go
using context = go.context_package;

using core = go.golang.org.x.tools.@internal.@event.core_package;
using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class @event_package
    {
        // Exporter is a function that handles events.
        // It may return a modified context and event.
        public delegate  context.Context Exporter(context.Context,  core.Event,  label.Map);

        // SetExporter sets the global exporter function that handles all events.
        // The exporter is called synchronously from the event call site, so it should
        // return quickly so as not to hold up user code.
        public static void SetExporter(Exporter e)
        {
            core.SetExporter(core.Exporter(e));
        }

        // Log takes a message and a label list and combines them into a single event
        // before delivering them to the exporter.
        public static void Log(context.Context ctx, @string message, params label.Label[] labels)
        {
            labels = labels.Clone();

            core.Export(ctx, core.MakeEvent(new array<label.Label>(new label.Label[] { keys.Msg.Of(message) }), labels));
        }

        // IsLog returns true if the event was built by the Log function.
        // It is intended to be used in exporters to identify the semantics of the
        // event when deciding what to do with it.
        public static bool IsLog(core.Event ev)
        {
            return ev.Label(0L).Key() == keys.Msg;
        }

        // Error takes a message and a label list and combines them into a single event
        // before delivering them to the exporter. It captures the error in the
        // delivered event.
        public static void Error(context.Context ctx, @string message, error err, params label.Label[] labels)
        {
            labels = labels.Clone();

            core.Export(ctx, core.MakeEvent(new array<label.Label>(new label.Label[] { keys.Msg.Of(message), keys.Err.Of(err) }), labels));
        }

        // IsError returns true if the event was built by the Error function.
        // It is intended to be used in exporters to identify the semantics of the
        // event when deciding what to do with it.
        public static bool IsError(core.Event ev)
        {
            return ev.Label(0L).Key() == keys.Msg && ev.Label(1L).Key() == keys.Err;
        }

        // Metric sends a label event to the exporter with the supplied labels.
        public static void Metric(context.Context ctx, params label.Label[] labels)
        {
            labels = labels.Clone();

            core.Export(ctx, core.MakeEvent(new array<label.Label>(new label.Label[] { keys.Metric.New() }), labels));
        }

        // IsMetric returns true if the event was built by the Metric function.
        // It is intended to be used in exporters to identify the semantics of the
        // event when deciding what to do with it.
        public static bool IsMetric(core.Event ev)
        {
            return ev.Label(0L).Key() == keys.Metric;
        }

        // Label sends a label event to the exporter with the supplied labels.
        public static context.Context Label(context.Context ctx, params label.Label[] labels)
        {
            labels = labels.Clone();

            return core.Export(ctx, core.MakeEvent(new array<label.Label>(new label.Label[] { keys.Label.New() }), labels));
        }

        // IsLabel returns true if the event was built by the Label function.
        // It is intended to be used in exporters to identify the semantics of the
        // event when deciding what to do with it.
        public static bool IsLabel(core.Event ev)
        {
            return ev.Label(0L).Key() == keys.Label;
        }

        // Start sends a span start event with the supplied label list to the exporter.
        // It also returns a function that will end the span, which should normally be
        // deferred.
        public static (context.Context, Action) Start(context.Context ctx, @string name, params label.Label[] labels)
        {
            context.Context _p0 = default;
            Action _p0 = default;
            labels = labels.Clone();

            return core.ExportPair(ctx, core.MakeEvent(new array<label.Label>(new label.Label[] { keys.Start.Of(name) }), labels), core.MakeEvent(new array<label.Label>(new label.Label[] { keys.End.New() }), null));
        }

        // IsStart returns true if the event was built by the Start function.
        // It is intended to be used in exporters to identify the semantics of the
        // event when deciding what to do with it.
        public static bool IsStart(core.Event ev)
        {
            return ev.Label(0L).Key() == keys.Start;
        }

        // IsEnd returns true if the event was built by the End function.
        // It is intended to be used in exporters to identify the semantics of the
        // event when deciding what to do with it.
        public static bool IsEnd(core.Event ev)
        {
            return ev.Label(0L).Key() == keys.End;
        }

        // Detach returns a context without an associated span.
        // This allows the creation of spans that are not children of the current span.
        public static context.Context Detach(context.Context ctx)
        {
            return core.Export(ctx, core.MakeEvent(new array<label.Label>(new label.Label[] { keys.Detach.New() }), null));
        }

        // IsDetach returns true if the event was built by the Detach function.
        // It is intended to be used in exporters to identify the semantics of the
        // event when deciding what to do with it.
        public static bool IsDetach(core.Event ev)
        {
            return ev.Label(0L).Key() == keys.Detach;
        }
    }
}}}}}
