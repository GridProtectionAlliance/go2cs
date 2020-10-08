// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package eventtest supports logging events to a test.
// You can use NewContext to create a context that knows how to deliver
// telemetry events back to the test.
// You must use this context or a derived one anywhere you want telemetry to be
// correctly routed back to the test it was constructed with.
// Any events delivered to a background context will be dropped.
//
// Importing this package will cause it to register a new global telemetry
// exporter that understands the special contexts returned by NewContext.
// This means you should not import this package if you are not going to call
// NewContext.
// package eventtest -- go2cs converted at 2020 October 08 04:54:57 UTC
// import "golang.org/x/tools/internal/event/export/eventtest" ==> using eventtest = go.golang.org.x.tools.@internal.@event.export.eventtest_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\eventtest\eventtest.go
using bytes = go.bytes_package;
using context = go.context_package;
using sync = go.sync_package;
using testing = go.testing_package;

using @event = go.golang.org.x.tools.@internal.@event_package;
using core = go.golang.org.x.tools.@internal.@event.core_package;
using export = go.golang.org.x.tools.@internal.@event.export_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event {
namespace export
{
    public static partial class eventtest_package
    {
        private static void init()
        {
            ptr<testExporter> e = addr(new testExporter(buffer:&bytes.Buffer{}));
            e.logger = export.LogWriter(e.buffer, false);

            @event.SetExporter(export.Spans(e.processEvent));
        }

        private partial struct testingKeyType // : long
        {
        }

        private static readonly var testingKey = (var)testingKeyType(0L);

        // NewContext returns a context you should use for the active test.


        // NewContext returns a context you should use for the active test.
        public static context.Context NewContext(context.Context ctx, testing.TB t)
        {
            return context.WithValue(ctx, testingKey, t);
        }

        private partial struct testExporter
        {
            public sync.Mutex mu;
            public ptr<bytes.Buffer> buffer;
            public event.Exporter logger;
        }

        private static context.Context processEvent(this ptr<testExporter> _addr_w, context.Context ctx, core.Event ev, label.Map tm) => func((defer, _, __) =>
        {
            ref testExporter w = ref _addr_w.val;

            w.mu.Lock();
            defer(w.mu.Unlock()); 
            // build our log message in buffer
            var result = w.logger(ctx, ev, tm);
            var v = ctx.Value(testingKey); 
            // get the testing.TB
            if (w.buffer.Len() > 0L && v != null)
            {
                v._<testing.TB>().Log(w.buffer);
            }

            w.buffer.Truncate(0L);
            return result;

        });
    }
}}}}}}}
