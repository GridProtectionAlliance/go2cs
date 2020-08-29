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

// package profile -- go2cs converted at 2020 August 29 10:06:29 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\profile\merge.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof
{
    public static partial class profile_package
    {
        // Compact performs garbage collection on a profile to remove any
        // unreferenced fields. This is useful to reduce the size of a profile
        // after samples or locations have been removed.
        private static ref Profile Compact(this ref Profile p)
        {
            p, _ = Merge(new slice<ref Profile>(new ref Profile[] { p }));
            return p;
        }

        // Merge merges all the profiles in profs into a single Profile.
        // Returns a new profile independent of the input profiles. The merged
        // profile is compacted to eliminate unused samples, locations,
        // functions and mappings. Profiles must have identical profile sample
        // and period types or the merge will fail. profile.Period of the
        // resulting profile will be the maximum of all profiles, and
        // profile.TimeNanos will be the earliest nonzero one.
        public static (ref Profile, error) Merge(slice<ref Profile> srcs)
        {
            if (len(srcs) == 0L)
            {
                return (null, fmt.Errorf("no profiles to merge"));
            }
            var (p, err) = combineHeaders(srcs);
            if (err != null)
            {
                return (null, err);
            }
            profileMerger pm = ref new profileMerger(p:p,samples:make(map[sampleKey]*Sample,len(srcs[0].Sample)),locations:make(map[locationKey]*Location,len(srcs[0].Location)),functions:make(map[functionKey]*Function,len(srcs[0].Function)),mappings:make(map[mappingKey]*Mapping,len(srcs[0].Mapping)),);

            foreach (var (_, src) in srcs)
            { 
                // Clear the profile-specific hash tables
                pm.locationsByID = make_map<ulong, ref Location>(len(src.Location));
                pm.functionsByID = make_map<ulong, ref Function>(len(src.Function));
                pm.mappingsByID = make_map<ulong, mapInfo>(len(src.Mapping));

                if (len(pm.mappings) == 0L && len(src.Mapping) > 0L)
                { 
                    // The Mapping list has the property that the first mapping
                    // represents the main binary. Take the first Mapping we see,
                    // otherwise the operations below will add mappings in an
                    // arbitrary order.
                    pm.mapMapping(srcs[0L].Mapping[0L]);
                }
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in src.Sample)
                    {
                        s = __s;
                        if (!isZeroSample(s))
                        {
                            pm.mapSample(s);
                        }
                    }

                    s = s__prev2;
                }

            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample)
                {
                    s = __s;
                    if (isZeroSample(s))
                    { 
                        // If there are any zero samples, re-merge the profile to GC
                        // them.
                        return Merge(new slice<ref Profile>(new ref Profile[] { p }));
                    }
                }

                s = s__prev1;
            }

            return (p, null);
        }

        // Normalize normalizes the source profile by multiplying each value in profile by the
        // ratio of the sum of the base profile's values of that sample type to the sum of the
        // source profile's value of that sample type.
        private static error Normalize(this ref Profile p, ref Profile pb)
        {
            {
                var err = p.compatible(pb);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            var baseVals = make_slice<long>(len(p.SampleType));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in pb.Sample)
                {
                    s = __s;
                    {
                        var i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in s.Value)
                        {
                            i = __i;
                            v = __v;
                            baseVals[i] += v;
                        }

                        i = i__prev2;
                        v = v__prev2;
                    }

                }

                s = s__prev1;
            }

            var srcVals = make_slice<long>(len(p.SampleType));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample)
                {
                    s = __s;
                    {
                        var i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in s.Value)
                        {
                            i = __i;
                            v = __v;
                            srcVals[i] += v;
                        }

                        i = i__prev2;
                        v = v__prev2;
                    }

                }

                s = s__prev1;
            }

            var normScale = make_slice<double>(len(baseVals));
            {
                var i__prev1 = i;

                foreach (var (__i) in baseVals)
                {
                    i = __i;
                    if (srcVals[i] == 0L)
                    {
                        normScale[i] = 0.0F;
                    }
                    else
                    {
                        normScale[i] = float64(baseVals[i]) / float64(srcVals[i]);
                    }
                }

                i = i__prev1;
            }

            p.ScaleN(normScale);
            return error.As(null);
        }

        private static bool isZeroSample(ref Sample s)
        {
            foreach (var (_, v) in s.Value)
            {
                if (v != 0L)
                {
                    return false;
                }
            }
            return true;
        }

        private partial struct profileMerger
        {
            public ptr<Profile> p; // Memoization tables within a profile.
            public map<ulong, ref Location> locationsByID;
            public map<ulong, ref Function> functionsByID;
            public map<ulong, mapInfo> mappingsByID; // Memoization tables for profile entities.
            public map<sampleKey, ref Sample> samples;
            public map<locationKey, ref Location> locations;
            public map<functionKey, ref Function> functions;
            public map<mappingKey, ref Mapping> mappings;
        }

        private partial struct mapInfo
        {
            public ptr<Mapping> m;
            public long offset;
        }

        private static ref Sample mapSample(this ref profileMerger pm, ref Sample src)
        {
            Sample s = ref new Sample(Location:make([]*Location,len(src.Location)),Value:make([]int64,len(src.Value)),Label:make(map[string][]string,len(src.Label)),NumLabel:make(map[string][]int64,len(src.NumLabel)),NumUnit:make(map[string][]string,len(src.NumLabel)),);
            {
                var i__prev1 = i;

                foreach (var (__i, __l) in src.Location)
                {
                    i = __i;
                    l = __l;
                    s.Location[i] = pm.mapLocation(l);
                }

                i = i__prev1;
            }

            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in src.Label)
                {
                    k = __k;
                    v = __v;
                    var vv = make_slice<@string>(len(v));
                    copy(vv, v);
                    s.Label[k] = vv;
                }

                k = k__prev1;
                v = v__prev1;
            }

            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in src.NumLabel)
                {
                    k = __k;
                    v = __v;
                    var u = src.NumUnit[k];
                    vv = make_slice<long>(len(v));
                    var uu = make_slice<@string>(len(u));
                    copy(vv, v);
                    copy(uu, u);
                    s.NumLabel[k] = vv;
                    s.NumUnit[k] = uu;
                } 
                // Check memoization table. Must be done on the remapped location to
                // account for the remapped mapping. Add current values to the
                // existing sample.

                k = k__prev1;
                v = v__prev1;
            }

            var k = s.key();
            {
                var (ss, ok) = pm.samples[k];

                if (ok)
                {
                    {
                        var i__prev1 = i;
                        var v__prev1 = v;

                        foreach (var (__i, __v) in src.Value)
                        {
                            i = __i;
                            v = __v;
                            ss.Value[i] += v;
                        }

                        i = i__prev1;
                        v = v__prev1;
                    }

                    return ss;
                }

            }
            copy(s.Value, src.Value);
            pm.samples[k] = s;
            pm.p.Sample = append(pm.p.Sample, s);
            return s;
        }

        // key generates sampleKey to be used as a key for maps.
        private static sampleKey key(this ref Sample sample)
        {
            var ids = make_slice<@string>(len(sample.Location));
            foreach (var (i, l) in sample.Location)
            {
                ids[i] = strconv.FormatUint(l.ID, 16L);
            }
            var labels = make_slice<@string>(0L, len(sample.Label));
            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in sample.Label)
                {
                    k = __k;
                    v = __v;
                    labels = append(labels, fmt.Sprintf("%q%q", k, v));
                }

                k = k__prev1;
                v = v__prev1;
            }

            sort.Strings(labels);

            var numlabels = make_slice<@string>(0L, len(sample.NumLabel));
            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in sample.NumLabel)
                {
                    k = __k;
                    v = __v;
                    numlabels = append(numlabels, fmt.Sprintf("%q%x%x", k, v, sample.NumUnit[k]));
                }

                k = k__prev1;
                v = v__prev1;
            }

            sort.Strings(numlabels);

            return new sampleKey(strings.Join(ids,"|"),strings.Join(labels,""),strings.Join(numlabels,""),);
        }

        private partial struct sampleKey
        {
            public @string locations;
            public @string labels;
            public @string numlabels;
        }

        private static ref Location mapLocation(this ref profileMerger pm, ref Location src)
        {
            if (src == null)
            {
                return null;
            }
            {
                var l__prev1 = l;

                var (l, ok) = pm.locationsByID[src.ID];

                if (ok)
                {
                    pm.locationsByID[src.ID] = l;
                    return l;
                }

                l = l__prev1;

            }

            var mi = pm.mapMapping(src.Mapping);
            Location l = ref new Location(ID:uint64(len(pm.p.Location)+1),Mapping:mi.m,Address:uint64(int64(src.Address)+mi.offset),Line:make([]Line,len(src.Line)),);
            foreach (var (i, ln) in src.Line)
            {
                l.Line[i] = pm.mapLine(ln);
            } 
            // Check memoization table. Must be done on the remapped location to
            // account for the remapped mapping ID.
            var k = l.key();
            {
                var (ll, ok) = pm.locations[k];

                if (ok)
                {
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
        private static locationKey key(this ref Location l)
        {
            locationKey key = new locationKey(addr:l.Address,);
            if (l.Mapping != null)
            { 
                // Normalizes address to handle address space randomization.
                key.addr -= l.Mapping.Start;
                key.mappingID = l.Mapping.ID;
            }
            var lines = make_slice<@string>(len(l.Line) * 2L);
            foreach (var (i, line) in l.Line)
            {
                if (line.Function != null)
                {
                    lines[i * 2L] = strconv.FormatUint(line.Function.ID, 16L);
                }
                lines[i * 2L + 1L] = strconv.FormatInt(line.Line, 16L);
            }
            key.lines = strings.Join(lines, "|");
            return key;
        }

        private partial struct locationKey
        {
            public ulong addr;
            public ulong mappingID;
            public @string lines;
        }

        private static mapInfo mapMapping(this ref profileMerger pm, ref Mapping src)
        {
            if (src == null)
            {
                return new mapInfo();
            }
            {
                var mi__prev1 = mi;

                var (mi, ok) = pm.mappingsByID[src.ID];

                if (ok)
                {
                    return mi;
                } 

                // Check memoization tables.

                mi = mi__prev1;

            } 

            // Check memoization tables.
            var (bk, pk) = src.key();
            if (src.BuildID != "")
            {
                {
                    var m__prev2 = m;

                    var (m, ok) = pm.mappings[bk];

                    if (ok)
                    {
                        mapInfo mi = new mapInfo(m,int64(m.Start)-int64(src.Start));
                        pm.mappingsByID[src.ID] = mi;
                        return mi;
                    }

                    m = m__prev2;

                }
            }
            if (src.File != "")
            {
                {
                    var m__prev2 = m;

                    (m, ok) = pm.mappings[pk];

                    if (ok)
                    {
                        mi = new mapInfo(m,int64(m.Start)-int64(src.Start));
                        pm.mappingsByID[src.ID] = mi;
                        return mi;
                    }

                    m = m__prev2;

                }
            }
            Mapping m = ref new Mapping(ID:uint64(len(pm.p.Mapping)+1),Start:src.Start,Limit:src.Limit,Offset:src.Offset,File:src.File,BuildID:src.BuildID,HasFunctions:src.HasFunctions,HasFilenames:src.HasFilenames,HasLineNumbers:src.HasLineNumbers,HasInlineFrames:src.HasInlineFrames,);
            pm.p.Mapping = append(pm.p.Mapping, m); 

            // Update memoization tables.
            if (m.BuildID != "")
            {
                pm.mappings[bk] = m;
            }
            if (m.File != "")
            {
                pm.mappings[pk] = m;
            }
            mi = new mapInfo(m,0);
            pm.mappingsByID[src.ID] = mi;
            return mi;
        }

        // key generates encoded strings of Mapping to be used as a key for
        // maps. The first key represents only the build id, while the second
        // represents only the file path.
        private static (mappingKey, mappingKey) key(this ref Mapping m)
        { 
            // Normalize addresses to handle address space randomization.
            // Round up to next 4K boundary to avoid minor discrepancies.
            const ulong mapsizeRounding = 0x1000UL;



            var size = m.Limit - m.Start;
            size = size + mapsizeRounding - 1L;
            size = size - (size % mapsizeRounding);

            buildIDKey = new mappingKey(size,m.Offset,m.BuildID,);

            pathKey = new mappingKey(size,m.Offset,m.File,);
            return;
        }

        private partial struct mappingKey
        {
            public ulong size;
            public ulong offset;
            public @string buildidIDOrFile;
        }

        private static Line mapLine(this ref profileMerger pm, Line src)
        {
            Line ln = new Line(Function:pm.mapFunction(src.Function),Line:src.Line,);
            return ln;
        }

        private static ref Function mapFunction(this ref profileMerger pm, ref Function src)
        {
            if (src == null)
            {
                return null;
            }
            {
                var f__prev1 = f;

                var (f, ok) = pm.functionsByID[src.ID];

                if (ok)
                {
                    return f;
                }

                f = f__prev1;

            }
            var k = src.key();
            {
                var f__prev1 = f;

                (f, ok) = pm.functions[k];

                if (ok)
                {
                    pm.functionsByID[src.ID] = f;
                    return f;
                }

                f = f__prev1;

            }
            Function f = ref new Function(ID:uint64(len(pm.p.Function)+1),Name:src.Name,SystemName:src.SystemName,Filename:src.Filename,StartLine:src.StartLine,);
            pm.functions[k] = f;
            pm.functionsByID[src.ID] = f;
            pm.p.Function = append(pm.p.Function, f);
            return f;
        }

        // key generates a struct to be used as a key for maps.
        private static functionKey key(this ref Function f)
        {
            return new functionKey(f.StartLine,f.Name,f.SystemName,f.Filename,);
        }

        private partial struct functionKey
        {
            public long startLine;
            public @string name;
            public @string systemName;
            public @string fileName;
        }

        // combineHeaders checks that all profiles can be merged and returns
        // their combined profile.
        private static (ref Profile, error) combineHeaders(slice<ref Profile> srcs)
        {
            {
                var s__prev1 = s;

                foreach (var (_, __s) in srcs[1L..])
                {
                    s = __s;
                    {
                        var err = srcs[0L].compatible(s);

                        if (err != null)
                        {
                            return (null, err);
                        }

                    }
                }

                s = s__prev1;
            }

            long timeNanos = default;            long durationNanos = default;            long period = default;

            slice<@string> comments = default;
            @string defaultSampleType = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in srcs)
                {
                    s = __s;
                    if (timeNanos == 0L || s.TimeNanos < timeNanos)
                    {
                        timeNanos = s.TimeNanos;
                    }
                    durationNanos += s.DurationNanos;
                    if (period == 0L || period < s.Period)
                    {
                        period = s.Period;
                    }
                    comments = append(comments, s.Comments);
                    if (defaultSampleType == "")
                    {
                        defaultSampleType = s.DefaultSampleType;
                    }
                }

                s = s__prev1;
            }

            Profile p = ref new Profile(SampleType:make([]*ValueType,len(srcs[0].SampleType)),DropFrames:srcs[0].DropFrames,KeepFrames:srcs[0].KeepFrames,TimeNanos:timeNanos,DurationNanos:durationNanos,PeriodType:srcs[0].PeriodType,Period:period,Comments:comments,DefaultSampleType:defaultSampleType,);
            copy(p.SampleType, srcs[0L].SampleType);
            return (p, null);
        }

        // compatible determines if two profiles can be compared/merged.
        // returns nil if the profiles are compatible; otherwise an error with
        // details on the incompatibility.
        private static error compatible(this ref Profile p, ref Profile pb)
        {
            if (!equalValueType(p.PeriodType, pb.PeriodType))
            {
                return error.As(fmt.Errorf("incompatible period types %v and %v", p.PeriodType, pb.PeriodType));
            }
            if (len(p.SampleType) != len(pb.SampleType))
            {
                return error.As(fmt.Errorf("incompatible sample types %v and %v", p.SampleType, pb.SampleType));
            }
            foreach (var (i) in p.SampleType)
            {
                if (!equalValueType(p.SampleType[i], pb.SampleType[i]))
                {
                    return error.As(fmt.Errorf("incompatible sample types %v and %v", p.SampleType, pb.SampleType));
                }
            }
            return error.As(null);
        }

        // equalValueType returns true if the two value types are semantically
        // equal. It ignores the internal fields used during encode/decode.
        private static bool equalValueType(ref ValueType st1, ref ValueType st2)
        {
            return st1.Type == st2.Type && st1.Unit == st2.Unit;
        }
    }
}}}}}}
