// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package metric -- go2cs converted at 2020 October 08 04:55:00 UTC
// import "golang.org/x/tools/internal/event/export/metric" ==> using metric = go.golang.org.x.tools.@internal.@event.export.metric_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\metric\info.go
using keys = go.golang.org.x.tools.@internal.@event.keys_package;
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
    public static partial class metric_package
    {
        // Scalar represents the construction information for a scalar metric.
        public partial struct Scalar
        {
            public @string Name; // Description can be used by observers to describe the metric to users.
            public @string Description; // Keys is the set of labels that collectively describe rows of the metric.
            public slice<label.Key> Keys;
        }

        // HistogramInt64 represents the construction information for an int64 histogram metric.
        public partial struct HistogramInt64
        {
            public @string Name; // Description can be used by observers to describe the metric to users.
            public @string Description; // Keys is the set of labels that collectively describe rows of the metric.
            public slice<label.Key> Keys; // Buckets holds the inclusive upper bound of each bucket in the histogram.
            public slice<long> Buckets;
        }

        // HistogramFloat64 represents the construction information for an float64 histogram metric.
        public partial struct HistogramFloat64
        {
            public @string Name; // Description can be used by observers to describe the metric to users.
            public @string Description; // Keys is the set of labels that collectively describe rows of the metric.
            public slice<label.Key> Keys; // Buckets holds the inclusive upper bound of each bucket in the histogram.
            public slice<double> Buckets;
        }

        // Count creates a new metric based on the Scalar information that counts
        // the number of times the supplied int64 measure is set.
        // Metrics of this type will use Int64Data.
        public static void Count(this Scalar info, ptr<Config> _addr_e, label.Key key)
        {
            ref Config e = ref _addr_e.val;

            ptr<Int64Data> data = addr(new Int64Data(Info:&info,key:nil));
            e.subscribe(key, data.count);
        }

        // SumInt64 creates a new metric based on the Scalar information that sums all
        // the values recorded on the int64 measure.
        // Metrics of this type will use Int64Data.
        public static void SumInt64(this Scalar info, ptr<Config> _addr_e, ptr<keys.Int64> _addr_key)
        {
            ref Config e = ref _addr_e.val;
            ref keys.Int64 key = ref _addr_key.val;

            ptr<Int64Data> data = addr(new Int64Data(Info:&info,key:key));
            e.subscribe(key, data.sum);
        }

        // LatestInt64 creates a new metric based on the Scalar information that tracks
        // the most recent value recorded on the int64 measure.
        // Metrics of this type will use Int64Data.
        public static void LatestInt64(this Scalar info, ptr<Config> _addr_e, ptr<keys.Int64> _addr_key)
        {
            ref Config e = ref _addr_e.val;
            ref keys.Int64 key = ref _addr_key.val;

            ptr<Int64Data> data = addr(new Int64Data(Info:&info,IsGauge:true,key:key));
            e.subscribe(key, data.latest);
        }

        // SumFloat64 creates a new metric based on the Scalar information that sums all
        // the values recorded on the float64 measure.
        // Metrics of this type will use Float64Data.
        public static void SumFloat64(this Scalar info, ptr<Config> _addr_e, ptr<keys.Float64> _addr_key)
        {
            ref Config e = ref _addr_e.val;
            ref keys.Float64 key = ref _addr_key.val;

            ptr<Float64Data> data = addr(new Float64Data(Info:&info,key:key));
            e.subscribe(key, data.sum);
        }

        // LatestFloat64 creates a new metric based on the Scalar information that tracks
        // the most recent value recorded on the float64 measure.
        // Metrics of this type will use Float64Data.
        public static void LatestFloat64(this Scalar info, ptr<Config> _addr_e, ptr<keys.Float64> _addr_key)
        {
            ref Config e = ref _addr_e.val;
            ref keys.Float64 key = ref _addr_key.val;

            ptr<Float64Data> data = addr(new Float64Data(Info:&info,IsGauge:true,key:key));
            e.subscribe(key, data.latest);
        }

        // Record creates a new metric based on the HistogramInt64 information that
        // tracks the bucketized counts of values recorded on the int64 measure.
        // Metrics of this type will use HistogramInt64Data.
        public static void Record(this HistogramInt64 info, ptr<Config> _addr_e, ptr<keys.Int64> _addr_key)
        {
            ref Config e = ref _addr_e.val;
            ref keys.Int64 key = ref _addr_key.val;

            ptr<HistogramInt64Data> data = addr(new HistogramInt64Data(Info:&info,key:key));
            e.subscribe(key, data.record);
        }

        // Record creates a new metric based on the HistogramFloat64 information that
        // tracks the bucketized counts of values recorded on the float64 measure.
        // Metrics of this type will use HistogramFloat64Data.
        public static void Record(this HistogramFloat64 info, ptr<Config> _addr_e, ptr<keys.Float64> _addr_key)
        {
            ref Config e = ref _addr_e.val;
            ref keys.Float64 key = ref _addr_key.val;

            ptr<HistogramFloat64Data> data = addr(new HistogramFloat64Data(Info:&info,key:key));
            e.subscribe(key, data.record);
        }
    }
}}}}}}}
