// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package prometheus -- go2cs converted at 2022 March 06 23:31:45 UTC
// import "golang.org/x/tools/internal/event/export/prometheus" ==> using prometheus = go.golang.org.x.tools.@internal.@event.export.prometheus_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\prometheus\prometheus.go
using bytes = go.bytes_package;
using context = go.context_package;
using fmt = go.fmt_package;
using http = go.net.http_package;
using sort = go.sort_package;
using sync = go.sync_package;

using @event = go.golang.org.x.tools.@internal.@event_package;
using core = go.golang.org.x.tools.@internal.@event.core_package;
using metric = go.golang.org.x.tools.@internal.@event.export.metric_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using System;


namespace go.golang.org.x.tools.@internal.@event.export;

public static partial class prometheus_package {

public static ptr<Exporter> New() {
    return addr(new Exporter());
}

public partial struct Exporter {
    public sync.Mutex mu;
    public slice<metric.Data> metrics;
}

private static context.Context ProcessEvent(this ptr<Exporter> _addr_e, context.Context ctx, core.Event ev, label.Map ln) => func((defer, _, _) => {
    ref Exporter e = ref _addr_e.val;

    if (!@event.IsMetric(ev)) {
        return ctx;
    }
    e.mu.Lock();
    defer(e.mu.Unlock());
    slice<metric.Data> metrics = metric.Entries.Get(ln)._<slice<metric.Data>>();
    foreach (var (_, data) in metrics) {
        var name = data.Handle(); 
        // We keep the metrics in name sorted order so the page is stable and easy
        // to read. We do this with an insertion sort rather than sorting the list
        // each time
        var index = sort.Search(len(e.metrics), i => {
            return e.metrics[i].Handle() >= name;
        });
        if (index >= len(e.metrics) || e.metrics[index].Handle() != name) { 
            // we have a new metric, so we need to make a space for it
            var old = e.metrics;
            e.metrics = make_slice<metric.Data>(len(old) + 1);
            copy(e.metrics, old[..(int)index]);
            copy(e.metrics[(int)index + 1..], old[(int)index..]);

        }
        e.metrics[index] = data;

    }    return ctx;

});

private static void header(this ptr<Exporter> _addr_e, http.ResponseWriter w, @string name, @string description, bool isGauge, bool isHistogram) {
    ref Exporter e = ref _addr_e.val;

    @string kind = "counter";
    if (isGauge) {
        kind = "gauge";
    }
    if (isHistogram) {
        kind = "histogram";
    }
    fmt.Fprintf(w, "# HELP %s %s\n", name, description);
    fmt.Fprintf(w, "# TYPE %s %s\n", name, kind);

}

private static void row(this ptr<Exporter> _addr_e, http.ResponseWriter w, @string name, slice<label.Label> group, @string extra, object value) {
    ref Exporter e = ref _addr_e.val;

    fmt.Fprint(w, name);
    ptr<bytes.Buffer> buf = addr(new bytes.Buffer());
    fmt.Fprint(buf, group);
    if (extra != "") {
        if (buf.Len() > 0) {
            fmt.Fprint(buf, ",");
        }
        fmt.Fprint(buf, extra);

    }
    if (buf.Len() > 0) {
        fmt.Fprint(w, "{");
        buf.WriteTo(w);
        fmt.Fprint(w, "}");
    }
    fmt.Fprintf(w, " %v\n", value);

}

private static void Serve(this ptr<Exporter> _addr_e, http.ResponseWriter w, ptr<http.Request> _addr_r) => func((defer, _, _) => {
    ref Exporter e = ref _addr_e.val;
    ref http.Request r = ref _addr_r.val;

    e.mu.Lock();
    defer(e.mu.Unlock());
    {
        var data__prev1 = data;

        foreach (var (_, __data) in e.metrics) {
            data = __data;
            switch (data.type()) {
                case ptr<metric.Int64Data> data:
                    e.header(w, data.Info.Name, data.Info.Description, data.IsGauge, false);
                    {
                        var i__prev2 = i;
                        var group__prev2 = group;

                        foreach (var (__i, __group) in data.Groups()) {
                            i = __i;
                            group = __group;
                            e.row(w, data.Info.Name, group, "", data.Rows[i]);
                        }

                        i = i__prev2;
                        group = group__prev2;
                    }
                    break;
                case ptr<metric.Float64Data> data:
                    e.header(w, data.Info.Name, data.Info.Description, data.IsGauge, false);
                    {
                        var i__prev2 = i;
                        var group__prev2 = group;

                        foreach (var (__i, __group) in data.Groups()) {
                            i = __i;
                            group = __group;
                            e.row(w, data.Info.Name, group, "", data.Rows[i]);
                        }

                        i = i__prev2;
                        group = group__prev2;
                    }
                    break;
                case ptr<metric.HistogramInt64Data> data:
                    e.header(w, data.Info.Name, data.Info.Description, false, true);
                    {
                        var i__prev2 = i;
                        var group__prev2 = group;

                        foreach (var (__i, __group) in data.Groups()) {
                            i = __i;
                            group = __group;
                            var row = data.Rows[i];
                            {
                                var j__prev3 = j;
                                var b__prev3 = b;

                                foreach (var (__j, __b) in data.Info.Buckets) {
                                    j = __j;
                                    b = __b;
                                    e.row(w, data.Info.Name + "_bucket", group, fmt.Sprintf("le=\"%v\"", b), row.Values[j]);
                                }

                                j = j__prev3;
                                b = b__prev3;
                            }

                            e.row(w, data.Info.Name + "_bucket", group, "le=\"+Inf\"", row.Count);
                            e.row(w, data.Info.Name + "_count", group, "", row.Count);
                            e.row(w, data.Info.Name + "_sum", group, "", row.Sum);

                        }

                        i = i__prev2;
                        group = group__prev2;
                    }
                    break;
                case ptr<metric.HistogramFloat64Data> data:
                    e.header(w, data.Info.Name, data.Info.Description, false, true);
                    {
                        var i__prev2 = i;
                        var group__prev2 = group;

                        foreach (var (__i, __group) in data.Groups()) {
                            i = __i;
                            group = __group;
                            row = data.Rows[i];
                            {
                                var j__prev3 = j;
                                var b__prev3 = b;

                                foreach (var (__j, __b) in data.Info.Buckets) {
                                    j = __j;
                                    b = __b;
                                    e.row(w, data.Info.Name + "_bucket", group, fmt.Sprintf("le=\"%v\"", b), row.Values[j]);
                                }

                                j = j__prev3;
                                b = b__prev3;
                            }

                            e.row(w, data.Info.Name + "_bucket", group, "le=\"+Inf\"", row.Count);
                            e.row(w, data.Info.Name + "_count", group, "", row.Count);
                            e.row(w, data.Info.Name + "_sum", group, "", row.Sum);

                        }

                        i = i__prev2;
                        group = group__prev2;
                    }
                    break;
            }

        }
        data = data__prev1;
    }
});

} // end prometheus_package
