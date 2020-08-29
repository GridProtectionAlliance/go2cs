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
// package profile -- go2cs converted at 2020 August 29 10:06:32 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\profile\profile.go
using bytes = go.bytes_package;
using gzip = go.compress.gzip_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof
{
    public static partial class profile_package
    {
        // Profile is an in-memory representation of profile.proto.
        public partial struct Profile
        {
            public slice<ref ValueType> SampleType;
            public @string DefaultSampleType;
            public slice<ref Sample> Sample;
            public slice<ref Mapping> Mapping;
            public slice<ref Location> Location;
            public slice<ref Function> Function;
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
        public partial struct ValueType
        {
            public @string Type; // cpu, wall, inuse_space, etc
            public @string Unit; // seconds, nanoseconds, bytes, etc

            public long typeX;
            public long unitX;
        }

        // Sample corresponds to Profile.Sample
        public partial struct Sample
        {
            public slice<ref Location> Location;
            public slice<long> Value;
            public map<@string, slice<@string>> Label;
            public map<@string, slice<long>> NumLabel;
            public map<@string, slice<@string>> NumUnit;
            public slice<ulong> locationIDX;
            public slice<label> labelX;
        }

        // label corresponds to Profile.Label
        private partial struct label
        {
            public long keyX; // Exactly one of the two following values must be set
            public long strX;
            public long numX; // Integer value for this label
// can be set if numX has value
            public long unitX;
        }

        // Mapping corresponds to Profile.Mapping
        public partial struct Mapping
        {
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
        public partial struct Location
        {
            public ulong ID;
            public ptr<Mapping> Mapping;
            public ulong Address;
            public slice<Line> Line;
            public ulong mappingIDX;
        }

        // Line corresponds to Profile.Line
        public partial struct Line
        {
            public ptr<Function> Function;
            public long Line;
            public ulong functionIDX;
        }

        // Function corresponds to Profile.Function
        public partial struct Function
        {
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
        public static (ref Profile, error) Parse(io.Reader r)
        {
            var (data, err) = ioutil.ReadAll(r);
            if (err != null)
            {
                return (null, err);
            }
            return ParseData(data);
        }

        // ParseData parses a profile from a buffer and checks for its
        // validity.
        public static (ref Profile, error) ParseData(slice<byte> data)
        {
            ref Profile p = default;
            error err = default;
            if (len(data) >= 2L && data[0L] == 0x1fUL && data[1L] == 0x8bUL)
            {
                var (gz, err) = gzip.NewReader(bytes.NewBuffer(data));
                if (err == null)
                {
                    data, err = ioutil.ReadAll(gz);
                }
                if (err != null)
                {
                    return (null, fmt.Errorf("decompressing profile: %v", err));
                }
            }
            p, err = ParseUncompressed(data);

            if (err != null && err != errNoData)
            {
                p, err = parseLegacy(data);
            }
            if (err != null)
            {
                return (null, fmt.Errorf("parsing profile: %v", err));
            }
            {
                error err__prev1 = err;

                err = p.CheckValid();

                if (err != null)
                {
                    return (null, fmt.Errorf("malformed profile: %v", err));
                }

                err = err__prev1;

            }
            return (p, null);
        }

        private static var errUnrecognized = fmt.Errorf("unrecognized profile format");
        private static var errMalformed = fmt.Errorf("malformed profile format");
        private static var errNoData = fmt.Errorf("empty input file");

        private static (ref Profile, error) parseLegacy(slice<byte> data)
        {
            Func<slice<byte>, (ref Profile, error)> parsers = new slice<Func<slice<byte>, (ref Profile, error)>>(new Func<slice<byte>, (ref Profile, error)>[] { parseCPU, parseHeap, parseGoCount, parseThread, parseContention, parseJavaProfile });

            foreach (var (_, parser) in parsers)
            {
                var (p, err) = parser(data);
                if (err == null)
                {
                    p.addLegacyFrameInfo();
                    return (p, null);
                }
                if (err != errUnrecognized)
                {
                    return (null, err);
                }
            }
            return (null, errUnrecognized);
        }

        // ParseUncompressed parses an uncompressed protobuf into a profile.
        public static (ref Profile, error) ParseUncompressed(slice<byte> data)
        {
            if (len(data) == 0L)
            {
                return (null, errNoData);
            }
            Profile p = ref new Profile();
            {
                var err__prev1 = err;

                var err = unmarshal(data, p);

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = p.postDecode();

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }

            return (p, null);
        }

        private static var libRx = regexp.MustCompile("([.]so$|[.]so[._][0-9]+)");

        // massageMappings applies heuristic-based changes to the profile
        // mappings to account for quirks of some environments.
        private static void massageMappings(this ref Profile p)
        { 
            // Merge adjacent regions with matching names, checking that the offsets match
            if (len(p.Mapping) > 1L)
            {
                ref Mapping mappings = new slice<ref Mapping>(new ref Mapping[] { p.Mapping[0] });
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in p.Mapping[1L..])
                    {
                        m = __m;
                        var lm = mappings[len(mappings) - 1L];
                        if (adjacent(lm, m))
                        {
                            lm.Limit = m.Limit;
                            if (m.File != "")
                            {
                                lm.File = m.File;
                            }
                            if (m.BuildID != "")
                            {
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

            // Use heuristics to identify main binary and move it to the top of the list of mappings
            {
                var i__prev1 = i;
                var m__prev1 = m;

                foreach (var (__i, __m) in p.Mapping)
                {
                    i = __i;
                    m = __m;
                    var file = strings.TrimSpace(strings.Replace(m.File, "(deleted)", "", -1L));
                    if (len(file) == 0L)
                    {
                        continue;
                    }
                    if (len(libRx.FindStringSubmatch(file)) > 0L)
                    {
                        continue;
                    }
                    if (file[0L] == '[')
                    {
                        continue;
                    } 
                    // Swap what we guess is main to position 0.
                    p.Mapping[0L] = p.Mapping[i];
                    p.Mapping[i] = p.Mapping[0L];
                    break;
                } 

                // Keep the mapping IDs neatly sorted

                i = i__prev1;
                m = m__prev1;
            }

            {
                var i__prev1 = i;
                var m__prev1 = m;

                foreach (var (__i, __m) in p.Mapping)
                {
                    i = __i;
                    m = __m;
                    m.ID = uint64(i + 1L);
                }

                i = i__prev1;
                m = m__prev1;
            }

        }

        // adjacent returns whether two mapping entries represent the same
        // mapping that has been split into two. Check that their addresses are adjacent,
        // and if the offsets match, if they are available.
        private static bool adjacent(ref Mapping m1, ref Mapping m2)
        {
            if (m1.File != "" && m2.File != "")
            {
                if (m1.File != m2.File)
                {
                    return false;
                }
            }
            if (m1.BuildID != "" && m2.BuildID != "")
            {
                if (m1.BuildID != m2.BuildID)
                {
                    return false;
                }
            }
            if (m1.Limit != m2.Start)
            {
                return false;
            }
            if (m1.Offset != 0L && m2.Offset != 0L)
            {
                var offset = m1.Offset + (m1.Limit - m1.Start);
                if (offset != m2.Offset)
                {
                    return false;
                }
            }
            return true;
        }

        private static void updateLocationMapping(this ref Profile p, ref Mapping from, ref Mapping to)
        {
            foreach (var (_, l) in p.Location)
            {
                if (l.Mapping == from)
                {
                    l.Mapping = to;
                }
            }
        }

        private static slice<byte> serialize(ref Profile p)
        {
            p.encodeMu.Lock();
            p.preEncode();
            var b = marshal(p);
            p.encodeMu.Unlock();
            return b;
        }

        // Write writes the profile as a gzip-compressed marshaled protobuf.
        private static error Write(this ref Profile _p, io.Writer w) => func(_p, (ref Profile p, Defer defer, Panic _, Recover __) =>
        {
            var zw = gzip.NewWriter(w);
            defer(zw.Close());
            var (_, err) = zw.Write(serialize(p));
            return error.As(err);
        });

        // WriteUncompressed writes the profile as a marshaled protobuf.
        private static error WriteUncompressed(this ref Profile p, io.Writer w)
        {
            var (_, err) = w.Write(serialize(p));
            return error.As(err);
        }

        // CheckValid tests whether the profile is valid. Checks include, but are
        // not limited to:
        //   - len(Profile.Sample[n].value) == len(Profile.value_unit)
        //   - Sample.id has a corresponding Profile.Location
        private static error CheckValid(this ref Profile p)
        { 
            // Check that sample values are consistent
            var sampleLen = len(p.SampleType);
            if (sampleLen == 0L && len(p.Sample) != 0L)
            {
                return error.As(fmt.Errorf("missing sample type information"));
            }
            foreach (var (_, s) in p.Sample)
            {
                if (s == null)
                {
                    return error.As(fmt.Errorf("profile has nil sample"));
                }
                if (len(s.Value) != sampleLen)
                {
                    return error.As(fmt.Errorf("mismatch: sample has %d values vs. %d types", len(s.Value), len(p.SampleType)));
                }
                {
                    var l__prev2 = l;

                    foreach (var (_, __l) in s.Location)
                    {
                        l = __l;
                        if (l == null)
                        {
                            return error.As(fmt.Errorf("sample has nil location"));
                        }
                    }

                    l = l__prev2;
                }

            } 

            // Check that all mappings/locations/functions are in the tables
            // Check that there are no duplicate ids
            var mappings = make_map<ulong, ref Mapping>(len(p.Mapping));
            {
                var m__prev1 = m;

                foreach (var (_, __m) in p.Mapping)
                {
                    m = __m;
                    if (m == null)
                    {
                        return error.As(fmt.Errorf("profile has nil mapping"));
                    }
                    if (m.ID == 0L)
                    {
                        return error.As(fmt.Errorf("found mapping with reserved ID=0"));
                    }
                    if (mappings[m.ID] != null)
                    {
                        return error.As(fmt.Errorf("multiple mappings with same id: %d", m.ID));
                    }
                    mappings[m.ID] = m;
                }

                m = m__prev1;
            }

            var functions = make_map<ulong, ref Function>(len(p.Function));
            {
                var f__prev1 = f;

                foreach (var (_, __f) in p.Function)
                {
                    f = __f;
                    if (f == null)
                    {
                        return error.As(fmt.Errorf("profile has nil function"));
                    }
                    if (f.ID == 0L)
                    {
                        return error.As(fmt.Errorf("found function with reserved ID=0"));
                    }
                    if (functions[f.ID] != null)
                    {
                        return error.As(fmt.Errorf("multiple functions with same id: %d", f.ID));
                    }
                    functions[f.ID] = f;
                }

                f = f__prev1;
            }

            var locations = make_map<ulong, ref Location>(len(p.Location));
            {
                var l__prev1 = l;

                foreach (var (_, __l) in p.Location)
                {
                    l = __l;
                    if (l == null)
                    {
                        return error.As(fmt.Errorf("profile has nil location"));
                    }
                    if (l.ID == 0L)
                    {
                        return error.As(fmt.Errorf("found location with reserved id=0"));
                    }
                    if (locations[l.ID] != null)
                    {
                        return error.As(fmt.Errorf("multiple locations with same id: %d", l.ID));
                    }
                    locations[l.ID] = l;
                    {
                        var m__prev1 = m;

                        var m = l.Mapping;

                        if (m != null)
                        {
                            if (m.ID == 0L || mappings[m.ID] != m)
                            {
                                return error.As(fmt.Errorf("inconsistent mapping %p: %d", m, m.ID));
                            }
                        }

                        m = m__prev1;

                    }
                    foreach (var (_, ln) in l.Line)
                    {
                        {
                            var f__prev1 = f;

                            var f = ln.Function;

                            if (f != null)
                            {
                                if (f.ID == 0L || functions[f.ID] != f)
                                {
                                    return error.As(fmt.Errorf("inconsistent function %p: %d", f, f.ID));
                                }
                            }

                            f = f__prev1;

                        }
                    }
                }

                l = l__prev1;
            }

            return error.As(null);
        }

        // Aggregate merges the locations in the profile into equivalence
        // classes preserving the request attributes. It also updates the
        // samples to point to the merged locations.
        private static error Aggregate(this ref Profile p, bool inlineFrame, bool function, bool filename, bool linenumber, bool address)
        {
            foreach (var (_, m) in p.Mapping)
            {
                m.HasInlineFrames = m.HasInlineFrames && inlineFrame;
                m.HasFunctions = m.HasFunctions && function;
                m.HasFilenames = m.HasFilenames && filename;
                m.HasLineNumbers = m.HasLineNumbers && linenumber;
            } 

            // Aggregate functions
            if (!function || !filename)
            {
                foreach (var (_, f) in p.Function)
                {
                    if (!function)
                    {
                        f.Name = "";
                        f.SystemName = "";
                    }
                    if (!filename)
                    {
                        f.Filename = "";
                    }
                }
            } 

            // Aggregate locations
            if (!inlineFrame || !address || !linenumber)
            {
                foreach (var (_, l) in p.Location)
                {
                    if (!inlineFrame && len(l.Line) > 1L)
                    {
                        l.Line = l.Line[len(l.Line) - 1L..];
                    }
                    if (!linenumber)
                    {
                        foreach (var (i) in l.Line)
                        {
                            l.Line[i].Line = 0L;
                        }
                    }
                    if (!address)
                    {
                        l.Address = 0L;
                    }
                }
            }
            return error.As(p.CheckValid());
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
        private static (map<@string, @string>, map<@string, slice<@string>>) NumLabelUnits(this ref Profile p)
        {
            map numLabelUnits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
            map ignoredUnits = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, map<@string, bool>>{};
            map encounteredKeys = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{}; 

            // Determine units based on numeric tags for each sample.
            foreach (var (_, s) in p.Sample)
            {
                foreach (var (k) in s.NumLabel)
                {
                    encounteredKeys[k] = true;
                    {
                        var unit__prev3 = unit;

                        foreach (var (_, __unit) in s.NumUnit[k])
                        {
                            unit = __unit;
                            if (unit == "")
                            {
                                continue;
                            }
                            {
                                var (wantUnit, ok) = numLabelUnits[k];

                                if (!ok)
                                {
                                    numLabelUnits[k] = unit;
                                }
                                else if (wantUnit != unit)
                                {
                                    {
                                        var (v, ok) = ignoredUnits[k];

                                        if (ok)
                                        {
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
            } 
            // Infer units for keys without any units associated with
            // numeric tag values.
            {
                var key__prev1 = key;

                foreach (var (__key) in encounteredKeys)
                {
                    key = __key;
                    var unit = numLabelUnits[key];
                    if (unit == "")
                    {
                        switch (key)
                        {
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

                // Copy ignored units into more readable format

                key = key__prev1;
            }

            var unitsIgnored = make_map<@string, slice<@string>>(len(ignoredUnits));
            {
                var key__prev1 = key;

                foreach (var (__key, __values) in ignoredUnits)
                {
                    key = __key;
                    values = __values;
                    var units = make_slice<@string>(len(values));
                    long i = 0L;
                    {
                        var unit__prev2 = unit;

                        foreach (var (__unit) in values)
                        {
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
        private static @string String(this ref Profile p)
        {
            var ss = make_slice<@string>(0L, len(p.Comments) + len(p.Sample) + len(p.Mapping) + len(p.Location));
            foreach (var (_, c) in p.Comments)
            {
                ss = append(ss, "Comment: " + c);
            }
            {
                var pt = p.PeriodType;

                if (pt != null)
                {
                    ss = append(ss, fmt.Sprintf("PeriodType: %s %s", pt.Type, pt.Unit));
                }

            }
            ss = append(ss, fmt.Sprintf("Period: %d", p.Period));
            if (p.TimeNanos != 0L)
            {
                ss = append(ss, fmt.Sprintf("Time: %v", time.Unix(0L, p.TimeNanos)));
            }
            if (p.DurationNanos != 0L)
            {
                ss = append(ss, fmt.Sprintf("Duration: %.4v", time.Duration(p.DurationNanos)));
            }
            ss = append(ss, "Samples:");
            @string sh1 = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.SampleType)
                {
                    s = __s;
                    @string dflt = "";
                    if (s.Type == p.DefaultSampleType)
                    {
                        dflt = "[dflt]";
                    }
                    sh1 = sh1 + fmt.Sprintf("%s/%s%s ", s.Type, s.Unit, dflt);
                }

                s = s__prev1;
            }

            ss = append(ss, strings.TrimSpace(sh1));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample)
                {
                    s = __s;
                    ss = append(ss, s.@string());
                }

                s = s__prev1;
            }

            ss = append(ss, "Locations");
            foreach (var (_, l) in p.Location)
            {
                ss = append(ss, l.@string());
            }
            ss = append(ss, "Mappings");
            foreach (var (_, m) in p.Mapping)
            {
                ss = append(ss, m.@string());
            }
            return strings.Join(ss, "\n") + "\n";
        }

        // string dumps a text representation of a mapping. Intended mainly
        // for debugging purposes.
        private static @string @string(this ref Mapping m)
        {
            @string bits = "";
            if (m.HasFunctions)
            {
                bits = bits + "[FN]";
            }
            if (m.HasFilenames)
            {
                bits = bits + "[FL]";
            }
            if (m.HasLineNumbers)
            {
                bits = bits + "[LN]";
            }
            if (m.HasInlineFrames)
            {
                bits = bits + "[IN]";
            }
            return fmt.Sprintf("%d: %#x/%#x/%#x %s %s %s", m.ID, m.Start, m.Limit, m.Offset, m.File, m.BuildID, bits);
        }

        // string dumps a text representation of a location. Intended mainly
        // for debugging purposes.
        private static @string @string(this ref Location l)
        {
            @string ss = new slice<@string>(new @string[] {  });
            var locStr = fmt.Sprintf("%6d: %#x ", l.ID, l.Address);
            {
                var m = l.Mapping;

                if (m != null)
                {
                    locStr = locStr + fmt.Sprintf("M=%d ", m.ID);
                }

            }
            if (len(l.Line) == 0L)
            {
                ss = append(ss, locStr);
            }
            foreach (var (li) in l.Line)
            {
                @string lnStr = "??";
                {
                    var fn = l.Line[li].Function;

                    if (fn != null)
                    {
                        lnStr = fmt.Sprintf("%s %s:%d s=%d", fn.Name, fn.Filename, l.Line[li].Line, fn.StartLine);
                        if (fn.Name != fn.SystemName)
                        {
                            lnStr = lnStr + "(" + fn.SystemName + ")";
                        }
                    }

                }
                ss = append(ss, locStr + lnStr); 
                // Do not print location details past the first line
                locStr = "             ";
            }
            return strings.Join(ss, "\n");
        }

        // string dumps a text representation of a sample. Intended mainly
        // for debugging purposes.
        private static @string @string(this ref Sample s)
        {
            @string ss = new slice<@string>(new @string[] {  });
            @string sv = default;
            foreach (var (_, v) in s.Value)
            {
                sv = fmt.Sprintf("%s %10d", sv, v);
            }
            sv = sv + ": ";
            foreach (var (_, l) in s.Location)
            {
                sv = sv + fmt.Sprintf("%d ", l.ID);
            }
            ss = append(ss, sv);
            const @string labelHeader = "                ";

            if (len(s.Label) > 0L)
            {
                ss = append(ss, labelHeader + labelsToString(s.Label));
            }
            if (len(s.NumLabel) > 0L)
            {
                ss = append(ss, labelHeader + numLabelsToString(s.NumLabel, s.NumUnit));
            }
            return strings.Join(ss, "\n");
        }

        // labelsToString returns a string representation of a
        // map representing labels.
        private static @string labelsToString(map<@string, slice<@string>> labels)
        {
            @string ls = new slice<@string>(new @string[] {  });
            foreach (var (k, v) in labels)
            {
                ls = append(ls, fmt.Sprintf("%s:%v", k, v));
            }
            sort.Strings(ls);
            return strings.Join(ls, " ");
        }

        // numLablesToString returns a string representation of a map
        // representing numeric labels.
        private static @string numLabelsToString(map<@string, slice<long>> numLabels, map<@string, slice<@string>> numUnits)
        {
            @string ls = new slice<@string>(new @string[] {  });
            foreach (var (k, v) in numLabels)
            {
                var units = numUnits[k];
                @string labelString = default;
                if (len(units) == len(v))
                {
                    var values = make_slice<@string>(len(v));
                    foreach (var (i, vv) in v)
                    {
                        values[i] = fmt.Sprintf("%d %s", vv, units[i]);
                    }
                else
                    labelString = fmt.Sprintf("%s:%v", k, values);
                }                {
                    labelString = fmt.Sprintf("%s:%v", k, v);
                }
                ls = append(ls, labelString);
            }
            sort.Strings(ls);
            return strings.Join(ls, " ");
        }

        // Scale multiplies all sample values in a profile by a constant.
        private static void Scale(this ref Profile p, double ratio)
        {
            if (ratio == 1L)
            {
                return;
            }
            var ratios = make_slice<double>(len(p.SampleType));
            foreach (var (i) in p.SampleType)
            {
                ratios[i] = ratio;
            }
            p.ScaleN(ratios);
        }

        // ScaleN multiplies each sample values in a sample by a different amount.
        private static error ScaleN(this ref Profile p, slice<double> ratios)
        {
            if (len(p.SampleType) != len(ratios))
            {
                return error.As(fmt.Errorf("mismatched scale ratios, got %d, want %d", len(ratios), len(p.SampleType)));
            }
            var allOnes = true;
            foreach (var (_, r) in ratios)
            {
                if (r != 1L)
                {
                    allOnes = false;
                    break;
                }
            }
            if (allOnes)
            {
                return error.As(null);
            }
            foreach (var (_, s) in p.Sample)
            {
                foreach (var (i, v) in s.Value)
                {
                    if (ratios[i] != 1L)
                    {
                        s.Value[i] = int64(float64(v) * ratios[i]);
                    }
                }
            }
            return error.As(null);
        }

        // HasFunctions determines if all locations in this profile have
        // symbolized function information.
        private static bool HasFunctions(this ref Profile p)
        {
            foreach (var (_, l) in p.Location)
            {
                if (l.Mapping != null && !l.Mapping.HasFunctions)
                {
                    return false;
                }
            }
            return true;
        }

        // HasFileLines determines if all locations in this profile have
        // symbolized file and line number information.
        private static bool HasFileLines(this ref Profile p)
        {
            foreach (var (_, l) in p.Location)
            {
                if (l.Mapping != null && (!l.Mapping.HasFilenames || !l.Mapping.HasLineNumbers))
                {
                    return false;
                }
            }
            return true;
        }

        // Unsymbolizable returns true if a mapping points to a binary for which
        // locations can't be symbolized in principle, at least now. Examples are
        // "[vdso]", [vsyscall]" and some others, see the code.
        private static bool Unsymbolizable(this ref Mapping m)
        {
            var name = filepath.Base(m.File);
            return strings.HasPrefix(name, "[") || strings.HasPrefix(name, "linux-vdso") || strings.HasPrefix(m.File, "/dev/dri/");
        }

        // Copy makes a fully independent copy of a profile.
        private static ref Profile Copy(this ref Profile _p) => func(_p, (ref Profile p, Defer _, Panic panic, Recover __) =>
        {
            Profile pp = ref new Profile();
            {
                var err__prev1 = err;

                var err = unmarshal(serialize(p), pp);

                if (err != null)
                {
                    panic(err);
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = pp.postDecode();

                if (err != null)
                {
                    panic(err);
                }

                err = err__prev1;

            }

            return pp;
        });
    }
}}}}}}
