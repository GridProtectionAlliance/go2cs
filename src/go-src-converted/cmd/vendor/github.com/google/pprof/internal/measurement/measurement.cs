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

// package measurement -- go2cs converted at 2022 March 13 06:36:45 UTC
// import "cmd/vendor/github.com/google/pprof/internal/measurement" ==> using measurement = go.cmd.vendor.github.com.google.pprof.@internal.measurement_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\measurement\measurement.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using fmt = fmt_package;
using math = math_package;
using strings = strings_package;
using time = time_package;

using profile = github.com.google.pprof.profile_package;


// ScaleProfiles updates the units in a set of profiles to make them
// compatible. It scales the profiles to the smallest unit to preserve
// data.

public static partial class measurement_package {

public static error ScaleProfiles(slice<ptr<profile.Profile>> profiles) {
    if (len(profiles) == 0) {
        return error.As(null!)!;
    }
    var periodTypes = make_slice<ptr<profile.ValueType>>(0, len(profiles));
    {
        var p__prev1 = p;

        foreach (var (_, __p) in profiles) {
            p = __p;
            if (p.PeriodType != null) {
                periodTypes = append(periodTypes, p.PeriodType);
            }
        }
        p = p__prev1;
    }

    var (periodType, err) = CommonValueType(periodTypes);
    if (err != null) {
        return error.As(fmt.Errorf("period type: %v", err))!;
    }
    var numSampleTypes = len(profiles[0].SampleType);
    {
        var p__prev1 = p;

        foreach (var (_, __p) in profiles[(int)1..]) {
            p = __p;
            if (numSampleTypes != len(p.SampleType)) {
                return error.As(fmt.Errorf("inconsistent samples type count: %d != %d", numSampleTypes, len(p.SampleType)))!;
            }
        }
        p = p__prev1;
    }

    var sampleType = make_slice<ptr<profile.ValueType>>(numSampleTypes);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < numSampleTypes; i++) {
            var sampleTypes = make_slice<ptr<profile.ValueType>>(len(profiles));
            {
                var p__prev2 = p;

                foreach (var (__j, __p) in profiles) {
                    j = __j;
                    p = __p;
                    sampleTypes[j] = p.SampleType[i];
                }
                p = p__prev2;
            }

            sampleType[i], err = CommonValueType(sampleTypes);
            if (err != null) {
                return error.As(fmt.Errorf("sample types: %v", err))!;
            }
        }

        i = i__prev1;
    }

    {
        var p__prev1 = p;

        foreach (var (_, __p) in profiles) {
            p = __p;
            if (p.PeriodType != null && periodType != null) {
                var (period, _) = Scale(p.Period, p.PeriodType.Unit, periodType.Unit);
                (p.Period, p.PeriodType.Unit) = (int64(period), periodType.Unit);
            }
            var ratios = make_slice<double>(len(p.SampleType));
            {
                nint i__prev2 = i;

                foreach (var (__i, __st) in p.SampleType) {
                    i = __i;
                    st = __st;
                    if (sampleType[i] == null) {
                        ratios[i] = 1;
                        continue;
                    }
                    ratios[i], _ = Scale(1, st.Unit, sampleType[i].Unit);
                    p.SampleType[i].Unit = sampleType[i].Unit;
                }
                i = i__prev2;
            }

            {
                var err = p.ScaleN(ratios);

                if (err != null) {
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
public static (ptr<profile.ValueType>, error) CommonValueType(slice<ptr<profile.ValueType>> ts) {
    ptr<profile.ValueType> _p0 = default!;
    error _p0 = default!;

    if (len(ts) <= 1) {
        return (_addr_null!, error.As(null!)!);
    }
    var minType = ts[0];
    foreach (var (_, t) in ts[(int)1..]) {
        if (!compatibleValueTypes(_addr_minType, _addr_t)) {
            return (_addr_null!, error.As(fmt.Errorf("incompatible types: %v %v", minType.val, t.val))!);
        }
        {
            var (ratio, _) = Scale(1, t.Unit, minType.Unit);

            if (ratio < 1) {
                minType = t;
            }

        }
    }    ref var rcopy = ref heap(minType.val, out ptr<var> _addr_rcopy);
    return (_addr__addr_rcopy!, error.As(null!)!);
}

private static bool compatibleValueTypes(ptr<profile.ValueType> _addr_v1, ptr<profile.ValueType> _addr_v2) {
    ref profile.ValueType v1 = ref _addr_v1.val;
    ref profile.ValueType v2 = ref _addr_v2.val;

    if (v1 == null || v2 == null) {
        return true; // No grounds to disqualify.
    }
    {
        var t1 = strings.TrimSuffix(v1.Type, "s");
        var t2 = strings.TrimSuffix(v2.Type, "s");

        if (t1 != t2) {
            return false;
        }
    }

    return v1.Unit == v2.Unit || (timeUnits.sniffUnit(v1.Unit) != null && timeUnits.sniffUnit(v2.Unit) != null) || (memoryUnits.sniffUnit(v1.Unit) != null && memoryUnits.sniffUnit(v2.Unit) != null) || (gcuUnits.sniffUnit(v1.Unit) != null && gcuUnits.sniffUnit(v2.Unit) != null);
}

// Scale a measurement from an unit to a different unit and returns
// the scaled value and the target unit. The returned target unit
// will be empty if uninteresting (could be skipped).
public static (double, @string) Scale(long value, @string fromUnit, @string toUnit) {
    double _p0 = default;
    @string _p0 = default;
 
    // Avoid infinite recursion on overflow.
    if (value < 0 && -value > 0) {
        var (v, u) = Scale(-value, fromUnit, toUnit);
        return (-v, u);
    }
    {
        var (m, u, ok) = memoryUnits.convertUnit(value, fromUnit, toUnit);

        if (ok) {
            return (m, u);
        }
    }
    {
        var (t, u, ok) = timeUnits.convertUnit(value, fromUnit, toUnit);

        if (ok) {
            return (t, u);
        }
    }
    {
        var (g, u, ok) = gcuUnits.convertUnit(value, fromUnit, toUnit);

        if (ok) {
            return (g, u);
        }
    } 
    // Skip non-interesting units.
    switch (toUnit) {
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
public static @string Label(long value, @string unit) {
    return ScaledLabel(value, unit, "auto");
}

// ScaledLabel scales the passed-in measurement (if necessary) and
// returns the label used to describe a float measurement.
public static @string ScaledLabel(long value, @string fromUnit, @string toUnit) {
    var (v, u) = Scale(value, fromUnit, toUnit);
    var sv = strings.TrimSuffix(fmt.Sprintf("%.2f", v), ".00");
    if (sv == "0" || sv == "-0") {
        return "0";
    }
    return sv + u;
}

// Percentage computes the percentage of total of a value, and encodes
// it as a string. At least two digits of precision are printed.
public static @string Percentage(long value, long total) {
    double ratio = default;
    if (total != 0) {
        ratio = math.Abs(float64(value) / float64(total)) * 100;
    }

    if (math.Abs(ratio) >= 99.95F && math.Abs(ratio) <= 100.05F) 
        return "  100%";
    else if (math.Abs(ratio) >= 1.0F) 
        return fmt.Sprintf("%5.2f%%", ratio);
    else 
        return fmt.Sprintf("%5.2g%%", ratio);
    }

// unit includes a list of aliases representing a specific unit and a factor
// which one can multiple a value in the specified unit by to get the value
// in terms of the base unit.
private partial struct unit {
    public @string canonicalName;
    public slice<@string> aliases;
    public double factor;
}

// unitType includes a list of units that are within the same category (i.e.
// memory or time units) and a default unit to use for this type of unit.
private partial struct unitType {
    public unit defaultUnit;
    public slice<unit> units;
}

// findByAlias returns the unit associated with the specified alias. It returns
// nil if the unit with such alias is not found.
private static ptr<unit> findByAlias(this unitType ut, @string alias) {
    foreach (var (_, u) in ut.units) {
        foreach (var (_, a) in u.aliases) {
            if (alias == a) {
                return _addr__addr_u!;
            }
        }
    }    return _addr_null!;
}

// sniffUnit simpifies the input alias and returns the unit associated with the
// specified alias. It returns nil if the unit with such alias is not found.
private static ptr<unit> sniffUnit(this unitType ut, @string unit) {
    unit = strings.ToLower(unit);
    if (len(unit) > 2) {
        unit = strings.TrimSuffix(unit, "s");
    }
    return _addr_ut.findByAlias(unit)!;
}

// autoScale takes in the value with units of the base unit and returns
// that value scaled to a reasonable unit if a reasonable unit is
// found.
private static (double, @string, bool) autoScale(this unitType ut, double value) {
    double _p0 = default;
    @string _p0 = default;
    bool _p0 = default;

    double f = default;
    @string unit = default;
    foreach (var (_, u) in ut.units) {
        if (u.factor >= f && (value / u.factor) >= 1.0F) {
            f = u.factor;
            unit = u.canonicalName;
        }
    }    if (f == 0) {
        return (0, "", false);
    }
    return (value / f, unit, true);
}

// convertUnit converts a value from the fromUnit to the toUnit, autoscaling
// the value if the toUnit is "minimum" or "auto". If the fromUnit is not
// included in the unitType, then a false boolean will be returned. If the
// toUnit is not in the unitType, the value will be returned in terms of the
// default unitType.
private static (double, @string, bool) convertUnit(this unitType ut, long value, @string fromUnitStr, @string toUnitStr) {
    double _p0 = default;
    @string _p0 = default;
    bool _p0 = default;

    var fromUnit = ut.sniffUnit(fromUnitStr);
    if (fromUnit == null) {
        return (0, "", false);
    }
    var v = float64(value) * fromUnit.factor;
    if (toUnitStr == "minimum" || toUnitStr == "auto") {
        {
            var v__prev2 = v;

            var (v, u, ok) = ut.autoScale(v);

            if (ok) {
                return (v, u, true);
            }

            v = v__prev2;

        }
        return (v / ut.defaultUnit.factor, ut.defaultUnit.canonicalName, true);
    }
    var toUnit = ut.sniffUnit(toUnitStr);
    if (toUnit == null) {
        return (v / ut.defaultUnit.factor, ut.defaultUnit.canonicalName, true);
    }
    return (v / toUnit.factor, toUnit.canonicalName, true);
}

private static unitType memoryUnits = new unitType(units:[]unit{{"B",[]string{"b","byte"},1},{"kB",[]string{"kb","kbyte","kilobyte"},float64(1<<10)},{"MB",[]string{"mb","mbyte","megabyte"},float64(1<<20)},{"GB",[]string{"gb","gbyte","gigabyte"},float64(1<<30)},{"TB",[]string{"tb","tbyte","terabyte"},float64(1<<40)},{"PB",[]string{"pb","pbyte","petabyte"},float64(1<<50)},},defaultUnit:unit{"B",[]string{"b","byte"},1},);

private static unitType timeUnits = new unitType(units:[]unit{{"ns",[]string{"ns","nanosecond"},float64(time.Nanosecond)},{"us",[]string{"μs","us","microsecond"},float64(time.Microsecond)},{"ms",[]string{"ms","millisecond"},float64(time.Millisecond)},{"s",[]string{"s","sec","second"},float64(time.Second)},{"hrs",[]string{"hour","hr"},float64(time.Hour)},},defaultUnit:unit{"s",[]string{},float64(time.Second)},);

private static unitType gcuUnits = new unitType(units:[]unit{{"n*GCU",[]string{"nanogcu"},1e-9},{"u*GCU",[]string{"microgcu"},1e-6},{"m*GCU",[]string{"milligcu"},1e-3},{"GCU",[]string{"gcu"},1},{"k*GCU",[]string{"kilogcu"},1e3},{"M*GCU",[]string{"megagcu"},1e6},{"G*GCU",[]string{"gigagcu"},1e9},{"T*GCU",[]string{"teragcu"},1e12},{"P*GCU",[]string{"petagcu"},1e15},},defaultUnit:unit{"GCU",[]string{},1.0},);

} // end measurement_package
