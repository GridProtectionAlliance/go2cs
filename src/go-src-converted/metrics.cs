// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ocagent -- go2cs converted at 2020 October 09 06:01:49 UTC
// import "golang.org/x/tools/internal/event/export/ocagent" ==> using ocagent = go.golang.org.x.tools.@internal.@event.export.ocagent_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\ocagent\metrics.go
using time = go.time_package;

using metric = go.golang.org.x.tools.@internal.@event.export.metric_package;
using wire = go.golang.org.x.tools.@internal.@event.export.ocagent.wire_package;
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
    public static partial class ocagent_package
    {
        // dataToMetricDescriptor return a *wire.MetricDescriptor based on data.
        private static ptr<wire.MetricDescriptor> dataToMetricDescriptor(metric.Data data)
        {
            if (data == null)
            {
                return _addr_null!;
            }
            ptr<wire.MetricDescriptor> descriptor = addr(new wire.MetricDescriptor(Name:data.Handle(),Description:getDescription(data),Type:dataToMetricDescriptorType(data),LabelKeys:getLabelKeys(data),));

            return _addr_descriptor!;

        }

        // getDescription returns the description of data.
        private static @string getDescription(metric.Data data)
        {
            switch (data.type())
            {
                case ptr<metric.Int64Data> d:
                    return d.Info.Description;
                    break;
                case ptr<metric.Float64Data> d:
                    return d.Info.Description;
                    break;
                case ptr<metric.HistogramInt64Data> d:
                    return d.Info.Description;
                    break;
                case ptr<metric.HistogramFloat64Data> d:
                    return d.Info.Description;
                    break;

            }

            return "";

        }

        // getLabelKeys returns a slice of *wire.LabelKeys based on the keys
        // in data.
        private static slice<ptr<wire.LabelKey>> getLabelKeys(metric.Data data)
        {
            switch (data.type())
            {
                case ptr<metric.Int64Data> d:
                    return infoKeysToLabelKeys(d.Info.Keys);
                    break;
                case ptr<metric.Float64Data> d:
                    return infoKeysToLabelKeys(d.Info.Keys);
                    break;
                case ptr<metric.HistogramInt64Data> d:
                    return infoKeysToLabelKeys(d.Info.Keys);
                    break;
                case ptr<metric.HistogramFloat64Data> d:
                    return infoKeysToLabelKeys(d.Info.Keys);
                    break;

            }

            return null;

        }

        // dataToMetricDescriptorType returns a wire.MetricDescriptor_Type based on the
        // underlying type of data.
        private static wire.MetricDescriptor_Type dataToMetricDescriptorType(metric.Data data)
        {
            switch (data.type())
            {
                case ptr<metric.Int64Data> d:
                    if (d.IsGauge)
                    {
                        return wire.MetricDescriptor_GAUGE_INT64;
                    }

                    return wire.MetricDescriptor_CUMULATIVE_INT64;
                    break;
                case ptr<metric.Float64Data> d:
                    if (d.IsGauge)
                    {
                        return wire.MetricDescriptor_GAUGE_DOUBLE;
                    }

                    return wire.MetricDescriptor_CUMULATIVE_DOUBLE;
                    break;
                case ptr<metric.HistogramInt64Data> d:
                    return wire.MetricDescriptor_CUMULATIVE_DISTRIBUTION;
                    break;
                case ptr<metric.HistogramFloat64Data> d:
                    return wire.MetricDescriptor_CUMULATIVE_DISTRIBUTION;
                    break;

            }

            return wire.MetricDescriptor_UNSPECIFIED;

        }

        // dataToTimeseries returns a slice of *wire.TimeSeries based on the
        // points in data.
        private static slice<ptr<wire.TimeSeries>> dataToTimeseries(metric.Data data, time.Time start)
        {
            if (data == null)
            {
                return null;
            }

            var numRows = numRows(data);
            ref var startTimestamp = ref heap(convertTimestamp(start), out ptr<var> _addr_startTimestamp);
            var timeseries = make_slice<ptr<wire.TimeSeries>>(0L, numRows);

            for (long i = 0L; i < numRows; i++)
            {
                timeseries = append(timeseries, addr(new wire.TimeSeries(StartTimestamp:&startTimestamp,Points:dataToPoints(data,i),)));
            }


            return timeseries;

        }

        // numRows returns the number of rows in data.
        private static long numRows(metric.Data data)
        {
            switch (data.type())
            {
                case ptr<metric.Int64Data> d:
                    return len(d.Rows);
                    break;
                case ptr<metric.Float64Data> d:
                    return len(d.Rows);
                    break;
                case ptr<metric.HistogramInt64Data> d:
                    return len(d.Rows);
                    break;
                case ptr<metric.HistogramFloat64Data> d:
                    return len(d.Rows);
                    break;

            }

            return 0L;

        }

        // dataToPoints returns an array of *wire.Points based on the point(s)
        // in data at index i.
        private static slice<ptr<wire.Point>> dataToPoints(metric.Data data, long i)
        {
            switch (data.type())
            {
                case ptr<metric.Int64Data> d:
                    ref var timestamp = ref heap(convertTimestamp(d.EndTime), out ptr<var> _addr_timestamp);
                    return new slice<ptr<wire.Point>>(new ptr<wire.Point>[] { {Value:wire.PointInt64Value{Int64Value:d.Rows[i],},Timestamp:&timestamp,} });
                    break;
                case ptr<metric.Float64Data> d:
                    timestamp = convertTimestamp(d.EndTime);
                    return new slice<ptr<wire.Point>>(new ptr<wire.Point>[] { {Value:wire.PointDoubleValue{DoubleValue:d.Rows[i],},Timestamp:&timestamp,} });
                    break;
                case ptr<metric.HistogramInt64Data> d:
                    var row = d.Rows[i];
                    var bucketBounds = make_slice<double>(len(d.Info.Buckets));
                    foreach (var (i, val) in d.Info.Buckets)
                    {
                        bucketBounds[i] = float64(val);
                    }
                    return distributionToPoints(row.Values, row.Count, float64(row.Sum), bucketBounds, d.EndTime);
                    break;
                case ptr<metric.HistogramFloat64Data> d:
                    row = d.Rows[i];
                    return distributionToPoints(row.Values, row.Count, row.Sum, d.Info.Buckets, d.EndTime);
                    break;

            }

            return null;

        }

        // distributionToPoints returns an array of *wire.Points containing a
        // wire.PointDistributionValue representing a distribution with the
        // supplied counts, count, and sum.
        private static slice<ptr<wire.Point>> distributionToPoints(slice<long> counts, long count, double sum, slice<double> bucketBounds, time.Time end)
        {
            var buckets = make_slice<ptr<wire.Bucket>>(len(counts));
            for (long i = 0L; i < len(counts); i++)
            {
                buckets[i] = addr(new wire.Bucket(Count:counts[i],));
            }

            ref var timestamp = ref heap(convertTimestamp(end), out ptr<var> _addr_timestamp);
            return new slice<ptr<wire.Point>>(new ptr<wire.Point>[] { {Value:wire.PointDistributionValue{DistributionValue:&wire.DistributionValue{Count:count,Sum:sum,Buckets:buckets,BucketOptions:&wire.BucketOptionsExplicit{Bounds:bucketBounds,},},},Timestamp:&timestamp,} });

        }

        // infoKeysToLabelKeys returns an array of *wire.LabelKeys containing the
        // string values of the elements of labelKeys.
        private static slice<ptr<wire.LabelKey>> infoKeysToLabelKeys(slice<label.Key> infoKeys)
        {
            var labelKeys = make_slice<ptr<wire.LabelKey>>(0L, len(infoKeys));
            foreach (var (_, key) in infoKeys)
            {
                labelKeys = append(labelKeys, addr(new wire.LabelKey(Key:key.Name(),)));
            }
            return labelKeys;

        }
    }
}}}}}}}
