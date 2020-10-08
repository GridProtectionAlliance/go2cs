// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Package measurement export utility functions to manipulate/format performance profile sample values.
// package measurement -- go2cs converted at 2020 October 08 04:43:17 UTC
// import "cmd/vendor/github.com/google/pprof/internal/measurement" ==> using measurement = go.cmd.vendor.github.com.google.pprof.@internal.measurement_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\measurement\measurement.go
using fmt = go.fmt_package;
using math = go.math_package;
using strings = go.strings_package;
using time = go.time_package;

using profile = go.github.com.google.pprof.profile_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class measurement_package
    {
        // ScaleProfiles updates the units in a set of profiles to make them
        // compatible. It scales the profiles to the smallest unit to preserve
        // data.
        public static error ScaleProfiles(slice<ptr<profile.Profile>> profiles)
        {
            if (len(profiles) == 0L)
            {
                return error.As(null!)!;
            }
            var periodTypes = make_slice<ptr<profile.ValueType>>(0L, len(profiles));
            {
                var p__prev1 = p;

                foreach (var (_, __p) in profiles)
                {
                    p = __p;
                    if (p.PeriodType != null)
                    {
                        periodTypes = append(periodTypes, p.PeriodType);
                    }
                }
                p = p__prev1;
            }

            var (periodType, err) = CommonValueType(periodTypes);
            if (err != null)
            {
                return error.As(fmt.Errorf("period type: %v", err))!;
            }
            var numSampleTypes = len(profiles[0L].SampleType);
            {
                var p__prev1 = p;

                foreach (var (_, __p) in profiles[1L..])
                {
                    p = __p;
                    if (numSampleTypes != len(p.SampleType))
                    {
                        return error.As(fmt.Errorf("inconsistent samples type count: %d != %d", numSampleTypes, len(p.SampleType)))!;
                    }
                }
                p = p__prev1;
            }

            var sampleType = make_slice<ptr<profile.ValueType>>(numSampleTypes);
            {
                long i__prev1 = i;

                for (long i = 0L; i < numSampleTypes; i++)
                {
                    var sampleTypes = make_slice<ptr<profile.ValueType>>(len(profiles));
                    {
                        var p__prev2 = p;

                        foreach (var (__j, __p) in profiles)
                        {
                            j = __j;
                            p = __p;
                            sampleTypes[j] = p.SampleType[i];
                        }
                        p = p__prev2;
                    }

                    sampleType[i], err = CommonValueType(sampleTypes);
                    if (err != null)
                    {
                        return error.As(fmt.Errorf("sample types: %v", err))!;
                    }
                }

                i = i__prev1;
            }

            {
                var p__prev1 = p;

                foreach (var (_, __p) in profiles)
                {
                    p = __p;
                    if (p.PeriodType != null && periodType != null)
                    {
                        var (period, _) = Scale(p.Period, p.PeriodType.Unit, periodType.Unit);
                        p.Period = int64(period);
                        p.PeriodType.Unit = periodType.Unit;

                    }
                    var ratios = make_slice<double>(len(p.SampleType));
                    {
                        long i__prev2 = i;

                        foreach (var (__i, __st) in p.SampleType)
                        {
                            i = __i;
                            st = __st;
                            if (sampleType[i] == null)
                            {
                                ratios[i] = 1L;
                                continue;
                            }
                            ratios[i], _ = Scale(1L, st.Unit, sampleType[i].Unit);
                            p.SampleType[i].Unit = sampleType[i].Unit;

                        }
                        i = i__prev2;
                    }

                    {
                        var err = p.ScaleN(ratios);

                        if (err != null)
                        {
                            return error.As(fmt.Errorf("scale: %v", err))!;
                        }
                    }

                }
                p = p__prev1;
            }

            return error.As(null!)!;

        }

        // CommonValueType returns the finest type from a set of compatible
        // types.
        public static (ptr<profile.ValueType>, error) CommonValueType(slice<ptr<profile.ValueType>> ts)
        {
            ptr<profile.ValueType> _p0 = default!;
            error _p0 = default!;

            if (len(ts) <= 1L)
            {
                return (_addr_null!, error.As(null!)!);
            }

            var minType = ts[0L];
            foreach (var (_, t) in ts[1L..])
            {
                if (!compatibleValueTypes(_addr_minType, _addr_t))
                {
                    return (_addr_null!, error.As(fmt.Errorf("incompatible types: %v %v", minType.val, t.val))!);
                }

                {
                    var (ratio, _) = Scale(1L, t.Unit, minType.Unit);

                    if (ratio < 1L)
                    {
                        minType = t;
                    }

                }

            }
            ref var rcopy = ref heap(minType.val, out ptr<var> _addr_rcopy);
            return (_addr__addr_rcopy!, error.As(null!)!);

        }

        private static bool compatibleValueTypes(ptr<profile.ValueType> _addr_v1, ptr<profile.ValueType> _addr_v2)
        {
            ref profile.ValueType v1 = ref _addr_v1.val;
            ref profile.ValueType v2 = ref _addr_v2.val;

            if (v1 == null || v2 == null)
            {
                return true; // No grounds to disqualify.
            } 
            // Remove trailing 's' to permit minor mismatches.
            {
                var t1 = strings.TrimSuffix(v1.Type, "s");
                var t2 = strings.TrimSuffix(v2.Type, "s");

                if (t1 != t2)
                {
                    return false;
                }

            }


            return v1.Unit == v2.Unit || (isTimeUnit(v1.Unit) && isTimeUnit(v2.Unit)) || (isMemoryUnit(v1.Unit) && isMemoryUnit(v2.Unit));

        }

        // Scale a measurement from an unit to a different unit and returns
        // the scaled value and the target unit. The returned target unit
        // will be empty if uninteresting (could be skipped).
        public static (double, @string) Scale(long value, @string fromUnit, @string toUnit)
        {
            double _p0 = default;
            @string _p0 = default;
 
            // Avoid infinite recursion on overflow.
            if (value < 0L && -value > 0L)
            {
                var (v, u) = Scale(-value, fromUnit, toUnit);
                return (-v, u);
            }

            {
                var (m, u, ok) = memoryLabel(value, fromUnit, toUnit);

                if (ok)
                {
                    return (m, u);
                }

            }

            {
                var (t, u, ok) = timeLabel(value, fromUnit, toUnit);

                if (ok)
                {
                    return (t, u);
                } 
                // Skip non-interesting units.

            } 
            // Skip non-interesting units.
            switch (toUnit)
            {
                case "count": 

                case "sample": 

                case "unit": 

                case "minimum": 

                case "auto": 
                    return (float64(value), "");
                    break;
                default: 
                    return (float64(value), toUnit);
                    break;
            }

        }

        // Label returns the label used to describe a certain measurement.
        public static @string Label(long value, @string unit)
        {
            return ScaledLabel(value, unit, "auto");
        }

        // ScaledLabel scales the passed-in measurement (if necessary) and
        // returns the label used to describe a float measurement.
        public static @string ScaledLabel(long value, @string fromUnit, @string toUnit)
        {
            var (v, u) = Scale(value, fromUnit, toUnit);
            var sv = strings.TrimSuffix(fmt.Sprintf("%.2f", v), ".00");
            if (sv == "0" || sv == "-0")
            {
                return "0";
            }

            return sv + u;

        }

        // Percentage computes the percentage of total of a value, and encodes
        // it as a string. At least two digits of precision are printed.
        public static @string Percentage(long value, long total)
        {
            double ratio = default;
            if (total != 0L)
            {
                ratio = math.Abs(float64(value) / float64(total)) * 100L;
            }


            if (math.Abs(ratio) >= 99.95F && math.Abs(ratio) <= 100.05F) 
                return "  100%";
            else if (math.Abs(ratio) >= 1.0F) 
                return fmt.Sprintf("%5.2f%%", ratio);
            else 
                return fmt.Sprintf("%5.2g%%", ratio);
            
        }

        // isMemoryUnit returns whether a name is recognized as a memory size
        // unit.
        private static bool isMemoryUnit(@string unit)
        {
            switch (strings.TrimSuffix(strings.ToLower(unit), "s"))
            {
                case "byte": 

                case "b": 

                case "kilobyte": 

                case "kb": 

                case "megabyte": 

                case "mb": 

                case "gigabyte": 

                case "gb": 
                    return true;
                    break;
            }
            return false;

        }

        private static (double, @string, bool) memoryLabel(long value, @string fromUnit, @string toUnit)
        {
            double v = default;
            @string u = default;
            bool ok = default;

            fromUnit = strings.TrimSuffix(strings.ToLower(fromUnit), "s");
            toUnit = strings.TrimSuffix(strings.ToLower(toUnit), "s");

            switch (fromUnit)
            {
                case "byte": 

                case "b": 
                    break;
                case "kb": 

                case "kbyte": 

                case "kilobyte": 
                    value *= 1024L;
                    break;
                case "mb": 

                case "mbyte": 

                case "megabyte": 
                    value *= 1024L * 1024L;
                    break;
                case "gb": 

                case "gbyte": 

                case "gigabyte": 
                    value *= 1024L * 1024L * 1024L;
                    break;
                case "tb": 

                case "tbyte": 

                case "terabyte": 
                    value *= 1024L * 1024L * 1024L * 1024L;
                    break;
                case "pb": 

                case "pbyte": 

                case "petabyte": 
                    value *= 1024L * 1024L * 1024L * 1024L * 1024L;
                    break;
                default: 
                    return (0L, "", false);
                    break;
            }

            if (toUnit == "minimum" || toUnit == "auto")
            {

                if (value < 1024L) 
                    toUnit = "b";
                else if (value < 1024L * 1024L) 
                    toUnit = "kb";
                else if (value < 1024L * 1024L * 1024L) 
                    toUnit = "mb";
                else if (value < 1024L * 1024L * 1024L * 1024L) 
                    toUnit = "gb";
                else if (value < 1024L * 1024L * 1024L * 1024L * 1024L) 
                    toUnit = "tb";
                else 
                    toUnit = "pb";
                
            }

            double output = default;
            switch (toUnit)
            {
                case "kb": 

                case "kbyte": 

                case "kilobyte": 
                    output = float64(value) / 1024L;
                    toUnit = "kB";
                    break;
                case "mb": 

                case "mbyte": 

                case "megabyte": 
                    output = float64(value) / (1024L * 1024L);
                    toUnit = "MB";
                    break;
                case "gb": 

                case "gbyte": 

                case "gigabyte": 
                    output = float64(value) / (1024L * 1024L * 1024L);
                    toUnit = "GB";
                    break;
                case "tb": 

                case "tbyte": 

                case "terabyte": 
                    output = float64(value) / (1024L * 1024L * 1024L * 1024L);
                    toUnit = "TB";
                    break;
                case "pb": 

                case "pbyte": 

                case "petabyte": 
                    output = float64(value) / (1024L * 1024L * 1024L * 1024L * 1024L);
                    toUnit = "PB";
                    break;
                default: 
                    output = float64(value);
                    toUnit = "B";
                    break;
            }
            return (output, toUnit, true);

        }

        // isTimeUnit returns whether a name is recognized as a time unit.
        private static bool isTimeUnit(@string unit)
        {
            unit = strings.ToLower(unit);
            if (len(unit) > 2L)
            {
                unit = strings.TrimSuffix(unit, "s");
            }

            switch (unit)
            {
                case "nanosecond": 

                case "ns": 

                case "microsecond": 

                case "millisecond": 

                case "ms": 

                case "s": 

                case "second": 

                case "sec": 

                case "hr": 

                case "day": 

                case "week": 

                case "year": 
                    return true;
                    break;
            }
            return false;

        }

        private static (double, @string, bool) timeLabel(long value, @string fromUnit, @string toUnit)
        {
            double v = default;
            @string u = default;
            bool ok = default;

            fromUnit = strings.ToLower(fromUnit);
            if (len(fromUnit) > 2L)
            {
                fromUnit = strings.TrimSuffix(fromUnit, "s");
            }

            toUnit = strings.ToLower(toUnit);
            if (len(toUnit) > 2L)
            {
                toUnit = strings.TrimSuffix(toUnit, "s");
            }

            time.Duration d = default;
            switch (fromUnit)
            {
                case "nanosecond": 

                case "ns": 
                    d = time.Duration(value) * time.Nanosecond;
                    break;
                case "microsecond": 
                    d = time.Duration(value) * time.Microsecond;
                    break;
                case "millisecond": 

                case "ms": 
                    d = time.Duration(value) * time.Millisecond;
                    break;
                case "second": 

                case "sec": 

                case "s": 
                    d = time.Duration(value) * time.Second;
                    break;
                case "cycle": 
                    return (float64(value), "", true);
                    break;
                default: 
                    return (0L, "", false);
                    break;
            }

            if (toUnit == "minimum" || toUnit == "auto")
            {

                if (d < 1L * time.Microsecond) 
                    toUnit = "ns";
                else if (d < 1L * time.Millisecond) 
                    toUnit = "us";
                else if (d < 1L * time.Second) 
                    toUnit = "ms";
                else if (d < 1L * time.Minute) 
                    toUnit = "sec";
                else if (d < 1L * time.Hour) 
                    toUnit = "min";
                else if (d < 24L * time.Hour) 
                    toUnit = "hour";
                else if (d < 15L * 24L * time.Hour) 
                    toUnit = "day";
                else if (d < 120L * 24L * time.Hour) 
                    toUnit = "week";
                else 
                    toUnit = "year";
                
            }

            double output = default;
            var dd = float64(d);
            switch (toUnit)
            {
                case "ns": 

                case "nanosecond": 
                    output = dd / float64(time.Nanosecond);
                    toUnit = "ns";
                    break;
                case "us": 

                case "microsecond": 
                    output = dd / float64(time.Microsecond);
                    toUnit = "us";
                    break;
                case "ms": 

                case "millisecond": 
                    output = dd / float64(time.Millisecond);
                    toUnit = "ms";
                    break;
                case "min": 

                case "minute": 
                    output = dd / float64(time.Minute);
                    toUnit = "mins";
                    break;
                case "hour": 

                case "hr": 
                    output = dd / float64(time.Hour);
                    toUnit = "hrs";
                    break;
                case "day": 
                    output = dd / float64(24L * time.Hour);
                    toUnit = "days";
                    break;
                case "week": 

                case "wk": 
                    output = dd / float64(7L * 24L * time.Hour);
                    toUnit = "wks";
                    break;
                case "year": 

                case "yr": 
                    output = dd / float64(365L * 24L * time.Hour);
                    toUnit = "yrs";
                    break;
                default: 
                    // "sec", "second", "s" handled by default case.
                    output = dd / float64(time.Second);
                    toUnit = "s";
                    break;
            }
            return (output, toUnit, true);

        }
    }
}}}}}}}
