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

// Package profile provides a representation of profile.proto and
// methods to encode/decode profiles in this format.

// package profile -- go2cs converted at 2022 March 13 06:37:10 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\profile\profile.go
namespace go.cmd.vendor.github.com.google.pprof;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using fmt = fmt_package;
using io = io_package;
using ioutil = io.ioutil_package;
using math = math_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;


// Profile is an in-memory representation of profile.proto.

using System;
public static partial class profile_package {

public partial struct Profile {
    public slice<ptr<ValueType>> SampleType;
    public @string DefaultSampleType;
    public slice<ptr<Sample>> Sample;
    public slice<ptr<Mapping>> Mapping;
    public slice<ptr<Location>> Location;
    public slice<ptr<Function>> Function;
    public slice<@string> Comments;
    public @string DropFrames;
    public @string KeepFrames;
    public long TimeNanos;
    public long DurationNanos;
    public ptr<ValueType> PeriodType;
    public long Period; // The following fields are modified during encoding and copying,
// so are protected by a Mutex.
    public sync.Mutex encodeMu;
    public slice<long> commentX;
    public long dropFramesX;
    public long keepFramesX;
    public slice<@string> stringTable;
    public long defaultSampleTypeX;
}

// ValueType corresponds to Profile.ValueType
public partial struct ValueType {
    public @string Type; // cpu, wall, inuse_space, etc
    public @string Unit; // seconds, nanoseconds, bytes, etc

    public long typeX;
    public long unitX;
}

// Sample corresponds to Profile.Sample
public partial struct Sample {
    public slice<ptr<Location>> Location;
    public slice<long> Value;
    public map<@string, slice<@string>> Label;
    public map<@string, slice<long>> NumLabel;
    public map<@string, slice<@string>> NumUnit;
    public slice<ulong> locationIDX;
    public slice<label> labelX;
}

// label corresponds to Profile.Label
private partial struct label {
    public long keyX; // Exactly one of the two following values must be set
    public long strX;
    public long numX; // Integer value for this label
// can be set if numX has value
    public long unitX;
}

// Mapping corresponds to Profile.Mapping
public partial struct Mapping {
    public ulong ID;
    public ulong Start;
    public ulong Limit;
    public ulong Offset;
    public @string File;
    public @string BuildID;
    public bool HasFunctions;
    public bool HasFilenames;
    public bool HasLineNumbers;
    public bool HasInlineFrames;
    public long fileX;
    public long buildIDX;
}

// Location corresponds to Profile.Location
public partial struct Location {
    public ulong ID;
    public ptr<Mapping> Mapping;
    public ulong Address;
    public slice<Line> Line;
    public bool IsFolded;
    public ulong mappingIDX;
}

// Line corresponds to Profile.Line
public partial struct Line {
    public ptr<Function> Function;
    public long Line;
    public ulong functionIDX;
}

// Function corresponds to Profile.Function
public partial struct Function {
    public ulong ID;
    public @string Name;
    public @string SystemName;
    public @string Filename;
    public long StartLine;
    public long nameX;
    public long systemNameX;
    public long filenameX;
}

// Parse parses a profile and checks for its validity. The input
// may be a gzip-compressed encoded protobuf or one of many legacy
// profile formats which may be unsupported in the future.
public static (ptr<Profile>, error) Parse(io.Reader r) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var (data, err) = ioutil.ReadAll(r);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return _addr_ParseData(data)!;
}

// ParseData parses a profile from a buffer and checks for its
// validity.
public static (ptr<Profile>, error) ParseData(slice<byte> data) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    ptr<Profile> p;
    error err = default!;
    if (len(data) >= 2 && data[0] == 0x1f && data[1] == 0x8b) {
        var (gz, err) = gzip.NewReader(bytes.NewBuffer(data));
        if (err == null) {
            data, err = ioutil.ReadAll(gz);
        }
        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("decompressing profile: %v", err))!);
        }
    }
    p, err = ParseUncompressed(data);

    if (err != null && err != errNoData && err != errConcatProfile) {
        p, err = parseLegacy(data);
    }
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("parsing profile: %v", err))!);
    }
    {
        error err__prev1 = err;

        err = p.CheckValid();

        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("malformed profile: %v", err))!);
        }
        err = err__prev1;

    }
    return (_addr_p!, error.As(null!)!);
}

private static var errUnrecognized = fmt.Errorf("unrecognized profile format");
private static var errMalformed = fmt.Errorf("malformed profile format");
private static var errNoData = fmt.Errorf("empty input file");
private static var errConcatProfile = fmt.Errorf("concatenated profiles detected");

private static (ptr<Profile>, error) parseLegacy(slice<byte> data) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    Func<slice<byte>, (ptr<Profile>, error)> parsers = new slice<Func<slice<byte>, (ptr<Profile>, error)>>(new Func<slice<byte>, (ptr<Profile>, error)>[] { parseCPU, parseHeap, parseGoCount, parseThread, parseContention, parseJavaProfile });

    foreach (var (_, parser) in parsers) {
        var (p, err) = parser(data);
        if (err == null) {
            p.addLegacyFrameInfo();
            return (_addr_p!, error.As(null!)!);
        }
        if (err != errUnrecognized) {
            return (_addr_null!, error.As(err)!);
        }
    }    return (_addr_null!, error.As(errUnrecognized)!);
}

// ParseUncompressed parses an uncompressed protobuf into a profile.
public static (ptr<Profile>, error) ParseUncompressed(slice<byte> data) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    if (len(data) == 0) {
        return (_addr_null!, error.As(errNoData)!);
    }
    ptr<Profile> p = addr(new Profile());
    {
        var err__prev1 = err;

        var err = unmarshal(data, p);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = p.postDecode();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    return (_addr_p!, error.As(null!)!);
}

private static var libRx = regexp.MustCompile("([.]so$|[.]so[._][0-9]+)");

// massageMappings applies heuristic-based changes to the profile
// mappings to account for quirks of some environments.
private static void massageMappings(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;
 
    // Merge adjacent regions with matching names, checking that the offsets match
    if (len(p.Mapping) > 1) {
        ptr<Mapping> mappings = new slice<ptr<Mapping>>(new ptr<Mapping>[] { p.Mapping[0] });
        {
            var m__prev1 = m;

            foreach (var (_, __m) in p.Mapping[(int)1..]) {
                m = __m;
                var lm = mappings[len(mappings) - 1];
                if (adjacent(_addr_lm, _addr_m)) {
                    lm.Limit = m.Limit;
                    if (m.File != "") {
                        lm.File = m.File;
                    }
                    if (m.BuildID != "") {
                        lm.BuildID = m.BuildID;
                    }
                    p.updateLocationMapping(m, lm);
                    continue;
                }
                mappings = append(mappings, m);
            }

            m = m__prev1;
        }

        p.Mapping = mappings;
    }
    {
        var i__prev1 = i;
        var m__prev1 = m;

        foreach (var (__i, __m) in p.Mapping) {
            i = __i;
            m = __m;
            var file = strings.TrimSpace(strings.Replace(m.File, "(deleted)", "", -1));
            if (len(file) == 0) {
                continue;
            }
            if (len(libRx.FindStringSubmatch(file)) > 0) {
                continue;
            }
            if (file[0] == '[') {
                continue;
            } 
            // Swap what we guess is main to position 0.
            (p.Mapping[0], p.Mapping[i]) = (p.Mapping[i], p.Mapping[0]);            break;
        }
        i = i__prev1;
        m = m__prev1;
    }

    {
        var i__prev1 = i;
        var m__prev1 = m;

        foreach (var (__i, __m) in p.Mapping) {
            i = __i;
            m = __m;
            m.ID = uint64(i + 1);
        }
        i = i__prev1;
        m = m__prev1;
    }
}

// adjacent returns whether two mapping entries represent the same
// mapping that has been split into two. Check that their addresses are adjacent,
// and if the offsets match, if they are available.
private static bool adjacent(ptr<Mapping> _addr_m1, ptr<Mapping> _addr_m2) {
    ref Mapping m1 = ref _addr_m1.val;
    ref Mapping m2 = ref _addr_m2.val;

    if (m1.File != "" && m2.File != "") {
        if (m1.File != m2.File) {
            return false;
        }
    }
    if (m1.BuildID != "" && m2.BuildID != "") {
        if (m1.BuildID != m2.BuildID) {
            return false;
        }
    }
    if (m1.Limit != m2.Start) {
        return false;
    }
    if (m1.Offset != 0 && m2.Offset != 0) {
        var offset = m1.Offset + (m1.Limit - m1.Start);
        if (offset != m2.Offset) {
            return false;
        }
    }
    return true;
}

private static void updateLocationMapping(this ptr<Profile> _addr_p, ptr<Mapping> _addr_from, ptr<Mapping> _addr_to) {
    ref Profile p = ref _addr_p.val;
    ref Mapping from = ref _addr_from.val;
    ref Mapping to = ref _addr_to.val;

    foreach (var (_, l) in p.Location) {
        if (l.Mapping == from) {
            l.Mapping = to;
        }
    }
}

private static slice<byte> serialize(ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    p.encodeMu.Lock();
    p.preEncode();
    var b = marshal(p);
    p.encodeMu.Unlock();
    return b;
}

// Write writes the profile as a gzip-compressed marshaled protobuf.
private static error Write(this ptr<Profile> _addr_p, io.Writer w) => func((defer, _, _) => {
    ref Profile p = ref _addr_p.val;

    var zw = gzip.NewWriter(w);
    defer(zw.Close());
    var (_, err) = zw.Write(serialize(_addr_p));
    return error.As(err)!;
});

// WriteUncompressed writes the profile as a marshaled protobuf.
private static error WriteUncompressed(this ptr<Profile> _addr_p, io.Writer w) {
    ref Profile p = ref _addr_p.val;

    var (_, err) = w.Write(serialize(_addr_p));
    return error.As(err)!;
}

// CheckValid tests whether the profile is valid. Checks include, but are
// not limited to:
//   - len(Profile.Sample[n].value) == len(Profile.value_unit)
//   - Sample.id has a corresponding Profile.Location
private static error CheckValid(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;
 
    // Check that sample values are consistent
    var sampleLen = len(p.SampleType);
    if (sampleLen == 0 && len(p.Sample) != 0) {
        return error.As(fmt.Errorf("missing sample type information"))!;
    }
    foreach (var (_, s) in p.Sample) {
        if (s == null) {
            return error.As(fmt.Errorf("profile has nil sample"))!;
        }
        if (len(s.Value) != sampleLen) {
            return error.As(fmt.Errorf("mismatch: sample has %d values vs. %d types", len(s.Value), len(p.SampleType)))!;
        }
        {
            var l__prev2 = l;

            foreach (var (_, __l) in s.Location) {
                l = __l;
                if (l == null) {
                    return error.As(fmt.Errorf("sample has nil location"))!;
                }
            }

            l = l__prev2;
        }
    }    var mappings = make_map<ulong, ptr<Mapping>>(len(p.Mapping));
    {
        var m__prev1 = m;

        foreach (var (_, __m) in p.Mapping) {
            m = __m;
            if (m == null) {
                return error.As(fmt.Errorf("profile has nil mapping"))!;
            }
            if (m.ID == 0) {
                return error.As(fmt.Errorf("found mapping with reserved ID=0"))!;
            }
            if (mappings[m.ID] != null) {
                return error.As(fmt.Errorf("multiple mappings with same id: %d", m.ID))!;
            }
            mappings[m.ID] = m;
        }
        m = m__prev1;
    }

    var functions = make_map<ulong, ptr<Function>>(len(p.Function));
    {
        var f__prev1 = f;

        foreach (var (_, __f) in p.Function) {
            f = __f;
            if (f == null) {
                return error.As(fmt.Errorf("profile has nil function"))!;
            }
            if (f.ID == 0) {
                return error.As(fmt.Errorf("found function with reserved ID=0"))!;
            }
            if (functions[f.ID] != null) {
                return error.As(fmt.Errorf("multiple functions with same id: %d", f.ID))!;
            }
            functions[f.ID] = f;
        }
        f = f__prev1;
    }

    var locations = make_map<ulong, ptr<Location>>(len(p.Location));
    {
        var l__prev1 = l;

        foreach (var (_, __l) in p.Location) {
            l = __l;
            if (l == null) {
                return error.As(fmt.Errorf("profile has nil location"))!;
            }
            if (l.ID == 0) {
                return error.As(fmt.Errorf("found location with reserved id=0"))!;
            }
            if (locations[l.ID] != null) {
                return error.As(fmt.Errorf("multiple locations with same id: %d", l.ID))!;
            }
            locations[l.ID] = l;
            {
                var m__prev1 = m;

                var m = l.Mapping;

                if (m != null) {
                    if (m.ID == 0 || mappings[m.ID] != m) {
                        return error.As(fmt.Errorf("inconsistent mapping %p: %d", m, m.ID))!;
                    }
                }

                m = m__prev1;

            }
            foreach (var (_, ln) in l.Line) {
                var f = ln.Function;
                if (f == null) {
                    return error.As(fmt.Errorf("location id: %d has a line with nil function", l.ID))!;
                }
                if (f.ID == 0 || functions[f.ID] != f) {
                    return error.As(fmt.Errorf("inconsistent function %p: %d", f, f.ID))!;
                }
            }
        }
        l = l__prev1;
    }

    return error.As(null!)!;
}

// Aggregate merges the locations in the profile into equivalence
// classes preserving the request attributes. It also updates the
// samples to point to the merged locations.
private static error Aggregate(this ptr<Profile> _addr_p, bool inlineFrame, bool function, bool filename, bool linenumber, bool address) {
    ref Profile p = ref _addr_p.val;

    foreach (var (_, m) in p.Mapping) {
        m.HasInlineFrames = m.HasInlineFrames && inlineFrame;
        m.HasFunctions = m.HasFunctions && function;
        m.HasFilenames = m.HasFilenames && filename;
        m.HasLineNumbers = m.HasLineNumbers && linenumber;
    }    if (!function || !filename) {
        foreach (var (_, f) in p.Function) {
            if (!function) {
                f.Name = "";
                f.SystemName = "";
            }
            if (!filename) {
                f.Filename = "";
            }
        }
    }
    if (!inlineFrame || !address || !linenumber) {
        foreach (var (_, l) in p.Location) {
            if (!inlineFrame && len(l.Line) > 1) {
                l.Line = l.Line[(int)len(l.Line) - 1..];
            }
            if (!linenumber) {
                foreach (var (i) in l.Line) {
                    l.Line[i].Line = 0;
                }
            }
            if (!address) {
                l.Address = 0;
            }
        }
    }
    return error.As(p.CheckValid())!;
}

// NumLabelUnits returns a map of numeric label keys to the units
// associated with those keys and a map of those keys to any units
// that were encountered but not used.
// Unit for a given key is the first encountered unit for that key. If multiple
// units are encountered for values paired with a particular key, then the first
// unit encountered is used and all other units are returned in sorted order
// in map of ignored units.
// If no units are encountered for a particular key, the unit is then inferred
// based on the key.
private static (map<@string, @string>, map<@string, slice<@string>>) NumLabelUnits(this ptr<Profile> _addr_p) {
    map<@string, @string> _p0 = default;
    map<@string, slice<@string>> _p0 = default;
    ref Profile p = ref _addr_p.val;

    map numLabelUnits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
    map ignoredUnits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, map<@string, bool>>{};
    map encounteredKeys = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{}; 

    // Determine units based on numeric tags for each sample.
    foreach (var (_, s) in p.Sample) {
        foreach (var (k) in s.NumLabel) {
            encounteredKeys[k] = true;
            {
                var unit__prev3 = unit;

                foreach (var (_, __unit) in s.NumUnit[k]) {
                    unit = __unit;
                    if (unit == "") {
                        continue;
                    }
                    {
                        var (wantUnit, ok) = numLabelUnits[k];

                        if (!ok) {
                            numLabelUnits[k] = unit;
                        }
                        else if (wantUnit != unit) {
                            {
                                var (v, ok) = ignoredUnits[k];

                                if (ok) {
                                    v[unit] = true;
                                }
                                else
 {
                                    ignoredUnits[k] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{unit:true};
                                }

                            }
                        }

                    }
                }

                unit = unit__prev3;
            }
        }
    }    {
        var key__prev1 = key;

        foreach (var (__key) in encounteredKeys) {
            key = __key;
            var unit = numLabelUnits[key];
            if (unit == "") {
                switch (key) {
                    case "alignment": 

                    case "request": 
                        numLabelUnits[key] = "bytes";
                        break;
                    default: 
                        numLabelUnits[key] = key;
                        break;
                }
            }
        }
        key = key__prev1;
    }

    var unitsIgnored = make_map<@string, slice<@string>>(len(ignoredUnits));
    {
        var key__prev1 = key;

        foreach (var (__key, __values) in ignoredUnits) {
            key = __key;
            values = __values;
            var units = make_slice<@string>(len(values));
            nint i = 0;
            {
                var unit__prev2 = unit;

                foreach (var (__unit) in values) {
                    unit = __unit;
                    units[i] = unit;
                    i++;
                }

                unit = unit__prev2;
            }

            sort.Strings(units);
            unitsIgnored[key] = units;
        }
        key = key__prev1;
    }

    return (numLabelUnits, unitsIgnored);
}

// String dumps a text representation of a profile. Intended mainly
// for debugging purposes.
private static @string String(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    var ss = make_slice<@string>(0, len(p.Comments) + len(p.Sample) + len(p.Mapping) + len(p.Location));
    foreach (var (_, c) in p.Comments) {
        ss = append(ss, "Comment: " + c);
    }    {
        var pt = p.PeriodType;

        if (pt != null) {
            ss = append(ss, fmt.Sprintf("PeriodType: %s %s", pt.Type, pt.Unit));
        }
    }
    ss = append(ss, fmt.Sprintf("Period: %d", p.Period));
    if (p.TimeNanos != 0) {
        ss = append(ss, fmt.Sprintf("Time: %v", time.Unix(0, p.TimeNanos)));
    }
    if (p.DurationNanos != 0) {
        ss = append(ss, fmt.Sprintf("Duration: %.4v", time.Duration(p.DurationNanos)));
    }
    ss = append(ss, "Samples:");
    @string sh1 = default;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in p.SampleType) {
            s = __s;
            @string dflt = "";
            if (s.Type == p.DefaultSampleType) {
                dflt = "[dflt]";
            }
            sh1 = sh1 + fmt.Sprintf("%s/%s%s ", s.Type, s.Unit, dflt);
        }
        s = s__prev1;
    }

    ss = append(ss, strings.TrimSpace(sh1));
    {
        var s__prev1 = s;

        foreach (var (_, __s) in p.Sample) {
            s = __s;
            ss = append(ss, s.@string());
        }
        s = s__prev1;
    }

    ss = append(ss, "Locations");
    foreach (var (_, l) in p.Location) {
        ss = append(ss, l.@string());
    }    ss = append(ss, "Mappings");
    foreach (var (_, m) in p.Mapping) {
        ss = append(ss, m.@string());
    }    return strings.Join(ss, "\n") + "\n";
}

// string dumps a text representation of a mapping. Intended mainly
// for debugging purposes.
private static @string @string(this ptr<Mapping> _addr_m) {
    ref Mapping m = ref _addr_m.val;

    @string bits = "";
    if (m.HasFunctions) {
        bits = bits + "[FN]";
    }
    if (m.HasFilenames) {
        bits = bits + "[FL]";
    }
    if (m.HasLineNumbers) {
        bits = bits + "[LN]";
    }
    if (m.HasInlineFrames) {
        bits = bits + "[IN]";
    }
    return fmt.Sprintf("%d: %#x/%#x/%#x %s %s %s", m.ID, m.Start, m.Limit, m.Offset, m.File, m.BuildID, bits);
}

// string dumps a text representation of a location. Intended mainly
// for debugging purposes.
private static @string @string(this ptr<Location> _addr_l) {
    ref Location l = ref _addr_l.val;

    @string ss = new slice<@string>(new @string[] {  });
    var locStr = fmt.Sprintf("%6d: %#x ", l.ID, l.Address);
    {
        var m = l.Mapping;

        if (m != null) {
            locStr = locStr + fmt.Sprintf("M=%d ", m.ID);
        }
    }
    if (l.IsFolded) {
        locStr = locStr + "[F] ";
    }
    if (len(l.Line) == 0) {
        ss = append(ss, locStr);
    }
    foreach (var (li) in l.Line) {
        @string lnStr = "??";
        {
            var fn = l.Line[li].Function;

            if (fn != null) {
                lnStr = fmt.Sprintf("%s %s:%d s=%d", fn.Name, fn.Filename, l.Line[li].Line, fn.StartLine);
                if (fn.Name != fn.SystemName) {
                    lnStr = lnStr + "(" + fn.SystemName + ")";
                }
            }

        }
        ss = append(ss, locStr + lnStr); 
        // Do not print location details past the first line
        locStr = "             ";
    }    return strings.Join(ss, "\n");
}

// string dumps a text representation of a sample. Intended mainly
// for debugging purposes.
private static @string @string(this ptr<Sample> _addr_s) {
    ref Sample s = ref _addr_s.val;

    @string ss = new slice<@string>(new @string[] {  });
    @string sv = default;
    foreach (var (_, v) in s.Value) {
        sv = fmt.Sprintf("%s %10d", sv, v);
    }    sv = sv + ": ";
    foreach (var (_, l) in s.Location) {
        sv = sv + fmt.Sprintf("%d ", l.ID);
    }    ss = append(ss, sv);
    const @string labelHeader = "                ";

    if (len(s.Label) > 0) {
        ss = append(ss, labelHeader + labelsToString(s.Label));
    }
    if (len(s.NumLabel) > 0) {
        ss = append(ss, labelHeader + numLabelsToString(s.NumLabel, s.NumUnit));
    }
    return strings.Join(ss, "\n");
}

// labelsToString returns a string representation of a
// map representing labels.
private static @string labelsToString(map<@string, slice<@string>> labels) {
    @string ls = new slice<@string>(new @string[] {  });
    foreach (var (k, v) in labels) {
        ls = append(ls, fmt.Sprintf("%s:%v", k, v));
    }    sort.Strings(ls);
    return strings.Join(ls, " ");
}

// numLabelsToString returns a string representation of a map
// representing numeric labels.
private static @string numLabelsToString(map<@string, slice<long>> numLabels, map<@string, slice<@string>> numUnits) {
    @string ls = new slice<@string>(new @string[] {  });
    foreach (var (k, v) in numLabels) {
        var units = numUnits[k];
        @string labelString = default;
        if (len(units) == len(v)) {
            var values = make_slice<@string>(len(v));
            foreach (var (i, vv) in v) {
                values[i] = fmt.Sprintf("%d %s", vv, units[i]);
            }
        else
            labelString = fmt.Sprintf("%s:%v", k, values);
        } {
            labelString = fmt.Sprintf("%s:%v", k, v);
        }
        ls = append(ls, labelString);
    }    sort.Strings(ls);
    return strings.Join(ls, " ");
}

// SetLabel sets the specified key to the specified value for all samples in the
// profile.
private static void SetLabel(this ptr<Profile> _addr_p, @string key, slice<@string> value) {
    ref Profile p = ref _addr_p.val;

    foreach (var (_, sample) in p.Sample) {
        if (sample.Label == null) {
            sample.Label = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<@string>>{key:value};
        }
        else
 {
            sample.Label[key] = value;
        }
    }
}

// RemoveLabel removes all labels associated with the specified key for all
// samples in the profile.
private static void RemoveLabel(this ptr<Profile> _addr_p, @string key) {
    ref Profile p = ref _addr_p.val;

    foreach (var (_, sample) in p.Sample) {
        delete(sample.Label, key);
    }
}

// HasLabel returns true if a sample has a label with indicated key and value.
private static bool HasLabel(this ptr<Sample> _addr_s, @string key, @string value) {
    ref Sample s = ref _addr_s.val;

    foreach (var (_, v) in s.Label[key]) {
        if (v == value) {
            return true;
        }
    }    return false;
}

// DiffBaseSample returns true if a sample belongs to the diff base and false
// otherwise.
private static bool DiffBaseSample(this ptr<Sample> _addr_s) {
    ref Sample s = ref _addr_s.val;

    return s.HasLabel("pprof::base", "true");
}

// Scale multiplies all sample values in a profile by a constant and keeps
// only samples that have at least one non-zero value.
private static void Scale(this ptr<Profile> _addr_p, double ratio) {
    ref Profile p = ref _addr_p.val;

    if (ratio == 1) {
        return ;
    }
    var ratios = make_slice<double>(len(p.SampleType));
    foreach (var (i) in p.SampleType) {
        ratios[i] = ratio;
    }    p.ScaleN(ratios);
}

// ScaleN multiplies each sample values in a sample by a different amount
// and keeps only samples that have at least one non-zero value.
private static error ScaleN(this ptr<Profile> _addr_p, slice<double> ratios) {
    ref Profile p = ref _addr_p.val;

    if (len(p.SampleType) != len(ratios)) {
        return error.As(fmt.Errorf("mismatched scale ratios, got %d, want %d", len(ratios), len(p.SampleType)))!;
    }
    var allOnes = true;
    foreach (var (_, r) in ratios) {
        if (r != 1) {
            allOnes = false;
            break;
        }
    }    if (allOnes) {
        return error.As(null!)!;
    }
    nint fillIdx = 0;
    foreach (var (_, s) in p.Sample) {
        var keepSample = false;
        foreach (var (i, v) in s.Value) {
            if (ratios[i] != 1) {
                var val = int64(math.Round(float64(v) * ratios[i]));
                s.Value[i] = val;
                keepSample = keepSample || val != 0;
            }
        }        if (keepSample) {
            p.Sample[fillIdx] = s;
            fillIdx++;
        }
    }    p.Sample = p.Sample[..(int)fillIdx];
    return error.As(null!)!;
}

// HasFunctions determines if all locations in this profile have
// symbolized function information.
private static bool HasFunctions(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    foreach (var (_, l) in p.Location) {
        if (l.Mapping != null && !l.Mapping.HasFunctions) {
            return false;
        }
    }    return true;
}

// HasFileLines determines if all locations in this profile have
// symbolized file and line number information.
private static bool HasFileLines(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    foreach (var (_, l) in p.Location) {
        if (l.Mapping != null && (!l.Mapping.HasFilenames || !l.Mapping.HasLineNumbers)) {
            return false;
        }
    }    return true;
}

// Unsymbolizable returns true if a mapping points to a binary for which
// locations can't be symbolized in principle, at least now. Examples are
// "[vdso]", [vsyscall]" and some others, see the code.
private static bool Unsymbolizable(this ptr<Mapping> _addr_m) {
    ref Mapping m = ref _addr_m.val;

    var name = filepath.Base(m.File);
    return strings.HasPrefix(name, "[") || strings.HasPrefix(name, "linux-vdso") || strings.HasPrefix(m.File, "/dev/dri/");
}

// Copy makes a fully independent copy of a profile.
private static ptr<Profile> Copy(this ptr<Profile> _addr_p) => func((_, panic, _) => {
    ref Profile p = ref _addr_p.val;

    ptr<Profile> pp = addr(new Profile());
    {
        var err__prev1 = err;

        var err = unmarshal(serialize(_addr_p), pp);

        if (err != null) {
            panic(err);
        }
        err = err__prev1;

    }
    {
        var err__prev1 = err;

        err = pp.postDecode();

        if (err != null) {
            panic(err);
        }
        err = err__prev1;

    }

    return _addr_pp!;
});

} // end profile_package
