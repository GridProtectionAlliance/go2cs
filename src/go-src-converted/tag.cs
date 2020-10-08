// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package export -- go2cs converted at 2020 October 08 04:54:55 UTC
// import "golang.org/x/tools/internal/event/export" ==> using export = go.golang.org.x.tools.@internal.@event.export_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\tag.go
using context = go.context_package;

using @event = go.golang.org.x.tools.@internal.@event_package;
using core = go.golang.org.x.tools.@internal.@event.core_package;
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
    public static partial class export_package
    {
        // Labels builds an exporter that manipulates the context using the event.
        // If the event is type IsLabel or IsStartSpan then it returns a context updated
        // with label values from the event.
        // For all other event types the event labels will be updated with values from the
        // context if they are missing.
        public static event.Exporter Labels(event.Exporter output)
        {
            return (ctx, ev, lm) =>
            {
                label.Map (stored, _) = ctx.Value(labelContextKey)._<label.Map>();
                if (@event.IsLabel(ev) || @event.IsStart(ev))
                { 
                    // update the label map stored in the context
                    var fromEvent = label.Map(ev);
                    if (stored == null)
                    {
                        stored = fromEvent;
                    }
                    else
                    {
                        stored = label.MergeMaps(fromEvent, stored);
                    }
                    ctx = context.WithValue(ctx, labelContextKey, stored);

                }
                lm = label.MergeMaps(lm, stored);
                return output(ctx, ev, lm);

            };

        }
    }
}}}}}}
