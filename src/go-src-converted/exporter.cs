// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package metric aggregates events into metrics that can be exported.
// package metric -- go2cs converted at 2022 March 06 23:31:41 UTC
// import "golang.org/x/tools/internal/event/export/metric" ==> using metric = go.golang.org.x.tools.@internal.@event.export.metric_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\metric\exporter.go
using context = go.context_package;
using sync = go.sync_package;
using time = go.time_package;

using @event = go.golang.org.x.tools.@internal.@event_package;
using core = go.golang.org.x.tools.@internal.@event.core_package;
using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using System;


namespace go.golang.org.x.tools.@internal.@event.export;

public static partial class metric_package {

public static var Entries = keys.New("metric_entries", "The set of metrics calculated for an event");

public partial struct Config {
}

public delegate  Data subscriber(time.Time,  label.Map,  label.Label);

private static void subscribe(this ptr<Config> _addr_e, label.Key key, subscriber s) {
    ref Config e = ref _addr_e.val;

    if (e.subscribers == null) {
        e.subscribers = make();
    }
    e.subscribers[key] = append(e.subscribers[key], s);

}

private static event.Exporter Exporter(this ptr<Config> _addr_e, event.Exporter output) => func((defer, _, _) => {
    ref Config e = ref _addr_e.val;

    sync.Mutex mu = default;
    return (ctx, ev, lm) => {
        if (!@event.IsMetric(ev)) {
            return output(ctx, ev, lm);
        }
        mu.Lock();
        defer(mu.Unlock());
        slice<Data> metrics = default;
        for (nint index = 0; ev.Valid(index); index++) {
            var l = ev.Label(index);
            if (!l.Valid()) {
                continue;
            }
            var id = l.Key();
            {
                var list = e.subscribers[id];

                if (len(list) > 0) {
                    foreach (var (_, s) in list) {
                        metrics = append(metrics, s(ev.At(), lm, l));
                    }
                }

            }

        }
        lm = label.MergeMaps(label.NewMap(Entries.Of(metrics)), lm);
        return output(ctx, ev, lm);

    };

});

} // end metric_package
