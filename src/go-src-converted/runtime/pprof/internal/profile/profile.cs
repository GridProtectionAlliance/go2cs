// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package profile provides a representation of profile.proto and
// methods to encode/decode profiles in this format.
//
// This package is only for testing runtime/pprof.
// It is not used by production Go programs.
// package profile -- go2cs converted at 2020 August 29 08:24:18 UTC
// import "runtime/pprof/internal/profile" ==> using profile = go.runtime.pprof.@internal.profile_package
// Original source: C:\Go\src\runtime\pprof\internal\profile\profile.go
using bytes = go.bytes_package;
using gzip = go.compress.gzip_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using regexp = go.regexp_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace runtime {
namespace pprof {
namespace @internal
{
    public static partial class profile_package
    {
        // Profile is an in-memory representation of profile.proto.
        public partial struct Profile
        {
            public slice<ref ValueType> SampleType;
            public slice<ref Sample> Sample;
            public slice<ref Mapping> Mapping;
            public slice<ref Location> Location;
            public slice<ref Function> Function;
            public @string DropFrames;
            public @string KeepFrames;
            public long TimeNanos;
            public long DurationNanos;
            public ptr<ValueType> PeriodType;
            public long Period;
            public long dropFramesX;
            public long keepFramesX;
            public slice<@string> stringTable;
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
            public slice<ulong> locationIDX;
            public slice<Label> labelX;
        }

        // Label corresponds to Profile.Label
        public partial struct Label
        {
            public long keyX; // Exactly one of the two following values must be set
            public long strX;
            public long numX; // Integer value for this label
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
            var (orig, err) = ioutil.ReadAll(r);
            if (err != null)
            {
                return (null, err);
            }
            ref Profile p = default;
            if (len(orig) >= 2L && orig[0L] == 0x1fUL && orig[1L] == 0x8bUL)
            {
                var (gz, err) = gzip.NewReader(bytes.NewBuffer(orig));
                if (err != null)
                {
                    return (null, fmt.Errorf("decompressing profile: %v", err));
                }
                var (data, err) = ioutil.ReadAll(gz);
                if (err != null)
                {
                    return (null, fmt.Errorf("decompressing profile: %v", err));
                }
                orig = data;
            }
            p, err = parseUncompressed(orig);

            if (err != null)
            {
                p, err = parseLegacy(orig);

                if (err != null)
                {
                    return (null, fmt.Errorf("parsing profile: %v", err));
                }
            }
            {
                var err = p.CheckValid();

                if (err != null)
                {
                    return (null, fmt.Errorf("malformed profile: %v", err));
                }

            }
            return (p, null);
        }

        private static var errUnrecognized = fmt.Errorf("unrecognized profile format");
        private static var errMalformed = fmt.Errorf("malformed profile format");

        private static (ref Profile, error) parseLegacy(slice<byte> data)
        {
            Func<slice<byte>, (ref Profile, error)> parsers = new slice<Func<slice<byte>, (ref Profile, error)>>(new Func<slice<byte>, (ref Profile, error)>[] { parseCPU, parseHeap, parseGoCount, parseThread, parseContention });

            foreach (var (_, parser) in parsers)
            {
                var (p, err) = parser(data);
                if (err == null)
                {
                    p.setMain();
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

        private static (ref Profile, error) parseUncompressed(slice<byte> data)
        {
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

        // setMain scans Mapping entries and guesses which entry is main
        // because legacy profiles don't obey the convention of putting main
        // first.
        private static void setMain(this ref Profile p)
        {
            for (long i = 0L; i < len(p.Mapping); i++)
            {
                var file = strings.TrimSpace(strings.Replace(p.Mapping[i].File, "(deleted)", "", -1L));
                if (len(file) == 0L)
                {
                    continue;
                }
                if (len(libRx.FindStringSubmatch(file)) > 0L)
                {
                    continue;
                }
                if (strings.HasPrefix(file, "["))
                {
                    continue;
                } 
                // Swap what we guess is main to position 0.
                var tmp = p.Mapping[i];
                p.Mapping[i] = p.Mapping[0L];
                p.Mapping[0L] = tmp;
                break;
            }

        }

        // Write writes the profile as a gzip-compressed marshaled protobuf.
        private static error Write(this ref Profile _p, io.Writer w) => func(_p, (ref Profile p, Defer defer, Panic _, Recover __) =>
        {
            p.preEncode();
            var b = marshal(p);
            var zw = gzip.NewWriter(w);
            defer(zw.Close());
            var (_, err) = zw.Write(b);
            return error.As(err);
        });

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
                if (len(s.Value) != sampleLen)
                {
                    return error.As(fmt.Errorf("mismatch: sample has: %d values vs. %d types", len(s.Value), len(p.SampleType)));
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
            foreach (var (_, l) in p.Location)
            {
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

        // Print dumps a text representation of a profile. Intended mainly
        // for debugging purposes.
        private static @string String(this ref Profile p)
        {
            var ss = make_slice<@string>(0L, len(p.Sample) + len(p.Mapping) + len(p.Location));
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
                ss = append(ss, fmt.Sprintf("Duration: %v", time.Duration(p.DurationNanos)));
            }
            ss = append(ss, "Samples:");
            @string sh1 = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.SampleType)
                {
                    s = __s;
                    sh1 = sh1 + fmt.Sprintf("%s/%s ", s.Type, s.Unit);
                }

                s = s__prev1;
            }

            ss = append(ss, strings.TrimSpace(sh1));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample)
                {
                    s = __s;
                    @string sv = default;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in s.Value)
                        {
                            v = __v;
                            sv = fmt.Sprintf("%s %10d", sv, v);
                        }

                        v = v__prev2;
                    }

                    sv = sv + ": ";
                    {
                        var l__prev2 = l;

                        foreach (var (_, __l) in s.Location)
                        {
                            l = __l;
                            sv = sv + fmt.Sprintf("%d ", l.ID);
                        }

                        l = l__prev2;
                    }

                    ss = append(ss, sv);
                    const @string labelHeader = "                ";

                    if (len(s.Label) > 0L)
                    {
                        var ls = labelHeader;
                        {
                            var k__prev2 = k;
                            var v__prev2 = v;

                            foreach (var (__k, __v) in s.Label)
                            {
                                k = __k;
                                v = __v;
                                ls = ls + fmt.Sprintf("%s:%v ", k, v);
                            }

                            k = k__prev2;
                            v = v__prev2;
                        }

                        ss = append(ss, ls);
                    }
                    if (len(s.NumLabel) > 0L)
                    {
                        ls = labelHeader;
                        {
                            var k__prev2 = k;
                            var v__prev2 = v;

                            foreach (var (__k, __v) in s.NumLabel)
                            {
                                k = __k;
                                v = __v;
                                ls = ls + fmt.Sprintf("%s:%v ", k, v);
                            }

                            k = k__prev2;
                            v = v__prev2;
                        }

                        ss = append(ss, ls);
                    }
                }

                s = s__prev1;
            }

            ss = append(ss, "Locations");
            {
                var l__prev1 = l;

                foreach (var (_, __l) in p.Location)
                {
                    l = __l;
                    var locStr = fmt.Sprintf("%6d: %#x ", l.ID, l.Address);
                    {
                        var m__prev1 = m;

                        var m = l.Mapping;

                        if (m != null)
                        {
                            locStr = locStr + fmt.Sprintf("M=%d ", m.ID);
                        }

                        m = m__prev1;

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
                }

                l = l__prev1;
            }

            ss = append(ss, "Mappings");
            {
                var m__prev1 = m;

                foreach (var (_, __m) in p.Mapping)
                {
                    m = __m;
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
                    ss = append(ss, fmt.Sprintf("%d: %#x/%#x/%#x %s %s %s", m.ID, m.Start, m.Limit, m.Offset, m.File, m.BuildID, bits));
                }

                m = m__prev1;
            }

            return strings.Join(ss, "\n") + "\n";
        }

        // Merge adds profile p adjusted by ratio r into profile p. Profiles
        // must be compatible (same Type and SampleType).
        // TODO(rsilvera): consider normalizing the profiles based on the
        // total samples collected.
        private static error Merge(this ref Profile p, ref Profile pb, double r)
        {
            {
                var err = p.Compatible(pb);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            pb = pb.Copy(); 

            // Keep the largest of the two periods.
            if (pb.Period > p.Period)
            {
                p.Period = pb.Period;
            }
            p.DurationNanos += pb.DurationNanos;

            p.Mapping = append(p.Mapping, pb.Mapping);
            {
                var i__prev1 = i;

                foreach (var (__i, __m) in p.Mapping)
                {
                    i = __i;
                    m = __m;
                    m.ID = uint64(i + 1L);
                }

                i = i__prev1;
            }

            p.Location = append(p.Location, pb.Location);
            {
                var i__prev1 = i;

                foreach (var (__i, __l) in p.Location)
                {
                    i = __i;
                    l = __l;
                    l.ID = uint64(i + 1L);
                }

                i = i__prev1;
            }

            p.Function = append(p.Function, pb.Function);
            {
                var i__prev1 = i;

                foreach (var (__i, __f) in p.Function)
                {
                    i = __i;
                    f = __f;
                    f.ID = uint64(i + 1L);
                }

                i = i__prev1;
            }

            if (r != 1.0F)
            {
                foreach (var (_, s) in pb.Sample)
                {
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __v) in s.Value)
                        {
                            i = __i;
                            v = __v;
                            s.Value[i] = int64((float64(v) * r));
                        }

                        i = i__prev2;
                    }

                }
            }
            p.Sample = append(p.Sample, pb.Sample);
            return error.As(p.CheckValid());
        }

        // Compatible determines if two profiles can be compared/merged.
        // returns nil if the profiles are compatible; otherwise an error with
        // details on the incompatibility.
        private static error Compatible(this ref Profile p, ref Profile pb)
        {
            if (!compatibleValueTypes(p.PeriodType, pb.PeriodType))
            {
                return error.As(fmt.Errorf("incompatible period types %v and %v", p.PeriodType, pb.PeriodType));
            }
            if (len(p.SampleType) != len(pb.SampleType))
            {
                return error.As(fmt.Errorf("incompatible sample types %v and %v", p.SampleType, pb.SampleType));
            }
            foreach (var (i) in p.SampleType)
            {
                if (!compatibleValueTypes(p.SampleType[i], pb.SampleType[i]))
                {
                    return error.As(fmt.Errorf("incompatible sample types %v and %v", p.SampleType, pb.SampleType));
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
                if (l.Mapping == null || !l.Mapping.HasFunctions)
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
                if (l.Mapping == null || (!l.Mapping.HasFilenames || !l.Mapping.HasLineNumbers))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool compatibleValueTypes(ref ValueType v1, ref ValueType v2)
        {
            if (v1 == null || v2 == null)
            {
                return true; // No grounds to disqualify.
            }
            return v1.Type == v2.Type && v1.Unit == v2.Unit;
        }

        // Copy makes a fully independent copy of a profile.
        private static ref Profile Copy(this ref Profile _p) => func(_p, (ref Profile p, Defer _, Panic panic, Recover __) =>
        {
            p.preEncode();
            var b = marshal(p);

            Profile pp = ref new Profile();
            {
                var err__prev1 = err;

                var err = unmarshal(b, pp);

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

        // Demangler maps symbol names to a human-readable form. This may
        // include C++ demangling and additional simplification. Names that
        // are not demangled may be missing from the resulting map.
        public delegate  error) Demangler(slice<@string>,  (map<@string,  @string>);

        // Demangle attempts to demangle and optionally simplify any function
        // names referenced in the profile. It works on a best-effort basis:
        // it will silently preserve the original names in case of any errors.
        private static error Demangle(this ref Profile p, Demangler d)
        { 
            // Collect names to demangle.
            slice<@string> names = default;
            {
                var fn__prev1 = fn;

                foreach (var (_, __fn) in p.Function)
                {
                    fn = __fn;
                    names = append(names, fn.SystemName);
                } 

                // Update profile with demangled names.

                fn = fn__prev1;
            }

            var (demangled, err) = d(names);
            if (err != null)
            {
                return error.As(err);
            }
            {
                var fn__prev1 = fn;

                foreach (var (_, __fn) in p.Function)
                {
                    fn = __fn;
                    {
                        var (dd, ok) = demangled[fn.SystemName];

                        if (ok)
                        {
                            fn.Name = dd;
                        }

                    }
                }

                fn = fn__prev1;
            }

            return error.As(null);
        }

        // Empty returns true if the profile contains no samples.
        private static bool Empty(this ref Profile p)
        {
            return len(p.Sample) == 0L;
        }
    }
}}}}
