// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package profile provides a representation of
// github.com/google/pprof/proto/profile.proto and
// methods to encode/decode/merge profiles in this format.
namespace go.@internal;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using time = time_package;
using compress;

partial class profile_package {

// Profile is an in-memory representation of profile.proto.
[GoType] partial struct Profile {
    public slice<ж<ValueType>> SampleType;
    public @string DefaultSampleType;
    public slice<ж<Sample>> Sample;
    public slice<ж<Mapping>> Mapping;
    public slice<ж<Location>> Location;
    public slice<ж<Function>> Function;
    public slice<@string> Comments;
    public @string DropFrames;
    public @string KeepFrames;
    public int64 TimeNanos;
    public int64 DurationNanos;
    public ж<ValueType> PeriodType;
    public int64 Period;
    internal slice<int64> commentX;
    internal int64 dropFramesX;
    internal int64 keepFramesX;
    internal slice<@string> stringTable;
    internal int64 defaultSampleTypeX;
}

// ValueType corresponds to Profile.ValueType
[GoType] partial struct ValueType {
    public @string Type; // cpu, wall, inuse_space, etc
    public @string Unit; // seconds, nanoseconds, bytes, etc
    internal int64 typeX;
    internal int64 unitX;
}

// Sample corresponds to Profile.Sample
[GoType] partial struct Sample {
    public slice<ж<Location>> Location;
    public slice<int64> Value;
    public map<@string, slice<@string>> Label;
    public map<@string, slice<int64>> NumLabel;
    public map<@string, slice<@string>> NumUnit;
    internal slice<uint64> locationIDX;
    internal slice<Label> labelX;
}

// Label corresponds to Profile.Label
[GoType] partial struct Label {
    internal int64 keyX;
    // Exactly one of the two following values must be set
    internal int64 strX;
    internal int64 numX; // Integer value for this label
}

// Mapping corresponds to Profile.Mapping
[GoType] partial struct Mapping {
    public uint64 ID;
    public uint64 Start;
    public uint64 Limit;
    public uint64 Offset;
    public @string File;
    public @string BuildID;
    public bool HasFunctions;
    public bool HasFilenames;
    public bool HasLineNumbers;
    public bool HasInlineFrames;
    internal int64 fileX;
    internal int64 buildIDX;
}

// Location corresponds to Profile.Location
[GoType] partial struct Location {
    public uint64 ID;
    public ж<Mapping> Mapping;
    public uint64 Address;
    public slice<Line> Line;
    public bool IsFolded;
    internal uint64 mappingIDX;
}

// Line corresponds to Profile.Line
[GoType] partial struct Line {
    public ж<Function> Function;
    public int64 Line;
    internal uint64 functionIDX;
}

// Function corresponds to Profile.Function
[GoType] partial struct Function {
    public uint64 ID;
    public @string Name;
    public @string SystemName;
    public @string Filename;
    public int64 StartLine;
    internal int64 nameX;
    internal int64 systemNameX;
    internal int64 filenameX;
}

// Parse parses a profile and checks for its validity. The input must be an
// encoded pprof protobuf, which may optionally be gzip-compressed.
public static (ж<Profile>, error) Parse(io.Reader r) {
    (orig, err) = io.ReadAll(r);
    if (err != default!) {
        return (default!, err);
    }
    if (len(orig) >= 2 && orig[0] == 31 && orig[1] == 139) {
        (gz, errΔ1) = gzip.NewReader(~bytes.NewBuffer(orig));
        if (errΔ1 != default!) {
            return (default!, fmt.Errorf("decompressing profile: %v"u8, errΔ1));
        }
        (data, err) = io.ReadAll(~gz);
        if (errΔ1 != default!) {
            return (default!, fmt.Errorf("decompressing profile: %v"u8, errΔ1));
        }
        orig = data;
    }
    (p, err) = parseUncompressed(orig);
    if (err != default!) {
        return (default!, fmt.Errorf("parsing profile: %w"u8, err));
    }
    {
        var errΔ2 = p.CheckValid(); if (errΔ2 != default!) {
            return (default!, fmt.Errorf("malformed profile: %v"u8, errΔ2));
        }
    }
    return (p, default!);
}

internal static error errMalformed = fmt.Errorf("malformed profile format"u8);

public static error ErrNoData = fmt.Errorf("empty input file"u8);

internal static (ж<Profile>, error) parseUncompressed(slice<byte> data) {
    if (len(data) == 0) {
        return (default!, ErrNoData);
    }
    var p = Ꮡ(new Profile(nil));
    {
        var err = unmarshal(data, ~p); if (err != default!) {
            return (default!, err);
        }
    }
    {
        var err = p.postDecode(); if (err != default!) {
            return (default!, err);
        }
    }
    return (p, default!);
}

// Write writes the profile as a gzip-compressed marshaled protobuf.
[GoRecv] public static error Write(this ref Profile p, io.Writer w) => func((defer, _) => {
    p.preEncode();
    var b = marshal(~p);
    var zw = gzip.NewWriter(w);
    var zwʗ1 = zw;
    defer(zwʗ1.Close);
    var (_, err) = zw.Write(b);
    return err;
});

// CheckValid tests whether the profile is valid. Checks include, but are
// not limited to:
//   - len(Profile.Sample[n].value) == len(Profile.value_unit)
//   - Sample.id has a corresponding Profile.Location
[GoRecv] public static error CheckValid(this ref Profile p) {
    // Check that sample values are consistent
    nint sampleLen = len(p.SampleType);
    if (sampleLen == 0 && len(p.Sample) != 0) {
        return fmt.Errorf("missing sample type information"u8);
    }
    foreach (var (_, s) in p.Sample) {
        if (len((~s).Value) != sampleLen) {
            return fmt.Errorf("mismatch: sample has: %d values vs. %d types"u8, len((~s).Value), len(p.SampleType));
        }
    }
    // Check that all mappings/locations/functions are in the tables
    // Check that there are no duplicate ids
    var mappings = new map<uint64, ж<Mapping>>(len(p.Mapping));
    foreach (var (_, m) in p.Mapping) {
        if ((~m).ID == 0) {
            return fmt.Errorf("found mapping with reserved ID=0"u8);
        }
        if (mappings[(~m).ID] != nil) {
            return fmt.Errorf("multiple mappings with same id: %d"u8, (~m).ID);
        }
        mappings[(~m).ID] = m;
    }
    var functions = new map<uint64, ж<Function>>(len(p.Function));
    foreach (var (_, f) in p.Function) {
        if ((~f).ID == 0) {
            return fmt.Errorf("found function with reserved ID=0"u8);
        }
        if (functions[(~f).ID] != nil) {
            return fmt.Errorf("multiple functions with same id: %d"u8, (~f).ID);
        }
        functions[(~f).ID] = f;
    }
    var locations = new map<uint64, ж<Location>>(len(p.Location));
    foreach (var (_, l) in p.Location) {
        if ((~l).ID == 0) {
            return fmt.Errorf("found location with reserved id=0"u8);
        }
        if (locations[(~l).ID] != nil) {
            return fmt.Errorf("multiple locations with same id: %d"u8, (~l).ID);
        }
        locations[(~l).ID] = l;
        {
            var m = l.val.Mapping; if (m != nil) {
                if ((~m).ID == 0 || mappings[(~m).ID] != m) {
                    return fmt.Errorf("inconsistent mapping %p: %d"u8, m, (~m).ID);
                }
            }
        }
        foreach (var (_, ln) in (~l).Line) {
            {
                var f = ln.Function; if (f != nil) {
                    if ((~f).ID == 0 || functions[(~f).ID] != f) {
                        return fmt.Errorf("inconsistent function %p: %d"u8, f, (~f).ID);
                    }
                }
            }
        }
    }
    return default!;
}

// Aggregate merges the locations in the profile into equivalence
// classes preserving the request attributes. It also updates the
// samples to point to the merged locations.
[GoRecv] public static error Aggregate(this ref Profile p, bool inlineFrame, bool function, bool filename, bool linenumber, bool address) {
    foreach (var (_, m) in p.Mapping) {
        m.val.HasInlineFrames = (~m).HasInlineFrames && inlineFrame;
        m.val.HasFunctions = (~m).HasFunctions && function;
        m.val.HasFilenames = (~m).HasFilenames && filename;
        m.val.HasLineNumbers = (~m).HasLineNumbers && linenumber;
    }
    // Aggregate functions
    if (!function || !filename) {
        foreach (var (_, f) in p.Function) {
            if (!function) {
                f.val.Name = ""u8;
                f.val.SystemName = ""u8;
            }
            if (!filename) {
                f.val.Filename = ""u8;
            }
        }
    }
    // Aggregate locations
    if (!inlineFrame || !address || !linenumber) {
        foreach (var (_, l) in p.Location) {
            if (!inlineFrame && len((~l).Line) > 1) {
                l.val.Line = (~l).Line[(int)(len((~l).Line) - 1)..];
            }
            if (!linenumber) {
                foreach (var (i, _) in (~l).Line) {
                    (~l).Line[i].Line = 0;
                }
            }
            if (!address) {
                l.val.Address = 0;
            }
        }
    }
    return p.CheckValid();
}

// Print dumps a text representation of a profile. Intended mainly
// for debugging purposes.
[GoRecv] public static @string String(this ref Profile p) {
    var ss = new slice<@string>(0, len(p.Sample) + len(p.Mapping) + len(p.Location));
    {
        var pt = p.PeriodType; if (pt != nil) {
            ss = append(ss, fmt.Sprintf("PeriodType: %s %s"u8, (~pt).Type, (~pt).Unit));
        }
    }
    ss = append(ss, fmt.Sprintf("Period: %d"u8, p.Period));
    if (p.TimeNanos != 0) {
        ss = append(ss, fmt.Sprintf("Time: %v"u8, time.Unix(0, p.TimeNanos)));
    }
    if (p.DurationNanos != 0) {
        ss = append(ss, fmt.Sprintf("Duration: %v"u8, ((time.Duration)p.DurationNanos)));
    }
    ss = append(ss, "Samples:"u8);
    @string sh1 = default!;
    foreach (var (_, s) in p.SampleType) {
        sh1 = sh1 + fmt.Sprintf("%s/%s "u8, (~s).Type, (~s).Unit);
    }
    ss = append(ss, strings.TrimSpace(sh1));
    foreach (var (_, s) in p.Sample) {
        @string sv = default!;
        foreach (var (_, v) in (~s).Value) {
            sv = fmt.Sprintf("%s %10d"u8, sv, v);
        }
        sv = sv + ": "u8;
        foreach (var (_, l) in (~s).Location) {
            sv = sv + fmt.Sprintf("%d "u8, (~l).ID);
        }
        ss = append(ss, sv);
        @string labelHeader = "                "u8;
        if (len((~s).Label) > 0) {
            @string ls = labelHeader;
            foreach (var (k, v) in (~s).Label) {
                ls = ls + fmt.Sprintf("%s:%v "u8, k, v);
            }
            ss = append(ss, ls);
        }
        if (len((~s).NumLabel) > 0) {
            @string ls = labelHeader;
            foreach (var (k, v) in (~s).NumLabel) {
                ls = ls + fmt.Sprintf("%s:%v "u8, k, v);
            }
            ss = append(ss, ls);
        }
    }
    ss = append(ss, "Locations"u8);
    foreach (var (_, l) in p.Location) {
        @string locStr = fmt.Sprintf("%6d: %#x "u8, (~l).ID, (~l).Address);
        {
            var m = l.val.Mapping; if (m != nil) {
                locStr = locStr + fmt.Sprintf("M=%d "u8, (~m).ID);
            }
        }
        if (len((~l).Line) == 0) {
            ss = append(ss, locStr);
        }
        foreach (var (li, _) in (~l).Line) {
            @string lnStr = "??"u8;
            {
                var fn = (~l).Line[li].Function; if (fn != nil) {
                    lnStr = fmt.Sprintf("%s %s:%d s=%d"u8,
                        (~fn).Name,
                        (~fn).Filename,
                        (~l).Line[li].Line,
                        (~fn).StartLine);
                    if ((~fn).Name != (~fn).SystemName) {
                        lnStr = lnStr + "("u8 + (~fn).SystemName + ")"u8;
                    }
                }
            }
            ss = append(ss, locStr + lnStr);
            // Do not print location details past the first line
            locStr = "             "u8;
        }
    }
    ss = append(ss, "Mappings"u8);
    foreach (var (_, m) in p.Mapping) {
        @string bits = ""u8;
        if ((~m).HasFunctions) {
            bits += "[FN]"u8;
        }
        if ((~m).HasFilenames) {
            bits += "[FL]"u8;
        }
        if ((~m).HasLineNumbers) {
            bits += "[LN]"u8;
        }
        if ((~m).HasInlineFrames) {
            bits += "[IN]"u8;
        }
        ss = append(ss, fmt.Sprintf("%d: %#x/%#x/%#x %s %s %s"u8,
            (~m).ID,
            (~m).Start, (~m).Limit, (~m).Offset,
            (~m).File,
            (~m).BuildID,
            bits));
    }
    return strings.Join(ss, "\n"u8) + "\n"u8;
}

// Merge adds profile p adjusted by ratio r into profile p. Profiles
// must be compatible (same Type and SampleType).
// TODO(rsilvera): consider normalizing the profiles based on the
// total samples collected.
[GoRecv] public static error Merge(this ref Profile p, ж<Profile> Ꮡpb, float64 r) {
    ref var pb = ref Ꮡpb.val;

    {
        var err = p.Compatible(Ꮡpb); if (err != default!) {
            return err;
        }
    }
    pb = pb.Copy();
    // Keep the largest of the two periods.
    if (pb.Period > p.Period) {
        p.Period = pb.Period;
    }
    p.DurationNanos += pb.DurationNanos;
    p.Mapping = append(p.Mapping, pb.Mapping.ꓸꓸꓸ);
    foreach (var (i, m) in p.Mapping) {
        m.val.ID = ((uint64)(i + 1));
    }
    p.Location = append(p.Location, pb.Location.ꓸꓸꓸ);
    foreach (var (i, l) in p.Location) {
        l.val.ID = ((uint64)(i + 1));
    }
    p.Function = append(p.Function, pb.Function.ꓸꓸꓸ);
    foreach (var (i, f) in p.Function) {
        f.val.ID = ((uint64)(i + 1));
    }
    if (r != 1.0F) {
        foreach (var (_, s) in pb.Sample) {
            foreach (var (i, v) in (~s).Value) {
                (~s).Value[i] = ((int64)(((float64)v) * r));
            }
        }
    }
    p.Sample = append(p.Sample, pb.Sample.ꓸꓸꓸ);
    return p.CheckValid();
}

// Compatible determines if two profiles can be compared/merged.
// returns nil if the profiles are compatible; otherwise an error with
// details on the incompatibility.
[GoRecv] public static error Compatible(this ref Profile p, ж<Profile> Ꮡpb) {
    ref var pb = ref Ꮡpb.val;

    if (!compatibleValueTypes(p.PeriodType, pb.PeriodType)) {
        return fmt.Errorf("incompatible period types %v and %v"u8, p.PeriodType, pb.PeriodType);
    }
    if (len(p.SampleType) != len(pb.SampleType)) {
        return fmt.Errorf("incompatible sample types %v and %v"u8, p.SampleType, pb.SampleType);
    }
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in p.SampleType) {
        if (!compatibleValueTypes(p.SampleType[i], pb.SampleType[i])) {
            return fmt.Errorf("incompatible sample types %v and %v"u8, p.SampleType, pb.SampleType);
        }
    }
    return default!;
}

// HasFunctions determines if all locations in this profile have
// symbolized function information.
[GoRecv] public static bool HasFunctions(this ref Profile p) {
    foreach (var (_, l) in p.Location) {
        if ((~l).Mapping == nil || !(~(~l).Mapping).HasFunctions) {
            return false;
        }
    }
    return true;
}

// HasFileLines determines if all locations in this profile have
// symbolized file and line number information.
[GoRecv] public static bool HasFileLines(this ref Profile p) {
    foreach (var (_, l) in p.Location) {
        if ((~l).Mapping == nil || (!(~(~l).Mapping).HasFilenames || !(~(~l).Mapping).HasLineNumbers)) {
            return false;
        }
    }
    return true;
}

internal static bool compatibleValueTypes(ж<ValueType> Ꮡv1, ж<ValueType> Ꮡv2) {
    ref var v1 = ref Ꮡv1.val;
    ref var v2 = ref Ꮡv2.val;

    if (v1 == nil || v2 == nil) {
        return true;
    }
    // No grounds to disqualify.
    return v1.Type == v2.Type && v1.Unit == v2.Unit;
}

// Copy makes a fully independent copy of a profile.
[GoRecv] public static ж<Profile> Copy(this ref Profile p) {
    p.preEncode();
    var b = marshal(~p);
    var pp = Ꮡ(new Profile(nil));
    {
        var err = unmarshal(b, ~pp); if (err != default!) {
            throw panic(err);
        }
    }
    {
        var err = pp.postDecode(); if (err != default!) {
            throw panic(err);
        }
    }
    return pp;
}

public delegate (map<@string, @string>, error) Demangler(slice<@string> name);

// Demangle attempts to demangle and optionally simplify any function
// names referenced in the profile. It works on a best-effort basis:
// it will silently preserve the original names in case of any errors.
[GoRecv] public static error Demangle(this ref Profile p, Demangler d) {
    // Collect names to demangle.
    slice<@string> names = default!;
    foreach (var (_, fn) in p.Function) {
        names = append(names, (~fn).SystemName);
    }
    // Update profile with demangled names.
    var demangled = d(names);
    var err = d(names);
    if (err != default!) {
        return err;
    }
    foreach (var (_, fn) in p.Function) {
        {
            @string dd = demangled[(~fn).SystemName];
            var ok = demangled[(~fn).SystemName]; if (ok) {
                fn.val.Name = dd;
            }
        }
    }
    return default!;
}

// Empty reports whether the profile contains no samples.
[GoRecv] public static bool Empty(this ref Profile p) {
    return len(p.Sample) == 0;
}

// Scale multiplies all sample values in a profile by a constant.
[GoRecv] public static void Scale(this ref Profile p, float64 ratio) {
    if (ratio == 1) {
        return;
    }
    var ratios = new slice<float64>(len(p.SampleType));
    foreach (var (i, _) in p.SampleType) {
        ratios[i] = ratio;
    }
    p.ScaleN(ratios);
}

// ScaleN multiplies each sample values in a sample by a different amount.
[GoRecv] public static error ScaleN(this ref Profile p, slice<float64> ratios) {
    if (len(p.SampleType) != len(ratios)) {
        return fmt.Errorf("mismatched scale ratios, got %d, want %d"u8, len(ratios), len(p.SampleType));
    }
    var allOnes = true;
    foreach (var (_, r) in ratios) {
        if (r != 1) {
            allOnes = false;
            break;
        }
    }
    if (allOnes) {
        return default!;
    }
    foreach (var (_, s) in p.Sample) {
        foreach (var (i, v) in (~s).Value) {
            if (ratios[i] != 1) {
                (~s).Value[i] = ((int64)(((float64)v) * ratios[i]));
            }
        }
    }
    return default!;
}

} // end profile_package
