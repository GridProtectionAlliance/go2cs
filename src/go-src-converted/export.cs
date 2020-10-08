// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package core -- go2cs converted at 2020 October 08 04:54:53 UTC
// import "golang.org/x/tools/internal/event/core" ==> using core = go.golang.org.x.tools.@internal.@event.core_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\core\export.go
using context = go.context_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;

using label = go.golang.org.x.tools.@internal.@event.label_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event
{
    public static partial class core_package
    {
        // Exporter is a function that handles events.
        // It may return a modified context and event.
        public delegate  context.Context Exporter(context.Context,  Event,  label.Map);

        private static unsafe.Pointer exporter = default;

        // SetExporter sets the global exporter function that handles all events.
        // The exporter is called synchronously from the event call site, so it should
        // return quickly so as not to hold up user code.
        public static void SetExporter(Exporter e)
        {
            var p = @unsafe.Pointer(_addr_e);
            if (e == null)
            { 
                // &e is always valid, and so p is always valid, but for the early abort
                // of ProcessEvent to be efficient it needs to make the nil check on the
                // pointer without having to dereference it, so we make the nil function
                // also a nil pointer
                p = null;

            }

            atomic.StorePointer(_addr_exporter, p);

        }

        // deliver is called to deliver an event to the supplied exporter.
        // it will fill in the time.
        private static context.Context deliver(context.Context ctx, Exporter exporter, Event ev)
        { 
            // add the current time to the event
            ev.at = time.Now(); 
            // hand the event off to the current exporter
            return exporter(ctx, ev, ev);

        }

        // Export is called to deliver an event to the global exporter if set.
        public static context.Context Export(context.Context ctx, Event ev)
        { 
            // get the global exporter and abort early if there is not one
            var exporterPtr = (Exporter.val)(atomic.LoadPointer(_addr_exporter));
            if (exporterPtr == null)
            {
                return ctx;
            }

            return deliver(ctx, exporterPtr.val, ev);

        }

        // ExportPair is called to deliver a start event to the supplied exporter.
        // It also returns a function that will deliver the end event to the same
        // exporter.
        // It will fill in the time.
        public static (context.Context, Action) ExportPair(context.Context ctx, Event begin, Event end)
        {
            context.Context _p0 = default;
            Action _p0 = default;
 
            // get the global exporter and abort early if there is not one
            var exporterPtr = (Exporter.val)(atomic.LoadPointer(_addr_exporter));
            if (exporterPtr == null)
            {
                return (ctx, () =>
                {
                });

            }

            ctx = deliver(ctx, exporterPtr.val, begin);
            return (ctx, () =>
            {
                deliver(ctx, exporterPtr.val, end);
            });

        }
    }
}}}}}}
