// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package metric -- go2cs converted at 2020 October 09 06:01:48 UTC
// import "golang.org/x/tools/internal/event/export/metric" ==> using metric = go.golang.org.x.tools.@internal.@event.export.metric_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\metric\data.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using time = go.time_package;

using keys = go.golang.org.x.tools.@internal.@event.keys_package;
using label = go.golang.org.x.tools.@internal.@event.label_package;
using static go.builtin;
using System;

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
        // Data represents a single point in the time series of a metric.
        // This provides the common interface to all metrics no matter their data
        // format.
        // To get the actual values for the metric you must type assert to a concrete
        // metric type.
        public partial interface Data
        {
            slice<slice<label.Label>> Handle(); // Groups reports the rows that currently exist for this metric.
            slice<slice<label.Label>> Groups();
        }

        // Int64Data is a concrete implementation of Data for int64 scalar metrics.
        public partial struct Int64Data
        {
            public ptr<Scalar> Info; // IsGauge is true for metrics that track values, rather than increasing over time.
            public bool IsGauge; // Rows holds the per group values for the metric.
            public slice<long> Rows; // End is the last time this metric was updated.
            public time.Time EndTime;
            public slice<slice<label.Label>> groups;
            public ptr<keys.Int64> key;
        }

        // Float64Data is a concrete implementation of Data for float64 scalar metrics.
        public partial struct Float64Data
        {
            public ptr<Scalar> Info; // IsGauge is true for metrics that track values, rather than increasing over time.
            public bool IsGauge; // Rows holds the per group values for the metric.
            public slice<double> Rows; // End is the last time this metric was updated.
            public time.Time EndTime;
            public slice<slice<label.Label>> groups;
            public ptr<keys.Float64> key;
        }

        // HistogramInt64Data is a concrete implementation of Data for int64 histogram metrics.
        public partial struct HistogramInt64Data
        {
            public ptr<HistogramInt64> Info; // Rows holds the per group values for the metric.
            public slice<ptr<HistogramInt64Row>> Rows; // End is the last time this metric was updated.
            public time.Time EndTime;
            public slice<slice<label.Label>> groups;
            public ptr<keys.Int64> key;
        }

        // HistogramInt64Row holds the values for a single row of a HistogramInt64Data.
        public partial struct HistogramInt64Row
        {
            public slice<long> Values; // Count is the total count.
            public long Count; // Sum is the sum of all the values recorded.
            public long Sum; // Min is the smallest recorded value.
            public long Min; // Max is the largest recorded value.
            public long Max;
        }

        // HistogramFloat64Data is a concrete implementation of Data for float64 histogram metrics.
        public partial struct HistogramFloat64Data
        {
            public ptr<HistogramFloat64> Info; // Rows holds the per group values for the metric.
            public slice<ptr<HistogramFloat64Row>> Rows; // End is the last time this metric was updated.
            public time.Time EndTime;
            public slice<slice<label.Label>> groups;
            public ptr<keys.Float64> key;
        }

        // HistogramFloat64Row holds the values for a single row of a HistogramFloat64Data.
        public partial struct HistogramFloat64Row
        {
            public slice<long> Values; // Count is the total count.
            public long Count; // Sum is the sum of all the values recorded.
            public double Sum; // Min is the smallest recorded value.
            public double Min; // Max is the largest recorded value.
            public double Max;
        }

        private static bool labelListEqual(slice<label.Label> a, slice<label.Label> b)
        { 
            //TODO: make this more efficient
            return fmt.Sprint(a) == fmt.Sprint(b);

        }

        private static bool labelListLess(slice<label.Label> a, slice<label.Label> b)
        { 
            //TODO: make this more efficient
            return fmt.Sprint(a) < fmt.Sprint(b);

        }

        private static (long, bool) getGroup(label.Map lm, ptr<slice<slice<label.Label>>> _addr_g, slice<label.Key> keys)
        {
            long _p0 = default;
            bool _p0 = default;
            ref slice<slice<label.Label>> g = ref _addr_g.val;

            var group = make_slice<label.Label>(len(keys));
            foreach (var (i, key) in keys)
            {
                var l = lm.Find(key);
                if (l.Valid())
                {
                    group[i] = l;
                }

            }
            slice<slice<label.Label>> old = g;
            var index = sort.Search(len(old), i =>
            {
                return !labelListLess(old[i], group);
            });
            if (index < len(old) && labelListEqual(group, old[index]))
            { 
                // not a new group
                return (index, false);

            }

            g = make_slice<slice<label.Label>>(len(old) + 1L);
            copy(g, old[..index]);
            copy((g)[index + 1L..], old[index..]);
            (g)[index] = group;
            return (index, true);

        }

        private static @string Handle(this ptr<Int64Data> _addr_data)
        {
            ref Int64Data data = ref _addr_data.val;

            return data.Info.Name;
        }
        private static slice<slice<label.Label>> Groups(this ptr<Int64Data> _addr_data)
        {
            ref Int64Data data = ref _addr_data.val;

            return data.groups;
        }

        private static Data modify(this ptr<Int64Data> _addr_data, time.Time at, label.Map lm, Func<long, long> f)
        {
            ref Int64Data data = ref _addr_data.val;

            var (index, insert) = getGroup(lm, _addr_data.groups, data.Info.Keys);
            var old = data.Rows;
            if (insert)
            {
                data.Rows = make_slice<long>(len(old) + 1L);
                copy(data.Rows, old[..index]);
                copy(data.Rows[index + 1L..], old[index..]);
            }
            else
            {
                data.Rows = make_slice<long>(len(old));
                copy(data.Rows, old);
            }

            data.Rows[index] = f(data.Rows[index]);
            data.EndTime = at;
            ref var frozen = ref heap(data.val, out ptr<var> _addr_frozen);
            return _addr_frozen;

        }

        private static Data count(this ptr<Int64Data> _addr_data, time.Time at, label.Map lm, label.Label l)
        {
            ref Int64Data data = ref _addr_data.val;

            return data.modify(at, lm, v =>
            {
                return v + 1L;
            });

        }

        private static Data sum(this ptr<Int64Data> _addr_data, time.Time at, label.Map lm, label.Label l)
        {
            ref Int64Data data = ref _addr_data.val;

            return data.modify(at, lm, v =>
            {
                return v + data.key.From(l);
            });

        }

        private static Data latest(this ptr<Int64Data> _addr_data, time.Time at, label.Map lm, label.Label l)
        {
            ref Int64Data data = ref _addr_data.val;

            return data.modify(at, lm, v =>
            {
                return data.key.From(l);
            });

        }

        private static @string Handle(this ptr<Float64Data> _addr_data)
        {
            ref Float64Data data = ref _addr_data.val;

            return data.Info.Name;
        }
        private static slice<slice<label.Label>> Groups(this ptr<Float64Data> _addr_data)
        {
            ref Float64Data data = ref _addr_data.val;

            return data.groups;
        }

        private static Data modify(this ptr<Float64Data> _addr_data, time.Time at, label.Map lm, Func<double, double> f)
        {
            ref Float64Data data = ref _addr_data.val;

            var (index, insert) = getGroup(lm, _addr_data.groups, data.Info.Keys);
            var old = data.Rows;
            if (insert)
            {
                data.Rows = make_slice<double>(len(old) + 1L);
                copy(data.Rows, old[..index]);
                copy(data.Rows[index + 1L..], old[index..]);
            }
            else
            {
                data.Rows = make_slice<double>(len(old));
                copy(data.Rows, old);
            }

            data.Rows[index] = f(data.Rows[index]);
            data.EndTime = at;
            ref var frozen = ref heap(data.val, out ptr<var> _addr_frozen);
            return _addr_frozen;

        }

        private static Data sum(this ptr<Float64Data> _addr_data, time.Time at, label.Map lm, label.Label l)
        {
            ref Float64Data data = ref _addr_data.val;

            return data.modify(at, lm, v =>
            {
                return v + data.key.From(l);
            });

        }

        private static Data latest(this ptr<Float64Data> _addr_data, time.Time at, label.Map lm, label.Label l)
        {
            ref Float64Data data = ref _addr_data.val;

            return data.modify(at, lm, v =>
            {
                return data.key.From(l);
            });

        }

        private static @string Handle(this ptr<HistogramInt64Data> _addr_data)
        {
            ref HistogramInt64Data data = ref _addr_data.val;

            return data.Info.Name;
        }
        private static slice<slice<label.Label>> Groups(this ptr<HistogramInt64Data> _addr_data)
        {
            ref HistogramInt64Data data = ref _addr_data.val;

            return data.groups;
        }

        private static Data modify(this ptr<HistogramInt64Data> _addr_data, time.Time at, label.Map lm, Action<ptr<HistogramInt64Row>> f)
        {
            ref HistogramInt64Data data = ref _addr_data.val;

            var (index, insert) = getGroup(lm, _addr_data.groups, data.Info.Keys);
            var old = data.Rows;
            ref HistogramInt64Row v = ref heap(out ptr<HistogramInt64Row> _addr_v);
            if (insert)
            {
                data.Rows = make_slice<ptr<HistogramInt64Row>>(len(old) + 1L);
                copy(data.Rows, old[..index]);
                copy(data.Rows[index + 1L..], old[index..]);
            }
            else
            {
                data.Rows = make_slice<ptr<HistogramInt64Row>>(len(old));
                copy(data.Rows, old);
                v = data.Rows[index].val;
            }

            var oldValues = v.Values;
            v.Values = make_slice<long>(len(data.Info.Buckets));
            copy(v.Values, oldValues);
            f(_addr_v);
            _addr_data.Rows[index] = _addr_v;
            data.Rows[index] = ref _addr_data.Rows[index].val;
            data.EndTime = at;
            ref var frozen = ref heap(data.val, out ptr<var> _addr_frozen);
            return _addr_frozen;

        }

        private static Data record(this ptr<HistogramInt64Data> _addr_data, time.Time at, label.Map lm, label.Label l)
        {
            ref HistogramInt64Data data = ref _addr_data.val;

            return data.modify(at, lm, v =>
            {
                var value = data.key.From(l);
                v.Sum += value;
                if (v.Min > value || v.Count == 0L)
                {
                    v.Min = value;
                }

                if (v.Max < value || v.Count == 0L)
                {
                    v.Max = value;
                }

                v.Count++;
                foreach (var (i, b) in data.Info.Buckets)
                {
                    if (value <= b)
                    {
                        v.Values[i]++;
                    }

                }

            });

        }

        private static @string Handle(this ptr<HistogramFloat64Data> _addr_data)
        {
            ref HistogramFloat64Data data = ref _addr_data.val;

            return data.Info.Name;
        }
        private static slice<slice<label.Label>> Groups(this ptr<HistogramFloat64Data> _addr_data)
        {
            ref HistogramFloat64Data data = ref _addr_data.val;

            return data.groups;
        }

        private static Data modify(this ptr<HistogramFloat64Data> _addr_data, time.Time at, label.Map lm, Action<ptr<HistogramFloat64Row>> f)
        {
            ref HistogramFloat64Data data = ref _addr_data.val;

            var (index, insert) = getGroup(lm, _addr_data.groups, data.Info.Keys);
            var old = data.Rows;
            ref HistogramFloat64Row v = ref heap(out ptr<HistogramFloat64Row> _addr_v);
            if (insert)
            {
                data.Rows = make_slice<ptr<HistogramFloat64Row>>(len(old) + 1L);
                copy(data.Rows, old[..index]);
                copy(data.Rows[index + 1L..], old[index..]);
            }
            else
            {
                data.Rows = make_slice<ptr<HistogramFloat64Row>>(len(old));
                copy(data.Rows, old);
                v = data.Rows[index].val;
            }

            var oldValues = v.Values;
            v.Values = make_slice<long>(len(data.Info.Buckets));
            copy(v.Values, oldValues);
            f(_addr_v);
            _addr_data.Rows[index] = _addr_v;
            data.Rows[index] = ref _addr_data.Rows[index].val;
            data.EndTime = at;
            ref var frozen = ref heap(data.val, out ptr<var> _addr_frozen);
            return _addr_frozen;

        }

        private static Data record(this ptr<HistogramFloat64Data> _addr_data, time.Time at, label.Map lm, label.Label l)
        {
            ref HistogramFloat64Data data = ref _addr_data.val;

            return data.modify(at, lm, v =>
            {
                var value = data.key.From(l);
                v.Sum += value;
                if (v.Min > value || v.Count == 0L)
                {
                    v.Min = value;
                }

                if (v.Max < value || v.Count == 0L)
                {
                    v.Max = value;
                }

                v.Count++;
                foreach (var (i, b) in data.Info.Buckets)
                {
                    if (value <= b)
                    {
                        v.Values[i]++;
                    }

                }

            });

        }
    }
}}}}}}}
