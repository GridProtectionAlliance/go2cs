// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ocagent adds the ability to export all telemetry to an ocagent.
// This keeps the compile time dependencies to zero and allows the agent to
// have the exporters needed for telemetry aggregation and viewing systems.
// package ocagent -- go2cs converted at 2020 October 08 04:55:03 UTC
// import "golang.org/x/tools/internal/event/export/ocagent" ==> using ocagent = go.golang.org.x.tools.@internal.@event.export.ocagent_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\ocagent\ocagent.go
using bytes = go.bytes_package;
using context = go.context_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using http = go.net.http_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sync = go.sync_package;
using time = go.time_package;

using @event = go.golang.org.x.tools.@internal.@event_package;
using core = go.golang.org.x.tools.@internal.@event.core_package;
using export = go.golang.org.x.tools.@internal.@event.export_package;
using metric = go.golang.org.x.tools.@internal.@event.export.metric_package;
using wire = go.golang.org.x.tools.@internal.@event.export.ocagent.wire_package;
using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event {
namespace export
{
    public static partial class ocagent_package
    {
        public partial struct Config
        {
            public time.Time Start;
            public @string Host;
            public uint Process;
            public ptr<http.Client> Client;
            public @string Service;
            public @string Address;
            public time.Duration Rate;
        }

        private static sync.Mutex connectMu = default;        private static var exporters = make_map<Config, ptr<Exporter>>();

        // Discover finds the local agent to export to, it will return nil if there
        // is not one running.
        // TODO: Actually implement a discovery protocol rather than a hard coded address
        public static ptr<Config> Discover()
        {
            return addr(new Config(Address:"http://localhost:55678",));
        }

        public partial struct Exporter
        {
            public sync.Mutex mu;
            public Config config;
            public slice<ptr<export.Span>> spans;
            public slice<metric.Data> metrics;
        }

        // Connect creates a process specific exporter with the specified
        // serviceName and the address of the ocagent to which it will upload
        // its telemetry.
        public static ptr<Exporter> Connect(ptr<Config> _addr_config) => func((defer, _, __) =>
        {
            ref Config config = ref _addr_config.val;

            if (config == null || config.Address == "off")
            {
                return _addr_null!;
            }

            Config resolved = config;
            if (resolved.Host == "")
            {
                var (hostname, _) = os.Hostname();
                resolved.Host = hostname;
            }

            if (resolved.Process == 0L)
            {
                resolved.Process = uint32(os.Getpid());
            }

            if (resolved.Client == null)
            {
                resolved.Client = http.DefaultClient;
            }

            if (resolved.Service == "")
            {
                resolved.Service = filepath.Base(os.Args[0L]);
            }

            if (resolved.Rate == 0L)
            {
                resolved.Rate = 2L * time.Second;
            }

            connectMu.Lock();
            defer(connectMu.Unlock());
            {
                var exporter__prev1 = exporter;

                var (exporter, found) = exporters[resolved];

                if (found)
                {
                    return _addr_exporter!;
                }

                exporter = exporter__prev1;

            }

            ptr<Exporter> exporter = addr(new Exporter(config:resolved));
            exporters[resolved] = exporter;
            if (exporter.config.Start.IsZero())
            {
                exporter.config.Start = time.Now();
            }

            go_(() => () =>
            {
                foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in time.Tick(exporter.config.Rate))
                {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                    exporter.Flush();
                }

            }());
            return _addr_exporter!;

        });

        private static context.Context ProcessEvent(this ptr<Exporter> _addr_e, context.Context ctx, core.Event ev, label.Map lm) => func((defer, _, __) =>
        {
            ref Exporter e = ref _addr_e.val;


            if (@event.IsEnd(ev)) 
                e.mu.Lock();
                defer(e.mu.Unlock());
                var span = export.GetSpan(ctx);
                if (span != null)
                {
                    e.spans = append(e.spans, span);
                }

            else if (@event.IsMetric(ev)) 
                e.mu.Lock();
                defer(e.mu.Unlock());
                slice<metric.Data> data = metric.Entries.Get(lm)._<slice<metric.Data>>();
                e.metrics = append(e.metrics, data);
                        return ctx;

        });

        private static void Flush(this ptr<Exporter> _addr_e) => func((defer, _, __) =>
        {
            ref Exporter e = ref _addr_e.val;

            e.mu.Lock();
            defer(e.mu.Unlock());
            var spans = make_slice<ptr<wire.Span>>(len(e.spans));
            {
                var i__prev1 = i;

                foreach (var (__i, __s) in e.spans)
                {
                    i = __i;
                    s = __s;
                    spans[i] = convertSpan(_addr_s);
                }

                i = i__prev1;
            }

            e.spans = null;
            var metrics = make_slice<ptr<wire.Metric>>(len(e.metrics));
            {
                var i__prev1 = i;

                foreach (var (__i, __m) in e.metrics)
                {
                    i = __i;
                    m = __m;
                    metrics[i] = convertMetric(m, e.config.Start);
                }

                i = i__prev1;
            }

            e.metrics = null;

            if (len(spans) > 0L)
            {
                e.send("/v1/trace", addr(new wire.ExportTraceServiceRequest(Node:e.config.buildNode(),Spans:spans,)));
            }

            if (len(metrics) > 0L)
            {
                e.send("/v1/metrics", addr(new wire.ExportMetricsServiceRequest(Node:e.config.buildNode(),Metrics:metrics,)));
            }

        });

        private static ptr<wire.Node> buildNode(this ptr<Config> _addr_cfg)
        {
            ref Config cfg = ref _addr_cfg.val;

            return addr(new wire.Node(Identifier:&wire.ProcessIdentifier{HostName:cfg.Host,Pid:cfg.Process,StartTimestamp:convertTimestamp(cfg.Start),},LibraryInfo:&wire.LibraryInfo{Language:wire.LanguageGo,ExporterVersion:"0.0.1",CoreLibraryVersion:"x/tools",},ServiceInfo:&wire.ServiceInfo{Name:cfg.Service,},));
        }

        private static void send(this ptr<Exporter> _addr_e, @string endpoint, object message)
        {
            ref Exporter e = ref _addr_e.val;

            var (blob, err) = json.Marshal(message);
            if (err != null)
            {
                errorInExport("ocagent failed to marshal message for %v: %v", endpoint, err);
                return ;
            }

            var uri = e.config.Address + endpoint;
            var (req, err) = http.NewRequest("POST", uri, bytes.NewReader(blob));
            if (err != null)
            {
                errorInExport("ocagent failed to build request for %v: %v", uri, err);
                return ;
            }

            req.Header.Set("Content-Type", "application/json");
            var (res, err) = e.config.Client.Do(req);
            if (err != null)
            {
                errorInExport("ocagent failed to send message: %v \n", err);
                return ;
            }

            if (res.Body != null)
            {
                res.Body.Close();
            }

        }

        private static void errorInExport(@string message, params object[] args)
        {
            args = args.Clone();
 
            // This function is useful when debugging the exporter, but in general we
            // want to just drop any export
        }

        private static wire.Timestamp convertTimestamp(time.Time t)
        {
            return t.Format(time.RFC3339Nano);
        }

        private static ptr<wire.TruncatableString> toTruncatableString(@string s)
        {
            if (s == "")
            {
                return _addr_null!;
            }

            return addr(new wire.TruncatableString(Value:s));

        }

        private static ptr<wire.Span> convertSpan(ptr<export.Span> _addr_span)
        {
            ref export.Span span = ref _addr_span.val;

            ptr<wire.Span> result = addr(new wire.Span(TraceID:span.ID.TraceID[:],SpanID:span.ID.SpanID[:],TraceState:nil,ParentSpanID:span.ParentID[:],Name:toTruncatableString(span.Name),Kind:wire.UnspecifiedSpanKind,StartTime:convertTimestamp(span.Start().At()),EndTime:convertTimestamp(span.Finish().At()),Attributes:convertAttributes(span.Start(),1),TimeEvents:convertEvents(span.Events()),SameProcessAsParentSpan:true,));
            return _addr_result!;
        }

        private static ptr<wire.Metric> convertMetric(metric.Data data, time.Time start)
        {
            var descriptor = dataToMetricDescriptor(data);
            var timeseries = dataToTimeseries(data, start);

            if (descriptor == null && timeseries == null)
            {
                return _addr_null!;
            } 

            // TODO: handle Histogram metrics
            return addr(new wire.Metric(MetricDescriptor:descriptor,Timeseries:timeseries,));

        }

        private static (long, label.Label) skipToValidLabel(label.List list, long index)
        {
            long _p0 = default;
            label.Label _p0 = default;
 
            // skip to the first valid label
            while (list.Valid(index))
            {
                var l = list.Label(index);
                if (!l.Valid() || l.Key() == keys.Label)
                {
                    continue;
                index++;
                }

                return (index, l);

            }

            return (-1L, new label.Label());

        }

        private static ptr<wire.Attributes> convertAttributes(label.List list, long index)
        {
            var (index, l) = skipToValidLabel(list, index);
            if (!l.Valid())
            {
                return _addr_null!;
            }

            var attributes = make_map<@string, wire.Attribute>();
            while (true)
            {
                if (l.Valid())
                {
                    attributes[l.Key().Name()] = convertAttribute(l);
                }

                index++;
                if (!list.Valid(index))
                {
                    return addr(new wire.Attributes(AttributeMap:attributes));
                }

                l = list.Label(index);

            }


        }

        private static wire.Attribute convertAttribute(label.Label l)
        {
            switch (l.Key().type())
            {
                case ptr<keys.Int> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.Int8> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.Int16> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.Int32> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.Int64> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.UInt> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.UInt8> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.UInt16> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.UInt32> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.UInt64> key:
                    return new wire.IntAttribute(IntValue:int64(key.From(l)));
                    break;
                case ptr<keys.Float32> key:
                    return new wire.DoubleAttribute(DoubleValue:float64(key.From(l)));
                    break;
                case ptr<keys.Float64> key:
                    return new wire.DoubleAttribute(DoubleValue:key.From(l));
                    break;
                case ptr<keys.Boolean> key:
                    return new wire.BoolAttribute(BoolValue:key.From(l));
                    break;
                case ptr<keys.String> key:
                    return new wire.StringAttribute(StringValue:toTruncatableString(key.From(l)));
                    break;
                case ptr<keys.Error> key:
                    return new wire.StringAttribute(StringValue:toTruncatableString(key.From(l).Error()));
                    break;
                case ptr<keys.Value> key:
                    return new wire.StringAttribute(StringValue:toTruncatableString(fmt.Sprint(key.From(l))));
                    break;
                default:
                {
                    var key = l.Key().type();
                    return new wire.StringAttribute(StringValue:toTruncatableString(fmt.Sprintf("%T",key)));
                    break;
                }
            }

        }

        private static ptr<wire.TimeEvents> convertEvents(slice<core.Event> events)
        { 
            //TODO: MessageEvents?
            var result = make_slice<wire.TimeEvent>(len(events));
            foreach (var (i, event) in events)
            {
                result[i] = convertEvent(event);
            }
            return addr(new wire.TimeEvents(TimeEvent:result));

        }

        private static wire.TimeEvent convertEvent(core.Event ev)
        {
            return new wire.TimeEvent(Time:convertTimestamp(ev.At()),Annotation:convertAnnotation(ev),);
        }

        private static (@string, long) getAnnotationDescription(core.Event ev)
        {
            @string _p0 = default;
            long _p0 = default;

            var l = ev.Label(0L);
            if (l.Key() != keys.Msg)
            {
                return ("", 0L);
            }

            {
                var msg = keys.Msg.From(l);

                if (msg != "")
                {
                    return (msg, 1L);
                }

            }

            l = ev.Label(1L);
            if (l.Key() != keys.Err)
            {
                return ("", 1L);
            }

            {
                var err = keys.Err.From(l);

                if (err != null)
                {
                    return (err.Error(), 2L);
                }

            }

            return ("", 2L);

        }

        private static ptr<wire.Annotation> convertAnnotation(core.Event ev)
        {
            var (description, index) = getAnnotationDescription(ev);
            {
                var (_, l) = skipToValidLabel(ev, index);

                if (!l.Valid() && description == "")
                {
                    return _addr_null!;
                }

            }

            return addr(new wire.Annotation(Description:toTruncatableString(description),Attributes:convertAttributes(ev,index),));

        }
    }
}}}}}}}
