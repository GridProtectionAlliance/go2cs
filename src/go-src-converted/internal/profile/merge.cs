// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;

partial class profile_package {

// Merge merges all the profiles in profs into a single Profile.
// Returns a new profile independent of the input profiles. The merged
// profile is compacted to eliminate unused samples, locations,
// functions and mappings. Profiles must have identical profile sample
// and period types or the merge will fail. profile.Period of the
// resulting profile will be the maximum of all profiles, and
// profile.TimeNanos will be the earliest nonzero one.
public static (ж<Profile>, error) Merge(slice<ж<Profile>> srcs) {
    if (len(srcs) == 0) {
        return (default!, fmt.Errorf("no profiles to merge"u8));
    }
    (p, err) = combineHeaders(srcs);
    if (err != default!) {
        return (default!, err);
    }
    var pm = Ꮡ(new profileMerger(
        p: p,
        samples: new profile.Sample(len(srcs[0].Sample)),
        locations: new profile.Location(len(srcs[0].Location)),
        functions: new profile.Function(len(srcs[0].Function)),
        mappings: new profile.Mapping(len(srcs[0].Mapping))
    ));
    foreach (var (_, src) in srcs) {
        // Clear the profile-specific hash tables
        pm.val.locationsByID = new map<uint64, ж<Location>>(len((~src).Location));
        pm.val.functionsByID = new map<uint64, ж<Function>>(len((~src).Function));
        pm.val.mappingsByID = new map<uint64, mapInfo>(len((~src).Mapping));
        if (len((~pm).mappings) == 0 && len((~src).Mapping) > 0) {
            // The Mapping list has the property that the first mapping
            // represents the main binary. Take the first Mapping we see,
            // otherwise the operations below will add mappings in an
            // arbitrary order.
            pm.mapMapping((~src).Mapping[0]);
        }
        foreach (var (_, s) in (~src).Sample) {
            if (!isZeroSample(s)) {
                pm.mapSample(s);
            }
        }
    }
    foreach (var (_, s) in (~p).Sample) {
        if (isZeroSample(s)) {
            // If there are any zero samples, re-merge the profile to GC
            // them.
            return Merge(new ж<Profile>[]{p}.slice());
        }
    }
    return (p, default!);
}

// Normalize normalizes the source profile by multiplying each value in profile by the
// ratio of the sum of the base profile's values of that sample type to the sum of the
// source profile's value of that sample type.
[GoRecv] public static error Normalize(this ref Profile p, ж<Profile> Ꮡpb) {
    ref var pb = ref Ꮡpb.val;

    {
        var err = p.compatible(Ꮡpb); if (err != default!) {
            return err;
        }
    }
    var baseVals = new slice<int64>(len(p.SampleType));
    foreach (var (_, s) in pb.Sample) {
        foreach (var (i, v) in (~s).Value) {
            baseVals[i] += v;
        }
    }
    var srcVals = new slice<int64>(len(p.SampleType));
    foreach (var (_, s) in p.Sample) {
        foreach (var (i, v) in (~s).Value) {
            srcVals[i] += v;
        }
    }
    var normScale = new slice<float64>(len(baseVals));
    foreach (var (i, _) in baseVals) {
        if (srcVals[i] == 0){
            normScale[i] = 0.0F;
        } else {
            normScale[i] = ((float64)baseVals[i]) / ((float64)srcVals[i]);
        }
    }
    p.ScaleN(normScale);
    return default!;
}

internal static bool isZeroSample(ж<Sample> Ꮡs) {
    ref var s = ref Ꮡs.val;

    foreach (var (_, v) in s.Value) {
        if (v != 0) {
            return false;
        }
    }
    return true;
}

[GoType] partial struct profileMerger {
    internal ж<Profile> p;
    // Memoization tables within a profile.
    internal map<uint64, ж<Location>> locationsByID;
    internal map<uint64, ж<Function>> functionsByID;
    internal map<uint64, mapInfo> mappingsByID;
    // Memoization tables for profile entities.
    internal profile.Sample samples;
    internal profile.Location locations;
    internal profile.Function functions;
    internal profile.Mapping mappings;
}

[GoType] partial struct mapInfo {
    internal ж<Mapping> m;
    internal int64 offset;
}

[GoRecv] internal static ж<Sample> mapSample(this ref profileMerger pm, ж<Sample> Ꮡsrc) {
    ref var src = ref Ꮡsrc.val;

    var s = Ꮡ(new Sample(
        Location: new slice<ж<Location>>(len(src.Location)),
        Value: new slice<int64>(len(src.Value)),
        Label: new map<@string, slice<@string>>(len(src.Label)),
        NumLabel: new map<@string, slice<int64>>(len(src.NumLabel)),
        NumUnit: new map<@string, slice<@string>>(len(src.NumLabel))
    ));
    foreach (var (i, l) in src.Location) {
        (~s).Location[i] = pm.mapLocation(l);
    }
    foreach (var (kΔ1, v) in src.Label) {
        var vv = new slice<@string>(len(v));
        copy(vv, v);
        (~s).Label[kΔ1] = vv;
    }
    foreach (var (kΔ2, v) in src.NumLabel) {
        var u = src.NumUnit[kΔ2];
        var vv = new slice<int64>(len(v));
        var uu = new slice<@string>(len(u));
        copy(vv, v);
        copy(uu, u);
        (~s).NumLabel[kΔ2] = vv;
        (~s).NumUnit[kΔ2] = uu;
    }
    // Check memoization table. Must be done on the remapped location to
    // account for the remapped mapping. Add current values to the
    // existing sample.
    var k = s.key();
    {
        var ss = pm.samples[k];
        var ok = pm.samples[k]; if (ok) {
            foreach (var (i, v) in src.Value) {
                (~ss).Value[i] += v;
            }
            return ss;
        }
    }
    copy((~s).Value, src.Value);
    pm.samples[k] = s;
    pm.p.Sample = append(pm.p.Sample, s);
    return s;
}

// key generates sampleKey to be used as a key for maps.
[GoRecv] internal static sampleKey key(this ref Sample sample) {
    var ids = new slice<@string>(len(sample.Location));
    foreach (var (i, l) in sample.Location) {
        ids[i] = strconv.FormatUint((~l).ID, 16);
    }
    var labels = new slice<@string>(0, len(sample.Label));
    foreach (var (k, v) in sample.Label) {
        labels = append(labels, fmt.Sprintf("%q%q"u8, k, v));
    }
    sort.Strings(labels);
    var numlabels = new slice<@string>(0, len(sample.NumLabel));
    foreach (var (k, v) in sample.NumLabel) {
        numlabels = append(numlabels, fmt.Sprintf("%q%x%x"u8, k, v, sample.NumUnit[k]));
    }
    sort.Strings(numlabels);
    return new sampleKey(
        strings.Join(ids, "|"u8),
        strings.Join(labels, ""u8),
        strings.Join(numlabels, ""u8)
    );
}

[GoType] partial struct sampleKey {
    internal @string locations;
    internal @string labels;
    internal @string numlabels;
}

[GoRecv] internal static ж<Location> mapLocation(this ref profileMerger pm, ж<Location> Ꮡsrc) {
    ref var src = ref Ꮡsrc.val;

    if (src == nil) {
        return default!;
    }
    {
        var lΔ1 = pm.locationsByID[src.ID];
        var ok = pm.locationsByID[src.ID]; if (ok) {
            pm.locationsByID[src.ID] = lΔ1;
            return lΔ1;
        }
    }
    var mi = pm.mapMapping(src.Mapping);
    var l = Ꮡ(new Location(
        ID: ((uint64)(len(pm.p.Location) + 1)),
        Mapping: mi.m,
        Address: ((uint64)(((int64)src.Address) + mi.offset)),
        Line: new slice<Line>(len(src.Line)),
        IsFolded: src.IsFolded
    ));
    foreach (var (i, ln) in src.Line) {
        (~l).Line[i] = pm.mapLine(ln);
    }
    // Check memoization table. Must be done on the remapped location to
    // account for the remapped mapping ID.
    var k = l.key();
    {
        var ll = pm.locations[k];
        var ok = pm.locations[k]; if (ok) {
            pm.locationsByID[src.ID] = ll;
            return ll;
        }
    }
    pm.locationsByID[src.ID] = l;
    pm.locations[k] = l;
    pm.p.Location = append(pm.p.Location, l);
    return l;
}

// key generates locationKey to be used as a key for maps.
[GoRecv] internal static locationKey key(this ref Location l) {
    var key = new locationKey(
        addr: l.Address,
        isFolded: l.IsFolded
    );
    if (l.Mapping != nil) {
        // Normalizes address to handle address space randomization.
        key.addr -= l.Mapping.Start;
        key.mappingID = l.Mapping.ID;
    }
    var lines = new slice<@string>(len(l.Line) * 2);
    foreach (var (i, line) in l.Line) {
        if (line.Function != nil) {
            lines[i * 2] = strconv.FormatUint((~line.Function).ID, 16);
        }
        lines[i * 2 + 1] = strconv.FormatInt(line.Line, 16);
    }
    key.lines = strings.Join(lines, "|"u8);
    return key;
}

[GoType] partial struct locationKey {
    internal uint64 addr;
    internal uint64 mappingID;
    internal @string lines;
    internal bool isFolded;
}

[GoRecv] internal static mapInfo mapMapping(this ref profileMerger pm, ж<Mapping> Ꮡsrc) {
    ref var src = ref Ꮡsrc.val;

    if (src == nil) {
        return new mapInfo(nil);
    }
    {
        var (miΔ1, ok) = pm.mappingsByID[src.ID]; if (ok) {
            return miΔ1;
        }
    }
    // Check memoization tables.
    var mk = src.key();
    {
        var mΔ1 = pm.mappings[mk];
        var ok = pm.mappings[mk]; if (ok) {
            var miΔ2 = new mapInfo(mΔ1, ((int64)(~mΔ1).Start) - ((int64)src.Start));
            pm.mappingsByID[src.ID] = miΔ2;
            return miΔ2;
        }
    }
    var m = Ꮡ(new Mapping(
        ID: ((uint64)(len(pm.p.Mapping) + 1)),
        Start: src.Start,
        Limit: src.Limit,
        Offset: src.Offset,
        File: src.File,
        BuildID: src.BuildID,
        HasFunctions: src.HasFunctions,
        HasFilenames: src.HasFilenames,
        HasLineNumbers: src.HasLineNumbers,
        HasInlineFrames: src.HasInlineFrames
    ));
    pm.p.Mapping = append(pm.p.Mapping, m);
    // Update memoization tables.
    pm.mappings[mk] = m;
    var mi = new mapInfo(m, 0);
    pm.mappingsByID[src.ID] = mi;
    return mi;
}

// key generates encoded strings of Mapping to be used as a key for
// maps.
[GoRecv] internal static mappingKey key(this ref Mapping m) {
    // Normalize addresses to handle address space randomization.
    // Round up to next 4K boundary to avoid minor discrepancies.
    static readonly UntypedInt mapsizeRounding = /* 0x1000 */ 4096;
    var size = m.Limit - m.Start;
    size = size + mapsizeRounding - 1;
    size = size - (size % mapsizeRounding);
    var key = new mappingKey(
        size: size,
        offset: m.Offset
    );
    switch (ᐧ) {
    case {} when m.BuildID != ""u8: {
        key.buildIDOrFile = m.BuildID;
        break;
    }
    case {} when m.File != ""u8: {
        key.buildIDOrFile = m.File;
        break;
    }
    default: {
        break;
    }}

    // A mapping containing neither build ID nor file name is a fake mapping. A
    // key with empty buildIDOrFile is used for fake mappings so that they are
    // treated as the same mapping during merging.
    return key;
}

[GoType] partial struct mappingKey {
    internal uint64 size;
    internal uint64 offset;
    internal @string buildIDOrFile;
}

[GoRecv] internal static Line mapLine(this ref profileMerger pm, Line src) {
    var ln = new Line(
        Function: pm.mapFunction(src.Function),
        Line: src.Line
    );
    return ln;
}

[GoRecv] internal static ж<Function> mapFunction(this ref profileMerger pm, ж<Function> Ꮡsrc) {
    ref var src = ref Ꮡsrc.val;

    if (src == nil) {
        return default!;
    }
    {
        var fΔ1 = pm.functionsByID[src.ID];
        var ok = pm.functionsByID[src.ID]; if (ok) {
            return fΔ1;
        }
    }
    var k = src.key();
    {
        var fΔ2 = pm.functions[k];
        var ok = pm.functions[k]; if (ok) {
            pm.functionsByID[src.ID] = fΔ2;
            return fΔ2;
        }
    }
    var f = Ꮡ(new Function(
        ID: ((uint64)(len(pm.p.Function) + 1)),
        Name: src.Name,
        SystemName: src.SystemName,
        Filename: src.Filename,
        StartLine: src.StartLine
    ));
    pm.functions[k] = f;
    pm.functionsByID[src.ID] = f;
    pm.p.Function = append(pm.p.Function, f);
    return f;
}

// key generates a struct to be used as a key for maps.
[GoRecv] internal static functionKey key(this ref Function f) {
    return new functionKey(
        f.StartLine,
        f.Name,
        f.SystemName,
        f.Filename
    );
}

[GoType] partial struct functionKey {
    internal int64 startLine;
    internal @string name;
    internal @string systemName;
    internal @string fileName;
}

// combineHeaders checks that all profiles can be merged and returns
// their combined profile.
internal static (ж<Profile>, error) combineHeaders(slice<ж<Profile>> srcs) {
    foreach (var (_, s) in srcs[1..]) {
        {
            var err = srcs[0].compatible(s); if (err != default!) {
                return (default!, err);
            }
        }
    }
    ref var timeNanos = ref heap(new int64(), out var ᏑtimeNanos);
    ref var durationNanos = ref heap(new int64(), out var ᏑdurationNanos);
    ref var period = ref heap(new int64(), out var Ꮡperiod);
    slice<@string> comments = default!;
    var seenComments = new map<@string, bool>{};
    ref var defaultSampleType = ref heap(new @string(), out var ᏑdefaultSampleType);
    foreach (var (_, s) in srcs) {
        if (timeNanos == 0 || (~s).TimeNanos < timeNanos) {
            timeNanos = s.val.TimeNanos;
        }
        durationNanos += s.val.DurationNanos;
        if (period == 0 || period < (~s).Period) {
            period = s.val.Period;
        }
        foreach (var (_, c) in (~s).Comments) {
            {
                var seen = seenComments[c]; if (!seen) {
                    comments = append(comments, c);
                    seenComments[c] = true;
                }
            }
        }
        if (defaultSampleType == ""u8) {
            defaultSampleType = s.val.DefaultSampleType;
        }
    }
    var p = Ꮡ(new Profile(
        SampleType: new slice<ж<ValueType>>(len(srcs[0].SampleType)),
        DropFrames: srcs[0].DropFrames,
        KeepFrames: srcs[0].KeepFrames,
        TimeNanos: timeNanos,
        DurationNanos: durationNanos,
        PeriodType: srcs[0].PeriodType,
        Period: period,
        Comments: comments,
        DefaultSampleType: defaultSampleType
    ));
    copy((~p).SampleType, srcs[0].SampleType);
    return (p, default!);
}

// compatible determines if two profiles can be compared/merged.
// returns nil if the profiles are compatible; otherwise an error with
// details on the incompatibility.
[GoRecv] public static error compatible(this ref Profile p, ж<Profile> Ꮡpb) {
    ref var pb = ref Ꮡpb.val;

    if (!equalValueType(p.PeriodType, pb.PeriodType)) {
        return fmt.Errorf("incompatible period types %v and %v"u8, p.PeriodType, pb.PeriodType);
    }
    if (len(p.SampleType) != len(pb.SampleType)) {
        return fmt.Errorf("incompatible sample types %v and %v"u8, p.SampleType, pb.SampleType);
    }
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in p.SampleType) {
        if (!equalValueType(p.SampleType[i], pb.SampleType[i])) {
            return fmt.Errorf("incompatible sample types %v and %v"u8, p.SampleType, pb.SampleType);
        }
    }
    return default!;
}

// equalValueType returns true if the two value types are semantically
// equal. It ignores the internal fields used during encode/decode.
internal static bool equalValueType(ж<ValueType> Ꮡst1, ж<ValueType> Ꮡst2) {
    ref var st1 = ref Ꮡst1.val;
    ref var st2 = ref Ꮡst2.val;

    return st1.Type == st2.Type && st1.Unit == st2.Unit;
}

} // end profile_package
